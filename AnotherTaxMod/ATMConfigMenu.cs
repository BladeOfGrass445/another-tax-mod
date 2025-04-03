using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModdingAPI;
using StardewConfigFramework;
using static System.Net.Mime.MediaTypeNames;
using AnotherTaxMod.Integrations;

namespace AnotherTaxMod
{
    class ATMConfigMenu
    {
        private readonly IModHelper Helper;
        private readonly IMonitor Monitor;
        private readonly IManifest ModManifest;

        public ATMConfig? taxConfig;
        private bool resetTaxConfig;

        internal ATMConfigMenu(IModHelper helper, IMonitor monitor, IManifest manifest, ATMConfig taxConfig)
        {
            Helper = helper;
            Monitor = monitor;
            ModManifest = manifest;
            this.taxConfig = taxConfig;
            this.resetTaxConfig = false;
        }
        public void RegisterMenu()
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => ResetConfig(),
                save: () => CommitConfig()
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.seeconfig-paragraph")
            );
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.generaloptions-title"),
                tooltip: () => ""
            );
            configMenu.AddKeybind(
                mod: this.ModManifest,
                getValue: () => this.taxConfig.keybindTaxMenu, 
                setValue: value => this.taxConfig.keybindTaxMenu = value,
                name: () => "Open the Tax Menu",
                tooltip: () => ""
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Tax Exemption Spring Year 1",
                tooltip: () => "Whether to not be taxed for spring year 1 (payed at the start of summer year 1)",
                getValue: () => this.taxConfig.taxExemptionSpring1,
                setValue: value => this.taxConfig.taxExemptionSpring1 = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Debt Daily Interest",
                tooltip: () => "Configure the daily increase of the debt when in debt. 0.01 stands for +1% daily; 0.5 stands for +50% Daily. Minimal increase is 1 G.",
                getValue: () => this.taxConfig.debtDailyIncrease,
                setValue: value => this.taxConfig.debtDailyIncrease = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "DEBT YEAR LATENESS FINE (see config.json: \"debtLatenessFine\") \nThe fine applied when you are late to pay your taxes depending on the year of the game. The 1st value correspons to year 1, the 2nd to year 2 etc. The last value is applied to every year hereafter."
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "In Debt Daily Frienship Decrease",
                tooltip: () => "Configure the daily decrease of friendship with the villager when in debt. Defaults to 10 (A full heart is 250 friendship points).",
                getValue: () => this.taxConfig.debtDailyFriendshipDecrease,
                setValue: value => this.taxConfig.debtDailyFriendshipDecrease = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Debt Blocks JoJa Access",
                tooltip: () => "Whether to prevent access to Joja Mart when in debt.",
                getValue: () => this.taxConfig.debtBlockJoja,
                setValue: value => this.taxConfig.debtBlockJoja = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Debt Blocks Community Center Access",
                tooltip: () => "Whether to prevent access to Community Center when in debt.",
                getValue: () => this.taxConfig.debtBlockCommunityCenter,
                setValue: value => this.taxConfig.debtBlockCommunityCenter = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Debt Blocks Robin's Building Shop",
                tooltip: () => "Whether to prevent access to Robin's Building Shop when in debt.",
                getValue: () => this.taxConfig.debtBlockBuildingShop,
                setValue: value => this.taxConfig.debtBlockBuildingShop = value
            );



            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Income Taxes",
                tooltip: () => "Income Tax is calculated each season depending what was sold through the SHipping Bin (and optionally through loacl vendors too)"
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Income Tax is calculated each season depending what was sold through the SHipping Bin (and optionally through loacl vendors too)"
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Income Taxing",
                tooltip: () => "Whether to enable tax on income",
                getValue: () => this.taxConfig.incomeTaxEnable,
                setValue: value => this.taxConfig.incomeTaxEnable = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "INCOME TAX BRACKETS (see config.json: \"incomeTaxMult\") \nThis progressive tax system applies increasing tax rates to portions of income based on predefined brackets. Each segment of income is taxed separately at its corresponding rate.\n\nUsing the given scale:\n- 0 - 2,999: 10%\n- 3,000 - 9,999: 12.5%\n- 10,000 - 29,999: 15%\n- 30,000 - 79,999: 20%\n- 80,000 - 179,999: 20%\n- 180,000 - 499,999: 25%\n- 500,000 - 1,299,999: 30%\n- 1,300,000+: 35%\n\nYou are only taxed at a higher rate for the portion of your income that exceeds each bracket. For example, if your income is 5,000:\n- The first 3,000 is taxed at 10% → 300\n- The remaining 2,000 (5,000 - 3,000) is taxed at 12.5% → 250\n\nTotal tax: 550. "
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Tax Locally Sold Products",
                tooltip: () => "Whether to enable tax on income gained through selling to local vendors (Pierre, Willie...)\n!!! Incompatible with Enable Tax Police !!!",
                getValue: () => this.taxConfig.incomeTaxLocalShopEnable,
                setValue: value => this.taxConfig.incomeTaxLocalShopEnable = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Tax Incentive to Local Selling",
                tooltip: () => "Configure the percentage of tax reduction applied to product sold locally (through Pierre, Willie...). Between 0 and 100. 90 corresponds 90% Tax Reduction, 25 corresponds to 25% Tax Reduction...",
                getValue: () => this.taxConfig.incomeTaxLocalShopReductionPercentage,
                setValue: value => this.taxConfig.incomeTaxLocalShopReductionPercentage = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enable Tax Administration",
                tooltip: () => "Whether to enable the tax administration that has a chance to catch and fine you if you sold too much through local vendors\n!!! Incompatible with Tax Locally Sold Products !!!",
                getValue: () => this.taxConfig.incomeTaxAdministrationEnable,
                setValue: value => this.taxConfig.incomeTaxAdministrationEnable = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Local Selling Threshold",
                tooltip: () => "!!! IF Tax Locally Sold Products !!! Configure the Locally sold Gold threshold over which you will be taxed when selling through local shops.\n!!! IF Enable Tax Administration !!! Locally sold Gold Threshold under which you have no chance of being caught.",
                getValue: () => this.taxConfig.incomeTaxLocalShopThreshold,
                setValue: value => this.taxConfig.incomeTaxLocalShopThreshold = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Administration Max Catch Chance For Fraud",
                tooltip: () => "Configure the maximum percentage chance the administration has to catch you committing fraud (by selling to local vendors instead of through the shipping bin). Between 0 and 100.",
                getValue: () => this.taxConfig.incomeTaxAdministrationMaxChancePercentage,
                setValue: value => this.taxConfig.incomeTaxAdministrationMaxChancePercentage = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Fraud Fine",
                tooltip: () => "Configure the percentage of what was sold locally you will need to pay if caught committing fraud. Eg. if 0.6 and 10 000G were sold locally, then you will pay 6 000G when caught frauding.",
                getValue: () => this.taxConfig.incomeTaxFraud,
                setValue: value => this.taxConfig.incomeTaxFraud = value
            );



            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Land Taxes",
                tooltip: () => "Land Tax is calculated each season depending on the number of tillable tiles of the farm"
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Land Tax is calculated each season depending on the number of tillable tiles of the farm"
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Land Taxing",
                tooltip: () => "Whether to enable tax on land property",
                getValue: () => this.taxConfig.landTaxEnable,
                setValue: value => this.taxConfig.landTaxEnable = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "LAND YEAR FLAT TAXES (see config.json: \"landTaxFlat\") \nThe land tax is calculated per tile per season. If the tax rate is 1.0 and your farm has 5,000 tillable tiles, the total land tax will be 5,000 G each season.\nEach entry in the list represents the tax rate for a specific year: the 1st entry applies to year 1, the 2nd entry to year 2, and so on.\nIf the list contains fewer entries than the number of years (e.g., only 3 entries but the game reaches year 4+), the last entry is used for all subsequent years."
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Conservationist Apply To Land Tax (WoL)",
                tooltip: () => "Whether to Conservationist tax reduction apply to Land Taxes (From the Walk of Life Mod)",
                getValue: () => this.taxConfig.conservationistApplyToLandTax,
                setValue: value => this.taxConfig.conservationistApplyToLandTax = value
            );


            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Building Taxes",
                tooltip: () => "Building Tax is calculated each season depending on the type of buildings, the number of buildings and (if set up) the current year"
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Building Tax is calculated each season depending on the type of buildings, the number of buildings and (if set up) the current year"
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Building Taxing",
                tooltip: () => "Whether to enable tax on buildings",
                getValue: () => this.taxConfig.buildingTaxEnable,
                setValue: value => this.taxConfig.buildingTaxEnable = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Building Count Tax Multiplier.",
                tooltip: () => "Determines the multiplier for building tax. Calculated as:\nBuildingCountAdd + NumberOfBuildings * Multiplier,\nthis value scales the base building tax accordingly.",
                getValue: () => this.taxConfig.buildingCountMult,
                setValue: value => this.taxConfig.buildingCountMult = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Building Count Tax Modifier",
                tooltip: () => "Determines the modifier for building tax. Calculated as:\nModifier + NumberOfBuildings * Multiplier,\nthis value scales the base building tax accordingly.",
                getValue: () => this.taxConfig.buildingCountAdd,
                setValue: value => this.taxConfig.buildingCountAdd = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "EXCLUDED BUILDINDS FOR COUNT (see config.json: \"buildingExcluded\") \n A list of building names that are excluded from the total building count when calculating the tax based on the number of buildings."
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "BUILDING YEAR TAX MULTIPLIER (see config.json: \"buildingTaxMult\") \nBuilding Tax Multipliers by Year\nThis list of floats scales the building tax each year. Entry 1 applies to Year 1, Entry 2 to Year 2, and so on.\nIf there are more years than entries, the last value is used for all following years.\nThe final building tax is multiplied by the corresponding year's value from this list."
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Greenhouse Flat Tax (see config.json: \"buildingTaxFlat_Greenhouse\") \nFlat tax applied to the Greenhouse building.\nThe 1st value is the flat tax for the abandoned greenhouse. The 2nd value is for the repaired greenhouse"
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Farmhouse Flat Tax (see config.json: \"buildingTaxFlat_Greenhouse\") \nFlat tax applied to the Farmhouse building.\nThe 1st value is the flat tax for the unupgraded farmhouse. The 2nd value is for the 1st upgrade of the farmhouse. The 3rd value is for the 2nd upgrade of the farmhouse."
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Coop Flat Tax (see config.json: \"buildingTaxFlat_Coop\") \nFlat tax applied to the Coop building.\nThe 1st value is the flat tax for the Coop, then the Big Coop, then the Deluxe Coop. The 4th value is for the Premium Coop (from Stardew Valley Expanded)."
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Barn Flat Tax (see config.json: \"buildingTaxFlat_Barn\") \nFlat tax applied to the Barn building.\nThe 1st value is the flat tax for the Barn, then the Big Barn, then the Deluxe Barn. The 4th value is for the Premium Barn (from Stardew Valley Expanded)."
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Cabin Flat Tax",
                tooltip: () => "Flat tax applied to the Cabin building.",
                getValue: () => this.taxConfig.buildingTaxFlat_Cabin,
                setValue: value => this.taxConfig.buildingTaxFlat_Cabin = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Well Flat Tax",
                tooltip: () => "Flat tax applied to the Well building.",
                getValue: () => this.taxConfig.buildingTaxFlat_Well,
                setValue: value => this.taxConfig.buildingTaxFlat_Well = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Stable Flat Tax",
                tooltip: () => "Flat tax applied to the Stable building.",
                getValue: () => this.taxConfig.buildingTaxFlat_Stable,
                setValue: value => this.taxConfig.buildingTaxFlat_Stable = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Tractor Flat Tax (Tractor Mod)",
                tooltip: () => "Tractor Mod Only. Flat tax applied to the Tractor building.",
                getValue: () => this.taxConfig.buildingTaxFlat_Tractor,
                setValue: value => this.taxConfig.buildingTaxFlat_Tractor = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Slime Hutch Flat Tax",
                tooltip: () => "Flat tax applied to the Slime Hutch building.",
                getValue: () => this.taxConfig.buildingTaxFlat_SlimeHutch,
                setValue: value => this.taxConfig.buildingTaxFlat_SlimeHutch = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Silo Flat Tax",
                tooltip: () => "Flat tax applied to the Silo building.",
                getValue: () => this.taxConfig.buildingTaxFlat_Silo,
                setValue: value => this.taxConfig.buildingTaxFlat_Silo = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Shed Flat Tax (see config.json: \"buildingTaxFlat_Shed\") \nFlat tax applied to the Shed building.\nThe 1st value is the flat tax for the Shed. The 2nd value is for the Big Shed."
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Mill Flat Tax",
                tooltip: () => "Flat tax applied to the Mill building.",
                getValue: () => this.taxConfig.buildingTaxFlat_Mill,
                setValue: value => this.taxConfig.buildingTaxFlat_Mill = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Fish Pond Flat Tax",
                tooltip: () => "Flat tax applied to the Fish Pond building.",
                getValue: () => this.taxConfig.buildingTaxFlat_FishPond,
                setValue: value => this.taxConfig.buildingTaxFlat_FishPond = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Shipping Bin Flat Tax",
                tooltip: () => "Flat tax applied to the Shipping Bin building.",
                getValue: () => this.taxConfig.buildingTaxFlat_ShippingBin,
                setValue: value => this.taxConfig.buildingTaxFlat_ShippingBin = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Pet Bowl Flat Tax",
                tooltip: () => "Flat tax applied to the Pet Bowl building.",
                getValue: () => this.taxConfig.buildingTaxFlat_PetBowl,
                setValue: value => this.taxConfig.buildingTaxFlat_PetBowl = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enabled Grandpa Shed (SVE)",
                tooltip: () => "Do you play on a Farm Map that has Grandpa Shed (Stardew Valley Expanded)",
                getValue: () => this.taxConfig.hasGrandpaShed,
                setValue: value => this.taxConfig.hasGrandpaShed = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => " Grandpa Shed Flat Tax (SVE) (see config.json: \"buildingTaxFlat_GrandpaShed\") \nFlat tax applied to the Grandpa Shed building.\nThe 1st value is the flat tax for the abandoned Grandpa Shed. The 2nd value is for the repaired Grandpa Shed"
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Winery Flat Tax (SVE)",
                tooltip: () => "Stardew Valley Expanded Only. Flat tax applied to the Winery building.",
                getValue: () => this.taxConfig.buildingTaxFlat_Winery,
                setValue: value => this.taxConfig.buildingTaxFlat_Winery = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Greenhouse Flat Tax (Greenhouse Upgrade mod) (see config.json: \"buildingTaxFlat_UpgradableGreenhouse\") \nFlat tax applied to the Greenhourse building (Modded).\nThe 1st value is the flat tax for the first upgrade, the 2nd for 2nd upgrade until the 5th upgrade."
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Conservationist Apply To Building Tax (WoL)",
                tooltip: () => "Whether to Conservationist tax reduction apply to Building Taxes (From the Walk of Life Mod)",
                getValue: () => this.taxConfig.conservationistApplyToLandTax,
                setValue: value => this.taxConfig.conservationistApplyToLandTax = value
            );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Extra Options",
                tooltip: () => ""
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Show Details in Log",
                tooltip: () => "Whether to show calculation details in the log when opening the Tax Menu",
                getValue: () => this.taxConfig.extraCalculationLog,
                setValue: value => this.taxConfig.extraCalculationLog = value
            );

        }

        private void CommitConfig()
        {
            if (!this.resetTaxConfig)
            {
                ATMConfig readTaxConfig = Helper.ReadConfig<ATMConfig>();

                this.taxConfig.debtLatenessFine = readTaxConfig.debtLatenessFine;

                this.taxConfig.incomeTaxMult = readTaxConfig.incomeTaxMult;

                this.taxConfig.landTaxFlat = readTaxConfig.landTaxFlat;

                this.taxConfig.buildingExcluded = readTaxConfig.buildingExcluded;
                this.taxConfig.buildingTaxMult = readTaxConfig.buildingTaxMult;
                this.taxConfig.buildingTaxFlat_Greenhouse = readTaxConfig.buildingTaxFlat_Greenhouse;
                this.taxConfig.buildingTaxFlat_UpgradableGreenhouse = readTaxConfig.buildingTaxFlat_UpgradableGreenhouse;
                this.taxConfig.buildingTaxFlat_GrandpaShed = readTaxConfig.buildingTaxFlat_GrandpaShed;
                this.taxConfig.buildingTaxFlat_Farmhouse = readTaxConfig.buildingTaxFlat_Farmhouse;
                this.taxConfig.buildingTaxFlat_Coop = readTaxConfig.buildingTaxFlat_Coop;
                this.taxConfig.buildingTaxFlat_Barn = readTaxConfig.buildingTaxFlat_Barn;
                this.taxConfig.buildingTaxFlat_Shed = readTaxConfig.buildingTaxFlat_Shed;
            }
            Helper.WriteConfig<ATMConfig>(this.taxConfig);
            this.resetTaxConfig = false;
        }

        private void ResetConfig()
        {
            this.resetTaxConfig = true;
            this.taxConfig.ResetToDefaults();
        }




    }

}

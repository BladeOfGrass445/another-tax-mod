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
                name: () => Helper.Translation.Get("configmenu.keybind-name"),
                tooltip: () => ""
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.taxexemptionspring1-name"),
                tooltip: () => Helper.Translation.Get("configmenu.taxexemptionspring1-tooltip"),
                getValue: () => this.taxConfig.taxExemptionSpring1,
                setValue: value => this.taxConfig.taxExemptionSpring1 = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.debtdailyincrease-name"),
                tooltip: () => Helper.Translation.Get("configmenu.debtdailyincrease-tooltip"),
                getValue: () => this.taxConfig.debtDailyIncrease,
                setValue: value => this.taxConfig.debtDailyIncrease = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.debtlatenessfine")
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.debtDailyFriendshipDecrease-name"),
                tooltip: () => Helper.Translation.Get("configmenu.debtDailyFriendshipDecrease-tooltip"),
                getValue: () => this.taxConfig.debtDailyFriendshipDecrease,
                setValue: value => this.taxConfig.debtDailyFriendshipDecrease = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.debtBlockJoja-name"),
                tooltip: () => Helper.Translation.Get("configmenu.debtBlockJoja-tooltip"),
                getValue: () => this.taxConfig.debtBlockJoja,
                setValue: value => this.taxConfig.debtBlockJoja = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.debtBlockCommunityCenter-name"),
                tooltip: () => Helper.Translation.Get("configmenu.debtBlockCommunityCenter-tooltip"),
                getValue: () => this.taxConfig.debtBlockCommunityCenter,
                setValue: value => this.taxConfig.debtBlockCommunityCenter = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.debtBlockBuildingShop-name"),
                tooltip: () => Helper.Translation.Get("configmenu.debtBlockBuildingShop-tooltip"),
                getValue: () => this.taxConfig.debtBlockBuildingShop,
                setValue: value => this.taxConfig.debtBlockBuildingShop = value
            );



            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.incometax-name"),
                tooltip: () => Helper.Translation.Get("configmenu.incometax-tooltip")
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.incometax-paragraph")
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.incomeTaxEnable-name"),
                tooltip: () => Helper.Translation.Get("configmenu.incomeTaxEnable-tooltip"),
                getValue: () => this.taxConfig.incomeTaxEnable,
                setValue: value => this.taxConfig.incomeTaxEnable = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.incometaxbrackets-paragraph")
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.incomeTaxLocalShopEnable-name"),
                tooltip: () => Helper.Translation.Get("configmenu.incomeTaxLocalShopEnable-tooltip"),
                getValue: () => this.taxConfig.incomeTaxLocalShopEnable,
                setValue: value => this.taxConfig.incomeTaxLocalShopEnable = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.incomeTaxLocalShopReductionPercentage-name"),
                tooltip: () => Helper.Translation.Get("configmenu.incomeTaxLocalShopReductionPercentage-tooltip"),
                getValue: () => this.taxConfig.incomeTaxLocalShopReductionPercentage,
                setValue: value => this.taxConfig.incomeTaxLocalShopReductionPercentage = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.incomeTaxAdministrationEnable-name"),
                tooltip: () => Helper.Translation.Get("configmenu.incomeTaxAdministrationEnable-tooltip"),
                getValue: () => this.taxConfig.incomeTaxAdministrationEnable,
                setValue: value => this.taxConfig.incomeTaxAdministrationEnable = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.incomeTaxLocalShopThreshold-name"),
                tooltip: () => Helper.Translation.Get("configmenu.incomeTaxLocalShopThreshold-tooltip"),
                getValue: () => this.taxConfig.incomeTaxLocalShopThreshold,
                setValue: value => this.taxConfig.incomeTaxLocalShopThreshold = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.incomeTaxAdministrationMaxChancePercentage-name"),
                tooltip: () => Helper.Translation.Get("configmenu.incomeTaxAdministrationMaxChancePercentage-tooltip"),
                getValue: () => this.taxConfig.incomeTaxAdministrationMaxChancePercentage,
                setValue: value => this.taxConfig.incomeTaxAdministrationMaxChancePercentage = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.incomeTaxFraud-name"),
                tooltip: () => Helper.Translation.Get("configmenu.incomeTaxFraud-tooltip"),
                getValue: () => this.taxConfig.incomeTaxFraud,
                setValue: value => this.taxConfig.incomeTaxFraud = value
            );



            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.landtax-title"),
                tooltip: () => Helper.Translation.Get("configmenu.landtax-tooltip")
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.landtax-paragraph")
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.landTaxEnable-name"),
                tooltip: () => Helper.Translation.Get("configmenu.landTaxEnable-tooltip"),
                getValue: () => this.taxConfig.landTaxEnable,
                setValue: value => this.taxConfig.landTaxEnable = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.landTaxFlat-paragraph")
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.conservationistApplyToLandTax-name"),
                tooltip: () => Helper.Translation.Get("configmenu.conservationistApplyToLandTax-tooltip"),
                getValue: () => this.taxConfig.conservationistApplyToLandTax,
                setValue: value => this.taxConfig.conservationistApplyToLandTax = value
            );


            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingtax-title"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingtax-tooltip")
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingtax-paragraph")
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxEnable-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxEnable-tooltip"),
                getValue: () => this.taxConfig.buildingTaxEnable,
                setValue: value => this.taxConfig.buildingTaxEnable = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingCountMult-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingCountMult-tooltip"),
                getValue: () => this.taxConfig.buildingCountMult,
                setValue: value => this.taxConfig.buildingCountMult = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingCountAdd-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingCountAdd-tooltip"),
                getValue: () => this.taxConfig.buildingCountAdd,
                setValue: value => this.taxConfig.buildingCountAdd = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingExcluded-paragraph")
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingTaxMult-paragraph")
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Greenhouse-paragraph")
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Farmhouse-paragraph")
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Coop-paragraph")
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Barn-paragraph")
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Cabin-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Cabin-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_Cabin,
                setValue: value => this.taxConfig.buildingTaxFlat_Cabin = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Well-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Well-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_Well,
                setValue: value => this.taxConfig.buildingTaxFlat_Well = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Stable-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Stable-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_Stable,
                setValue: value => this.taxConfig.buildingTaxFlat_Stable = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Tractor-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Tractor-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_Tractor,
                setValue: value => this.taxConfig.buildingTaxFlat_Tractor = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_SlimeHutch-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_SlimeHutch-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_SlimeHutch,
                setValue: value => this.taxConfig.buildingTaxFlat_SlimeHutch = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Silo-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Silo-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_Silo,
                setValue: value => this.taxConfig.buildingTaxFlat_Silo = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Shed-paragraph")
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Mill-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Mill-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_Mill,
                setValue: value => this.taxConfig.buildingTaxFlat_Mill = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_FishPond-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_FishPond-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_FishPond,
                setValue: value => this.taxConfig.buildingTaxFlat_FishPond = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_ShippingBin-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_ShippingBin-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_ShippingBin,
                setValue: value => this.taxConfig.buildingTaxFlat_ShippingBin = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_PetBowl-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_PetBowl-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_PetBowl,
                setValue: value => this.taxConfig.buildingTaxFlat_PetBowl = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.hasGrandpaShed-name"),
                tooltip: () => Helper.Translation.Get("configmenu.hasGrandpaShed-tooltip"),
                getValue: () => this.taxConfig.hasGrandpaShed,
                setValue: value => this.taxConfig.hasGrandpaShed = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingTaxFlat_GrandpaShed-paragraph")
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Winery-name"),
                tooltip: () => Helper.Translation.Get("configmenu.buildingTaxFlat_Winery-tooltip"),
                getValue: () => this.taxConfig.buildingTaxFlat_Winery,
                setValue: value => this.taxConfig.buildingTaxFlat_Winery = value
            );
            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.buildingTaxFlat_UpgradableGreenhouse-paragraph")
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.conservationistApplyToBuildingTax-name"),
                tooltip: () => Helper.Translation.Get("configmenu.conservationistApplyToBuildingTax-tooltip"),
                getValue: () => this.taxConfig.conservationistApplyToBuildingTax,
                setValue: value => this.taxConfig.conservationistApplyToBuildingTax = value
            );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("configmenu.extraoptions-paragraph"),
                tooltip: () => ""
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("configmenu.extraCalculationLog-name"),
                tooltip: () => Helper.Translation.Get("configmenu.extraCalculationLog-tooltip"),
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

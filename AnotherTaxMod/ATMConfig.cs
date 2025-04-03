using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace AnotherTaxMod
{
    public sealed class ATMConfig
    {
        public SButton keybindTaxMenu { get; set; } = SButton.U;
        public bool taxExemptionSpring1 { get; set; } = true;
        public float debtDailyIncrease { get; set; } = 0.01f;
        public int[] debtLatenessFine { get; set; } = new int[10] { 500, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000 };
        public int debtDailyFriendshipDecrease { get; set; } = 10;
        public bool debtBlockJoja { get; set; } = true;
        public bool debtBlockCommunityCenter { get; set; } = true;
        public bool debtBlockBuildingShop { get; set; } = true;

        public bool incomeTaxEnable { get; set; } = true;
        public (int, float)[] incomeTaxMult { get; set; } = new (int, float)[8] { (0, 0.1f), (3000, 0.125f), (10000, 0.15f), (30000, 0.20f), (80000, 0.20f), (180000, 0.25f), (500000, 0.30f), (1300000, 0.35f) };
        public bool incomeTaxLocalShopEnable { get; set; } = false;
        public int incomeTaxLocalShopReductionPercentage { get; set; } = 0;
        public bool incomeTaxAdministrationEnable { get; set; } = true;
        public int incomeTaxLocalShopThreshold { get; set; } = 5000;
        public int incomeTaxAdministrationMaxChancePercentage { get; set; } = 50;
        public float incomeTaxFraud { get; set; } = 0.5f;


        public bool landTaxEnable { get; set; } = true;
        public float[] landTaxFlat { get; set; } = new float[15] { 0.0625f, 0.125f, 0.25f, 0.25f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 3.0f };
        public bool conservationistApplyToLandTax { get; set; } = false;

        public bool buildingTaxEnable { get; set; } = true;
        public float buildingCountMult { get; set; } = 0.02f;
        public float buildingCountAdd { get; set; } = 1.0f;
        public HashSet<string> buildingExcluded { get; set; } = new HashSet<string> { "Shipping Bin", "Well", "Pet Bowl" };
        public float[] buildingTaxMult { get; set; } = new float[10] { 1.0f, 1.0f, 1.25f, 1.25f, 1.5f, 1.5f, 1.75f, 1.75f, 1.75f, 2.0f };
        public int[] buildingTaxFlat_Greenhouse { get; set; } = new int[2] { 50, 500 };
        public int[] buildingTaxFlat_Farmhouse { get; set; } = new int[4] { 500, 1000, 2000, 3000 };
        public int[] buildingTaxFlat_Coop { get; set; } = new int[4] { 250, 500, 750, 1500 };
        public int[] buildingTaxFlat_Barn { get; set; } = new int[4] { 350, 700, 1000, 2000 };
        public int buildingTaxFlat_Cabin { get; set; } = 500;
        public int buildingTaxFlat_Well { get; set; } = -500;
        public int buildingTaxFlat_Stable { get; set; } = 1000;
        public int buildingTaxFlat_Tractor { get; set; } = 2000;
        public int buildingTaxFlat_SlimeHutch { get; set; } = 1000;
        public int buildingTaxFlat_Silo { get; set; } = 100;
        public int[] buildingTaxFlat_Shed { get; set; } = new int[2] { 500, 750 };
        public int buildingTaxFlat_Mill { get; set; } = 250;
        public int buildingTaxFlat_FishPond { get; set; } = 250;
        public int buildingTaxFlat_ShippingBin { get; set; } = 0;
        public int buildingTaxFlat_PetBowl { get; set; } = 0;
        public bool hasGrandpaShed { get; set; } = false;
        public int[] buildingTaxFlat_GrandpaShed { get; set; } = new int[2] { 150, 1500 };
        public int buildingTaxFlat_Winery { get; set; } = 3000;
        public int[] buildingTaxFlat_UpgradableGreenhouse { get; set; } = new int[5] { 750, 1000, 2000, 2000, 2500 };
        public bool conservationistApplyToBuildingTax { get; set; } = false;

        public bool extraCalculationLog { get; set; } = false;



        public void ResetToDefaults()
        {
            this.keybindTaxMenu = SButton.U;
            this.taxExemptionSpring1 = true;
            this.debtDailyIncrease = 0.01f;
            this.debtLatenessFine = new int[10] { 500, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000 };
            this.debtDailyFriendshipDecrease = 10;
            this.debtBlockJoja = true;
            this.debtBlockCommunityCenter = true;
            this.debtBlockBuildingShop = true;

            this.incomeTaxEnable = true;
            this.incomeTaxLocalShopEnable = false;
            this.incomeTaxLocalShopReductionPercentage = 0;
            this.incomeTaxAdministrationEnable = true;
            this.incomeTaxLocalShopThreshold = 5000;
            this.incomeTaxAdministrationMaxChancePercentage = 50;
            this.incomeTaxFraud = 0.5f;
            this.incomeTaxMult = new (int, float)[8] { (0, 0.1f), (3000, 0.125f), (10000, 0.15f), (30000, 0.20f), (80000, 0.20f), (180000, 0.25f), (500000, 0.30f), (1300000, 0.35f) };

            this.landTaxEnable  = true;
            this.landTaxFlat = new float[15] { 0.0625f, 0.125f, 0.25f, 0.25f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 3.0f };
            this.conservationistApplyToLandTax = false;

            this.buildingTaxEnable = true;
            this.buildingCountMult = 0.02f;
            this.buildingCountAdd = 1.0f;
            this.buildingExcluded = new HashSet<string> { "Shipping Bin", "Well", "Pet Bowl" };
            this.buildingTaxMult = new float[10] { 1.0f, 1.0f, 1.25f, 1.25f, 1.5f, 1.5f, 1.75f, 1.75f, 1.75f, 2.0f };
            this.buildingTaxFlat_Greenhouse = new int[2] { 50, 500 };
            this.buildingTaxFlat_Farmhouse = new int[4] { 500, 1000, 2000, 3000 };
            this.buildingTaxFlat_Coop = new int[4] { 250, 500, 750, 1500 };
            this.buildingTaxFlat_Barn = new int[4] { 350, 700, 1000, 2000 };
            this.buildingTaxFlat_Cabin = 500;
            this.buildingTaxFlat_Well = -500;
            this.buildingTaxFlat_Stable = 1000;
            this.buildingTaxFlat_Tractor = 2000;
            this.buildingTaxFlat_SlimeHutch = 1000;
            this.buildingTaxFlat_Silo = 100;
            this.buildingTaxFlat_Shed = new int[2] { 500, 750 };
            this.buildingTaxFlat_Mill = 250;
            this.buildingTaxFlat_FishPond = 250;
            this.buildingTaxFlat_ShippingBin = 0;
            this.buildingTaxFlat_PetBowl = 0;
            this.hasGrandpaShed = false;
            this.buildingTaxFlat_GrandpaShed = new int[2] { 150, 1500 };
            this.buildingTaxFlat_Winery = 3000;
            this.buildingTaxFlat_UpgradableGreenhouse = new int[5] { 750, 1000, 2000, 2000, 2500 };
            this.conservationistApplyToBuildingTax = false;


            this.extraCalculationLog = false;
        }
 

    }
}

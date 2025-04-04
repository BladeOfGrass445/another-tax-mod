using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.SpecialOrders;
using static StardewValley.Menus.CoopMenu;

namespace AnotherTaxMod
{
    internal class ATMBuildingTax
    {
        public static int GetBuildingTaxFlat(string buildingname, ATMConfig taxConfig)
        {
            if (buildingname == "Greenhouse")
            {
                if (Game1.player.hasOrWillReceiveMail("CosmicKillsSouls.GreenhouseV6"))
                {
                    return taxConfig.buildingTaxFlat_UpgradableGreenhouse[4];
                }
                else if (Game1.player.hasOrWillReceiveMail("CosmicKillsSouls.GreenhouseV5"))
                {
                    return taxConfig.buildingTaxFlat_UpgradableGreenhouse[3];
                }
                else if (Game1.player.hasOrWillReceiveMail("CosmicKillsSouls.GreenhouseV4"))
                {
                    return taxConfig.buildingTaxFlat_UpgradableGreenhouse[2];
                }
                else if (Game1.player.hasOrWillReceiveMail("CosmicKillsSouls.GreenhouseV3"))
                {
                    return taxConfig.buildingTaxFlat_UpgradableGreenhouse[1];
                }
                else if (Game1.player.hasOrWillReceiveMail("CosmicKillsSouls.GreenhouseV2"))
                {
                    return taxConfig.buildingTaxFlat_UpgradableGreenhouse[0];
                }
                else if (Game1.player.hasOrWillReceiveMail("ccPantry"))
                {
                    return taxConfig.buildingTaxFlat_Greenhouse[1];
                }
                else
                {
                    return taxConfig.buildingTaxFlat_Greenhouse[0];
                }
            }
            else if (buildingname == "GrandpaShed")
            {
                if (Game1.player.hasOrWillReceiveMail("ShedRepaired"))
                {
                    return taxConfig.buildingTaxFlat_GrandpaShed[1];
                }
                else
                {
                    return taxConfig.buildingTaxFlat_GrandpaShed[0];
                }
            }
            else if (buildingname == "Farmhouse")
            {
                return taxConfig.buildingTaxFlat_Farmhouse[Game1.player.HouseUpgradeLevel];
            }
            else if (buildingname == "Coop")
            {
                return taxConfig.buildingTaxFlat_Coop[0];
            }
            else if (buildingname == "Big Coop")
            {
                return taxConfig.buildingTaxFlat_Coop[1];
            }
            else if (buildingname == "Deluxe Coop")
            {
                return taxConfig.buildingTaxFlat_Coop[2];
            }
            else if (buildingname == "FlashShifter.StardewValleyExpandedCP_PremiumCoop")
            {
                return taxConfig.buildingTaxFlat_Coop[3];
            }
            else if (buildingname == "Barn")
            {
                return taxConfig.buildingTaxFlat_Barn[0];
            }
            else if (buildingname == "Big Barn")
            {
                return taxConfig.buildingTaxFlat_Barn[1];
            }
            else if (buildingname == "Deluxe Barn")
            {
                return taxConfig.buildingTaxFlat_Barn[2];
            }
            else if (buildingname == "FlashShifter.StardewValleyExpandedCP_PremiumBarn")
            {
                return taxConfig.buildingTaxFlat_Barn[3];
            }
            else if (buildingname == "FlashShifter.StardewValleyExpandedCP_Winery")
            {
                return taxConfig.buildingTaxFlat_Winery;
            }
            else if (buildingname == "Cabin")
            {
                return taxConfig.buildingTaxFlat_Cabin;
            }
            else if (buildingname == "Well")
            {
                return taxConfig.buildingTaxFlat_Well;
            }
            else if (buildingname == "Stable")
            {
                return taxConfig.buildingTaxFlat_Stable;
            }
            else if (buildingname == "Pathoschild.TractorMod_Stable")
            {
                return taxConfig.buildingTaxFlat_Tractor;
            }
            else if (buildingname == "Slime Hutch")
            {
                return taxConfig.buildingTaxFlat_SlimeHutch;
            }
            else if (buildingname == "Silo")
            {
                return taxConfig.buildingTaxFlat_Silo;
            }
            else if (buildingname == "Shed")
            {
                return taxConfig.buildingTaxFlat_Shed[0];
            }
            else if (buildingname == "Big Shed")
            {
                return taxConfig.buildingTaxFlat_Shed[1];
            }
            else if (buildingname == "Mill")
            {
                return taxConfig.buildingTaxFlat_Mill;
            }
            else if (buildingname == "Fish Pond")
            {
                return taxConfig.buildingTaxFlat_FishPond;
            }
            else if (buildingname == "Shipping Bin")
            {
                return taxConfig.buildingTaxFlat_ShippingBin;
            }
            else if (buildingname == "Pet Bowl")
            {
                return taxConfig.buildingTaxFlat_PetBowl;
            }
            else
            {
                return 0;
            }
            
        }

        public static Dictionary<string, int> GetBuildingEnum(ATMConfig taxConfig)
        {
            // Get the current player's farm
            var farm = Game1.getFarm();

            // Initialize a dictionary to store building counts
            Dictionary<string, int> buildingCounts = new Dictionary<string, int>();

            if (taxConfig.hasGrandpaShed)
            {
                buildingCounts["GrandpaShed"] = 1; //With modconfig
            }


            // Loop through each building on the farm
            foreach (Building building in farm.buildings)
            {
                string buildingType = building.buildingType.Value;

                // Increment the count for this building type
                if (buildingCounts.ContainsKey(buildingType))
                {
                    buildingCounts[buildingType]++;
                }
                else
                {
                    buildingCounts[buildingType] = 1;
                }
            }

            return buildingCounts;
        }

        public static int GetBuildingTax(ATMConfig taxConfig, float daLionConservationist = 0)
        {
            float tax = 0;
            Dictionary<string, int> buildingEnum = GetBuildingEnum(taxConfig);
            int numberBuildings = GetBuildingCount(taxConfig, buildingEnum);
            foreach (var pair in buildingEnum)
            {
                tax += GetBuildingTaxFlat(pair.Key, taxConfig) * pair.Value;

            }
            tax *= GetBuildingYearMult(taxConfig);
            tax *= GetBuildingCountMult(taxConfig, numberBuildings);
            tax *= Convert.ToInt32(taxConfig.buildingTaxEnable);
            
            if (taxConfig.conservationistApplyToBuildingTax)
            {
                tax += tax * daLionConservationist;
            }
            return tax < 0 ? 0 : (int)tax;
            
                
        }

        public static float GetBuildingCountMult(ATMConfig taxConfig, int numberBuildings)
        {
            return taxConfig.buildingCountAdd + numberBuildings * taxConfig.buildingCountMult;
        }

        public static float GetBuildingYearMult(ATMConfig taxConfig)
        {
            if (Game1.year - 1 >= taxConfig.buildingTaxMult.Length)
            {
                return taxConfig.buildingTaxMult[taxConfig.buildingTaxMult.Length - 1];
            }
            else
            {
                return taxConfig.buildingTaxMult[Game1.year - 1];
            }
        }

        public static int GetBuildingCount(ATMConfig taxConfig, Dictionary<string, int> buildingEnum)
        {
            int count = 0;
            
            foreach (var pair in buildingEnum)
            {
                if (!taxConfig.buildingExcluded.Contains(pair.Key))
                {
                    count++;
                }
            }
            
            return count;
        }

    }
}

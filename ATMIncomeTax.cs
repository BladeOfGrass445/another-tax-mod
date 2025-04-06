using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;


namespace AnotherTaxMod
{
    internal class ATMIncomeTax
    {
        public static int GetIncomeTax(ATMTaxData taxData, ATMConfig taxConfig, float daLionConservationist = 0)
        {
            float tax = 0.0f;
            int income = taxData.SoldShipped;
            //Monitor.Log($"Income = {income}G", LogLevel.Info);

            if (taxConfig.incomeTaxLocalShopEnable && taxData.SoldLocally >= taxConfig.incomeTaxLocalShopThreshold)
            {
                income += (int)(taxData.SoldLocally * (1f - (taxConfig.incomeTaxLocalShopReductionPercentage / 100f)));
            }
            //Monitor.Log($"Income = {income}G", LogLevel.Info);
            for (int i = 0; i < taxConfig.incomeTaxMult.Length; i++)
            {
                if (i + 1 < taxConfig.incomeTaxMult.Length && income > taxConfig.incomeTaxMult[i + 1].Item1)
                {
                    tax += taxConfig.incomeTaxMult[i].Item2 * (taxConfig.incomeTaxMult[i + 1].Item1 - taxConfig.incomeTaxMult[i].Item1);
                }
                else
                {
                    tax += taxConfig.incomeTaxMult[i].Item2 * (income < taxConfig.incomeTaxMult[i].Item1 ? 0 : (income - taxConfig.incomeTaxMult[i].Item1));
                }
                //Monitor.Log($"tax palier {i} = {tax}G", LogLevel.Info);
            }
            tax *= Convert.ToInt32(taxConfig.incomeTaxEnable);
            tax -= tax * daLionConservationist;
            //Monitor.Log($"taxdalion = {tax}G", LogLevel.Info);

            return tax < 0 ? 0 : (int)tax;
        }


        public static int GetShippingBinSold()
        {
            IList<Item> shippingBin = Game1.getFarm().getShippingBin(Game1.player);
            int totalSold = 0; // Track total gold gained

            for (int i = 0; i < shippingBin.Count; i++)
            {
                if (shippingBin[i] is StardewValley.Object obj && obj.canBeShipped())
                {
                    int quantity = obj.Stack;
                    int price = obj.sellToStorePrice(); // Original sell price
                    totalSold += price * quantity;
                }
            }

            return totalSold;
        }

        public static int GetLocalStoreSold(List<Item> previousInventory)
        {
            int totalSold = 0; // Track total gold gained

            foreach (var item in previousInventory)
            {
                if (item.Name == "Scythe")
                {
                    continue;
                }
                if (!Game1.player.Items.ContainsId(item.ItemId))
                {
                    totalSold += item.sellToStorePrice() * item.Stack;
                }
                else
                {
                    foreach (var item2 in Game1.player.Items.GetById(item.ItemId))
                    {
                        if (item2.Quality == item.Quality && item2.Stack != item.Stack)
                        {
                            totalSold += item.sellToStorePrice() * (item.Stack - item2.Stack);
                        }
                    }
                }
            }

            return totalSold;
        }

        public static int GetFraudPenalty(ATMTaxData taxData, ATMConfig taxConfig, double policeChance)
        {
            if (Game1.random.NextDouble() < policeChance)
            {
                return (int)(taxData.SoldLocally * taxConfig.incomeTaxFraud);
            }
            else
            {
                return 0;
            }
        }

        public static float GetPoliceChance(ATMTaxData taxData, ATMConfig taxConfig)
        {
            float policeChance = 0;
            float maxPoliceChance = taxConfig.incomeTaxAdministrationMaxChancePercentage / 100.0f;
            maxPoliceChance = maxPoliceChance > 1 ? 1 : maxPoliceChance;
            maxPoliceChance = maxPoliceChance < 0 ? 0 : maxPoliceChance;
            if (taxData.SoldLocally <= 0)
            {
                policeChance = 0;
            }
            else if (taxData.SoldShipped <= 0)
            {
                policeChance = maxPoliceChance;
            }
            else if ((taxData.SoldLocally < taxData.SoldShipped) && (taxData.SoldLocally > taxConfig.incomeTaxLocalShopThreshold))
            {
                policeChance = (taxData.SoldLocally - taxConfig.incomeTaxLocalShopThreshold) * 0.05f / (taxData.SoldShipped - taxConfig.incomeTaxLocalShopThreshold);
            }
            else if (taxData.SoldLocally >= taxData.SoldShipped)
            {
                policeChance = (float)Math.Pow(2, (float)(taxData.SoldLocally - taxData.SoldShipped) / (float)taxData.SoldShipped) * 0.05f;
            }
            policeChance = policeChance < 0 ? 0 : policeChance;
            return policeChance >= maxPoliceChance ? maxPoliceChance : policeChance;
        }

        public static Dictionary<(string Name, int Quality), (Item ItemOne, int Count)> CopyInventoryToDict(Inventory source)
        {
            //Convert Inventory to List-
            List<Item> tempsource = source
            .Where(item => item != null)
            .Select(item =>
            {
                return item;
            })
            .ToList();

            // Create a dictionary to track items by their name and quality.
            var itemDictionary = new Dictionary<(string Name, int Quality), (Item Item, int Count)>();
            foreach (var item in tempsource)
            {
                Item copyOne = item.getOne();
                var key = (copyOne.Name, copyOne.Quality);

                if (itemDictionary.ContainsKey(key))
                {
                    itemDictionary[key] = (itemDictionary[key].Item, itemDictionary[key].Count + item.Stack); // Combine stack sizes.
                }
                else
                {
                    // If it's a new item, add it to the dictionary.
                    itemDictionary[key] = (copyOne, item.Stack);
                }
            }

            return itemDictionary;
        }
    }
}

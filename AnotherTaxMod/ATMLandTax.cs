using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace AnotherTaxMod
{
    internal class ATMLandTax
    {

        public static int GetFarmTileCounts()
        {
            int tillableCount = 0;

            // Get the farm object
            Farm farm = Game1.getFarm();

            for (var i = 0; i < farm.Map.Layers[0].LayerWidth; i++)
            {
                for (var j = 0; j < farm.Map.Layers[0].LayerHeight; j++)
                {
                    if (farm.doesTileHaveProperty(i, j, "Diggable", "Back") is not null)
                    {
                        tillableCount++;
                    }
                }
            }
            return tillableCount;
        }

        public static float GetUnitaryLandTax(float[] landTaxFlat)
        {
            float current_landTaxFlat;
            if (Game1.year - 1 >= landTaxFlat.Length)
            {
                current_landTaxFlat = landTaxFlat[landTaxFlat.Length-1];
            }
            else
            {
                current_landTaxFlat = landTaxFlat[Game1.year - 1];
            }

            return current_landTaxFlat;
        }

        public static int GetLandTax(ATMConfig taxConfig, float daLionConservationist = 0)
        {
            float tax = GetFarmTileCounts() * GetUnitaryLandTax(taxConfig.landTaxFlat) * Convert.ToInt32(taxConfig.landTaxEnable);

            if (taxConfig.conservationistApplyToLandTax)
            {
                tax += tax * daLionConservationist;
            }

            return tax < 0 ? 0 : (int)tax;
        }


    }
}

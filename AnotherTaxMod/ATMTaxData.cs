using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace AnotherTaxMod
{
    public class ATMTaxData
    {

        // Properties
        public bool InDebt { get; set; } = false;
        public bool PaymentAvailable { get; set; } = false;
        public int Balance { get; set; } = 0;
        public int DebtIncreaseAmount { get; set; } = 0;
        public int SoldShipped { get; set; } = 0;
        public int SoldLocally { get; set; } = 0;



        /*public Dictionary<string, int> SoldLocally { get; set; } = new Dictionary<string, int>
        {
            { "Mineral", 0 },       //category_gem & category_minerals
            { "Fish", 0 },          //category_fish & *category_sell_at_fish_shop
            { "AnimalProduct", 0 }, //category_egg & category_milk & category_meat & category_sell_at_pierres_and_marnies
            { "Cooking", 0 },       //category_cooking & category_ingredients
            { "Crafting", 0 },      //category_crafting & *category_big_craftable
            { "Resource", 0 },      //category_building_resources & category_metal_resources
            { "Junk", 0 },          //category_junk & *category_litter
            { "Misc", 0 },          //category_fertilizer & category_bait & category_tackle
            { "Furniture", 0 },     //category_furniture
            { "ArtisanGood", 0 },   //category_artisan_goods
            { "Syrup", 0 },         //category_syrup
            { "MonsterLoot", 0 },   //category_monster_loot
            { "Seed", 0 },          //category_seeds
            { "Vegetable", 0 },     //category_vegetable & *category_sell_at_pierres
            { "Fruit", 0 },         //category_fruits
            { "Flower", 0 },        //category_flowers
            { "Forage", 0 },        //category_greens
            { "Clothing", 0 },      //*category_equipment & *category_hat & *category_boots & *category_clothing
            { "Accessory", 0 },     //*category_trinket & *category_ring
            { "Tool", 0 },          //*category_tool & *category_weapon
            { "Book", 0 }           //*category_books & *category_skill_books
        };*/

        public void EnsureDefaults()
        {
            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                var defaultValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;

                if (property.GetValue(this) == null)
                {
                    property.SetValue(this, defaultValue);
                }
            }
        }

        public int PayTaxes()
        {
            int playermoney = Game1.player.Money;
            int taxpayed = 0;
            if (Balance <= playermoney)
            {
                taxpayed = Balance;
            }
            else
            {
                taxpayed = playermoney;
            }
            Game1.player.Money -= taxpayed;
            Balance -= taxpayed;
            return taxpayed;
        }

        public void AssessTax()
        {
            if (Balance <= 0)
            {
                Balance = 0;
                PaymentAvailable = false;
                InDebt = false;
                DebtIncreaseAmount = 0;
            }
        }

        public static int getTotalTax(ATMTaxData taxData, ATMConfig taxConfig, float daLionConservationist = 0)
        {
            return ATMIncomeTax.GetIncomeTax(taxData, taxConfig, daLionConservationist) + ATMLandTax.GetLandTax(taxConfig, daLionConservationist) + ATMBuildingTax.GetBuildingTax(taxConfig, daLionConservationist);
        }
    }
}

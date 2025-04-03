using System;
using System.Collections.Immutable;
using System.Linq;
using AnotherTaxMod.Integrations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace AnotherTaxMod
{

    /// <summary>The mod entry point.</summary>
    internal sealed class ATMMain : Mod
    {

        public int TillableLand => ATMLandTax.GetFarmTileCounts();

        public ATMTaxData? taxData;
        public ATMConfig? taxConfig;
        private IDaLionWalkOfLifeApi api;
        private List<Item> previousInventory; // Store player's inventory before selling
        float result;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.taxConfig = Helper.ReadConfig<ATMConfig>();
            
            Helper.Events.GameLoop.GameLaunched += this.GameLaunched;

            Helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            Helper.Events.GameLoop.DayEnding += this.OnDayEnding;

            Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

            Helper.Events.GameLoop.Saving += this.OnSaving;

            Helper.Events.GameLoop.DayStarted += this.OnDayStarded;

            Helper.Events.Player.Warped += this.OnPlayerWarped;

            Helper.Events.Display.MenuChanged += this.OnMenuChanged;

        }




        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (e.Button == taxConfig.keybindTaxMenu) // Example: Open menu with "I" key
            {
                if (Game1.activeClickableMenu is ATMTaxStatementMenu)
                {
                    Game1.playSound("bigDeSelect"); // Play closing sound
                    Game1.exitActiveMenu(); // Close if already open
                }
                else if (Game1.activeClickableMenu is null)
                {
                    Game1.playSound("bigSelect"); // Play open sound
                    float daLionConservationist;
                    daLionConservationist = api != null ? api.GetConservationistTaxDeduction(Game1.player) : 0f;
                    daLionConservationist = daLionConservationist > 1.0f ? 1.0f : daLionConservationist;
                    Game1.activeClickableMenu = new ATMTaxStatementMenu(Helper, Monitor, ModManifest, taxData, taxConfig, daLionConservationist);
                    //Monitor.Log($"Tax Deduction: {conservationist}", LogLevel.Info);

                    if (taxConfig.extraCalculationLog)
                    {
                        Monitor.Log($"{Helper.Translation.Get("taxlog.incometaxtitle")}", LogLevel.Info);
                        Monitor.Log($"{Helper.Translation.Get("taxlog.incomeshipped", new { incomeShipped = taxData.SoldShipped })}", LogLevel.Info);
                        Monitor.Log($"{Helper.Translation.Get("taxlog.incomelocal", new { incomeLocal = taxData.SoldLocally })}", LogLevel.Info);
                        Monitor.Log($"{Helper.Translation.Get("taxlog.incometaxtotal", new { incomeTaxTotal = ATMIncomeTax.GetIncomeTax(taxData, taxConfig, daLionConservationist) })}", LogLevel.Info);
                        if (taxConfig.incomeTaxAdministrationEnable)
                        {
                            double policechance = ATMIncomeTax.GetPoliceChance(taxData, taxConfig);
                            int penalty = ATMIncomeTax.GetFraudPenalty(taxData, taxConfig, policechance);
                            Monitor.Log($"Police Chance: {(policechance*100)}%", LogLevel.Info);
                            Monitor.Log($"Potential Penalty: {penalty}G", LogLevel.Info);
                        }

                        Monitor.Log("LAND TAX:", LogLevel.Info);
                        Monitor.Log($"NumberOfTiles ({ATMLandTax.GetFarmTileCounts()}) * TilePrice Year {Game1.year} ({ATMLandTax.GetUnitaryLandTax(taxConfig.landTaxFlat)}G) = {ATMLandTax.GetLandTax(taxConfig, daLionConservationist)}G", LogLevel.Info);

                        Monitor.Log("BUILDING TAX:", LogLevel.Info);
                        Dictionary<string, int> buildingDict = ATMBuildingTax.GetBuildingEnum(taxConfig);
                        foreach (var pair in buildingDict)
                            Monitor.Log($"{pair.Key}: {pair.Value} = {ATMBuildingTax.GetBuildingTaxFlat(pair.Key, taxConfig) * pair.Value}G", LogLevel.Info);
                        int numberBuildings = ATMBuildingTax.GetBuildingCount(taxConfig, buildingDict);
                        Monitor.Log($"Number of Buildings: {numberBuildings},  Multiplier: {ATMBuildingTax.GetBuildingCountMult(taxConfig, numberBuildings)}", LogLevel.Info);
                        Monitor.Log($"Year: {Game1.year},  Multiplier: {ATMBuildingTax.GetBuildingYearMult(taxConfig)}", LogLevel.Info);
                        Monitor.Log($"Total Buildings = {ATMBuildingTax.GetBuildingTax(taxConfig, daLionConservationist)}G", LogLevel.Info);
                        Monitor.Log("OTHERS:", LogLevel.Info);
                        Monitor.Log($"Conservationist Reduction: {daLionConservationist}", LogLevel.Info);

                    }
                    
                }
            }
        }

        /*********
        ** Private methods
        *********/
        /// <summary>called when day start</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            taxData = Helper.Data.ReadSaveData<ATMTaxData>("ATMTaxData") ?? new ATMTaxData();
            taxData.EnsureDefaults();
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData("ATMTaxData", taxData);
        }

        private void OnDayStarded(object sender, DayStartedEventArgs e)
        {
            if (Game1.dayOfMonth == 1)
            {
                if (!(Game1.year == 1 && Game1.currentSeason == "spring"))
                {
                    if (!(taxConfig.taxExemptionSpring1 && (Game1.year == 1 && Game1.currentSeason == "summer")))
                    {
                        taxData.PaymentAvailable = true;

                        float daLionConservationist;
                        daLionConservationist = api != null ? api.GetConservationistTaxDeduction(Game1.player) : 0f;
                        daLionConservationist = daLionConservationist > 1.0f ? 1.0f : daLionConservationist;
                        taxData.Balance += ATMTaxData.getTotalTax(taxData, taxConfig, daLionConservationist);

                        if (taxConfig.incomeTaxAdministrationEnable && !taxConfig.incomeTaxLocalShopEnable)
                        {
                            int penalty = ATMIncomeTax.GetFraudPenalty(taxData, taxConfig, ATMIncomeTax.GetPoliceChance(taxData, taxConfig));
                            if (penalty > 0)
                            {
                                Game1.showGlobalMessage($"Incurred a {penalty}G penalty for fraud!");
                                taxData.Balance += penalty;
                                taxData.DebtIncreaseAmount += penalty;
                            }
                        }

                        taxData.SoldLocally = 0;
                        taxData.SoldShipped = 0;
                    }
                }

            }
            else if (Game1.dayOfMonth == 8 && taxData.Balance > 0)
            {
                taxData.InDebt = true;

                //Lateness fine
                int latenessFine = 0;
                if (Game1.year - 1 >= taxConfig.debtLatenessFine.Length)
                {
                    latenessFine = taxConfig.debtLatenessFine[taxConfig.debtLatenessFine.Length - 1];
                }
                else
                {
                    latenessFine = taxConfig.debtLatenessFine[Game1.year - 1];
                }
                taxData.Balance += latenessFine;
                taxData.DebtIncreaseAmount += latenessFine;

            }

            if (taxData.PaymentAvailable)
            {
                Game1.showGlobalMessage("A tax payment is available!");
            }

            if (taxData.InDebt)
            {
                Game1.showGlobalMessage("You are in debt!");
                float interest = taxData.Balance * taxConfig.debtDailyIncrease;
                taxData.Balance += interest < 1 ? 1 : (int)interest;
                taxData.DebtIncreaseAmount += interest < 1 ? 1 : (int)interest;
                foreach (var npc in Game1.player.friendshipData.Keys.ToList())
                {
                    Game1.player.friendshipData[npc].Points -= taxConfig.debtDailyFriendshipDecrease; // Reduce by ? points
                }
            }
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            if (taxData != null)
                taxData.SoldShipped += ATMIncomeTax.GetShippingBinSold();
        }


        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //Load Walks of Life API
            api = Helper.ModRegistry.GetApi<IDaLionWalkOfLifeApi>("DaLion.Professions");
            // load gmcm
            var configMenuHandler = new ATMConfigMenu(Helper, Monitor, ModManifest, taxConfig);
            configMenuHandler.RegisterMenu();

        }

        // Event handler for player warping
        private void OnPlayerWarped(object sender, EventArgs e)
        {
            if (taxConfig.debtBlockCommunityCenter && taxData.InDebt)
            {
                if (Game1.player.currentLocation is CommunityCenter)
                {
                    // Prevent entering and display a message
                    Game1.drawObjectDialogue("Due to your debt the Community Center is currently off-limits. Access is blocked for now.");

                    // Warp the player back to the entrance of the Community Center (exit point)
                    Game1.warpFarmer("Town", 52, 20, true);
                }
            }

            if (taxConfig.debtBlockJoja && taxData.InDebt)
            {
                // Check if the player is trying to enter the Community Center
                if (Game1.player.currentLocation is JojaMart)
                {
                    // Prevent entering and display a message
                    Game1.drawObjectDialogue("Due to your debt Joja is closed for you. Your Access is blocked for now.");

                    // Warp the player back to the entrance of the Community Center (exit point)
                    Game1.warpFarmer("Town", 95, 51, true);
                }
            }
            
        }

        private void OnMenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (taxConfig.debtBlockBuildingShop)
            {
                if (e.NewMenu is CarpenterMenu carpenterMenu && taxData.InDebt)
                {
                    // Close the menu
                    Game1.exitActiveMenu();
                    Game1.drawObjectDialogue("Due to your debt Robin's Building Shop is currently unavailable.");
                }
            }

            if (e.NewMenu is ShopMenu)
            {
                // Capture current inventory when a shop opens
                previousInventory = Game1.player.Items
                .Where(item => item != null)
                .Select(item => {
                    Item copy = item.getOne();
                    copy.Stack = item.Stack; // Copy stack size manually
                    return copy;
                })
                .ToList();
                /*foreach (var item in previousInventory)
                {
                    Monitor.Log($"Item: {item.Name} Quality {item.Quality} : {item.Stack} : {item.sellToStorePrice()* item.Stack} G", LogLevel.Info);
                }*/

            }
            if (e.OldMenu is ShopMenu)
            {
                //Compare current and previous inventory to know what was sold.
                taxData.SoldLocally += ATMIncomeTax.GetLocalStoreSold(previousInventory);
            }
        }

    }


}

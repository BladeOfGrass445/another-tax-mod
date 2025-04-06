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
using StardewValley.Inventories;
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
        private Dictionary<(string Name, int Quality), (Item Item, int Count)> previousInventory; // Store player's inventory before selling
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

            Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;

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
                        Monitor.Log(Helper.Translation.Get("taxlog.incometaxtitle"), LogLevel.Info);
                        Monitor.Log(Helper.Translation.Get("taxlog.incomeshipped", new { incomeShipped = taxData.SoldShipped }), LogLevel.Info);
                        Monitor.Log(Helper.Translation.Get("taxlog.incomelocal", new { incomeLocal = taxData.SoldLocally }), LogLevel.Info);
                        Monitor.Log(Helper.Translation.Get("taxlog.incometaxtotal", new { incomeTaxTotal = ATMIncomeTax.GetIncomeTax(taxData, taxConfig, daLionConservationist) }), LogLevel.Info);
                        if (taxConfig.incomeTaxAdministrationEnable)
                        {
                            double policechance = ATMIncomeTax.GetPoliceChance(taxData, taxConfig);
                            int penalty = ATMIncomeTax.GetFraudPenalty(taxData, taxConfig, policechance);
                            Monitor.Log(Helper.Translation.Get("taxlog.frauddiscovery", new { fraudChance = string.Format("{0:0.##}", ATMIncomeTax.GetPoliceChance(taxData, taxConfig) * 100) }), LogLevel.Info);
                            Monitor.Log(Helper.Translation.Get("taxlog.penalty", new { penalty = ATMIncomeTax.GetFraudPenalty(taxData, taxConfig, policechance) }), LogLevel.Info);
                        }

                        Monitor.Log(Helper.Translation.Get("taxlog.landtaxtitle"), LogLevel.Info);
                        Monitor.Log(Helper.Translation.Get("taxlog.landtaxdetails", new { farmTileCount = ATMLandTax.GetFarmTileCounts(), year = Game1.year, unitaryTilePrice = ATMLandTax.GetUnitaryLandTax(taxConfig.landTaxFlat), landTaxTotal = ATMLandTax.GetLandTax(taxConfig, daLionConservationist) }), LogLevel.Info);

                        Monitor.Log(Helper.Translation.Get("taxlog.buildingtaxtitle"), LogLevel.Info);
                        Dictionary<string, int> buildingDict = ATMBuildingTax.GetBuildingEnum(taxConfig);
                        foreach (var pair in buildingDict)
                            Monitor.Log($"{pair.Key}: {pair.Value} = {ATMBuildingTax.GetBuildingTaxFlat(pair.Key, taxConfig) * pair.Value} G", LogLevel.Info);
                        int numberBuildings = ATMBuildingTax.GetBuildingCount(taxConfig, buildingDict);
                        Monitor.Log(Helper.Translation.Get("taxlog.numberbuildingtax", new { numberBuildings = numberBuildings, numberBuildingsMultiplier = ATMBuildingTax.GetBuildingCountMult(taxConfig, numberBuildings) }), LogLevel.Info);
                        Monitor.Log(Helper.Translation.Get("taxlog.yearbuildingtax", new { year = Game1.year, yearBuildingMultiplier = ATMBuildingTax.GetBuildingYearMult(taxConfig) }), LogLevel.Info);
                        Monitor.Log(Helper.Translation.Get("taxlog.buildingtaxtotal", new { buildingTaxTotal = ATMBuildingTax.GetBuildingTax(taxConfig, daLionConservationist) }), LogLevel.Info);
                        Monitor.Log(Helper.Translation.Get("taxlog.othertitle"), LogLevel.Info);
                        Monitor.Log(Helper.Translation.Get("taxlog.otherconservationist", new { daLionConservationist = daLionConservationist }), LogLevel.Info);

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
                                Game1.showGlobalMessage(Helper.Translation.Get("message.penalty", new { penalty = penalty }));
                                taxData.Balance += penalty;
                                taxData.DebtIncreaseAmount += penalty;
                            }
                        }

                    }
                }
                taxData.SoldLocally = 0;
                taxData.SoldShipped = 0;

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
                Game1.showGlobalMessage(Helper.Translation.Get("message.paymentavailable"));
            }

            if (taxData.InDebt)
            {
                Game1.showGlobalMessage(Helper.Translation.Get("message.indebt"));
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
                    Game1.drawObjectDialogue(Helper.Translation.Get("message.blockedcommunitycenter"));

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
                    Game1.drawObjectDialogue(Helper.Translation.Get("message.blockedjoja"));

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
                    Game1.drawObjectDialogue(Helper.Translation.Get("message.blockedrobinbuild"));
                }
            }

            if (e.NewMenu is ShopMenu)
            {
                // Capture current inventory when a shop opens
                previousInventory = ATMIncomeTax.CopyInventoryToDict(Game1.player.Items);
                if (taxConfig.debugLog)
                {
                    foreach (var entry in previousInventory)
                    {
                        Monitor.Log($"Item: {entry.Key.Name} Quality {entry.Key.Quality} : {entry.Value.Count} : {entry.Value.Item.sellToStorePrice() * entry.Value.Count} G", LogLevel.Debug);
                    }
                }
            }
        }

        // Event handler to detect inventory changes
        private void OnInventoryChanged(object sender, EventArgs e)
        {
            // Check if the current menu is a ShopMenu
            if (Game1.activeClickableMenu is ShopMenu)
            {
                // It's a shop menu, let's track the inventory change
                Dictionary<(string Name, int Quality), (Item Item, int Count)> currentInventory = ATMIncomeTax.CopyInventoryToDict(Game1.player.Items);

                // Compare the previous inventory with the current one
                foreach (var previousEntry in previousInventory)
                {
                    // Check if the item is no longer in the current inventory
                    if (!currentInventory.ContainsKey(previousEntry.Key))
                    {
                        // This item has been sold (removed from inventory)
                        if (taxConfig.debugLog)
                            Monitor.Log($"Item sold: {previousEntry.Key.Name} (Quality: {previousEntry.Key.Quality}, Count: {previousEntry.Value.Count}) = {previousEntry.Value.Item.sellToStorePrice() * previousEntry.Value.Count} G", LogLevel.Debug);
                        taxData.SoldLocally += previousEntry.Value.Item.sellToStorePrice() * previousEntry.Value.Count;
                    }
                    else if (currentInventory[previousEntry.Key].Count < previousEntry.Value.Count)
                    {
                        // The stack size of this item has decreased (partially sold)
                        int soldAmount = previousEntry.Value.Count - currentInventory[previousEntry.Key].Count;
                        if (taxConfig.debugLog)
                            Monitor.Log($"Partial sale: {soldAmount} of {previousEntry.Key.Name} (Quality: {previousEntry.Key.Quality}) = {previousEntry.Value.Item.sellToStorePrice() * soldAmount} G", LogLevel.Debug);
                        taxData.SoldLocally += previousEntry.Value.Item.sellToStorePrice() * soldAmount;
                    }
                }

                // Update the previous inventory to the current one after the check
                previousInventory = currentInventory;
            }
        }
    }


}

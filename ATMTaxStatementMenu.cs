using System;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace AnotherTaxMod
{
    public class ATMTaxStatementMenu : IClickableMenu
    {
        private readonly IModHelper Helper;
        private readonly IMonitor Monitor;
        private readonly IManifest ModManifest;
        private ClickableTextureComponent payButton;
        private ATMTaxData? taxData;
        private ATMConfig? taxConfig;
        private float daLionConservationist;



        public ATMTaxStatementMenu(IModHelper helper, IMonitor monitor, IManifest manifest, ATMTaxData taxData, ATMConfig taxConfig, float daLionConservationist = 0) : base(0, 0, Game1.viewport.Width, Game1.viewport.Height)
        {
            Helper = helper;
            Monitor = monitor;
            ModManifest = manifest;
            this.taxData = taxData;
            this.taxConfig = taxConfig;
            this.daLionConservationist = daLionConservationist;

            // Center the menu
            width = 800;
            height = 700;
            xPositionOnScreen = (Game1.viewport.Width - width) / 2;
            yPositionOnScreen = (Game1.viewport.Height - height) / 2;

            // Create the PAY button, centered
            int buttonWidth = 200;
            int buttonHeight = 80;
            payButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + (width / 2) - (buttonWidth / 2), yPositionOnScreen + height - 110, buttonWidth, buttonHeight),
                Game1.mouseCursors, new Rectangle(0, 0, 64, 64), 1f);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            base.draw(spriteBatch);

            // Draw background
            IClickableMenu.drawTextureBox(spriteBatch, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            // Centered Title
            string title = Helper.Translation.Get("taxmenu.title");
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Vector2 titlePosition = new Vector2(
                xPositionOnScreen + (width / 2) - (titleSize.X / 2),
                yPositionOnScreen + 40);
            spriteBatch.DrawString(Game1.dialogueFont, title, titlePosition, Color.Black);

            // Left-aligned tax details
            int textX = xPositionOnScreen + 50;
            int textY = yPositionOnScreen + 160;
            int spacing = 40;

            spriteBatch.DrawString(Game1.smallFont, "---------------", new Vector2(textX, textY - 0.8f * spacing), Color.Black);
            
            spriteBatch.DrawString(Game1.smallFont, Helper.Translation.Get("taxmenu.incometaxtitle", new { incomeTax = ATMIncomeTax.GetIncomeTax(taxData, taxConfig, daLionConservationist) }), new Vector2(textX, textY), Color.Black);
            spriteBatch.DrawString(Game1.smallFont, "---------------", new Vector2(textX, textY + 0.7f * spacing), Color.Black);

            if (taxConfig.incomeTaxAdministrationEnable)
            {
                spriteBatch.DrawString(Game1.smallFont, Helper.Translation.Get("taxmenu.frauddiscovery", new { fraudChance = string.Format("{0:0.##}", ATMIncomeTax.GetPoliceChance(taxData, taxConfig)*100) } ), new Vector2(textX + 450, textY), Color.Black);
            }
            
            spriteBatch.DrawString(Game1.smallFont, Helper.Translation.Get("taxmenu.landtaxtitle", new { landTax = ATMLandTax.GetLandTax(taxConfig, daLionConservationist) } ), new Vector2(textX, textY + 1.5f * spacing), Color.Black);
            spriteBatch.DrawString(Game1.smallFont, "---------------", new Vector2(textX, textY + 2.2f * spacing), Color.Black);

            spriteBatch.DrawString(Game1.smallFont, Helper.Translation.Get("taxmenu.buildingtaxtitle", new { buildingTax = ATMBuildingTax.GetBuildingTax(taxConfig, daLionConservationist) }), new Vector2(textX, textY + 3 * spacing), Color.Black);
            spriteBatch.DrawString(Game1.smallFont, "---------------", new Vector2(textX, textY + 3.7f * spacing), Color.Black);

            // Total
            spriteBatch.DrawString(Game1.smallFont, Helper.Translation.Get("taxmenu.totaltaxtitle", new { totalTax = ATMTaxData.getTotalTax(taxData, taxConfig, daLionConservationist) }), new Vector2(textX, textY + 4.5f * spacing), Color.Black);
            spriteBatch.DrawString(Game1.smallFont, "---------------", new Vector2(textX, textY + 5.3f * spacing), Color.Black);

            spriteBatch.DrawString(Game1.smallFont, Helper.Translation.Get("taxmenu.debtinteresttitle", new { debtInterest = taxData.DebtIncreaseAmount }), new Vector2(textX, textY + 6.1f * spacing), Color.Black);
            spriteBatch.DrawString(Game1.smallFont, "---------------", new Vector2(textX, textY + 6.9f * spacing), Color.Black);

            spriteBatch.DrawString(Game1.dialogueFont, Helper.Translation.Get("taxmenu.balance", new { balance = taxData.Balance }), new Vector2(textX, textY + 8f * spacing), Color.Black);



            // Check if button should be visible
            if (taxData.PaymentAvailable && Game1.player.Money > 0)
            {
                // Draw the PAY button
                IClickableMenu.drawTextureBox(spriteBatch, payButton.bounds.X, payButton.bounds.Y, payButton.bounds.Width, payButton.bounds.Height, Color.Yellow);
            }
            else
            {
                // Draw the PAY button
                IClickableMenu.drawTextureBox(spriteBatch, payButton.bounds.X, payButton.bounds.Y, payButton.bounds.Width, payButton.bounds.Height, Color.Gray);
            }
            // Draw "PAY" text centered inside the button
            string payText = Helper.Translation.Get("taxmenu.button"); ;
            Vector2 payTextSize = Game1.dialogueFont.MeasureString(payText);
            Vector2 payTextPosition = new Vector2(
                payButton.bounds.X + (payButton.bounds.Width / 2) - (payTextSize.X / 2),
                payButton.bounds.Y + (payButton.bounds.Height / 2) - (payTextSize.Y / 2)
            );
            spriteBatch.DrawString(Game1.dialogueFont, payText, payTextPosition, Color.Black);
           
            // Make cursor visible
            drawMouse(spriteBatch);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (payButton.containsPoint(x, y))
            {
                if (taxData.PaymentAvailable && Game1.player.Money > 0)
                {
                    Game1.playSound("purchaseClick"); // Play coin sound
                    int amountpayed = taxData.PayTaxes();
                    Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("message.paidtaxes", new { amountPayed = amountpayed }), HUDMessage.achievement_type));
                    Game1.playSound("purchase"); // Play coin sounds
                    taxData.AssessTax();
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("message.cannotpay"), HUDMessage.error_type));
                    Game1.playSound("cancel"); // Play cancel sound
                }
            }
            
        }

        

        
    }
}


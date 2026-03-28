using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace GiftTasteHighlighter
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Display.RenderedHud += OnRenderedHud;
        }


        //When inventory open 
        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu gameMenu)
            {
                var inventoryPage = gameMenu.GetCurrentPage() as InventoryPage;

                if (inventoryPage != null)
                {
                    DrawTestHighlight(e.SpriteBatch, inventoryPage);
                }
            }
        }


        //When the inventory is open 
        private void DrawTestHighlight(SpriteBatch spriteBatch, InventoryPage inventoryPage)
        {
            var inventoryMenu = inventoryPage.inventory;

            for (int i = 0; i < inventoryMenu.inventory.Count; i++)
            {   
                var slot = inventoryMenu.inventory[i];

                Item? item = (i < Game1.player.Items.Count) ? Game1.player.Items[i] : null;
                Color highlightColor = GetHighlightColor(item);

                spriteBatch.Draw(
                    Game1.staminaRect,
                    slot.bounds,
                    highlightColor
                );
            }
        }


                //When the hotbar is visible
        private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
    
        if (Game1.activeClickableMenu == null && Game1.displayHUD)
        {
            DrawTestHighlighthotbar(e.SpriteBatch);
        }
        }

        private void DrawTestHighlighthotbar(SpriteBatch spriteBatch)
        {
            var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
            if (toolbar == null) return;

            for (int i = 0; i < toolbar.buttons.Count; i++)
            {
                var slot = toolbar.buttons[i];

                Item? item = (i < Game1.player.Items.Count) ? Game1.player.Items[i] : null;

                Color highlightColor = GetHighlightColor(item);

                spriteBatch.Draw(
                    Game1.staminaRect,
                    slot.bounds,
                    highlightColor
                );
            }
        }


        private Item? GetItemUnderCursor()
        {
            if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.GetCurrentPage() is InventoryPage inventoryPage)
            {
                return inventoryPage.inventory.getItemAt(Game1.getMouseX(), Game1.getMouseY());
            }
    
            var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
            if (toolbar != null)
            {
                foreach (var button in toolbar.buttons)
                {
                    if (button.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        int slotIndex = toolbar.buttons.IndexOf(button);
                        if (slotIndex >= 0 && slotIndex < Game1.player.Items.Count)
                            return Game1.player.Items[slotIndex];
                    }
                }
            }
            return null;
        }

        private Color GetHighlightColor(Item item)
        {
            NPC? nearbyNpc = Game1.currentLocation.characters
                 .FirstOrDefault(c => Vector2.Distance(Game1.player.Tile, c.Tile) <= 2f);
        
            if (nearbyNpc == null || item == null) 
                return Color.Red * 0f; 

            // getGiftTasteForThisItem returns: 0 (Love), 2 (Like), 4 (Dislike), 6 (Hate), 8 (Neutral)
            int taste = nearbyNpc.getGiftTasteForThisItem(item);

            return taste switch
            {
                0 => Color.Purple * 0.4f, // Loved
                2 => Color.Green * 0.4f,  // Liked
                4 => Color.Orange * 0.4f, // Disliked
                6 => Color.Red * 0.4f,  // Hated
                _ => Color.Gray * 0.4f    // Neutral
            };
        }
    }
}
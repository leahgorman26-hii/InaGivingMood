using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using StardewValley.Characters;
using StardewValley.Objects;

namespace GiftTasteHighlighter
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Display.Rendered += OnRendered;
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
                    DrawGiftTasteIcons(e.SpriteBatch);
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
            DrawGiftTasteIcons(e.SpriteBatch);
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

private Color GetHighlightColor(Item? item)
{
    if (item == null)
        return Color.Red * 0f;

    // Check nearby farm animals (separate from NPC hierarchy)
    FarmAnimal? nearbyAnimal = (Game1.currentLocation as AnimalHouse)?.animals.Values
        .FirstOrDefault(a => Vector2.Distance(Game1.player.Tile, a.Tile) <= 2f);

    if (nearbyAnimal != null)
    {
        return item.Name == "Hay"
            ? Color.Green * 0.4f
            : Color.Red * 0f;
    }

    // Check nearby NPCs
    NPC? nearbyNpc = Game1.currentLocation.characters
        .FirstOrDefault(c => Vector2.Distance(Game1.player.Tile, c.Tile) <= 2f);

    if (nearbyNpc == null)
        return Color.Red * 0f;

    if (nearbyNpc.IsVillager && Game1.player.friendshipData.ContainsKey(nearbyNpc.Name))
    { 
        if (nearbyNpc.CanReceiveGifts() && item.canBeGivenAsGift())
        {
            int taste = nearbyNpc.getGiftTasteForThisItem(item);
            return taste switch
            {
                0 => Color.Purple * 0.4f, // Loved
                2 => Color.Green * 0.4f,  // Liked
                4 => Color.Orange * 0.4f, // Disliked
                6 => Color.Red * 0.4f,    // Hated
                _ => Color.Gray * 0.4f    // Neutral
            };
        }
        return Color.Red * 0f;
    }

    return Color.Red * 0f;
}

        private Item? GetItemUnderCursor() { 
            if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.GetCurrentPage() is InventoryPage inventoryPage) 
            { 
                return inventoryPage.inventory.getItemAt(Game1.getMouseX(), Game1.getMouseY()); 
            } 
                
            var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault(); 
            
            if (toolbar != null) 
            { 
                foreach (var button in toolbar.buttons) { 
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

        private void DrawGiftTasteIcons(SpriteBatch spriteBatch)
        {
            var item = GetItemUnderCursor();
            if (item == null) 
                return;

            var npcs = Utility.getAllCharacters();

            var loved = new List<NPC>();
            var liked = new List<NPC>();
            var disliked = new List<NPC>();

            foreach (var npc in npcs)
            {
                if (!(npc ==null) && npc.CanReceiveGifts() && !(npc.Portrait == null))
                {
                    int taste = npc.getGiftTasteForThisItem(item);

                    if (taste == 0) loved.Add(npc);
                    else if (taste == 2) liked.Add(npc);
                    else if (taste == 4) disliked.Add(npc);
                }
            }


            int x = 0;
            int y = 0;

            InventoryPage? inventoryPage = null;

            if (Game1.activeClickableMenu is GameMenu gameMenu)
            {
                inventoryPage = gameMenu.GetCurrentPage() as InventoryPage;
            }

            var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();

            bool toolbarAtTop = toolbar != null && toolbar.yPositionOnScreen < Game1.viewport.Height / 2;
            bool inventoryOpen = inventoryPage != null;

            if (toolbarAtTop || inventoryOpen)
            {
                x = Game1.getMouseX() - 10;
                y = Game1.getMouseY() + 80;
            }
            else
            {
                x = Game1.getMouseX() - 5;
                y = Game1.getMouseY() - 200;
            }
            

            int size = 32;
            int spacing = 4;

            int totalIcons = loved.Count + liked.Count;
            int height = totalIcons * (size + spacing);
            int width = size;

            int totalGiftable = npcs.Count(n => n.CanReceiveGifts());

            bool universallyLoved = loved.Count == totalGiftable;
            bool universallyLiked = liked.Count + loved.Count > totalGiftable - 6;



            if (universallyLoved)
            {   
                height=32;
                
                IClickableMenu.drawTextureBox(
                    spriteBatch,
                    x + 10,
                    y - 12,
                    width + 24,
                    height + 24,
                    Color.White
                );

                
                //Draw heart
                int scale = 5;

                spriteBatch.Draw(
                Game1.mouseCursors,
                new Rectangle(x + size - 12, y + 2, 7 * scale, 6 * scale),
                new Rectangle(211, 428, 7, 6),
                Color.White
                );

                return;
            }

            else if (universallyLiked)
            {   
                totalIcons = disliked.Count +1;
                height = totalIcons * (size + spacing);

                IClickableMenu.drawTextureBox(
                    spriteBatch,
                    x - 12,
                    y - 12,
                    width + 24,
                    height + 24,
                    Color.White
                );

                int scale = 2;

                //Draw heart
                spriteBatch.Draw(
                Game1.mouseCursors,
                new Rectangle(x + size - 12, y + 2,  7 * scale, 6 * scale), // position + scale
                new Rectangle(211, 428, 7, 6),
                Color.White
                );

                //disliked list and diplay
                foreach (var npc in disliked)
                {          
                    y += size + spacing;

                    spriteBatch.Draw(npc.Portrait,
                    new Rectangle(x, y, size, size),
                    new Rectangle(0, 0, 64, 64),
                    Color.White);

                    //Adding an X
                    scale = 2;
                    spriteBatch.Draw(
                    Game1.mouseCursors,
                    new Rectangle(x + size - 12, y + 2, 7 * scale, 6 * scale),
                    new Rectangle(268, 470, 15, 16),
                    Color.White
                    );

                }

                return;
            }

            if (liked.Count > 0 || loved.Count > 0) 
            {
                IClickableMenu.drawTextureBox(
                    spriteBatch,
                    x - 12,
                    y - 12,
                    width + 24,
                    height + 24,
                    Color.White
                );


                foreach (var npc in loved)
                {                
                    spriteBatch.Draw(npc.Portrait,
                    new Rectangle(x, y, size, size),
                    new Rectangle(0, 0, 64, 64),
                    Color.White);

                    //Draw heart
                    spriteBatch.Draw(
                    Game1.mouseCursors,
                    new Rectangle(x + size - 12, y + 2, 10, 10), // position + scale
                    new Rectangle(211, 428, 7, 6),
                    Color.White
                    );

                    y += size + spacing;
                }   

                foreach (var npc in liked)
                {
                    spriteBatch.Draw(npc.Portrait,
                    new Rectangle(x, y, size, size),
                    new Rectangle(0, 0, 64, 64),
                    Color.White);

                    y += size + spacing;
                }
            }
        }

        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            DrawGiftTasteIcons(e.SpriteBatch);
        }
    }
}

/*
✔Check if character giftable - , animals chickens etc hay???
✔Figure out why honey isn't working
✔Why diamond not showing everyone
✔Made symbol for universally loved
Made symbol for universally liked and -exceptions
✔stop highlighting ungiftable items
Make configuration to only include selected character and to turn on and off features
Add for pop up when in chest

*/


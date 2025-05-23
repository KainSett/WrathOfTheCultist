using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using WrathOfTheCultist.Common.Config;

namespace WrathOfTheCultist.Common.UI;

public class DraggableUIPanel : UIPanel
{
        // Stores the offset from the top left of the UIPanel while dragging
    private Vector2 offset;
        // A flag that checks if the panel is currently being dragged
    private bool dragging;

    public override void LeftMouseDown(UIMouseEvent evt)
    {
            // When the mouse button is down on this element, then we start dragging
        if (evt.Target == this && ModContent.GetInstance<ClientConfig>().EldritchShieldUIDraggable)
        {
            DragStart(evt);
        }
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
            // When the mouse button is up, then we stop dragging
        if (evt.Target == this && ModContent.GetInstance<ClientConfig>().EldritchShieldUIDraggable)
        {
            DragEnd(evt);
        }
    }

    private void DragStart(UIMouseEvent evt)
    {
            // The offset variable helps to remember the position of the panel relative to the mouse position
            // So no matter where you start dragging the panel, it will move smoothly
        if (ModContent.GetInstance<ClientConfig>().EldritchShieldUIDraggable)
        {
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }
    }

    private void DragEnd(UIMouseEvent evt)
    {
        if (ModContent.GetInstance<ClientConfig>().EldritchShieldUIDraggable)
        {
            Vector2 endMousePosition = evt.MousePosition;
            dragging = false;

            Left.Set(endMousePosition.X - offset.X, 0f);
            Top.Set(endMousePosition.Y - offset.Y, 0f);

            Recalculate();

            ModContent.GetInstance<ClientConfig>().EldritchShieldUIPosition = new Vector2(GetOuterDimensions().X / Main.screenWidth, GetOuterDimensions().Y / Main.screenHeight);
        }
    }

    public override void Update(GameTime gameTime)
    {
            // Checking ContainsPoint and then setting mouseInterface to true is very common
            // This causes clicks on this UIElement to not cause the player to use current items
        if (ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;

        if (dragging)
        {
            Left.Set(Main.mouseX - offset.X, 0f); // Main.MouseScreen.X and Main.mouseX are the same
            Top.Set(Main.mouseY - offset.Y, 0f);
            Recalculate();

            ModContent.GetInstance<ClientConfig>().EldritchShieldUIPosition = new Vector2(GetOuterDimensions().X / Main.screenWidth, GetOuterDimensions().Y / Main.screenHeight);
        }
        else
        {
            Left.Set(ModContent.GetInstance<ClientConfig>().EldritchShieldUIPosition.X * Main.screenWidth, 0f);
            Top.Set(ModContent.GetInstance<ClientConfig>().EldritchShieldUIPosition.Y * Main.screenHeight, 0f);
            Recalculate();
        }

            // Here we check if the DraggableUIPanel is outside the Parent UIElement rectangle
            // (In our example, the parent would be ExampleCoinsUI, a UIState. This means that we are checking that the DraggableUIPanel is outside the whole screen)
            // By doing this and some simple math, we can snap the panel back on screen if the user resizes his window or otherwise changes resolution
        var parentSpace = Parent.GetDimensions().ToRectangle();
        if (!GetDimensions().ToRectangle().Intersects(parentSpace))
        {
            Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
            Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                // Recalculate forces the UI system to do the positioning math again.
            Recalculate();
        }
    }
}
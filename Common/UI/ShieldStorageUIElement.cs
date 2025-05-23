using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using WrathOfTheCultist.Common.Config;
using WrathOfTheCultist.Content;

namespace WrathOfTheCultist.Common.UI;

public class EldritchBarrierUIElement : UIElement
{
    private static readonly Vector2 GiftOffset = new(25f);

    private static readonly Color OuterCircleColor = new(70, 160, 150, 150);
    private const float OuterCircleScale = 0.2f;

    private const byte InnerCircleOpacity = 150;
    private const float InnerCircleScale = 0.195f;

    public override void OnInitialize() => IgnoresMouseInteraction = true;

    public override void Draw(SpriteBatch spriteBatch)
    {
        Vector2 center = ModContent.GetInstance<ClientConfig>().EldritchShieldUIPosition * new Vector2(screenWidth, screenHeight) + GiftOffset;

        Texture2D texture = Shield[0].Value;

        Vector2 origin = texture.Size() * 0.5f;

        spriteBatch.Draw(texture, center, null, OuterCircleColor, 0, origin, OuterCircleScale, SpriteEffects.None, 0);

        texture = Shield[1].Value;
        origin = texture.Size() / 2;

        Rectangle rect = texture.Bounds;
        rect.Height = (int)(rect.Height * (Main.LocalPlayer.GetModPlayer<EldritchShieldPlayer>().Barrier.value * (1f / LocalPlayer.GetModPlayer<EldritchShieldPlayer>().max)));

        Color innerColor = ModContent.GetInstance<ClientConfig>().EldritchShieldUIColor;
        innerColor.A = InnerCircleOpacity;

        spriteBatch.Draw(texture, center, rect, innerColor, Pi, origin, InnerCircleScale, SpriteEffects.None, 0);

        texture = Shield[0].Value;
        Vector2 offset = new(texture.Width / 12.5f, 0);
        Utils.DrawBorderString(spriteBatch, $"{Main.LocalPlayer.GetModPlayer<EldritchShieldPlayer>().Barrier.value}", center + offset.RotatedBy(PiOver2), Color.White, 1f, 0.5f, 0.5f);

        texture = Shield[2].Value;
        origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, center, null, Color.White, 0, origin, 1, SpriteEffects.None, 0);
    }
}

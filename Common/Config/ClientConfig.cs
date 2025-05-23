using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WrathOfTheCultist.Common.Config;

public class ClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(false)]
    public bool EldritchShieldUIDraggable;
    
    [DefaultValue(typeof(Vector2), "0.78125, 0.15")]
    public Vector2 EldritchShieldUIPosition;

    [DefaultValue(typeof(Color), "115, 70, 255, 150")]
    public Color EldritchShieldUIColor;
}
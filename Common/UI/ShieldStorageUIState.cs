using Terraria;
using Terraria.UI;

namespace WrathOfTheCultist.Common.UI;

public class EldritchBarrierUIState : UIState
{
    public override void OnInitialize()
    {
        DraggableUIPanel panel = new();
        panel.SetPadding(0);

        panel.Left.Set(Main.screenWidth * 0.78125f, 0f);
        panel.Top.Set(Main.screenHeight * 0.13888f, 0f);

        panel.Width.Set(50f, 0f);
        panel.Height.Set(50f, 0f);

        panel.BackgroundColor = Color.Transparent;
        panel.BorderColor = Color.Transparent;
        Append(panel);

        EldritchBarrierUIElement element = new();
        panel.Append(element);
    }
}

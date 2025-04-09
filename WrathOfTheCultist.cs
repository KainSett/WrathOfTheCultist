
namespace WrathOfTheCultist
{
	public class WrathOfTheCultist : Mod
	{
		public class Personalrandom : ModSystem
        {
            public static float random = 0;
            public override void PostUpdateEverything()
            {
                if (netMode != NetmodeID.MultiplayerClient)
                    random = rand.NextFloat();
            }
        }
	}
}

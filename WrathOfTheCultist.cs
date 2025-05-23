

using ReLogic.Content;

namespace WrathOfTheCultist
{
    public class WrathOfTheCultist : Mod
    {

        private const string prefix = "WrathOfTheCultist/Content/";

        public static readonly Asset<Texture2D>[] Shield = LoadTexture2Ds("Shield", 3);

        // thanks for load stuffs, zen
        private static Asset<Texture2D> LoadTexture2D(string TexturePath)
        {
            if (Main.dedServ)
                return null;

            return ModContent.Request<Texture2D>(prefix + TexturePath);
        }

        private static Asset<Texture2D>[] LoadTexture2Ds(string TexturePath, int count)
        {
            if (Main.dedServ)
                return null;

            Asset<Texture2D>[] textures = new Asset<Texture2D>[count];

            for (int i = 0; i < count; i++)
                textures[i] = ModContent.Request<Texture2D>(prefix + TexturePath + i);

            return textures;
        }
    }
}
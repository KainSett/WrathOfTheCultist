using System.Collections.Generic;
using WrathOfTheCultist.Common.UI;

namespace WrathOfTheCultist.Content;

[AutoloadEquip(EquipType.Shield)]
public class EldritchShield : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.width = 34;
        Item.height = 40;
        Item.defense = 5;
        Item.rare = ItemRarityID.Expert;
        Item.value = Item.buyPrice(0, 5);
        Item.expert = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var p = player.GetModPlayer<EldritchShieldPlayer>();
        p.counter.timer = Math.Max(0, p.counter.timer - 1);
        if (p.counter.timer == 0)
            p.counter.buffer = p.counter.buffer % 1 + 0.35f;


        p.Barrier.value = Math.Min(p.Barrier.value + (int)float.Floor(p.counter.buffer), player.GetModPlayer<EldritchShieldPlayer>().max);
        p.ShieldUIActive = true;


        var proj = Array.Find(projectile, p => p.type == ModContent.ProjectileType<EldritchBarrier>());
        if (proj != null) 
        { 
            proj.ai[1] = p.Barrier.value / (float)player.GetModPlayer<EldritchShieldPlayer>().max;
            proj.timeLeft++;
        }

        if (Main.LocalPlayer.whoAmI == player.whoAmI)
        {
            ModContent.GetInstance<EldritchBarrierUISystem>()?.Show();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<EldritchBarrier>()] == 0)
                NewProjectile(player.GetSource_Accessory(Item, "Eldritch Barrier vfx"), player.Center, Vector2.Zero, ModContent.ProjectileType<EldritchBarrier>(), 0, 0, ai1: 1);
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var max = LocalPlayer.GetModPlayer<EldritchShieldPlayer>().max;

        string text = string.Format(Language.GetTextValue($"Mods.WrathOfTheCultist.Items.{Name}.Tooltip"), $"{max}");

        int index = tooltips.FindIndex(line => line.Name == "Tooltip0");
        if (index != -1)
        {
            text = text.Remove(text.LastIndexOf($"\n"));
            text = text.Remove(text.LastIndexOf($"\n"));
            tooltips[index].Text = text;
        }
    }
}

public class EldritchShieldPlayer : ModPlayer
{
    public bool ShieldUIActive = false;

    public (int value, bool reduce) Barrier = (0, false);

    public (int timer, float buffer) counter = (0, 0);

    public int max = 60;

    public override void ResetEffects()
    {
        if (!ShieldUIActive)
        {
            Barrier.value = 0;
            if (LocalPlayer.whoAmI == Player.whoAmI)
                ModContent.GetInstance<EldritchBarrierUISystem>()?.Hide();
        }

        ShieldUIActive = false;
    }

    public override void PostUpdateEquips()
    {
        max = 20 + (int)LocalPlayer.GetTotalDamage(DamageClass.Magic).ApplyTo(80);
    }

    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (Barrier.value > 0 && info.Damage <= Barrier.value) 
        {
            counter = (300, 0);

            Barrier.value -= info.Damage;
            Player.AddImmuneTime(info.CooldownCounter, Player.longInvince ? 80 : 40);
            Player.immune = true;

            var proj = Array.Find(projectile, p => p.type == ModContent.ProjectileType<EldritchBarrier>() && p.owner == Player.whoAmI);
            if (proj != null)
                proj.ai[0] = 1;

            return true;
        }

        return base.FreeDodge(info);
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers) =>  modifiers.ModifyHurtInfo += EldritchBarrier;

    private void EldritchBarrier(ref Player.HurtInfo info)
    {
        Barrier.reduce = false;

        if (Barrier.value > 0 && info.Damage > Barrier.value)
        {
            info.Damage -= Barrier.value;
            Barrier.reduce = true;

            var proj = Array.Find(projectile, p => p.type == ModContent.ProjectileType<EldritchBarrier>() && p.owner == Player.whoAmI);
            if (proj != null)
                proj.ai[0] = 1;
        }
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        counter = (300, 0);

        if (Barrier.reduce)
            Barrier = (0, false);
    }
}

public class EldritchBarrier : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.width = 60;
        Projectile.height = 60;
        Projectile.netImportant = true;
        Projectile.light = 0.4f;
    }

    public override void AI()
    {
        Projectile.width = (int)(128 * Projectile.scale);
        Projectile.height = (int)(128 * Projectile.scale);

        if (player[Projectile.owner].active && player[Projectile.owner].GetModPlayer<EldritchShieldPlayer>().Barrier.value > 0)
            Projectile.Center = player[Projectile.owner].Center;
        else
        {
            Projectile.Kill();
            return;
        }

        Projectile.ai[2]++;

        if (Projectile.ai[2] % 60 == 0)
            vfxThingies = DefaultVFXThingies;

        for (int i = 0; i < vfxThingies.Count - 1; i++)
            vfxThingies[i] += new Vector3(0, 0, 1 / 60f);

        Projectile.rotation += PiOver4 / 6;

        Projectile.ai[0] = Math.Max(0, Projectile.ai[0] - 0.01f);
    }

    public List<Vector3> vfxThingies = DefaultVFXThingies;

    public static List<Vector3> DefaultVFXThingies
    {
        get => [new Vector3(rand.NextVector2Circular(60, 60), 0), 
            new Vector3(rand.NextVector2Circular(60, 60), 0), 
            new Vector3(rand.NextVector2Circular(60, 60), 0), 
            new Vector3(rand.NextVector2Circular(60, 60), 0),
            new Vector3(rand.NextVector2Circular(60, 60), 0)];
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float scale = Projectile.scale * 0.2f;

        lightColor = Projectile.ai[0] == 0 ? lightColor : Color.MediumPurple * (0.3f + float.Pow(2f * Projectile.ai[0] - 1f, 2));

        Texture2D texture = TextureAssets.Extra[98].Value;

        for (int i = 0; i < vfxThingies.Count; i++)
        {
            var power = Math.Max(0, 80 + 100f * Projectile.ai[1] + Math.Min(0, 44f - vfxThingies[i].Z * 60f) * 10) / 255f;
            var target = new Vector2(vfxThingies[i].X, vfxThingies[i].Y);
            spriteBatch.Draw(texture, Projectile.Center + target * vfxThingies[i].Z * 0.8f - screenPosition, null, new Color(power * (lightColor.R / 255f), power * (lightColor.G / 255f), power * (lightColor.B / 255f), 0), target.ToRotation() + PiOver2, texture.Size() * 0.5f, new Vector2(scale, scale + 0.01f * target.Length() * vfxThingies[i].Z), SpriteEffects.None, 0);
        }

        lightColor *= (20 + 100f * Projectile.ai[1]) / 255f;
        lightColor.A = 0;

        return true;
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        instance.DrawCacheNPCsOverPlayers.Add(index);
        overPlayers.Add(index);
    }
}
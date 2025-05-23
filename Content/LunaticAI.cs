





using System.Linq;
using Terraria.GameContent.ItemDropRules;

namespace WrathOfTheCultist.Content;

public class LunaticAI : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == CultistBoss;
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == CultistBoss)
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsExpert(), ModContent.ItemType<EldritchShield>()));
    }

    public enum State
    {
        Stationary,
        Teleporting,
        Uzumaki,
        Creep,
        Vile,
        Skele,
        Lightning,
        Shadowflame,
        True,
        Balls
    }

    public State state = State.Stationary;

    public LunaticAnimation.Anim anim = LunaticAnimation.Anim.None;

    public Vector2 targetPosition = Vector2.Zero;

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.WritePackedVector2(targetPosition);
        binaryWriter.Write7BitEncodedInt((int)state);
        binaryWriter.Write7BitEncodedInt((int)anim);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        targetPosition = binaryReader.ReadPackedVector2();
        state = (State)binaryReader.Read7BitEncodedInt();
        anim = (LunaticAnimation.Anim)binaryReader.Read7BitEncodedInt();
    }

    public override bool PreAI(NPC npc)
    {
        npc.frameCounter -= 0.85;
        if (npc.life == npc.lifeMax)
        {

            return false;
        }
        else if (anim == LunaticAnimation.Anim.None)
            anim = LunaticAnimation.Anim.Laugh;



        if (npc.ai[1] > 500)
            state = State.Stationary;

        if (npc.ai[1] > 90 && anim == LunaticAnimation.Anim.Laugh)
            anim = LunaticAnimation.Anim.Fly;

        if (state == State.Stationary && netMode != NetmodeID.MultiplayerClient)
        {
            var roll = (State)rand.Next(11);

            if (roll > State.Balls)
                roll = State.Balls;

            state = roll;   

            anim = LunaticAnimation.Anim.Fly;

            if (anim == LunaticAnimation.Anim.Fly && rand.NextBool(10))
            {
                anim = LunaticAnimation.Anim.Laugh;
                SoundEngine.PlaySound(SoundID.Zombie105 with { Pitch = 0.3f }, npc.Center);
            }

            targetPosition = Vector2.Zero;

            npc.direction = rand.Next(2) * 2 - 1;

            npc.ai[1] = 0;

            npc.netUpdate = true;
        }

        switch (state)
        {
            case State.Stationary:
                npc.ai[1]++;

                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Fly;
                break;

            case State.Teleporting:
                Teleport(npc);

                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Cast;
                break;

            case State.Uzumaki:
                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Attack;
                Uzumaki(npc);

                break;

            case State.Creep:
                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Attack;
                Creep(npc);

                break;

            case State.Vile:
                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Cast;
                Vile(npc);

                break;
            
            case State.Skele:
                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Cast;
                Skele(npc);

                break;
            
            case State.Lightning:
                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Attack;
                Lightning(npc);

                break;

            case State.Shadowflame:
                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Attack;
                Shadowflame(npc);

                break;

            case State.True:
                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Cast;
                True(npc);

                break;

            case State.Balls:
                if (anim != LunaticAnimation.Anim.Laugh)
                    anim = LunaticAnimation.Anim.Attack;
                Balls(npc);

                break;

            default:
                npc.ai[1]++;
                state = State.Stationary;

                if (anim != LunaticAnimation.Anim.Laugh)
                        anim = LunaticAnimation.Anim.Fly;
                break;
        }

        if (player.All(p => !p.active || p == null || p.respawnTimer > 0))
            npc.life = 0;

        return false;
    }

    public void Teleport(NPC npc)
    {
        npc.ai[1]++;

        npc.teleporting = true;
        npc.teleportStyle = 0;

        if (targetPosition == Vector2.Zero)
        {
            targetPosition = npc.Center + new Vector2(0, -800).RotatedBy(rand.NextFloat() * TwoPi);
            targetPosition.Y = Math.Clamp((int)targetPosition.Y / 16, 200, (int)worldSurface - 70) * 16;
            targetPosition.X = Math.Clamp((int)targetPosition.X / 16, 400, (int)maxTilesX - 400) * 16;

            if (targetPosition.Distance(npc.Center) <= 400)
                targetPosition += npc.Center.DirectionTo(new Vector2(spawnTileX, spawnTileY)) * 500;


            Player player = null;
            var distance = 0f;
            foreach (var p in ActivePlayers)
            {
                if (targetPosition.Distance(npc.Center) <= distance || distance == 0)
                { 
                    player = p;
                    distance = targetPosition.Distance(npc.Center); 
                }
            }

            if (player != null && player.Center.Distance(targetPosition) > 800)
                targetPosition = player.Center + player.Center.DirectionTo(targetPosition) * 800;

        }

        if (npc.ai[1] > 100)
        {

            if (npc.AI_AttemptToFindTeleportSpot(ref targetPosition, (int)targetPosition.ToTileCoordinates().X, (int)targetPosition.ToTileCoordinates().Y, teleportInAir: true, solidTileCheckFluff: 5))
            {
                npc.Teleport(targetPosition.ToWorldCoordinates());
                state = State.Stationary;
                targetPosition = Vector2.Zero;
                npc.ai[1] = 0;

                npc.netUpdate = true;
            }
        }

    }

    public void Uzumaki(NPC npc)
    {

        npc.ai[1]++;

        if (targetPosition == Vector2.Zero)
        {
            targetPosition = npc.Center + new Vector2(0, -500).RotatedBy(rand.NextFloat() * TwoPi);
            targetPosition.Y = Math.Max(50, targetPosition.Y);
        }

        UzumakiCurseAI.UzumakiCenter = targetPosition;

        if (netMode == NetmodeID.MultiplayerClient)
            return;

        if (npc.ai[1] % 5 == 0 && npc.ai[1] < 150)
        {
            for (float i = 0; i < TwoPi; i += PiOver4)
            {
                var direction = rand.Next(2) * 2 - 1;

                var velocity = new Vector2(4 + direction).RotatedBy(i + rand.NextFloat()) * (npc.ai[1] % 15);

                NewNPC(npc.GetSource_FromAI("Cthulhu servants uzumaki spell"), (int)(targetPosition.X + velocity.X), (int)(targetPosition.Y + velocity.Y), ServantofCthulhu, ai0: direction);
            }
        }


        if (npc.ai[1] > 300)
            SetToTeleport(npc);

        else if (npc.ai[1] > 150 && anim != LunaticAnimation.Anim.Laugh)
                anim = LunaticAnimation.Anim.Fly;
    }

    public void Creep(NPC npc)
    {
        if (netMode == NetmodeID.MultiplayerClient)
            return;

        npc.ai[1]++;

        if (npc.ai[1] % 4 == 0 && npc.ai[1] < 200)
        {
            targetPosition = npc.Center + (new Vector2(100) + new Vector2(500) * rand.NextFloat()).RotatedBy(rand.NextFloat() * TwoPi);

            NewNPC(npc.GetSource_FromAI("Creeper obstacles spell"), (int)targetPosition.X, (int)targetPosition.Y, Creeper, ai0: rand.NextFloat());
        }

        if (npc.ai[1] > 300)
            SetToTeleport(npc);

        else if (npc.ai[1] > 200 && anim != LunaticAnimation.Anim.Laugh)
            anim = LunaticAnimation.Anim.Fly;
    }

    public void Vile(NPC npc)
    {
        if (netMode == NetmodeID.MultiplayerClient)
            return;

        npc.ai[1]++;

        if (npc.ai[1] % 20 == 0 && npc.ai[1] < 100)
            NewNPC(npc.GetSource_FromAI("Vile spit bombs spell"), (int)npc.Center.X, (int)npc.Center.Y, VileSpit, ai0: TwoPi / 5 * (rand.NextFloat() + npc.ai[1] / 20));
        

        if (npc.ai[1] > 300)
            SetToTeleport(npc);

        else if (npc.ai[1] > 100 && anim != LunaticAnimation.Anim.Laugh)
            anim = LunaticAnimation.Anim.Fly;
    }

    public void Skele(NPC npc)
    {
        if (netMode == NetmodeID.MultiplayerClient)
            return;

        npc.ai[1]++;

        if (npc.ai[1] == 20)
        {
            NewNPC(npc.GetSource_FromAI("Skele hands spell"), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, ai0: 1, ai1: npc.whoAmI);
            NewNPC(npc.GetSource_FromAI("Skele hands spell"), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, ai0: -1, ai1: npc.whoAmI);
        }

        if (npc.ai[1] > 300)
            SetToTeleport(npc);

        else if (npc.ai[1] > 20 && anim != LunaticAnimation.Anim.Laugh)
            anim = LunaticAnimation.Anim.Fly;
    }

    public void Shadowflame(NPC npc)
    {
        if (netMode == NetmodeID.MultiplayerClient)
            return;

        npc.ai[1]++;

        if (targetPosition == Vector2.Zero)
        {
            targetPosition = npc.Center + new Vector2(0, -500).RotatedBy(rand.NextFloat() * TwoPi);
            targetPosition.Y = Math.Max(50, targetPosition.Y);
        }

        if (npc.ai[1] == 60)
        {
            NewProjectile(npc.GetSource_FromThis(), targetPosition, Vector2.Zero, ModContent.ProjectileType<ShadowflameSpell>(), 0, 0);
            SetToTeleport(npc);
        }

        if (npc.ai[1] >= 300)
            SetToTeleport(npc);

        else if (npc.ai[1] > 60 && anim != LunaticAnimation.Anim.Laugh)
            anim = LunaticAnimation.Anim.Fly;
    }

    public void True(NPC npc)
    {
        if (netMode == NetmodeID.MultiplayerClient)
            return;

        npc.ai[1]++;

        if (npc.ai[1] == 60)
        {
            NewNPC(npc.GetSource_FromThis("Lunatic True... spell"), (int)npc.Center.X, (int)npc.Center.Y, MoonLordFreeEye, ai3: npc.whoAmI);
            SetToTeleport(npc);
        }

        if (npc.ai[1] >= 300)
            SetToTeleport(npc);

        else if (npc.ai[1] > 60 && anim != LunaticAnimation.Anim.Laugh)
            anim = LunaticAnimation.Anim.Fly;
    }

    public void Balls(NPC npc)
    {
        if (netMode == NetmodeID.MultiplayerClient)
            return;

        npc.ai[1]++;

        if (npc.ai[1] % 20 == 0 && npc.ai[1] <= 80)
            NewProjectile(npc.GetSource_FromAI("Fire balls spell"), npc.Center, new Vector2(8, 0).RotatedBy(npc.ai[1] / 20 * PiOver2), CultistBossFireBall, 50, 4, ai1: 50);

        if (npc.ai[1] > 300)
            SetToTeleport(npc);

        else if (npc.ai[1] > 100 && anim != LunaticAnimation.Anim.Laugh)
            anim = LunaticAnimation.Anim.Fly;
    }

    public void Lightning(NPC npc)
    {
        if (netMode == NetmodeID.MultiplayerClient)
            return;

        npc.ai[1]++;

        if (npc.ai[1] == 60)
        {
            NewProjectile(npc.GetSource_FromAI("Lightning spell"), npc.Center - new Vector2(0, 150), Vector2.Zero, CultistBossLightningOrb, 50, 4);
        }

        if (npc.ai[1] > 300)
            SetToTeleport(npc);

        else if (npc.ai[1] > 80 && anim != LunaticAnimation.Anim.Laugh)
            anim = LunaticAnimation.Anim.Fly;
    }

    public void SetToTeleport(NPC npc)
    {
        state = State.Teleporting;
        targetPosition = Vector2.Zero;
        npc.ai[1] = 0;

        npc.netUpdate = true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (state == State.Teleporting)
        {
            var texture = TextureAssets.Extra[51];

            var rect = texture.Frame(1, 4, 0, (int)(npc.ai[1] / 4 % 4));

            var offset = new Vector2(texture.Width() / 2.5f, texture.Height() * 1.29f);

            spriteBatch.Draw(texture.Value, npc.position + offset - screenPosition, rect, Color.Gold, 0, texture.Size() / 2, 2f, SpriteEffects.None, 0);
        }
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[CultistBoss];

        var rect = LunaticAnimation.FindFrame(ref anim, ref npc.frameCounter);


        spriteBatch.Draw(texture.Value, npc.position - new Vector2(npc.width / 2, npc.height / 6) - screenPos, rect, drawColor, npc.rotation, npc.Size / 2, npc.scale, npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

        return false;
    }
}

public class LunaticAnimation
{
    public enum Anim
    {
        None,
        Fly,
        Attack,
        Cast,
        Laugh
    }

    public static Rectangle FindFrame(ref Anim anim, ref double frame)
    {
        //var frameHeight = TextureAssets.Npc[CultistBoss].Value.Height / 16 - 4;
        var animOffset = 0;

        switch (anim)
        {
            case Anim.Fly:
                if (frame > 3)
                    frame = 0;

                animOffset = 3;

                break;

            case Anim.Attack:
                if (frame > 2)
                    frame = 0;

                animOffset = 10;

                break;

            case Anim.Cast:
                if (frame > 2)
                    frame = 0;

                animOffset = 7;

                break;

            case Anim.Laugh:
                if (frame > 2)
                    frame = 0;

                animOffset = 13;

                break;

            default:
                if (frame > 3)
                {
                    frame = 0;
                }

                break;
        }

        var frameOnTexture = (int)Math.Floor(frame) + animOffset;

        return TextureAssets.Npc[CultistBoss].Value.Frame(1, 16, 0, frameOnTexture, 0, 2);
    }
}

public class UzumakiCurseAI : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == ServantofCthulhu;
    }

    public bool InUzumaki = false;

    public static Vector2 UzumakiCenter = Vector2.Zero;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (source != null && source.Context == "Cthulhu servants uzumaki spell")
        {
            InUzumaki = true;

            npc.lifeMax *= 100;
            npc.life *= 100;
            npc.damage *= 6;
            npc.knockBackResist = 0f;
        }
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(InUzumaki);

        binaryWriter.Write7BitEncodedInt(npc.lifeMax);
        binaryWriter.Write7BitEncodedInt(npc.life);
        binaryWriter.Write(npc.scale);
        binaryWriter.Write7BitEncodedInt(npc.damage);
        binaryWriter.Write(npc.knockBackResist);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        var flag = bitReader.ReadBit();

        if (!InUzumaki)
            InUzumaki = flag;

        npc.lifeMax = binaryReader.Read7BitEncodedInt();
        npc.life = binaryReader.Read7BitEncodedInt();
        npc.scale = binaryReader.ReadSingle();
        npc.damage = binaryReader.Read7BitEncodedInt();
        npc.knockBackResist = binaryReader.ReadSingle();
    }

    public override bool PreAI(NPC npc)
    {
        if (InUzumaki)
        {
            npc.ai[1]++;

            if (npc.ai[1] > 300)
                npc.ai[0] = 0;

            if (npc.ai[1] > 500)
            {
                npc.life = 0;
                npc.active = false;
            }

            if (npc.ai[0] != 0)
                npc.velocity = npc.Center.DirectionFrom(UzumakiCenter).RotatedBy(PiOver2 * (0.9f + 0.069f * (npc.ai[0] + 1)) * Math.Sign(npc.ai[0])) * 9;


            npc.rotation = npc.velocity.ToRotation() - PiOver2;

            return false;
        }

        return base.PreAI(npc);
    }
}

public class CreepAI : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == Creeper;
    }

    public bool Obstacle = false;

    public Vector2 SpawnPosition = Vector2.Zero;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (source != null && source.Context == "Creeper obstacles spell")
        {
            Obstacle = true;

            npc.lifeMax *= 15;
            npc.life *= 15;
            npc.defense *= 3;
            npc.damage *= 6;
            npc.knockBackResist = 0f;
            SpawnPosition = npc.Center;
        }
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(Obstacle);

        binaryWriter.Write7BitEncodedInt(npc.lifeMax);
        binaryWriter.Write7BitEncodedInt(npc.life);
        binaryWriter.Write(npc.scale);
        binaryWriter.Write7BitEncodedInt(npc.damage);
        binaryWriter.Write(npc.knockBackResist);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        var flag = bitReader.ReadBit();

        if (!Obstacle)
            Obstacle = flag;

        npc.lifeMax = binaryReader.Read7BitEncodedInt();
        npc.life = binaryReader.Read7BitEncodedInt();
        npc.scale = binaryReader.ReadSingle();
        npc.damage = binaryReader.Read7BitEncodedInt();
        npc.knockBackResist = binaryReader.ReadSingle();
    }

    public override bool PreAI(NPC npc)
    {
        if (Obstacle)
        {
            if (SpawnPosition == Vector2.Zero)
                SpawnPosition = npc.Center;

            npc.ai[1]++;

            if (npc.ai[1] > 50 && npc.ai[1] % 2 == 0)
            {
                if (npc.ai[1] > 1200) 
                { 
                    npc.life = 0;
                    npc.active = false;
                }

                npc.velocity = Vector2.One.RotatedBy(Pi * 0.67f * (npc.ai[1] % 3) + npc.ai[0]) * 2;
            }

            else if (npc.ai[1] % 4 == 0)
            {
                for (float i = 0; i < TwoPi; i += 1.2f)
                {
                    var offset = new Vector2(npc.width / 2).RotatedBy(i);

                    Dust.NewDustPerfect(SpawnPosition + offset, DustID.Blood).noGravity = true;
                }
            }

            return false;
        }

        return base.PreAI(npc);
    }

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        if (npc.ai[1] <= 50)
            return false;

        return base.CanBeHitByItem(npc, player, item);
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (npc.ai[1] <= 50)
            return false;

        return base.CanBeHitByProjectile(npc, projectile);
    }

    public override bool CanBeHitByNPC(NPC npc, NPC attacker)
    {
        if (npc.ai[1] <= 50)
            return false;

        return base.CanBeHitByNPC(npc, attacker);
    }

    public override bool CanHitNPC(NPC npc, NPC target)
    {
        if (npc.ai[1] <= 50)
            return false;

        return base.CanHitNPC(npc, target);
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        if (npc.ai[1] <= 50)
            return false;

        return base.CanHitPlayer(npc, target, ref cooldownSlot);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.ai[1] <= 50)
            return false;

        return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }
}

public class VileSpitAI : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == VileSpit;
    }

    public bool Bomb = false;

    public Vector2 SpawnPosition = Vector2.Zero;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (source != null && source.Context == "Vile spit bombs spell")
        {
            Bomb = true;

            npc.lifeMax = (int)(1000 * GetBalance());
            npc.life = npc.lifeMax;
            npc.scale = 2;
            npc.knockBackResist = 0;
        }
        else if (source != null && source.Context == "Vile spit bomb detonate")
        {
            Bomb = true;

            npc.lifeMax = (int)(400 * GetBalance());
            npc.life = npc.lifeMax;
            npc.scale = 1.12f;
            npc.damage = (int)npc.ai[0] / 2 + 100;
        }
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(Bomb);

        binaryWriter.Write7BitEncodedInt(npc.lifeMax);
        binaryWriter.Write7BitEncodedInt(npc.life);
        binaryWriter.Write(npc.scale);
        binaryWriter.Write7BitEncodedInt(npc.damage);
        binaryWriter.Write(npc.knockBackResist);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        var flag = bitReader.ReadBit();

        if (!Bomb)
            Bomb = flag;

        npc.lifeMax = binaryReader.Read7BitEncodedInt();
        npc.life = binaryReader.Read7BitEncodedInt();
        npc.scale = binaryReader.ReadSingle();
        npc.damage = binaryReader.Read7BitEncodedInt();
        npc.knockBackResist = binaryReader.ReadSingle();
    }

    public override bool PreAI(NPC npc)
    {
        if (Bomb)
        {
            npc.ai[1]++;
            if (npc.ai[1] > 600)
                npc.life = 0;

            npc.rotation = npc.ai[0] * TwoPi;

            if (npc.scale != 2f)
            {
                npc.velocity = new Vector2(npc.ai[2], npc.ai[3]);
                return false;
            }

            if (npc.ai[1] > 100 && npc.ai[3] == 0)
            {
                npc.ai[3] = 300;
                npc.velocity *= 0;
            }

            else if (npc.ai[3] == 0)
            {
                npc.velocity = Vector2.One.RotatedBy(npc.ai[0] + float.Sin(npc.ai[1] / 30)) * 5;
            }


            if (npc.ai[3] > 0)
            {
                npc.ai[3]--;

                if (npc.ai[1] % 4 == 0)
                {
                    for (float i = 0; i < TwoPi; i += PiOver2)
                    {
                        var offset = new Vector2(npc.width / 2).RotatedBy(i + npc.ai[0] * TwoPi);
                        var position = npc.Center + offset;


                        if (npc.ai[3] > 10)
                            Dust.NewDustPerfect(position, DustID.ToxicBubble, offset, Scale: 2).noGravity = true;

                        else if (netMode != NetmodeID.MultiplayerClient)
                        {
                            NewNPC(npc.GetSource_FromAI("Vile spit bomb detonate"), (int)position.X, (int)position.Y, VileSpit, ai0: npc.ai[1], ai2: offset.X * 2, ai3: offset.Y * 2);
                            npc.life = 0;
                            npc.active = false;
                        }
                    }
                }
            }

            return false;
        }

        return base.PreAI(npc);
    }

    public override void OnKill(NPC npc)
    {
        if (npc.scale == 2)
        {
            for (float i = 0; i < TwoPi; i += PiOver2)
            {
                var offset = new Vector2(npc.width / 2).RotatedBy(i + npc.ai[0] * TwoPi);
                var position = npc.Center + offset;

                if (netMode != NetmodeID.MultiplayerClient)
                    NewNPC(npc.GetSource_FromAI("Vile spit bomb detonate"), (int)position.X, (int)position.Y, VileSpit, ai0: npc.ai[1], ai2: offset.X * 2, ai3: offset.Y * 2);
            }
        }
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.velocity.LengthSquared() < 0.1f)
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

        var texture = TextureAssets.Npc[npc.type];


        for (int i = 1; i < 16; i++)
        {
            var alpha = (byte)(150 / i);

            var color = drawColor;
            color *= alpha * (1f / 255f);

            spriteBatch.Draw(texture.Value, npc.Center - screenPos - npc.velocity / 3 * i, null, color with { A = 0 }, npc.rotation, texture.Size() / 2, npc.scale, SpriteEffects.None, 0);
        }

        return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }
}

public class SkeleHandAI : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.SkeletronHand;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (source != null && source.Context != null && source.Context == "Skele hands spell")
        {
            ASpell = true;
            npc.lifeMax *= 5;
            npc.life = npc.lifeMax;
            npc.defense *= 2;
            npc.damage *= 3;
        }
    }

    public bool ASpell = false;

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(ASpell);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        var flag = bitReader.ReadBit();

        if (!ASpell)
        {
            ASpell = flag;
            if (ASpell)
            {
                npc.lifeMax *= 5;
                npc.life = npc.lifeMax;
                npc.defense *= 2;
                npc.damage *= 3;
            }
        }
    }

    public int counter = 0;

    public override bool PreAI(NPC npc)
    {
        if (ASpell)
        {
            var lunatic = Main.npc[(int)npc.ai[1]];


            npc.ai[2] = 0;
            npc.ai[3] = 0;

            counter++;

            if (counter > 100 && counter <= 145)
            {
                var rot = npc.ai[0] * TwoPi / 60 * (counter - 100);

                var vel = new Vector2(20 * npc.ai[0], -20).RotatedBy(rot);

                vel.X *= 0.75f;
                npc.velocity = vel;

                vel = npc.Center.DirectionFrom(lunatic.Center);
                vel *= new Vector2(1.6f, 0.6f);
                if (counter % 5 == 0)
                    NewProjectile(npc.GetSource_FromAI("Skele hands skulls"), npc.Center, vel * 3, ProjectileID.Skull, 50, 4);
            }
            else if (counter > 600)
            {
                npc.velocity = npc.Center.DirectionTo(lunatic.Center).RotatedBy(-npc.ai[0] * PiOver4) * 9;

                if (npc.Center.DistanceSQ(lunatic.Center) <= 50 * 50)
                {
                    for (float r = 0; r <= Pi; r += 0.4f)
                    {
                        var offset = new Vector2(0, -18).RotatedBy(r * npc.ai[0]);
                        NewProjectile(npc.GetSource_FromAI("Skele hands bones"), npc.Center, offset, ProjectileID.Bone, 60, 4);
                    }
                    npc.StrikeInstantKill();
                }
            }
            else
            {
                npc.velocity = npc.Center.DirectionFrom(lunatic.Center) * (10 - lunatic.Center.Distance(npc.Center) / 30);

                npc.velocity += Vector2.One.RotatedBy(counter * (1f / 20f) + npc.ai[0]);


                if (npc.Center.DistanceSQ(lunatic.Center) > 90 * 90 && npc.Center.DistanceSQ(lunatic.Center) < 400 * 400 && counter > 150)
                {
                    var distance = 300f * 300f;
                    var target = Vector2.Zero;
                    foreach (var proj in ActiveProjectiles)
                    {
                        if ((npc.ai[0] > 0 && proj.Center.X < lunatic.Center.X) || (npc.ai[0] < 0 && proj.Center.X > lunatic.Center.X))
                        {
                            if (proj.friendly && proj.damage > 0 && proj.Center.DistanceSQ(lunatic.Center) <= distance)
                            {
                                distance = proj.Center.DistanceSQ(lunatic.Center);

                                target = proj.Center + proj.velocity * 4;
                            }
                        }
                    }
                    if (target != Vector2.Zero)
                    {
                        npc.velocity += npc.Center.DirectionTo(lunatic.Center) * 4;
                    }
                }
                else
                    npc.velocity += npc.Center.DirectionTo(lunatic.Center) * 2;

            }

            if (npc.Center == lunatic.Center)
                npc.Center += Vector2.One;

            npc.rotation = npc.rotation.AngleTowards(npc.Center.DirectionFrom(lunatic.Center).ToRotation() - PiOver2, 0.06f);

            if (counter >= 900)
                npc.life = 0;

            return false;
        } 

        return base.PreAI(npc);
    }
}

public class SkeleHandsBoneAI : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        return entity.type == ProjectileID.Bone;
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source != null && source.Context != null && source.Context == "Skele hands bones")
        {
            ASpell = true;
            projectile.friendly = false;
            projectile.hostile = true;
        }
    }

    public bool ASpell = false;

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(ASpell);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        var flag = bitReader.ReadBit();

        if (!ASpell)
            ASpell = flag;

        if (flag)
        { 
            projectile.friendly = false;
            projectile.hostile = true;
        }
    }
}

public class ShadowflameSpell : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 408;
        Projectile.height = 408;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
    }

    public override void AI()
    {
        Projectile.ai[0]++;
        Projectile.scale = 0.6f;

        Projectile.rotation = Projectile.ai[0] * PiOver4 / 20;

        if (netMode == NetmodeID.MultiplayerClient)
            return;

        if (Projectile.ai[0] < 260)
        {
            if (Projectile.ai[0] % 40 != 0)
                return;

            for (float r = 0; r < TwoPi; r += PiOver2 / 1.5f + rand.NextFloat(PiOver4 / 2))
            {
                var rot = rand.NextFloat(0.2f);
                NewProjectile(Projectile.GetSource_FromAI("Shadowflame spell tentacles"), Projectile.Center, new Vector2(6.6f).RotatedBy(r), ShadowFlame, 60, 3, ai1: 0.1f - rot, ai0: rot);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
                NewProjectile(Projectile.GetSource_FromAI("Shadowflame spell fireball"), Projectile.Center, new Vector2(6).RotatedByRandom(TwoPi), CultistBossFireBallClone, 80, 5);

            Projectile.Kill();
        }
    }

    public override void PostDraw(Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type];

        var scale = 0.6f;
        var smolScale = 0.275f + 0.005f * Projectile.ai[0] + 0.1f * float.Sin(Projectile.rotation);
        
        spriteBatch.Draw(texture.Value, Projectile.Center - screenPosition, null, lightColor, -Projectile.rotation, texture.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0);
        spriteBatch.Draw(texture.Value, Projectile.Center - screenPosition, null, lightColor, Projectile.rotation * 0.4f, texture.Size() / 2, smolScale * Projectile.scale, SpriteEffects.None, 0);
    }
}

public class ShadowflameSpellTentacles : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        return entity.type == ShadowFlame;
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source != null && source.Context != null && source.Context == "Shadowflame spell tentacles")
        {
            ASpell = true;
            projectile.friendly = false;
            projectile.hostile = true;
        }
    }

    public bool ASpell = false;

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(ASpell);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        var flag = bitReader.ReadBit();

        if (!ASpell)
            ASpell = flag;

        if (flag)
        {
            projectile.friendly = false;
            projectile.hostile = true;
        }
    }
}

public class TrueEyeButNotML : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public bool ASpell = false;

    public int life = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == MoonLordFreeEye;
    }

    public override bool PreAI(NPC npc)
    {
        if (ASpell)
        {
            life = npc.life;
            return true;
        }

        return base.PreAI(npc);
    }

    public override void PostAI(NPC npc)
    {
        if (ASpell && Main.npc[(int)npc.ai[3]].active && Main.npc[(int)npc.ai[3]].type == CultistBoss)
        {
            npc.life = life;
            npc.active = true; 
        }
    }

    public override bool PreKill(NPC npc)
    {
        if (ASpell && npc.ai[0] < 900)
            return false;

        return base.PreKill(npc);
    }
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (source != null && source.Context == "Lunatic True... spell")
        {
            ASpell = true;
            npc.dontTakeDamage = false;
            npc.lifeMax *= 25;
            npc.life = npc.lifeMax;
        }
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(ASpell);
        binaryWriter.Write7BitEncodedInt(npc.lifeMax);
        binaryWriter.Write7BitEncodedInt(npc.life);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        var flag = bitReader.ReadBit();
        npc.lifeMax = binaryReader.Read7BitEncodedInt();
        npc.life = binaryReader.Read7BitEncodedInt();

        if (!ASpell)
            ASpell = flag;

        if (ASpell)
            npc.dontTakeDamage = false;
    }
}

public class FireballsSpell : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        return entity.type == CultistBossFireBall;
    }

    public override bool PreAI(Projectile projectile)
    {
        return base.PreAI(projectile);
    }
}




namespace WrathOfTheCultist.Content
{
    public class LunaticAI : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.type == CultistBoss;
        }

        public enum State
        {
            Stationary,
            Teleporting,
            Uzumaki,
            Creep,
            Vile
        }

        public State state = State.Stationary;

        public Vector2 targetPosition = Vector2.Zero;

        public int TargetPlayerID = -1;

        public static Vector2 GenerateNewPosition(float size)
        {
            var pos = new Vector2(screenWidth * Math.Clamp(float.CosPi(random), 0.5f - size / 2, 0.5f + size / 2), screenHeight * Math.Clamp(float.Sin(random), 0.5f - size / 2, 0.5f + size / 2));
            
            return pos;
        }

        public override bool PreAI(NPC npc)
        {
            if (npc.life == npc.lifeMax)
                return false;
            // Dust.NewDustPerfect(targetPosition, DustID.Fireworks, Vector2.Zero);

            if (npc.ai[1] % 4 == 0)
            {
                TargetPlayerID = -1;

                var distance = 3000f * 3000f;
                foreach (var p in ActivePlayers)
                {
                    if (p.Center.DistanceSQ(npc.Center) - p.aggro * p.aggro < distance)
                    {
                        distance = p.Center.DistanceSQ(npc.Center) - p.aggro * p.aggro;
                        TargetPlayerID = p.whoAmI;
                    }
                }
            }

            if (TargetPlayerID == -1)
            {
                npc.ai[1]--;
                if (npc.ai[1] <= -200 && LocalPlayer.active)
                        TargetPlayerID = LocalPlayer.whoAmI;
                
            }
            if (TargetPlayerID == -1)
            {
                if (npc.ai[1] < 220)
                    npc.life = 0;

                else
                    return false;
            }

            if (npc.ai[1] > 600)
            {
                state = State.Stationary;
            }

            if (state == State.Stationary)
            {
                var roll = (State)Math.Clamp((int)(random * 6), 2, 4);

                if (roll > State.Vile)
                    roll = State.Vile;

                state = roll;

                targetPosition = Vector2.Zero;

                npc.ai[1] = 0;
            }

            switch (state)
            {
                case State.Stationary:
                    npc.ai[1]++;
                    break;

                case State.Teleporting:
                    Teleport(npc);
                    break;

                case State.Uzumaki:
                    Uzumaki(npc);
                    break;

                case State.Creep:
                    Creep(npc);
                    break;

                case State.Vile:
                    Vile(npc);
                    break;
                    /*
                case State.Skele:

                    break;

                case State.Lightning:

                    break;

                case State.Shadowflame:

                    break;

                case State.True:

                    break;

                case State.Balls:

                    break; */

                default:
                    npc.ai[1]++;
                    state = State.Stationary;
                    break;
            }

            return false;
        }

        public void Teleport(NPC npc)
        {
            if (TargetPlayerID != -1)
            {
                npc.ai[1]++;

                npc.teleporting = true;
                npc.teleportStyle = 0;

                if (targetPosition == Vector2.Zero)
                {
                    var player = Main.player[TargetPlayerID];
                    targetPosition = player.Center + new Vector2(0, -800).RotatedBy(random * Pi - PiOver2);
                    targetPosition.Y = Math.Max(50, targetPosition.Y);
                }

                if (npc.ai[1] > 100)
                {
                    if (targetPosition.Y > worldSurface)
                        targetPosition.Y = (int)worldSurface * 8 - 100;

                    targetPosition.Y = Math.Max(50, targetPosition.Y);

                    if (npc.AI_AttemptToFindTeleportSpot(ref targetPosition, (int)targetPosition.ToTileCoordinates().X, (int)targetPosition.ToTileCoordinates().Y, teleportInAir: true, solidTileCheckFluff: 0))
                    {
                        if (targetPosition.ToWorldCoordinates().DistanceSQ(Main.player[TargetPlayerID].Center) < 800)
                            return;

                        npc.Teleport(targetPosition.ToWorldCoordinates());
                        state = State.Stationary;
                        targetPosition = Vector2.Zero;
                        npc.ai[1] = 0;
                    }
                }
            }
        }

        public void Uzumaki(NPC npc)
        {
            if (netMode != NetmodeID.MultiplayerClient && TargetPlayerID != -1)
            {
                UzumakiCurseAI.UzumakiCenter = targetPosition;

                npc.ai[1]++;

                if (targetPosition == Vector2.Zero)
                {
                    var player = Main.player[TargetPlayerID];
                    targetPosition = player.Center + new Vector2(0, -300).RotatedBy(random * TwoPi);
                    targetPosition.Y = Math.Max(50, targetPosition.Y);
                }

                if (npc.ai[1] % 5 == 0 && npc.ai[1] < 150)
                {
                    for (float i = 0; i < TwoPi; i += PiOver4)
                    {
                        var direction = rand.Next(0, 2) * 2 - 1;

                        var velocity = new Vector2(4 + direction).RotatedBy(i + random) * (npc.ai[1] % 15);

                        NewNPC(npc.GetSource_FromAI("Cthulhu servants uzumaki spell"), (int)(targetPosition.X + velocity.X), (int)(targetPosition.Y + velocity.Y), ServantofCthulhu, ai0: direction);
                    }
                }

                if (npc.ai[1] > 500)
                    SetToTeleport(npc);
            }
        }

        public void Creep(NPC npc)
        {
            if (netMode != NetmodeID.MultiplayerClient && TargetPlayerID != -1)
            {
                npc.ai[1]++;

                if (npc.ai[1] % 4 == 0 && npc.ai[1] < 200)
                {
                    var player = Main.player[TargetPlayerID];
                    targetPosition = player.Center + (new Vector2(100) + new Vector2(500) * random).RotatedBy(rand.NextFloat() * TwoPi);

                    NewNPC(npc.GetSource_FromAI("Creeper obstacles spell"), (int)targetPosition.X, (int)targetPosition.Y, Creeper, ai0: random);
                }

                if (npc.ai[1] > 200)
                    SetToTeleport(npc);
            }
        }

        public void Vile(NPC npc)
        {
            if (netMode != NetmodeID.MultiplayerClient && TargetPlayerID != -1)
            {
                npc.ai[1]++;

                if (npc.ai[1] % 20 == 0 && npc.ai[1] < 100)
                {
                    NewNPC(npc.GetSource_FromAI("Vile spit bombs spell"), (int)npc.Center.X, (int)npc.Center.Y, VileSpit, ai0: random, ai2: TargetPlayerID);
                }

                if (npc.ai[1] > 100)
                    SetToTeleport(npc);
            }
        }

        public void SetToTeleport(NPC npc)
        {
            targetPosition = screenPosition + GenerateNewPosition(0.6f);
            targetPosition.Y = Math.Max(50, targetPosition.Y);
            state = State.Teleporting;
            targetPosition = Vector2.Zero;
            npc.ai[1] = 0;
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
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            var flag = bitReader.ReadBit();

            if (!InUzumaki)
                InUzumaki = flag;
        }

        public override bool PreAI(NPC npc)
        {
            if (InUzumaki)
            {
                npc.ai[1]++;

                if (npc.ai[1] > 300)
                    npc.ai[0] = 0;

                if (npc.ai[1] > 500)
                    npc.life = 0;

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
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            var flag = bitReader.ReadBit();

            if (!Obstacle)
                Obstacle = flag;
        }

        public override bool PreAI(NPC npc)
        {
            if (Obstacle)
            {
                npc.ai[1]++;

                if (npc.ai[1] > 50 && npc.ai[1] % 2 == 0)
                {
                    if (npc.ai[1] > 1200)
                        npc.life = 0;

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
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            var flag = bitReader.ReadBit();

            if (!Bomb)
                Bomb = flag;
        }

        public override bool PreAI(NPC npc)
        {
            if (Bomb)
            {
                npc.ai[1]++;
                if (npc.ai[1] > 600)
                    npc.life = 0;

                if (npc.scale != 2)
                {
                    npc.velocity = new Vector2(npc.ai[2], npc.ai[3]);
                    return false;
                }

                if (player.Length <= (int)npc.ai[2] || (int)npc.ai[2] < 0 || !player[(int)npc.ai[2]].active)
                {
                    npc.life = 0;
                    return false;
                }

                npc.rotation = npc.ai[0] * TwoPi;

                var Player = player[(int)npc.ai[2]];
                if (npc.ai[1] > 60 && npc.Center.DistanceSQ(Player.Center) < 250 * 250 && npc.ai[3] == 0)
                {
                    npc.ai[3] = 300;
                    npc.velocity *= 0;
                }

                else if (npc.ai[3] == 0)
                {
                    npc.velocity = npc.Center.DirectionTo(Player.Center).RotatedBy(PiOver2 * 1.5f * npc.ai[0] - PiOver2 * 0.75f) * 6;
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

                            else
                            {
                                NewNPC(npc.GetSource_FromAI("Vile spit bomb detonate"), (int)position.X, (int)position.Y, VileSpit, ai0: npc.ai[1], ai2: offset.X * 2, ai3: offset.Y * 2);
                                npc.life = 0;
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

                    NewNPC(npc.GetSource_FromAI("Vile spit bomb detonate"), (int)position.X, (int)position.Y, VileSpit, ai0: npc.ai[1], ai2: offset.X * 2, ai3: offset.Y * 2);
                }
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
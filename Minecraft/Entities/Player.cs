using System;
using System.Collections.Generic;
using System.IO;
using Minecraft.Items;
using Minecraft.Map;
using Minecraft.Net;
using NBTLibrary;
using Minecraft.Packet;
using Minecraft.Command;

namespace Minecraft.Entities
{
    public class Player : Entity
    {
        private List<Chunk> LoadedChunks = new List<Chunk>();
        private List<Player> LoadedPlayers = new List<Player>();

        public bool OnGround { get; set; }
        public short HoldingSlot { get; set; }
        public double Stance { get; set; }
        public double MotionX { get; set; }
        public double MotionY { get; set; }
        public double MotionZ { get; set; }
        public float FallDistance { get; set; }
        public int Dimension { get; set; } // Currently not needed, but if I want nether, this is needed.
        public short Air { get; set; }
        public short AttackTime { get; set; }
        public short DeathTime { get; set; }
        public short Fire { get; set; }
        public short Health { get; set; }
        public short HurtTime { get; set; }
        public string Username { get; set; }
        public uint EID { get; set; }
        public Chunk CurrentChunk { get; set; }
        public Dictionary<byte, Item> Inventory = new Dictionary<byte, Item>();
        public MinecraftClient Client { get; set; }
        public MinecraftRank Rank { get; set; }
        public Rotation Rotation { get; set; }


        public Player(MinecraftClient client, string username, uint eid)
        {
            Client = client;
            Username = username;
            EID = eid;

            Rank = MinecraftServer.Instance.GetRank(Username);

            if (!Load())
            {
                OnGround = true;
                FallDistance = 0;
                Dimension = 0;
                Air = 0;
                AttackTime = 0;
                DeathTime = 0;
                Fire = 0;
                Health = 20;
                HurtTime = 0;
                X = MinecraftServer.Instance.SpawnX + 0.5;
                Y = MinecraftServer.Instance.SpawnY + 3;
                Z = MinecraftServer.Instance.SpawnZ + 0.5;
                Stance = Y;
                Rotation = new Rotation();
            }
        }

        public bool Load()
        {
            try
            {
                using (NBTFile file = NBTFile.Open(Path.Combine(MinecraftServer.Instance.Path, "players", Username + ".dat")))
                {
                    Tag[] motion = (Tag[])file.FindPayload("Motion");
                    MotionX = (double)motion[0].Payload;
                    MotionY = (double)motion[1].Payload;
                    MotionZ = (double)motion[2].Payload;

                    OnGround = BitConverter.ToBoolean(new byte[] { (byte)file.FindPayload("OnGround") }, 0);

                    HurtTime = (short)file.FindPayload("HurtTime");

                    Health = (short)file.FindPayload("Health");

                    Dimension = (int)file.FindPayload("Dimension");

                    Air = (short)file.FindPayload("Air");

                    Tag[] inventory = (Tag[])file.FindPayload("Inventory");
                    foreach (Tag tag in inventory)
                    {
                        Item i = new Item();
                        i.ID = (short)file.FindPayload("id", tag);
                        i.Damage = (short)file.FindPayload("Damage", tag);
                        i.Count = (byte)file.FindPayload("Count", tag);
                        i.Slot = (byte)file.FindPayload("Slot", tag);
                        Inventory.Add(i.Slot, i);
                    }

                    Tag[] pos = (Tag[])file.FindPayload("Pos");
                    X = (double)pos[0].Payload;
                    Y = (double)pos[1].Payload + 3;
                    Z = (double)pos[2].Payload;

                    AttackTime = (short)file.FindPayload("AttackTime");

                    Fire = (short)file.FindPayload("Fire");

                    FallDistance = (float)file.FindPayload("FallDistance");

                    Tag[] rotation = (Tag[])file.FindPayload("Rotation");
                    Rotation = new Rotation() { Yaw = (float)rotation[0].Payload, Pitch = (float)rotation[1].Payload };

                    DeathTime = (short)file.FindPayload("DeathTime");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">The x coordinate in chunks (16 blocks).</param>
        /// <param name="z">The z coordinate in chunks (16 blocks).</param>
        /// <returns></returns>
        public bool IsInRange(int x, int z)
        {
            if (Math.Abs(CurrentChunk.X - x) + Math.Abs(CurrentChunk.Z - z) < 8)
            {
                return true;
            }
            return false;
        }

        public void AddInventoryItem(Item t)
        {
            //TODO: Find better way to do this
            Item i = new Item(t);
            Inventory.Add(i.Slot, i);
        }

        public void Save()
        {
            //TODO: There has to be a better way to do this...
            using (NBTFile file = new NBTFile())
            {
                file.File = new Tag();
                file.File.Type = TagType.Compound;

                List<Tag> payload = new List<Tag>();

                Tag motion = new Tag();
                motion.Type = TagType.List;
                motion.Name = "Motion";

                Tag[] motionPayload = new Tag[3];

                Tag motionx = new Tag();
                motionx.Type = TagType.Double;
                motionx.Payload = MotionX;
                motionPayload[0] = motionx;

                Tag motiony = new Tag();
                motiony.Type = TagType.Double;
                motiony.Payload = MotionY;
                motionPayload[1] = motiony;

                Tag motionz = new Tag();
                motionz.Type = TagType.Double;
                motionz.Payload = MotionZ;
                motionPayload[2] = motionz;

                motion.Payload = motionPayload;

                payload.Add(motion);

                Tag onGround = new Tag();
                onGround.Type = TagType.Byte;
                onGround.Name = "OnGround";
                onGround.Payload = BitConverter.GetBytes(OnGround)[0];

                payload.Add(onGround);

                Tag hurtTime = new Tag();
                hurtTime.Type = TagType.Short;
                hurtTime.Name = "HurtTime";
                hurtTime.Payload = HurtTime;

                payload.Add(hurtTime);

                Tag health = new Tag();
                health.Type = TagType.Short;
                health.Name = "Health";
                health.Payload = Health;

                payload.Add(health);

                Tag dimension = new Tag();
                dimension.Type = TagType.Int;
                dimension.Name = "Dimension";
                dimension.Payload = Dimension;

                payload.Add(dimension);

                Tag air = new Tag();
                air.Type = TagType.Short;
                air.Name = "Air";
                air.Payload = Air;

                payload.Add(air);

                Tag inventory = new Tag();
                inventory.Type = TagType.List;
                inventory.Name = "Inventory";

                Tag[] inventoryPayload = new Tag[Inventory.Count];

                int index = 0;
                foreach (Item i in Inventory.Values)
                {
                    Tag item = new Tag();
                    item.Type = TagType.Compound;

                    List<Tag> itemPayload = new List<Tag>();

                    Tag id = new Tag();
                    id.Type = TagType.Short;
                    id.Name = "id";
                    id.Payload = i.ID;

                    itemPayload.Add(id);

                    Tag damage = new Tag();
                    damage.Type = TagType.Short;
                    damage.Name = "Damage";
                    damage.Payload = i.Damage;

                    itemPayload.Add(damage);

                    Tag count = new Tag();
                    count.Type = TagType.Byte;
                    count.Name = "Count";
                    count.Payload = i.Count;

                    itemPayload.Add(count);

                    Tag slot = new Tag();
                    slot.Type = TagType.Byte;
                    slot.Name = "Slot";
                    slot.Payload = i.Slot;

                    itemPayload.Add(slot);

                    item.Payload = itemPayload;

                    inventoryPayload[index++] = item;
                }

                inventory.Payload = inventoryPayload;

                payload.Add(inventory);

                Tag pos = new Tag();
                pos.Type = TagType.List;
                pos.Name = "Pos";

                Tag[] posPayload = new Tag[3];

                Tag posx = new Tag();
                posx.Type = TagType.Double;
                posx.Payload = X;
                posPayload[0] = posx;

                Tag posy = new Tag();
                posy.Type = TagType.Double;
                posy.Payload = Y;
                posPayload[1] = posy;

                Tag posz = new Tag();
                posz.Type = TagType.Double;
                posz.Payload = Z;
                posPayload[2] = posz;

                pos.Payload = posPayload;

                payload.Add(pos);

                Tag attackTime = new Tag();
                attackTime.Type = TagType.Short;
                attackTime.Name = "AttackTime";
                attackTime.Payload = AttackTime;

                payload.Add(attackTime);

                Tag fire = new Tag();
                fire.Type = TagType.Short;
                fire.Name = "Fire";
                fire.Payload = Fire;

                payload.Add(fire);

                Tag fallDistance = new Tag();
                fallDistance.Type = TagType.Float;
                fallDistance.Name = "FallDistance";
                fallDistance.Payload = FallDistance;

                payload.Add(fallDistance);

                Tag rotation = new Tag();
                rotation.Type = TagType.List;
                rotation.Name = "Rotation";

                Tag[] rotationPayload = new Tag[2];

                Tag yaw = new Tag();
                yaw.Type = TagType.Float;
                yaw.Payload = Rotation.Yaw;
                rotationPayload[0] = yaw;

                Tag pitch = new Tag();
                pitch.Type = TagType.Float;
                pitch.Payload = Rotation.Pitch;
                rotationPayload[1] = pitch;

                rotation.Payload = rotationPayload;

                payload.Add(rotation);

                Tag deathTime = new Tag();
                deathTime.Type = TagType.Short;
                deathTime.Name = "DeathTime";
                deathTime.Payload = DeathTime;

                payload.Add(deathTime);

                file.File.Payload = payload;

                file.Save(Path.Combine(MinecraftServer.Instance.Path, "players", Username + ".dat"));
            }
        }

        /// <summary>
        /// Everytime player position changes, the update function makes sure all map data gets updated.
        /// </summary>
        public void Update()
        {
            Chunk currentChunk = MinecraftServer.Instance.ChunkManager.GetChunk((int)X / 16, (int)Z / 16);
            if (CurrentChunk == null || currentChunk != CurrentChunk)
            {
                List<Chunk> chunks = MinecraftServer.Instance.ChunkManager.GetChunks((int)X / 16, (int)Z / 16);
                foreach (Chunk c in LoadedChunks)
                {
                    if (!chunks.Contains(c))
                    {
                        // Unload chunks
                        Client.Send(MinecraftPacketCreator.GetPreChunk(c.X, c.Z, false));
                    }
                }

                foreach (Chunk c in chunks)
                {
                    // NOTE: Probably be better to not pass the whole chunk........
                    if (!LoadedChunks.Contains(c))
                    {
                        Client.Send(MinecraftPacketCreator.GetPreChunk(c.X, c.Z, true));
                        Client.Send(MinecraftPacketCreator.GetMapChunk(c));
                    }
                }

                LoadedChunks = chunks;
                CurrentChunk = currentChunk;


                //TODO: This section is truely flawed... Fix
                List<Player> players = new List<Player>();
                foreach (Player p in MinecraftServer.Instance.Players.Values)
                {
                    if (p != this && p.IsInRange(CurrentChunk.X, CurrentChunk.Z))
                    {
                        players.Add(p);
                    }
                }

                short currentItemID = 0;
                short currentItemDamage = 0;
                byte key = (byte)(HoldingSlot + 36);
                if (Inventory.ContainsKey(key))
                {
                    currentItemID = Inventory[key].ID;
                    currentItemDamage = Inventory[key].Damage;
                }

                foreach (Player p in players)
                {
                    if (!LoadedPlayers.Contains(p))
                    {
                        p.Client.Send(MinecraftPacketCreator.GetNamedEntitySpawn(EID, Username, (int)(X * 32), (int)(Y * 32), (int)(Z * 32), (byte)Rotation.Yaw, (byte)Rotation.Pitch, currentItemID));
                        for (byte b = 5; b <= 8; ++b)
                        {
                            if (Inventory.ContainsKey(b))
                            {
                                Item i = Inventory[b];
                                p.Client.Send(MinecraftPacketCreator.GetEntityEquipment(EID, (short)(b - 4), i.ID, i.Damage));
                            }
                            else
                            {
                                p.Client.Send(MinecraftPacketCreator.GetEntityEquipment(EID, (short)(b - 4), -1, 0));
                            }
                        }
                        p.Client.Send(MinecraftPacketCreator.GetEntityEquipment(EID, 0, currentItemID, currentItemDamage));

                        if (!p.LoadedPlayers.Contains(this))
                        {
                            short pCurrentItemID = 0;
                            short pCurrentItemDamage = 0;
                            byte pKey = (byte)(p.HoldingSlot + 36);
                            if (p.Inventory.ContainsKey(pKey))
                            {
                                pCurrentItemID = p.Inventory[pKey].ID;
                                pCurrentItemDamage = p.Inventory[pKey].Damage;
                            }
                            Client.Send(MinecraftPacketCreator.GetNamedEntitySpawn(p.EID, p.Username, (int)(p.X * 32), (int)(p.Y * 32), (int)(p.Z * 32), (byte)p.Rotation.Yaw, (byte)p.Rotation.Pitch, pCurrentItemID));

                            for (byte b = 5; b <= 8; ++b)
                            {
                                if (p.Inventory.ContainsKey(b))
                                {
                                    Item i = p.Inventory[b];
                                    Client.Send(MinecraftPacketCreator.GetEntityEquipment(p.EID, (short)(b - 4), i.ID, i.Damage));
                                }
                                else
                                {
                                    Client.Send(MinecraftPacketCreator.GetEntityEquipment(p.EID, (short)(b - 4), -1, 0));
                                }
                            }
                            Client.Send(MinecraftPacketCreator.GetEntityEquipment(p.EID, 0, pCurrentItemID, pCurrentItemDamage));
                            p.LoadedPlayers.Add(this);
                        }
                    }
                }

                LoadedPlayers = players;
            }
        }

        public void ToSpawn()
        {
            Move(MinecraftServer.Instance.SpawnX, MinecraftServer.Instance.SpawnY, MinecraftServer.Instance.SpawnZ);
        }

        public void Move(double x, double y, double z)
        {
            X = x + 0.5;
            Y = y + 5;
            Z = z + 0.5;

            Rotation.Yaw = 0;
            Rotation.Pitch = 0;

            Update();

            Client.Send(MinecraftPacketCreator.GetPositionLook(X, Y, Z, Rotation, false));
        }
    }
}

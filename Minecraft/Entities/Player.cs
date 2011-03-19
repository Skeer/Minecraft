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
        private NBTFile Data;

        public bool OnGround
        {
            get { return (byte)Data["OnGround"].Payload == 0x01; }
            set { Data["OnGround"].Payload = value ? (byte)0x01 : (byte)0x00; }
        }
        public short HoldingSlot { get; set; }
        public double Stance { get; set; }
        public double MotionX
        {
            get { return (double)((List<Tag>)Data["Motion"].Payload)[0].Payload; }
            set { ((List<Tag>)Data["Motion"].Payload)[0].Payload = value; }
        }
        public double MotionY
        {
            get { return (double)((List<Tag>)Data["Motion"].Payload)[1].Payload; }
            set { ((List<Tag>)Data["Motion"].Payload)[1].Payload = value; }
        }
        public double MotionZ
        {
            get { return (double)((List<Tag>)Data["Motion"].Payload)[2].Payload; }
            set { ((List<Tag>)Data["Motion"].Payload)[2].Payload = value; }
        }
        public float FallDistance
        {
            get { return (float)Data["FallDistance"].Payload; }
            set { Data["FallDistance"].Payload = value; }
        }
        public int Dimension
        {
            get { return (int)Data["Dimension"].Payload; }
            set { Data["Dimension"].Payload = value; }
        }
        public short Air
        {
            get { return (short)Data["Air"].Payload; }
            set { Data["Air"].Payload = value; }
        }
        public short AttackTime
        {
            get { return (short)Data["AttackTime"].Payload; }
            set { Data["AttackTime"].Payload = value; }
        }
        public short DeathTime
        {
            get { return (short)Data["DeathTime"].Payload; }
            set { Data["DeathTime"].Payload = value; }
        }
        public short Fire
        {
            get { return (short)Data["Fire"].Payload; }
            set { Data["Fire"].Payload = value; }
        }
        public short Health
        {
            get { return (short)Data["Health"].Payload; }
            set { Data["Health"].Payload = value; }
        }
        public short HurtTime
        {
            get { return (short)Data["HurtTime"].Payload; }
            set { Data["HurtTime"].Payload = value; }
        }
        public string Username { get; set; }
        public uint EID { get; set; }
        public Chunk CurrentChunk { get; set; }
        public Dictionary<byte, Item> Inventory = new Dictionary<byte, Item>();
        public MinecraftClient Client { get; set; }
        public MinecraftRank Rank { get; set; }
        public float Yaw
        {
            get { return (float)((List<Tag>)Data["Rotation"].Payload)[0].Payload; }
            set { ((List<Tag>)Data["Rotation"].Payload)[0].Payload = value; }
        }
        public float Pitch
        {
            get { return (float)((List<Tag>)Data["Rotation"].Payload)[1].Payload; }
            set { ((List<Tag>)Data["Rotation"].Payload)[1].Payload = value; }
        }
        public new double X
        {
            get { return (double)((List<Tag>)Data["Pos"].Payload)[0].Payload; }
            set { ((List<Tag>)Data["Pos"].Payload)[0].Payload = value; }
        }
        public new double Y
        {
            get { return (double)((List<Tag>)Data["Pos"].Payload)[1].Payload; }
            set { ((List<Tag>)Data["Pos"].Payload)[1].Payload = value; }
        }
        public new double Z
        {
            get { return (double)((List<Tag>)Data["Pos"].Payload)[2].Payload; }
            set { ((List<Tag>)Data["Pos"].Payload)[2].Payload = value; }
        }


        public Player(MinecraftClient client, string username, uint eid)
        {
            Client = client;
            Username = username;
            EID = eid;

            Rank = MinecraftServer.Instance.GetRank(Username);

            if (!Load())
            {
                // Default shit
                Data = new NBTFile();
                Data.Path = Path.Combine(MinecraftServer.Instance.Path, "players", Username + ".dat");

                Data.Root = new Tag();
                Data.Root.Type = TagType.Compound;

                List<Tag> payload = new List<Tag>();

                Tag motion = new Tag();
                motion.Type = TagType.List;
                motion.Name = "Motion";

                List<Tag> motionPayload = new List<Tag>();

                Tag motionx = new Tag();
                motionx.Type = TagType.Double;
                motionPayload.Add(motionx);

                Tag motiony = new Tag();
                motiony.Type = TagType.Double;
                motionPayload.Add(motiony);

                Tag motionz = new Tag();
                motionz.Type = TagType.Double;
                motionPayload.Add(motionz);

                motion.Payload = motionPayload;

                payload.Add(motion);

                Tag onGround = new Tag();
                onGround.Type = TagType.Byte;
                onGround.Name = "OnGround";

                payload.Add(onGround);

                Tag hurtTime = new Tag();
                hurtTime.Type = TagType.Short;
                hurtTime.Name = "HurtTime";

                payload.Add(hurtTime);

                Tag health = new Tag();
                health.Type = TagType.Short;
                health.Name = "Health";

                payload.Add(health);

                Tag dimension = new Tag();
                dimension.Type = TagType.Int;
                dimension.Name = "Dimension";

                payload.Add(dimension);

                Tag air = new Tag();
                air.Type = TagType.Short;
                air.Name = "Air";

                payload.Add(air);

                Tag inventory = new Tag();
                inventory.Type = TagType.List;
                inventory.Name = "Inventory";

                payload.Add(inventory);

                Tag pos = new Tag();
                pos.Type = TagType.List;
                pos.Name = "Pos";

                List<Tag> posPayload = new List<Tag>();

                Tag posx = new Tag();
                posx.Type = TagType.Double;
                posPayload.Add(posx);

                Tag posy = new Tag();
                posy.Type = TagType.Double;
                posPayload.Add(posy);

                Tag posz = new Tag();
                posz.Type = TagType.Double;
                posPayload.Add(posz);

                pos.Payload = posPayload;

                payload.Add(pos);

                Tag attackTime = new Tag();
                attackTime.Type = TagType.Short;
                attackTime.Name = "AttackTime";

                payload.Add(attackTime);

                Tag fire = new Tag();
                fire.Type = TagType.Short;
                fire.Name = "Fire";

                payload.Add(fire);

                Tag fallDistance = new Tag();
                fallDistance.Type = TagType.Float;
                fallDistance.Name = "FallDistance";

                payload.Add(fallDistance);

                Tag rotation = new Tag();
                rotation.Type = TagType.List;
                rotation.Name = "Rotation";

                List<Tag> rotationPayload = new List<Tag>();

                Tag yaw = new Tag();
                yaw.Type = TagType.Float;
                rotationPayload.Add(yaw);

                Tag pitch = new Tag();
                pitch.Type = TagType.Float;
                rotationPayload.Add(pitch);

                rotation.Payload = rotationPayload;

                payload.Add(rotation);

                Tag deathTime = new Tag();
                deathTime.Type = TagType.Short;
                deathTime.Name = "DeathTime";

                payload.Add(deathTime);

                Data.Root.Payload = payload;


                // Load new configuration structure...
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
                Yaw = 0;
                Pitch = 0;

                Save();
            }
        }

        public bool Load()
        {
            try
            {
                Data = NBTFile.Open(Path.Combine(MinecraftServer.Instance.Path, "players", Username + ".dat"));

                List<Tag> inventory = (List<Tag>)Data.FindPayload("Inventory");
                foreach (Tag tag in inventory)
                {
                    Item i = new Item();
                    i.ID = (short)tag["id"].Payload;
                    i.Damage = (short)tag["Damage"].Payload;
                    i.Count = (byte)tag["Count"].Payload;
                    i.Slot = (byte)tag["Slot"].Payload;
                    Inventory.Add(i.Slot, i);
                }

                Y += 3;

                return true;
            }
            catch
            {
                // NOTE: Is this needed?
                if (Data != null)
                {
                    Data.Dispose();
                    Data = null;
                }
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
            SyncInventory();
            Data.Save();
        }

        private void SyncInventory()
        {
            List<Tag> inventoryPayload = new List<Tag>();

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

                inventoryPayload.Add(item);
            }

            Data["Inventory"].Payload = inventoryPayload;
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
                        p.Client.Send(MinecraftPacketCreator.GetNamedEntitySpawn(EID, Username, (int)(X * 32), (int)(Y * 32), (int)(Z * 32), (byte)Yaw, (byte)Pitch, currentItemID));
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
                            Client.Send(MinecraftPacketCreator.GetNamedEntitySpawn(p.EID, p.Username, (int)(p.X * 32), (int)(p.Y * 32), (int)(p.Z * 32), (byte)p.Yaw, (byte)p.Pitch, pCurrentItemID));

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

            Yaw = 0;
            Pitch = 0;

            Update();

            Client.Send(MinecraftPacketCreator.GetPositionLook(X, Y, Z, Yaw, Pitch, false));
        }
    }
}

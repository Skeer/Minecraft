using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using NBTLibrary.Tags;
using Minecraft.Map;
using Minecraft.Net;

namespace Minecraft.Entities
{
    class Drop : Entity, IDisposable
    {
        public short ID { get; set; }

        public short Damage { get; set; }
        public short Uses { get; set; }


        public override double X
        {
            get;
            set;
        }

        public override double Y
        {
            get;
            set;
        }

        public override double Z
        {
            get;
            set;
        }

        public override float Yaw
        {
            get;
            set;
        }

        public override float Pitch
        {
            get;
            set;
        }

        private bool _Ready = false;

        public bool Ready
        {
            get { return _Ready; }
            set { _Ready = value; }
        }

        private Timer ReadyTimer = new Timer(1000);
        private Timer DecayTimer = new Timer(300000);
        private bool Disposed = false;

        public Drop()
        {
            ReadyTimer.Start();
            ReadyTimer.Elapsed += new ElapsedEventHandler(ReadyTimer_Elapsed);
            DecayTimer.Start();
            DecayTimer.Elapsed += new ElapsedEventHandler(DecayTimer_Elapsed);
        }

        void DecayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Chunk c = MinecraftServer.Instance.ChunkManager.GetChunkFromBlockCoords(X, Z);
            c.Entities.Remove(this);
            MinecraftServer.Instance.Entities.Remove(EID);
            Dispose();
        }

        void ReadyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _Ready = true;
            ReadyTimer.Dispose();
            ReadyTimer = null;
        }

        public static Mob Load(Tag data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Drop()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (ReadyTimer != null)
                    {
                        ReadyTimer.Dispose();
                    }

                    if (DecayTimer != null)
                    {
                        DecayTimer.Dispose();
                    }

                }
                ReadyTimer = null;
                DecayTimer = null;
                Disposed = true;
            }
        }
    }
}

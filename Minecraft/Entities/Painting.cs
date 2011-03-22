using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBTLibrary.Tags;

namespace Minecraft.Entities
{
    class Painting : Entity
    {
        public override double X
        {
            get { return (int)Data["TileX"].Payload; }
            set { Data["Motive"].Payload = (int)value; }
        }

        public override double Y
        {
            get { return (int)Data["TileY"].Payload; }
            set { Data["Motive"].Payload = (int)value; }
        }

        public override double Z
        {
            get { return (int)Data["TileZ"].Payload; }
            set { Data["Motive"].Payload = (int)value; }
        }

        public int Direction
        {
            get { return (int)Data["Dir"].Payload; }
            set { Data["Dir"].Payload = (int)value; }
        }

        public override float Yaw
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override float Pitch
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Title
        {
            get { return (string)Data["Motive"].Payload; }
            set { Data["Motive"].Payload = value; }
        }

        public static Painting Load(uint eid, Tag data)
        {
            return new Painting(eid, data);
        }

        protected Painting(uint eid, Tag data)
        {
            EID = eid;
            Data = data;
        }

        private Tag Data;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBTLibrary.Tags;

namespace Minecraft.Entities
{
    class Sign : Entity
    {
        Tag Data;

        public static Sign Load(uint eid, Tag data)
        {
            return new Sign(eid, data);
        }

        protected Sign(uint eid, Tag data)
        {
            EID = eid;
            Data = data;
        }

        public override double X
        {
            get { return (int)Data["x"].Payload; }
            set { Data["x"].Payload = (int)value; }
        }

        public override double Y
        {
            get { return (int)Data["y"].Payload; }
            set { Data["y"].Payload = (int)value; }
        }

        public override double Z
        {
            get { return (int)Data["z"].Payload; }
            set { Data["z"].Payload = (int)value; }
        }

        public string Text1
        {
            get { return (string)Data["Text1"].Payload; }
            set { Data["Text1"].Payload = value; }
        }

        public string Text2
        {
            get { return (string)Data["Text2"].Payload; }
            set { Data["Text2"].Payload = value; }
        }

        public string Text3
        {
            get { return (string)Data["Text3"].Payload; }
            set { Data["Text3"].Payload = value; }
        }

        public string Text4
        {
            get { return (string)Data["Text4"].Payload; }
            set { Data["Text4"].Payload = value; }
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
    }
}

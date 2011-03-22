using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBTLibrary.Tags;

namespace Minecraft.Entities
{
    class Slime : Mob
    {
        Tag Data;

        public static new Slime Load(uint eid, Tag data)
        {
            return new Slime(eid, data);
        }

        protected Slime(uint eid, Tag data)
        {
            EID = eid;
            Data = data;
        }

        public override void Drop()
        {
            throw new NotImplementedException();
        }

        public override double X
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

        public override double Y
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

        public override double Z
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

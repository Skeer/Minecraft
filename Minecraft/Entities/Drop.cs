using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minecraft.Entities
{
    class Drop : Entity
    {
        public short ID { get; set; }

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
    }
}

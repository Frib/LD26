using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD26.entities
{
    class Checkpoint : Entity
    {
        public Checkpoint(Vector2 vector2)
        {
            Position = vector2;
        }

        public override void Update()
        {
            if ((World.player.Position - this.Position).Length() < 6)
            {
                World.NextLevel = true;
            }
        }

        internal static Checkpoint Create(string p)
        {
            var pos = p.Split(',');


            return new Checkpoint(new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        public override string Export()
        {
            return "E:C:" + Position.ToExportString();
        }
    }
}

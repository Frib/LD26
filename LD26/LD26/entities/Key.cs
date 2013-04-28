using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace LD26.entities
{
    class Key : Entity
    {
        private Cube cube;
        private int door;
        private bool pressed = false;

        private static SoundEffectInstance pressSound = RM.sounds["buttonpress"].CreateInstance();

        public Key(Vector2 vector2, string texture, int door)
        {
            this.Position = vector2;
            cube = new Cube(Position.ToVector3(), new Vector3(4, 4, 4), RM.GetTexture(texture));
            cube.SetPosition(new Vector3(Position.X, 6, Position.Y));
            this.door = door;
        }

        public override void Update()
        {
            if (!pressed && RM.IsPressed(InputAction.Use) && (World.player.Position - Position).Length() < 8)
            {
                if (cube.box.Intersects(new Ray(World.player.Position.ToVector3() + new Vector3(0, World.player.eyeYHeight, 0), World.player.getLookAt())) != null)
                {
                    World.entities.OfType<Door>().Skip(door).First().state = DoorState.Opening;
                    pressed = true;
                    pressSound.Play();
                }
            }
            base.Update();
        }

        internal static Key CreateKey(string p)
        {
            string[] pos;
            string texture = "white";
            int doorIndex = 0;
            if (p.Contains(':'))
            {
                var split = p.Split(':');
                pos = split[0].Split(',');
                texture = split[1];
                doorIndex = int.Parse(split[2]);
            }
            else
            {
                pos = p.Split(',');
            }

            return new Key(new Vector2(int.Parse(pos[0]), int.Parse(pos[1])), texture, doorIndex);
        }

        public override string Export()
        {
            return "E:K:" + this.Position.ToExportString();
        }

        public override void Draw()
        {
            cube.Draw(Selected);
            base.Draw();
        }
    }
}

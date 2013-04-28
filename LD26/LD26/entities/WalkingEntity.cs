using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD26.entities
{
    public abstract class WalkingEntity : Entity
    {
        public virtual float WalkSpeed { get { return 0.25f; } }
        public virtual float RunSpeed { get { return 0.6f; } }

        public int PanicLevel { get; set; }
        public bool running { get { return CanRun && WantsToRun; } }

        public Vector2 Direction;

        public virtual float CrouchModifier { get { return 0.6f; } }
        public virtual float ProneModifier { get { return 0.25f; } }

        public override bool Grabable
        {
            get
            {
                return !Grabbed;
            }
        }

        public override void Update()
        {
            if (Direction.Length() > 0)
            {
                Direction.Normalize();
                Velocity = Direction * (running ? RunSpeed : WalkSpeed);
            }
            else
            {
                Velocity = Vector2.Zero;
            }
            
            base.Update();
        }

        public override float eyeYHeight
        {
            get { return 14; }
        }

        public bool CanRun { get; set; }

        protected Vector3 lastCamDir = Vector3.Right;
        public override Vector3 CamDirection
        {
            get
            {
                if (Direction.Length() != 0)
                {
                    lastCamDir = Direction.ToVector3();
                    lastCamDir.Normalize();
                   
                }
                return lastCamDir;
            }
        }

        public bool WantsToRun { get; set; }
    }
}

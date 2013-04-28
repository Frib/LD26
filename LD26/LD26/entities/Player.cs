using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace LD26.entities
{
    public class Player : WalkingEntity
    {
        private Cube cube;

        public int abilityTimer = -60;

        public static SoundEffectInstance monsterplayer = RM.sounds["monsternoise"].CreateInstance();
        public SoundEffectInstance footstepplayer = RM.sounds["footstep"].CreateInstance();

        public Player(World world, Vector2 pos)
        {
            cube = new Cube(new Vector3(pos.X, 0, pos.Y), new Vector3(4, 16, 4), RM.GetTexture("white"));
            Camera3d.c.leftRightRot = 0;
            Position = pos;
            Direction = new Vector2(0, 0);
            this.World = world;
            CanRun = true;
        }

        private bool frozen = false;
        public override void Update()
        {

            if (!frozen && RM.IsDown(InputAction.Action))
            {
                abilityTimer++;
                if (abilityTimer > 60)
                {
                    abilityTimer = 60;
                }
            }
            else
            {
                if (abilityTimer <= 60)
                {
                    abilityTimer = -60;
                }
                else
                {
                    abilityTimer--;
                }
            }

            if (abilityTimer > 1)
            {
               
                Vector3 cameraRotatedTarget = CamDirection;
                var cameraFinalTarget = Position.ToVector3() + new Vector3(0, eyeYHeight, 0) + cameraRotatedTarget;

                var v = Matrix.CreateLookAt(Position.ToVector3() + new Vector3(0, eyeYHeight, 0), cameraFinalTarget, Vector3.Up);
                var p = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Math.Min(60, abilityTimer)), (float)G.g.Window.ClientBounds.Width / (float)G.g.Window.ClientBounds.Height, 1f, 10000f);

                 var bf = new BoundingFrustum(v * p);

                foreach (var m in World.entities.OfType<Monster>().Where(m => !m.Active))
                {
                    if (bf.Contains(new BoundingSphere(m.Position.ToVector3() + new Vector3(0, 14, 0), 1)) == ContainmentType.Contains)
                    {
                        var dist = (m.Position - Position).Length();

                        List<Vector3> chain = new List<Vector3>();
                        
                        var newDir = m.Position - Position;
                        var distance = newDir.Length();
                        newDir.Normalize();

                        for (int i = 1; i < dist / 4; i++)
                        {
                            chain.Add((Position + (newDir * i * 4)).ToVector3());
                        }
                        if (World.IsOnFloor(chain))
                        {
                            var ray = new Ray(m.Position.ToVector3() + new Vector3(0, m.eyeYHeight, 0), newDir.ToVector3());
                            if (
                                !World.entities.OfType<Door>()
                                     .Any(d => (d.cube.box.Intersects(ray) ?? float.MaxValue) < distance))
                            {
                                m.Active = true;
                                m.LastKnownPlayerPos = Position;
                            }
                        }
                    }
                }

                foreach (var e in World.entities.OfType<EndPortal>())
                {
                    e.PlayerIsLooking();
                }
            }
                
            WantsToRun = RM.IsDown(InputAction.Sprint);
            
            if (abilityTimer > 0)
            {
                World.SubPerc = Math.Min(60, abilityTimer);
            }
            else
            {
                World.SubPerc = 0;
            }

            float xDifference = IM.MouseDelta.X;
            float yDifference = IM.MouseDelta.Y;

            Vector3 moveVector = Vector3.Zero;

            if (frozen)
            {
                Camera3d.c.leftRightRot -= 0.0007f;
            }
            else
            { 
                Camera3d.c.leftRightRot -= xDifference * IM.MouseSensitivity;
                Camera3d.c.upDownRot -= yDifference * IM.MouseSensitivity;

                if (Camera3d.c.upDownRot > MathHelper.PiOver2 - 0.1f)
                    Camera3d.c.upDownRot = MathHelper.PiOver2 - 0.1f;
                else if (Camera3d.c.upDownRot < (MathHelper.PiOver2 - 0.1f) * -1)
                    Camera3d.c.upDownRot = (MathHelper.PiOver2 - 0.1f) * -1;

                if (Camera3d.c.leftRightRot > MathHelper.TwoPi)
                    Camera3d.c.leftRightRot -= MathHelper.TwoPi;
                else if (Camera3d.c.leftRightRot < MathHelper.TwoPi * -1)
                    Camera3d.c.leftRightRot += MathHelper.TwoPi;
           


                if (RM.IsDown(InputAction.Left))
                    moveVector += Vector3.Left;
                if (RM.IsDown(InputAction.Right))
                    moveVector += Vector3.Right;
                if (RM.IsDown(InputAction.Up))
                    moveVector += Vector3.Forward;
                if (RM.IsDown(InputAction.Down))
                    moveVector += Vector3.Backward;
            }
            MovePlayer(moveVector);

            cube.SetPosition(new Vector3(Position.X, 0, Position.Y));

            float closest = float.MaxValue;

            foreach (var m in World.entities.OfType<Monster>())
            {
                var distance = (Position - m.Position).Length();
                if (distance < closest)
                {
                    closest = distance;
                }
                if (distance < 5)
                {
                    if (!DeleteMe)
                    {
                        DeleteMe = true;
                        RM.PlaySoundEueue("monstereat");
                    }
                }
            }

            if (monsterplayer.State != SoundState.Playing)
            {
                monsterplayer.Play();
            }
            if (footstepplayer.State != SoundState.Playing)
            {
                footstepplayer.Play();
            }

            var volMult = (Math.Sqrt((double) closest));
            if (volMult < 6)
            {
                monsterplayer.Volume = 1 / (float)volMult;                
            }
            else if (volMult < 12)
            {
                monsterplayer.Volume = 1 / (float)(volMult + ((volMult - 6) * 10));  
            }
            else
            {
                monsterplayer.Volume = 0;
            }
        }

        public bool IsVisible { get { return abilityTimer > 1 || (running && footstepplayer.Volume > 0.6f); } }

        private void MovePlayer(Vector3 vector)
        {
            Matrix cameraRotation = Matrix.CreateRotationY(Camera3d.c.leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(vector, cameraRotation);
            if (rotatedVector.Length() > 0)
            {
                rotatedVector.Normalize();
                Direction.X = rotatedVector.X;
                Direction.Y = rotatedVector.Z;

                Direction.Normalize();

                var TempVel = Direction * (running ? RunSpeed : WalkSpeed);

                var expectedPos = Position + TempVel;

                cube.SetPosition(expectedPos.ToVector3());
                if (!World.IsOnFloor(cube.box))
                {
                    expectedPos = Position + new Vector2(TempVel.X, 0);
                    cube.SetPosition(expectedPos.ToVector3());

                    if (!World.IsOnFloor(cube.box))
                    {
                        TempVel = new Vector2(0, TempVel.Y);
                    }

                    expectedPos = Position + new Vector2(0, TempVel.Y);
                    cube.SetPosition(expectedPos.ToVector3());

                    if (!World.IsOnFloor(cube.box))
                    {
                        TempVel = new Vector2(TempVel.X, 0);
                    }

                }

                Direction = TempVel;

                if (World.entities.OfType<Door>().Any(d => d.state != DoorState.Open && d.cube.box.Intersects(this.cube.box)))
                {
                    Direction = Vector2.Zero;
                }
            }
            else
            {
                Direction = Vector2.Zero;
            }

            Position += Direction;

            footstepplayer.Volume = running && (Direction.Length() > 0) ? 1 : 0;
        }

        public override Vector3 CamDirection
        {
            get
            {
                return getLookAt();
            }
        }
        
        public Vector3 getLookAt()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(Camera3d.c.upDownRot) * Matrix.CreateRotationY(Camera3d.c.leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(Vector3.Forward, cameraRotation);
            rotatedVector.Normalize();
            return rotatedVector;
        }

        public override string Export()
        {
            return "E:P:" + Position.ToExportString();
        }

        internal static Entity Create(World w, string p)
        {
            var pos = p.Split(',');

            return new Player(w, new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        public override void Draw()
        {
            cube.Draw();
            base.Draw();
        }

        protected override void DrawExtraHudShit(int offset)
        {

            var sb = G.g.spriteBatch;
            sb.Draw(RM.GetTexture("white"), new Rectangle(1191, 675, 69, 1), Color.White);
            sb.Draw(RM.GetTexture("white"), new Rectangle(1191, 675, 1, 25), Color.White);
            sb.Draw(RM.GetTexture("white"), new Rectangle(1260, 675, 1, 25), Color.White);
            sb.Draw(RM.GetTexture("white"), new Rectangle(1191, 700, 69, 1), Color.White);
            if (abilityTimer > -60)
            {
                sb.Draw(RM.GetTexture("white"), new Rectangle(1196, 680, (Math.Min(0, abilityTimer) + 60), 16), Color.Lime);
            }
        }

        internal void Freeze()
        {
            frozen = true;
        }

        internal void UnFreeze()
        {
            frozen = false;
        }
    }
}

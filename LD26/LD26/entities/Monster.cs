using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace LD26.entities
{
    class Monster : WalkingEntity
    {
        private VertexPositionNormalTexture[] billboardVertices;

        private SoundEffectInstance activeSound = RM.sounds["monsteractive"].CreateInstance();

        public override float WalkSpeed
        {
            get
            {
                return 0.4f;
            }
        }

        private int ticker = G.r.Next(360);
        private Vector3 size;

        private float offsetX = 0;
        private float offsetZ = 0;

        public Monster(World w, Vector2 pos)
        {
            Active = false;
            size = new Vector3(12);

            this.World = w;
            billboardVertices = new VertexPositionNormalTexture[6];
            Vector3 e = new Vector3(-size.X / 2, size.Y + 8, 0);
            Vector3 f = new Vector3(size.X / 2, size.Y + 8, 0);
            Vector3 g = new Vector3(size.X / 2, 8, 0);
            Vector3 h = new Vector3(-size.X / 2, 8, 0);

            this.size = new Vector3(size.X, size.Y, size.X);

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
            billboardVertices[0] = new VertexPositionNormalTexture(h, frontNormal, new Vector2(0, 1));
            billboardVertices[1] = new VertexPositionNormalTexture(e, frontNormal, new Vector2(0, 0));
            billboardVertices[2] = new VertexPositionNormalTexture(f, frontNormal, new Vector2(1, 0));
            billboardVertices[3] = new VertexPositionNormalTexture(h, frontNormal, new Vector2(0, 1));
            billboardVertices[4] = new VertexPositionNormalTexture(f, frontNormal, new Vector2(1, 0));
            billboardVertices[5] = new VertexPositionNormalTexture(g, frontNormal, new Vector2(1, 1));
            this.Position = pos;
        }

        public override string Export()
        {
            return "E:M:" + Position.ToExportString();
        }

        

        public override void Update()
        {
            ticker++;
            offsetX = (float)Math.Sin(MathHelper.ToRadians(ticker % 360));
            offsetZ = (float)Math.Cos(MathHelper.ToRadians((ticker / 3) % 360));

            var dist = (World.player.Position - Position).Length();

            if (dist < 32 && World.player.running && World.player.footstepplayer.Volume > 0.6f)
            {
                Active = true;
                LastKnownPlayerPos = World.player.Position;
            }

            if (Active)
            {
                if (World.player.IsVisible)
                {
                    List<Vector3> chain = new List<Vector3>();

                    var newDir = World.player.Position - Position;
                    var distance = newDir.Length();
                    newDir.Normalize();

                    for (int i = 1; i < dist/4; i++)
                    {
                        chain.Add((Position + (newDir*i*4)).ToVector3());
                    }
                    if (World.IsOnFloor(chain))
                    {
                        var ray = new Ray(Position.ToVector3() + new Vector3(0, eyeYHeight, 0), newDir.ToVector3());
                        if (
                            !World.entities.OfType<Door>()
                                 .Any(d => (d.cube.box.Intersects(ray) ?? float.MaxValue) < distance))
                        {
                            LastKnownPlayerPos = World.player.Position;
                        }
                    }
                }

                Direction = (LastKnownPlayerPos - Position);
                if (Direction.Length() < 3)
                {
                    Direction = Vector2.Zero;
                }
                else
                {
                    Direction.Normalize();
                }
            }

            Move();
            //base.Update();
        }

        private void Move()
        {
            if (Direction.Length() > 0)
            {
                var TempVel = Direction*(running ? RunSpeed : WalkSpeed);

                var expectedPos = Position + TempVel;

                if (!World.IsOnFloor(new List<Vector3> { expectedPos.ToVector3() }))
                {
                    expectedPos = Position + new Vector2(TempVel.X, 0);

                    if (!World.IsOnFloor(new List<Vector3> { expectedPos.ToVector3() }))
                    {
                        TempVel = new Vector2(0, TempVel.Y);
                    }

                    expectedPos = Position + new Vector2(0, TempVel.Y);

                    if (!World.IsOnFloor(new List<Vector3> { expectedPos.ToVector3() }))
                    {
                        TempVel = new Vector2(TempVel.X, 0);
                    }
                }

                if (World.entities.OfType<Door>().Any(d => d.state != DoorState.Open && d.cube.box.Intersects(new BoundingBox(expectedPos.ToVector3(), expectedPos.ToVector3() + Vector3.One))))
                {
                    TempVel = Vector2.Zero;
                }

                Position += TempVel;
            }
        }
        
        public override void Draw()
        {

            var GraphicsDevice = G.g.GraphicsDevice;
            G.g.VoidEffect.Parameters["World"].SetValue(GetMatrixChain());
            G.g.VoidEffect.Parameters["Texture"].SetValue(RM.GetTexture("monster\\m" + (ticker % 100 / 5)));
            G.g.VoidEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, billboardVertices, 0, 2);

            G.g.VoidEffect.Parameters["Texture"].SetValue(RM.GetTexture("monster\\face"));
            G.g.VoidEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, billboardVertices, 0, 2);

        }
        
        protected virtual Matrix GetMatrixChain()
        {
            var pos = Position;
            if (World.player != null)
            {
                pos = World.player.Position - Position;
            }
            return Matrix.CreateRotationY((float)Math.Atan2(pos.X, pos.Y)) * Matrix.CreateTranslation(Position.ToVector3() + new Vector3(offsetX, 0 , offsetZ));
        }

        internal static Entity Create(World w, string p)
        {
            var pos = p.Split(',');

            return new Monster(w, new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        private bool active;
        public bool Active { get { return active; } 
        set
        {
            if (!active && value == true)
            {
                activeSound.Volume = 0.5f;
                activeSound.Play();
            }
            active = value;
        }}

        public Vector2 LastKnownPlayerPos { get; set; }
    }
}

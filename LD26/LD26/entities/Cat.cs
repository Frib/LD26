using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD26.entities
{
    internal class Cat : WalkingEntity
    {
        private VertexPositionNormalTexture[] billboardVertices;

        private Vector3 size;

        public Cat(World w, Vector2 pos)
        {
            size = new Vector3(6);

            this.World = w;
            billboardVertices = new VertexPositionNormalTexture[6];
            Vector3 e = new Vector3(-size.X/2, size.Y, 0);
            Vector3 f = new Vector3(size.X/2, size.Y, 0);
            Vector3 g = new Vector3(size.X/2, 0, 0);
            Vector3 h = new Vector3(-size.X/2, 0, 0);

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
            return "E:S:" + Position.ToExportString();
        }

        private int ticker;
        public override void Update()
        {
            ticker++;

            var dist = (World.player.Position - Position).Length();

            if (ticker % 600 == 0 && dist < 128)
            {
                RM.sounds["meow"].Play(1f/dist, 0, 0);
            }

            if (dist < 8)
            {
                DeleteMe = true;
                Achievements.Achieve("mrsnuggles");
                RM.PlaySoundEueue("voice\\dude\\L3E1");

            }
        }
        
        public override void Draw()
        {
            var GraphicsDevice = G.g.GraphicsDevice;
            G.g.e.World = GetMatrixChain();
            G.g.e.Texture = RM.GetTexture("cat");
            G.g.e.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, billboardVertices,
                                                                           0, 2);
        }

        protected virtual Matrix GetMatrixChain()
        {
            var pos = Position;
            if (World.player != null)
            {
                pos = World.player.Position - Position;
            }
            return Matrix.CreateRotationY((float) Math.Atan2(pos.X, pos.Y))*
                   Matrix.CreateTranslation(Position.ToVector3());
        }

        internal static Entity Create(World w, string p)
        {
            var pos = p.Split(',');

            return new Cat(w, new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }
    }
}
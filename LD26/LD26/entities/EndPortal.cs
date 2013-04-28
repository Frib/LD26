using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace LD26.entities
{
    class EndPortal : Entity
    {
        private int charge;
        private bool active = false;
        private bool charging = false;

        private SoundEffectInstance swarmup = RM.sounds["portalwarmup"].CreateInstance();
        private SoundEffectInstance scharging = RM.sounds["portalcharging"].CreateInstance();
        private SoundEffectInstance sdone = RM.sounds["portalchargedone"].CreateInstance();
        private SoundEffectInstance ssustain = RM.sounds["portalchargedonesustain"].CreateInstance();

        private SoundEffectInstance music1 = RM.sounds["musicbossp1"].CreateInstance();
        private SoundEffectInstance music2 = RM.sounds["musicbossp2"].CreateInstance();
        private SoundEffectInstance music3 = RM.sounds["musicbossp3"].CreateInstance();
        private SoundEffectInstance music4 = RM.sounds["musicbossp4"].CreateInstance();

        private SoundEffectInstance currentMusic;

        public static int musicindex = 0;

        private Vector3 h;
        private Vector3 g;
        private Vector3 f;
        private Vector3 e;
        private Vector3 d;
        private Vector3 c;
        private Vector3 b;
        private Vector3 a;

        private Matrix mworld;

        public EndPortal(Vector2 vector2)
        {
            currentMusic = music1;

            music1.Volume = 0.4f;
            music2.Volume = 0.3f;
            music3.Volume = 0.2f;
            music4.Volume = 0.5f;
            musicindex = 0;
            this.Position = vector2;

            a = new Vector3(-0.5f, 1, -0.5f);
            b = new Vector3(0.5f, 1, -0.5f);
            c = new Vector3(0.5f, 0, -0.5f);
            d = new Vector3(-0.5f, 0, -0.5f);
            e = new Vector3(-0.5f, 1, 0.5f);
            f = new Vector3(0.5f, 1, 0.5f);
            g = new Vector3(0.5f, 0, 0.5f);
            h = new Vector3(-0.5f, 0, 0.5f);

            var scale = Matrix.CreateScale(new Vector3(8, 24, 8));
            mworld = scale * Matrix.CreateTranslation(Position.ToVector3());
        }

        public override void Draw()
        {
            var pointList = new VertexPositionColor[36];
            var color = (active ? monsterEscaped ? Color.Orange : Color.Fuchsia : new Color(new Vector3(charge/18000f)));

            //Achterkant
            pointList[0] = new VertexPositionColor(d, color);
            pointList[1] = new VertexPositionColor(b, color);
            pointList[2] = new VertexPositionColor(a, color);
            pointList[3] = new VertexPositionColor(d, color);
            pointList[4] = new VertexPositionColor(c, color);
            pointList[5] = new VertexPositionColor(b, color);

            //Voorkant
            pointList[12] = new VertexPositionColor(h,  color);
            pointList[13] = new VertexPositionColor(e,  color);
            pointList[14] = new VertexPositionColor(f,  color);
            pointList[15] = new VertexPositionColor(h,  color);
            pointList[16] = new VertexPositionColor(f,  color);
            pointList[17] = new VertexPositionColor(g,  color);

            //Links
            pointList[6] = new VertexPositionColor(d, color); 
            pointList[7] = new VertexPositionColor(a, color); 
            pointList[8] = new VertexPositionColor(e, color); 
            pointList[9] = new VertexPositionColor(d, color); 
            pointList[10] = new VertexPositionColor(e, color);
            pointList[11] = new VertexPositionColor(h, color);

            //Rechts
            pointList[18] = new VertexPositionColor(g, color); 
            pointList[19] = new VertexPositionColor(f, color); 
            pointList[20] = new VertexPositionColor(b, color); 
            pointList[21] = new VertexPositionColor(g, color); 
            pointList[22] = new VertexPositionColor(b, color); 
            pointList[23] = new VertexPositionColor(c, color); 

            //Bovenkant
            pointList[24] = new VertexPositionColor(e,  color);
            pointList[25] = new VertexPositionColor(a,  color);
            pointList[26] = new VertexPositionColor(b,  color);
            pointList[27] = new VertexPositionColor(e,  color);
            pointList[28] = new VertexPositionColor(b,  color);
            pointList[29] = new VertexPositionColor(f,  color);

            //Onderkant
            pointList[30] = new VertexPositionColor(h, color);
            pointList[31] = new VertexPositionColor(c, color);
            pointList[32] = new VertexPositionColor(d, color);
            pointList[33] = new VertexPositionColor(h, color);
            pointList[34] = new VertexPositionColor(g, color);
            pointList[35] = new VertexPositionColor(c, color);


            G.g.e.World = mworld;
            G.g.e.LightingEnabled = false;
            G.g.e.TextureEnabled = false;
            G.g.e.VertexColorEnabled = true;
            G.g.e.CurrentTechnique.Passes[0].Apply();
            G.g.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, pointList, 0, 12);
            G.g.e.LightingEnabled = true;
            G.g.e.TextureEnabled = true;
            G.g.e.VertexColorEnabled = false;
        }

        private bool heruawheriausdhfa = false;
        private int timerAfterActive = 0;
        public override void Update()
        {
            if (currentMusic.State != SoundState.Playing)
            {
                switch (musicindex)
                {
                    case 0:
                        break;
                    case 1:
                        currentMusic = music1; currentMusic.Play();
                        break;

                    case 2:
                        currentMusic = music2; currentMusic.Play();
                        break;

                    case 3:
                        currentMusic = music3; currentMusic.Play();
                        break;

                    case 4:
                        currentMusic = music4; currentMusic.Play();
                        break;
                        default:
                        break;
                }
            }

            if (!charging)
            {
                if (charge > 0)
                {
                    charging = true;
                    swarmup.Play();
                    RM.PlaySoundEueue("voice\\dude\\L6E1");
                    musicindex = 2;
                }
            }
            else if (!active)
            {
                if (swarmup.State == SoundState.Playing)
                {
                    swarmup.Stop(true);
                }
                if (scharging.State != SoundState.Playing)
                {
                    scharging.Play();
                }
            }
            else
            {
                if (sdone.State != SoundState.Playing)
                {
                    ssustain.Play();
                }
            }

            charge -= 3;
            if (charge < 0)
            {
                charge = 0;
            }

            var volume = charge/18000f;
            if (active)
            {
                volume = 1f;
            }

            volume *= 0.3f;
            if (active)
            {
                volume *= 0.5f;
            }

            swarmup.Volume = volume;
            scharging.Volume = volume;
            ssustain.Volume = volume;
            sdone.Volume = volume;

            if (!halfway && charge >= 9000)
            {
                RM.PlaySoundEueue("voice\\dude\\L6E2");
                halfway = true;
                musicindex = 3;
            }
            if (!threefourth && charge >= 13500)
            {
                RM.PlaySoundEueue("voice\\voidling\\L6E2");
                threefourth = true;

            }

            if (charge >= 18000)
            {
                if (!active)
                {
                    RM.PlaySoundEueue("voice\\voidling\\L6E3");
                    RM.PlaySoundEueue("voice\\dude\\L6E3");
                }
                active = true;
                scharging.Stop(true);
                if (!heruawheriausdhfa)
                {
                    heruawheriausdhfa = true;
                    sdone.Play();
                    musicindex = 4;

                }
            }
            var newDir = World.player.Position - Position;
            var distance = newDir.Length();

            if (active && distance < 4)
            {
                musicindex = 5;
                currentMusic.Stop(true);
                if (monsterEscaped)
                {
                    G.g.Showscreen(new FailScreen());
                }
                else
                {
                    G.g.Showscreen(new WinScreen());                    
                }
            }

            if (active)
            {
                timerAfterActive++;

                if (timerAfterActive > 300)
                {
                    if (!dasfasdfjh)
                    {
                        RM.PlaySoundEueue("voice\\voidling\\L6E4");
                        dasfasdfjh = true;
                    }
                    foreach (var m in World.entities.OfType<Monster>())
                    {
                        var dist = (m.Position - Position).Length();
                        if (dist < 4)
                        {
                            m.Position = new Vector2(-1000, -1000);
                            m.Active = false;
                            m.DeleteMe = true;
                            monsterEscaped = true;
                        }

                        m.LastKnownPlayerPos = Position;
                        m.Active = true;
                    }
                }
            }
        }

        private bool halfway = false;
        private bool threefourth = false;
        private bool dasfasdfjh = false;

        public void PlayerIsLooking()
        {
            List<Vector3> chain = new List<Vector3>();

            var newDir = World.player.Position - Position;
            var distance = newDir.Length();
            newDir.Normalize();

            for (int i = 1; i < distance / 4; i++)
            {
                chain.Add((Position + (newDir * i * 4)).ToVector3());
            }
            if (World.IsOnFloor(chain))
            {
                var ray = new Ray(Position.ToVector3() + new Vector3(0, eyeYHeight, 0), newDir.ToVector3());
                if (
                    !World.entities.OfType<Door>()
                         .Any(d => (d.cube.box.Intersects(ray) ?? float.MaxValue) < distance))
                {
                    charge += 10;
                }
            }
        }

        internal static EndPortal Create(string p)
        {
            var pos = p.Split(',');
            
            return new EndPortal(new Vector2(int.Parse(pos[0]), int.Parse(pos[1])));
        }

        public override string Export()
        {
            return "E:F:" + Position.ToExportString();
        }

        public bool monsterEscaped { get; set; }
    }
}

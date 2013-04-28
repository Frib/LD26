using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using LD26.entities;

namespace LD26
{
    public class GameScreen : Screen
    {
        private Camera3d camera;
        private World world;
        private bool paused;
        public int currentLevel;

        public GameScreen(int level)
        {
            camera = new Camera3d();
            if (level == -1)
            {
                world = new World();
                world.editorEnabled = true;
                world.allowEditor = true;
            }
            else
            {
                world = new World(RM.Levels[level]);
                //world.editorEnabled = true;
                //world.allowEditor = true;
                //world.player = null;
            }

            currentLevel = level;
        }

        public override void Update()
        { 
#if debug
            if (RM.IsPressed(InputAction.RestartLevel))
            {
                RM.SaveScreenshotsToDisk();
                world = new World(RM.Levels[currentLevel]);
            }

            if (RM.IsPressed(InputAction.EditorNextLevel) && world.allowEditor && world.editorEnabled)
            {
                currentLevel++;
                if (currentLevel >= RM.Levels.Count)
                {
                    currentLevel = 0;
                }

                world = new World(RM.Levels[currentLevel]);
                world.editorEnabled = true;
                world.allowEditor = true;
                Dead = false;
            }
#endif

            if (!world.entities.OfType<Player>().Any() && !world.editorEnabled)
            {
                Dead = true;
            }

            //if (!Dead && !world.editorEnabled && world.entities.OfType<Human>().All(x => !x.Alive))
            //{
            //    Dead = true;
            //    world.score += (1000 - world.timer / 6);

            //    RM.SaveScreenshotsToDisk();
            //}

            if (!paused)
            {
                if (!world.editorEnabled)
                {
                    IM.SnapToCenter = true;
                    G.g.IsMouseVisible = false;
                }
                world.Update();
            }
            else
            {
                if (RM.IsPressed(InputAction.Accept))
                {
                    g.Showscreen(new MainMenuScreen());
                }
            }

            if (Dead && RM.IsPressed(InputAction.Accept))
            {
                world = new World(RM.Levels[currentLevel]);
                Dead = false;
            }

            if (world.NextLevel)
            {
                currentLevel++;
                if (currentLevel >= RM.Levels.Count)
                {
                    g.Showscreen(new MainMenuScreen());
                    return;
                }

                world = new World(RM.Levels[currentLevel]);
                Dead = false;
            }

            if (RM.IsPressed(InputAction.Back))
            {
                RM.SaveScreenshotsToDisk();
                paused = !paused;
            }

            //if (RM.NextMusic.Count <= 0)
            //{
            //    if (Dead)
            //    {
            //        RM.NextMusic.Enqueue("music4");
            //        RM.NextMusic.Enqueue("music4");
            //        RM.NextMusic.Enqueue("music4");
            //        RM.NextMusic.Enqueue("music3");
            //    }
            //    else if (world.IsAlarmRinging)
            //    {
            //        RM.NextMusic.Enqueue("music3");
            //        RM.NextMusic.Enqueue("music3");
            //        RM.NextMusic.Enqueue("music3");
            //        RM.NextMusic.Enqueue("music4");
            //    }
            //    else
            //    {
            //        RM.NextMusic.Enqueue("music2");
            //    }
            //}

            // camera.position.Y = island.CheckHeightCollision(camera.position);
        }

        public override void Draw()
        {
            if (world.DrawSubshit)
            {
                RenderTarget2D screenshot = new RenderTarget2D(GraphicsDevice, G.Width, G.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                GraphicsDevice.SetRenderTarget(screenshot);
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };
                GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
                GraphicsDevice.Clear(Color.Black);
                world.DrawReal();
                GraphicsDevice.SetRenderTarget(null);

                Color[] data = new Color[G.Width * G.Height];
                screenshot.GetData<Color>(0, new Rectangle(0, 0, G.Width, G.Height), data, 0, data.Length);
                shot = new Texture2D(GraphicsDevice, G.Width, G.Height);
                shot.SetData<Color>(data);
            }

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            //camera.ApplyCustom(e, wor);

            foreach (var p in e.CurrentTechnique.Passes)
            {
                p.Apply();
            }

            world.DrawMinimal();
            
            
                spriteBatch.Begin();

                if (world.DrawSubshit && shot != null)
                {
                    spriteBatch.Draw(shot, new Rectangle((G.Width - world.SubWidth) / 2, (G.Height - world.SubHeight) / 2, world.SubWidth, world.SubHeight), new Rectangle((G.Width - world.SubWidth) / 2, (G.Height - world.SubHeight) / 2, world.SubWidth, world.SubHeight), Color.White);
                }

            if (paused)
            {
                spriteBatch.Draw(RM.GetTexture("white"), new Rectangle((int)(G.Width * 0.25f), (int)(G.Height * 0.4f), (int)(G.Width * 0.5f), (int)(G.Height * 0.2f)), Color.Black);
                spriteBatch.DrawString(g.font, "Game paused. Press " + RM.GetButtons(InputAction.Accept).First().ToString() + " to exit", new Vector2((G.Width * 0.25f) + 64, G.Height / 2 - 32), Color.Red);
                spriteBatch.DrawString(g.font, "Press " + RM.GetButtons(InputAction.Back).First().ToString() + " to continue playing", new Vector2((G.Width * 0.25f) + 64, G.Height / 2), Color.Green);
            }
            if (Dead)
            {
                spriteBatch.DrawString(g.font, "You were eaten! press " + RM.GetButtons(InputAction.Accept).First().ToString() + " to restart", new Vector2((G.Width * 0.25f) + 64, G.Height - 64), Color.White);
            }
            if (achievementToRender == null && Achievements.toShow.Any())
            {
                var newAchievement = Achievements.toShow.Dequeue();
                achievementToRender = newAchievement;
            }

            if (achievementToRender != null)
            {
                bool shouldRemove = achievementToRender.Draw();
                if (shouldRemove)
                {
                    achievementToRender = null;
                }
            }

            //spriteBatch.DrawString(g.font, camera.GetCameraDirection().ToString(), Vector2.Zero, Color.White);
            //spriteBatch.DrawString(g.font, camera.upDownRot.ToString() + ", " + camera.leftRightRot.ToString(), new Vector2(0, 64), Color.White);
            world.DrawSprites();
            spriteBatch.End();
        }

        public override void Show()
        {
            IM.SnapToCenter = false;
            g.IsMouseVisible = true;
            base.Show();
            RM.MusicEnabled = false;
        }

        public override void Hide()
        {
            Player.monsterplayer.Stop(true);
            base.Hide();
        }

        Achievement achievementToRender;
        private Texture2D shot;

        public bool Dead { get; set; }
    }
}

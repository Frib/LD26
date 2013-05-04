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

        RenderTarget2D leftEye = new RenderTarget2D(G.g.GraphicsDevice, G.Width / 2, G.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        RenderTarget2D rightEye = new RenderTarget2D(G.g.GraphicsDevice, G.Width / 2, G.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

        RenderTarget2D leftGoggle = new RenderTarget2D(G.g.GraphicsDevice, G.Width / 2, G.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        RenderTarget2D rightGoggle = new RenderTarget2D(G.g.GraphicsDevice, G.Width / 2, G.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

        public override void Draw()
        {
            Camera3d.c.ApplyCustom(world.player);

            if (world.DrawSubshit)
            {
                GraphicsDevice.SetRenderTarget(leftGoggle);
                Camera3d.c.SetLeftEye(G.g.VoidEffect);
                GraphicsDevice.Clear(Color.Black);
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };
                world.DrawReal();

                Camera3d.c.SetRightEye(G.g.VoidEffect);
                GraphicsDevice.SetRenderTarget(rightGoggle);
                GraphicsDevice.Clear(Color.Black);
                world.DrawReal();
            }

            Camera3d.c.SetLeftEye(G.g.VoidEffect);
            GraphicsDevice.SetRenderTarget(leftEye);
            GraphicsDevice.Clear(Color.Black);
            world.DrawMinimal();

            Camera3d.c.SetRightEye(G.g.VoidEffect);
            GraphicsDevice.SetRenderTarget(rightEye);
            GraphicsDevice.Clear(Color.Black);
            world.DrawMinimal();

            PostProcessNone();
            
            spriteBatch.Begin();
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

        void PostProcessNone()
        {
           //RenderTarget2D finalLeft = new RenderTarget2D(GraphicsDevice, 640, 800);
           //RenderTarget2D finalRight = new RenderTarget2D(GraphicsDevice, 640, 800);

           GraphicsDevice.SetRenderTarget(null);
           GraphicsDevice.Clear(Color.Black);
           spriteBatch.Begin();
           spriteBatch.Draw(leftEye, new Rectangle(0, 0, 640, 800), Color.White);
           spriteBatch.Draw(rightEye, new Rectangle(640, 0, 640, 800), Color.White);

           if (world.DrawSubshit)
           {
               spriteBatch.Draw(leftGoggle, new Rectangle(180, 200, 460, 400), new Rectangle(180, 200, 460, 400), Color.White);
               spriteBatch.Draw(rightGoggle, new Rectangle(640, 200, 460, 400), new Rectangle(0, 200, 460, 400), Color.White);
           }
           spriteBatch.End();
        }

        //void PostProcess()
        //{
        //    RenderTarget2D finalLeft = new RenderTarget2D(GraphicsDevice, 640, 800);
        //    RenderTarget2D finalRight = new RenderTarget2D(GraphicsDevice, 640, 800);

        //    GraphicsDevice.SetRenderTarget(finalLeft);
        //    GraphicsDevice.Clear(Color.Black);
        //    spriteBatch.Begin();
        //    spriteBatch.Draw(leftEye, new Rectangle(0, 0, 640, 800), Color.White);

        //    if (world.DrawSubshit)
        //    {
        //        spriteBatch.Draw(leftGoggle, new Rectangle(180, 200, 460, 400), new Rectangle(180, 200, 460, 400), Color.White);
        //    }
        //    spriteBatch.End();
        //    GraphicsDevice.SetRenderTarget(finalRight);
        //    GraphicsDevice.Clear(Color.Black);
        //    spriteBatch.Begin();
        //    spriteBatch.Draw(rightEye, new Rectangle(0, 0, 640, 800), Color.White);

        //    if (world.DrawSubshit)
        //    {
        //        spriteBatch.Draw(rightGoggle, new Rectangle(0, 200, 460, 400), new Rectangle(0, 200, 460, 400), Color.White);
        //    } 
        //    spriteBatch.End();

        //    //RenderTarget2D resultleft = new RenderTarget2D(GraphicsDevice, 1280, 800);
        //    //RenderTarget2D resultright = new RenderTarget2D(GraphicsDevice, 1280, 800);

        //    GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        //    GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        //    GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };

        //    float w = (GraphicsDevice.Viewport.Width) / (float)(G.Width);
        //    float h = (GraphicsDevice.Viewport.Height) / (float)(G.Height);
        //    float x = (GraphicsDevice.Viewport.X) / (float)(G.Width);
        //    float y = (GraphicsDevice.Viewport.Y) / (float)(G.Height);

        //    float ratio = (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;

        //    float scaleFactor = 1f;

        //    G.g.BarrelEffect.Parameters["Texture"].SetValue(finalLeft);
        //    G.g.BarrelEffect.Parameters["ScaleIn"].SetValue(new Vector2((2 / w), (2 / h) / ratio));
        //    G.g.BarrelEffect.Parameters["Scale"].SetValue(new Vector2((w / 2) * scaleFactor, (h / 2) * scaleFactor * ratio));
        //    G.g.BarrelEffect.Parameters["HmdWarpParam"].SetValue(RiftSettings.DistortionK);
        //    G.g.BarrelEffect.Parameters["ScreenCenter"].SetValue(new Vector2(x + w * 0.5f, y + h * 0.5f));
        //    G.g.BarrelEffect.Parameters["LensCenter"].SetValue(new Vector2(x + (w + RiftSettings.LensSeparationDistance * 0.5f) * 0.5f, y + h * 0.5f));

        //    Matrix texm = new Matrix(w, 0, 0, x,
        //                0, h, 0, y,
        //                0, 0, 0, 0,
        //                0, 0, 0, 1);

        //    G.g.BarrelEffect.Parameters["Texm"].SetValue(texm);

        //    Matrix view = new Matrix(2, 0, 0, -1f,
        //                              0, 2, 0, -1f,
        //                              0, 0, 0, 0,
        //                              0, 0, 0, 1);

        //    G.g.BarrelEffect.Parameters["View"].SetValue(view);
        //    G.g.BarrelEffect.Techniques[0].Passes[0].Apply();

        //    GraphicsDevice.SetRenderTarget(null);
        //    GraphicsDevice.Clear(Color.Black);
        //    QuadRender.RenderQuadLeft(GraphicsDevice, G.g.BarrelEffect);
        //    G.g.BarrelEffect.Parameters["ScreenCenter"].SetValue(new Vector2(x + w * 0.5f, y + h * 0.5f));
        //    G.g.BarrelEffect.Parameters["Texture"].SetValue(finalRight);
        //    G.g.BarrelEffect.Parameters["LensCenter"].SetValue(new Vector2(x + (w - RiftSettings.LensSeparationDistance * 0.5f) * 0.5f, y + h * 0.5f));
        //    QuadRender.RenderQuadRight(GraphicsDevice, G.g.BarrelEffect);

            
        //    //GraphicsDevice.SetRenderTarget(null);

        //    finalLeft.Dispose();
        //    finalRight.Dispose();
        //    //GraphicsDevice.Clear(Color.Black);
        //    //spriteBatch.Begin();
        //    //spriteBatch.Draw(resultleft, new Rectangle(0, 0, 640, 800), Color.White);
        //    //spriteBatch.Draw(resultright, new Rectangle(640, 0, 640, 800), Color.White);
        //    //spriteBatch.End();
        //    //resultleft.Dispose();
        //    //resultright.Dispose();
        //}

        //void PostProcessOld()
        //{
        //    RenderTarget2D final = new RenderTarget2D(GraphicsDevice, 1280, 800);

        //    GraphicsDevice.SetRenderTarget(final);
        //    GraphicsDevice.Clear(Color.Black);

        //    spriteBatch.Begin();

        //    spriteBatch.Draw(leftEye, new Rectangle(0, 0, 640, 800), Color.White);
        //    spriteBatch.Draw(rightEye, new Rectangle(640, 0, 640, 800), Color.White);

        //    if (world.DrawSubshit)
        //    {
        //        spriteBatch.Draw(leftGoggle, new Rectangle(180, 200, 480, 400), new Rectangle(180, 200, 480, 400), Color.White);
        //        spriteBatch.Draw(rightGoggle, new Rectangle(640, 200, 480, 400), new Rectangle(0, 200, 480, 400), Color.White);
        //    }

        //    spriteBatch.End();

        //    GraphicsDevice.SetRenderTarget(null);
        //    GraphicsDevice.Clear(Color.Black);
        //    GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        //    GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        //    GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };

        //    float w = (GraphicsDevice.Viewport.Width) / (float)(G.Width);
        //    float h = (GraphicsDevice.Viewport.Height) / (float)(G.Height);
        //    float x = (GraphicsDevice.Viewport.X) / (float)(G.Width);
        //    float y = (GraphicsDevice.Viewport.Y) / (float)(G.Height);

        //    float ratio = (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;

        //    float scaleFactor = 1f;

        //    G.g.BarrelEffect.Parameters["Texture"].SetValue(final);
        //    G.g.BarrelEffect.Parameters["ScaleIn"].SetValue(new Vector2((2/w), (2/h) / ratio));
        //    G.g.BarrelEffect.Parameters["Scale"].SetValue(new Vector2((w/2) * scaleFactor, (h/2) * scaleFactor * ratio));
        //    G.g.BarrelEffect.Parameters["HmdWarpParam"].SetValue(RiftSettings.DistortionK);
        //    G.g.BarrelEffect.Parameters["ScreenCenter"].SetValue(new Vector2(x + w * 0.5f, y + h * 0.5f));
        //    G.g.BarrelEffect.Parameters["LensCenter"].SetValue(new Vector2(x + (w + RiftSettings.LensSeparationDistance * 0.5f)*0.5f, y + h*0.5f));

        //    Matrix texm = new Matrix(w, 0, 0, x,
        //                0, h, 0, y,
        //                0, 0, 0, 0,
        //                0, 0, 0, 1);

        //    G.g.BarrelEffect.Parameters["Texm"].SetValue(texm);

        //    Matrix view = new Matrix(2, 0, 0, -1,
        //                              0, 2, 0, -1,
        //                              0, 0, 0, 0,
        //                              0, 0, 0, 1);

        //    G.g.BarrelEffect.Parameters["View"].SetValue(view);
        //    G.g.BarrelEffect.Techniques[0].Passes[0].Apply();

        //    //QuadRender.RenderQuad(GraphicsDevice, G.g.BarrelEffect);

        //    final.Dispose();
        //}

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

        public bool Dead { get; set; }
    }
}

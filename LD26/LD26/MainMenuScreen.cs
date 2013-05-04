using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LD26.entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD26
{
    class MainMenuScreen : Screen
    {
        private List<MenuOption> options = new List<MenuOption>();
        private MenuOption selected;

        public MainMenuScreen()
        {

        }

        public override void Show()
        {
            CreateControls();
            G.g.IsMouseVisible = false;
            IM.SnapToCenter = false;
            EndPortal.musicindex = 0;
        }

        private void CreateControls()
        {
            options.Clear();
            options.Add(new MenuOption("Play!", RM.font) { Action = new Action(() => g.Showscreen(new IntroScreen())) });
            //options.Add(new MenuOption("Editorclick level", RM.font) { Action = new Action(() => g.Showscreen(new LevelSelectScreen())) });
            //options.Add(new MenuOption("Help", RM.font) { Action = new Action(() => g.Showscreen(new HelpScreen(this))) });
            //options.Add(new MenuOption("View achievements", RM.font) { Action = new Action(() => g.Showscreen(new AchievementScreen(this))) });
            //options.Add(new MenuOption("View screenshots", RM.font) { Action = new Action(() => g.Showscreen(new ScreenshotScreen(this))) });
            options.Add(new MenuOption("Configure Controls", RM.font) { Action = new Action(() => g.Showscreen(new ControlScreen(this))) });
            options.Add(new MenuOption("Enable fullscreen (recommended)", RM.font) { Action = () => G.g.graphics.ToggleFullScreen()});
            options.Add(new MenuOption("Quit", RM.font) { Action = new Action(() => g.Exit()) });
            CalculatePositions();
        }

        private void CalculatePositions()
        {
            int offsetY = 400 - 16 * options.Count;
            var font = RM.font;
            var minSize = font.MeasureString("<EMPTY>").X;
            foreach (var o in options)
            {
                var size = Math.Max(font.MeasureString(o.Name).X, minSize);
                o.Position = new Vector2(320 - (size / 2), offsetY);
                offsetY += 32;
            }
        }

        private int ticker = 0;
        public override void Update()
        {
            ticker++;
            if (ticker % 400 == 0)
            {
                options[0].Name = "Die!";
            }
            if (ticker % 400 == 15)
            {
                options[0].Name = "Play!";
            }
            var m = IM.MousePos;
            var mpos = new Vector2(m.X, m.Y);

            foreach (var o in options)
            {
                if (o.Intersects(mpos))
                {
                    if (RM.IsPressed(InputAction.Accept) || RM.IsPressed(InputAction.EditorLeftClick))
                    {
                        o.Action();
                    }
                    selected = o;
                }
            }
        }

        private int offset = 0;
        public override void Draw()
        {
            offset += (G.r.Next(5) - 2);
            if (offset < 0)
            {
                offset = 0;
            }
            if (offset > 96)
            {
                offset = 96;
            }
            GraphicsDevice.Clear(new Color(offset, 0, 0));
            SpriteBatch sb = G.g.spriteBatch;
            sb.Begin();
            foreach (var o in options)
            {

                o.Draw(selected == o ? Color.White : Color.Yellow);

            }

            //sb.Draw(RM.GetTexture("camera"), new Rectangle(500, 192, 128, 128), Color.White);
            //sb.DrawString(RM.font, "The Void", new Vector2(464, 320), Color.Green);
            sb.DrawString(RM.font, "Where things go null", new Vector2(64, 464+128), Color.Fuchsia);
            sb.DrawString(RM.font, "into the void", new Vector2(64, 490+128), Color.Fuchsia);

            sb.Draw(RM.GetTexture("white"), new Rectangle((int)IM.MousePos.X, (int)IM.MousePos.Y, 12, 12), Color.Lime);
            sb.Draw(RM.GetTexture("white"), new Rectangle((int)IM.MousePos.X + 640, (int)IM.MousePos.Y, 9, 9), Color.Lime);
            sb.Draw(RM.GetTexture("white"), new Rectangle((int)IM.MousePos.X - 640, (int)IM.MousePos.Y, 9, 9), Color.Lime);
            sb.End();
        }
    }
}

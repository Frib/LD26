using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD26
{
    class ScreenshotScreen : Screen
    {
        private MainMenuScreen gameScreen;
        int offset;
        public ScreenshotScreen(MainMenuScreen gameScreen)
        {
            this.gameScreen = gameScreen;
            offset = (RM.screenshots.Count - 2) * -650 - 120;
        }
        public override void Update()
        {
            if (RM.IsPressed(InputAction.Back))
            {
                g.Showscreen(gameScreen);
            }

            if (RM.IsDown(InputAction.Left))
                offset += 20;
            if (RM.IsDown(InputAction.Right))
                offset -= 20;
            offset -= IM.ScrollDelta;            
        }

        public override void Show()
        {
            IM.SnapToCenter = false;
            g.IsMouseVisible = true;
            base.Show();
        }

        public override void Draw()
        {
            GraphicsDevice.Clear(new Color(48, 48, 48));

            spriteBatch.Begin();
            for (int i = 0; i < RM.screenshots.Count; i++)
            {
                spriteBatch.Draw(RM.screenshots[i], new Rectangle(64 + (i * 650) + offset, 160, 640, 360), Color.White);
            }
            spriteBatch.End();
        }
    }

}

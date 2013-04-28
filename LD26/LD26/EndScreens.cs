using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD26
{
    public class FailScreen : Screen
    {
        public override void Show()
        {
            RM.MusicEnabled = true;
            base.Show();
        }

        private int timer = 0;
        public override void Update()
        {
            timer++;
            if (timer > 60 && (RM.IsPressed(InputAction.Accept) || RM.IsPressed(InputAction.Back)))
            {
                G.g.Showscreen(new GameScreen(5));
            }
        }

        public override void Draw()
        {
            G.g.spriteBatch.Begin(); 
            G.g.spriteBatch.DrawString(RM.font, "You escaped, but the monster escaped with you.", new Vector2(400, 200), Color.OrangeRed);
            G.g.spriteBatch.DrawString(RM.font, "The world is doomed. So here's a potato to cheer you up.", new Vector2(300, 300), Color.Yellow);
            G.g.spriteBatch.Draw(RM.GetTexture("potato"), new Vector2(600, 400), Color.White);
            if (timer > 180)
            {
                G.g.spriteBatch.DrawString(RM.font, "Press " + RM.GetFirstMappedButton(InputAction.Accept) + " to try again!", new Vector2(400, 600), Color.Yellow);
            }
            G.g.spriteBatch.End();
        }
    }

    public class WinScreen : Screen
    {
        private int timer = 0;

        public override void Show()
        {
            RM.MusicEnabled = true;
            base.Show();
        }
        public override void Update()
        {
            timer++;
            if (timer > 60 && (RM.IsPressed(InputAction.Accept) || RM.IsPressed(InputAction.Back)))
            {
                G.g.Showscreen(new MainMenuScreen());
            }
        }

        public override void Draw()
        {
            G.g.spriteBatch.Begin();
            G.g.spriteBatch.DrawString(RM.font, "You escaped, and no monster escaped with you.", new Vector2(400, 200), Color.Green);
            G.g.spriteBatch.DrawString(RM.font, "World peace and prosperity are now a reality, somehow. Good job!", new Vector2(350, 300), Color.LightGreen);

            if (timer > 180)
            {
                G.g.spriteBatch.DrawString(RM.font, "Press " + RM.GetFirstMappedButton(InputAction.Accept) + " to continue", new Vector2(400, 600), Color.Yellow);
            }
            G.g.spriteBatch.End();
        }
    }
}

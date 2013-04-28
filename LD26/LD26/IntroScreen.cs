using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD26
{
    class IntroScreen : Screen
    {
        public override void Draw()
        {
            var sb = G.g.spriteBatch;
            sb.Begin();

            sb.Draw(RM.GetTexture("intro"), new Vector2(0, 0), Color.White);
            sb.End();
        }

        public override void Update()
        {
            if (RM.IsPressed(InputAction.Accept) || RM.IsPressed(InputAction.EditorLeftClick) ||
                RM.IsPressed(InputAction.Action) || RM.IsPressed(InputAction.Back))
            {
                G.g.Showscreen(new GameScreen(0));
            }
        }
    }
}

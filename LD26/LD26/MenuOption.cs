using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LD26
{
    public class MenuOption
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private Vector2 size;

        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }
        private Vector2 position;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        private Action action;

        public Action Action
        {
            get { return action; }
            set { action = value; }
        }

        private SpriteFont spriteFont;

        public SpriteFont SpriteFont
        {
            get { return spriteFont; }
            set
            {
                spriteFont = value;
                Size = spriteFont.MeasureString(Name);
            }
        }

        private bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        public bool Intersects(Vector2 position)
        {
            return (visible && position.X >= Position.X &&
                     position.X <= Position.X + Size.X &&
                     position.Y >= Position.Y &&
                     position.Y <= Position.Y + Size.Y) ||
 
                     (visible && position.X >= Position.X + 640 &&
                     position.X <= Position.X + Size.X+ 640 &&
                     position.Y >= Position.Y &&
                     position.Y <= Position.Y + Size.Y) ;
        }

        internal void Draw(Color color)
        {
            if (visible)
            {
                G.g.spriteBatch.DrawString(spriteFont, name, position, color);
                G.g.spriteBatch.DrawString(spriteFont, name, position + new Vector2(640, 0), color);
            }
        }

        public MenuOption(string name, SpriteFont font)
        {
            this.Name = name;
            this.SpriteFont = font;
        }
    }
}

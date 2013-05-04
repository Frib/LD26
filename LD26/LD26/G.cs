using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LD26
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class G : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public static G g;
        public static Random r = new Random();

        public Effect VoidEffect;
        public Effect BarrelEffect;

        public SpriteFont font;
        Screen currentScreen;

        public G()
        {
            g = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "The Void";
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 1280;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            RM.ConfigureKeys();
            currentScreen = new MainMenuScreen();
            currentScreen.Show();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            VoidEffect = Content.Load<Effect>("voidshader");
            BarrelEffect = Content.Load<Effect>("barrelshader");

            VoidEffect.Parameters["AmbientLightColor"]    .SetValue(new Vector3(0.2f, 0.2f, 0.2f));
            VoidEffect.Parameters["DirLight0Direction"]   .SetValue(new Vector3(-0.1f, -0.4f, 0.7f));
            VoidEffect.Parameters["DirLight0DiffuseColor"].SetValue(new Vector3(0.6f, 0.6f, 0.6f));
            VoidEffect.Parameters["DirLight1Direction"]   .SetValue(new Vector3(0.4f, 0.4f, 0.7f));
            VoidEffect.Parameters["DirLight1DiffuseColor"].SetValue(new Vector3(0.2f, 0.2f, 0.2f));

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            RM.font = font;

            LoadTexture("white");
            LoadTexture("red");
            LoadTexture("r");
            LoadTexture("potato");
            LoadTexture("cat");
            LoadTexture("door");
            LoadTexture("blue");
            LoadTexture("intro");
            LoadTexture("unknownachievement");

            for (int i = 0; i < 20; i++)
            {
                LoadTexture("monster\\m" + i);
            }
            LoadTexture("monster\\face");

            RM.LoadLevels();
            RM.AddSound("monsternoise", Content.Load<SoundEffect>("monsternoise"));
            RM.AddSound("monstereat", Content.Load<SoundEffect>("monstereat"));
            RM.AddSound("monsteractive", Content.Load<SoundEffect>("monsteractive"));
            RM.AddSound("footstep", Content.Load<SoundEffect>("footstep"));
            RM.AddSound("musicmenu", Content.Load<SoundEffect>("musicmenu"));
            RM.AddSound("buttonpress", Content.Load<SoundEffect>("buttonpress"));

            RM.AddSound("portalwarmup", Content.Load<SoundEffect>("portalwarmup"));
            RM.AddSound("portalcharging", Content.Load<SoundEffect>("portalcharging"));
            RM.AddSound("portalchargedone", Content.Load<SoundEffect>("portalchargedone"));
            RM.AddSound("portalchargedonesustain", Content.Load<SoundEffect>("portalchargedonesustain"));

            LoadSound("voice\\dude\\L1E1");
            LoadSound("voice\\dude\\L1E2");
            LoadSound("voice\\dude\\L1E3");
            LoadSound("voice\\dude\\L1V1");
            LoadSound("voice\\dude\\L1V2");
            LoadSound("voice\\dude\\L1V3");
            LoadSound("voice\\dude\\L1V4");
            LoadSound("voice\\dude\\L2E1");
            LoadSound("voice\\dude\\L2E2");
            LoadSound("voice\\dude\\L2V1");
            LoadSound("voice\\dude\\L3E1");
            LoadSound("voice\\dude\\L3V1");
            LoadSound("voice\\dude\\L4V1");
            LoadSound("voice\\dude\\L4V2");
            LoadSound("voice\\dude\\L5V1");
            LoadSound("voice\\dude\\L6E1");
            LoadSound("voice\\dude\\L6E2");
            LoadSound("voice\\dude\\L6E3");

            LoadSound("voice\\voidling\\L2V1");
            LoadSound("voice\\voidling\\L3E1");
            LoadSound("voice\\voidling\\L3E2");
            LoadSound("voice\\voidling\\L3E3");
            LoadSound("voice\\voidling\\L3V1");
            LoadSound("voice\\voidling\\L3V2");
            LoadSound("voice\\voidling\\L4E1");
            LoadSound("voice\\voidling\\L5V1");
            LoadSound("voice\\voidling\\L6E1");
            LoadSound("voice\\voidling\\L6E2");
            LoadSound("voice\\voidling\\L6E3");
            LoadSound("voice\\voidling\\L6E4");

            LoadSound("musicbossp1");
            LoadSound("musicbossp2");
            LoadSound("musicbossp3");
            LoadSound("musicbossp4");

            LoadSound("meow");

            RM.Volume = 1;

            Achievements.AddAchievement("havingfun", "Daring this far", "Halfway there!", RM.GetTexture("monster\\m19"));
            Achievements.AddAchievement("mrsnuggles", "Nine lives", "Rescued Mr Snuggles", RM.GetTexture("cat"));

            Achievements.Load();
        }

        private void LoadTexture(string name)
        {
            RM.AddTexture(name, Content.Load<Texture2D>(name));
        }

        private void LoadSound(string name)
        {
            RM.AddSound(name, Content.Load<SoundEffect>(name));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            IM.NewState();
#if debug
            if (RM.IsPressed(InputAction.ChangeSound))
            {
                RM.Volume += 1;
                if (RM.Volume >= 3)
                {
                    RM.Volume = 0;
                }
            }
#endif

            RM.UpdateMusic();

            currentScreen.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };
            currentScreen.Draw();

            base.Draw(gameTime);
        }

        public static bool HasFocus { get { return g.IsActive; } }
        public static int Width { get { return g.Window.ClientBounds.Width; } }
        public static int Height { get { return g.Window.ClientBounds.Height; } }

        internal void Showscreen(Screen newScreen)
        {
            currentScreen.Hide();
            currentScreen = newScreen;
            currentScreen.Show();
        }
    }
}

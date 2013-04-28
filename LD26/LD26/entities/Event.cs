using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD26.entities
{
    public class Event : Entity
    {
        public override string Export()
        {
            return "E:E:OMGFILLTHISSHITKTHX";
        }

        internal static Entity Create(string p)
        {
            switch (p)
            {
                case "EventLevelOne": return new EventLevelOne();
                case "EventLevelTwo": return new EventLevelTwo();
                case "EventLevelThree": return new EventLevelThree();
                case "EventLevelFour": return new EventLevelFour();
                case "EventLevelFive": return new EventLevelFive();
                case "EventLevelSix": return new EventLevelSix();
                default: throw new Exception();
            }
        }

        public override void DrawHUDInfo()
        {
            DrawExtraHudShit(0);
        }
    }

    public class EventLevelFive : Event
    {
        private int ticker = 0;

        public override void Update()
        {
            ticker++;
            if (ticker == 60)
            {
                RM.PlaySoundEueue("voice\\dude\\L5V1");
            }
            if (ticker == 900)
            {
                RM.PlaySoundEueue("voice\\voidling\\L5V1");
            }
            if (ticker == 1200)
            {
                World.entities.OfType<Door>().Last().state = DoorState.Opening;
            }
        }
    }

    public class EventLevelSix : Event
    {
        private int ticker = 0;

        private Monster outer;
        private Monster inner;

        List<Vector2> outerRing = new List<Vector2>() { new Vector2(72, -72), new Vector2(72, 56), new Vector2(-56, 56), new Vector2(-56, -72) };
        List<Vector2> innerRing = new List<Vector2>() { new Vector2(40, -40), new Vector2(40, 24), new Vector2(-24, 24), new Vector2(-24, -40) };

        private int outerindex = 0;
        private int innerindex = 0;
        private bool e1;

        public override void Update()
        {
            if (ticker == 0)
            {
                init();
            }
            ticker++;
            if (ticker % 480 == 0)
            {
                outerindex++;
                if (outerindex >= outerRing.Count)
                {
                    outerindex = 0;
                }

                outer.LastKnownPlayerPos = outerRing[outerindex];
            }

            if (ticker % 380 == 0)
            {
                innerindex++;
                if (innerindex >= innerRing.Count)
                {
                    innerindex = 0;
                }

                //inner.LastKnownPlayerPos = innerRing[outerindex];
            }
            
            if (!e1 && (World.player.Position - new Vector2(-116, 8)).Length() < 8)
            {
                RM.PlaySoundEueue("voice\\voidling\\L6E1");
                e1 = true;
            }

            if (!asdfaseura && World.entities.OfType<Door>().Last().state == DoorState.Open)
            {
                asdfaseura = true;
                EndPortal.musicindex = 1;
            }
        }

        private bool asdfaseura;

        private void init()
        {
            var ms = World.entities.OfType<Monster>().ToList();

            outer = ms[1];
            //inner = ms[2];

            outer.LastKnownPlayerPos = outerRing[0];
            //inner.LastKnownPlayerPos = innerRing[0];

            outer.Active = true;
            //inner.Active = true;
        }
    }

    public class EventLevelFour : Event
    {
        private int ticker = 0;

        public override void Update()
        {
            ticker++;

            if (ticker == 180)
            {
                RM.PlaySoundEueue("voice\\dude\\L4V1");
            }
            if (ticker == 1200)
            {
                RM.PlaySoundEueue("voice\\dude\\L4V2");
            }
            if (ticker == 7000)
            {
                RM.PlaySoundEueue("voice\\voidling\\L4E1");
            }
            if (ticker == 7200)
            {
                foreach (var d in World.entities.OfType<Door>().Skip(8))
                {
                    d.state = DoorState.Opening;
                }
            }
        }
    }

    public class EventLevelThree : Event
    {
        private int ticker = 0;

        public override void Update()
        {
            ticker++;
            if (ticker == 60)
            {
                RM.PlaySoundEueue("voice\\dude\\L3V1");
            }
            if (ticker == 690)
            {
                RM.PlaySoundEueue("voice\\voidling\\L3V1");
            }
            if (ticker == 1800)
            {
                RM.PlaySoundEueue("voice\\voidling\\L3V2");
            }
            if (ticker == 3600)
            {
                RM.PlaySoundEueue("voice\\voidling\\L3E1");
            }
            if (ticker == 3900)
            {
                Achievements.Achieve("havingfun");
            }
            if (ticker == 4000)
            {
                RM.PlaySoundEueue("voice\\voidling\\L3E2");
            }
            if (ticker == 5400)
            {
                RM.PlaySoundEueue("voice\\voidling\\L3E3");
            }
        }
    }

    public class EventLevelTwo : Event
    {
        private int ticker = 0;

        public override void Update()
        {
            ticker++;
            if (ticker == 60)
            {
                RM.PlaySoundEueue("voice\\dude\\L2V1");
            }
            if (ticker == 900)
            {
                World.entities.OfType<Door>().Last().state = DoorState.Opening;
            }
            if (ticker == 1800)
            {
                RM.PlaySoundEueue("voice\\voidling\\L2V1");
            }
            if (ticker == 1950)
            {
                RM.PlaySoundEueue("voice\\dude\\L2E1");
            }
            if (ticker == 3000 && World.entities.OfType<Door>().Skip(1).First().state != DoorState.Open)
            {
                RM.PlaySoundEueue("voice\\dude\\L2E2");
            }
        }
    }

    public class EventLevelOne : Event
    {
        private int ticker = 0;

        public EventLevelOne()
        {
            Camera3d.c.leftRightRot = MathHelper.Pi;
        }

        public override void Update()
        {
            ticker++;

            if (ticker == 1)
            {
                World.player.Freeze();                
            }

            if (ticker == 60)
            {
                RM.PlaySoundEueue("voice\\dude\\L1V1");
            }
            if (ticker == 600)
            {
                World.player.abilityTimer = 300;
            }
            if (ticker == 660)
            {
                RM.PlaySoundEueue("voice\\dude\\L1V2");
            }
            if (ticker == 800)
            {
                World.player.UnFreeze();                
            }
            if (ticker == 1020)
            {
                RM.PlaySoundEueue("voice\\dude\\L1V3");
            }
            if (ticker == 1500)
            {
                World.entities.OfType<Door>().First().state = DoorState.Opening;
            }
            if (ticker == 1700)
            {
                RM.PlaySoundEueue("voice\\dude\\L1V4");
            }

            if (!e1 && (World.player.Position - new Vector2(-40, -88)).Length() < 8 && ticker > 2100)
            {
                RM.PlaySoundEueue("voice\\dude\\L1E1");
                e1 = true;
            }

            if (!e2 && (World.player.Position - new Vector2(-40, -192)).Length() < 8 && World.entities.OfType<Door>().ToList()[2].state == DoorState.Closed)
            {
                RM.PlaySoundEueue("voice\\dude\\L1E2");
                e2 = true;
            }
            if (e2 && ticker > 3000)
            {
                e2timer++;                
            }
        }

        private int e2timer = 0;
        private bool e1;
        private bool e2;

        protected override void DrawExtraHudShit(int offset)
        {
            if (ticker > 1200 && ticker < 1500)
            {
                G.g.spriteBatch.DrawString(RM.font, "Move around with " + RM.GetFirstMappedButton(InputAction.Up) + ", " + RM.GetFirstMappedButton(InputAction.Down) + ", " + RM.GetFirstMappedButton(InputAction.Left) + ", " + RM.GetFirstMappedButton(InputAction.Right) + " and the mouse", new Vector2(384, 640), Color.Yellow);
            }
            if (ticker > 1700 && ticker < 2200)
            {
                G.g.spriteBatch.DrawString(RM.font, "Press and hold " + RM.GetFirstMappedButton(InputAction.Action) + " to enable your goggles", new Vector2(384, 640), Color.Yellow);                
            }
            if (ticker > 2300 && ticker < 2800)
            {
                G.g.spriteBatch.DrawString(RM.font, "Hold " + RM.GetFirstMappedButton(InputAction.Sprint) + " to sprint, but this makes noise!", new Vector2(384, 640), Color.Yellow);
            }

            if (e2timer > 600 && e2timer < 1200)
            {
                G.g.spriteBatch.DrawString(RM.font, "Press " + RM.GetFirstMappedButton(InputAction.Use) + " to push buttons", new Vector2(384, 640), Color.Yellow);
            }
        }
    }
}

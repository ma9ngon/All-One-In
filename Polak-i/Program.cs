using System;

namespace OneKeyToWin_AIO_Sebby
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;

    class Program
    {
        public static Menu Config;

        public static MenuList QHitChance = new MenuList("QHitChance", "Q Hit Chance", new[] { "Very High", "High", "Medium" });
        public static MenuList WHitChance = new MenuList("WHitChance", "W Hit Chance", new[] { "Very High", "High", "Medium" });
        public static MenuList EHitChance = new MenuList("EHitChance", "E Hit Chance", new[] { "Very High", "High", "Medium" });
        public static MenuList RHitChance = new MenuList("RHitChance", "R Hit Chance", new[] { "Very High", "High", "Medium" });

        public static AIHeroClient Player { get { return ObjectManager.Player; } }
        public static Spell Q, W, E, R, Q1, R1;
        public static bool Combo = false, Farm = false, Harass = false, LaneClear = false, None = false;
        public static int tickIndex = 0;

        private static string OktNews = "Port and fix by Polak-i";

        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += GameEvent_OnGameLoad;
        }

        private static void GameEvent_OnGameLoad()
        {
            Config = new Menu("OneKeyToWin_AIO" + Player.CharacterName, "OneKeyToWin AIO", true);

            switch (Player.CharacterName)
            {
                case "Lucian":
                    new Champions.Lucian();
                    break;
                case "Graves":
                    new Champions.Graves();
                    break;
                case "Xerath":
                    new Champions.Xerath();
                    break;
                case "Jinx":
                    new Champions.Jinx();
                    break;
                case "Vayne":
                    new Champions.Vayne();
                    break;
                case "Sivir":
                    new Champions.Sivir();
                    break;
                case "Caitlyn":
                    new Champions.Caitlyn();
                    break;
                case "Kalista":
                    new Champions.Kalista();
                    break;
                case "Ashe":
                    new Champions.Ashe();
                    break;
                case "Corki":
                    new Champions.Corki();
                    break;
            }

            Config.Add(new Menu("predictionMode" + Player.CharacterName, "Prediction MODE")
            {
                QHitChance,
                WHitChance,
                EHitChance,
                RHitChance
            });

            Game.OnUpdate += Game_OnUpdate;

            Config.Attach();

            Game.Print("<font size='30'>OneKeyToWin</font> <font color='#b756c5'>by Sebby</font>");
            Game.Print("<font color='#b756c5'>OKTW NEWS: </font>" + OktNews);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Combo = Orbwalker.ActiveMode == OrbwalkerMode.Combo;
            Farm = Orbwalker.ActiveMode == OrbwalkerMode.Harass || Orbwalker.ActiveMode == OrbwalkerMode.LaneClear;
            LaneClear = Orbwalker.ActiveMode == OrbwalkerMode.LaneClear;
            None = Orbwalker.ActiveMode == OrbwalkerMode.None;

            if ((Config["extra"] as Menu)?.GetValue<MenuBool>("harassMixed").Enabled ?? false)
            {
                Harass = Orbwalker.ActiveMode == OrbwalkerMode.Harass;
            }
            else
            {
                Harass = Orbwalker.ActiveMode == OrbwalkerMode.Harass || Orbwalker.ActiveMode == OrbwalkerMode.LaneClear;
            }

            tickIndex++;

            if (tickIndex > 4)
            {
                tickIndex = 0;
            }
        }

        public static void CastSpell(Spell qwer, AIBaseClient target)
        {
            var hitChance = HitChance.High;

            if (qwer.Slot == SpellSlot.Q)
            {
                hitChance = (HitChance)(4 - QHitChance.Index);
            }
            else if (qwer.Slot == SpellSlot.W)
            {
                hitChance = (HitChance)(4 - WHitChance.Index);
            }
            else if (qwer.Slot == SpellSlot.E)
            {
                hitChance = (HitChance)(4 - EHitChance.Index);
            }
            else if (qwer.Slot == SpellSlot.R)
            {
                hitChance = (HitChance)(4 - RHitChance.Index);
            }

            qwer.CastIfHitchanceMinimum(target, hitChance);
        }

        public static bool LagFree(int index)
        {
            return tickIndex == index;
        }
    }
}
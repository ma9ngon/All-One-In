using System.Collections.Generic;
using System.Linq;

namespace OneKeyToWin_AIO_Sebby
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;

    class Base : Program
    {
        public static Menu Local, HarassMenu, FarmMenu;

        public static List<MenuBool> HarassList = new List<MenuBool>();

        public static MenuBool manaDisable = new MenuBool("manaDisable", "Disable mana manager in combo", false);
        public static MenuBool harassMixed = new MenuBool("harassMixed", "Spell-harass only in mixed mode", false);

        public static MenuBool spellFarm = new MenuBool("spellFarm", "OKTW spells farm");
        public static MenuSlider LCminions = new MenuSlider("LCminions", "Lane clear minimum minions", 2, 0, 10);
        public static MenuSlider LCmana = new MenuSlider("LCmana", "Lane clear minimum mana", 50, 0, 100);

        public static float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;

        public static bool FarmSpells
        {
            get
            {
                return spellFarm.Enabled
                    && Orbwalker.ActiveMode == OrbwalkerMode.LaneClear
                    && Player.ManaPercent > LCmana.Value;
            }
        }

        static Base()
        {
            Local = new Menu(Player.CharacterName, Player.CharacterName);

            HarassMenu = new Menu("harass", "Harass");
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                var harass = new MenuBool("harass" + enemy.CharacterName, enemy.CharacterName);
                HarassList.Add(harass);
                HarassMenu.Add(harass);
            }

            FarmMenu = new Menu("farm", "Farm");
            FarmMenu.Add(spellFarm);
            FarmMenu.Add(LCminions);
            FarmMenu.Add(LCmana);

            Local.Add(HarassMenu);
            Local.Add(FarmMenu);

            Config.Add(new Menu("extra", "Extra settings OKTW©")
            {
                manaDisable,
                harassMixed
            });
            Config.Add(Local);

            spellFarm.Permashow();
            harassMixed.Permashow();
        }

        public static bool InHarassList(AIHeroClient t)
        {
            return HarassList.Any(e => e.Enabled && e.Name == "harass" + t.CharacterName);
        }
    }
}
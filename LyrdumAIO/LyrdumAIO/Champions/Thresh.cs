using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LyrdumAIO.Champions
{

    public class Thresh
    {
        private static Spell Q, W, E, R, Q2;
        private static Menu Config;

        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "Thresh")
            {
                return;
            }
            Q = new Spell(SpellSlot.Q, 1000f) { AddHitBox = true };
            Q2 = new Spell(SpellSlot.Q, 1000f) { AddHitBox = true };
            W = new Spell(SpellSlot.W, 950f);
            E = new Spell(SpellSlot.E, 350f) {AddHitBox = true };
            R = new Spell(SpellSlot.R, 350f);

            Q.SetSkillshot(0.25f * 4, 10f, float.MaxValue, true, SpellType.Line);
            W.SetSkillshot(0.75f, 10f, float.MaxValue, false, SpellType.Circle);
            E.SetSkillshot(0.75f, 10f, float.MaxValue, false, SpellType.Line);
            Q2.SetSkillshot(0.25f * 4, 140f, float.MaxValue, true, SpellType.Line);

            Config = new Menu("Thresh", "🕸 [𝐋𝐲𝐫𝐝𝐮𝐦𝐀𝐈𝐎]: 𝐓𝐡𝐫𝐞𝐬𝐡 🕸 ", true);

            var menuD = new Menu("dsettings", "👁 𝐃𝐑𝐀𝐖𝐈𝐍𝐆𝐒 ");
            menuD.Add(new MenuBool("drawQ", "Q Range  (RED)", true));
            menuD.Add(new MenuBool("drawE", "E Range  (BLUE)", true));
            menuD.Add(new MenuBool("drawW", "W Range (GREEN)", true));
            menuD.Add(new MenuBool("drawR", "R Range  (WHITE)", false));

            var MenuC = new Menu("infor", "👻 [𝐈𝐌𝐏𝐎𝐑𝐓𝐀𝐍𝐓] 𝐈𝐍𝐅𝐎𝐑𝐌𝐀𝐓𝐈𝐎𝐍𝐒 ", false);
            MenuC.Add(new Menu("infotool", " Harass||LastHit =  C, " + "\n LaneClear||JungleFarm = V, " + "\n Combo = SPACEBAR" + " \n Last Hit = X \n" + " Disable Drawings = L " + " \n Fore more FPS Disable EzEvade and LyrdumAIO Drawings + [Awarness] Waypoint and check Jungle->track only jungle \n If you have a bug or suggestion post it on discord server discord.gg/KfQFVhdqtz"));

            var menuM = new Menu("mana", "🔪 𝐌𝐀𝐍𝐀 𝐇𝐀𝐑𝐀𝐒𝐒 ");
            menuM.Add(new MenuSlider("manaW", "W mana %", 60, 0, 100));
            menuM.Add(new MenuSlider("manaE", "E mana %", 60, 0, 100));
            menuM.Add(new MenuSlider("manaQ", "Q mana %", 60, 0, 100));

            var MenuS = new Menu("harass", "🐧 𝐇𝐀𝐑𝐀𝐒𝐒 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            MenuS.Add(new MenuBool("useQ", "Use Q ", true));
            MenuS.Add(new MenuBool("useE", "Use E ", true));
            MenuS.Add(new MenuBool("useW", "Use W ", false));

            var Menuclear = new Menu("laneclear", "🐧 𝐋𝐀𝐍𝐄 𝐂𝐋𝐄𝐀𝐑 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            Menuclear.Add(new MenuBool("useE", "Use E ", true));

            var MenuJungle = new Menu("jungleskills", "🐧 𝐉𝐔𝐍𝐆𝐋𝐄 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            MenuJungle.Add(new MenuBool("useQ", "Use Q ", true));
            MenuJungle.Add(new MenuBool("useE", "Use E ", true));
            MenuJungle.Add(new MenuBool("useW", "Use W ", false));


            var menuK = new Menu("skinslide", "🤖 𝐒𝐊𝐈𝐍 𝐂𝐇𝐀𝐍𝐆𝐄𝐑 ");
            menuK.Add(new MenuSliderButton("skin", "SkinID", 0, 0, 20, false));

            var menuRR = new Menu("semiR", "☠ 𝐒𝐄𝐌𝐈 𝐊𝐄𝐘𝐒");
            menuRR.Add(new MenuKeyBind("farm", "Lane Clear spells", Keys.Select, KeyBindType.Toggle));

            Config.Add(MenuC);
            Config.Add(menuD);
            Config.Add(MenuS);
            Config.Add(Menuclear);
            Config.Add(MenuJungle);
            Config.Add(menuM);
            Config.Add(menuK);
            Config.Add(menuRR);
            

            Config.Attach();

            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        public static void OnGameUpdate(EventArgs args)
        {

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    logicQ();
                    logicW();
                    logicE();
                    break;

                case OrbwalkerMode.Harass:
                    harass();
                    break;

                case OrbwalkerMode.LaneClear:
                    Jungle();
                    LaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    Jungle();
                    
                    break;

            }

            logicR();
            skinch();
        }

        private static void skinch()
        {
            if (Config["skinslide"].GetValue<MenuSliderButton>("skin").Enabled)
            {
                int skinut = Config["skinslide"].GetValue<MenuSliderButton>("skin").Value;

                if (GameObjects.Player.SkinId != skinut)
                    GameObjects.Player.SetSkin(skinut);
            }
        }

        private static void harass()
        {
            var manaQ = Config["mana"].GetValue<MenuSlider>("manaQ").Value;
            var manaW = Config["mana"].GetValue<MenuSlider>("manaW").Value;
            var manaE = Config["mana"].GetValue<MenuSlider>("manaE").Value;
            var mana = GameObjects.Player.ManaPercent;

            if (mana > manaQ && Config["harass"].GetValue<MenuBool>("useQ").Enabled)
            {
                logicQ2();
            }

            if (mana > manaW && Config["harass"].GetValue<MenuBool>("useW").Enabled)
            {
                logicW();
            }

            if (mana > manaE && Config["harass"].GetValue<MenuBool>("useE").Enabled)
            {
                logicE();
            }
        }


        private static void OnDraw(EventArgs args)
        {
            var position = GameObjects.Player.Position;

            if (Config["dsettings"].GetValue<MenuBool>("drawQ").Enabled)
            {
                Render.Circle.DrawCircle(position, Q.Range, System.Drawing.Color.Red);
            }

            if (Config["dsettings"].GetValue<MenuBool>("drawE").Enabled)
            {
                Render.Circle.DrawCircle(position, E.Range, System.Drawing.Color.Blue);
            }

            if (Config["dsettings"].GetValue<MenuBool>("drawW").Enabled)
            {
                Render.Circle.DrawCircle(position, W.Range, System.Drawing.Color.Green);
            }

            if (Config["dsettings"].GetValue<MenuBool>("drawR").Enabled)
            {
                Render.Circle.DrawCircle(position, R.Range, System.Drawing.Color.White);
            }

            CanSpellFarm();
        }

        private static void logicQ()
        {
            if (Q.IsReady())
            {
                var target = Q.GetTarget();

                if (!target.IsValidTarget())
                    return;

                var input = Q.GetPrediction(target, true);
;

                if (input.Hitchance >= HitChance.High)
                {
                    Q.Cast(input.CastPosition);
                }

            }
        }

        private static void logicQ2()
        {
            if (Q.IsReady())
            {
                var target = Q.GetTarget();
                var input = Q.GetPrediction(target, true);

                if (input.Hitchance >= HitChance.High && target.CanMove)
                {
                    Q.Cast(input.CastPosition);
                }
            }
        }

        private static void logicW()
        {
            if (W.IsReady())
            {
                var target = W.GetTarget();
                var ally = GameObjects.AllyHeroes.Where(x => x.IsValidTarget(W.Range, checkTeam: false) && !x.IsMe).OrderBy(x => x.Health).FirstOrDefault();

                if (ally != null)
                {

                    if (!target.CanMove || target.HasBuffOfType(BuffType.Stun))
                    {
                        W.Cast(ally.Position);
                    }

                    var enemys = ally.Position.CountEnemyHeroesInRange(ally.AttackRange);

                    if (enemys == 1 && ally.Health <= target.Health)
                    {
                        W.Cast(ally.Position);
                    }

                    if (enemys > 1 && ally.HealthPercent <= 30)
                    {
                        W.Cast(ally.Position);
                    }

                    if (ally.HasBuffOfType(BuffType.Stun)
                        || ally.HasBuffOfType(BuffType.Slow)
                        || ally.HasBuffOfType(BuffType.Silence)
                        || ally.HasBuffOfType(BuffType.Asleep)
                        || ally.HasBuffOfType(BuffType.Blind)
                        || ally.HasBuffOfType(BuffType.Charm)
                        || ally.HasBuffOfType(BuffType.Disarm)
                        || ally.HasBuffOfType(BuffType.Knockup)
                        || ally.HasBuffOfType(BuffType.Poison)
                        || ally.HasBuffOfType(BuffType.Polymorph)
                        || ally.HasBuffOfType(BuffType.Taunt)
                        || ally.HasBuffOfType(BuffType.Suppression)
                        || ally.HasBuffOfType(BuffType.Fear)
                        || !ally.CanMove)
                    {
                        W.Cast(ally.Position);
                    }
                }
            }
        }

        private static void logicE()
        {
            if (E.IsReady())
            {
                var target = E.GetTarget();

                var position = GameObjects.Player.Position;

                if (E.MinHitChance >= HitChance.High)
                    E.Cast(target.Position.ToVector2(), position.ToVector2());

                return;
            }

            return;
        }

        private static void logicR()
        {
            if (R.IsReady())
            {
                var target = R.GetTarget();

                if (!target.IsValidTarget())
                    return;

                if (target.HasBuffOfType(BuffType.SpellImmunity))
                    return;

                if (GameObjects.Player.CountEnemyHeroesInRange(R.Range) > 1)
                {
                    R.Cast();
                }

                if (target.HealthPercent <= 10)
                    R.Cast();

                return;
            }
        }

        private static void LaneClear()
        {
            if (E.IsReady() && Config["laneclear"].GetValue<MenuBool>("useE").Enabled)
            {
                var allMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range)).ToList();

                if (allMinions.Count == 0)
                    return;

                var farm = E.GetLineFarmLocation(allMinions, 160f);

                if (allMinions.Count > 2)
                {
                    E.Cast(farm.Position.ToVector3(), GameObjects.Player.Position);
                }
            }
            else
            {
                harass();
            }
        }


        private static void CanSpellFarm()
        {
            var positions = GameObjects.Player.Position;

            //sPELLfARM
            bool decision4;
            var text4 = "NotBinded";

            var attempt4 = Config["semiR"].GetValue<MenuKeyBind>("farm").Key;

            if (Config["semiR"].GetValue<MenuKeyBind>("farm").Active)
            {
                decision4 = true;
                text4 = "Spell Farm Key = " + attempt4 + " [ON]";
            }
            else
            {
                decision4 = false;
                text4 = "Spell Farm Key = " + attempt4 + " [OFF]";
            }
            var color4 = decision4 ? System.Drawing.Color.Red : System.Drawing.Color.Yellow;
            Drawing.DrawText(Drawing.WorldToScreen(positions - 20f), color4, text4);
        }

        private static void Jungle()
        {
            var normal = GameObjects.GetJungles(650);
            if (normal.Count > 0) { 
                

                foreach (var mob in normal)
                {
                    if (Q.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useQ").Enabled)
                        Q.Cast(mob.Position);

                    if (Config["jungleskills"].GetValue<MenuBool>("useW").Enabled && W.IsReady())
                    {
                        W.Cast(GameObjects.Player.Position);
                    }

                    if (E.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useE").Enabled)
                        E.Cast(mob.Position);
                }

            }
            else
            {
                harass();
            }
        }
    }
}

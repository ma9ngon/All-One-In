using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LyrdumAIO.Champions
{
    public class Brand
    {
        private static Spell Q, W, E, R;
        private static Menu Config;

        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "Brand")
            {
                return;
            }
            Q = new Spell(SpellSlot.Q, 1100f) { AddHitBox = true };
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 675f) { AddHitBox = true };
            R = new Spell(SpellSlot.R, 750f);

            Q.SetSkillshot(0.25f*4, 10f, float.MaxValue, false, SpellType.Line);
            W.SetSkillshot(0.75f*4, 10f, float.MaxValue, false, SpellType.Circle);

            Config = new Menu("Brand", "[LyrdumAIO]: Brand", true);

            var menuD = new Menu("dsettings", "👁 𝐃𝐑𝐀𝐖𝐈𝐍𝐆𝐒 ");
            menuD.Add(new MenuBool("drawQ", "Q Range  (RED)", true));
            menuD.Add(new MenuBool("drawE", "E Range  (BLUE)", true));
            menuD.Add(new MenuBool("drawW", "W Range (GREEN)", true));
            menuD.Add(new MenuBool("drawR", "R Range  (WHITE)", true));

            var MenuC = new Menu("infor", "👻 [𝐈𝐌𝐏𝐎𝐑𝐓𝐀𝐍𝐓] 𝐈𝐍𝐅𝐎𝐑𝐌𝐀𝐓𝐈𝐎𝐍𝐒 ", false);
            MenuC.Add(new Menu("infotool", " Harass||LastHit =  C, " + "\n LaneClear||JungleFarm = V, " + "\n Combo = SPACEBAR" + " \n Last Hit = X \n" + " Disable Drawings = L " + " \n Fore more FPS Disable EzEvade and LyrdumAIO Drawings + [Awarness] Waypoint and check Jungle->track only jungle \n If you have a bug or suggestion post it on discord server discord.gg/KfQFVhdqtz"));

            var MenuS = new Menu("harass", "🐧 𝐇𝐀𝐑𝐀𝐒𝐒 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            MenuS.Add(new MenuBool("useQ", "Use Q ", true));
            MenuS.Add(new MenuBool("useE", "Use E ", true));
            MenuS.Add(new MenuBool("useW", "Use W ", false));

            var Menuclear = new Menu("laneclear", "🐧 𝐋𝐀𝐍𝐄 𝐂𝐋𝐄𝐀𝐑 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            Menuclear.Add(new MenuBool("useQ", "Use Q ", true));
            Menuclear.Add(new MenuBool("useE", "Use E ", true));

            var MenuJungle = new Menu("jungleskills", "🐧 𝐉𝐔𝐍𝐆𝐋𝐄 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            MenuJungle.Add(new MenuBool("useQ", "Use Q ", true));
            MenuJungle.Add(new MenuBool("useE", "Use E ", true));
            MenuJungle.Add(new MenuBool("useW", "Use W ", false));

            var MenuL = new Menu("lasthit", "🐧 𝐋𝐀𝐒𝐓 𝐇𝐈𝐓 𝐒𝐊𝐈𝐋𝐋𝐒", false);
            MenuL.Add(new MenuBool("useQ", "Use Q ", true));

            var menuM = new Menu("mana", "🔪 𝐌𝐀𝐍𝐀 𝐇𝐀𝐑𝐀𝐒𝐒 ");
            menuM.Add(new MenuSlider("manaW", "W mana %", 60, 0, 100));
            menuM.Add(new MenuSlider("manaE", "E mana %", 60, 0, 100));
            menuM.Add(new MenuSlider("manaQ", "Q mana %", 60, 0, 100));

            var menuRR = new Menu("semiR", "☠ 𝐒𝐄𝐌𝐈 𝐊𝐄𝐘𝐒");
            menuRR.Add(new MenuKeyBind("farm", "Lane Clear spells", Keys.Select, KeyBindType.Toggle));

            var menuK = new Menu("skinslide", "🤖 𝐒𝐊𝐈𝐍 𝐂𝐇𝐀𝐍𝐆𝐄𝐑 ");
            menuK.Add(new MenuSliderButton("skin", "SkinID", 0, 0, 20, false));

            Config.Add(MenuC);
            Config.Add(menuD);
            Config.Add(MenuS);
            Config.Add(Menuclear);
            Config.Add(MenuJungle);

            Config.Add(MenuS);
            Config.Add(menuK);
            Config.Add(menuM);
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
                    logicE();
                    logicQ();
                    logicW();
                    break;

                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;

                case OrbwalkerMode.Harass:
                    harass();
                    break;

                case OrbwalkerMode.LastHit:
                    Jungle();
                    harass();
                    break;
            }
            semir();
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

            CanSpellFarm();
        }

        private static void harass()
        {
            var manaQ = Config["mana"].GetValue<MenuSlider>("manaQ").Value;
            var manaW = Config["mana"].GetValue<MenuSlider>("manaW").Value;
            var manaE = Config["mana"].GetValue<MenuSlider>("manaE").Value;
            var mana = GameObjects.Player.ManaPercent;

            if (mana > manaQ && Config["harass"].GetValue<MenuBool>("useQ").Enabled)
            {
                logicQ();
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

        private static void semir()
        {
            if (R.IsReady() && Config["semiR"].GetValue<MenuKeyBind>("SemiR").Active)
            {
                var target = TargetSelector.SelectedTarget;
                var target2 = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range)).OrderBy(x => x.Health).FirstOrDefault();

                if (target == null && target2 == null)
                    return;

                if (target.IsValidTarget())
                {
                    R.Cast(target);
                }

                if (!target.IsValidTarget() && target2.IsValidTarget())
                {
                    R.Cast(target2);
                }
            }
        }

        private static void logicQ()
        {
            if (Q.IsReady())
            {
                var target = Q.GetTarget(); ;
                var input = Q.GetPrediction(target);

                if (!target.IsValidTarget(Q.Range))
                    return;

                if (Q.IsInRange(input.CastPosition) && input.Hitchance >= HitChance.High)
                {
                    if (target.HasBuff("BrandAblaze"))
                    {
                        Q.Cast(input.CastPosition);   
                    }


                    if (!target.HasBuff("BrandAblaze")  && !E.IsReady()  && !W.IsReady())
                    {
                        Q.Cast(input.CastPosition);
                    }
                }
            }
        }

        private static void logicW()
        {
            if (W.IsReady())
            {
                var target = W.GetTarget();
                var input = W.GetPrediction(target, true);

                if (!target.IsValidTarget(W.Range))
                    return;

                if (input.Hitchance >= HitChance.High && W.IsInRange(input.CastPosition))
                {
                    W.Cast(input.CastPosition);
                }
            }
        }

        private static void logicE()
        {
            if (E.IsReady())
            {
                var target = E.GetTarget(); ;

                if (!target.IsValidTarget(E.Range))
                    return;

                E.Cast(target);
            }
        }

        private static void logicR()
        {
            if (R.IsReady())
            {
                var target = R.GetTarget(R.Range);

                
                if (target.CountEnemyHeroesInRange(600f) > 0 && target.InRange(R.Range))
                {
                    R.Cast(target);
                }

                if (target.HealthPercent < 20 && GameObjects.Player.CountEnemyHeroesInRange(E.Range) > 0 && target.InRange(R.Range))
                {
                    R.Cast(target);
                }
            }
        }

        private static void CanSpellFarm()
        {
            //
            bool decision;
            var text = "NotBinded";

            var attempt = Config["semiR"].GetValue<MenuKeyBind>("SemiR").Key;

            if (Config["semiR"].GetValue<MenuKeyBind>("SemiR").Active)
            {
                decision = true;
                text = "Semi-R key = " + attempt + " [ON]";
            }
            else
            {
                decision = false;
                text = "Semi-R key = " + attempt + " [OFF]";
            }
            var color = decision ? System.Drawing.Color.Lime : System.Drawing.Color.Yellow;
            Drawing.DrawText(Drawing.WorldToScreen(GameObjects.Player.Position - 15f), color, text);
        }

        private static void LaneClear()
        {
            if (Config["semiR"].GetValue<MenuKeyBind>("farm").Active)
            {
                var allMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range * 3)).Cast<AIBaseClient>().ToList();

                if (allMinions.Count == 0)
                    return;

                var bestLocation = E.GetCircularFarmLocation(allMinions, 260f);

                if (bestLocation.MinionsHit > 1
                    && W.IsReady()
                    && W.IsInRange(bestLocation.Position)
                    && Config["laneclear"].GetValue<MenuBool>("useW").Enabled)
                {
                    W.Cast(bestLocation.Position);
                    return;
                }

                if (bestLocation.MinionsHit > 0 && E.IsReady() && E.IsInRange(bestLocation.Position)
                    && Config["laneclear"].GetValue<MenuBool>("useE").Enabled)
                {
                    E.Cast(allMinions[1]);
                    return;
                }

                if (bestLocation.MinionsHit > 0 && Q.IsReady() && Q.IsInRange(bestLocation.Position)
                    && Config["laneclear"].GetValue<MenuBool>("useQ").Enabled)
                {
                    Q.Cast(bestLocation.Position);
                    return;
                }
            }
            else
            {
                harass();
            }
        }

        private static void Jungle()
        {
            var normal = GameObjects.GetJungles(650f);
            if (normal.Count == 0)
                return;

            foreach (var mob in normal)
            {
                var input = Q.GetPrediction(mob);
                if (W.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useW").Enabled)
                {
                    W.Cast(input.CastPosition);
                }

                if (Q.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useQ").Enabled)
                    Q.Cast(input.CastPosition);

                if (E.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useE").Enabled)
                    E.Cast(mob);
            }

            return;
        }
    }
}

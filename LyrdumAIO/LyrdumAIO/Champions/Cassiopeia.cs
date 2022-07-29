using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using System;
using System.Linq;

namespace LyrdumAIO.Champions
{
    public class Cassiopeia
    {
        private static Spell Q, W, E, R;
        private static Menu Config;

        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "Cassiopeia")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 850f);
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 825f);

            Q.SetSkillshot(0.25f * 4, 10f, float.MaxValue, false, SpellType.Circle);
            W.SetSkillshot(0.25f * 4, 10f, float.MaxValue, false, SpellType.Line);
            R.SetSkillshot(0.25f * 4, 10f, float.MaxValue, false, SpellType.Line);

            Config = new Menu("Cassiopeia", "🕸 [𝐋𝐲𝐫𝐝𝐮𝐦𝐀𝐈𝐎]: 𝐂𝐚𝐬𝐬𝐢𝐨𝐩𝐞𝐢𝐚 🕸", true);

            var menuD = new Menu("dsettings", "👁 𝐃𝐑𝐀𝐖𝐈𝐍𝐆𝐒 ");
            menuD.Add(new MenuBool("drawQ", "Q Range  (RED)", true));
            menuD.Add(new MenuBool("drawE", "E Range  (BLUE)", true));
            menuD.Add(new MenuBool("drawW", "W Range (GREEN)", true));
            menuD.Add(new MenuBool("drawR", "R Range  (WHITE)", true));

            var MenuC = new Menu("infor", "👻 [𝐈𝐌𝐏𝐎𝐑𝐓𝐀𝐍𝐓] 𝐈𝐍𝐅𝐎𝐑𝐌𝐀𝐓𝐈𝐎𝐍𝐒", false);
            MenuC.Add(new Menu("infotool", " Harass||LastHit =  C, " + "\n LaneClear||JungleFarm = V, " + "\n Combo = SPACEBAR" + " \n Last Hit = X \n" + " Disable Drawings = L " + " \n Fore more FPS Disable EzEvade and LyrdumAIO Drawings + [Awarness] Waypoint and check Jungle->track only jungle \n If you have a bug or suggestion post it on discord server discord.gg/KfQFVhdqtz"));

            var MenuS = new Menu("harass", "🐧 𝐇𝐀𝐑𝐀𝐒𝐒 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            MenuS.Add(new MenuBool("useQ", "Use Q ", true));
            MenuS.Add(new MenuBool("useE", "Use E ", true));
            MenuS.Add(new MenuBool("useW", "Use W ", true));

            var Menuclear = new Menu("laneclear", "🐧 𝐋𝐀𝐍𝐄 𝐂𝐋𝐄𝐀𝐑 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            Menuclear.Add(new MenuBool("useQ", "Use Q ", true));
            Menuclear.Add(new MenuBool("useE", "Use E ", true));
            Menuclear.Add(new MenuBool("useW", "Use W ", true));

            var MenuJungle = new Menu("jungleskills", "🐧 𝐉𝐔𝐍𝐆𝐋𝐄 𝐒𝐊𝐈𝐋𝐋𝐒 ", false);
            MenuJungle.Add(new MenuBool("useQ", "Use Q ", true));
            MenuJungle.Add(new MenuBool("useE", "Use E ", true));
            MenuJungle.Add(new MenuBool("useW", "Use W ", true));

            var MenuL = new Menu("lasthit", "🐧 𝐋𝐀𝐒𝐓 𝐇𝐈𝐓 𝐒𝐊𝐈𝐋𝐋𝐒", false);
            MenuL.Add(new MenuBool("useQ", "Use Q ", true));
            MenuL.Add(new MenuBool("useE", "Use E ", true));
            MenuL.Add(new MenuBool("useW", "Use W ", true));

            var menuM = new Menu("mana", "🔪 𝐌𝐀𝐍𝐀 𝐇𝐀𝐑𝐀𝐒𝐒 ");
            menuM.Add(new MenuSlider("manaW", "W mana %", 60, 0, 100));
            menuM.Add(new MenuSlider("manaE", "E mana %", 60, 0, 100));
            menuM.Add(new MenuSlider("manaQ", "Q mana %", 60, 0, 100));

            var menuRR = new Menu("semiR", "☠ 𝐒𝐄𝐌𝐈 𝐊𝐄𝐘𝐒");
            menuRR.Add(new MenuKeyBind("SemiR", "Semi-R", Keys.Select, KeyBindType.Press));
            menuRR.Add(new MenuKeyBind("clear", "Lane Clear with R", Keys.Select, KeyBindType.Press));
            menuRR.Add(new MenuKeyBind("farm", "Lane Clear spells", Keys.Select, KeyBindType.Toggle));

            var menuK = new Menu("skinslide", "🤖 𝐒𝐊𝐈𝐍 𝐂𝐇𝐀𝐍𝐆𝐄𝐑 ");
            menuK.Add(new MenuSliderButton("skin", "SkinID", 0, 0, 30, false));

            Config.Add(MenuC);
            Config.Add(menuD);
            Config.Add(MenuS);
            Config.Add(Menuclear);
            Config.Add(MenuJungle);
            Config.Add(MenuL);
            Config.Add(menuM);
            Config.Add(menuRR);
            Config.Add(menuK);

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
                    logicE2();
                    logicW();
                    break;

                case OrbwalkerMode.LaneClear:
                    Jungle();
                    LaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    LastHitt();
                    break;

                case OrbwalkerMode.Harass:
                    LastHitt();
                    break;
            }

            if (GameObjects.Player.Level > 1)
            {
                if (GameObjects.Player.ManaPercent > 10 
                    && !GameObjects.Player.IsUnderEnemyTurret() && enemyobj() == 0 
                    && Orbwalker.ActiveMode == OrbwalkerMode.Combo) 
                {
                    Orbwalker.AttackEnabled = false;
                }
                else
                {
                    Orbwalker.AttackEnabled = true;
                }
            }

            logicR();
            skinch();
            semir();
            Rclear();
        }

        public static int enemyobj()
        {
            var target = Q.GetTarget();
            bool logic = GameObjects.Player.InAutoAttackRange(target);
            int logicf;

            if (logic && !target.HasBuffOfType(BuffType.Poison))
            {
                logicf = 1;
            }
            else
            {
                logicf = 0;
            }

            var inhibs = GameObjects.EnemyInhibitors
                .Where(x => x.IsValidTarget(650f))
                .ToList();

            var nex = Q.IsInRange(GameObjects.EnemyNexus);

            int nexint;

            if (nex == false)
            {
                nexint = 0;
            }
            else
            {
                nexint = 1;
            }

            return inhibs.Count + nexint + logicf;
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

            if (Config["dsettings"].GetValue<MenuBool>("drawR").Enabled)
            {
                Render.Circle.DrawCircle(position, R.Range, System.Drawing.Color.White);
            }

            CanSpellFarm();
        }

        private static void CanSpellFarm()
        {
            var position = GameObjects.Player.Position;

            // Semi R
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
            var color = decision ? System.Drawing.Color.Red : System.Drawing.Color.Yellow;
            Drawing.DrawText(Drawing.WorldToScreen(position - 20F), color, text);

            //R-Farm
            bool decision3;
            var text3 = "NotBinded";

            var attempt3 = Config["semiR"].GetValue<MenuKeyBind>("clear").Key;

            if (Config["semiR"].GetValue<MenuKeyBind>("clear").Active)
            {
                decision3 = true;
                text3 = "R-LaneClear key " + attempt3 + " [ON]";
            }
            else
            {
                decision3 = false;
                text3 = "R-LaneClear key = " + attempt3 + " [OFF]";
            }
            var color3 = decision3 ? System.Drawing.Color.Red : System.Drawing.Color.Yellow;
            Drawing.DrawText(Drawing.WorldToScreen(position - 40f), color3, text3);

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
            Drawing.DrawText(Drawing.WorldToScreen(position - 60f), color4, text4);
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


        private static void logicQ()
        {
            if (Q.IsReady())
            {
                var target = Q.GetTarget();
                var input = Q.GetPrediction(target, true);

                if (!target.IsValidTarget())
                    return;

                if (input.Hitchance >= HitChance.High
                    && !target.HasBuffOfType(BuffType.Poison)
                    && Q.IsInRange(input.CastPosition)) 
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

                var input = W.GetPrediction(target, true);

                if (!target.IsValidTarget())
                    return;

                if (input.Hitchance >= HitChance.Medium && W.IsInRange(input.CastPosition))
                {
                    W.Cast(input.CastPosition);
                }
            }
        }



        private static void logicE2()
        {
            if (E.IsReady())
            {
                var target = E.GetTarget();
                E.Cast(target);
                
            }
        }

        private static void logicE()
        {
            if (E.IsReady())
            {
                var target = E.GetTarget();

                if (target.HasBuffOfType(BuffType.Poison))
                    E.Cast(target);

            }
        }


        private static void logicR()
        {
            if (R.IsReady() && !Config["semiR"].GetValue<MenuKeyBind>("SemiR").Active)
            {
                var target = R.GetTarget();
                var input = R.GetPrediction(target, true);

                if (!target.IsValidTarget())
                    return;

                if (target.HasBuffOfType(BuffType.SpellShield))
                {
                    return;
                }

                if (target.IsBothFacing(GameObjects.Player)
                    && GameObjects.Player.CountEnemyHeroesInRange(400f) > 1
                    && R.IsInRange(input.CastPosition))
                {
                    R.Cast(input.CastPosition);
                }
            }
        }


        private static void semir()
        {
            if (R.IsReady() && Config["semiR"].GetValue<MenuKeyBind>("SemiR").Active)
            {
                var target = TargetSelector.SelectedTarget;
                var input = R.GetPrediction(target);

                var target2 = GameObjects.EnemyHeroes
                    .Where(x => x.IsValidTarget(R.Range))
                    .OrderBy(x => x.Health)
                    .ToList();

                var input2 = R.GetPrediction(target2[0]);

                if (target == null && target2 == null)
                    return;

                if (target.IsValidTarget()
                    && R.IsInRange(input.CastPosition))
                {
                    R.Cast(input.CastPosition);
                }

                if (input2.Hitchance >= HitChance.High
                    && !target.IsValidTarget()
                    && target2[0].IsValidTarget()
                    && R.IsInRange(input.CastPosition))
                {
                    R.Cast(input2.CastPosition);
                }
            }
        }

        private static void Rclear()
        {
            if (R.IsReady() && Config["semiR"].GetValue<MenuKeyBind>("clear").Active)
            {
                var allMinions = GameObjects.Minions.Where(x => x.IsValidTarget(R.Range)).ToList();

                if (allMinions.Count == 0)
                    return;

                var circ = R.GetLineFarmLocation(allMinions, R.Width * 5);

                if (circ.MinionsHit >= 3)
                {
                    R.Cast(circ.Position);
                }
            }
        }

        private static void LaneClear()
        {
            if (Config["semiR"].GetValue<MenuKeyBind>("farm").Active)
            {
                var allMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range * 2)).ToList();

                if (allMinions.Count == 0)
                    return;

                if (Q.IsReady() && Config["laneclear"].GetValue<MenuBool>("useQ").Enabled)
                {
                    var farm = Q.GetCircularFarmLocation(allMinions, 120f);

                    if (Q.IsInRange(farm.Position) && farm.MinionsHit > 2)
                        Q.Cast(farm.Position);
                    else
                        Q.Cast(farm.Position);
                }

                if (W.IsReady() && Config["laneclear"].GetValue<MenuBool>("useW").Enabled)
                {
                    var farm1 = W.GetCircularFarmLocation(allMinions, 200f);

                    if (W.IsInRange(farm1.Position))
                        W.Cast(farm1.Position);
                }

                if (E.IsReady() && Config["laneclear"].GetValue<MenuBool>("useE").Enabled)
                {
                    foreach (var mob in allMinions)
                    {
                        if (mob.Health <= E.GetDamage(mob))
                        {
                            E.Cast(mob);
                        }
                        else if (mob.HasBuffOfType(BuffType.Poison))
                        {
                            E.Cast(mob);
                        }
                    }
                }
            }else
            {
                harass();
            }
        }

        private static void LastHitt()
        {
            if (Config["semiR"].GetValue<MenuKeyBind>("farm").Active)
            {
                var MinionList = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range)).ToList();

                if (MinionList.Count == 0)
                    return;

                foreach (var minion in MinionList)
                {
                    var predHealth = E.GetHealthPrediction(minion);
                    var damage = E.GetDamage(minion);
                    var killable = predHealth <= damage;

                    if (minion.Health <= 45)
                    {
                        if (Config["lasthit"].GetValue<MenuBool>("useE").Enabled && E.IsReady())
                            E.Cast(minion);

                    }
                    else if (predHealth <= 45)
                    {
                        if (Config["lasthit"].GetValue<MenuBool>("useE").Enabled && E.IsReady())
                            E.Cast(minion);

                    }
                    else if (killable)
                    {
                        if (Config["lasthit"].GetValue<MenuBool>("useE").Enabled && E.IsReady())
                            E.Cast(minion);
                    }
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
                if (Q.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useQ").Enabled && Q.IsReady())
                {
                    var input = Q.GetPrediction(mob);
                    Q.Cast(input.CastPosition);
                }

                if (W.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useW").Enabled && W.IsReady())
                    W.Cast(mob.Position);

                if (E.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useE").Enabled && E.IsReady())
                    E.Cast(mob);
            }

            return;
        }
    }
}

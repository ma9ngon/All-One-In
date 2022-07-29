using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using System;
using System.Linq;

namespace LyrdumAIO.Champions
{
    public class Karthus
    {
        public static Spell Q, W, E, R;
        private static Menu Config;

        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "Karthus")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 875f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 550f);
            R = new Spell(SpellSlot.R, float.MaxValue);

            Q.SetSkillshot(0.95f, 10f, float.MaxValue, false, SpellType.Circle);
            W.SetSkillshot(0.75f, 10f, float.MaxValue, false, SpellType.Circle);

            Config = new Menu("Lux", "🕸 [𝐋𝐲𝐫𝐝𝐮𝐦𝐀𝐈𝐎]: 𝐊𝐚𝐫𝐭𝐡𝐮𝐬 🕸", true);

            var menuD = new Menu("dsettings", "👁 𝐃𝐑𝐀𝐖𝐈𝐍𝐆𝐒 ");
            menuD.Add(new MenuBool("drawQ", "Q Range  (RED)", true));
            menuD.Add(new MenuBool("drawE", "E Range  (BLUE)", true));
            menuD.Add(new MenuBool("drawW", "W Range (GREEN)", true));
            menuD.Add(new MenuBool("drawR", "R Range  (WHITE)", true));

            var MenuC = new Menu("infor", "👻 [𝐈𝐌𝐏𝐎𝐑𝐓𝐀𝐍𝐓] 𝐈𝐍𝐅𝐎𝐑𝐌𝐀𝐓𝐈𝐎𝐍𝐒 ", false);
            MenuC.Add(new Menu("infotool",
                  " ʜᴀʀᴀꜱꜱ = ᴄ "
                + "\n ʟᴀɴᴇᴄʟᴇᴀʀ = ᴠ, "
                + "\n ᴄᴏᴍʙᴏ = ꜱᴘᴀᴄᴇʙᴀʀ"
                + "\n JUNGLE FARM = 𝙭"
                + "\n ᴅɪꜱᴀʙʟᴇ ᴅʀᴀᴡɪɴɢꜱ = ʟ "
                + "\n ᴀᴜᴛᴏ ʀ ᴏɴ ʟᴏᴡ ᴛᴀʀɢᴇᴛ = ᴀᴜᴛᴏᴍᴀᴛɪᴄ"));

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


            var menuK = new Menu("skinslide", "🤖 𝐒𝐊𝐈𝐍 𝐂𝐇𝐀𝐍𝐆𝐄𝐑 ");
            menuK.Add(new MenuSliderButton("skin", "SkinID", 0, 0, 20, false));

            var menuRR = new Menu("semiR", "☠ 𝐒𝐄𝐌𝐈 𝐊𝐄𝐘𝐒");
            menuRR.Add(new MenuKeyBind("farm", "Lane Clear spells", Keys.Select, KeyBindType.Toggle));

            Config.Add(MenuC);
            Config.Add(menuD);
            Config.Add(MenuS);
            Config.Add(MenuJungle);
            Config.Add(Menuclear);
            Config.Add(MenuL);
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
                    logicE();
                    logicW();
                    break;

                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    Jungle();
                    LastHit();
                    break;

                case OrbwalkerMode.Harass:
                    LastHit();
                    break;
            }

            if (GameObjects.Player.Level > 1)
            {
                if (GameObjects.Player.ManaPercent > 10 && !GameObjects.Player.IsUnderEnemyTurret() && enemyobj() == 0
                    && Orbwalker.ActiveMode == OrbwalkerMode.Combo
                    || Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
                {
                    Orbwalker.AttackEnabled = false;
                }
                else
                {
                    Orbwalker.AttackEnabled = true;
                }
            }



            skinch();
            logicR();
            loigcee();
        }

        public static void loigcee()
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.None && E.ToggleState == SpellToggleState.On)
            {
                E.Cast();
            } 
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

        private static void logicQ()
        {
            if (Q.IsReady())
            {
                var target = Q.GetTarget();
                var input = Q.GetPrediction(target);

                if (input.Hitchance >= HitChance.High && Q.IsInRange(input.CastPosition))
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
                var input = W.GetPrediction(target);

                if (W.IsInRange(input.CastPosition) && input.Hitchance >= HitChance.Medium)
                    W.Cast(input.CastPosition);
            }
        }

        private static void logicE()
        {
            var target = E.GetTarget();
            if (!target.IsValidTarget() && E.ToggleState == SpellToggleState.On)
                E.Cast();

            if (!target.IsValidTarget(E.Range))
                return;

            if (target.InAutoAttackRange(E.Range) && E.ToggleState == SpellToggleState.Off)
            {
                E.Cast();
            }

            if (GameObjects.Player.CountEnemyHeroesInRange(E.Range) == 0 && E.ToggleState == SpellToggleState.On)
            {
                E.Cast();
            }
        }

        private static void logicR()
        {
            if (R.IsReady())
            {
                var target = R.GetTarget();

                if (!target.IsValidTarget())
                    return;

                var dmg = R.GetDamage(target);
                var dmg1 = target.Health;

                if (target.HasBuffOfType(BuffType.SpellImmunity))
                {
                    return;
                }

                if (!W.IsInRange(target, W.Range + 200f)
                    && dmg >= dmg1
                    && GameObjects.Player.CountEnemyHeroesInRange(W.Range) == 0
                    && !GameObjects.Player.IsUnderEnemyTurret()
                    && target.CountAllyHeroesInRange(W.Range) == 0)
                {
                    R.Cast();
                }

                if (dmg >= dmg1
                    && GameObjects.Player.HasBuff("KarthusDeathDefiedBuff")
                    && target.CountAllyHeroesInRange(W.Range) == 0)
                {
                    R.Cast();
                }
            }
        }

        private static void LaneClear()
        {
            if (Config["semiR"].GetValue<MenuKeyBind>("farm").Active)
            {
                var farm = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)).ToList();

                if (farm.Count == 0)
                {
                    if (E.ToggleState == SpellToggleState.On)
                        E.Cast();

                    return;
                }

                var location = Q.GetCircularFarmLocation(farm, 150f);

                if (Config["laneclear"].GetValue<MenuBool>("useQ").Enabled)
                {
                    if (location.MinionsHit > 2)
                    {
                        Q.Cast(location.Position);
                    }
                    else
                    {
                        if (farm[0].InRange(Q.Range))
                        {
                            Q.Cast(farm[0].Position);
                        }
                    }
                }

                if (E.IsReady()
                    && E.ToggleState == SpellToggleState.Off && Config["laneclear"].GetValue<MenuBool>("useE").Enabled)
                {
                    var farm1 = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range)).Take(1).ToList();
                    if (farm1.Count > 0)
                        E.Cast();
                }
            }
            else
            {
                harass();
            }
        }

        private static void CanSpellFarm()
        {
            var position = GameObjects.Player.Position;

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
            Drawing.DrawText(Drawing.WorldToScreen(position - 20f), color4, text4);
        }

        private static void LastHit()
        {
            if (Config["semiR"].GetValue<MenuKeyBind>("farm").Active)
            {
                var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValidTarget(Q.Range) &&
                    Q.GetHealthPrediction(x) < Q.GetDamage(x) && Q.GetHealthPrediction(x) > 0);

                if (minion != null && Variables.GameTimeTickCount - Q.LastCastAttemptTime > 557)
                {
                    var input = Q.GetPrediction(minion);

                    if (input.Hitchance >= HitChance.High && Config["lasthit"].GetValue<MenuBool>("useQ").Enabled)
                        Q.Cast(input.CastPosition);
                }
            }
            else
            {
                harass();
            }
        }

        private static void Jungle()
        {
            var final = GameObjects.Jungle.Where(x => x.IsValidTarget(650f)).Cast<AIBaseClient>().ToHashSet();
            var target = Q.GetTarget(Q.Range);

            if (E.ToggleState == SpellToggleState.On && final.Count == 0 && target == null)
            {
                E.Cast();
            }

            if (final.Count <= 0)
            {
                return;
            }

            var farmloc = Q.GetCircularFarmLocation(final, 150f);

            if (farmloc.MinionsHit > 2 && Config["jungleskills"].GetValue<MenuBool>("useQ").Enabled)
            {
                Q.Cast(farmloc.Position);
            }

            if (W.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useW").Enabled)
                W.Cast(farmloc.Position);

            foreach (var mob in final)
            {
                if (Q.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useQ").Enabled)
                {
                    var input = Q.GetPrediction(mob);
                    Q.Cast(input.CastPosition);
                }
                if (E.ToggleState == SpellToggleState.Off && mob.InRange(E.Range) && Config["jungleskills"].GetValue<MenuBool>("useE").Enabled)
                {
                    E.Cast();
                }
            }

            return;
        }
    }
}

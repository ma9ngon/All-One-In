using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LyrdumAIO.Champions
{
    public class Lux
    {
        private static Spell Q, W, E, R, Q2, E2;
        private static Menu Config;

        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "Lux")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1300f) { AddHitBox = true };
            Q2 = new Spell(SpellSlot.Q, 1300f) { AddHitBox = true };
            W = new Spell(SpellSlot.W, 1075f);
            E = new Spell(SpellSlot.E, 1100f) { AddHitBox = true };
            E2 = new Spell(SpellSlot.E, 1100f) { AddHitBox = true };
            R = new Spell(SpellSlot.R, 3100f);

            Q.SetSkillshot(0.25f * 4, 10f, float.MaxValue, false, SpellType.Line);
            W.SetSkillshot(0.25f * 4, 10f, float.MaxValue, false, SpellType.Line);
            E.SetSkillshot(0.25f * 4, 110f / 4, float.MaxValue, false, SpellType.Circle);
            R.SetSkillshot(0.25f * 4, 10f, float.MaxValue, false, SpellType.Line);

            E2.SetSkillshot(0.25f * 4, 220f, float.MaxValue, false, SpellType.Circle);
            Q2.SetSkillshot(0.25f * 4, 140f, float.MaxValue, false, SpellType.Line);

            
            
            Config = new Menu("Lux", "🕸 [𝐋𝐲𝐫𝐝𝐮𝐦𝐀𝐈𝐎]: 𝐋𝐮𝐱 🕸", true);


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
                + "\n ʟᴀꜱᴛ ʜɪᴛ = 𝙭"
                + "\n ᴅɪꜱᴀʙʟᴇ ᴅʀᴀᴡɪɴɢꜱ = ʟ "
                + "\n ᴅʀᴀᴋᴇ/ʙᴀʀᴏɴ/ʀɪꜰᴛ ꜱᴛᴇᴀʟ/ᴀᴜᴛᴏ ʀ ᴏɴ ʟᴏᴡ ᴛᴀʀɢᴇᴛ = ᴀᴜᴛᴏᴍᴀᴛɪᴄ"));

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

            var menuJ = new Menu("jungleset", "💥 𝐁𝐀𝐑𝐎𝐍/𝐃𝐑𝐀𝐆𝐎𝐍/𝐑𝐈𝐅𝐓 𝐒𝐓𝐄𝐀𝐋 ");
            menuJ.Add(new MenuBool("dragonR", "Baron/Dragon steal", true));

            var menuK = new Menu("skinslide", "🤖 𝐒𝐊𝐈𝐍 𝐂𝐇𝐀𝐍𝐆𝐄𝐑 ");
            menuK.Add(new MenuSliderButton("skin", "SkinID", 0, 0, 20, false));

            var menuRR = new Menu("semiR", "☠ 𝐒𝐄𝐌𝐈 𝐊𝐄𝐘𝐒");
            menuRR.Add(new MenuKeyBind("SemiR", "Semi-R", Keys.Select, KeyBindType.Press));
            menuRR.Add(new MenuKeyBind("buffs", "Buff Steal", Keys.Select, KeyBindType.Press));
            menuRR.Add(new MenuKeyBind("clear", "Lane Clear with R", Keys.Select, KeyBindType.Press));
            menuRR.Add(new MenuKeyBind("farm", "Lane Clear spells", Keys.Select, KeyBindType.Toggle));

            Config.Add(MenuC);
            Config.Add(menuD);
            Config.Add(MenuS);
            Config.Add(MenuJungle);
            Config.Add(Menuclear);
            Config.Add(MenuL);
            Config.Add(menuJ);
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
                    LogicE();
                    LogicQ();
                    LogicW();
                    break;

                case OrbwalkerMode.LaneClear:
                    Jungle();
                    LaneClear();
                    break;

                case OrbwalkerMode.LastHit:
                    LastHit();
                    harass();

                    break;

                case OrbwalkerMode.Harass:
                    harass();
                    LastHit();
                    break;
            }

            semir();
            Rclear();
            buffs_steal();
            skinch();
            LogicR();
            steal();
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

            // Blue Buff /Redbuff steal
            bool decision2;
            var text2 = "NotBinded";

            var attempt2 = Config["semiR"].GetValue<MenuKeyBind>("buffs").Key;

            if (Config["semiR"].GetValue<MenuKeyBind>("buffs").Active)
            {
                decision2 = true;
                text2 = "Buff steal key = " + attempt2 + " [ON]";
            }
            else
            {
                decision2 = false;
                text2 = "Buff steal key = " + attempt2 + " [OFF]";
            }
            var color2 = decision2 ? System.Drawing.Color.Red : System.Drawing.Color.Yellow;
            Drawing.DrawText(Drawing.WorldToScreen(position - 40f), color2, text2);

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
            Drawing.DrawText(Drawing.WorldToScreen(position - 60f), color3, text3);

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
            Drawing.DrawText(Drawing.WorldToScreen(position - 80f), color4, text4);
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
                    && input.Hitchance >= HitChance.High)
                {
                    R.Cast(input.CastPosition);
                }

                if (input2.Hitchance >= HitChance.High
                    && !target.IsValidTarget()
                    && target2[0].IsValidTarget())
                {
                    R.Cast(input2.CastPosition);
                }
            }
        }

        public static void buffs_steal()
        {
            if (Config["semiR"].GetValue<MenuKeyBind>("buffs").Active)
            {
                var normals = GameObjects.Jungle
                    .Where(x => x.IsValidTarget(R.Range)
                        && x.Name == "SRU_Red4.1.1" || x.Name == "SRU_Blue1.1.1" || x.Name == "SRU_Blue7.1.1" || x.Name == "SRU_Red10.1.1")
                    .ToList();

                if (normals.Count == 0)
                    return;

                foreach (var normal in normals)
                {
                    if (normal.Health <= R.GetDamage(normal) && normal.CountEnemyHeroesInRange(800f) > 0 && normal.CountAllyHeroesInRange(800f) == 0)
                    {
                        R.Cast(normal.Position);
                    }
                }
            }
        }

        private static void Jungle()
        {
            var normal = GameObjects.GetJungles(650f).Cast<AIBaseClient>().ToList();
            var test = Q.GetCircularFarmLocation(normal, 75f);

            if (normal.Count == 0)
                return;

            if (normal.Count > 0)
            {
                foreach (var mob in normal)
                {
                    if (Config["jungleskills"].GetValue<MenuBool>("useQ").Enabled) { 
                        var input = Q.GetPrediction(mob);
                        Q.Cast(input.CastPosition);
                    }

                    if (W.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useW").Enabled)
                    {
                        W.Cast(mob.Position);
                    }

                    if (E.IsReady() && Config["jungleskills"].GetValue<MenuBool>("useE").Enabled)
                    {
                        E.Cast(test.Position);
                    }
                }
            }
        }

        private static void steal()
        {
            if (Config["jungleset"].GetValue<MenuBool>("dragonR").Enabled && R.IsReady())
            {
                var normal = GameObjects.Jungle.Where(x => x.IsValidTarget(R.Range)
                        && (x.Name == "SRU_RiftHerald17.1.1"
                        || x.Name == "SRU_Dragon_Fire6.2.1"
                        || x.Name == "SRU_Dragon_Air6.1.1"
                        || x.Name == "SRU_Dragon_Earth6.4.1"
                        || x.Name == "SRU_Dragon_Water6.3.1"
                        || x.Name == "SRU_Dragon_Elder6.5.1"
                        || x.Name == "SRU_Baron12.1.1"))
                    .FirstOrDefault();

                if (normal == null)
                    return;

                if (normal.Health <= R.GetDamage(normal) && normal.CountEnemyHeroesInRange(800f) > 0)
                {
                    R.Cast(normal.Position);
                }
            }
        }

        private static void LogicR()
        {
            if (R.IsReady() && !Config["semiR"].GetValue<MenuKeyBind>("SemiR").Active)
            {
                var target = R.GetTarget();;
                var input = R.GetPrediction(target);

                if (!target.IsValidTarget() || !target.IsValidTarget() || target.Health > R.GetDamage(target))
                    return;

                if (input.Hitchance >= HitChance.High
                    && GameObjects.Player.CountEnemyHeroesInRange(650f) == 0
                    && target.CountAllyHeroesInRange(650f) == 0)
                {
                    R.Cast(input.CastPosition);
                }
            }
        }

        private static void LogicW()
        {
            if (W.IsReady())
            {
                var target = W.GetTarget();;
                var ally = GameObjects.AllyHeroes.Where(x => x.IsValidTarget(W.Range, checkTeam: false) && !x.IsMe && !x.IsDead).OrderBy(x => x.Health).ToList();
                var position = GameObjects.Player.Position;
                if (ally.Count > 0)
                {
                    var input = W.GetPrediction(ally[0]);

                    if (!target.CanMove || target.HasBuffOfType(BuffType.Stun))
                    {
                        W.Cast(input.CastPosition);
                    }

                    var enemys = ally[0].CountEnemyHeroesInRange(ally[0].AttackRange);

                    if (enemys == 1 && ally[0].Health <= target.Health)
                    {
                        W.Cast(input.CastPosition);
                    }

                    if (enemys > 1 && ally[0].HealthPercent <= 30)
                    {
                        W.Cast(input.CastPosition);
                    }

                    if (ally[0].HasBuffOfType(BuffType.Stun)
                        || ally[0].HasBuffOfType(BuffType.Slow)
                        || ally[0].HasBuffOfType(BuffType.Silence)
                        || ally[0].HasBuffOfType(BuffType.Asleep)
                        || ally[0].HasBuffOfType(BuffType.Blind)
                        || ally[0].HasBuffOfType(BuffType.Charm)
                        || ally[0].HasBuffOfType(BuffType.Disarm)
                        || ally[0].HasBuffOfType(BuffType.Knockup)
                        || ally[0].HasBuffOfType(BuffType.Poison)
                        || ally[0].HasBuffOfType(BuffType.Polymorph)
                        || ally[0].HasBuffOfType(BuffType.Taunt)
                        || ally[0].HasBuffOfType(BuffType.Suppression)
                        || ally[0].HasBuffOfType(BuffType.Fear)
                        || !ally[0].CanMove)
                    {
                        W.Cast(input.CastPosition);
                    }

                    if (GameObjects.Player.HealthPercent < 10 && GameObjects.Player.CountEnemyHeroesInRange(650f) > 0 && ally[0].HealthPercent > 50)
                    {
                        W.Cast(position);
                    }
                }
                else if (ally.Count == 0)
                {
                    if (!target.CanMove || target.HasBuffOfType(BuffType.Stun) && GameObjects.Player.CountEnemyHeroesInRange(650f) > 0)
                    {
                        W.Cast(target.Position);
                    }
                }
            }
        }

        private static void LogicQ()
        {
            var target = Q.GetTarget();;

            if (!target.IsValidTarget())
                return;

            var input = Q.GetPrediction(target, false);

            var col = Q2.GetCollision(GameObjects.Player.Position.ToVector2(), new List<Vector2> { input.CastPosition.ToVector2() }).Take(2).ToList();

            if (col.Count < 2 && input.Hitchance >= HitChance.High && Q.IsInRange(input.CastPosition))
            {
                Q.Cast(input.CastPosition);
            }

            return;
        }

        private static void LogicE()
        {
            if (E.IsReady())
            {
                var target = E.GetTarget();

                if (!target.IsValidTarget())
                    return;

                var input = E2.GetPrediction(target, true);

                if (input.Hitchance >= HitChance.High && E.IsInRange(input.CastPosition))
                {
                    E.Cast(input.CastPosition);
                }
            }
        }

        private static void LaneClear()
        {
            if (Config["semiR"].GetValue<MenuKeyBind>("farm").Active)
            {
                var allMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range * 3)).ToList();

                if (allMinions.Count == 0)
                    return;

                var bestLocation = E.GetCircularFarmLocation(allMinions, 360f);

                if (bestLocation.MinionsHit > 2
                    && E.IsInRange(bestLocation.Position)
                    && E.IsReady() 
                    && Config["laneclear"].GetValue<MenuBool>("useE").Enabled)
                {
                    E.Cast(bestLocation.Position);
                }

                if (Q.IsReady()
                    && Q.IsInRange(bestLocation.Position)
                    && bestLocation.MinionsHit < 3 
                    && bestLocation.MinionsHit > 0
                    && Config["laneclear"].GetValue<MenuBool>("useQ").Enabled)
                {
                    Q.Cast(bestLocation.Position);
                }

                if (W.IsReady() && Config["laneclear"].GetValue<MenuBool>("useW").Enabled)
                {
                    var allyminions = GameObjects.AllyMinions.Where(x => x.InRange(W.Range)).Take(1).ToList();
                    if (allyminions.Count == 0 && allMinions.Count > 1)
                    {
                        W.Cast(GameObjects.Player.Position);
                    }
                }
            }
            else
            {
                harass();
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

        private static void LastHit()
        {
            var MinionList = GameObjects.Minions.Where(x => x.IsValidTarget(Q.Range * 2)
                        && x.Health <= 140f
                        && !x.InRange(GameObjects.Player.AttackRange)
                        )
                     .OrderBy(x => x.Health).Cast<AIBaseClient>().ToList();

            if (MinionList.Count == 0)
                return;

            if (Config["lasthit"].GetValue<MenuBool>("useQ").Enabled && Q.IsReady())
            {
                foreach (var minion in MinionList)
                { 
                    var predHealth = Q.GetHealthPrediction(minion);

                    var damage = 145;
                    var killable = predHealth <= damage;

                    var col1 = Q2.GetCollision(GameObjects.Player.Position.ToVector2(), new List<Vector2> { minion.Position.ToVector2() });

                    if (col1.Count < 2)
                    {
                        if (col1[0].Health < 100 && col1.Count == 1)
                        {
                            if (minion.Health < 100)
                            {
                                Q.Cast(minion.Position);
                            }
                            else if (killable)
                            {
                                Q.Cast(minion.Position);
                            }
                            else if (predHealth < 100)
                            {
                                Q.Cast(minion.Position);
                            }
                        }
                        else if (col1.Count == 0)
                        {
                            if (minion.Health < 100)
                            {
                                Q.Cast(minion.Position);
                            }
                            else if (killable) {   
                             Q.Cast(minion.Position);
                            }
                            else if (predHealth < 100)
                            {
                            Q.Cast(minion.Position);
                            }
                        }
                    }
                }
            }

            if (Config["lasthit"].GetValue<MenuBool>("useE").Enabled && E.IsReady())
            {
                var bestLocation = E.GetCircularFarmLocation(MinionList, 360);

                if (bestLocation.MinionsHit > 1 && E.IsInRange(bestLocation.Position))
                {
                    E.Cast(bestLocation.Position);
                }
            }

            if (W.IsReady() && Config["lasthit"].GetValue<MenuBool>("useW").Enabled)
            {
                var allyminions = GameObjects.AllyMinions.Where(x => x.InRange(W.Range)).Take(1).FirstOrDefault();
                if (allyminions == null && MinionList.Count > 2)
                {
                    W.Cast(GameObjects.Player.Position);
                }
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
                LogicQ();
            }

            if (mana > manaW && Config["harass"].GetValue<MenuBool>("useW").Enabled)
            {
                LogicW();
            }

            if (mana > manaE && Config["harass"].GetValue<MenuBool>("useE").Enabled)
            {
                LogicE();
            }
        }
    }
}

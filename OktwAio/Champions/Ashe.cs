using System;
using System.Collections.Generic;
using System.Linq;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using SebbyLib;
    using SharpDX;

    class Ashe : Base
    {
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool rNoti = new MenuBool("rNoti", "R key info");

        private readonly MenuBool autoQ = new MenuBool("autoQ", "Auto Q");
        private readonly MenuBool harassQ = new MenuBool("harassQ", "Harass Q");

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W");
        private readonly MenuBool harassW = new MenuBool("harassW", "Harass W");
        private readonly MenuBool ksW = new MenuBool("ksW", "Auto KS W");
        private readonly MenuBool ccW = new MenuBool("ccW", "W immobile target");

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R");
        private readonly MenuBool ksComboR = new MenuBool("ksComboR", "R KS combo R + W + AA");
        private readonly MenuBool autoRaoe = new MenuBool("autoRaoe", "Auto R aoe");
        private readonly MenuBool autoRinter = new MenuBool("autoRinter", "Auto R OnPossibleToInterrupt");
        private readonly MenuKeyBind useR2 = new MenuKeyBind("useR2", "R key target cast", Keys.Y, KeyBindType.Press);
        private readonly MenuKeyBind useR = new MenuKeyBind("useR", "Semi-manual cast R key", Keys.T, KeyBindType.Press);
        private readonly MenuList useRmode = new MenuList("useRmode", "Semi-manual MODE", new[] { "LOW HP", "CLOSEST" });
        private readonly List<MenuBool> gapcloserList = new List<MenuBool>();

        private readonly MenuBool farmQ = new MenuBool("farmQ", "Lane clear Q");
        private readonly MenuBool farmW = new MenuBool("farmW", "Lane clear W");
        private readonly MenuBool jungleQ = new MenuBool("jungleQ", "Jungle clear Q");
        private readonly MenuBool jungleW = new MenuBool("jungleW", "Jungle clear W");

        private AIBaseClient targetR = null;

        public Ashe()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1100f);
            R = new Spell(SpellSlot.R, 2500f);

            W.SetSkillshot(0.25f, 20f, 2000f, true, SpellType.Line);
            R.SetSkillshot(0.25f, 130f, 1600f, true, SpellType.Line);

            Local.Add(new Menu("draw", "Draw")
            {
                onlyRdy,
                wRange,
                rNoti
            });

            Local.Add(new Menu("qConfig", "Q Config")
            {
                autoQ,
                harassQ
            });

            Local.Add(new Menu("wConfig", "W Config")
            {
                autoW,
                harassW,
                ksW,
                ccW
            });

            var gap = new Menu("gap", "GapCloser R");

            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                var item = new MenuBool("gapcloser" + enemy.CharacterName, enemy.CharacterName, false);

                gap.Add(item);
                gapcloserList.Add(item);
            }

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                ksComboR,
                autoRaoe,
                autoRinter,
                useR2,
                useR,
                useRmode,
                gap
            });

            FarmMenu.Add(farmQ);
            FarmMenu.Add(farmW);
            FarmMenu.Add(jungleQ);
            FarmMenu.Add(jungleW);

            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Interrupter.OnInterrupterSpell += Interrupter_OnInterrupterSpell;
            Orbwalker.OnAfterAttack += Orbwalker_OnAfterAttack;
        }

        private void AntiGapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!OktwCommon.CheckGapcloser(sender, args))
            {
                return;
            }

            if (R.IsReady())
            {
                if (sender.IsValidTarget(800) && gapcloserList.Any(e => e.Enabled && e.Name == "gapcloser" + sender.CharacterName))
                {
                    R.Cast(sender.PreviousPosition);
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (rNoti.Enabled)
            {
                if (targetR != null)
                {
                    drawText("R KEY TARGET: " + targetR.CharacterName, Player.Position, System.Drawing.Color.YellowGreen, 150);
                }
                else
                {
                    drawText("PLS CLICK LEFT ON R TARGET", Player.Position, System.Drawing.Color.YellowGreen, 150);
                }
            }

            if (wRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (W.IsReady())
                    {
                        Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1);
                    }
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1);
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (R.IsReady())
            {
                if (useR.Active)
                {
                    switch (useRmode.Index)
                    {
                        case 0:
                            {
                                var t = TargetSelector.GetTarget(1800, DamageType.Physical);

                                if (t.IsValidTarget())
                                {
                                    CastSpell(R, t);
                                }

                                break;
                            }
                        case 1:
                            {
                                var t = GameObjects.EnemyHeroes.OrderBy(e => e.Distance(Player)).FirstOrDefault();

                                if (t.IsValidTarget())
                                {
                                    CastSpell(R, t);
                                }

                                break;
                            }
                    }
                }

                if (useR2.Active)
                {
                    if (targetR.IsValidTarget())
                    {
                        CastSpell(R, targetR);
                    }
                }
            }

            if (LagFree(1))
            {
                SetMana();
                Jungle();
            }

            if (LagFree(3) && W.IsReady() && !Player.IsWindingUp && autoW.Enabled)
            {
                LogicW();
            }

            if (LagFree(4) && R.IsReady())
            {
                LogicR();
            }
        }

        private void Game_OnWndProc(GameWndEventArgs args)
        {
            if (args.Msg == 513)
            {
                var t = GameObjects.EnemyHeroes.ToList().OrderBy(e => e.Distance(Game.CursorPos)).ToList().FirstOrDefault(g => g.Distance(Game.CursorPos) < 300);

                if (t != null)
                {
                    targetR = t;
                }
            }
        }

        private void Interrupter_OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (autoRinter.Enabled && R.IsReady() && sender.IsValidTarget(2500))
            {
                R.Cast(sender);
            }
        }

        private void Orbwalker_OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            LogicQ(args.Target);
        }

        private void LogicQ(AttackableUnit t1)
        {
            var t = t1 as AIBaseClient;

            if (t.IsValidTarget())
            {
                if (t is AIHeroClient)
                {
                    if (Combo && autoQ.Enabled && (Player.Mana > QMANA + RMANA || t.Health < 5 * Player.GetAutoAttackDamage(t)))
                    {
                        Q.Cast();
                    }
                    else if (Harass && harassQ.Enabled && Player.Mana > QMANA + WMANA + RMANA && InHarassList(t as AIHeroClient))
                    {
                        Q.Cast();
                    }
                }
                else if (t is AIMinionClient && LaneClear && FarmSpells && farmQ.Enabled)
                {
                    if (GameObjects.EnemyMinions.Where(e => e.IsValidTarget(600)).Count() >= LCminions.Value)
                    {
                        Q.Cast();
                    }
                }
            }
        }

        private void LogicW()
        {
            var t = Orbwalker.GetTarget() as AIHeroClient;

            if (t == null)
            {
                t = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            }

            if (t.IsValidTarget())
            {
                if (Combo && Player.Mana > WMANA + RMANA)
                {
                    CastW(t);
                }
                else if (Harass && Player.Mana > QMANA + WMANA + WMANA + RMANA && harassW.Enabled && OktwCommon.CanHarass())
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && InHarassList(e)))
                    {
                        CastW(enemy);
                    }
                }
                else if (OktwCommon.GetKsDamage(t,W) > t.Health && ksW.Enabled)
                {
                    CastW(t);
                }

                if (!None && Player.Mana > WMANA + RMANA && ccW.Enabled)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && !OktwCommon.CanMove(e)))
                    {
                        W.Cast(t);
                    }
                }
            }
            else if (FarmSpells && farmW.Enabled)
            {
                var minionList = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(W.Range));
                var farmPosition = W.GetCircularFarmLocation(minionList, 300);

                if (farmPosition.MinionsHit >= LCminions.Value)
                {
                    W.Cast(farmPosition.Position);
                }
            }
        }

        private void LogicR()
        {
            if (autoR.Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(2000) && OktwCommon.ValidUlt(e)))
                {
                    var rDmg = OktwCommon.GetKsDamage(target, R);

                    if (Combo && target.CountEnemyHeroesInRange(250) > 2 && autoRaoe.Enabled && Player.Distance(target) < 1500)
                    {
                        CastSpell(R, target);
                    }

                    if (Combo && Player.Distance(target) < W.Range && ksComboR.Enabled && Player.GetAutoAttackDamage(target) * 5 + rDmg + W.GetDamage(target) > target.Health && target.HasBuffOfType(BuffType.Slow) && !OktwCommon.IsSpellHeroCollision(target, R))
                    {
                        CastSpell(R, target);
                    }

                    if (rDmg > target.Health && target.CountAllyHeroesInRange(600) == 0 && Player.Distance(target) > 1000 && !OktwCommon.IsSpellHeroCollision(target, R))
                    {
                        CastSpell(R, target);
                    }
                }
            }

            if (Player.HealthPercent < 50)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(300) && e.IsMelee && OktwCommon.ValidUlt(e) && gapcloserList.Any(g => g.Enabled && g.Name == "gapcloser" + e.CharacterName)))
                {
                    R.Cast(enemy);
                }
            }
        }

        private void Jungle()
        {
            if (LaneClear)
            {
                var mobs = GameObjects.Jungle.Where(e => e.IsValidTarget(600));

                if (mobs.Count() > 0)
                {
                    var mob = mobs.First();

                    if (jungleW.Enabled && W.IsReady())
                    {
                        W.Cast(mob.PreviousPosition);
                        return;
                    }

                    if (jungleQ.Enabled && Q.IsReady())
                    {
                        Q.Cast();
                        return;
                    }
                }
            }
        }

        private void SetMana()
        {
            if ((manaDisable.Enabled && Combo) || Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;

            if (!R.IsReady())
            {
                RMANA = WMANA - Player.PARRegenRate * W.Instance.Cooldown;
            }
            else
            {
                RMANA = R.Instance.ManaCost;
            }
        }

        private void CastW(AIBaseClient t)
        {
            var poutput = W.GetPrediction(t);

            if (poutput.Hitchance >= HitChance.High)
            {
                W.Cast(poutput.CastPosition);
            }
        }

        private void drawText(string msg, Vector3 hero, System.Drawing.Color color, int weight)
        {
            var wts = Drawing.WorldToScreen(hero);
            Drawing.DrawText(wts[0] - msg.Length * 5, wts[1] + weight, color, msg);
        }
    }
}
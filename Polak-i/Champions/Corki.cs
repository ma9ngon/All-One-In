using System;
using System.Linq;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using SebbyLib;
    using SharpDX;

    class Corki : Base
    {
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);

        private readonly MenuBool autoQ = new MenuBool("autoQ", "Auto Q");
        private readonly MenuBool harassQ = new MenuBool("harassQ", "Q harass");

        private readonly MenuBool nktdW = new MenuBool("nktdW", "NoKeyToDash");

        private readonly MenuBool autoE = new MenuBool("autoE", "Auto E");
        private readonly MenuBool harassE = new MenuBool("harassE", "E harass");

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R");
        private readonly MenuSlider ammoR = new MenuSlider("ammoR", "Minimum R ammo harass", 3, 0, 6);
        private readonly MenuBool minionR = new MenuBool("minionR", "Try R on minion");
        private readonly MenuKeyBind useR = new MenuKeyBind("useR", "Semi-manual cast R key", Keys.T, KeyBindType.Press);

        private readonly MenuSlider ammoRlc = new MenuSlider("ammoRlc", "Minimum R ammo Lane clear", 3, 0, 6);
        private readonly MenuBool farmQ = new MenuBool("farmQ", "LaneClear + jungle Q");
        private readonly MenuBool farmR = new MenuBool("farmR", "LaneClear + jungle R");

        public Corki()
        {
            Q = new Spell(SpellSlot.Q, 825);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 1300);

            Q.SetSkillshot(0.3f, 200, 1000, false, SpellType.Circle);
            R.SetSkillshot(0.2f, 40, 2000, true, SpellType.Line);

            Q.AddHitBox = false;

            Local.Add(new Menu("draw", "Draw")
            {
                onlyRdy,
                qRange,
                wRange,
                eRange,
                rRange
            });

            Local.Add(new Menu("qConfig", "Q Config")
            {
                autoQ,
                harassQ
            });

            Local.Add(new Menu("wConfig", "W Config")
            {
                nktdW
            });

            Local.Add(new Menu("eConfig", "E Config")
            {
                autoE,
                harassE
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                ammoR,
                minionR,
                useR
            });

            FarmMenu.Add(ammoRlc);
            FarmMenu.Add(farmQ);
            FarmMenu.Add(farmR);

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnBeforeAttack += Orbwalker_OnBeforeAttack;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (nktdW.Enabled)
            {
                if (Game.CursorPos.Distance(Player.Position) > Player.AttackRange + Player.BoundingRadius * 2)
                {
                    drawText("dash: ON", Player.Position, System.Drawing.Color.Red);
                }
                else
                {
                    drawText("dash: OFF", Player.Position, System.Drawing.Color.GreenYellow);
                }
            }

            if (qRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (Q.IsReady())
                    {
                        Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                    }
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
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

            if (eRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (E.IsReady())
                    {
                        Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
                    }
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
                }
            }

            if (rRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (R.IsReady())
                    {
                        Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1);
                    }
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1);
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (LagFree(0))
            {
                SetMana();
                LogicFarm();
            }

            if (LagFree(1) && Q.IsReady() && !Player.IsWindingUp && Sheen(Orbwalker.GetTarget()))
            {
                LogicQ();
            }

            if (LagFree(2) && Combo && W.IsReady())
            {
                LogicW();
            }

            if (LagFree(4) && R.IsReady() && !Player.IsWindingUp && Sheen(Orbwalker.GetTarget()))
            {
                LogicR();
            }
        }

        private void Orbwalker_OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (E.IsReady() && Sheen(args.Target) && args.Target.Type == GameObjectType.AIHeroClient)
            {
                if (Combo && autoE.Enabled && Player.Mana > EMANA + RMANA)
                {
                    E.Cast(args.Target.Position);
                }
                
                if (Harass && harassE.Enabled && Player.Mana > QMANA + EMANA + RMANA && OktwCommon.CanHarass())
                {
                    E.Cast(args.Target.Position);
                }

                if (!Q.IsReady() && E.IsReady() && args.Target.Health < Player.FlatPhysicalDamageMod * 2)
                {
                    E.Cast(args.Target.Position);
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                if (Combo && autoQ.Enabled && Player.Mana > QMANA + RMANA)
                {
                    CastSpell(Q, t);
                }
                else if (Harass && harassQ.Enabled && InHarassList(t) && Player.Mana > WMANA + EMANA + RMANA + RMANA && OktwCommon.CanHarass())
                {
                    CastSpell(Q, t);
                }
                else
                {
                    var qDmg = OktwCommon.GetKsDamage(t, Q);
                    var rDmg = R.GetDamage(t);

                    if (qDmg > t.Health)
                    {
                        Q.Cast(t);
                    }
                    else if (qDmg + rDmg > t.Health && Player.Mana > QMANA + RMANA)
                    {
                        CastSpell(Q, t);
                    }
                    else if (qDmg + 2 * rDmg > t.Health && Player.Mana > QMANA + RMANA * 2)
                    {
                        CastSpell(Q, t);
                    }

                    if (!None && Player.Mana > WMANA + EMANA + RMANA)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(Q.Range) && !OktwCommon.CanMove(e)))
                        {
                            Q.Cast(enemy);
                        }
                    }
                }
            }
        }

        private void LogicW()
        {
            var dashPosition = Player.Position.Extend(Game.CursorPos, W.Range);

            if (Game.CursorPos.Distance(Player.Position) > Player.AttackRange + Player.BoundingRadius * 2 && Combo && nktdW.Enabled && Player.Mana > WMANA + RMANA - 10)
            {
                W.Cast(dashPosition);
            }
        }

        private void LogicR()
        {
            var rSplash = 150f;

            if (BonusR())
            {
                rSplash = 300f;
            }

            var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                var rDmg = OktwCommon.GetKsDamage(t, R);
                var qDmg = Q.GetDamage(t);

                if (rDmg * 2 > t.Health)
                {
                    CastR(t);
                }
                else if (t.IsValidTarget(Q.Range) && qDmg + rDmg > t.Health)
                {
                    CastR(t);
                }

                if (R.Ammo > 1)
                {
                    var countR = 0;

                    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(R.Range)))
                    {
                        var tmpCount = enemy.CountEnemyHeroesInRange(rSplash);

                        if (tmpCount > 1)
                        {
                            if (countR == 0 || tmpCount > countR)
                            {
                                t = enemy;
                                countR = tmpCount;
                            }
                        }
                    }

                    if (Combo && Player.Mana > RMANA * 3)
                    {
                        CastR(t);
                    }
                    else if (Harass && Player.Mana > QMANA + WMANA + EMANA + RMANA && R.Ammo >= ammoR.Value && OktwCommon.CanHarass())
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(R.Range) && InHarassList(e)))
                        {
                            CastR(enemy);
                        }
                    }

                    if (!None && Player.Mana > QMANA + EMANA + RMANA)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(R.Range) && !OktwCommon.CanMove(e)))
                        {
                            CastR(enemy);
                        }
                    }
                }
            }
        }

        private void LogicFarm()
        {
            if (LaneClear && !Player.IsWindingUp && Sheen(Orbwalker.GetTarget()))
            {
                var mobs = GameObjects.Jungle.Where(e => e.IsValidTarget(Q.Range));

                if (mobs.Count() > 0 && Player.Mana > QMANA + WMANA + EMANA + RMANA)
                {
                    var mob = mobs.First();

                    if (farmQ.Enabled && Q.IsReady())
                    {
                        Q.Cast(mob.Position);
                        return;
                    }

                    if (farmR.Enabled && R.IsReady())
                    {
                        R.Cast(mob.Position);
                        return;
                    }
                }

                if (FarmSpells)
                {
                    var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range));

                    if (farmR.Enabled && R.IsReady() && R.Ammo >= ammoRlc.Value)
                    {
                        var rfarm = R.GetCircularFarmLocation(minions, 100);

                        if (rfarm.MinionsHit >= LCminions.Value)
                        {
                            R.Cast(rfarm.Position);
                            return;
                        }
                    }

                    if (farmQ.Enabled && Q.IsReady())
                    {
                        var qfarm = Q.GetCircularFarmLocation(minions);

                        if (qfarm.MinionsHit >= LCminions.Value)
                        {
                            Q.Cast(qfarm.Position);
                            return;
                        }
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
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
            {
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            }
            else
            {
                RMANA = R.Instance.ManaCost;
            }
        }

        private bool BonusR()
        {
            return Player.HasBuff("mbcheck2");
        }

        private void CastR(AIHeroClient t)
        {
            CastSpell(R, t);

            if (minionR.Enabled)
            {
                var poutput = R.GetPrediction(t);
                var col = poutput.CollisionObjects.Count(e => e.IsValidTarget() && e.IsEnemy);
                var prepos = Prediction.GetPrediction(t, 0.4f);

                if (col == 0 && prepos.Hitchance < HitChance.High)
                {
                    return;
                }

                var rSplash = 140f;

                if (BonusR())
                {
                    rSplash = 290f;
                }

                var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(R.Range - rSplash));

                foreach (var minion in minions.Where(e => e.Distance(poutput.CastPosition) < rSplash))
                {
                    if (R.Cast(minion) == CastStates.SuccessfullyCasted)
                    {
                        return;
                    }
                }
            }
        }

        private bool Sheen(AttackableUnit target)
        {
            return !(target.IsValidTarget() && Player.HasBuff("sheen"));
        }

        private void drawText(string msg, Vector3 hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(hero);
            Drawing.DrawText(wts[0] - msg.Length * 5, wts[1] - 200, color, msg);
        }
    }
}
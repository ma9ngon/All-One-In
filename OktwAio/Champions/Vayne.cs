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

    class Vayne : Base
    {
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool eRange2 = new MenuBool("eRange2", "E push position");

        private readonly MenuBool autoQ = new MenuBool("autoQ", "Auto Q");
        private readonly MenuSlider stackQ = new MenuSlider("stackQ", "Q at X stack", 2, 1, 2);
        private readonly MenuBool logicQE = new MenuBool("logicQE", "try Q + E");
        private readonly MenuBool onlyQ = new MenuBool("onlyQ", "Q only after AA", false);

        private readonly MenuBool gapE = new MenuBool("gapE", "Enable");
        private readonly List<MenuBool> gapList = new List<MenuBool>();
        private readonly List<MenuBool> stunList = new List<MenuBool>();
        private readonly MenuKeyBind useE = new MenuKeyBind("useE", "OneKeyToCast E closest person", Keys.T, KeyBindType.Press);
        private readonly MenuBool ksE = new MenuBool("ksE", "E KS");
        private readonly MenuBool comboE = new MenuBool("comboE", "E combo only", false);

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R");
        private readonly MenuBool visibleR = new MenuBool("visibleR", "Unvisable block AA");
        private readonly MenuBool autoQR = new MenuBool("autoQR", "Auto Q when R active");

        private readonly MenuBool farmQ = new MenuBool("farmQ", "Q farm helper");
        private readonly MenuBool farmQjungle = new MenuBool("farmQjungle", "Q jungle");

        public Core.OKTWdash Dash;

        public Vayne()
        {
            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550f);
            R = new Spell(SpellSlot.R);

            E.SetTargetted(0.25f, 2200f);

            Local.Add(new Menu("draw", "Draw")
            {
                onlyRdy,
                qRange,
                eRange,
                eRange2
            });

            Local.Add(new Menu("qConfig", "Q Config")
            {
                autoQ,
                stackQ,
                logicQE,
                onlyQ
            });

            var gapMenu = new Menu("gap", "GapCloser");
            var useMenu = new Menu("use", "Use E");

            gapMenu.Add(gapE);
            GameObjects.EnemyHeroes.ForEach(
                e =>
                {
                    var gap = new MenuBool("gap" + e.CharacterName, e.CharacterName);
                    var use = new MenuBool("stun" + e.CharacterName, e.CharacterName);

                    gapList.Add(gap);
                    stunList.Add(use);

                    gapMenu.Add(gap);
                    useMenu.Add(use);
                });


            Local.Add(new Menu("eConfig", "E Config")
            {
                gapMenu,
                useMenu,
                useE,
                ksE,
                comboE
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                visibleR,
                autoQR
            });

            FarmMenu.Add(farmQ);
            FarmMenu.Add(farmQjungle);

            Dash = new Core.OKTWdash(Q);

            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterrupterSpell += Interrupter_OnInterrupterSpell;
            Orbwalker.OnAfterAttack += Orbwalker_OnAfterAttack;
            Orbwalker.OnBeforeAttack += Orbwalker_OnBeforeAttack;
        }

        private void AntiGapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!OktwCommon.CheckGapcloser(sender, args))
            {
                return;
            }

            if (E.IsReady() && sender.IsValidTarget(E.Range) && gapE.Enabled && gapList.Any(e => e.Enabled && e.Name == "gap" + sender.CharacterName))
            {
                E.Cast(sender);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
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

            if (eRange2.Enabled && E.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(800)))
                {
                    var poutput = E.GetPrediction(target);
                    var pushDistance = 460;
                    var finalPosition = poutput.CastPosition.Extend(Player.PreviousPosition, -pushDistance);

                    if (finalPosition.IsWall())
                    {
                        Render.Circle.DrawCircle(finalPosition, 100, System.Drawing.Color.Red);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(finalPosition, 100, System.Drawing.Color.YellowGreen);
                    }
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            var dashPosition = Player.Position.Extend(Game.CursorPos, Q.Range);

            if (E.IsReady())
            {
                if (!comboE.Enabled || Combo)
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(E.Range) && e.Path.Count() < 2))
                    {
                        if (CondemnCheck(Player.PreviousPosition, target) && stunList.Any(e => e.Enabled && e.Name == "stun" + target.CharacterName))
                        {
                            E.Cast(target);
                        }
                        else if (Q.IsReady() && Dash.IsGoodPosition(dashPosition) && logicQE.Enabled && CondemnCheck(dashPosition, target))
                        {
                            Q.Cast(dashPosition);
                        }
                    }
                }
            }

            if (LagFree(1) && Q.IsReady())
            {
                if (autoQR.Enabled && Player.HasBuff("VayneInquisition") && Player.CountEnemyHeroesInRange(1500) > 0 && Player.CountEnemyHeroesInRange(550) != 1)
                {
                    var dashPos = Dash.CastDash();

                    if (!dashPos.IsZero)
                    {
                        Q.Cast(dashPos);
                    }
                }

                if (Combo && autoQ.Enabled && !onlyQ.Enabled)
                {
                    var t = TargetSelector.GetTarget(900, DamageType.Physical);

                    if (t.IsValidTarget() && !t.InAutoAttackRange() && t.Position.Distance(Game.CursorPos) < t.Position.Distance(Player.Position) && !t.IsFacing(Player))
                    {
                        var dashPos = Dash.CastDash();

                        if (!dashPos.IsZero)
                        {
                            Q.Cast(dashPos);
                        }
                    }
                }
            }

            if (LagFree(2))
            {
                AIHeroClient bestEnemy = null;

                foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(E.Range)))
                {
                    if (target.IsValidTarget(250) && target.IsMelee)
                    {
                        if (Q.IsReady() && autoQ.Enabled)
                        {
                            var dashPos = Dash.CastDash(true);

                            if (!dashPos.IsZero)
                            {
                                Q.Cast(dashPos);
                            }
                        }
                        else if (E.IsReady() && Player.HealthPercent < 40)
                        {
                            E.Cast(target);
                        }
                    }

                    if (bestEnemy == null)
                    {
                        bestEnemy = target;
                    }
                    else if (Player.Distance(target.Position) < Player.Distance(bestEnemy.Position))
                    {
                        bestEnemy = target;
                    }
                }

                if (useE.Active && bestEnemy != null)
                {
                    E.Cast(bestEnemy);
                }
            }

            if (LagFree(3) && R.IsReady())
            {
                if (autoR.Enabled)
                {
                    if (Player.CountEnemyHeroesInRange(700) > 2)
                    {
                        R.Cast();
                    }
                    else if (Combo && Player.CountEnemyHeroesInRange(600) > 1)
                    {
                        R.Cast();
                    }
                    else if (Player.HealthPercent < 50 && Player.CountEnemyHeroesInRange(500) > 0)
                    {
                        R.Cast();
                    }
                }
            }
        }

        private void Interrupter_OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (E.IsReady() && sender.IsValidTarget(E.Range))
            {
                E.Cast(sender);
            }
        }

        private void Orbwalker_OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            var t = args.Target as AIHeroClient;

            if (t != null)
            {
                if (E.IsReady() && ksE.Enabled)
                {
                    var incomingDmg = OktwCommon.GetIncomingDamage(t, 0.3f, false);

                    if (incomingDmg > t.Health)
                    {
                        return;
                    }

                    var dmgE = E.GetDamage(t) + incomingDmg;

                    if (GetWStacks(t) == 1)
                    {
                        dmgE += W.GetDamage(t);
                    }

                    if (dmgE > t.Health)
                    {
                        E.Cast(t);
                    }
                }

                if (Q.IsReady() && !None && autoQ.Enabled && (GetWStacks(t) == stackQ.Value - 1 || Player.HasBuff("VayneInquisition")))
                {
                    var dashPos = Dash.CastDash(true);

                    if (!dashPos.IsZero)
                    {
                        Q.Cast(dashPos);
                    }
                }
            }

            var m = args.Target as AIMinionClient;

            if (m != null && Q.IsReady() && Farm && farmQ.Enabled)
            {
                var dashPosition = Player.Position.Extend(Game.CursorPos, Q.Range);

                if (!Dash.IsGoodPosition(dashPosition))
                {
                    return;
                }

                if (farmQjungle.Enabled && m.Team == GameObjectTeam.Neutral)
                {
                    Q.Cast(dashPosition);
                }

                if (farmQ.Enabled)
                {
                    foreach (var minion in GameObjects.EnemyMinions.Where(e => e.InAutoAttackRange() && e.NetworkId != m.NetworkId))
                    {
                        var time = (int)(Player.AttackCastDelay * 1000) + Game.Ping / 2 + 1000 * (int)Math.Max(0, Player.Distance(minion) - Player.BoundingRadius) / (int)Player.BasicAttack.MissileSpeed;
                        var predHealth = HealthPrediction.GetPrediction(minion, time);

                        if (predHealth > 0 && predHealth < Player.GetAutoAttackDamage(minion) + Q.GetDamage(minion))
                        {
                            Q.Cast(dashPosition);
                        }
                    }
                }
            }
        }

        private void Orbwalker_OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (visibleR.Enabled && Player.HasBuff("vaynetumblefade") && Player.CountEnemyHeroesInRange(800) > 1)
            {
                args.Process = false;
            }

            if (args.Target.Type != GameObjectType.AIHeroClient)
            {
                return;
            }

            var t = args.Target as AIHeroClient;

            if (GetWStacks(t) < 2 && args.Target.Health > 5 * Player.GetAutoAttackDamage(t))
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(800) && GetWStacks(e) == 2))
                {
                    if (target.InAutoAttackRange() && args.Target.Health > 3 * Player.GetAutoAttackDamage(target))
                    {
                        args.Process = false;
                        Orbwalker.ForceTarget = target;
                    }
                }
            }
        }

        private bool CondemnCheck(Vector3 fromPosition, AIHeroClient target)
        {
            var prepos = E.GetPrediction(target);
            var pushDistance = 470f;

            if (Player.PreviousPosition != fromPosition)
            {
                pushDistance = 410f;
            }

            var radius = 250;
            var start2 = target.PreviousPosition;
            var end2 = prepos.CastPosition.Extend(fromPosition, -pushDistance);

            var start = start2.ToVector2();
            var end = end2.ToVector2();
            var dir = (end - start).Normalized();
            var pDir = dir.Perpendicular();

            var rightEndPos = end + pDir * radius;
            var leftEndPos = end - pDir * radius;

            var rEndPos = new Vector3(rightEndPos.X, rightEndPos.Y, Player.Position.Z);
            var lEndPos = new Vector3(leftEndPos.X, leftEndPos.Y, Player.Position.Z);

            var step = start2.Distance(rEndPos) / 10;

            for (var i =0; i < 10; i++)
            {
                var pr = start2.Extend(rEndPos, step * i);
                var pl = start2.Extend(lEndPos, step * i);

                if (pr.IsWall() && pl.IsWall())
                {
                    return true;
                }
            }

            return false;
        }

        private int GetWStacks(AIBaseClient target)
        {
            return target.GetBuffCount("VayneSilveredDebuff");
        }
    }
}
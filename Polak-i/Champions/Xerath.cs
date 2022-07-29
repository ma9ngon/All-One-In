using System;
using System.Linq;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.Utility;
    using SebbyLib;
    using SharpDX;

    class Xerath : Base
    {
        private readonly MenuBool noti = new MenuBool("noti", "Show notification & line");
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);
        private readonly MenuBool rRangeMini = new MenuBool("rRangeMini", "R range minimap", true);

        private readonly MenuBool autoQ = new MenuBool("autoQ", "Auto Q");
        private readonly MenuBool harassQ = new MenuBool("harassQ", "Harass Q");

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W");
        private readonly MenuBool harassW = new MenuBool("harassW", "Harass W");

        private readonly MenuBool autoE = new MenuBool("autoE", "Auto E");
        private readonly MenuBool harassE = new MenuBool("harassE", "Harass E");

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R 2x dmg R");
        private readonly MenuBool autoRlast = new MenuBool("autoRlast", "Cast last position if no target", false);
        private readonly MenuKeyBind useR = new MenuKeyBind("useR", "Semi-manual cast R key", Keys.T, KeyBindType.Press);
        private readonly MenuBool trinkiet = new MenuBool("trinkiet", "Auto blue trinkiet");
        private readonly MenuSlider delayR = new MenuSlider("delayR", "Custom R delay ms (1000ms = 1 sec)", 0, 0, 3000);
        private readonly MenuSlider maxRangeR = new MenuSlider("maxRangeR", "Max R adjustment (R range - slider)", 0, 0, 5000);

        private readonly MenuBool separate = new MenuBool("separate", "Separate laneclear from harass", false);
        private readonly MenuBool farmQ = new MenuBool("farmQ", "Lane clear Q");
        private readonly MenuBool farmW = new MenuBool("farmW", "Lane clear W");

        private readonly MenuBool jungleQ = new MenuBool("jungleQ", "Jungle clear Q");
        private readonly MenuBool jungleW = new MenuBool("jungleW", "Jungle clear W");
        private readonly MenuBool jungleE = new MenuBool("jungleE", "Jungle clear E");

        private readonly MenuBool force = new MenuBool("force", "Force passive use in combo on minion");

        private Vector3 targetR;
        private float lastR = 0;

        private Items.Item FarsightOrb = new Items.Item(ItemId.Farsight_Alteration, 4000f);

        private bool IsCastingR
        {
            get
            {
                var lastCastedSpell = Player.GetLastCastedSpell();

                return Player.HasBuff("XerathLocusOfPower2")
                    || (lastCastedSpell.Name == "XerathLocusOfPower2" && Variables.GameTimeTickCount - lastCastedSpell.StartTime < 500);
            }
        }

        public Xerath()
        {
            Q = new Spell(SpellSlot.Q, 1400f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 675f);

            Q.SetSkillshot(0.6f, 90f, float.MaxValue, false, SpellType.Line);
            W.SetSkillshot(0.7f, 105f, float.MaxValue, false, SpellType.Circle);
            E.SetSkillshot(0.25f, 60f, 1400f, true, SpellType.Line);
            R.SetSkillshot(0.7f, 130f, float.MaxValue, false, SpellType.Circle);

            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1400, 1.5f);

            Local.Add(new Menu("draw", "Draw")
            {
                noti,
                onlyRdy,
                qRange,
                wRange,
                eRange,
                rRange,
                rRangeMini
            });

            Local.Add(new Menu("qConfig", "Q Config")
            {
                autoQ,
                harassQ
            });

            Local.Add(new Menu("wConfig", "W Config")
            {
                autoW,
                harassW
            });

            Local.Add(new Menu("eConfig", "E Config")
            {
                autoE,
                harassE
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                autoRlast,
                useR,
                trinkiet,
                delayR,
                maxRangeR
            });

            FarmMenu.Add(separate);
            FarmMenu.Add(farmQ);
            FarmMenu.Add(farmW);
            FarmMenu.Add(jungleQ);
            FarmMenu.Add(jungleW);
            FarmMenu.Add(jungleE);

            Local.Add(force);

            AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
            AIBaseClient.OnIssueOrder += AIBaseClient_OnIssueOrder;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterrupterSpell += Interrupter_OnInterrupterSpell;
            Orbwalker.OnAfterAttack += Orbwalker_OnAfterAttack;
            Orbwalker.OnBeforeAttack += Orbwalker_OnBeforeAttack;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void AIBaseClient_OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "XerathLocusPulse")
                {
                    lastR = Game.Time;
                }
            }
        }

        private void AIBaseClient_OnIssueOrder(AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
        {
            if (args.Order == GameObjectOrder.AttackUnit && Q.IsCharging)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                args.Process = false;
            }
        }

        private void AntiGapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!OktwCommon.CheckGapcloser(sender, args))
            {
                return;
            }

            if (Player.Distance(sender.PreviousPosition) < E.Range)
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

            if (noti.Enabled && R.IsReady())
            {
                var t = TargetSelector.GetTarget(R.Range, DamageType.Magical);

                if (t.IsValidTarget())
                {
                    var rDmg = R.GetDamage(t);
                    var maxLoop = R.Level + 2;

                    for (var i = 1; i <= maxLoop; i++)
                    {
                        if (rDmg * i > t.Health)
                        {
                            Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, i + " x Ult can kill: " + t.CharacterName + " have: " + t.Health + " hp");
                            Drawing.DrawLine(Drawing.WorldToScreen(Player.Position), Drawing.WorldToScreen(t.Position), 10, System.Drawing.Color.Yellow);
                            break;
                        }
                    }
                }
            }
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (rRangeMini.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (R.IsReady())
                    {
                        MiniMap.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Aqua);
                    }
                }
                else
                {
                    MiniMap.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Aqua);
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (LagFree(3) && R.IsReady())
            {
                LogicR();
            }

            if (IsCastingR || Player.IsCastingImporantSpell())
            {
                Orbwalker.MoveEnabled = false;
                Orbwalker.AttackEnabled = false;
            }
            else
            {
                Orbwalker.MoveEnabled = true;
                Orbwalker.AttackEnabled = true;
            }

            if (Q.IsCharging && (int)(Game.Time * 10) % 2 == 0)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

            if (LagFree(1))
            {
                SetMana();
                LogicJungle();

                var manas = new int[] { 0, 30, 33, 36, 42, 48, 54, 63, 72, 81, 90, 102, 114, 126, 138, 150, 165, 180, 195 };
                var mana = manas[Math.Max(0, Math.Min(18, Player.Level))];

                if (!Player.HasBuff("xerathascended2onhit") || Player.Mana + mana > Player.MaxMana)
                {
                    Orbwalker.ForceTarget = null;
                }
                else if ((Combo || Harass) && force.Enabled && Orbwalker.GetTarget() == null)
                {
                    var minion = GameObjects.EnemyMinions
                        .Where(e => e.IsValidTarget(Player.AttackRange + Player.BoundingRadius * 2))
                        .OrderByDescending(e => e.Health)
                        .FirstOrDefault();

                    if (minion != null && OktwCommon.CanHarass())
                    {
                        Orbwalker.ForceTarget = minion;
                    }
                }
            }

            if (autoE.Enabled && E.IsReady())
            {
                LogicE();
            }

            if (LagFree(2) && autoW.Enabled && W.IsReady() && !Player.IsWindingUp)
            {
                LogicW();
            }

            if (LagFree(4) && autoQ.Enabled && Q.IsReady() && !Player.IsWindingUp)
            {
                LogicQ();
            }
        }

        private void Interrupter_OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (Player.Distance(sender.PreviousPosition) < E.Range)
            {
                E.Cast(sender);
            }
        }

        private void Orbwalker_OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            Orbwalker.ForceTarget = null;
        }

        private void Orbwalker_OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (args.Target is AIMinionClient && !Player.HasBuff("xerathascended2onhit") && Combo)
            {
                args.Process = false;
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R)
            {
                if (trinkiet.Enabled && !IsCastingR)
                {
                    if (FarsightOrb.IsReady)
                    {
                        FarsightOrb.Cast(targetR);
                    }
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var t2 = TargetSelector.GetTarget(1400, DamageType.Magical);

            if (t.IsValidTarget() && t2.IsValidTarget() && t == t2 && !(separate.Enabled && LaneClear))
            {
                if (Q.IsCharging)
                {
                    CastSpell(Q, t);

                    if (OktwCommon.GetPassiveTime(Player, "XerathArcanopulseChargeUp") < 1 || Player.CountEnemyHeroesInRange(800) > 0 || Player.Distance(t) > 1350)
                    {
                        Q.Cast(Q.GetPrediction(t).CastPosition);
                    }
                    else if (OktwCommon.GetPassiveTime(Player, "XerathArcanopulseChargeUp") < 2 || Player.CountEnemyHeroesInRange(1000) > 0)
                    {
                        Q.CastIfHitchanceMinimum(t, HitChance.VeryHigh);
                    }
                }
                else if (t.IsValidTarget(Q.Range - 300))
                {
                    if (t.Health < OktwCommon.GetKsDamage(t,Q))
                    {
                        Q.StartCharging();
                    }
                    else if (Combo && Player.Mana > QMANA + EMANA)
                    {
                        Q.StartCharging();
                    }
                    else if (Harass && harassQ.Enabled && Player.Mana > QMANA + QMANA + EMANA + RMANA && InHarassList(t) && !Player.IsUnderEnemyTurret())
                    {
                        Q.StartCharging();
                    }
                    else if ((Combo || Harass) && Player.Mana > WMANA + RMANA)
                    {
                        if (GameObjects.EnemyHeroes.Any(e => e.IsValidTarget(Q.Range) && !OktwCommon.CanMove(e)))
                        {
                            Q.StartCharging();
                        }
                    }
                }
            }
            else if (LaneClear && Q.Range > 1000 && Player.CountEnemyHeroesInRange(1350) == 0 && (Q.IsCharging || FarmSpells && farmQ.Enabled))
            {
                var allMinionsQ = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range)).ToList();
                var farmQ = Q.GetLineFarmLocation(allMinionsQ);

                if (farmQ.MinionsHit >= LCminions.Value || (Q.IsCharging && farmQ.MinionsHit > 0))
                {
                    Q.Cast(farmQ.Position);
                }
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, DamageType.Magical);

            if (t.IsValidTarget())
            {
                var qDmg = Q.GetDamage(t);
                var wDmg = OktwCommon.GetKsDamage(t, W);

                if (wDmg > t.Health)
                {
                    CastSpell(W, t);
                }
                else if (qDmg + wDmg > t.Health && Player.Mana > QMANA + WMANA)
                {
                    CastSpell(W, t);
                }
                else if (Combo && Player.Mana > WMANA + RMANA)
                {
                    CastSpell(W, t);
                }
                else if (Harass && OktwCommon.CanHarass() && harassW.Enabled && InHarassList(t) && Player.Mana > QMANA + WMANA + WMANA + EMANA + RMANA)
                {
                    CastSpell(W, t);
                }
                else if (Combo || Harass)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && !OktwCommon.CanMove(e)))
                    {
                        W.Cast(enemy);
                    }
                }
            }
            else if (FarmSpells && farmW.Enabled && LaneClear && Player.Mana > WMANA + RMANA)
            {
                var allMinionsW = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(W.Range)).ToList();
                var farmW = W.GetCircularFarmLocation(allMinionsW);

                if (farmW.MinionsHit >= LCminions.Value)
                {
                    W.Cast(farmW.Position);
                }
            }
        }

        private void LogicE()
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(E.Range) && E.GetDamage(e) + OktwCommon.GetKsDamage(e, Q) + W.GetDamage(e) > e.Health))
            {
                CastSpell(E, enemy);
            }

            var t = Orbwalker.GetTarget() as AIHeroClient;

            if (!t.IsValidTarget())
            {
                t = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            }

            if (t.IsValidTarget())
            {
                if (Combo && Player.Mana > EMANA + RMANA)
                {
                    CastSpell(E, t);
                }

                if (Harass && OktwCommon.CanHarass() && harassE.Enabled && Player.Mana > WMANA + EMANA + EMANA + RMANA)
                {
                    CastSpell(E, t);
                }

                foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(E.Range) && !OktwCommon.CanMove(e)))
                {
                    E.Cast(enemy);
                }
            }
        }

        private void LogicR()
        {
            if (!IsCastingR)
            {
                R.Range = 5000 - maxRangeR.Value;
            }

            var t = TargetSelector.GetTarget(R.Range, DamageType.Magical);

            if (t.IsValidTarget())
            {
                if (useR.Active && !IsCastingR)
                {
                    R.Cast();
                }

                if (!t.IsValidTarget(W.Range) && autoR.Enabled && !IsCastingR && t.CountAllyHeroesInRange(500) == 0 && Player.CountEnemyHeroesInRange(1100) == 0)
                {
                    if (OktwCommon.GetKsDamage(t, R) + R.GetDamage(t) > t.Health)
                    {
                        R.Cast();
                    }
                }

                if (Game.Time - lastR > 0.001 * delayR.Value && IsCastingR)
                {
                    CastSpell(R, t);
                }

                targetR = R.GetPrediction(t).CastPosition;
            }
            else if (autoRlast.Enabled && Game.Time - lastR > 0.001 * delayR.Value && IsCastingR)
            {
                R.Cast(targetR);
            }
        }

        private void LogicJungle()
        {
            if (LaneClear && Player.Mana > WMANA + WMANA + RMANA + RMANA)
            {
                var mobs = GameObjects.GetJungles(600);

                if (mobs.Count > 0)
                {
                    var mob = mobs.First();

                    if (jungleE.Enabled && E.IsReady())
                    {
                        E.Cast(mob.Position);
                        return;
                    }

                    if (jungleW.Enabled && W.IsReady())
                    {
                        W.Cast(mob.Position);
                        return;
                    }

                    if (jungleQ.Enabled && Q.IsReady())
                    {
                        Q.Cast(mob.Position);
                        return;
                    }
                }
            }
        }

        private void SetMana()
        {
            if ((manaDisable.Enabled && Combo) || Player.HealthPercent < 20 || Q.IsCharging)
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
    }
}
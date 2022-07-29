using System;
using System.Linq;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using SebbyLib;
    using SharpDX;

    class Kalista : Base
    {
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);
        private readonly MenuBool eDamageEnemies = new MenuBool("eDamageEnemies", "E damage % on enemies", false);
        private readonly MenuBool eDamageJungles = new MenuBool("eDamageJungles", "E damage % on jungles", false);

        private readonly MenuSlider qMana = new MenuSlider("qMana", "Q harass mana %", 50);
        private readonly MenuList qMode = new MenuList("qMode", "Q combo mode", new[] { "Always", "OKTW logic" }, 1);

        private readonly MenuBool dragonW = new MenuBool("dragonW", "Auto W Dragon, Baron, Blue, Red");

        private readonly MenuSlider countE = new MenuSlider("countE", "Auto E if x stacks", 10, 0, 30);
        private readonly MenuSlider dmgE = new MenuSlider("dmgE", "E % dmg adjust", 100, 50, 150);
        private readonly MenuBool deadE = new MenuBool("deadE", "Cast E before Kalista dead");
        private readonly MenuBool killminE = new MenuBool("killminE", "Cast E minion kill + harass target");

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R");

        //private readonly MenuBool balista = new MenuBool("balista", "Balista R");
        //private readonly MenuSlider rangeBalista = new MenuSlider("rangeBalista", "Balista min range", 500, 0, 1000);

        private readonly MenuBool minionAA = new MenuBool("minionAA", "AA minions if no enemies in range when combo mode");

        private readonly MenuBool farmQ = new MenuBool("farmQ", "Lane clear Q");
        private readonly MenuBool farmE = new MenuBool("farmE", "Lane clear E");
        private readonly MenuSlider farmQcount = new MenuSlider("farmQcount", "Lane clear Q if x minions", 2, 1, 10);
        private readonly MenuSlider farmEcount = new MenuSlider("farmEcount", "Auto E if x minions", 2, 1, 10);
        private readonly MenuBool minionE = new MenuBool("minionE", "Auto E big minion");
        private readonly MenuBool jungleE = new MenuBool("jungleE", "Jungle ks E");

        private float lastCastE = 0;
        //private float grabTime = 0;
        private int countW = 0;
        private AIHeroClient AllyR;

        public Kalista()
        {
            Q = new Spell(SpellSlot.Q, 1100f);
            Q1 = new Spell(SpellSlot.Q, 1100f);
            W = new Spell(SpellSlot.W, 5000f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1100f);

            Q.SetSkillshot(0.25f, 40f, 2400f, true, SpellType.Line);
            Q1.SetSkillshot(0.25f, 40f, 2400f, false, SpellType.Line);

            Local.Add(new Menu("draw", "Draw")
            {
                onlyRdy,
                qRange,
                eRange,
                rRange,
                eDamageEnemies,
                eDamageJungles
            });

            Local.Add(new Menu("qConfig", "Q Config")
            {
                qMana,
                qMode
            });

            Local.Add(new Menu("wConfig", "W Config")
            {
                dragonW
            });

            Local.Add(new Menu("eConfig", "E Config")
            {
                countE,
                dmgE,
                deadE,
                killminE
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR
            });

            //Local.Add(new Menu("balistaConfig", "Balista Config")
            //{
            //    balista,
            //    rangeBalista
            //});

            Local.Add(minionAA);

            FarmMenu.Add(farmQ);
            FarmMenu.Add(farmE);
            FarmMenu.Add(farmQcount);
            FarmMenu.Add(farmEcount);
            FarmMenu.Add(minionE);
            FarmMenu.Add(jungleE);

            AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private void AIBaseClient_OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "KalistaExpungeWrapper")
                {
                    lastCastE = Game.Time;
                }
                else if (args.SData.Name == "KalistaW")
                {
                    countW++;
                }
            }
            //else if (R.IsReady() && sender.IsAlly && args.SData.Name == "RocketGrab" && Player.Distance(sender.Position) < R.Range && Player.Distance(sender.Position) > rangeBalista.Value)
            //{
            //    grabTime = Game.Time;
            //}
        }

        private void AntiGapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (Q.IsReady() && Player.Mana > QMANA + EMANA)
            {
                if (sender.IsValidTarget(Q.Range))
                {
                    Q.Cast(sender);
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (eDamageEnemies.Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(E.Range + 500)))
                {
                    var dmg = E.GetDamage(enemy);

                    if (enemy.Health - dmg > 0)
                    {
                        drawText((int)(dmg / enemy.Health * 100) + " %", enemy, System.Drawing.Color.GreenYellow);
                    }
                    else
                    {
                        drawText("KILL E", enemy, System.Drawing.Color.Red);
                    }
                }
            }

            if (eDamageJungles.Enabled)
            {
                foreach (var mob in GameObjects.Jungle.Where(e => e.IsValidTarget(E.Range)))
                {
                    drawText((int)(E.GetDamage(mob) / mob.Health * 100 * (mob.GetJungleType().HasFlag(JungleType.Legendary) ? 0.5 : 1)) + " %", mob, System.Drawing.Color.GreenYellow);
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
            if (Player.IsRecalling())
            {
                return;
            }

            RefreshLogic();

            //if (R.IsReady() && AllyR.IsValidTarget(R.Range, false) && AllyR.CharacterName == "Blitzcrank" && balista.Enabled && Player.Distance(AllyR.Position) > rangeBalista.Value)
            //{
            //    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget() && e.GetBuff("rocketgrab2")?.Caster == AllyR))
            //    {
            //        R.Cast();
            //    }

            //    if (Game.Time - grabTime < 1)
            //    {
            //        return;
            //    }
            //}

            SurvivalLogic();

            if (minionAA.Enabled)
            {
                CatchUpLogic();
            }

            if (LagFree(0))
            {
                SetMana();
            }

            if (E.IsReady())
            {
                LogicE();
                JungleE();
            }

            if (LagFree(1) && Q.IsReady() && !Player.IsWindingUp && !Player.IsDashing())
            {
                LogicQ();
            }

            if (LagFree(2) && LaneClear && farmQ.Enabled && Q.IsReady() && !Player.IsWindingUp && !Player.IsDashing())
            {
                FarmQ();
            }

            if (LagFree(4) && W.IsReady() && None && !Player.IsWindingUp && !Player.IsDashing())
            {
                LogicW();
            }

            if (autoR.Enabled && R.IsReady())
            {
                LogicR();
            }
        }

        private void RefreshLogic()
        {
            AllyR = null;

            foreach (var ally in GameObjects.AllyHeroes.Where(e => e.IsValidTarget(float.MaxValue, false) && !e.IsMe && e.HasBuff("kalistacoopstrikeally")))
            {
                AllyR = ally;
                break;
            }
        }

        private void SurvivalLogic()
        {
            if (E.IsReady() && Player.HealthPercent < 50 && deadE.Enabled)
            {
                var dmg = OktwCommon.GetIncomingDamage(Player);

                if (dmg > 0)
                {
                    if (Player.Health - dmg < Player.CountEnemyHeroesInRange(700) * Player.Level * 5)
                    {
                        CastE();
                    }
                    else if (Player.Health - dmg < Player.Level * 5)
                    {
                        CastE();
                    }
                }
            }

            if (R.IsReady() && AllyR.IsValidTarget(R.Range, false) && AllyR.HealthPercent < 50)
            {
                var dmg = OktwCommon.GetIncomingDamage(AllyR, 1);

                if (dmg > 0)
                {
                    if (AllyR.Health - dmg < AllyR.CountEnemyHeroesInRange(700) * AllyR.Level * 10)
                    {
                        R.Cast();
                    }
                    else if (AllyR.Health - dmg < AllyR.Level * 10)
                    {
                        R.Cast();
                    }
                }
            }
        }

        private void CatchUpLogic()
        {
            if (Combo && Orbwalker.GetTarget() == null && Orbwalker.CanAttack())
            {
                var minions = GameObjects.EnemyMinions.Where(e => e.InAutoAttackRange()).OrderByDescending(e => e.Health);

                if (minions.Count() > 0)
                {
                    Orbwalker.Attack(minions.First());
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                var poutput = Q.GetPrediction(t);
                var col = poutput.CollisionObjects;
                var cast = true;

                foreach (var colobj in col)
                {
                    if (Q.GetDamage(colobj) < colobj.Health)
                    {
                        cast = false;
                    }
                }

                var qDmg = OktwCommon.GetKsDamage(t, Q) + Player.GetAutoAttackDamage(t);
                var eDmg = GetEdmg(t);

                if (qDmg > t.Health && eDmg < t.Health && Player.Mana > QMANA + EMANA)
                {
                    CastQ(cast, t);
                }
                else if ((qDmg * 1.1) + eDmg > t.Health && eDmg < t.Health && Player.Mana > QMANA + EMANA && t.InAutoAttackRange())
                {
                    CastQ(cast, t);
                }
                else if (Combo && Player.Mana > QMANA + EMANA + RMANA)
                {
                    switch (qMode.Index)
                    {
                        case 0:
                            CastQ(cast, t);
                            break;
                        default:
                            {
                                if (!t.InAutoAttackRange() || CountMeleeInRange(400) > 0)
                                {
                                    CastQ(cast, t);
                                }
                                break;
                            }
                    }
                }
                else if (Harass && !t.InAutoAttackRange() && InHarassList(t) && !Player.IsUnderEnemyTurret() && Player.ManaPercent > qMana.Value)
                {
                    CastQ(cast, t);
                }

                if ((Combo || Harass) && Player.Mana > QMANA + EMANA + RMANA)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(Q.Range) && !OktwCommon.CanMove(e)))
                    {
                        CastQ(false, enemy);
                    }
                }
            }
        }

        private void LogicW()
        {
            if (dragonW.Enabled && !Orbwalker.GetTarget().IsValidTarget() && !Combo && Player.CountEnemyHeroesInRange(800) == 0)
            {
                if (countW > 0)
                {
                    var baronPos = new Vector3(5232, 10788, 0);

                    if (Player.Distance(baronPos) < W.Range)
                    {
                        W.Cast(baronPos);
                    }
                }

                if (countW == 0)
                {
                    var dragonPos = new Vector3(9919, 4475, 0);

                    if (Player.Distance(dragonPos) < W.Range)
                    {
                        W.Cast(dragonPos);
                    }
                    else
                    {
                        countW++;
                    }

                    return;
                }

                if (countW == 1)
                {
                    var redPos = new Vector3(8022, 4156, 0);

                    if (Player.Distance(redPos) < W.Range)
                    {
                        W.Cast(redPos);
                    }
                    else
                    {
                        countW++;
                    }

                    return;
                }

                if (countW == 2)
                {
                    var bluePos = new Vector3(11396, 7076, 0);

                    if (Player.Distance(bluePos) < W.Range)
                    {
                        W.Cast(bluePos);
                    }
                    else
                    {
                        countW++;
                    }

                    return;
                }

                if (countW > 2)
                {
                    countW = 0;
                }
            }
        }

        private void LogicE()
        {
            var count = 0;
            var outRange = 0;
            var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(E.Range - 50) && e.HealthPercent < 80);

            foreach (var minion in minions)
            {
                var eDmg = E.GetDamage(minion);

                if (minion.Health < eDmg - minion.HPRegenRate && eDmg > 0)
                {
                    if (GetEtime(minion) > 0.5 && HealthPrediction.GetPrediction(minion, 300) > Player.GetAutoAttackDamage(minion) && !minion.HasBuff("KindredRNoDeathBuff"))
                    {
                        count++;

                        if (!minion.InAutoAttackRange())
                        {
                            outRange++;
                        }

                        if (minionE.Enabled)
                        {
                            var minionName = minion.CharacterName.ToLower();

                            if (minionName.Contains("siege") || minionName.Contains("super"))
                            {
                                outRange++;
                            }
                        }
                    }
                }
            }

            var near700 = Player.CountEnemyHeroesInRange(700) == 0;

            foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(E.Range) && e.HasBuff("kalistaexpungemarker") && OktwCommon.ValidUlt(e)))
            {
                var eDmg = GetEdmg(target);

                if (target.Health < eDmg)
                {
                    CastE();
                }

                if (eDmg > 0 && count > 0 && killminE.Enabled)
                {
                    CastE();
                }

                if (GetEstacks(target) >= countE.Value && (GetEtime(target) > 1 || near700) && Player.Mana > EMANA + RMANA)
                {
                    CastE();
                }
            }

            if (Farm && count > 0 && farmE.Enabled)
            {
                if (outRange > 0)
                {
                    CastE();
                }

                if (count >= farmEcount.Value || (Player.IsUnderAllyTurret() && !Player.IsUnderEnemyTurret() && Player.Mana > QMANA + EMANA + RMANA))
                {
                    CastE();
                }
            }
        }

        private void LogicR()
        {
            if (AllyR.IsValidTarget(R.Range, false))
            {
                if (AllyR.Health < AllyR.CountEnemyHeroesInRange(600) * AllyR.Level * 30)
                {
                    R.Cast();
                }
            }
        }

        private void FarmQ()
        {
            var countMinion = 0;
            AIBaseClient bestMinion = null;

            foreach (var minion in GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range) && e.HealthPercent < 95 && Q.GetDamage(e) > e.Health))
            {
                var poutput = Q.GetPrediction(minion);
                var col = poutput.CollisionObjects;
                var kill = 0;

                if (col.Count == 0)
                {
                    continue;
                }

                foreach (var colobj in col)
                {
                    if (Q.GetDamage(colobj) > colobj.Health)
                    {
                        kill++;
                    }
                    else
                    {
                        kill = 0;
                        break;
                    }
                }

                if (kill > 0 && (countMinion == 0 || countMinion < kill + 1))
                {
                    countMinion = kill + 1;
                    bestMinion = minion;
                }
            }

            if (bestMinion != null && countMinion >= farmQcount.Value)
            {
                Q1.Cast(bestMinion);
            }
        }

        private void JungleE()
        {
            if (!jungleE.Enabled)
            {
                return;
            }

            var mobs = GameObjects.Jungle.Where(e => e.IsValidTarget(E.Range));

            if (mobs.Count() > 0)
            {
                var mob = mobs.First();

                if (mob.Health < GetEdmg(mob))
                {
                    CastE();
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

        private void CastE()
        {
            if (Game.Time - lastCastE < 0.4)
            {
                return;
            }

            E.Cast();
        }

        private void CastQ(bool cast, AIBaseClient t)
        {
            if (cast)
            {
                CastSpell(Q1, t);
            }
            else
            {
                CastSpell(Q, t);
            }
        }

        private int CountMeleeInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(range) && e.IsMelee).Count();
        }

        private float GetEdmg(AIBaseClient t)
        {
            var eDmg = E.GetDamage(t);

            if ((t as AIMinionClient)?.GetJungleType().HasFlag(JungleType.Legendary) ?? false)
            {
                eDmg *= 0.5f;
            }

            if (t.CharacterName == "Blitzcrank" && !t.HasBuff("manabarrier") && !t.HasBuff("manabarriercooldown"))
            {
                eDmg -= 0.3f * t.MaxMana;
            }

            eDmg -= t.HPRegenRate;
            eDmg -= t.PercentLifeStealMod * 0.005f * t.TotalAttackDamage;
            eDmg = eDmg * 0.01f * dmgE.Value;

            return eDmg;
        }

        private int GetEstacks(AIBaseClient t)
        {
            return t.GetBuffCount("kalistaexpungemarker");
        }

        private float GetEtime(AIBaseClient t)
        {
            return OktwCommon.GetPassiveTime(t, "kalistaexpungemarker");
        }

        private void drawText(string msg, AIBaseClient target, System.Drawing.Color color)
        {
            var wts = target.HPBarPosition;
            Drawing.DrawText(wts[0] - msg.Length * 5, wts[1] - 80, color, msg);
        }
    }
}
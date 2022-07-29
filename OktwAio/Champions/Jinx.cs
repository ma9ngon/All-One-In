using System;
using System.Linq;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using SebbyLib;
    using SharpDX;

    class Jinx : Base
    {
        private readonly MenuBool noti = new MenuBool("noti", "Show notification");
        private readonly MenuBool semi = new MenuBool("semi", "Semi-manual R target");
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);

        private readonly MenuBool autoQ = new MenuBool("autoQ", "Auto Q");
        private readonly MenuBool harassQ = new MenuBool("harassQ", "Harass Q");

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W");
        private readonly MenuBool harassW = new MenuBool("harassW", "Harass W");

        private readonly MenuBool autoE = new MenuBool("autoE", "Auto E on CC");
        private readonly MenuBool comboE = new MenuBool("comboE", "Auto E in Combo BETA");
        private readonly MenuBool agcE = new MenuBool("agcE", "Auto E anti gapcloser");
        private readonly MenuBool opsE = new MenuBool("opsE", "Auto E on specific spells");
        private readonly MenuBool telE = new MenuBool("telE", "Auto E teleport");

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R");
        private readonly MenuBool jungleR = new MenuBool("jungleR", "R Jungle stealer");
        private readonly MenuBool dragonR = new MenuBool("dragonR", "Dragon");
        private readonly MenuBool baronR = new MenuBool("baronR", "Baron");
        private readonly MenuSlider hitChangeR = new MenuSlider("hitchangeR", "Hit Chance R", 2, 0, 3);
        private readonly MenuKeyBind useR = new MenuKeyBind("useR", "OneKeyToCast R", Keys.T, KeyBindType.Press);
        private readonly MenuBool turretR = new MenuBool("turretR", "Don't R under enemy turret");

        private readonly MenuBool farmQout = new MenuBool("farmQout", "Q farm out range AA");
        private readonly MenuBool farmQ = new MenuBool("farmQ", "Q LaneClear");

        private float castTimeW = 0;
        private float grabTime = 0;
        private float dragonTime = 0;
        private float dragonDmg = 0;

        private bool FishBoneActive
        {
            get
            {
                return Player.HasBuff("JinxQ");
            }
        }

        public Jinx()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1500f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 3000f);

            W.SetSkillshot(0.6f, 60f, 3300f, true, SpellType.Line);
            E.SetSkillshot(1.2f, 100f, 1750f, false, SpellType.Circle);
            R.SetSkillshot(0.6f, 140f, 1700f, false, SpellType.Line);

            Local.Add(new Menu("draw", "Draw")
            {
                noti,
                semi,
                onlyRdy,
                qRange,
                wRange,
                eRange
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
                comboE,
                agcE,
                opsE,
                telE
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                new Menu("rJungleStealer", "R Jungle stealer")
                {
                    jungleR,
                    dragonR,
                    baronR
                },
                hitChangeR,
                useR,
                turretR
            });

            FarmMenu.Add(farmQout);
            FarmMenu.Add(farmQ);

            AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnBeforeAttack += Orbwalker_OnBeforeAttack;
        }

        private void AIBaseClient_OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.Type != GameObjectType.AIHeroClient)
            {
                return;
            }

            if (sender.IsMe)
            {
                if (args.SData.Name == "JinxWMissile")
                {
                    castTimeW = Game.Time;
                }
            }

            if (E.IsReady())
            {
                if (sender.IsEnemy && opsE.Enabled && sender.IsValidTarget(E.Range) && ShouldUseE(args.SData.Name))
                {
                    E.Cast(sender.PreviousPosition);
                }

                if (sender.IsAlly && args.SData.Name == "RocketGrab" && Player.Distance(sender.Position) < E.Range)
                {
                    grabTime = Game.Time;
                }
            }
        }

        private void AntiGapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!OktwCommon.CheckGapcloser(sender,args))
            {
                return;
            }

            if (agcE.Enabled && E.IsReady() && Player.Mana > EMANA + RMANA)
            {
                if (sender.IsValidTarget(E.Range))
                {
                    E.Cast(args.EndPosition);
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (qRange.Enabled)
            {
                if (FishBoneActive)
                {
                    Render.Circle.DrawCircle(Player.Position, 590f + Player.BoundingRadius, System.Drawing.Color.Cyan, 1);
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, GetBonudRange() - 29, System.Drawing.Color.Cyan, 1);
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

            if (noti.Enabled)
            {
                var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);

                if (t.IsValidTarget(2000) && W.GetDamage(t) > t.Health)
                {
                    Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "W can kill: " + t.CharacterName + " have: " + t.Health + " hp");
                    Drawing.DrawLine(Drawing.WorldToScreen(Player.Position), Drawing.WorldToScreen(t.Position), 3, System.Drawing.Color.Yellow);
                }
                else if (R.IsReady() && t.IsValidTarget() && GetUltHitDamage(t) > t.Health)
                {
                    Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.CharacterName + " have: " + t.Health + " hp");
                    Drawing.DrawLine(Drawing.WorldToScreen(Player.Position), Drawing.WorldToScreen(t.Position), 5, System.Drawing.Color.Red);
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (R.IsReady())
            {
                if (useR.Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                    if (t.IsValidTarget())
                    {
                        R.Cast(t);
                    }
                }

                if (jungleR.Enabled)
                {
                    LogicKsJungle();
                }
            }

            if (LagFree(0))
            {
                SetMana();

                W.Delay = 0.6f - Math.Max(0, Math.Min(0.2f, 0.02f * ((Player.AttackSpeedMod - 1) / 0.25f)));
            }

            if (E.IsReady())
            {
                LogicE();
            }

            if (LagFree(2) && autoQ.Enabled && Q.IsReady())
            {
                LogicQ();
            }

            if (LagFree(3) && autoW.Enabled && W.IsReady() && !Player.IsWindingUp)
            {
                LogicW();
            }

            if (LagFree(4) && R.IsReady())
            {
                LogicR();
            }
        }

        private void Orbwalker_OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (!Q.IsReady() || !autoQ.Enabled || !FishBoneActive)
            {
                return;
            }

            var t = args.Target as AIHeroClient;

            if (t != null)
            {
                var realDistance = GetRealDistance(t) - 50;
                if (Combo && (realDistance < GetRealPowPowRange(t) || (Player.Mana < RMANA + 40 && Player.GetAutoAttackDamage(t) * 3 < t.Health)))
                {
                    Q.Cast();
                }
                else if (Harass && harassQ.Enabled && (realDistance > GetBonudRange() || realDistance < GetRealPowPowRange(t) || Player.Mana < WMANA + WMANA + EMANA + RMANA))
                {
                    Q.Cast();
                }
            }

            var minion = args.Target as AIMinionClient;

            if (Farm && minion != null)
            {
                var realDistance = GetRealDistance(minion);

                if (realDistance < GetRealPowPowRange(minion) || Player.ManaPercent < LCmana.Value)
                {
                    Q.Cast();
                }
            }
        }

        private void LogicQ()
        {
            var orbT = Orbwalker.GetTarget();

            if (Farm && !FishBoneActive && !Player.IsWindingUp && orbT == null && Orbwalker.CanAttack() && farmQout.Enabled && Player.Mana > WMANA + EMANA + RMANA + 20)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(e => e.IsValidTarget(GetBonudRange() + 30) && !e.InAutoAttackRange() && GetRealPowPowRange(e) < GetRealDistance(e) && GetBonudRange() < GetRealDistance(e)))
                {
                    var hpPred = HealthPrediction.GetPrediction(minion, 400);

                    if (hpPred < Player.GetAutoAttackDamage(minion) * 1.1 && hpPred > 5)
                    {
                        Orbwalker.ForceTarget = minion;
                        Q.Cast();

                        return;
                    }
                }
            }

            var t = TargetSelector.GetTarget(GetBonudRange() + 60, DamageType.Physical);

            if (t.IsValidTarget())
            {
                if (!FishBoneActive && (!t.InAutoAttackRange() || t.CountEnemyHeroesInRange(250) > 2) && orbT == null)
                {
                    var distance = GetRealDistance(t);

                    if (Combo && (Player.Mana > WMANA + RMANA + 20 || Player.GetAutoAttackDamage(t) * 3 > t.Health))
                    {
                        Q.Cast();
                    }
                    else if (Harass && !Player.IsWindingUp && Orbwalker.CanAttack() && harassQ.Enabled && !Player.IsUnderEnemyTurret() && Player.Mana > WMANA + EMANA + RMANA + 40 && distance < GetBonudRange() + t.BoundingRadius + Player.BoundingRadius)
                    {
                        Q.Cast();
                    }
                }
            }
            else if (!FishBoneActive && Combo && Player.Mana > WMANA + RMANA + 40 && Player.CountEnemyHeroesInRange(2000) > 0)
            {
                Q.Cast();
            }
            else if (FishBoneActive && Combo && Player.Mana < WMANA + RMANA + 40)
            {
                Q.Cast();
            }
            else if (FishBoneActive && Combo && Player.CountEnemyHeroesInRange(2000) == 0)
            {
                Q.Cast();
            }
            else if (FishBoneActive && Farm)
            {
                Q.Cast();
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && e.Distance(Player) > GetBonudRange()))
                {
                    var comboDmg = OktwCommon.GetKsDamage(enemy, W);

                    if (R.IsReady() && Player.Mana > WMANA + RMANA + 40)
                    {
                        comboDmg += GetUltHitDamage(enemy);
                    }

                    if (comboDmg > enemy.Health && OktwCommon.ValidUlt(enemy))
                    {
                        CastSpell(W, enemy);
                        return;
                    }
                }

                if (Player.CountEnemyHeroesInRange(GetBonudRange()) == 0)
                {
                    if (Combo && Player.Mana > WMANA + RMANA + 20)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && GetRealDistance(e) > GetBonudRange()).OrderBy(e => e.Health))
                        {
                            CastSpell(W, enemy);
                        }
                    }
                    else if (Harass && Player.Mana > WMANA + WMANA + EMANA + RMANA + 80 && OktwCommon.CanHarass() && harassW.Enabled)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && InHarassList(e)))
                        {
                            CastSpell(W, enemy);
                        }
                    }
                }

                if (!None && Player.Mana > WMANA + RMANA  && Player.CountEnemyHeroesInRange(GetRealPowPowRange(t)) == 0)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && !OktwCommon.CanMove(e)))
                    {
                        W.Cast(enemy);
                    }
                }
            }
        }

        private void LogicE()
        {
            if (Player.Mana > EMANA + RMANA && autoE.Enabled && Game.Time - grabTime > 1)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(E.Range + 50) && !OktwCommon.CanMove(e)))
                {
                    E.Cast(enemy);
                    return;
                }

                if (!LagFree(1))
                {
                    return;
                }

                if (telE.Enabled)
                {
                    var trapPos = OktwCommon.GetTrapPos(E.Range);

                    if (!trapPos.IsZero)
                    {
                        E.Cast(trapPos);
                    }
                }

                if (Combo && Player.IsMoving && comboE.Enabled && Player.Mana > WMANA + EMANA + RMANA)
                {
                    var t = TargetSelector.GetTarget(E.Range, DamageType.Physical);

                    if (t.IsValidTarget(E.Range) && E.GetPrediction(t).CastPosition.Distance(t.Position) > 200)
                    {
                        E.CastIfWillHit(t, 2);

                        if (t.HasBuffOfType(BuffType.Slow))
                        {
                            CastSpell(E, t);
                        }

                        if (OktwCommon.IsMovingInSameDirection(Player, t))
                        {
                            CastSpell(E, t);
                        }
                    }
                }
            }
        }

        private void LogicR()
        {
            if (Player.IsUnderEnemyTurret() && turretR.Enabled)
            {
                return;
            }

            if (Game.Time - castTimeW > 0.9 && autoR.Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(R.Range) && OktwCommon.ValidUlt(e)))
                {
                    var predictedHealth = enemy.Health - OktwCommon.GetIncomingDamage(enemy);
                    var dmgR = GetUltHitDamage(enemy);

                    if (dmgR > predictedHealth && !OktwCommon.IsSpellHeroCollision(enemy, R) && GetRealDistance(enemy) > GetBonudRange() + 200)
                    {
                        if (GetRealDistance(enemy) > GetBonudRange() + 300 + enemy.BoundingRadius && enemy.CountAllyHeroesInRange(500) == 0 && Player.CountEnemyHeroesInRange(400) == 0)
                        {
                            CastR(enemy);
                        }
                        else if (enemy.CountEnemyHeroesInRange(200) > 2)
                        {
                            R.Cast(enemy, false, true);
                        }
                    }
                }
            }
        }

        private void LogicKsJungle()
        {
            var mobs = GameObjects.GetJungles(Player.PreviousPosition, float.MaxValue);

            foreach (var mob in mobs)
            {
                if (mob.Health < mob.MaxHealth
                    && ((dragonR.Enabled && mob.CharacterName.Contains("SRU_Dragon_")) || (baronR.Enabled && mob.CharacterName == "SRU_Baron"))
                    && mob.CountAllyHeroesInRange(1000) == 0
                    && mob.Distance(Player.Position) > 1000)
                {
                    if (dragonDmg == 0)
                    {
                        dragonDmg = mob.Health;
                    }

                    if (Game.Time - dragonTime > 4)
                    {
                        if (dragonDmg - mob.Health > 0)
                        {
                            dragonDmg = mob.Health;
                        }

                        dragonTime = Game.Time;
                    }
                    else
                    {
                        var dmgSec = (dragonDmg - mob.Health) * (Math.Abs(dragonTime - Game.Time) / 4);

                        if (dragonDmg - mob.Health > 0)
                        {
                            var timeTravel = GetUltTravalTime(Player, R.Speed, R.Delay, mob.Position);
                            var timeR = (mob.Health - GetUltHitDamage(mob, true)) / (dmgSec / 4);

                            if (timeTravel > timeR)
                            {
                                R.Cast(mob.Position);
                            }
                        }
                        else
                        {
                            dragonDmg = mob.Health;
                        }
                    }
                }
            }
        }

        private void SetMana()
        {
            if ((manaDisable.Enabled && Combo) || Player.ManaPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = 20;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
            {
                RMANA = WMANA - Player.PARRegenRate * W.Instance.Cooldown;
            }
            else
            {
                RMANA = R.Instance.ManaCost;
            }
        }

        private void CastR(AIHeroClient target)
        {
            switch (hitChangeR.Value)
            {
                case 0:
                    {
                        R.Cast(R.GetPrediction(target).CastPosition);
                        break;
                    }
                case 1:
                    {
                        R.Cast(target);
                        break;
                    }
                case 2:
                    {
                        CastSpell(R, target);
                        break;
                    }
                case 3:
                    {
                        var waypoints = target.GetWaypoints();

                        if (Player.Distance(waypoints.Last<Vector2>().ToVector3()) - Player.Distance(target.Position) > 400)
                        {
                            CastSpell(R, target);
                        }
                        break;
                    }
            }
        }

        private float GetBonudRange()
        {
            return 670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level;
        }

        private float GetRealDistance(AIBaseClient target)
        {
            return Player.PreviousPosition.Distance(Prediction.GetPrediction(target, 0.05f).CastPosition) + Player.BoundingRadius + target.BoundingRadius;
        }

        private float GetRealPowPowRange(GameObject target)
        {
            return 620f + Player.BoundingRadius + target.BoundingRadius;
        }

        private float GetUltHitDamage(AIBaseClient target, bool area = false)
        {
            var percent = 0.1f + Math.Max(0, Math.Min(0.9f, Player.Distance(target) / 100 * 0.06f));
            var damage = R.GetDamage(target);

            if (area)
            {
                damage *= 0.8f;
            }

            damage *= percent;

            return damage;
        }

        private float GetUltTravalTime(AIHeroClient source, float speed, float delay, Vector3 targetPos)
        {
            var distance = Vector3.Distance(source.PreviousPosition, targetPos);
            var missileSpeed = speed;

            if (source.CharacterName == "Jinx" && distance > 1350)
            {
                var accelerationrate = 0.3f; //= (1500f - 1350f) / (2200 - speed), 1 unit = 0.3units/second
                var acceldifference = distance - 1350f;

                if (acceldifference > 150f) //it only accelerates 150 units
                {
                    acceldifference = 150f;
                }

                var difference = distance - 1500f;

                missileSpeed = (1350f * speed + acceldifference * (speed + accelerationrate * acceldifference) + difference * 2200f) / distance;
            }

            return distance / missileSpeed + delay;
        }

        private bool ShouldUseE(string spellName)
        {
            switch (spellName)
            {
                case "ThreshQ":
                    return true;
                case "KatarinaR":
                    return true;
                case "AlZaharNetherGrasp":
                    return true;
                case "GalioIdolOfDurand":
                    return true;
                case "LuxMaliceCannon":
                    return true;
                case "MissFortuneBulletTime":
                    return true;
                case "RocketGrabMissile":
                    return true;
                case "CaitlynPiltoverPeacemaker":
                    return true;
                case "EzrealTrueshotBarrage":
                    return true;
                case "InfiniteDuress":
                    return true;
                case "VelkozR":
                    return true;
            }
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.Utility;
    using SebbyLib;
    using SharpDX;

    class Caitlyn : Base
    {
        private readonly MenuBool noti = new MenuBool("noti", "Show notification & line");
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);

        private readonly MenuBool autoQ2 = new MenuBool("autoQ2", "Auto Q");
        private readonly MenuBool autoQ = new MenuBool("autoQ", "Reduce Q use");
        private readonly MenuBool aoeQ = new MenuBool("aoeQ", "Q aoe");
        private readonly MenuBool slowQ = new MenuBool("slowQ", "Q slow");

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W on hard CC");
        private readonly MenuBool telW = new MenuBool("telW", "Auto W teleport");
        private readonly MenuBool forceW = new MenuBool("forceW", "Force W before E");
        private readonly MenuBool bushW = new MenuBool("bushW", "Auto W bush after enemy enter");
        private readonly MenuBool bushW2 = new MenuBool("bushW2", "Auto W bush and turret if full ammo");
        private readonly MenuBool spellW = new MenuBool("spellW", "W on special spell detection");
        private readonly MenuList modeGCW = new MenuList("modeGCW", "Gap Closer position mode", new[] { "Dash end position", "My hero position" });
        private readonly List<MenuBool> championsGCW = new List<MenuBool>();

        private readonly MenuBool autoE = new MenuBool("autoE", "Auto E");
        private readonly MenuBool hitchanceE = new MenuBool("hitchanceE", "Auto E dash and immobile target");
        private readonly MenuBool harassEQ = new MenuBool("harassEQ", "TRY E + Q");
        private readonly MenuBool ksEQ = new MenuBool("ksEQ", "Ks E + Q + AA");
        private readonly MenuKeyBind useE = new MenuKeyBind("useE", "Dash E HotKeySmartcast", Keys.Z, KeyBindType.Press);
        private readonly MenuList modeGCE = new MenuList("modeGCE", "Gap Closer position mode", new[] { "Dash end position", "Cursor position", "Enemy position" }, 2);
        private readonly List<MenuBool> championsGCE = new List<MenuBool>();

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R KS");
        private readonly MenuSlider colR = new MenuSlider("colR", "R collision width [400]", 400, 1, 1000);
        private readonly MenuSlider rangeR = new MenuSlider("rangeR", "R minimum range [1000]", 1000, 1, 1500);
        private readonly MenuSlider rangeR2 = new MenuSlider("rangeR2", "R when no enemies in range [0 = disabled]", 1000, 0, 1500);
        private readonly MenuKeyBind useR = new MenuKeyBind("useR", "Semi-manual cast R key", Keys.T, KeyBindType.Press);
        private readonly MenuBool turretR = new MenuBool("turretR", "Don't R under enemy turret");

        private readonly MenuBool farmQ = new MenuBool("farmQ", "Lane clear Q");

        private float castTimeQ = 0;

        private static string[] Spells =
        {
            "katarinar","drain","consume","absolutezero", "staticfield","reapthewhirlwind","jinxw","jinxr","shenstandunited","threshe","threshrpenta","threshq","meditate","caitlynpiltoverpeacemaker", "volibearqattack",
            "cassiopeiapetrifyinggaze","ezrealtrueshotbarrage","galioidolofdurand","luxmalicecannon", "missfortunebullettime","infiniteduress","alzaharnethergrasp","lucianq","velkozr","rocketgrabmissile"
        };

        public Caitlyn()
        {
            Q = new Spell(SpellSlot.Q, 1200f);
            W = new Spell(SpellSlot.W, 800f);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 3500f);

            Q.SetSkillshot(0.63f, 60f, 2200f, false, SpellType.Line);
            W.SetSkillshot(1.5f, 20f, float.MaxValue, false, SpellType.Circle);
            E.SetSkillshot(0.15f, 70f, 1600f, true, SpellType.Line);
            R.SetSkillshot(0.7f, 200f, 1500f, false, SpellType.Circle);

            Local.Add(new Menu("draw", "Draw")
            {
                noti,
                onlyRdy,
                qRange,
                wRange,
                eRange,
                rRange
            });

            Local.Add(new Menu("qConfig", "Q Config")
            {
                autoQ2,
                autoQ,
                aoeQ,
                slowQ
            });

            var gcwList = new Menu("gcwList", "Cast on enemy:");
            var gceList = new Menu("gceList", "Cast on enemy:");

            foreach(var enemy in GameObjects.EnemyHeroes)
            {
                var championGCW = new MenuBool("championGCW" + enemy.CharacterName, enemy.CharacterName);
                var championGCE = new MenuBool("championGCE" + enemy.CharacterName, enemy.CharacterName);

                championsGCW.Add(championGCW);
                championsGCE.Add(championGCE);

                gcwList.Add(championGCW);
                gceList.Add(championGCE);
            }

            Local.Add(new Menu("wConfig", "W Config")
            {
                autoW,
                telW,
                forceW,
                bushW,
                bushW2,
                spellW,
                new Menu("gcw", "W Gap Closer")
                {
                    modeGCW,
                    gcwList
                }
            });

            Local.Add(new Menu("eConfig", "E Config")
            {
                autoE,
                hitchanceE,
                harassEQ,
                ksEQ,
                useE,
                new Menu("gce", "E Gap Closer")
                {
                    modeGCE,
                    gceList
                }
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                colR,
                rangeR,
                rangeR2,
                useR,
                turretR
            });

            FarmMenu.Add(farmQ);

            AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void AIBaseClient_OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (args.SData.Name == "CaitlynPiltoverPeacemaker" || args.SData.Name == "CaitlynEntrapment"))
            {
                castTimeQ = Game.Time;
            }

            if (!spellW.Enabled || !sender.IsValidTarget(W.Range) || sender.Type != GameObjectType.AIHeroClient || !W.IsReady())
            {
                return;
            }

            var foundSpell = Spells.Find(e => args.SData.Name.ToLower() == e);

            if (foundSpell != null)
            {
                W.Cast(sender.Position);
            }
        }

        private void AntiGapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (Player.Mana > WMANA + RMANA)
            {
                if (championsGCE.Any(e => e.Enabled && e.Name == "championGCE" + sender.CharacterName) && E.IsReady() && sender.IsValidTarget(E.Range))
                {
                    switch (modeGCE.Index)
                    {
                        case 0:
                            E.Cast(args.EndPosition);
                            break;
                        case 1:
                            E.Cast(Game.CursorPos);
                            break;
                        default:
                            E.Cast(sender.PreviousPosition);
                            break;
                    }
                }
                else if (championsGCW.Any(e => e.Enabled && e.Name == "championGCW" + sender.CharacterName) && W.IsReady() && sender.IsValidTarget(W.Range))
                {
                    switch (modeGCW.Index)
                    {
                        case 0:
                            W.Cast(args.EndPosition);
                            break;
                        default:
                            W.Cast(Player.PreviousPosition);
                            break;
                    }
                }
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

            if (noti.Enabled)
            {
                var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);

                if (t.IsValidTarget() && R.IsReady())
                {
                    if (R.GetDamage(t) > t.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.CharacterName + " have: " + t.Health + " hp");
                        Drawing.DrawLine(Drawing.WorldToScreen(t.Position), Drawing.WorldToScreen(Player.Position), 10, System.Drawing.Color.Yellow);
                    }
                }

                var tq = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                if (tq.IsValidTarget())
                {
                    if (Q.GetDamage(tq) > tq.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q can kill: " + tq.CharacterName + " have: " + tq.Health + " hp");
                    }
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsRecalling())
            {
                return;
            }

            if (useR.Active && R.IsReady())
            {
                var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);

                if (t.IsValidTarget())
                {
                    R.CastOnUnit(t);
                }
            }

            if (LagFree(0))
            {
                SetMana();
            }

            if (LagFree(1) && E.IsReady() && Orbwalker.CanMove(40, false))
            {
                LogicE();
            }

            var orbT = Orbwalker.GetTarget() as AIHeroClient;

            if (orbT != null && Player.GetAutoAttackDamage(orbT) * 2 > orbT.Health)
            {
                return;
            }

            if (LagFree(2) && W.IsReady())
            {
                LogicW();
            }

            if (LagFree(3) && Q.IsReady() && Orbwalker.CanMove(40,false) && autoQ2.Enabled)
            {
                LogicQ();
            }

            if (LagFree(4) && R.IsReady() && autoR.Enabled && Game.Time - castTimeQ > 1)
            {
                LogicR();
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.W)
            {
                if (GameObjects.EffectEmitters.Any(e => e.IsValid && e.Position.Distance(args.EndPosition) < 300 && e.Name.ToLower().Contains("yordleTrap_idle_green".ToLower())))
                {
                    args.Process = false;
                }
            }

            if (args.Slot == SpellSlot.E && Player.Mana > WMANA + RMANA && forceW.Enabled)
            {
                W.Cast(Player.Position.Extend(args.EndPosition, Player.Distance(args.EndPosition) + 50));
                DelayAction.Add(10, () => E.Cast(args.EndPosition));
            }
        }

        private void LogicQ()
        {
            if (Combo && Player.IsWindingUp)
            {
                return;
            }

            var t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                if (GetRealDistance(t) > GetBonusRange() + 250 && !t.InAutoAttackRange() && OktwCommon.GetKsDamage(t, Q) > t.Health && Player.CountEnemyHeroesInRange(400) == 0)
                {
                    CastSpell(Q, t);
                }
                else if (Combo && Player.Mana > QMANA + EMANA + RMANA + 10 && Player.CountEnemyHeroesInRange(GetBonusRange() + 100 + t.BoundingRadius) == 0 && !autoQ.Enabled)
                {
                    CastSpell(Q, t);
                }

                if ((Combo || Harass) && Player.Mana > QMANA + RMANA && Player.CountEnemyHeroesInRange(400) == 0)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(Q.Range) && (!OktwCommon.CanMove(e) || e.HasBuff("caitlynyordletrapinternal"))))
                    {
                        Q.Cast(enemy);
                    }

                    if (Player.CountEnemyHeroesInRange(GetBonusRange()) == 0 && OktwCommon.CanHarass())
                    {
                        if (slowQ.Enabled && t.HasBuffOfType(BuffType.Slow))
                        {
                            Q.Cast(t);
                        }

                        if (aoeQ.Enabled)
                        {
                            Q.CastIfWillHit(t, 2);
                        }
                    }
                }
            }
            else if (FarmSpells && farmQ.Enabled)
            {
                var allMinionsQ = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range)).ToList();
                var farmQ = Q.GetLineFarmLocation(allMinionsQ);

                if (farmQ.MinionsHit >= LCminions.Value)
                {
                    Q.Cast(farmQ.Position);
                }
            }
        }

        private void LogicW()
        {
            if (Player.Mana > WMANA + RMANA)
            {
                if (autoW.Enabled)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && !OktwCommon.CanMove(e) && !e.HasBuff("caitlynyordletrapinternal")))
                    {
                        W.Cast(enemy);
                    }
                }

                if (telW.Enabled)
                {
                    var trapPos = OktwCommon.GetTrapPos(W.Range);

                    if (!trapPos.IsZero)
                    {
                        W.Cast(trapPos);
                    }
                }

                if (!Orbwalker.CanMove(40, false))
                {
                    return;
                }

                if ((int)(Game.Time * 10) % 2 == 0 && bushW2.Enabled)
                {
                    if (Player.Spellbook.GetSpell(SpellSlot.W).Ammo == new[] { 0, 3, 3, 4, 4, 5 }[W.Level] && Player.CountEnemyHeroesInRange(1000) == 0)
                    {
                        var points = OktwCommon.CirclePoints(8, W.Range, Player.Position);

                        foreach (var point in points)
                        {
                            if (NavMesh.IsWallOfType(point, CollisionFlags.Grass, 0) || point.IsUnderEnemyTurret())
                            {
                                if (!OktwCommon.CirclePoints(8, 150, point).Any(e => e.IsWall()))
                                {
                                    W.Cast(point);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LogicE()
        {
            if (Combo && Player.IsWindingUp)
            {
                return;
            }

            if (autoE.Enabled)
            {
                var t = TargetSelector.GetTarget(E.Range, DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var positionT = Player.PreviousPosition - (t.Position - Player.PreviousPosition);

                    if (Player.Position.Extend(positionT, 400).CountEnemyHeroesInRange(700) < 2)
                    {
                        var qDmg = Q.GetDamage(t);
                        var eDmg = E.GetDamage(t);

                        if (ksEQ.Enabled && qDmg + eDmg + Player.GetAutoAttackDamage(t) > t.Health && Player.Mana > QMANA + EMANA)
                        {
                            CastSpell(E, t);
                        }
                        else if ((Combo || Harass) && harassEQ.Enabled && Player.Mana > QMANA + EMANA + RMANA)
                        {
                            CastSpell(E, t);
                        }
                    }

                    if (Player.Mana > EMANA + RMANA)
                    {
                        if (hitchanceE.Enabled)
                        {
                            E.CastIfHitchanceMinimum(t, HitChance.Immobile);
                        }

                        if (Player.HealthPercent < 30)
                        {
                            if (GetRealDistance(t) < 500)
                            {
                                E.Cast(t);
                            }

                            if (Player.CountEnemyHeroesInRange(250) > 0)
                            {
                                E.Cast(t);
                            }
                        }
                    }
                }
            }

            if (useE.Active)
            {
                E.Cast(Player.PreviousPosition - (Game.CursorPos - Player.PreviousPosition));
            }
        }

        private void LogicR()
        {
            if (turretR.Enabled && Player.IsUnderEnemyTurret())
            {
                return;
            }

            if (rangeR2.Value != 0 && Player.CountEnemyHeroesInRange(rangeR2.Value) > 0)
            {
                return;
            }

            var cast = false;

            foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(R.Range) && Player.Distance(e.Position) > rangeR.Value && e.CountEnemyHeroesInRange(colR.Value) == 1 && e.CountAllyHeroesInRange(500) == 0 && OktwCommon.ValidUlt(e)))
            {
                if (OktwCommon.GetKsDamage(target, R) > target.Health)
                {
                    cast = true;

                    var output = R.GetPrediction(target);
                    var direction = (output.CastPosition.ToVector2() - Player.Position.ToVector2()).Normalized();
                    var enemies = GameObjects.EnemyHeroes.Where(e => e.IsValidTarget()).ToList();

                    foreach (var enemy in enemies)
                    {
                        if (enemy.Name == target.Name || !cast)
                        {
                            continue;
                        }

                        var prediction = R.GetPrediction(enemy);
                        var predictedPosition = prediction.CastPosition;
                        var v = output.CastPosition - Player.PreviousPosition;
                        var w = predictedPosition - Player.PreviousPosition;
                        var c1 = Vector3.Dot(w, v);
                        var c2 = Vector3.Dot(v, v);
                        var b = c1 / c2;
                        var pb = Player.PreviousPosition + ((float)b * v);
                        var length = Vector3.Distance(predictedPosition, pb);

                        if (length < colR.Value + enemy.BoundingRadius && Player.Distance(predictedPosition) < Player.Distance(target.PreviousPosition))
                        {
                            cast = false;
                        }
                    }

                    if (cast)
                    {
                        R.CastOnUnit(target);
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

        private float GetBonusRange()
        {
            return 720f + Player.BoundingRadius;
        }

        private float GetRealDistance(GameObject target)
        {
            return Player.PreviousPosition.Distance(target.Position) + Player.BoundingRadius + target.BoundingRadius;
        }
    }
}
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
    class Viego : Base
    {
        public bool attackNow = true;

        private readonly MenuBool autoQ = new MenuBool("autoQ", "Auto Q", true);
        private readonly MenuBool harassQ = new MenuBool("harassQ", "Harass Q", true);

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W", true);
        private readonly MenuBool harassW = new MenuBool("harassW", "Harass W", true);

        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R", true);
        private readonly MenuBool PassivaCast = new MenuBool("PassiveCast", "Passive cast spells", true);

        private readonly MenuBool ComboInfo = new MenuBool("ComboInfo", "R killable info", true);
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);

        private readonly MenuBool sheen = new MenuBool("sheen", "Sheen logic", true);
        private readonly MenuBool AApriority = new MenuBool("AApriority", "AA priority over spell", true);

        private readonly MenuBool farmW = new MenuBool("farmW", "LaneClear W", true);
        private readonly MenuBool farmE = new MenuBool("farmE", "LaneClear E", true);

        private readonly MenuBool jungleQ = new MenuBool("jungleQ", "Jungle clear Q", true);
        private readonly MenuBool jugnleW = new MenuBool("jungleW", "Jungle clear W", true);

        public Viego()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 500);

            Q.SetSkillshot(0.4f, 70f, float.MaxValue, false, SpellType.Line);
            W.SetSkillshot(0f, 70f, 1500f, true, SpellType.Line);
            R.SetSkillshot(0.6f, 270f, float.MaxValue, false, SpellType.Circle);

            W.SetCharged("ViegoW", "ViegoW", 400, 850, 1f);

            Local.Add(new Menu("draw", "Draw")
            {
                ComboInfo,
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
                autoW,
                harassW

            });

            Local.Add(new Menu("Roption", "R option")
            {
                autoR

            });

            Local.Add(new Menu("sheen", "Sheen logic")
            {
                sheen

            });

            Local.Add(new Menu("AApriority", "AA priority over spell")
            {
                AApriority

            });

            FarmMenu.Add(farmW);
            FarmMenu.Add(farmE);
            FarmMenu.Add(jungleQ);
            FarmMenu.Add(jugnleW);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnBeforeAttack += BeforeAttack;
            Orbwalker.OnAfterAttack += Orbwalker_OnAfterAttack;
            AIBaseClient.OnBuffAdd += OnBuffAdd;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private void OnBuffAdd(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            if (sender.IsMe)
            {

                Console.WriteLine("buffname: " + args.Buff.Name);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            //if (AGC.Enabled && E.IsReady() && Player.Mana > RMANA + EMANA)
            //{
            //    var Target = (AIHeroClient)gapcloser.Sender;
            //    if (Target.IsValidTarget(E.Range))
            //    {
            //        E.Cast(Target, true);
            //        Program.debug("E AGC");
            //    }
            //}
            //return;
        }

        private void Orbwalker_OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            attackNow = true;
            if (Program.LaneClear && W.IsReady() && FarmSpells)
            {
                var minions = GameObjects.GetMinions(Player.Position, 650);

                if (minions.Count > 0)
                {
                    if (farmW.Enabled && minions.Count > 1)
                        W.Cast();
                }
            }
        }

        private void BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            attackNow = false;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            var viegosoul = ObjectManager.Get<AIMinionClient>()
                .FirstOrDefault(minion => minion.Team == GameObjectTeam.Neutral
                                          && minion.CharacterName == "ViegoSoul" && minion.IsHPBarRendered
                                          && minion.IsValidTarget(600));
            if (viegosoul != null && (Player.Position.CountEnemyHeroesInRange(1000) == 0 ||
                                      Player.HealthPercent < 25))
            {
                Orbwalker.SetOrbwalkerPosition(viegosoul.Position);
            }
            else
            {
                Orbwalker.SetOrbwalkerPosition(new SharpDX.Vector3());
            }

            if (!Player.CharacterName.Contains("Viego"))
            {
                if (!Program.LagFree(0))
                    return;
                var t = TargetSelector.GetTarget(R.Range + R.Width, DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var pred = Prediction.GetPrediction(t, 0.4f).CastPosition;
                    if (Player.InventoryItems.Count() == 2) // empty
                    {
                        Program.CastSpell(R, t);
                    }
                    else if (PassivaCast.Enabled)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            var spell = Player.Spellbook.Spells[i];
                            if (spell.SData != null && ObjectManager.Player.Spellbook.CanUseSpell(spell.Slot) ==
                                SpellState.Ready)
                            {
                                if (spell.SData.TargetingType == SpellDataTargetType.Target ||
                                    spell.SData.TargetingType == SpellDataTargetType.Self)
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot, t, true);
                                }
                                else if (spell.SData.TargetingType != SpellDataTargetType.Self &&
                                         spell.SData.TargetingType != SpellDataTargetType.SelfAoe)
                                {
                                    if (spell.SData.LineWidth > 0 || spell.SData.LineWidth > 0)
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(spell.Slot, pred, true);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Player.InventoryItems.Count() == 2) // empty
                    {
                        R.Cast(Game.CursorPos);
                    }
                }

                return;
            }


            if (Program.LagFree(0))
            {
                Jungle();
            }

            if (!Orbwalker.CanMove(50, true))
                return;

            if (Program.LagFree(2) && Q.IsReady() && autoQ.Enabled)
                LogicQ();

            if (Program.LagFree(4) && R.IsReady())
                LogicR();

            if (Program.LagFree(3) && W.IsReady() && autoW.Enabled)
                LogicW();
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (t.IsValidTarget())
            {
                var qDmg = OktwCommon.GetKsDamage(t, Q);
                var eDmg = E.GetDamage(t);
                if (t.IsValidTarget(W.Range) && qDmg + eDmg > t.Health)
                    Program.CastSpell(Q, t);
                else if (Program.Combo)
                    Program.CastSpell(Q, t);
                else if ((Program.Harass) && harassQ.Enabled && !Player.IsUnderEnemyTurret())
                    Program.CastSpell(Q, t);
                else if ((Program.Combo || Program.Harass))
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy =>
                        enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy, true);
                }
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.ChargedMaxRange, DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (!Player.HasBuff("ViegoW") && Program.Combo)
                {
                    var prediction = W.GetPrediction(t);

                    if (prediction.Hitchance >= HitChance.Low && prediction.Hitchance <= HitChance.VeryHigh)
                    {
                        W.StartCharging();
                    }

                    return;
                }

                var wDmg = OktwCommon.GetKsDamage(t, W);
                if (wDmg + Q.GetDamage(t) > t.Health)
                {
                    Program.CastSpell(W, t);
                }
                else if (Program.Combo)
                {
                    Program.CastSpell(W, t);
                }
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear)
            {
                var mobs = GameObjects.GetJungles(600);

                if (mobs.Count > 0)
                {
                    var mob = mobs.First();

                    if (jungleQ.Enabled && Q.IsReady())
                    {
                        Q.Cast(mob.Position);
                        return;
                    }
                    else if (jugnleW.Enabled && W.IsReady())
                        {
                        W.Cast(mob.Position);
                        return;
                    }
                }
            }
        }

        private void LogicR()
        {
            if (autoR.Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(target =>
                    target.IsValidTarget(R.Range + R.Width) && OktwCommon.ValidUlt(target)))
                {
                    var dmgR = OktwCommon.GetKsDamage(target, R, true) + 3 * Player.GetAutoAttackDamage(target);

                    if (dmgR > target.Health)
                    {
                        Program.CastSpell(R, target);
                    }
                }
            }
        }

        private void drawText(string msg, AIBaseClient Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero.Position);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1], color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (qRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (Q.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                }
                else
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }

            if (wRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (W.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
                }
                else
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }

            if (eRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (E.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
                }
                else
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }

            if (rRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (R.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
                }
                else
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }
    }
}
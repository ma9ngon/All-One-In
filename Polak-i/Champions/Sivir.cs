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

    class Sivir : Base
    {
        private readonly MenuBool notif = new MenuBool("notif", "Notification (timers)");
        private readonly MenuBool noti = new MenuBool("noti", "Show KS notification");
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells");
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);

        private readonly MenuBool farmQ = new MenuBool("farmQ", "Lane clear Q");
        private readonly MenuBool farmW = new MenuBool("farmW", "Lane clear W");

        private readonly MenuBool jungleQ = new MenuBool("jungleQ", "Jungle clear Q");
        private readonly MenuBool jungleW = new MenuBool("jungleW", "Jungle clear W");

        private readonly MenuBool autoQ = new MenuBool("autoQ", "Auto Q");
        private readonly MenuBool harassW = new MenuBool("harassW", "Harass W");
        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R");

        private readonly List<MenuBool> blockSpells = new List<MenuBool>();
        private readonly MenuBool autoE = new MenuBool("autoE", "Auto E");
        private readonly MenuBool autoEmissile = new MenuBool("autoEmissile", "Block unknown missile");
        private readonly MenuBool agcE = new MenuBool("agcE", "AntiGapcloserE");
        private readonly MenuSlider dmgE = new MenuSlider("dmgE", "Block under % hp", 90);

        public Core.MissileReturn MissileManager;

        public Sivir()
        {
            Q = new Spell(SpellSlot.Q, 1200f);
            Q1 = new Spell(SpellSlot.Q, 1200f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SpellType.Line);
            Q1.SetSkillshot(0.25f, 90f, 1350f, true, SpellType.Line);

            Local.Add(new Menu("draw", "Draw")
            {
                notif,
                noti,
                onlyRdy,
                qRange
            });

            FarmMenu.Add(farmQ);
            FarmMenu.Add(farmW);
            FarmMenu.Add(jungleQ);
            FarmMenu.Add(jungleW);

            Local.Add(new Menu("qConfig", "Q Config")
            {
                autoQ
            });
            Local.Add(harassW);
            Local.Add(autoR);

            var spellManager = new Menu("spellManager", "Spell Manager");

            foreach(var enemy in GameObjects.EnemyHeroes)
            {
                for (var i = 0; i < 4; i ++)
                {
                    var spell = enemy.Spellbook.GetSpell((SpellSlot)i);

                    if (spell.SData.TargetingType != SpellDataTargetType.Self && spell.SData.TargetingType != SpellDataTargetType.TargetOrLocation)
                    {
                        if (spell.SData.TargetingType == SpellDataTargetType.Target)
                        {
                            var item = new MenuBool("spell" + spell.SData.Name, spell.Name);

                            blockSpells.Add(item);
                            spellManager.Add(item);
                        }
                        else
                        {
                            var item = new MenuBool("spell" + spell.SData.Name, spell.Name, false);

                            blockSpells.Add(item);
                            spellManager.Add(item);
                        }
                    }
                }
            }

            Local.Add(new Menu("shieldE", "E Shield Config")
            {
                spellManager,
                autoE,
                autoEmissile,
                agcE,
                dmgE
            });

            MissileManager = new Core.MissileReturn("SivirQMissile", "SivirQMissileReturn", Q);

            AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            Orbwalker.OnAfterAttack += Orbwalker_OnAfterAttack;
        }

        private void AIBaseClient_OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid || sender.Type != GameObjectType.AIHeroClient || !sender.IsEnemy)
            {
                return;
            }

            if (!E.IsReady() || args.SData.Name.IsAutoAttack() || Player.HealthPercent > dmgE.Value || !autoE.Enabled)
            {
                return;
            }

            if (args.Target != null)
            {
                if (args.Target.IsMe)
                {
                    E.Cast();
                }
            }
            else if (OktwCommon.CanHitSkillShot(Player,args.Start,args.To, args.SData))
            {
                E.Cast();
            }
        }

        private void AntiGapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!OktwCommon.CheckGapcloser(sender, args))
            {
                return;
            }

            if (agcE.Enabled && E.IsReady() && sender.IsValidTarget(5000))
            {
                E.Cast();
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (notif.Enabled)
            {
                if (Player.HasBuff("SivirWMarker"))
                {
                    var color = System.Drawing.Color.Yellow;
                    var time = OktwCommon.GetPassiveTime(Player, "SivirWMarker");

                    if (time < 1)
                    {
                        color = System.Drawing.Color.Red;
                    }

                    drawText("W: " + string.Format("{0:0.0}", time), Player.Position, 175, color);
                }

                if (Player.HasBuff("SivirE"))
                {
                    var color = System.Drawing.Color.Aqua;
                    var time = OktwCommon.GetPassiveTime(Player, "SivirE");

                    if (time < 1)
                    {
                        color = System.Drawing.Color.Red;
                    }

                    drawText("E: " + string.Format("{0:0.0}", time), Player.Position, 200, color);
                }

                if (Player.HasBuff("SivirR"))
                {
                    var color = System.Drawing.Color.GreenYellow;
                    var time = OktwCommon.GetPassiveTime(Player, "SivirR");

                    if (time < 1)
                    {
                        color = System.Drawing.Color.Red;
                    }

                    drawText("R: " + string.Format("{0:0.0}", time), Player.Position, 225, color);
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

            if (noti.Enabled)
            {
                var target = TargetSelector.GetTarget(1500, DamageType.Physical);

                if (target.IsValidTarget())
                {
                    if (Q.GetDamage(target) * 2 > target.Health)
                    {
                        Render.Circle.DrawCircle(target.PreviousPosition, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Full Q can kill: " + target.CharacterName + " have: " + target.Health + " hp");
                    }
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (LagFree(0))
            {
                SetMana();
            }

            if (LagFree(1) && Q.IsReady() && !Player.IsWindingUp)
            {
                LogicQ();
            }

            if (LagFree(2) && R.IsReady() && Combo && autoR.Enabled)
            {
                LogicR();
            }

            if (LagFree(3) && LaneClear)
            {
                Jungle();
            }
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;

            if (missile != null && autoEmissile.Enabled)
            {
                if (!missile.SData.Name.IsAutoAttack() && missile.Target == Player && missile.SpellCaster.IsEnemy && missile.SpellCaster.Type == GameObjectType.AIHeroClient)
                {
                    E.Cast();
                }
            }
        }

        private void Orbwalker_OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            if (W.IsReady() && !None && args.Target.IsValidTarget())
            {
                var t = args.Target as AIHeroClient;

                if (t != null)
                {
                    if (Player.GetAutoAttackDamage(t)* 3 > t.Health - OktwCommon.GetIncomingDamage(t))
                    {
                        W.Cast();
                    }

                    if (Combo && Player.Mana > WMANA + RMANA)
                    {
                        W.Cast();
                    }
                    else if (harassW.Enabled && !Player.IsUnderEnemyTurret() && Player.Mana > QMANA + WMANA + RMANA && InHarassList(t))
                    {
                        W.Cast();
                    }
                }
                else
                {
                    var t2 = TargetSelector.GetTarget(900, DamageType.Physical);

                    if (t2.IsValidTarget() && harassW.Enabled && InHarassList(t2) && !Player.IsUnderEnemyTurret() && Player.Mana > QMANA + WMANA + RMANA && t2.Distance(args.Target.Position) < 500)
                    {
                        W.Cast();
                    }

                    if (args.Target is AIMinionClient && FarmSpells && farmW.Enabled && !Player.IsUnderEnemyTurret())
                    {
                        var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(500));

                        if (minions.Count() >= LCminions.Value)
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (t.IsValidTarget())
            {
                if (autoQ.Enabled)
                {
                    MissileManager.Target = t;

                    var qDmg = OktwCommon.GetKsDamage(t, Q) * 1.9;

                    if (t.InAutoAttackRange())
                    {
                        qDmg += Player.GetAutoAttackDamage(t) * 3;
                    }

                    if (qDmg > t.Health)
                    {
                        Q.Cast(t);
                    }
                    else if (Combo && Player.Mana > QMANA + RMANA)
                    {
                        CastSpell(Q, t);
                    }
                    else if (Harass && InHarassList(t) && !Player.IsUnderEnemyTurret())
                    {
                        if (Player.ManaPercent > 90)
                        {
                            CastSpell(Q, t);
                        }
                        else if (Player.Mana > QMANA + QMANA + WMANA + RMANA)
                        {
                            CastSpell(Q1, t);
                        }
                        else if (Player.Mana > QMANA + QMANA + WMANA + RMANA)
                        {
                            Q.CastIfWillHit(t, 2);

                            if (LaneClear)
                            {
                                CastSpell(Q, t);
                            }
                        }
                    }

                    if (Player.Mana > WMANA + RMANA)
                    {
                        foreach (var enemy in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(Q.Range) && !OktwCommon.CanMove(e)))
                        {
                            Q.Cast(enemy);
                        }
                    }
                }
            }
            else if (FarmSpells && farmQ.Enabled && LaneClear)
            {
                var allMinionsQ = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range)).ToList();
                var farmQ = Q.GetLineFarmLocation(allMinionsQ);

                if (farmQ.MinionsHit >= LCminions.Value)
                {
                    Q.Cast(farmQ.Position);
                }
            }
        }

        private void LogicR()
        {
            var t = TargetSelector.GetTarget(800, DamageType.Physical);

            if (Player.CountEnemyHeroesInRange(800) > 2)
            {
                R.Cast();
            }
            else if (t.IsValidTarget() && Orbwalker.GetTarget() == null && Combo && Player.GetAutoAttackDamage(t) * 2 > t.Health && !Q.IsReady() && t.CountEnemyHeroesInRange(800) < 3)
            {
                R.Cast();
            }
        }

        private void Jungle()
        {
            if (Player.Mana > WMANA + RMANA + RMANA)
            {
                var mobs = GameObjects.Jungle.Where(e => e.IsValidTarget(600));

                if (mobs.Count() > 0)
                {
                    var mob = mobs.First();

                    if (jungleW.Enabled && W.IsReady())
                    {
                        W.Cast();
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

        private void drawText(string msg, Vector3 hero, int high, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - high, color, msg);
        }
    }
}
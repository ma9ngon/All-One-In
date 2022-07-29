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
    class Ezreal : Base
    {
        Vector3 CursorPosition = Vector3.Zero;
        public double lag = 0;
        public double WCastTime = 0;
        public double QCastTime = 0;
        public float DragonDmg = 0;
        public double DragonTime = 0;
        public bool Esmart = false;
        public double OverKill = 0;
        public double OverFarm = 0;
        public double diag = 0;
        public double diagF = 0;
        public int Muramana = 3042;
        public int Tear = 3070;
        public int Manamune = 3004;
        public double NotTime = 0;
        public static Core.OKTWdash Dash;

        private readonly MenuBool noti = new MenuBool("noti", "Show notification",false); 
        private readonly MenuBool onlyRdy = new MenuBool("onlyRdy", "Draw only ready spells"); 
        private readonly MenuBool qRange = new MenuBool("qRange", "Q range", false);
        private readonly MenuBool wRange = new MenuBool("wRange", "W range", false);
        private readonly MenuBool eRange = new MenuBool("eRange", "E range", false);
        private readonly MenuBool rRange = new MenuBool("rRange", "R range", false);

        private readonly MenuBool autoW = new MenuBool("autoW", "Auto W");
        private readonly MenuBool wPush = new MenuBool("wPush", "W on towers");
        private readonly MenuBool harassW = new MenuBool("harassW", "Harass W");

        private readonly MenuKeyBind smartE = new MenuKeyBind("smartE", "SmartCast E key", Keys.T, KeyBindType.Press);
        private readonly MenuKeyBind smartEW = new MenuKeyBind("smartEW", "SmartCast E + W key", Keys.T, KeyBindType.Press);
        private readonly MenuBool EKsCombo = new MenuBool("EKsCombo", "E ks combo");
        private readonly MenuBool EAntiMelee = new MenuBool("EAntiMelee", "E anti-melee");
        private readonly MenuBool autoEgrab = new MenuBool("autoEgrab", "Auto E anti grab");

        private readonly MenuSlider HarassMana = new MenuSlider("HarassMana", "Harass Mana", 30, 0, 100);


        private readonly MenuBool autoR = new MenuBool("autoR", "Auto R");
        private readonly MenuBool Rcc = new MenuBool("Rcc", "R cc");
        private readonly MenuBool Rjungle = new MenuBool("Rjungle", "R Jungle stealer");
        private readonly MenuBool Rdragon = new MenuBool("Rdragon", "Dragon");
        private readonly MenuBool Rbaron = new MenuBool("Rbaron", "baron");
        private readonly MenuBool Rred = new MenuBool("Rred", "Red");
        private readonly MenuBool Rblue = new MenuBool("Rblue", "Rlue");
        private readonly MenuBool Rally = new MenuBool("Rally", "Ally stealer");
        private readonly MenuSlider Raoe = new MenuSlider("Raoe", "R AOE", 3, 0, 5);
        private readonly MenuBool Rturrent = new MenuBool("Rturrent", "Don't R under turret");
        private readonly MenuKeyBind useR = new MenuKeyBind("useR", "Semi-manual cast R key", Keys.T, KeyBindType.Press);

        private readonly MenuSlider MaxRangeR = new MenuSlider("MaxRangeR", "Max R range", 3000, 0, 5000);
        private readonly MenuSlider MinRangeR = new MenuSlider("MinRangeR", "Min R range", 900, 0, 5000);
        private readonly MenuBool FarmQ = new MenuBool("farmQ", "LaneClear Q");
        private readonly MenuBool FQ = new MenuBool("FQ", "Farm Q out range");
        private readonly MenuBool LCP = new MenuBool("LCP", "FAST LaneClear");


        private readonly MenuBool jungleQ = new MenuBool("jungleQ", "Jungle clear Q");
        private readonly MenuBool jungleW = new MenuBool("jungleW", "Jungle clear W");
        private readonly MenuBool jungleE = new MenuBool("jungleE", "Jungle clear E");
        public Ezreal()
        {
            Q = new Spell(SpellSlot.Q, 1180);
            W = new Spell(SpellSlot.W, 1180);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 3000f);
            
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SpellType.Line);
            W.SetSkillshot(0.25f, 60f, 1700f, false, SpellType.Line);
            R.SetSkillshot(1.1f, 160f, 2000f, false, SpellType.Line);

            Local.Add(new Menu("draw", "Draw")
            {
                onlyRdy,
                qRange,
                wRange,
                eRange,
                rRange
                
            });

            Local.Add(new Menu("wConfig", "W Config")
            {
                autoW,
                wPush,
                harassW
            });

            Local.Add(new Menu("eConfig", "E Config")
            {
                smartE,
                smartEW,
                EKsCombo,
                EAntiMelee,
                autoEgrab
            });

            Local.Add(new Menu("rConfig", "R Config")
            {
                autoR,
                useR,
                Rcc,
                Raoe,
                Rturrent,
                MaxRangeR,
                MinRangeR

            });
            
            Local.Add(new Menu("HarassMana", "Harass Mana")
            {
                HarassMana
            });
            
            Local.Add(new Menu("Rjungle", "Rjungle")
            {
              Rjungle,
              Rdragon,
              Rbaron,
              Rred,
              Rblue,
              Rally,
            });

            Dash = new Core.OKTWdash(E);
            
            FarmMenu.Add(FQ);
            FarmMenu.Add(FarmQ);
            FarmMenu.Add(LCP);
            FarmMenu.Add(jungleQ);
            FarmMenu.Add(jungleW);
            FarmMenu.Add(jungleE);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;
            AIBaseClient.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
        }


        private void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (wPush.Enabled && args.Target.Type == GameObjectType.AITurretClient)
            {
                if (W.IsReady() && Player.Mana > RMANA + WMANA + EMANA)
                {
                    if (W.Cast(args.Target.Position))
                    {
                        args.Process = false;
                        return;
                    }
                }
            }

            if (W.IsReady() && Player.Mana > RMANA + WMANA + EMANA)
            {
                var target = args.Target as AIHeroClient;
                if (target != null)
                {
                    var prediction = W.GetPrediction(target);

                    if (prediction.Hitchance < HitChance.Medium || target.Distance(Player) - Player.BoundingRadius > Player.AttackRange - 50)
                        return;

                    if (Program.Combo && Player.Mana > RMANA + WMANA + EMANA)
                    {
                        if (W.Cast(prediction.CastPosition))
                        {
                            args.Process = false;
                            return;
                        }
                    }
                    else if (Program.Harass && harassW.Enabled && Player.Mana > Player.MaxMana * 0.8 && Player.ManaPercent > HarassMana.Value && OktwCommon.CanHarass())
                    {
                        if (W.Cast(prediction.CastPosition))
                        {
                            args.Process = false;
                            return;
                        }
                    }
                }
            }
        }

        private void Obj_AI_Base_OnBuffAdd(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            if(sender.IsMe && autoEgrab.Enabled && E.IsReady())
            {
                if(args.Buff.Name == "ThreshQ" || args.Buff.Name == "rocketgrab2")
                {
                    var dashPos = Dash.CastDash(true);
                    if (!dashPos.IsZero)
                    {
                        E.Cast(dashPos);
                    }
                    else
                    {
                        E.Cast(Game.CursorPos);
                    }
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
                LogicJungle();
            }
            if (R.IsReady() && Rjungle.Enabled)
            {
                KsJungle();
            }
            else
                DragonTime = 0;

            if (E.IsReady())
            {
                if (Program.LagFree(0))
                    LogicE();

                if (smartE.Active)
                    Esmart = true;
                if (smartEW.Active && W.IsReady())
                {
                    CursorPosition = Game.CursorPos;
                    W.Cast(CursorPosition);
                }
                if (Esmart && Player.Position.Extend(Game.CursorPos, E.Range).CountEnemyHeroesInRange(500) < 4)
                    E.Cast(Player.Position.Extend(Game.CursorPos, E.Range));
                
                if (!CursorPosition.IsZero)
                    E.Cast(Player.Position.Extend(CursorPosition, E.Range));
            }
            else
            {
                CursorPosition = Vector3.Zero;
                Esmart = false;
            }

            if (Q.IsReady())
                LogicQ();

            if (Program.LagFree(3) && W.IsReady() && autoW.Enabled)
                LogicW();

            if (R.IsReady())
            {
                if (useR.Active)
                {
                    var t = TargetSelector.GetTarget(R.Range,DamageType.Physical);
                    if (t.IsValidTarget())
                        R.Cast(t, false, true);
                }

                if (Program.LagFree(4))
                    LogicR();
            }
        }

        private void LogicJungle()
        {
            if (LaneClear)
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
                    var drabar = (mob.SkinName.ToLower().Contains("dragon"))
                   || (mob.SkinName == "SRU_Baron") ;
                    if (jungleW.Enabled && W.IsReady()&&
                       drabar)
                    {
                        W.Cast(mob.Position);
                        return;
                    }
                }
            }
        }

        private void LogicQ()
        {
            if (Variables.GameTimeTickCount - W.LastCastAttemptTime < 125)
            {
                return;
            }

            if (Program.LagFree(1))
            {
                if (!Orbwalker.CanMove(50,true) )
                {
                    return;
                }
                    
                bool cc = !Program.None && Player.Mana > RMANA + QMANA + EMANA;
                bool harass = Program.Harass && Player.ManaPercent > HarassMana.Value && OktwCommon.CanHarass();

                if (Program.Combo && Player.Mana > RMANA + QMANA)
                {
                    var t = TargetSelector.GetTarget(Q.Range,DamageType.Physical);

                    if (t.IsValidTarget() && (!W.IsReady() || !autoW.Enabled || !CanCastSpellPred(W, t)))
                    {
                        Program.CastSpell(Q, t);
                    }
                }

                foreach (var t in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range)).OrderBy(t => t.Health))
                {
                    var qDmg = OktwCommon.GetKsDamage(t, Q);
                    var wDmg = W.GetDamage(t);
                    if (qDmg + wDmg > t.Health)
                    {
                        Program.CastSpell(Q, t);
                        OverKill = Game.Time;
                        return;
                    }

                    if (cc && (!W.IsReady() || !autoW.Enabled || !CanCastSpellPred(W, t)) && !OktwCommon.CanMove(t))
                        Q.Cast(t);

                    if (harass && (!W.IsReady() || !autoW.Enabled || !CanCastSpellPred(W, t)))
                        Program.CastSpell(Q, t);
                }
            }
            else if (Program.LagFree(2))
            {
                if (Player.Mana > QMANA && Farm)
                {
                    farmQ();
                    lag = Game.Time;
                }
               
            }
        }

        private void LogicW()
        {
            if (Variables.GameTimeTickCount - Q.LastCastAttemptTime < 125)
            {
                return;
            }

            if (!W.IsReady() || !autoW.Enabled)
                return;

            var t = TargetSelector.GetTarget(W.Range,DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (!((Q.IsReady() && CanCastSpellPred(Q, t)) || AIBaseClientExtensions.InAutoAttackRange(Player,t)))
                    return;

                if (Program.Combo && Player.Mana > RMANA + WMANA + EMANA)
                    Program.CastSpell(W, t);
                else if (Program.Harass && harassW.Enabled  && Player.Mana > Player.MaxMana * 0.8 && Player.ManaPercent > HarassMana.Value && OktwCommon.CanHarass())
                    Program.CastSpell(W, t);
                else
                {
                    var qDmg = Q.GetDamage(t);
                    var wDmg = OktwCommon.GetKsDamage(t, W);
                    if (wDmg > t.Health)
                    {
                        Program.CastSpell(W, t);
                        OverKill = Game.Time;
                    }
                    else if (wDmg + qDmg > t.Health && Q.IsReady())
                    {
                        Program.CastSpell(W, t);
                    }
                }

                if (!Program.None && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                        W.Cast(enemy, true);
                }
            }
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(1300,DamageType.Physical);
            
            if (EAntiMelee.Enabled)
            { 
                if (GameObjects.EnemyHeroes.Any(target => target.IsValidTarget(1000) && target.IsMelee && Player.Distance(Prediction.GetPrediction(target, 0.2f).CastPosition) < 250))
                {
                    var dashPos = Dash.CastDash(true);
                    if (!dashPos.IsZero)
                    {
                        E.Cast(dashPos);
                    }
                }
            }

            if (t.IsValidTarget() && Program.Combo && EKsCombo.Enabled && Player.HealthPercent > 40 && t.Distance(Game.CursorPos) + 300 < t.Position.Distance(Player.Position) && !AIBaseClientExtensions.InAutoAttackRange(Player,t) && !Player.IsUnderEnemyTurret() && (Game.Time - OverKill > 0.3) )
            {
                var dashPosition = Player.Position.Extend(Game.CursorPos, E.Range);

                if (dashPosition.CountEnemyHeroesInRange(900) < 3)
                {
                    var dmgCombo = 0f;
                    
                    if (t.IsValidTarget(950))
                    {
                        dmgCombo = (float)Player.GetAutoAttackDamage(t) + E.GetDamage(t);
                    }

                    if (Q.IsReady() && Player.Mana > QMANA + EMANA && Q.WillHit(dashPosition, Q.GetPrediction(t).UnitPosition))
                        dmgCombo = Q.GetDamage(t);

                    if (W.IsReady() && Player.Mana > QMANA + EMANA + WMANA )
                    {
                        dmgCombo += W.GetDamage(t);
                    }

                    if (dmgCombo > t.Health && OktwCommon.ValidUlt(t))
                    {
                        E.Cast(dashPosition);
                        OverKill = Game.Time;
                    }
                }
            }
        }

        private void LogicR()
        {
            if (Player.IsUnderEnemyTurret() && Rturrent.Enabled)
                return;

            if (autoR.Enabled  && Game.Time - OverKill > 0.6)
            {//&& Player.CountEnemyHeroesInRange(800) == 0
                R.Range = MaxRangeR.Value;
                foreach (var target in GameObjects.EnemyHeroes.Where(target => target.IsValidTarget(R.Range) && OktwCommon.ValidUlt(target)))
                {
                    double predictedHealth = target.Health - OktwCommon.GetIncomingDamage(target);

                    if (Rcc.Enabled && target.IsValidTarget(Q.Range + E.Range) && target.Health < Player.MaxHealth && !OktwCommon.CanMove(target))
                    {
                        R.Cast(target, true, true);
                    }

                    double Rdmg = R.GetDamage(target);

                    if (Rdmg > predictedHealth)
                        Rdmg = getRdmg(target);

                    if (Rdmg > predictedHealth && Player.Distance(target) > MinRangeR.Value)
                    {
                        //&& target.CountAllyHeroesInRange(500) == 0
                        Program.CastSpell(R,target);
                    }
                    if (Program.Combo )
                    {
                        //&& Player.CountEnemyHeroesInRange(1200) == 0
                        R.CastIfWillHit(target, Raoe.Value);
                    }
                }
            }
        }

        private bool DashCheck(Vector3 dash)
        {
            if (!dash.IsUnderEnemyTurret() || Program.Combo)
                return true;
            else
                return false;
        }

        private double getRdmg(AIHeroClient target)
        {
            var rDmg = R.GetDamage(target);
            var dmg = 0;
            PredictionOutput output = R.GetPrediction(target);
            Vector2 direction = output.CastPosition.ToVector2() - Player.Position.ToVector2();
            direction.Normalize();
            List<AIHeroClient> enemies = GameObjects.EnemyHeroes.Where(x =>x.IsValidTarget()).ToList();
            foreach (var enemy in enemies)
            {
                PredictionOutput prediction = R.GetPrediction(enemy);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - Player.Position;
                Vector3 w = predictedPosition - Player.Position;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = Player.Position + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (R.Width + 100 + enemy.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.Position))
                    dmg++;
            }
            var allMinionsR = GameObjects.GetMinions(ObjectManager.Player.Position, R.Range);
            foreach (var minion in allMinionsR)
            {
                PredictionOutput prediction = R.GetPrediction(minion);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - Player.Position;
                Vector3 w = predictedPosition - Player.Position;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = Player.Position + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (R.Width + 100 + minion.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.Position))
                    dmg++;
            }
            //if (Config.Item("debug", true).GetValue<bool>())
            //    Game.PrintChat("R collision" + dmg);
            if (dmg == 0)
                return rDmg;
            else if (dmg > 7)
                return rDmg * 0.7;
            else
                return rDmg - (rDmg * 0.1 * dmg);

        }

        private float GetPassiveTime()
        {
            return
                ObjectManager.Player.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == "ezrealrisingspellforce")
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        public void farmQ()
        {
            if (Program.LaneClear)
            {
                var mobs = GameObjects.GetMinions(Player.Position, 800);
                if (mobs.Count > 0)
                {
                    
                    Q.Cast(mobs.FirstOrDefault());
                }
            }

            if (!Orbwalker.CanMove(50,true) ||  Orbwalker.CanAttack())
            {
                return;
            }

            var minions = GameObjects.GetMinions(Player.Position, Q.Range);
            int orbTarget = 0;

            if (Orbwalker.GetTarget() != null)
                orbTarget = Orbwalker.GetTarget().NetworkId;

            if (FQ.Enabled)
            {
                foreach (var minion in minions.Where(minion => minion.IsValidTarget() && orbTarget != minion.NetworkId && !AIBaseClientExtensions.InAutoAttackRange(Player,minion)))
                {
                    int delay = (int)((minion.Distance(Player) / Q.Speed + Q.Delay) * 1000);
                    var hpPred = HealthPrediction.GetPrediction(minion, delay);
                    if (hpPred > 0 && hpPred < Q.GetDamage(minion))
                    {
                        if (Q.Cast(minion) == CastStates.SuccessfullyCasted)
                            return;
                    }
                }
            }

            if (FarmQ.Enabled && !Orbwalker.CanAttack() && FarmSpells)
            {
                
                var PT = Game.Time - GetPassiveTime() > -1.5 || !E.IsReady();

                foreach (var minion in minions.Where(minion => AIBaseClientExtensions.InAutoAttackRange(Player,minion)))
                {
                    int delay = (int)((minion.Distance(Player) / Q.Speed + Q.Delay) * 1000);
                    var hpPred = HealthPrediction.GetPrediction(minion, delay);
                    if (hpPred < 20)
                        continue;
                    
                    var qDmg = Q.GetDamage(minion);
                    if (hpPred < qDmg && orbTarget != minion.NetworkId)
                    {
                        if (Q.Cast(minion) ==CastStates.SuccessfullyCasted)
                            return; 
                    }
                    else if (PT || LCP.Enabled)
                    {
                        if (minion.HealthPercent > 80)
                        {
                            if (Q.Cast(minion) == CastStates.SuccessfullyCasted)
                                return;
                        }
                    }
                }
            }
        }

        private void KsJungle()
        {
            var mobs = GameObjects.GetMinions(Player.Position, float.MaxValue);
            foreach (var mob in mobs)
            {
                if (mob.Health == mob.MaxHealth)
                    continue;
                if (((mob.SkinName.ToLower().Contains("dragon") && Rdragon.Enabled)
                    || (mob.SkinName == "SRU_Baron" && Rbaron.Enabled)
                    || (mob.SkinName == "SRU_Red" && Rred.Enabled)
                    || (mob.SkinName == "SRU_Blue" && Rblue.Enabled)
                    && (mob.CountAllyHeroesInRange(1000) == 0 || Rally.Enabled)
                    && mob.Distance(Player.Position) > 1000
                    ))
                {
                    if (DragonDmg == 0)
                        DragonDmg = mob.Health;

                    if (Game.Time - DragonTime > 3)
                    {
                        if (DragonDmg - mob.Health > 0)
                        {
                            DragonDmg = mob.Health;
                        }
                        DragonTime = Game.Time;
                    }
                    else
                    {
                        var DmgSec = (DragonDmg - mob.Health) * (Math.Abs(DragonTime - Game.Time) / 3);
                        //Program.debug("DS  " + DmgSec);
                        if (DragonDmg - mob.Health > 0)
                        {
                            
                            var timeTravel = GetUltTravelTime(Player, R.Speed, R.Delay, mob.Position);
                            var timeR = (mob.Health - R.GetDamage(mob)) / (DmgSec / 3);
                            //Program.debug("timeTravel " + timeTravel + "timeR " + timeR + "d " + R.GetDamage(mob));
                            if (timeTravel > timeR)
                                R.Cast(mob.Position);
                        }
                        else
                            DragonDmg = mob.Health;
                        //Program.debug("" + GetUltTravelTime(ObjectManager.Player, R.Speed, R.Delay, mob.Position));
                    }
                }
            }
        }

        private float GetUltTravelTime(AIBaseClient source, float speed, float delay, Vector3 targetpos)
        {
            float distance = Vector3.Distance(source.Position, targetpos);
            float missilespeed = speed;

            return (distance / missilespeed + delay);
        }

        private void SetMana()
        {
            if ((manaDisable.Enabled && Program.Combo) || Player.HealthPercent < 20)
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
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        public static void drawText(string msg, AIHeroClient Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero.Position);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1], color, msg);
        }


        public static bool CanCastSpellPred(Spell QWER, AIHeroClient target)
        {
            int predIndex = 0;
            HitChance hitchance = HitChance.Low;

            if (QWER.Slot == SpellSlot.Q)
            {
                predIndex = QHitChance.Index;
                if (QHitChance.Index == 0)
                    hitchance = HitChance.VeryHigh;
                else if (QHitChance.Index == 1)
                    hitchance = HitChance.High;
                else if (QHitChance.Index == 2)
                    hitchance = HitChance.Medium;
            }
            else if (QWER.Slot == SpellSlot.W)
            {
                predIndex = WHitChance.Index;
                if (WHitChance.Index == 0)
                    hitchance = HitChance.VeryHigh;
                else if (WHitChance.Index == 1)
                    hitchance = HitChance.High;
                else if (WHitChance.Index == 2)
                    hitchance = HitChance.Medium;
            }
            else if (QWER.Slot == SpellSlot.E)
            {
                predIndex = EHitChance.Index;
                if (EHitChance.Index == 0)
                    hitchance = HitChance.VeryHigh;
                else if (EHitChance.Index == 1)
                    hitchance = HitChance.High;
                else if (EHitChance.Index == 2)
                    hitchance = HitChance.Medium;
            }
            else if (QWER.Slot == SpellSlot.R)
            {
                predIndex = RHitChance.Index;
                if (RHitChance.Index == 0)
                    hitchance = HitChance.VeryHigh;
                else if (RHitChance.Index == 1)
                    hitchance = HitChance.High;
                else if (RHitChance.Index == 2)
                    hitchance = HitChance.Medium;
            }

            if (predIndex == 3)
            {
                SpellType CoreType2 = SpellType.Line;
                bool aoe2 = false;

                if (QWER.Width > 80 && !QWER.Collision)
                    aoe2 = true;

                var predInput2 = new PredictionInput
                {
                    Aoe = aoe2,
                    Collision = QWER.Collision,
                    Speed = QWER.Speed,
                    Delay = QWER.Delay,
                    Range = QWER.Range,
                    From = Player.Position,
                    Radius = QWER.Width,
                    Unit = target,
                    Type = CoreType2
                };
                var poutput2 = Prediction.GetPrediction(predInput2);

                if (QWER.Speed != float.MaxValue && OktwCommon.CollisionYasuo(Player.Position, poutput2.CastPosition))
                    return false;

                if ((int)hitchance == 6)
                {
                    if (poutput2.Hitchance >= HitChance.VeryHigh)
                        return true;
                    else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= HitChance.High)
                    {
                        return true;
                    }

                }
                else if ((int)hitchance == 5)
                {
                    if (poutput2.Hitchance >= HitChance.High)
                        return true;

                }
                else if ((int)hitchance == 4)
                {
                    if (poutput2.Hitchance >= HitChance.Medium)
                        return true;
                }
            }
            else if (predIndex == 1)
            {
                SpellType CoreType2 = SpellType.Line;
                bool aoe2 = false;

                if (QWER.Type == SpellType.Circle)
                {
                    CoreType2 = SpellType.Circle;
                    aoe2 = true;
                }

                if (QWER.Width > 80 && !QWER.Collision)
                    aoe2 = true;

                var predInput2 = new PredictionInput
                {
                    Aoe = aoe2,
                    Collision = QWER.Collision,
                    Speed = QWER.Speed,
                    Delay = QWER.Delay,
                    Range = QWER.Range,
                    From = Player.Position,
                    Radius = QWER.Width,
                    Unit = target,
                    Type = CoreType2
                };
                var poutput2 = Prediction.GetPrediction(predInput2);

                //var poutput2 = QWER.GetPrediction(target);

                if (QWER.Speed != float.MaxValue && OktwCommon.CollisionYasuo(Player.Position, poutput2.CastPosition))
                    return false;

                if ((int)hitchance == 6)
                {
                    if (poutput2.Hitchance >= HitChance.VeryHigh)
                        return true;
                    else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= HitChance.High)
                    {
                        return true;
                    }

                }
                else if ((int)hitchance == 5)
                {
                    if (poutput2.Hitchance >= HitChance.High)
                        return true;

                }
                else if ((int)hitchance == 4)
                {
                    if (poutput2.Hitchance >= HitChance.Medium)
                        return true;
                }
                
            }
            else if (predIndex == 0)
            {
                return QWER.GetPrediction(target).Hitchance >= hitchance;
            }
            else if (predIndex == 2)
            {
                return QWER.GetPrediction(target).Hitchance >= HitChance.High;
            }
            return false;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (qRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (Q.IsReady())
                        Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
                       
                }
                else
                    Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }
            if (wRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (W.IsReady())
                        Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1); 
                }
                else
                    Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }
            if (eRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (E.IsReady())
                        Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
                }
                else
                    Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }
            if (rRange.Enabled)
            {
                if (onlyRdy.Enabled)
                {
                    if (R.IsReady())
                        Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1);
                }
                else
                    Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }


            if (noti.Enabled)
            {

                var target = TargetSelector.GetTarget(1500, DamageType.Physical);
                if (target.IsValidTarget())
                {

                    var poutput = Q.GetPrediction(target);
                    if ((int)poutput.Hitchance == 5)
                        Render.Circle.DrawCircle(poutput.CastPosition, 50, System.Drawing.Color.YellowGreen);
                    if (Q.GetDamage(target) > target.Health)
                    {
                        Render.Circle.DrawCircle(target.Position, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q kill: " + target.CharacterName + " have: " + target.Health + "hp");
                    }
                    else if (Q.GetDamage(target) + W.GetDamage(target) > target.Health)
                    {
                        Render.Circle.DrawCircle(target.Position, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q + W kill: " + target.CharacterName + " have: " + target.Health + "hp");
                    }
                    else if (Q.GetDamage(target) + W.GetDamage(target) + E.GetDamage(target) > target.Health)
                    {
                        Render.Circle.DrawCircle(target.Position, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q + W + E kill: " + target.CharacterName + " have: " + target.Health + "hp");
                    }
                }
            }
        }
    }
}

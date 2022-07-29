using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using System;
using System.Collections.Generic;
using SharpDX.Mathematics;
using SharpDX;
using System.Linq;

namespace SimpleAIO.Champions
{
    internal class MF
    {
        private static Spell Q, Q1, W, E, R;
        private static Menu mainMenu;
        public static Vector2 RCastPos = new Vector2();
        public static float UltiCastedTime { get; set; }
        public static bool IsCastingR
        {
            get { return ObjectManager.Player.HasBuff("missfortunebulletsound"); }
        }
        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "MissFortune") return;

            Q = new Spell(SpellSlot.Q, 650f);
            Q1 = new Spell(SpellSlot.Q, Q.Range + 450f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1400f);
            

            Q.SetTargetted(0.29f, 1400f);
            E.SetSkillshot(0.5f, 100f, float.MaxValue, false, SpellType.Circle);
            R.SetSkillshot(0.25f, 100f, 2000f, false, SpellType.Line);
            mainMenu = new Menu("Miss ", "miss fortune", true);
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", false));
            Combo.Add(new MenuBool("Q1use", "Use Q1", true));
           // Combo.Add(new MenuBool("QuseAA", "Use Q at champions only (disable the above option)", true));
            Combo.Add(new MenuBool("Wuse", "Use W", true));
            Combo.Add(new MenuBool("Euse", "Use E", true));
            Combo.Add(new MenuBool("Ruse", "Use R", true));
            Combo.Add(new MenuSlider("RuseNum", "Use R", 3, 1, 5));

            mainMenu.Add(Combo);
            var Harass = new Menu("Harass", "Harass Settings");
            Harass.Add(new MenuBool("Quse", "Use Q", false));
            Harass.Add(new MenuBool("Q1use", "Use Q1", true));
            Harass.Add(new MenuBool("Wuse", "Use W", true));
            Harass.Add(new MenuBool("Euse", "Use E", true));
            // Harass.Add(new MenuBool("Ruse","Use R",true));
            Harass.Add(new MenuSlider("mana%", "Mana percent", 50, 0, 100));
            mainMenu.Add(Harass);
            var LaneClear = new Menu("LaneClear","Lane Clear");
            LaneClear.Add(new MenuBool("Quse","use q",true));
            var Misc = new Menu("Misc", "Misc Settings");
            //Misc.Add(new MenuSlider("QHeat","Dont use Q if Heat =",79,0,100));
            //Misc.Add(new MenuSlider("EHeat","Dont use E if Heat =",89,0,100));
            Misc.Add(new MenuBool("usePackets", "Use Packets", true));
            Misc.Add(new MenuBool("AutoShield", "Auto W", true));
            Misc.Add(new MenuBool("DamageAfterCombo", "Draw Combo Damage", true));
            Misc.Add(new MenuBool("antiGapCloser", "antiGapCloser", true));
            Misc.Add(new MenuBool("Interrupter", "Interrupte with R", true));
            //Misc.Add(new MenuBool("Wuse","keep Heating",true));
            mainMenu.Add(Misc);
            var Draw = new Menu("Draw", "Draw Settings");
            Draw.Add(new MenuBool("qRange", "Draw Q range", true));
            Draw.Add(new MenuBool("wRange", "Draw W range", true));
            Draw.Add(new MenuBool("eRange", "Draw E range", true));
            Draw.Add(new MenuBool("rRange", "Draw R range", false));
            mainMenu.Add(LaneClear);
            Draw.Add(new MenuBool("lista", "Draw only if spell is ready", true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
           // AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
                  
        }


        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            //if (sender.Name == "MissFortuneBulletEMPTY")
            //{
            //    RCastPos = args.End.ToVector2();
            //}
        }

        internal struct Position
        {
            public readonly AIHeroClient Hero;
            public readonly AIBaseClient Base;
            public readonly Vector3 UnitPosition;
            
            public Position(AIHeroClient hero, Vector3 unitPosition)
            {
                Hero = hero;
                Base = null;
                UnitPosition = unitPosition;
            }

            public Position(AIBaseClient unit, Vector3 unitPosition)
            {
                Base = unit;
                Hero = null;
                UnitPosition = unitPosition;
            }
        }
        private static bool HasPassiveDebuff(AIHeroClient target)
        {
            return target.HasBuff("missfortunepassive");
        }

        private static void CastQ1()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                Q.CastOnUnit(target);
            }
            else if (1 == 1)
            {
                target = TargetSelector.GetTarget(Q1.Range,DamageType.Physical);
                if (target != null)
                {
                    var heroPositions = (from t in GameObjects.EnemyHeroes
                                         where t.IsValidTarget(Q1.Range)
                                         let prediction = Q.GetPrediction(t)
                                         select new Position(t, prediction.UnitPosition)).Where(
                            t => t.UnitPosition.Distance(GameObjects.Player.Position) < Q1.Range).ToList();
                    if (heroPositions.Any())
                    {
                        var minions = GameObjects.GetMinions(
                            Q1.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);

                        if (minions.Any(m => m.IsMoving) && !heroPositions.Any(h => HasPassiveDebuff(h.Hero)))
                        {
                            return;
                        }

                        var outerMinions = minions.Where(m => m.Distance(GameObjects.Player) > Q.Range).ToList();
                        var innerPositions = minions.Where(m => m.Distance(GameObjects.Player) < Q.Range).ToList();
                        foreach (var minion in innerPositions)
                        {
                            var lMinion = minion;
                            var coneBuff = new Geometry.Sector(minion.Position,
                                GameObjects.Player.Position.Extend(minion.Position, GameObjects.Player.Distance(minion) + Q.Range * 0.5f),
                                (float)(40 * Math.PI / 180), Q1.Range - Q.Range);

                            var coneNormal = new Geometry.Sector(minion.Position,
                                GameObjects.Player.Position.Extend(minion.Position, GameObjects.Player.Distance(minion) + Q.Range * 0.5f),
                                (float)(60 * Math.PI / 180), Q1.Range - Q.Range);

                            foreach (
                                var enemy in
                                    heroPositions.Where(
                                        m => m.UnitPosition.Distance(lMinion.Position) < Q1.Range - Q.Range))
                            {
                                if (coneBuff.IsInside(enemy.Hero) && HasPassiveDebuff(enemy.Hero))
                                {
                                    Q.CastOnUnit(minion);
                                    return;
                                }

                                if (coneNormal.IsInside(enemy.UnitPosition))
                                {
                                    var insideCone =
                                        outerMinions.Where(m => coneNormal.IsInside(m.Position)).ToList();
                                    if (!insideCone.Any() ||
                                        enemy.UnitPosition.Distance(minion.Position) <
                                        insideCone.Select(
                                            m => m.Position.Distance(minion.Position) - m.BoundingRadius)
                                            .DefaultIfEmpty(float.MaxValue)
                                            .Min())
                                    {
                                        Q.CastOnUnit(minion);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static bool Ractive()
        {
            return GameObjects.Player.Spellbook.ActiveSpell.SData.Name == "MissFortuneBulletTime";
        }
        private static void CastQ()
        {
            if (!Q.IsReady())
                return;

            var t = TargetSelector.GetTarget(Q.Range + 450, DamageType.Physical);
            if (t.IsValidTarget(Q.Range))
            {
                Q.CastOnUnit(t);
            }
        }
        //private static void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        //{
        //    if (sender.IsMe && args.SData.Name == "MissFortuneBulletTime") 
        //    {
        //        RCastPos = args.End.ToVector2();
        //        UltiCastedTime = Game.Time;
        //       // Game.Print("hello");
        //    }
                
        //}
        

        private static void ComboLogic()
        {
            if (GameObjects.Player.IsCastingImporantSpell())
            {
                return;
            }
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (!target.IsValidTarget()) return;
            if (W.IsReady() && mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && target.IsValidTarget(500))
            {
                W.Cast();
            }
            if (Q.IsReady() && mainMenu["Combo"].GetValue<MenuBool>("Q1use").Enabled)
            {
                CastQ1();
            }

            if (Q.IsReady() && mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled)
            {
                target = TargetSelector.GetTarget(Q.Range + 500, DamageType.Physical);
                var allMinion = GameObjects.GetMinions(target.Position, Q.Range, MinionTypes.All, MinionTeam.Enemy);
                if (target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);

                }
                var nearestMinion =
                    allMinion.Where(
                    minion =>
                    minion.Distance(ObjectManager.Player) <= target.Distance(ObjectManager.Player) &&
                            target.Distance(minion) < 450)
                        .OrderBy(minion => minion.Distance(ObjectManager.Player))
                        .FirstOrDefault();
                {
                    if (nearestMinion != null && nearestMinion.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(nearestMinion);
                    }
                }
            }

            if (E.CanCast(target) && mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled)
            {
                E.Cast(target);
            }

            if (R.CanCast(target) && mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && !Q.IsReady() && !W.IsReady()
                && !E.IsReady())
            {
                var slidercount = mainMenu["Combo"].GetValue<MenuSlider>("RuseNum").Value;
                
                R.CastIfWillHit(target, slidercount);
                //if (Ractive())
                //{
                //    var start = ObjectManager.Player.Position.ToVector2();
                //    var end = start.Extend(RCastPos, R.Range);
                //    var direction = (end - start).Normalized();
                //    var normal = direction.Perpendicular();

                //    var points = new List<Vector2>();
                //    var hitBox = target.BoundingRadius;
                //    points.Add(start + normal * (R.Width + hitBox));
                //    points.Add(start - normal * (R.Width + hitBox));
                //    points.Add(end + R.Range * direction - normal * (R.Width + hitBox));
                //    points.Add(end + R.Range * direction + normal * (R.Width + hitBox));

                //    for (var i = 0; i <= points.Count - 1; i++)
                //    {
                //        var A = points[i];
                //        var B = points[i == points.Count - 1 ? 0 : i + 1];

                //        if (target.Position.ToVector2().Distance(A, B, true) < 50 * 50)
                //        {
                //            Game.Print("out");
                //            GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                //        }
                //    }
                //    return;
                //}

                // Spellbook.OnCastSpell;
                //    OnProcessSpellCast
                //    AIBaseClient.OnProcessSpellCast;
                //AIBaseClient.OnDoCast

                if (target.Health <= R.GetDamage(target) || target.HealthPercent <= 30)
                {
                    Orbwalker.AttackEnabled = false;
                    Orbwalker.MoveEnabled = false;
                    R.Cast(target.Position);
                }
            }
        }










        private static void HarassLogic()
        {
            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (!target.IsValidTarget()) return;
                if (W.IsReady() && mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled && target.IsValidTarget(500))
                {
                    W.Cast();
                }
                if (Q.IsReady() && mainMenu["Harass"].GetValue<MenuBool>("Q1use").Enabled) { CastQ1(); }
                    if (Q.IsReady() && mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled )
                { target = TargetSelector.GetTarget(Q.Range + 500, DamageType.Physical);
                var allMinion = GameObjects.GetMinions(target.Position, Q.Range, MinionTypes.All, MinionTeam.Enemy);
                if (target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);

                }
                var nearestMinion =
                    allMinion.Where(
                    minion =>
                    minion.Distance(ObjectManager.Player) <= target.Distance(ObjectManager.Player) &&
                            target.Distance(minion) < 450)
                        .OrderBy(minion => minion.Distance(ObjectManager.Player))
                        .FirstOrDefault();
                {
                    if (nearestMinion != null && nearestMinion.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(nearestMinion);
                    }
                } }

                   

                if (E.CanCast(target) && mainMenu["Harass"].GetValue<MenuBool>("Euse").Enabled)
                {
                    E.Cast(target);
                }
            }

        }
        private static void LaneClear()
        {
            
                var useQ = mainMenu["LaneClear"].GetValue<MenuBool>("Quse").Enabled;

                if (Q.IsReady() && useQ)
                {
                    var vMinions = GameObjects.GetMinions(ObjectManager.Player.Position, Q.Range);
                    foreach (
                        var minions in
                            vMinions.Where(
                                minions =>
                                    minions.Health < ObjectManager.Player.GetSpellDamage(minions, SpellSlot.Q) - 20))
                        Q.Cast(minions);
                }
           
        }
        
        private static bool IsSafe()
        {
            return ObjectManager.Player.CountEnemyHeroesInRange(ObjectManager.Player.AttackRange) <=
                   ObjectManager.Player.CountAllyHeroesInRange(ObjectManager.Player.AttackRange);
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead || GameObjects.Player.IsRecalling()) return;
            var ultCasting = ObjectManager.Player.IsCastingImporantSpell();
            //var ultCasting = Game.Time - UltiCastedTime < 0.2 || ObjectManager.Player.IsCastingImporantSpell();
            if (ultCasting)
            {

                Orbwalker.AttackEnabled = false;
                Orbwalker.MoveEnabled = false;
            }
            else
            {
                Orbwalker.AttackEnabled = true;
                Orbwalker.MoveEnabled = true;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    ComboLogic();
                    break;
                case OrbwalkerMode.Harass:
                    HarassLogic();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;
            }
        }


        private static void OnDraw(EventArgs args)
        {
            var PlayerPos = GameObjects.Player.Position;
            
            if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
              if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled) if (Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan);
              if (mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled) if (W.IsReady()) Render.Circle.DrawCircle(PlayerPos, W.Range, System.Drawing.Color.Silver);
              if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled) if (E.IsReady()) Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow);
              if (mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled) if (R.IsReady()) Render.Circle.DrawCircle(PlayerPos, R.Range, System.Drawing.Color.Blue);
                 
            }

        }



    }
}

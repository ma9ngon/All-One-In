using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace SimpleAIO.Champions
{
    internal class Rumble{
        private static Spell Q,W,E,R;
        private static Menu mainMenu;
        private static readonly int maxRangeR = 2600;
        private static readonly int lengthR = 900;
        private static readonly int speedR = 1050;
        private static readonly int rangeR = 1700;
       
        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Rumble") return;

            Q = new Spell(SpellSlot.Q, 500f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 950f);
            R = new Spell(SpellSlot.R, rangeR);
            var Rmax = new Spell(SpellSlot.R, 2600);


            Q.SetSkillshot(0,350f,float.MaxValue,false,SpellType.Cone);
            E.SetSkillshot(0.25f, 40f,2000f,true,SpellType.Line);
            R.SetSkillshot( 0.5833f,200f,1600f,false,SpellType.Line);

          
            mainMenu = new Menu("Rumble ","Rumble ",true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Ruse1","Use R",true));
           // Combo.Add(new MenuBool("Ruse2","Use R logic 2",false));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            Harass.Add(new MenuBool("Wuse","Use W",true));
            Harass.Add(new MenuBool("Euse","Use E",true));
           // Harass.Add(new MenuBool("Ruse","Use R",true));
            Harass.Add(new MenuSlider("mana%","Mana porcent",50,0,100));
            mainMenu.Add(Harass);
            var Misc = new Menu("Misc","Misc Settings");
            Misc.Add(new MenuSlider("QHeat","Dont use Q if Heat =",79,0,100));
            Misc.Add(new MenuSlider("EHeat","Dont use E if Heat =",89,0,100));

            Misc.Add(new MenuBool("Wuse","keep Heating",true));
            mainMenu.Add(Misc);
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("RRange","Draw R range",false));

            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
           
   
           
        }
        private static void HeatMangement()
        {
            if(mainMenu["Misc"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && GameObjects.Player.ManaPercent < 40)
            {
                
               W.Cast();
               
            }
        }
        
        private static void ComboLogic()
        {
           

            var targetQ = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            var inputQ = Q.GetPrediction(targetQ);

           
            var targetE = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var inputE = E.GetPrediction(targetE);

            var Rtarget =TargetSelector.GetTarget(maxRangeR, DamageType.Magical);

            if(mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() &&
            targetQ.IsValidTarget()&& 
            inputQ.Hitchance >= HitChance.VeryHigh &&
            mainMenu["Misc"].GetValue<MenuSlider>("QHeat").Value > GameObjects.Player.ManaPercent)
            {
                
               Q.Cast(inputQ.CastPosition);
               
            }
            
            if(mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && GameObjects.Player.ManaPercent < 40)
            {
                
               W.Cast();
               
            }

            if(mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() &&
             targetE.IsValidTarget()&& inputE.Hitchance >= HitChance.VeryHigh &&
              mainMenu["Misc"].GetValue<MenuSlider>("EHeat").Value > GameObjects.Player.ManaPercent)
            {
                
               E.Cast(inputE.CastPosition);
               
            }

            if(mainMenu["Combo"].GetValue<MenuBool>("Ruse1").Enabled && R.IsReady()  )
            {
               

                   PredictCastR(Rtarget);

            }
           
        }
        

        private static void HarassLogic()
        {
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var inputQ = Q.GetPrediction(targetQ);

           
            var targetE = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var inputE = E.GetPrediction(targetE);

             if(mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() &&
            targetQ.IsValidTarget()&& 
            inputQ.Hitchance >= HitChance.VeryHigh &&
            mainMenu["Misc"].GetValue<MenuSlider>("QHeat").Value > GameObjects.Player.ManaPercent)
            {
                
               Q.Cast(inputQ.CastPosition);
               
            }
            
            if(mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && GameObjects.Player.ManaPercent < 40)
            {
                
               W.Cast();
               
            }

            if(mainMenu["Harass"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() &&
             targetE.IsValidTarget()&& inputE.Hitchance >= HitChance.VeryHigh &&
              mainMenu["Misc"].GetValue<MenuSlider>("EHeat").Value > GameObjects.Player.ManaPercent)
            {
                
               E.Cast(inputE.CastPosition);
               
            }

            
           
        }

       
      

        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead || GameObjects.Player.IsRecalling()) return;
            if (mainMenu["Misc"].GetValue<MenuBool>("Wuse").Enabled)
	{ 
            HeatMangement();

	}
            switch (Orbwalker.ActiveMode){
                case OrbwalkerMode.Combo:
                    ComboLogic();
                    break;
                    case OrbwalkerMode.Harass:
                    
                    HarassLogic();
                    break;
                case OrbwalkerMode.LaneClear:
                   
                    break;
            }
        }
        private static void OnDraw(EventArgs args)
        {
            var PlayerPos = GameObjects.Player.Position;
            if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled && Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled && W.IsReady()) Render.Circle.DrawCircle(PlayerPos, W.Range, System.Drawing.Color.Silver, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled && E.IsReady()) Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled && R.IsReady()) Render.Circle.DrawCircle(PlayerPos, R.Range, System.Drawing.Color.Green, 1);
            }
        }

       // private void HandleR(AIBaseClient target, bool manual = false)
        //{
        //    if
        //    {
        //        var targE = R.GetPrediction(target);
        //        if ((mainMenu["Combo"].GetValue<MenuBool>("Ruse2").Enabled && GameObjects.Player.CountEnemiesInRange(R.Range + 175) <= 1 &&
        //             )
        //        {
        //            if (target.IsMoving)
        //            {
        //                var pos = targE.CastPosition;
        //                if (pos.IsValid() && pos.Distance(player.Position) < R.Range + 1000 &&
        //                    targE.Hitchance >= HitChance.High)
        //                {
        //                    R.Cast(target.Position.Extend(pos, -500), pos);
        //                }
        //            }
        //            else
        //            {
        //                R.Cast(target.Position.Extend(player.Position, 500), target.Position);
        //            }
        //        }
        //        else if (targE.Hitchance >= HitChance.High )
        //        {
        //            var pred = getBestRVector3(target, targE);
        //            if (pred != Vector3.Zero &&
        //                CombatHelper.GetCollisionCount(
        //                    target, target.Position.Extend(pred, 1000), R.Width, new[] { CollisionableObjects.Heroes, }) >=
        //                config.Item("Rmin", true).GetValue<Slider>().Value)
        //            {
        //                R.Cast(target.Position.Extend(pred, -target.MoveSpeed), pred);
        //            }
        //        }
        //    }
        //}

        //private Vector3 getBestRVector3(Obj_AI_Base target, PredictionOutput targE)
        //{
        //    var otherHeroes =
        //        HeroManager.Enemies.Where(
        //            e => e.IsValidTarget() && e.NetworkId != target.NetworkId && player.Distance(e) < 1000)
        //            .Select(e => R.GetPrediction(e))
        //            .Where(o => o.Hitchance > HitChance.High && o.CastPosition.Distance(targE.UnitPosition) < 1000);
        //    if (otherHeroes.Any())
        //    {
        //        var best =
        //            otherHeroes.OrderByDescending(
        //                hero =>
        //                    CombatHelper.GetCollisionCount(
        //                        target, target.Position.Extend(hero.CastPosition, 1000), R.Width,
        //                        new[] { CollisionableObjects.Heroes, })).FirstOrDefault();
        //        if (best != null)
        //        {
        //            return best.CastPosition;
        //        }
        //    }
        //    return Vector3.Zero;
        //}

        //private IEnumerable<Vector3> GetRpoints(Obj_AI_Base target)
        //{
        //    var targetPos = R.GetPrediction(target);
        //    return
        //        CombatHelper.PointsAroundTheTargetOuterRing(targetPos.CastPosition, 345, 16)
        //            .Where(p => player.Distance(p) < R.Range);
        //}
        private static void PredictCastR(AIHeroClient target)
        {
            var inRange = SharpDX.Vector2.DistanceSquared(target.Position.ToVector2(), ObjectManager.Player.Position.ToVector2()) < R.Range * R.Range;
            PredictionOutput prediction;
            var spellCasted = false;
            Vector3 pos1, pos2;

            var nearChamps = (from champ in ObjectManager.Get<AIHeroClient>() where champ.IsValidTarget(maxRangeR) && target != champ select champ).ToList();
            var innerChamps = new List<AIHeroClient>();
            var outerChamps = new List<AIHeroClient>();

            foreach (var champ in nearChamps)
            {
                if (SharpDX.Vector2.DistanceSquared(champ.Position.ToVector2(), ObjectManager.Player.Position.ToVector2()) < R.Range * R.Range)
                    innerChamps.Add(champ);
                else
                    outerChamps.Add(champ);
            }

            var nearMinions = GameObjects.GetMinions(ObjectManager.Player.Position, maxRangeR);
            var innerMinions = new List<AIBaseClient>();
            var outerMinions = new List<AIBaseClient>();
            foreach (var minion in nearMinions)
            {
                if (SharpDX.Vector2.DistanceSquared(minion.Position.ToVector2(), ObjectManager.Player.Position.ToVector2()) < R.Range * R.Range)
                    innerMinions.Add(minion);
                else
                    outerMinions.Add(minion);
            }

            if (inRange)
            {
                R.Speed = speedR * 0.9f;
                R.From = target.Position + (SharpDX.Vector3.Normalize(ObjectManager.Player.Position - target.Position) * (lengthR * 0.1f));
                prediction = R.GetPrediction(target);
                R.From = ObjectManager.Player.Position;

                if (prediction.CastPosition.Distance(ObjectManager.Player.Position) < R.Range)
                    pos1 = prediction.CastPosition;
                else
                {
                    pos1 = target.Position;
                    R.Speed = speedR;
                }

                R.From = pos1;
                R.RangeCheckFrom = pos1;
                R.Range = lengthR;

                if (nearChamps.Count > 0)
                {
                    var closeToPrediction = new List<AIHeroClient>();
                    foreach (var enemy in nearChamps)
                    {
                        prediction = R.GetPrediction(enemy);

                        if (prediction.Hitchance >= HitChance.High && SharpDX.Vector2.DistanceSquared(pos1.ToVector2(), prediction.CastPosition.ToVector2()) < (R.Range * R.Range) * 0.8)
                            closeToPrediction.Add(enemy);
                    }

                    if (closeToPrediction.Count > 0)
                    {
                        if (closeToPrediction.Count > 1)
                            closeToPrediction.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                        prediction = R.GetPrediction(closeToPrediction[0]);
                        pos2 = prediction.CastPosition;

                        CastR(pos1, pos2);
                        spellCasted = true;
                    }
                }

                if (!spellCasted)
                {
                    CastR(pos1, R.GetPrediction(target).CastPosition);
                }

                R.Speed = speedR;
                R.Range = rangeR;
                R.From = ObjectManager.Player.Position;
                R.RangeCheckFrom = ObjectManager.Player.Position;
            }
            else
            {
                float startPointRadius = 150;

                SharpDX.Vector3 startPoint = ObjectManager.Player.Position + SharpDX.Vector3.Normalize(target.Position - ObjectManager.Player.Position) * rangeR;

                var targets = (from champ in nearChamps where SharpDX.Vector2.DistanceSquared(champ.Position.ToVector2(), startPoint.ToVector2()) < startPointRadius * startPointRadius && SharpDX.Vector2.DistanceSquared(ObjectManager.Player.Position.ToVector2(), champ.Position.ToVector2()) < rangeR * rangeR select champ).ToList();
                if (targets.Count > 0)
                {
                    if (targets.Count > 1)
                        targets.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                    pos1 = targets[0].Position;
                }
                else
                {
                    var minionTargets = (from minion in nearMinions where SharpDX.Vector2.DistanceSquared(minion.Position.ToVector2(), startPoint.ToVector2()) < startPointRadius * startPointRadius && SharpDX.Vector2.DistanceSquared(ObjectManager.Player.Position.ToVector2(), minion.Position.ToVector2()) < rangeR * rangeR select minion).ToList();
                    if (minionTargets.Count > 0)
                    {
                        if (minionTargets.Count > 1)
                            minionTargets.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                        pos1 = minionTargets[0].Position;
                    }
                    else
                        pos1 = startPoint;
                }

                R.From = pos1;
                R.Range = lengthR;
                R.RangeCheckFrom = pos1;
                prediction = R.GetPrediction(target);

                if (prediction.Hitchance >= HitChance.High)
                    CastR(pos1, prediction.CastPosition);

                R.Range = rangeR;
                R.From = ObjectManager.Player.Position;
                R.RangeCheckFrom = ObjectManager.Player.Position;
            }
        }
        private static void CastR(SharpDX.Vector3 source, SharpDX.Vector3 destination)
        {
            R.Cast(source, destination);
        }

        private static void CastR(SharpDX.Vector2 source, SharpDX.Vector2 destination)
        {
            R.Cast(source, destination);
        }
    }
}

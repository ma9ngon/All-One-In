using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace SimpleAIO.Champions{
    internal class Jayce{
        private static Spell Q,W,E,R,Q2,Q3,Q4,W2,E2;
        private static Menu mainMenu;
        public static void OnGameLoad(){
            //Melee
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W, 285f);
            E = new Spell(SpellSlot.E, 650f);  
            R = new Spell(SpellSlot.R);

            //Ranged
            Q2 = new Spell(SpellSlot.Q, 1050);
            Q3 = new Spell(SpellSlot.Q, 1470);
            Q4 = new Spell(SpellSlot.Q, 650f);
            W2 = new Spell(SpellSlot.W);
            E2 = new Spell(SpellSlot.E, 650f);

            


            Q2.SetSkillshot(0.2143f, 10f, 1450, true,SpellType.Line );
            Q3.SetSkillshot(0.2143f, 10f, 1890, true,SpellType.Line );
	    Q4.SetSkillshot(0.2143f, 10f, 650, true, SpellType.Line);
            Q.SetTargetted(0f, float.MaxValue);
            E.SetTargetted(0f, float.MaxValue);



           
          
            mainMenu = new Menu("Jayce","Jayce",true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Q2use","Use Q Ranged",true));
            Combo.Add(new MenuBool("Q3use","Use EQ ",true));
            Combo.Add(new MenuBool("Q4use","Use EQ at Q range Bug",true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            Combo.Add(new MenuKeyBind ("flee","flee",Keys.Z,KeyBindType.Press,false));

            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            Harass.Add(new MenuBool("Wuse","Use W",true));
            Harass.Add(new MenuBool("Q2use","Use Q Ranged",true));
            Harass.Add(new MenuBool("Q3use","Use EQ ",true));
            Harass.Add(new MenuSlider("mana%","Mana porcent",50,0,100));
            mainMenu.Add(Harass);
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnAfterAttack += OnAfterAttack;

   
           
        }
        private static void Flee() 
        {
            if (mainMenu["Combo"].GetValue<MenuKeyBind>("flee").Active)
	        {
                R.Cast();
	        }
        }
       
        
        public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
               if (args.Target == null || !args.Target.IsValidTarget()) return;
                

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient &&
                mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && !Ismelee() )
            {
                if (W.Cast()) Orbwalker.ResetAutoAttackTimer();
            }
        }

        private static void ComboMelee() 
        {
         var targetQ = Q.GetTarget();
         var targetE = E.GetTarget();
         var targetW = W.GetTarget();

            if(mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget() && Q.IsInRange(targetQ))
            {
                    Q.Cast(targetQ);
               
                    
            }
            if (mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && targetW.IsValidTarget() && W.IsInRange(targetW))
	        {
                    W.Cast();
	        }

             if(mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() && targetE.IsValidTarget() && targetE.Health<(Q2.GetDamage(targetE)+ GameObjects.Player.GetAutoAttackDamage(targetE) * 4 ) )
             {
                    E.Cast(targetE);
                    R.Cast();
             }

             
        }
        
        private static void ComboLogic()
        {
            
            var targetQ2 = Q2.GetTarget();
            var targetQ3 = Q3.GetTarget();
	    var targetQ4 = Q4.GetTarget();
            var PlayerPos = GameObjects.Player.Position;

            var inputQ2 = Q2.GetPrediction(targetQ2);
            var inputQ3 = Q3.GetPrediction(targetQ3);
            var inputQ4 = Q4.GetPrediction(targetQ4);

            if(mainMenu["Combo"].GetValue<MenuBool>("Q2use").Enabled && Q.IsReady()&& !E2.IsReady() && targetQ2.IsValidTarget() && inputQ2.Hitchance >= HitChance.VeryHigh)
            {
                    Q2.Cast(inputQ2.CastPosition);
            }
            if (mainMenu["Combo"].GetValue<MenuBool>("Q4use").Enabled && Q.IsReady() && targetQ4.IsValidTarget() && inputQ4.Hitchance >= HitChance.VeryHigh && E2.IsReady())
            {
		E2.Cast(inputQ4.CastPosition);
                Q4.Cast(inputQ4.CastPosition);
                

            }
             if(mainMenu["Combo"].GetValue<MenuBool>("Q3use").Enabled && Q.IsReady() && targetQ3.IsValidTarget() && inputQ3.Hitchance >= HitChance.VeryHigh && E2.IsReady())
             {
                Q3.Cast(inputQ3.CastPosition);
                E2.Cast(PlayerPos);
                    
             }
		
        }

         public static bool Ismelee()
         {
            
             return GameObjects.Player.IsMelee ;
         }
 
        private static void HarassLogic()
        {
            var target = TargetSelector.GetTarget(W.Range,DamageType.Mixed);
            var inputW = W.GetPrediction(target);
            
           if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent && !Ismelee())
           {
                  var targetQ2 = Q2.GetTarget();
            var targetQ3 = Q3.GetTarget();
            var PlayerPos = GameObjects.Player.Position;

            var inputQ2 = Q2.GetPrediction(targetQ2);
            var inputQ3 = Q3.GetPrediction(targetQ2);
            

             if(mainMenu["Harass"].GetValue<MenuBool>("Q3use").Enabled && Q.IsReady() && targetQ3.IsValidTarget() && inputQ3.Hitchance >= HitChance.VeryHigh && E2.IsReady())
             {
                Q3.Cast(inputQ3.CastPosition);
                E2.Cast(PlayerPos);
                    
             }
                
             if(mainMenu["Harass"].GetValue<MenuBool>("Q2use").Enabled && Q.IsReady() && targetQ2.IsValidTarget() && inputQ2.Hitchance >= HitChance.VeryHigh)
             {
                    Q2.Cast(inputQ2.CastPosition);
             }
           }
        }

       
             
      

        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead) return;
            
            Flee();
            switch (Orbwalker.ActiveMode){
                case OrbwalkerMode.Combo:
                   
                    if (!Ismelee())
                        ComboLogic();
                    else
                        ComboMelee();
                    break;

                    case OrbwalkerMode.Harass:
                    HarassLogic();
                    break;
                case OrbwalkerMode.LaneClear:
                   
                    break;
            }
        }
        private static void OnDraw(EventArgs args){
            var PlayerPos = GameObjects.Player.Position;
            if (Ismelee())
	        {
              if(mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled){
                if(mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled){
                    if(Q.IsReady()){
                        Render.Circle.DrawCircle(PlayerPos, Q.Range,System.Drawing.Color.Cyan,1);
                    }
                }
                if(mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled){
                    if(W.IsReady()){
                        Render.Circle.DrawCircle(PlayerPos, W.Range,System.Drawing.Color.Silver,1);
                    }
                }
                if(mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled){
                    if(E.IsReady()){
                        Render.Circle.DrawCircle(PlayerPos, E.Range,System.Drawing.Color.Yellow,1);
                    }

                }
              }
            }
            else 
            {
              if(mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled){
                if(mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled){
                    if(Q.IsReady()){
                        Render.Circle.DrawCircle(PlayerPos, Q2.Range,System.Drawing.Color.Cyan,1);
                    }
                }
                
                if(mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled){
                    if(E.IsReady()){
                        Render.Circle.DrawCircle(PlayerPos, Q3.Range,System.Drawing.Color.Yellow,1);
                    }

                }
              }
            }
           
        }
    }
}

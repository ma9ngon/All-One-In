using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace SimpleAIO.Champions{
    internal class Illaoi{
        private static Spell Q,W,E,R;
        private static Menu mainMenu;
        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Illaoi") return;
            Q = new Spell(SpellSlot.Q, 825){ AddHitBox=true};
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 900){ AddHitBox=true};
            R = new Spell(SpellSlot.R,450);
            W.SetTargetted(0.25f,float.MaxValue);
            Q.SetSkillshot(0.75f, 10f, float.MaxValue, false,SpellType.Line);
            E.SetSkillshot(0.25f, 10f, 1900f, true,SpellType.Line);

            
           
            string [] hitChances = {"Low","Medium","High","VeryHigh"};
            mainMenu = new Menu("Illoi","Illaoi",true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));                 
            Combo.Add(new MenuList ("Qhit","HitChance",hitChances));
            Combo.Add(new MenuBool("Wuse","Use W Alwas",true));
            Combo.Add(new MenuBool("WuseAAresrt","Use W reset only",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            Harass.Add(new MenuBool("Wuse","Use W",true));
            Harass.Add(new MenuBool("Euse","Use E",true));
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

        public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
               if (args.Target == null || !args.Target.IsValidTarget()) return;
                

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient &&
                mainMenu["Combo"].GetValue<MenuBool>("WuseAAresrt").Enabled )
            {
                if (W.Cast()) Orbwalker.ResetAutoAttackTimer();
                
            }
        }
        
        
        private static void ComboLogic()
        {
            var targetQ = Q.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);
            var targetW = W.GetTarget();
            var targetE = E.GetTarget();
            var inputE = E.GetPrediction(targetE);
            var hitValu = mainMenu["Combo"].GetValue<MenuList>("Qhit").SelectedValue;
            var hit=HitChance.High;
            switch (hitValu)
	        {case "Low" : hit = HitChance.Low ;
                        break;
                    case "Medium": hit =HitChance.Medium;
                        break;
                    case "High": hit =HitChance.High;
                        break;
                    case "VeryHigh": hit =HitChance.VeryHigh;
                        break;
	        }


            if(mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget()&& inputQ.Hitchance >= hit && Q.IsInRange(inputQ.CastPosition))
            {
                    Q.Cast(inputQ.CastPosition);
            }

            if(mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && targetW.IsValidTarget() && W.IsInRange(targetW))
            {
                    W.Cast();
            }

            if(mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled &&E.IsReady() && targetE.IsValidTarget() && inputE.Hitchance >= HitChance.VeryHigh && E.IsInRange(inputE.CastPosition))
            {
                        E.Cast(inputE.CastPosition);
            }
            
        
        }
        private static void HarassLogic()
        {
            var targetQ = Q.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);
            var targetW = W.GetTarget();
            var targetE = E.GetTarget();
            var inputE = E.GetPrediction(targetE);

            if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent && mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget()&& inputQ.Hitchance >= HitChance.VeryHigh && Q.IsInRange(inputQ.CastPosition))
            {
                    Q.Cast(inputQ.CastPosition);
            }

            if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent && mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && targetW.IsValidTarget() && W.IsInRange(targetW))
            {
                    W.Cast();
            }

            if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent && mainMenu["Harass"].GetValue<MenuBool>("Euse").Enabled &&E.IsReady() && targetE.IsValidTarget() && inputE.Hitchance >= HitChance.VeryHigh && E.IsInRange(inputE.CastPosition))
            {
                        E.Cast(inputE.CastPosition);
            }
           
        }

        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead) return;
            
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
        private static void OnDraw(EventArgs args){
            var PlayerPos = GameObjects.Player.Position;

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
    }
}

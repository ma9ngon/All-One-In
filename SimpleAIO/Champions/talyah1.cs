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
    internal class Taliyah
    {
        private static Spell Q,W,E,R;
        private static Menu mainMenu;
        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Taliyah") return;

            Q = new Spell(SpellSlot.Q,1000);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R);
            Q.SetSkillshot(0.25f,60f,2000,true,SpellType.Line);
            W.SetSkillshot(0.25f,12f,float.MaxValue,false,SpellType.Circle);
            E.SetSkillshot(0.25f,400f,float.MaxValue,false,SpellType.Cone);

            mainMenu = new Menu("TALIYAH","TALIYAH",true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuSlider("Wcase", "1for bull 2 for push", 1, 1, 2));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            Harass.Add(new MenuBool("Quse","Use W",true));
            Harass.Add(new MenuBool("Quse","Use E",true));
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
        }
         
        
        
        private static void ComboLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            var input = Q.GetPrediction(target);
            var inputW = W.GetPrediction(target);
            var inputE = E.GetPrediction(target);
            if(mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && input.Hitchance >= HitChance.VeryHigh)
            {
             Q.Cast(input.CastPosition);
            }

            if(mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled&& W.IsReady()&&inputW.Hitchance >= HitChance.VeryHigh)
            {
                switch (mainMenu["Combo"].GetValue<MenuSlider>("Wcase").Value)
                {
                    case 1:
                       W.Cast(inputW.CastPosition, inputW.CastPosition.Extend(ObjectManager.Player.Position, 50));
                        break;
                    case 2:
                       W.Cast(inputW.CastPosition, inputW.CastPosition.Extend(ObjectManager.Player.Position, -50));
                        break;
                }
                
            }

            if(mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady()&& inputE.Hitchance >= HitChance.VeryHigh)
            {            
             E.Cast(inputE.CastPosition);
            }
            
        
        }
        private static void HarassLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var input = Q.GetPrediction(target);
            
            var inputE = E.GetPrediction(target);
            if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent){
                if(mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled){
                   if(Q.IsReady() && target.IsValidTarget() && input.Hitchance >= HitChance.VeryHigh)
                    {
                        Q.Cast(input.UnitPosition);
                    }
                }
            }
            if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent){
                if(mainMenu["Harass"].GetValue<MenuBool>("Euse").Enabled){
                    if(E.IsReady() && target.IsValidTarget() && inputE.Hitchance >= HitChance.VeryHigh)
                {
                        E.Cast(inputE.UnitPosition);
                }
                }
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
            if(mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled){
                if(mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled){
                    if(Q.IsReady()){
                        Render.Circle.DrawCircle(GameObjects.Player.Position, Q.Range,System.Drawing.Color.Cyan,1);
                    }
                }
                if(mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled){
                    if(W.IsReady()){
                        Render.Circle.DrawCircle(GameObjects.Player.Position, W.Range,System.Drawing.Color.Silver,1);
                    }
                }
                if(mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled){
                    if(E.IsReady()){
                        Render.Circle.DrawCircle(GameObjects.Player.Position, E.Range,System.Drawing.Color.Yellow,1);
                    }

                }
            }
        }
    }
}

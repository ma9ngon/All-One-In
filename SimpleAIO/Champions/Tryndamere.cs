#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
#endregion
namespace SimpleAIO.Champions{
    internal class Tryndamere  {
       
        #region declaring
        private static Spell Q,W,E,R;
        private static Menu mainMenu;
        #endregion
        #region OnGameLoad

        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Tryndamere") return;
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 660f);  
            R = new Spell(SpellSlot.R);
            //Targeting input
            E.SetSkillshot(0,93f,1300f,false,SpellType.Line);
            //Menu                 
            mainMenu = new Menu("Tryndamere","Tryndamere",true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuSlider("qBelow","-> If Hp <",40,0,95));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            Combo.Add(new MenuSlider("rBelow","R -> If Hp% <",13,0,95));
            mainMenu.Add(Combo);
            var Flee =new Menu("Flee","Flee Settings",true);
            Flee.Add(new MenuKeyBind("EFlee","use e",Keys.Z,KeyBindType.Press,false));
            mainMenu.Add(Flee);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            Harass.Add(new MenuBool("Wuse","Use W",true));
            Harass.Add(new MenuSlider("mana%","Mana porcent",50,0,100));
            mainMenu.Add(Harass);
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("RRange","Draw R range",true));
            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);
            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Dash.OnDash += OnDash;
        }
       #endregion 
        
        private static void OnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (sender.Distance(ObjectManager.Player.Position) > W.Range || !W.IsReady())
            {
                return;
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled &&
                args.EndPos.Distance(ObjectManager.Player.Position) > 125 &&
                args.EndPos.Distance(ObjectManager.Player) > args.StartPos.Distance(ObjectManager.Player) &&
                !sender.IsFacing(GameObjects.Player))
            {
                W.Cast();
            }
        }
        private static void Flee() 
        {
            if (mainMenu["Flee"].GetValue<MenuKeyBind>("EFlee").Active )
	        {
                E.Cast(Game.CursorPos);
                GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
          	}
        }

        #region Combo
        private static void ComboLogic()
        {
            
            var target = E.GetTarget();
            var targetW =W.GetTarget();
            var Eprd = E.GetPrediction(target);
             if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && GameObjects.Player.HealthPercent < mainMenu["Combo"].GetValue<MenuSlider>("qBelow").Value && !GameObjects.Player.HasBuff("UndyingRage"))
             {
                if (ObjectManager.Get<AIHeroClient>().Where(x => x.IsEnemy && x.Distance(GameObjects.Player.Position) <= 1200).Count() > 0)
                {
                    Q.Cast();
                }
             }
             if(mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && !targetW.IsFacing(GameObjects.Player)) { W.Cast(); }

             if (mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() &&Eprd.Hitchance >= HitChance.High )
            {
                
                E.Cast(Eprd.CastPosition);
            }

             if (mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady() && ObjectManager.Get<AIHeroClient>().Where(x => x.IsEnemy && x.Distance(GameObjects.Player.Position) <= 1100).Count() > 0 && GameObjects.Player.HealthPercent < mainMenu["Combo"].GetValue<MenuSlider>("rBelow").Value)
            {
                R.Cast();
            }
        }
        #endregion
        #region Harass
        private static void HarassLogic()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var inputW = W.GetPrediction(target);
            
           if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
           {
                 if(mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled)
                 {
                     if(W.IsReady() && target.IsValidTarget()&& inputW.Hitchance >= HitChance.VeryHigh)
                     {
                        W.Cast(inputW.CastPosition);
                     }
                 }
           }
        }
        #endregion
        #region OnGameUpdate

        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead) return;
            
            Flee() ;

            switch (Orbwalker.ActiveMode){


                case OrbwalkerMode.Combo:
                        ComboLogic();
                    break;

                    case OrbwalkerMode.Harass:
                   // HarassLogic();
                    break;
                case OrbwalkerMode.LaneClear:
                   
                    break;
            }
        }
        #endregion
        #region OnDraw
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
                if(mainMenu["Draw"].GetValue<MenuBool>("RRange").Enabled){
                    if(R.IsReady()){
                        Render.Circle.DrawCircle(PlayerPos, R.Range,System.Drawing.Color.Yellow,1);
                    }

                }
            }
        }
        #endregion
    }
}

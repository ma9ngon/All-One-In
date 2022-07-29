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
namespace YORICK{
    internal class Program{
       
        #region declaring
        private static Spell Q,W,E,R;
        private static Menu mainMenu;
        #endregion
        #region Main
        public static void Main(string[] args){
            GameEvent.OnGameLoad += OnGameLoad;
        }
        #endregion
        #region OnGameLoad

        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Yorick") return;
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 600f);
            E = new Spell(SpellSlot.E, 700f);  
            R = new Spell(SpellSlot.R, 600f);
            //Targeting input
            W.SetSkillshot(0,10f,float.MaxValue,false,SpellType.Circle);
            E.SetSkillshot(0,33f,1300f,false,SpellType.Line);
            //Menu                 
            mainMenu = new Menu("YORICK","YORICK",true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            mainMenu.Add(Combo);
            var Flee =new Menu("Flee","Flee Settings",true);
            Flee.Add(new MenuKeyBind("EFlee","use e",Keys.Z,KeyBindType.Press,false));
         //   mainMenu.Add(Flee);
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
            Orbwalker.OnAfterAttack +=OnAfterAttack;     
        }
       #endregion 
        #region QReset
        public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
         {
            if (args.Target == null || !args.Target.IsValidTarget()) return;
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient && mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled) if (Q.Cast()) Orbwalker.ResetAutoAttackTimer();
         }
        #endregion
        #region Combo
        private static void ComboLogic()
        {
            
            var target = E.GetTarget();
            var targetW =W.GetTarget();
            var Eprd = E.GetPrediction(target);
            var Wprd = W.GetPrediction(targetW);

             if(mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && Wprd.Hitchance >= HitChance.High && W.IsReady() && W.IsInRange(Wprd.CastPosition)) W.Cast(Wprd.CastPosition);

             if (mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() &&Eprd.Hitchance >= HitChance.High && E.IsInRange(Eprd.CastPosition))
            {
                
                E.Cast(Eprd.CastPosition);
            }

        }
        #endregion
        #region Harass
        private static void HarassLogic()
        {
           var target = E.GetTarget();
           var Eprd = E.GetPrediction(target);

           if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
           {
               if (mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() &&Eprd.Hitchance >= HitChance.VeryHigh && E.IsInRange(Eprd.CastPosition)) E.Cast(Eprd.CastPosition);
           }
        }
        #endregion
        #region OnGameUpdate

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

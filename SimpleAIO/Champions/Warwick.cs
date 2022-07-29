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
    internal class Warwick
    {
        private static Spell Q,W,E,R;
        private static Menu mainMenu;
        private static float Rrang = 700;
        public static void OnGameLoad(){

            
            if (GameObjects.Player.CharacterName != "Warwick") return;
            

            Q = new Spell(SpellSlot.Q, 350f);
            
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 375f);
            R = new Spell(SpellSlot.R, Rrang);
            Q.SetTargetted(0.25f,float.MaxValue);
            R.SetSkillshot(.1f,10f,float.MaxValue,false,SpellType.Line);
            mainMenu = new Menu("Warwick","Warwick",true);
            var Combo = new Menu("Combo","Combo Settings");
            //Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Quse2", "Use Q  at dashing target", true));
            Combo.Add(new MenuBool("Quse3", "Use Q", true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            //Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("EuseInstant","Use E",true));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            mainMenu.Add(Combo);
            //var Harass = new Menu("Harass","Harass Settings");
            //Harass.Add(new MenuBool("Quse","Use Q",true));
            //Harass.Add(new MenuBool("Wuse","Use W",true));
            //Harass.Add(new MenuSlider("mana%","Mana porcent",50,0,100));
            //mainMenu.Add(Harass);
        
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("rRange","Draw R range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            //Orbwalker.OnAfterAttack += OnAfterAttack;
            Dash.OnDash += OnDash;

        }
        private static void OnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (sender.Distance(ObjectManager.Player.Position) > Q.Range || !Q.IsReady())
            {
                return;
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && mainMenu["Combo"].GetValue<MenuBool>("Quse2").Enabled &&
                args.EndPos.Distance(ObjectManager.Player.Position) > Q.Range &&
                args.EndPos.Distance(ObjectManager.Player) > args.StartPos.Distance(ObjectManager.Player))
            {
                Q.CastOnUnit(sender);
            }
        }

        private static void ComboLogic()
        {
            Rrang = GameObjects.Player.MoveSpeed * 200 / 100;
            var targetQ = Q.GetTarget();
            var targetE = E.GetTarget();
            var targetR = R.GetTarget();
           // double vIn = 2*GameObjects.Player.GetAutoAttackDamage(targetR);
            //float vOut = Convert.ToSingle(vIn);
            var RDmgL1 = 175 + (167 / 100 * GameObjects.Player.FlatPhysicalDamageMod);
            var RDmgL2 = 350 + (167 / 100 * GameObjects.Player.FlatPhysicalDamageMod);
            var RDmgL3 = 525 + (167 / 100 * GameObjects.Player.FlatPhysicalDamageMod);
            var RDmg = 175f;
            switch (R.Level)
            {
                case 2:
                    RDmg = RDmgL2;
                    break;
                case 3:
                    RDmg = RDmgL3;
                    break;
                default:
                    RDmg = RDmgL1;
                    break;
            }
            var inputR = R.GetPrediction(targetR);
            //var inputQ2 = Q2.GetPrediction(targetR);
            if(mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget() && Q.IsInRange(targetQ))  Q.Cast(targetQ);
            //if(mainMenu["Combo"].GetValue<MenuBool>("EuseInstant").Enabled && E.IsReady() && targetE.IsValidTarget())  E.Cast(targetE);
            if(mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() && targetE.IsValidTarget() && !targetE.HasBuff("WarwickE")&& E.IsInRange(targetE))  E.Cast();
            if(mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady() && targetR.IsValidTarget()&& inputR.Hitchance >= HitChance.High && RDmg >= targetR.Health)  R.Cast(inputR.CastPosition);
            //WarwickE
        }

        //public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
        //{
        //    if (args.Target == null || !args.Target.IsValidTarget()) return;
        //    var target = Q2.GetTarget();
        //    if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient && mainMenu["Combo"].GetValue<MenuBool>("Quse2").Enabled) if (Q2.Cast()) Orbwalker.ResetAutoAttackTimer();
        //}
        private static void HarassLogic()
        {
           // var target = TargetSelector.GetTarget(W.Range);
           // var inputW = W.GetPrediction(target);
            
           //if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
           //{
           //      if(mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled)
           //      {
           //          if(W.IsReady() && target.IsValidTarget()&& inputW.Hitchance >= HitChance.VeryHigh)
           //          {
           //             W.Cast(inputW.CastPosition);
           //          }
           //      }
           //}
        }

      
        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead) return;
            Rrang = GameObjects.Player.MoveSpeed * 200 / 100;
            switch (Orbwalker.ActiveMode){
                case OrbwalkerMode.Combo:
                    ComboLogic();
                    break;
                    case OrbwalkerMode.Harass:
                    //HarassLogic();
                    break;
                case OrbwalkerMode.LaneClear:
                   
                    break;
            }
        }
        private static void OnDraw(EventArgs args){
           var PlayerPos = GameObjects.Player.Position;
            if(mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled){
                if(mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled) if(Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range,System.Drawing.Color.Cyan,1);
                if(mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled) if(W.IsReady()) Render.Circle.DrawCircle(PlayerPos, W.Range,System.Drawing.Color.Silver,1);
                if(mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled) if(E.IsReady())Render.Circle.DrawCircle(PlayerPos, E.Range,System.Drawing.Color.Yellow,1);
                if(mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled) if(R.IsReady())Render.Circle.DrawCircle(PlayerPos, R.Range,System.Drawing.Color.DeepSkyBlue,1);
            }
        }
    }
}

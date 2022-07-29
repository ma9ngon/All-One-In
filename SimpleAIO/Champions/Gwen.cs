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
    internal class Gwen{
       
        #region declaring
        private static Spell Q,W,E,R,R2,EQ,AA;
        private static Menu mainMenu;
        //private static AIHeroClient _target;
        #endregion
        #region OnGameLoad
        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Gwen") return;
            
            Q = new Spell(SpellSlot.Q, 450f );
            W = new Spell(SpellSlot.W, 370f);
            E = new Spell(SpellSlot.E ,400f);  
            R = new Spell(SpellSlot.R, 1200f);
            R2 = new Spell(SpellSlot.R, 1200f);
            EQ = new Spell(SpellSlot.Q, 750f);
            AA = new Spell(SpellSlot.Unknown,250f);

            //Targeting input
            Q.SetSkillshot(.5f,12f ,float.MaxValue,false,SpellType.Line);
            EQ.SetSkillshot(.5f, 12f , float.MaxValue, false, SpellType.Line);
            E.SetSkillshot(0,12f,float.MaxValue,false,SpellType.Line);

            R.SetSkillshot(.25f,10,500,false,SpellType.Line);
            R.SetSkillshot(.25f, 10, 500, false, SpellType.Line);
            R2.SetSkillshot(.5f, 10, 500, false, SpellType.Line);


            mainMenu = new Menu("Gwen", "Gwen", true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Qfull","Use Q 4 stack only",true));
            Combo.Add(new MenuBool("EQuse","Use EQ",true));
            Combo.Add(new MenuBool("EQfull", "Use EQ 4 stack only", true));

            Combo.Add(new MenuBool("Euse","Use E auto reset",true));
            Combo.Add(new MenuBool("Euse2","Use E gap closer",true));
            Combo.Add(new MenuBool("Ruse","Use R " ,true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse", "Use Q", true));
            Harass.Add(new MenuBool("Qfull", "Use Q 4 stack only", true));
            Harass.Add(new MenuBool("EQuse", "Use EQ", true));
            Harass.Add(new MenuBool("EQfull", "Use EQ 4 stack only", true));
            Harass.Add(new MenuSlider("mana%","Mana porcent",50,0,100));
            mainMenu.Add(Harass);
            var Misc = new Menu("Misc","Misc");
            Misc.Add(new MenuBool("1", "ResetAutoAttackTimer", true));
            Misc.Add(new MenuBool("2", "CancelAnimation", true));
            Misc.Add(new MenuBool("3", "Nothing", true));
          //  mainMenu.Add(Misc);
        
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("rRange","Draw R range",true));
            Draw.Add(new MenuBool("eqRange", "Draw EQ range", true));
            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnAfterAttack += OnAfterAttack;
        }
        #endregion
        #region OnAfterAttack
        public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            if (args.Target == null || !args.Target.IsValidTarget()) return;
            var targetE = E.GetTarget();
            var movePos = Game.CursorPos;
            if (GameObjects.Player.Distance(targetE.Position) < 600)
            {
                movePos = GameObjects.Player.Position.Extend(targetE.Position, 100);
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient && mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled) if (E.Cast(movePos)) Orbwalker.ResetAutoAttackTimer();
        }
        #endregion
        #region Combo
        private static void ComboLogic()
        {
            var targetQ = Q.GetTarget();
            var targetE = E.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);
            var targetEQ = EQ.GetTarget();
            var inputEQ = EQ.GetPrediction(targetEQ);
            var targerR = R.GetTarget();
            var predR = R.GetPrediction(targerR);
            bool fullstackQ = GameObjects.Player.GetBuffCount("GwenQ") == 4;
            if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.High)
            {
                if ((mainMenu["Combo"].GetValue<MenuBool>("Qfull").Enabled && fullstackQ) || !mainMenu["Combo"].GetValue<MenuBool>("Qfull").Enabled)
                {
                 Q.Cast(inputQ.CastPosition);
                }
            }
            if (mainMenu["Combo"].GetValue<MenuBool>("Euse2").Enabled && targetE.IsValidTarget() && !AA.IsInRange(targetE) )
            {
                E.Cast(targetE.Position);
            }
            if (mainMenu["Combo"].GetValue<MenuBool>("EQuse").Enabled && targetEQ.IsValidTarget() && inputEQ.Hitchance >= HitChance.High && E.IsReady() && Q.IsReady() && !Q.IsInRange(targetEQ))
            {
                if ((mainMenu["Combo"].GetValue<MenuBool>("EQfull").Enabled && fullstackQ) || !mainMenu["Combo"].GetValue<MenuBool>("EQfull").Enabled)
                {
                    E.Cast(inputEQ.CastPosition);
                    EQ.Cast(inputEQ.CastPosition);
                }
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && targerR.IsValidTarget() && predR.Hitchance >= HitChance.High)
            {
                R.Cast(predR.CastPosition);
            }
        }

        #endregion
        #region Harass
        private static void HarassLogic()
        {
            var targetQ = Q.GetTarget();
            var targetE = E.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);
            var targetEQ = EQ.GetTarget();
            var inputEQ = EQ.GetPrediction(targetEQ);
            var targerR = R.GetTarget();
            var predR = R.GetPrediction(targerR);
            bool fullstackQ = GameObjects.Player.GetBuffCount("GwenQ") == 4;

            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
           {
                if (mainMenu["Harass"].GetValue<MenuBool>("EQuse").Enabled && targetEQ.IsValidTarget() && inputEQ.Hitchance >= HitChance.High && E.IsReady() && Q.IsReady() && !Q.IsInRange(targetEQ))
                {
                    if ((mainMenu["Harass"].GetValue<MenuBool>("EQfull").Enabled && fullstackQ) || !mainMenu["Combo"].GetValue<MenuBool>("EQfull").Enabled)
                    {
                        E.Cast(inputEQ.CastPosition);
                        EQ.Cast(inputEQ.CastPosition);
                    }

                }
                if (mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.High)
                {
                    if ((mainMenu["Harass"].GetValue<MenuBool>("Qfull").Enabled && fullstackQ) || !mainMenu["Combo"].GetValue<MenuBool>("Qfull").Enabled)
                    {
                        Q.Cast(inputQ.CastPosition);
                    }
                }
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
            if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled && Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled && W.IsReady()) Render.Circle.DrawCircle(PlayerPos, W.Range, System.Drawing.Color.Silver, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled && E.IsReady()) Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled && R.IsReady()) Render.Circle.DrawCircle(PlayerPos, R.Range, System.Drawing.Color.Green, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("eqRange").Enabled && Q.IsReady() && E.IsReady()) Render.Circle.DrawCircle(PlayerPos, EQ.Range, System.Drawing.Color.SteelBlue, 2);

            }
        }
        #endregion
    }
}

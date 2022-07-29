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
    internal class ryze{
        private static Spell Q,Q2,W,E,R;
        private static Menu mainMenu;

        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Ryze") return;
            Q = new Spell(SpellSlot.Q,1000f);
            Q2 = new Spell(SpellSlot.Q,1000f);
            W = new Spell(SpellSlot.W, 600f);
            E = new Spell(SpellSlot.E, 600f);
           // R = new Spell(SpellSlot.R);
            Q2.SetSkillshot(.25f,110f,1700f,true,SpellType.Line);
            Q.SetSkillshot(.25f,110f,1700f,false,SpellType.Line);
            W.SetTargetted(0.25f,float.MaxValue);
            E.SetTargetted(0.25f,float.MaxValue);
            string[] combos = { "weq", "ewq" };
            mainMenu = new Menu("ryze","Simpleryze",true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuList("ct","combotype",combos));
            //Combo.Add(new MenuBool("Ruse","Use R",true));
            //Combo.Add(new MenuBool("Qstart","wait to hit q alwas",true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            Harass.Add(new MenuBool("Wuse","Use W",true));
            Harass.Add(new MenuSlider("mana%","Mana percent",50,0,100));
            mainMenu.Add(Harass);
            var LaneClear = new Menu("LaneClear", "LaneClear",true);
            LaneClear.Add(new MenuBool("eq","LaneClear EQ",true));
            LaneClear.Add(new MenuSlider("mana%", "Mana percent", 50, 0, 100));
            mainMenu.Add(LaneClear);
            var flee = new Menu("flee","flee",true);
            flee.Add(new MenuKeyBind("flee","ew flee",Keys.Z,KeyBindType.Press,false));
            flee.Add(new MenuSliderButton("skinch","skin id",0,0,30,false));
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);
            mainMenu.Add(flee);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;



        }
        private static bool CanCastSpicialItem() 
        { 
            if (GameObjects.Player.HasItem(ItemId.Everfrost))
            {
                return GameObjects.Player.CanUseItem((int)ItemId.Everfrost);
                
            } return false;
        }
       
        
        private static void ComboLogic()
        {
            if (mainMenu["Combo"].GetValue<MenuList>("ct").Index==0)
            {
                var target = TargetSelector.GetTarget(-1, DamageType.Magical);
                var qTarget = TargetSelector.GetTarget(900f, DamageType.Magical);
                var castQ = !W.IsReady() && !E.IsReady() || target == null;
                if (castQ && Q.IsReady() && qTarget != null)
                {
                    if (Q.Cast(qTarget) == CastStates.SuccessfullyCasted)
                    {
                        return;
                    }
                }

                if (target != null)
                {
                    if (!W.IsReady())
                    {
                        if (E.Cast(target) == CastStates.SuccessfullyCasted)
                        {
                            return;
                        }
                    }

                    if (W.Cast(target) == CastStates.SuccessfullyCasted)
                    {
                        return;
                    }
                }

            }
            else 
            {

                var target = TargetSelector.GetTarget(-1, DamageType.Magical);
                var qTarget = TargetSelector.GetTarget(900f, DamageType.Magical);
                var castQ = !W.IsReady() && !E.IsReady() || target == null;
                if (castQ && Q.IsReady() && qTarget != null)
                {
                    if (Q.Cast(qTarget) == CastStates.SuccessfullyCasted)
                    {
                        return;
                    }
                }

                if (target != null)
                {
                    if (!E.IsReady())
                    {
                        if (W.Cast(target) == CastStates.SuccessfullyCasted)
                        {
                            return;
                        }
                    }

                    if (E.Cast(target) == CastStates.SuccessfullyCasted)
                    {
                        return;
                    }
                }
            }

            if (CanCastSpicialItem())
            {
            var target = TargetSelector.GetTarget(-1, DamageType.Magical);

            GameObjects.Player.UseItem(6656, target);
            }
            

            }
            
  

        private static void Laneclear()
        {
            
            if (mainMenu["LaneClear"].GetValue<MenuBool>("eq").Enabled && (Q.IsReady()|| E.IsReady()) && mainMenu["LaneClear"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
            {
                var minionsE = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range) && x.IsMinion() && x.HasBuff("RyzeE")).Cast<AIBaseClient>().ToList();
                var minions = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValidTarget(E.Range));
                if (minions == null)
                {
                    return;
                }

                E.CastOnUnit(minions);
                    
                if (minionsE.Any())
                {
                    var qFarmLocation = W.GetLineFarmLocation(minionsE);
                    if (qFarmLocation.Position.IsValid())
                    {
                        Q.Cast(qFarmLocation.Position);
                        return;
                    }
                }
            }

           
        }

        private static void HarassLogic()
        {
            var target = TargetSelector.GetTarget(-1, DamageType.Magical);
            var qTarget = TargetSelector.GetTarget(900f, DamageType.Magical);
            var inputQ = Q.GetPrediction(qTarget);
            
           if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
           {
                 if(mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && qTarget.IsValidTarget() && inputQ.Hitchance >= HitChance.VeryHigh)
                 {
                        Q.Cast(inputQ.CastPosition);
                 }
                if(mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && target.IsValidTarget())
                 {
                        W.Cast(target);
                 }
           }
        }

        private static void Flee()
        {
            if (mainMenu["flee"].GetValue<MenuKeyBind>("flee").Active)
            {
                var target = TargetSelector.GetTarget(-1, DamageType.Magical);
                if (target != null)
                {
                    if (!E.IsReady())
                    {
                        if (W.Cast(target) == CastStates.SuccessfullyCasted)
                        {
                            return;
                        }
                    }

                    if (E.Cast(target) == CastStates.SuccessfullyCasted)
                    {
                        return;
                    }
                }
                Orbwalker.AttackEnabled = false;
                GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }else Orbwalker.AttackEnabled = true;
        }
        private static void skinch()
        {
            if (mainMenu["flee"].GetValue<MenuSliderButton>("skinch").Enabled)
            {
                int skinut = mainMenu["flee"].GetValue<MenuSliderButton>("skinch").Value;

                if (GameObjects.Player.SkinId != skinut)
                    GameObjects.Player.SetSkin(skinut);
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
                    Laneclear();
                    break;
            }
            Flee();
            skinch();
        }

        [Obsolete]
        private static void OnDraw(EventArgs args){
            if(mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled){
                if(mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled){
                    if(Q.IsReady()){
                        Render.Circle.DrawCircle(GameObjects.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
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

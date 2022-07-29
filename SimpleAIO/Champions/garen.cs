using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace SimpleAIO.Champions{
    internal class Garen{
        private static Spell Q,W,E,R;
        private static Menu mainMenu;
        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Garen") return;
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325f);
            R = new Spell(SpellSlot.R, 400f);
            R.SetTargetted(0.435f,float.MaxValue);
            mainMenu = new Menu("Garen","Garen",true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Ecancel","Stop E After Shred",true));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            Harass.Add(new MenuBool("Wuse","Use W",true));
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
            Orbwalker.OnAfterAttack +=OnAfterAttack;     
        }
        private static void ComboLogic()
        {
            //var targetR = R.GetTarget();
            var targetE = E.GetTarget();
            if(mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() && targetE.IsValidTarget() && !GameObjects.Player.HasBuff("GarenE"))  E.Cast();
            if(mainMenu["Combo"].GetValue<MenuBool>("Ecancel").Enabled && targetE.HasBuff("gareneshred") && GameObjects.Player.HasBuff("GarenE")) E.Cast();
            
                
            if(mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady() && GameObjects.Player.CountEnemyHeroesInRange(R.Range) > 0)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && R.GetDamage(x) > x.Health && x.DistanceToPlayer() <= 400 && ValidUlt(x))) R.Cast(target);
            }
        }
        private static void HarassLogic()
        {
           // var target = TargetSelector.GetTarget(W.Range);
           // var inputW = W.GetPrediction(target);
           //if(mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent && mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && target.IsValidTarget()&& inputW.Hitchance >= HitChance.VeryHigh) W.Cast(inputW.CastPosition);
        }
        public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
         {
            if (args.Target == null || !args.Target.IsValidTarget()) return;
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient && mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled) if (Q.Cast()) Orbwalker.ResetAutoAttackTimer();
         }
        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead) return;
            if (GameObjects.Player.HasBuff("GarenE"))
            {
                Orbwalker.AttackEnabled = false;
                Orbwalker.MoveEnabled = false;
                GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
            else
            {
                Orbwalker.AttackEnabled = true;
                Orbwalker.MoveEnabled = true;
            }

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
        private static void logicE()
        {
            var target = E.GetTarget();
            if (!target.IsValidTarget() && E.ToggleState == SpellToggleState.On)
                E.Cast();

            if (!target.IsValidTarget(E.Range))
                return;

            if (target.InAutoAttackRange(E.Range) && E.ToggleState == SpellToggleState.Off)
            {
                E.Cast();
            }

            if (GameObjects.Player.CountEnemyHeroesInRange(E.Range) == 0 && E.ToggleState == SpellToggleState.On)
            {
                E.Cast();
            }

            if (target.HasBuff("gareneshred")&& E.ToggleState == SpellToggleState.On)
            {
                E.Cast();
            }
        }
        private static void OnDraw(EventArgs args){
           var PlayerPos = GameObjects.Player.Position;
            if(mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled){
                if(mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled) if(Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range,System.Drawing.Color.Cyan,1);
                if(mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled) if(W.IsReady()) Render.Circle.DrawCircle(PlayerPos, W.Range,System.Drawing.Color.Silver,1);
                if(mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled) if(E.IsReady())Render.Circle.DrawCircle(PlayerPos, E.Range,System.Drawing.Color.Yellow,1);
                if(mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled) if(R.IsReady())Render.Circle.DrawCircle(PlayerPos, R.Range,System.Drawing.Color.DarkGoldenrod,1);
                //if(mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled) if(E.IsReady()) Drawing.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Black);

            }
        }
        public static bool ValidUlt(AIHeroClient target)
        {
            return !(Invulnerable.Check(target)
                || target.HaveSpellShield()
                || target.HasBuffOfType(BuffType.SpellImmunity)
                || target.HasBuffOfType(BuffType.PhysicalImmunity)
                );
        }
    }
}

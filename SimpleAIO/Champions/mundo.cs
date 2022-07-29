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
    internal class DrMundo
    {
        private static Spell Q,W,E,R;
        private static Menu mainMenu;

        public static void OnGameLoad(){
            //if (GameObjects.Player.CharacterName != "DrMundo") return;

            Q = new Spell(SpellSlot.Q, 1050);
            W = new Spell(SpellSlot.W, 325);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 120f, 2000f, true, SpellType.Line);

            mainMenu = new Menu("Mundo ", "simple mundo", true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuSliderButton("Ruse","Use R at health %",20,0,100,true));
            
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            mainMenu.Add(Harass);
            //var Misc = new Menu("Misc","Misc Settings");
            //mainMenu.Add(Misc);
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("rRange","Draw R range",false));
            //MundoPCannister/objectname/DrMundoPlummunity
            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            //AIBaseClient.OnProcessSpellCast += AIBaseClient_OnProcessSpellCast;
            //AntiGapcloser.OnGapcloser += OnGapcloser;
            //AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
            Orbwalker.OnAfterAttack += Orbwalker_OnAfterAttack;
            //GameObject.OnCreate += GameObject_OnCreate;
        }

        //private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        //{
            
        //}

        private static void Orbwalker_OnAfterAttack(object sender, AfterAttackEventArgs e)
        {
            if (e.Target == null || !e.Target.IsValidTarget()) return;
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && e.Target is AIHeroClient ) if (E.Cast()) Orbwalker.ResetAutoAttackTimer();

        }

        private static void ComboLogic()
        {
            var target = Q.GetTarget();
            var pred = Q.GetPrediction(target);
            if (target.IsValidTarget()&& pred.Hitchance>= HitChance.VeryHigh)
            {
                Q.Cast(pred.CastPosition);
            }
            if (mainMenu["Combo"].GetValue<MenuSliderButton>("Ruse").Enabled)
            {
                if (GameObjects.Player.CountEnemyHeroesInRange(800) > 0 && GameObjects.Player.HealthPercent<= mainMenu["Combo"].GetValue<MenuSliderButton>("Ruse").Value)
                {
                    R.Cast();
                }
            }
            //SRU_Baron 
        }
        private static void HarassLogic()
        {
            var target = Q.GetTarget();
            var pred = Q.GetPrediction(target);
            if (target.IsValidTarget() && pred.Hitchance >= HitChance.VeryHigh)
            {
                Q.Cast(pred.CastPosition);
            }
        }
        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead || GameObjects.Player.IsRecalling()) return;
            
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

        #region drawings
        private static void OnDraw(EventArgs args)
        {
            var PlayerPos = GameObjects.Player.Position;
            if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled)
                    Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled) 
                    Render.Circle.DrawCircle(PlayerPos, W.Range, System.Drawing.Color.Silver, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled)
                    Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled)  
                    Render.Circle.DrawCircle(PlayerPos, R.Range, System.Drawing.Color.Blue, 1);
            }
           
        }
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (mainMenu["Draw"].GetValue<MenuBool>("rRangeMini").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
                {
                    if (R.IsReady())
                        MiniMap.DrawCircle(GameObjects.Player.Position, R.Range, System.Drawing.Color.Aqua, 1);
                }
                else
                    MiniMap.DrawCircle(GameObjects.Player.Position, R.Range, System.Drawing.Color.Aqua, 1);
            }
        }
        #endregion


        #region oktwcommon
        private static bool HardCC(AIHeroClient target)
        {

            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned)
            {
                return true;

            }
            else
                return false;
        }
        public static bool CanMove(AIHeroClient target)
        {
            return !((!target.IsWindingUp && !target.CanMove)
                || target.MoveSpeed < 50
                || target.HaveImmovableBuff());
        }
        public static bool CanHitSkillShot(AIBaseClient target, Vector3 start, Vector3 end, SpellData sdata)
        {
            if (target.IsValidTarget(float.MaxValue, false))
            {
                var pred = Prediction.GetPrediction(target, 0.25f)?.CastPosition;

                if (pred == null)
                {
                    return false;
                }

                if (sdata.LineWidth > 0)
                {
                    var powCalc = Math.Pow(sdata.LineWidth + target.BoundingRadius, 2);

                    if (pred.Value.ToVector2().DistanceSquared(end.ToVector2(), start.ToVector2(), true) <= powCalc || target.PreviousPosition.ToVector2().DistanceSquared(end.ToVector2(), start.ToVector2(), true) <= powCalc)
                    {
                        return true;
                    }
                }
                else if (target.Distance(end) < 50 + target.BoundingRadius || pred.Value.Distance(end) < 50 + target.BoundingRadius)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool ValidUlt(AIHeroClient target)
        {
            return !(Invulnerable.Check(target)
                || target.HaveSpellShield()
                || target.HasBuffOfType(BuffType.SpellImmunity)
                || target.HasBuffOfType(BuffType.PhysicalImmunity)
                );
        }


        #endregion

    }
}

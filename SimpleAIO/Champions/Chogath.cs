
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
    internal class Chogath
    {
        private static Spell Q, W, E, R;
        private static Menu mainMenu;
        public static void OnGameLoad()
        {
           if (GameObjects.Player.CharacterName != "Chogath") return;
            Q = new Spell(SpellSlot.Q,950f);
            W = new Spell(SpellSlot.W,700f);
            E = new Spell(SpellSlot.E,500f);
            R = new Spell(SpellSlot.R,175f);
            Q.SetSkillshot(0.5f, 10f, 1300, false, SpellType.Circle);
            W.SetSkillshot(0.5f, 60f , float.MaxValue, false,SpellType.Cone);
            R.SetTargetted(.25f,float.MaxValue);
            mainMenu = new Menu("Chogath", "Chogath", true);
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true));
            Combo.Add(new MenuBool("Wuse", "Use W", true));
            Combo.Add(new MenuBool("Euse", "Use E", true));
            Combo.Add(new MenuBool("Ruse", "Use R", true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass", "Harass Settings");
            Harass.Add(new MenuBool("Quse", "Use Q", true));
            Harass.Add(new MenuBool("Wuse", "Use W", true));
            Harass.Add(new MenuSlider("mana%", "Mana porcent", 50, 0, 100));
            mainMenu.Add(Harass);
            var Draw = new Menu("Draw", "Draw Settings");
            Draw.Add(new MenuBool("qRange", "Draw Q range", true));
            Draw.Add(new MenuBool("wRange", "Draw W range", true));
            Draw.Add(new MenuBool("eRange", "Draw E range", true));
            Draw.Add(new MenuBool("RRange", "Draw R range", true));
            Draw.Add(new MenuBool("lista", "Draw only if spell is ready", true));
            mainMenu.Add(Draw);
            var Misc = new Menu("Misc", "Misc Settings");
            Misc.Add(new MenuBool("Interrupter", "Interrupte W && Q", true));
            mainMenu.Add(Misc);
            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnAfterAttack += OnAfterAttack;
            Interrupter.OnInterrupterSpell += OnInterrupterSpell;
        }
        
       
        public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            if (args.Target == null || !args.Target.IsValidTarget()) return;


            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient &&
                mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled )
            {
                if (E.Cast()) Orbwalker.ResetAutoAttackTimer();
            }
        }
        
       
        static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!mainMenu["Misc"].GetValue<MenuBool>("Interrupter").Enabled)
            {
                return;
            }

            if (args.DangerLevel <= Interrupter.DangerLevel.Low)
            {
                return;
            }

            if (sender.IsAlly)
            {
                return;
            }

            if (sender.IsEnemy && W.IsReady() && sender.Distance(GameObjects.Player) <= W.Range)
            {
                W.Cast(sender);
            }
            else
            {
                if (sender.IsEnemy && Q.IsReady() && sender.Distance(GameObjects.Player) <= Q.Range)
                {
                    Q.Cast(sender);
                }
            }
        }
        
       

        private static void ComboLogic()
        {
            var targetQ = Q.GetTarget();
            var targetW = W.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);
            //var inputW = W.GetPrediction(targetW);
            var targetR = R.GetTarget();
            var RDmgL1 = (300 +(10/100 * GameObjects.Player.BonusHealth) +(50/100 * GameObjects.Player.AbilityResource)); 
            var RDmgL2 = (475 + (10 / 100 * GameObjects.Player.BonusHealth) + (50 / 100 * GameObjects.Player.AbilityResource));
            var RDmgL3 = (650 + (10 / 100 * GameObjects.Player.BonusHealth) + (50 / 100 * GameObjects.Player.AbilityResource));
            var RDmg = 300f;
            switch (GameObjects.Player.Level)
            { case 11: RDmg = RDmgL2;
                    break;
                case 16: RDmg = RDmgL3;
                    break;
                default: RDmg = RDmgL1;
                    break;
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.VeryHigh)
            {
                Q.Cast(inputQ.CastPosition);
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && targetW.IsValidTarget() && W.IsReady())
            {
                W.CastOnBestTarget ();
            }
            if (mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady()&& RDmg >= targetR.Health && targetR.IsValidTarget())
            {
                R.Cast(targetR);
            }

        }

        
        
        private static void HarassLogic()
        {
            var targetW = TargetSelector.GetTarget(W.Range,DamageType.Magical);
            var inputW = W.GetPrediction(targetW);
            var targetQ = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            var inputQ = W.GetPrediction(targetQ);
            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent) if (mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && targetW.IsValidTarget() && inputW.Hitchance >= HitChance.VeryHigh)   W.Cast(inputW.CastPosition);
            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent) if (mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.VeryHigh)   Q.Cast(inputQ.CastPosition);

        }
        
        

        private static void OnGameUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead) return;

            switch (Orbwalker.ActiveMode)
            {
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
        
        
        private static void OnDraw(EventArgs args)
        {
            var PlayerPos = GameObjects.Player.Position;

            if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled)
                {
                    if (Q.IsReady())
                    {
                        Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan, 1);
                    }
                }
                if (mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled)
                {
                    if (W.IsReady())
                    {
                        Render.Circle.DrawCircle(PlayerPos, W.Range, System.Drawing.Color.Silver, 1);
                    }
                }
                if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled)
                {
                    if (E.IsReady())
                    {
                        Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow, 1);
                    }

                }
                if (mainMenu["Draw"].GetValue<MenuBool>("RRange").Enabled)
                {
                    if (R.IsReady())
                    {
                        Render.Circle.DrawCircle(PlayerPos, R.Range, System.Drawing.Color.Yellow, 1);
                    }

                }
            }
        }
        
    }
}
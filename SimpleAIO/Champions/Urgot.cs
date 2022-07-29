
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
    internal class Urgot
    {
        private static Spell Q, W, E, R;
        private static Menu mainMenu;
        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "Urgot") return;
            Q = new Spell(SpellSlot.Q, 800f);
            W = new Spell(SpellSlot.W, 490f);
            E = new Spell(SpellSlot.E, 450f);
            R = new Spell(SpellSlot.R, 2500f);
            Q.SetSkillshot(0.75f, 10f,float.MaxValue, false, SpellType.Circle);
            E.SetSkillshot(0.45f, 10f, 1200, false, SpellType.Line);
            R.SetSkillshot(.5f,10f,float.MaxValue,false, SpellType.Line);
            mainMenu = new Menu("Urgot", "Urgot", true);
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true));
            Combo.Add(new MenuBool("Wuse", "Use W", true));
            Combo.Add(new MenuBool("Wdisable","disable w combo at when maxed",true));
            Combo.Add(new MenuBool("Euse", "Use E", true));
            Combo.Add(new MenuBool("Ruse", "Use R", true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass", "Harass Settings");
            Harass.Add(new MenuBool("Quse", "Use Q", true));
            Harass.Add(new MenuBool("Euse", "Use E", true));
            Harass.Add(new MenuSlider("mana%", "Mana porcent", 50, 0, 100));
            mainMenu.Add(Harass);
            var Draw = new Menu("Draw", "Draw Settings");
            Draw.Add(new MenuBool("qRange", "Draw Q range", true));
            Draw.Add(new MenuBool("wRange", "Draw W range", true));
            Draw.Add(new MenuBool("eRange", "Draw E range", true));
            Draw.Add(new MenuBool("RRange", "Draw R range", true));
            Draw.Add(new MenuBool("lista", "Draw only if spell is ready", true));
            mainMenu.Add(Draw);
            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }
        private static void ComboLogic()
        {
            var targetQ = Q.GetTarget();
            //var targetW = W.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);
            var targetR = R.GetTarget();
            var targetE = E.GetTarget();
            var inputE = E.GetPrediction(targetE);
            var inputR = R.GetPrediction(targetR);
            var RDmgL1 = (100 + (50  * GameObjects.Player.PercentBasePhysicalDamageAsFlatBonusMod) + ((25 / 100) * targetR.Health));
            var RDmgL2 = (225 + (50  * GameObjects.Player.PercentBasePhysicalDamageAsFlatBonusMod) + ((25 / 100) * targetR.Health));
            var RDmgL3 = (350 + (50  * GameObjects.Player.PercentBasePhysicalDamageAsFlatBonusMod) + ((25 / 100) * targetR.Health));
            var RDmg = 100f;
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

            if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.VeryHigh)
            {
                Q.Cast(inputQ.CastPosition);
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled )
            {
                LogicW();
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() && targetE.IsValidTarget() && inputE.Hitchance >= HitChance.VeryHigh)
            {
                E.Cast(inputE.CastPosition);
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady() && RDmg >= targetR.Health  && targetR.IsValidTarget() && inputR.Hitchance >= HitChance.VeryHigh)
            {
                R.Cast(inputR.CastPosition);
            }

        }

        private static void LogicW()
        {
            var toggleWOn = GameObjects.Player.HasBuff("UrgotW");
            var target = W.GetTarget();
            var Wdisable = mainMenu["Combo"].GetValue<MenuBool>("Wdisable").Enabled && W.Level == 5;
            if (!target.IsValidTarget() && toggleWOn)
                W.Cast();

            if (!target.IsValidTarget(W.Range)|| Wdisable)
                return;

            if (target.InAutoAttackRange(W.Range) && !toggleWOn)
            {
                W.Cast();
            }
            if (toggleWOn)
            {
                GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
            }
            

            if (GameObjects.Player.CountEnemyHeroesInRange(W.Range) == 0 && toggleWOn)
            {
                W.Cast();
            }
        }



        private static void HarassLogic()
        {
            var targetE =W.GetTarget();
            var inputE = E.GetPrediction(targetE);
            var targetQ = Q.GetTarget(Q.Range);
            var inputQ = W.GetPrediction(targetQ);
            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent) if (mainMenu["Harass"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() && targetE.IsValidTarget() && inputE.Hitchance >= HitChance.VeryHigh) E.Cast(inputE.CastPosition);
            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent) if (mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.VeryHigh) Q.Cast(inputQ.CastPosition);

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
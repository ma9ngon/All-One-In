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
    internal class Kayle
    {
        private static Spell Q, W, E, R;
        private static Menu mainMenu;
        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "Kayle") return;
            Q = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 525f);
            R = new Spell(SpellSlot.R, 900f);
            Q.SetSkillshot(0.5f, 150f, 1300, true, SpellType.Circle);
            W.SetTargetted(.25f,float.MaxValue);
            R.SetTargetted(1.5f, float.MaxValue);
            mainMenu = new Menu("Kayle", "Kayle", true);
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true));
            Combo.Add(new MenuBool("Wuse", "Use W", true));
            Combo.Add(new MenuBool("Euse", "Use E", true));
            Combo.Add(new MenuBool("Ruse", "Use R", true));
            Combo.Add(new MenuSlider("rBelow", "R -> If Hp% <", 13, 0, 95));
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
            Orbwalker.OnAfterAttack += OnAfterAttack;
            AIBaseClient.OnProcessSpellCast += Wlogic;
        }


        public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            if (args.Target == null || !args.Target.IsValidTarget()) return;


            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient &&
                mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && GameObjects.Player.Level >= 6)
            {
                if (E.Cast()) Orbwalker.ResetAutoAttackTimer();
            }
        }

        private static void Wlogic(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsMe|| sender == null || !sender.IsEnemy || sender.IsMinion() || W.Level < 1 || !sender.IsValid ||!mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled || GameObjects.Player.Spellbook.IsAutoAttack) return;
                var lowestally = GameObjects.AllyHeroes.Where(a => !a.IsDead && a.IsValidTarget(W.Range) && a.IsValid && !a.IsMe).OrderBy(a => a.Health).FirstOrDefault();

                //Will Prioritize W cast on Allies
                if (args.Target.IsMe && W.IsReady() && lowestally == null && sender.IsEnemy)
                    W.Cast(Game.CursorPos);

                if (lowestally.Distance(GameObjects.Player.Position) > W.Range) return;

                if (args.Target.IsMe && lowestally != null && W.IsReady() && sender.IsEnemy)
                    W.Cast(lowestally);

                //Ally Receiving Damage
                if (args.Target.Position.Distance(GameObjects.Player.Position) <= W.Range && args.Target.IsAlly && sender.IsEnemy && lowestally != null)
                    W.Cast(lowestally);
            }
            catch (Exception)
            {
            }
        }

        private static void ComboLogic()
        {
            var targetQ = Q.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);
            var targetE = E.GetTarget();
           

            if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.VeryHigh)
            {
                Q.Cast(inputQ.CastPosition);
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() && GameObjects.Player.Level < 6 && targetE.IsValidTarget())
            {
                E.Cast();
            }
            if (mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady() && ObjectManager.Get<AIHeroClient>().Where(x => x.IsEnemy && x.Distance(GameObjects.Player.Position) <= 1100).Count() > 0 && GameObjects.Player.HealthPercent < mainMenu["Combo"].GetValue<MenuSlider>("rBelow").Value)
            {
                R.Cast(GameObjects.Player);
            }
        }



        private static void HarassLogic()
        {
            var targetE = TargetSelector.GetTarget(W.Range,DamageType.Mixed);
            var targetQ = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            var inputQ = W.GetPrediction(targetQ);
            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent) if (mainMenu["Harass"].GetValue<MenuBool>("Euse").Enabled && E.IsReady() && targetE.IsValidTarget() && GameObjects.Player.Level<6) E.Cast();
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
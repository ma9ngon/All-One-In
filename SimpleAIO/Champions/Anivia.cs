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
namespace SimpleAIO.Champions
{
    internal class Anivia
    {

        #region declaring
        private static Spell Q, W, E, R;
        private static Menu mainMenu;
        public static GameObject AniviaQ;
        #endregion
        #region OnGameLoad
        public static void OnGameLoad()
        {

            Q = new Spell(SpellSlot.Q, 1100f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 600f);
            R = new Spell(SpellSlot.R, 750f);

            //Targeting input
            Q.SetSkillshot(.5f, 110 , 950f , false, SpellType.Line);
            E.SetTargetted(.25f,1600f);
            W.SetSkillshot(.25f,10f,float.MaxValue,false,SpellType.Circle);
            R.SetSkillshot(.25f,200, float.MaxValue, false, SpellType.Circle);
            


            mainMenu = new Menu("Anivia", "Anivia", true);
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true));
            Combo.Add(new MenuBool("Wuse", "use W", true));
            Combo.Add(new MenuBool("Euse", "Use E ", true));
            Combo.Add(new MenuBool("Ruse", "Use R ", true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass", "Harass Settings");
            Harass.Add(new MenuBool("Quse", "Use Q", true));
            Harass.Add(new MenuBool("Euse", "Use E ", true));
            Harass.Add(new MenuSlider("mana%", "Mana porcent", 50, 0, 100));
            mainMenu.Add(Harass);
            var Misc = new Menu("Misc", "Misc");
            Misc.Add(new MenuBool("1", "ResetAutoAttackTimer", true));
            Misc.Add(new MenuBool("2", "CancelAnimation", true));
            Misc.Add(new MenuBool("3", "Nothing", true));
            //  mainMenu.Add(Misc);
            var Draw = new Menu("Draw", "Draw Settings");
            Draw.Add(new MenuBool("qRange", "Draw Q range", true));
            Draw.Add(new MenuBool("wRange", "Draw W range", true));
            Draw.Add(new MenuBool("eRange", "Draw E range", true));
            Draw.Add(new MenuBool("rRange", "Draw R range", true));
            Draw.Add(new MenuBool("lista", "Draw only if spell is ready", true));
            mainMenu.Add(Draw);
            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
        }
        #endregion
        private static void LogicR()
        {
            var togglerOn = R.ToggleState == SpellToggleState.On;
            var target = R.GetTarget();
            var inputR = R.GetPrediction(target);
            if (!target.IsValidTarget() && togglerOn)
                R.Cast();

            if (!target.IsValidTarget(R.Range) || !mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled)
                return;

            if (target.InRange(R.Range) && !togglerOn)
            {
                R.Cast(inputR.CastPosition);
            }

            if (GameObjects.Player.CountEnemyHeroesInRange(W.Range) == 0 && togglerOn)
            {
                R.Cast();
            }
        }

        private static void OnCreate(GameObject objects, EventArgs args)
        {
            if (objects.Name == "FlashFrostSpell")
            {
                AniviaQ = objects;

            }
        }

        private static void OnDelete(GameObject objects, EventArgs args)
        {
            if (objects.Name == "FlashFrostSpell")
            {
                AniviaQ = null;
            }
        }

        #region Combo
        private static void ComboLogic()
        {
            var targetQ = Q.GetTarget();
            var targetE = E.GetTarget();
            var targetW = W.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);
            var inputW = W.GetPrediction(targetW);
            if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.High && Q.ToggleState == SpellToggleState.Off)
            {
                    Q.Cast(inputQ.CastPosition);
            }
            if (mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && targetE.IsValidTarget())
            {
                E.Cast(targetE);
            }
            if (mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled&& inputW.Hitchance >= HitChance.High)
            {
                W.Cast(inputW.CastPosition+50);
            }
            LogicR();
            if (AniviaQ.Position.CountEnemyHeroesInRange(225) >= 1 && GameObjects.Player.HasBuff("FlashFrost"))
            {
                Q.Cast();
            }
        }

        #endregion
        #region Harass
        private static void HarassLogic()
        {
            var targetQ = Q.GetTarget();
            var targetE = E.GetTarget();
            var inputQ = Q.GetPrediction(targetQ);


            if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
            {
                if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && targetQ.IsValidTarget() && inputQ.Hitchance >= HitChance.High && Q.ToggleState == SpellToggleState.Off)
                {
                    Q.Cast(inputQ.CastPosition);
                }
                if (mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && targetE.IsValidTarget())
                {
                    E.Cast(targetE);
                }

            }
        }
        #endregion
        #region OnGameUpdate

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
            if (AniviaQ.Position.CountEnemyHeroesInRange(225) >= 1 && GameObjects.Player.HasBuff("FlashFrost"))
            {
                Q.Cast();
            }
        }
        #endregion
        #region OnDraw
        private static void OnDraw(EventArgs args)
        {
            var PlayerPos = GameObjects.Player.Position;
            if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled && Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled && W.IsReady()) Render.Circle.DrawCircle(PlayerPos, W.Range, System.Drawing.Color.Silver, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled && E.IsReady()) Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled && R.IsReady()) Render.Circle.DrawCircle(PlayerPos, R.Range, System.Drawing.Color.Green, 1);
            }
        }
        #endregion
    }
}

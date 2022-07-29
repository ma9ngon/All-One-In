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
    internal class Morgana
    {
        private static Spell Q, W,E, R;
        private static Menu mainMenu;


        //public static void Main(string[] args)
        //{
        //    GameEvent.OnGameLoad += OnGameLoad;
        //}
        public static void OnGameLoad()
        {
            if (GameObjects.Player.CharacterName != "Morgana") return;
            Q = new Spell(SpellSlot.Q, 1300F);
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 800f);
            R = new Spell(SpellSlot.R, 575f);
            Q.SetSkillshot(0.25f, 140f, 1300f, true, SpellType.Line);
            W.SetSkillshot(0.25f, 275f, 900f, false, SpellType.Circle);
            mainMenu = new Menu("Morgana", "SimpleMorg", true);
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true));
            Combo.Add(new MenuBool("Wuse", "Use W", true));
            Combo.Add(new MenuBool("Ruse", "Use R", true));
            Combo.Add(new MenuSlider("enemies", "use r if enemies >=", 3, 1, 5));
            var LaneClear = new Menu("LaneClear ", "LaneClear Settings");
            LaneClear.Add(new MenuBool("Quse", "Use Q", true));
            LaneClear.Add(new MenuBool("Wuse", "Use W", true));
            var list = new Menu("blacklist", "blacklist", true);
            mainMenu.Add(Combo);
            mainMenu.Add(LaneClear);
            var Misc = new Menu("Misc", "Misc Settings");
            
            var enemy = from hero in ObjectManager.Get<AIHeroClient>()
                        where hero.IsEnemy == true
                        select hero;
            foreach (var e in enemy)
            {
                list.Add(new MenuBool(e.CharacterName, e.CharacterName));
            }
            
           

            Misc.Add(list);
            mainMenu.Add(Misc);



            //draw
            var Draw = new Menu("Draw", "Draw Settings");
            Draw.Add(new MenuBool("qRange", "Draw Q range", true));
            Draw.Add(new MenuBool("wRange", "Draw W range", true));
            Draw.Add(new MenuBool("lista", "Draw only if spell is ready", true));
            mainMenu.Add(Draw);
            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }
        private static void ComboLogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var inputQ = Q.GetPrediction(target);
            var inputW = W.GetPrediction(target);
            if (mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && Q.IsReady() && target.IsValidTarget() && inputQ.Hitchance >= HitChance.VeryHigh && Q.IsInRange(inputQ.CastPosition))
            { Q.Cast(inputQ.CastPosition); }
            if (mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled && W.IsReady() && target.IsValidTarget() && inputW.Hitchance >= HitChance.Immobile && W.IsInRange(inputW.CastPosition))
            { W.Cast(inputW.CastPosition); }
            W.IsInRange(inputW.CastPosition);
            if (mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady() && GameObjects.Player.CountEnemyHeroesInRange(R.Range) >= mainMenu["Combo"].GetValue<MenuSlider>("enemies").Value)
            { R.Cast(); }



        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead) return;

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    ComboLogic();
                    break;
                //case OrbwalkerMode.Harass:
                //break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;
            }
        }
            private static void LaneClear()
            {
            var allMinions = GameObjects.GetMinions(
            GameObjects.Player.Position,
            W.Range,
            MinionTypes.All,
            MinionTeam.Enemy,
            MinionOrderTypes.MaxHealth);
            if (allMinions.Count == 0) return;

            if (mainMenu["LaneClear"].GetValue<MenuBool>("Wuse").Enabled)

            {
                var bestLocation = E.GetCircularFarmLocation(allMinions, 260f);
                if (bestLocation.MinionsHit > 1
                    && W.IsReady()
                    && W.IsInRange(bestLocation.Position))
                {
                    W.Cast(bestLocation.Position);
                    return;
                }

            }



            }

            private static void OnDraw(EventArgs args)
            {
                var PlayerPos = GameObjects.Player.Position;
                if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
                {
                    if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled) if (Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Red);
                    if (mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled) if (W.IsReady()) Render.Circle.DrawCircle(PlayerPos, W.Range, System.Drawing.Color.Silver, 1);
                    if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled) if (E.IsReady()) Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Brown, 1);
                }
            }
        }
    }

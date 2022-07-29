using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
namespace SimpleAIO.Champions
{
    internal class Wukong
    {
        private static Spell Q, W, E, R;
        private static Menu mainMenu;
        public static void OnGameLoad()
        {
            //AIHeroClient player = GameObjects.Player;
            Q = new Spell(SpellSlot.Q,420f);
            W = new Spell(SpellSlot.W, 300f);
            E = new Spell(SpellSlot.E, 625f);
            R = new Spell(SpellSlot.R, 162f);

            //Targeting input
            E.SetTargetted(25f, 2000f);



            mainMenu = new Menu("Wukong", "Wukong", true);
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true));
            Combo.Add(new MenuBool("intR", "Interrupt with R", true));
            Combo.Add(new MenuBool("antiW", "AntiGapcloser with W", true));
            Combo.Add(new MenuBool("antiR", "AntiGapcloser with R", true));
            Combo.Add(new MenuBool("Wuse", "use W", true));
            Combo.Add(new MenuBool("Euse", "Use E ", true));
            Combo.Add(new MenuBool("Ruse", "Use R ", true));
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass", "Harass Settings");
            Harass.Add(new MenuBool("Quse", "Use Q", true));
            Harass.Add(new MenuBool("Euse", "Use E ", true));
            Harass.Add(new MenuSlider("mana%", "Mana porcent", 50, 0, 100));
            Harass.Add(new MenuSliderButton("autoR", "auto R if it will hit:", 3, 1, 5));
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
            Interrupter.OnInterrupterSpell += Interrupter_OnInterrupterSpell; ;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnGapcloser;
            Orbwalker.OnAfterAttack += Orbwalker_OnAfterAttack;
            //AIBaseClient.OnBuffAdd += AIBaseClient_OnBuffAdd;
            
        }

        //private static void AIBaseClient_OnBuffAdd(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        //{
        //    throw new NotImplementedException();
        //}

        private static void Orbwalker_OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            if (args.Target == null || !args.Target.IsValidTarget()) return;
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient && mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled) if (Q.Cast()) Orbwalker.ResetAutoAttackTimer();

        }

        private static void Interrupter_OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {

            if (mainMenu["Combo"].GetValue<MenuBool>("intR").Enabled  && GameObjects.Player.Distance(sender) < R.Range && R.IsReady())
            {
                R.Cast();
            }
        }

        private static void AntiGapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            var fullHP = GameObjects.Player.MaxHealth;
            var HP = GameObjects.Player.Health;
            var critHP = fullHP / 4;
            if (W.IsReady() && sender.IsValidTarget() && mainMenu["Combo"].GetValue<MenuBool>("antiW").Enabled)
                W.Cast(args.EndPosition);
            else if (!W.IsReady() && sender.IsValidTarget(R.Range) && R.IsReady() && (HP <= critHP) && mainMenu["Combo"].GetValue<MenuBool>("antiR").Enabled)
                R.Cast(args.EndPosition);
        }

        private static void ComboLogic()
        {
            var target = TargetSelector.GetTarget(1500, DamageType.Physical);
            var targetE = E.GetTarget();
            var targetW = W.GetTarget();
            var targetR = R.GetTarget();
            

            //bool useQ = mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled;
            bool useW = mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled;
            bool useE = mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled;
            bool useR = mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled;
            if (((targetE != null && useE && E.IsReady() && targetE.DistanceToPlayer() > 420) || (targetE != null && !Q.IsReady() && targetE.DistanceToPlayer() <= 420)) &&targetE.DistanceToPlayer() <= 625 && !GameObjects.Player.HasBuff("MonkeyKingSpinToWin")) 
            {
                E.Cast(target);
            }
            if (target != null && useW && W.IsReady() && (GameObjects.Player.HealthPercent <= 80 || target.HealthPercent <= 65))
            {
                W.Cast();
            }

            if (target != null && useR && R.IsReady() &&target.DistanceToPlayer() < 162 && !GameObjects.Player.HasBuff("MonkeyKingSpinToWin"))
            {
                R.Cast();
            }
           
            
            
        }
        private static void KillstealR() 
        {
            foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (R.IsReady() && hero.DistanceToPlayer() <= R.Range &&
                    GameObjects.Player.GetSpellDamage(hero, SpellSlot.R) >= hero.Health)
                    R.Cast();
            }
        }
        private static void KillstealQ() 
        {
            foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsValidTarget(Q.Range)))
            {
                if (Q.IsReady() && hero.DistanceToPlayer() <= Q.Range &&
                    GameObjects.Player.GetSpellDamage(hero, SpellSlot.Q) >= hero.Health)
                    Q.Cast();
            }
        }
        private static void KillstealE() 
        {
            foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsValidTarget(E.Range)))
            {
                if (E.IsReady() && hero.DistanceToPlayer() <= E.Range &&
                   GameObjects.Player.GetSpellDamage(hero, SpellSlot.E) >= hero.Health)
                    E.Cast(hero);
            }
        }
      
        //private static void LaneClear() 
        //{
        //    var minion = GameObjects.GetMinions(GameObjects.Player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
        //    var lanemana = Config.Item("LaneMana").GetValue<Slider>().Value;
        //    if (Player.ManaPercent >= lanemana && (minion.Count > 0))
        //    {
        //        var minions = minion[0];
        //        if (Config.Item("LaneClearQ").GetValue<bool>() && Q.IsReady() && minions.IsValidTarget(Q.Range) && Q.IsKillable(minions))
        //        {
        //            Q.Cast();
        //        }
        //    }
        //    if (Player.ManaPercent >= lanemana && (minion.Count > 0))
        //        if (minion.Count > 2)
        //        {
        //            var minions = minion[2];
        //            if (Config.Item("LaneClearE").GetValue<bool>() && E.IsReady() && minions.IsValidTarget(E.Range))
        //            {
        //                E.Cast(minions);
        //            }
        //        }
        //}
        //private static void HarassLogic() 
        //{
        //}
        private static void OnGameUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead) return;

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    ComboLogic();
                    break;
                case OrbwalkerMode.Harass:
                    //HarassLogic();
                    break;
                case OrbwalkerMode.LaneClear:
                    //LaneClear();
                    break;
            }
            if (GameObjects.Player.HasBuff("MonkeyKingSpinToWin"))
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
            try
            {
                if (mainMenu["Combo"].GetValue<MenuBool>("KillstealR").Enabled && R.IsReady())
                    KillstealR();
                if (mainMenu["Combo"].GetValue<MenuBool>("KillstealQ").Enabled && Q.IsReady())
                    KillstealQ();
                if (mainMenu["Combo"].GetValue<MenuBool>("KillstealE").Enabled && E.IsReady())
                    KillstealE();
                if (mainMenu["Combo"].GetValue<MenuSliderButton>("autoR").Enabled && GameObjects.Player.CountEnemyHeroesInRange((int)R.Range) >= mainMenu["Combo"].GetValue<MenuSliderButton>("autoR").Value && R.IsReady())
                    R.Cast();
            }
            catch (Exception)
            {


            }

            
        }
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
    }
}
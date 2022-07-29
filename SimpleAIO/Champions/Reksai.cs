using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleAIO.Champions
{
    internal class RekSai
    {
        private static Spell Qburrowed, QUnburrowed, Wburrowed, WUnburrowed, Eburrowed, EUnburrowed, R;
        private static Menu mainMenu;
        public static void OnGameLoad()
        {
            //Unburrowed
            QUnburrowed = new Spell(SpellSlot.Q, 600f);
            WUnburrowed = new Spell(SpellSlot.W, 285f);
            EUnburrowed = new Spell(SpellSlot.E, 225f);
            R= new Spell(SpellSlot.R,1500);

            //burrowed
            Qburrowed = new Spell(SpellSlot.Q, 1050);
            Wburrowed = new Spell(SpellSlot.W, 1470);
            Eburrowed = new Spell(SpellSlot.E, 650f);

            R.SetTargetted(.25f,1400);
            EUnburrowed.SetTargetted(.25f,float.MaxValue);
            Qburrowed.SetSkillshot(0.125f, 60, 4000, true, SpellType.Line);
            Eburrowed.SetSkillshot(0, 60, 1600, false, SpellType.Line);
           

            mainMenu = new Menu("RekSai", "RekSai", true);
            //combo
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true));
            Combo.Add(new MenuBool("Wuse", "Use W", true));
            Combo.Add(new MenuBool("Eburroweduse", "Use Eburrowed", true));
            Combo.Add(new MenuBool("EUnburroweduse", "Use EUnburrowed", true));
            Combo.Add(new MenuBool("Ruse", "Use R", true));
            //harass
            var Harass = new Menu("Harass", "Harass Settings");
            Harass.Add(new MenuBool("Quse", "Use Q", true));
            Harass.Add(new MenuBool("Wuse", "Use W", true));
            Harass.Add(new MenuBool("Eburroweduse", "Use Eburroweduse", true));
            Harass.Add(new MenuBool("EUnburroweduse", "Use EUnburroweduse", true));
            //jungle
            var Jungle = new Menu("Jungle", "Jungle Settings");
            Jungle.Add(new MenuBool("Quse", "Use Q", true));
            Jungle.Add(new MenuBool("Qburroweduse", "Use Q [Burrowed]", true));
            Jungle.Add(new MenuBool("Wuse", "Use W [Auto Switch]", true));
            Jungle.Add(new MenuBool("Euse", "Use E", true));
            Jungle.Add(new MenuBool("Eusemax", "Use E at max fury only", true));
            //misc
            var Misc = new Menu("Misc", "Misc Settings");
            var list = new Menu("blacklist", "blacklist", true);
            var enemy = from hero in ObjectManager.Get<AIHeroClient>()
                        where hero.IsEnemy == true
                        select hero;
            foreach (var e in enemy)
            {
                Misc.Add(new MenuBool(e.CharacterName, e.CharacterName));
            }
            
            //Draw
            var Draw = new Menu("Draw", "Draw Settings");
            Draw.Add(new MenuBool("EUnburrowedRange", "EUnburrowed Range", true));
            Draw.Add(new MenuBool("EburrowedRange", "Draw Eburrowed Range", true));
            Draw.Add(new MenuBool("Rdmg", "Draw killable with R", true));
            Draw.Add(new MenuBool("lista", "Draw only if spell is ready", true));

            


            mainMenu.Add(Combo);
            mainMenu.Add(Harass);
            mainMenu.Add(Jungle);
            
            mainMenu.Add(Misc);
            mainMenu.Add(Draw);
            mainMenu.Attach();




            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalker.OnAfterAttack += OnAfterAttack;
        }
        public static bool IsBurrowed()
        {
            return GameObjects.Player.HasBuff("RekSaiW");
            //if (Wburrowed.Instance.Name.ToLower().Contains("burrowed"))
            //    return true;
            //else return false;
        }
        
        public static void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            if (args.Target == null || !args.Target.IsValidTarget()) return;
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient &&
                mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled && !IsBurrowed())
            {
                if (QUnburrowed.Cast()) Orbwalker.ResetAutoAttackTimer();
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass && args.Target is AIHeroClient &&
                mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled && !IsBurrowed())
            {
                if (QUnburrowed.Cast()) Orbwalker.ResetAutoAttackTimer();
            }
            //var Etarget = EUnburrowed.GetTarget();

            //if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && args.Target is AIHeroClient &&
            //    mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled && !IsBurrowed()&&GameObjects.Player.ManaPercent==100)
            //{
            //    EUnburrowed.Cast(Etarget);
            //}

        }
        private static void ComboLogic() 
        {
            var useQ = mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled;
            var useE = mainMenu["Combo"].GetValue<MenuBool>("EUnburroweduse").Enabled;
            var autoSwitch = mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled;
            var useQburrowed = mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled;
            var useEburrowed = mainMenu["Combo"].GetValue<MenuBool>("Eburroweduse").Enabled;
            var Qburrowedtarget = Qburrowed.GetTarget();
            var Qburrowedpred = Qburrowed.GetPrediction(Qburrowedtarget);
            var Eburrowedtarget = Eburrowed.GetTarget();
            var Eburrowedpred = Eburrowed.GetPrediction(Eburrowedtarget);
            var targetR = R.GetTarget();
            
            //var autoR = mainMenu["Misc"]["list"].GetValue<MenuBool>(target).Enabled;


            if (IsBurrowed())
            {
                if (Qburrowed.IsReady() && useQburrowed && Qburrowedtarget.IsValidTarget() && Qburrowedpred.Hitchance>HitChance.High)
                {
                            Qburrowed.Cast(Qburrowedpred.CastPosition);
                }
                if (Eburrowed.IsReady() && useEburrowed && Eburrowedtarget.IsValidTarget()  )
                {
                        Eburrowed.Cast(Eburrowedtarget.Position - 50);
                }

                if (Wburrowed.IsReady() && !Qburrowed.IsReady() && !Eburrowed.IsReady() && autoSwitch && (Eburrowedtarget.Distance(GameObjects.Player.Position) < 260 || Qburrowedtarget.Distance(GameObjects.Player.Position) < 260)) // Auto Switch
                {
                    Wburrowed.Cast();
                }
            }
            else
            {
                if (EUnburrowed.IsReady() && useE)
                {
                    foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(x => x.IsEnemy && GameObjects.Player.ManaPercent==100 && x.IsValidTarget(EUnburrowed.Range)))
                    {
                        EUnburrowed.Cast(enemy);
                    }
                }
                if (WUnburrowed.IsReady() && !QUnburrowed.IsReady() && !EUnburrowed.IsReady() && autoSwitch) // Auto Switch
                {
                    WUnburrowed.Cast();
                }
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled && R.IsReady() && GameObjects.Player.CountEnemyHeroesInRange(R.Range) > 0)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && R.GetDamage(x) > x.Health && x.HasBuff("reksairprey") && mainMenu["Misc"].GetValue<MenuBool>(x.CharacterName).Enabled)) R.Cast(target);
            }

        }
        private static void Jungle()
        {
            var useQUnburrowed = mainMenu["Jungle"].GetValue<MenuBool>("Quse").Enabled;
            var autoSwitch = mainMenu["Jungle"].GetValue<MenuBool>("Wuse").Enabled;
            var useE = mainMenu["Jungle"].GetValue<MenuBool>("Euse").Enabled;
            var useEMax = mainMenu["Jungle"].GetValue<MenuBool>("Eusemax").Enabled;
            var useQburrowed = mainMenu["Jungle"].GetValue<MenuBool>("Qburroweduse").Enabled;

            var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(QUnburrowed.Range)).ToList();

            if (mobs == null || (mobs != null && mobs.Count == 0))
            {
                return;
            }
            var mob = mobs[0];

            if (IsBurrowed())
            {
                if (Qburrowed.IsReady() && useQburrowed)
                {
                    Qburrowed.Cast(mob.Position);
                }
                if (!QUnburrowed.IsReady() && Wburrowed.IsReady() && autoSwitch)
                {
                    Wburrowed.Cast();
                }
            }
            else
            {
                if (QUnburrowed.IsReady() && useQUnburrowed && ObjectManager.Player.Distance(mob.Position) < EUnburrowed.Range)
                {
                    QUnburrowed.Cast();
                }
               
                if (EUnburrowed.IsReady() && ((useE && !useEMax)||(useE && useEMax && GameObjects.Player.ManaPercent == 100)))
                {
                    EUnburrowed.Cast(mob);
                }

                if (!QUnburrowed.IsReady() && !EUnburrowed.IsReady() && WUnburrowed.IsReady() && autoSwitch)
                {
                    WUnburrowed.Cast();
                }
            }
        }
        private static void HarassLogic()
        {
            var useQ = mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled;
            var useE = mainMenu["Harass"].GetValue<MenuBool>("EUnburroweduse").Enabled;
            var autoSwitch = mainMenu["Harass"].GetValue<MenuBool>("Wuse").Enabled;
            var useQburrowed = mainMenu["Harass"].GetValue<MenuBool>("Quse").Enabled;
            var useEburrowed = mainMenu["Harass"].GetValue<MenuBool>("Eburroweduse").Enabled;
            var Qburrowedtarget = Qburrowed.GetTarget();
            var Qburrowedpred = Qburrowed.GetPrediction(Qburrowedtarget);
            var Eburrowedtarget = Eburrowed.GetTarget();
            var Eburrowedpred = Eburrowed.GetPrediction(Eburrowedtarget);


            if (IsBurrowed())
            {
                if (Qburrowed.IsReady() && useQburrowed && Qburrowedtarget.IsValidTarget() && Qburrowedpred.Hitchance > HitChance.High)
                {
                    Qburrowed.Cast(Qburrowedpred.CastPosition);
                }
                if (Eburrowed.IsReady() && useEburrowed && Eburrowedtarget.IsValidTarget())
                {
                    Eburrowed.Cast(Eburrowedtarget.Position - 50);
                }

                if (Wburrowed.IsReady() && !Qburrowed.IsReady() && !Eburrowed.IsReady() && autoSwitch && (Eburrowedtarget.Distance(GameObjects.Player.Position) < 260 || Qburrowedtarget.Distance(GameObjects.Player.Position) < 260)) // Auto Switch
                {
                    Wburrowed.Cast();
                }
            }
            else
            {
                if (EUnburrowed.IsReady() && useE)
                {
                    foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(x => x.IsEnemy && GameObjects.Player.ManaPercent == 100 && x.IsValidTarget(EUnburrowed.Range)))
                    {
                        EUnburrowed.Cast(enemy);
                    }
                }
                if (WUnburrowed.IsReady() && !QUnburrowed.IsReady() && !EUnburrowed.IsReady() && autoSwitch) // Auto Switch
                {
                    WUnburrowed.Cast();
                }
            }
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
                    Jungle();
                    break;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            var PlayerPos = GameObjects.Player.Position;
            if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("EburrowedRange").Enabled && IsBurrowed() && Eburrowed.IsReady()) Render.Circle.DrawCircle(PlayerPos, Eburrowed.Range, System.Drawing.Color.Yellow, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("EUnburrowedRange").Enabled && !IsBurrowed() && EUnburrowed.IsReady()) Render.Circle.DrawCircle(PlayerPos, Eburrowed.Range, System.Drawing.Color.DarkGreen, 1);
            }
            if (!mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("EburrowedRange").Enabled && IsBurrowed() && Eburrowed.IsReady()) Render.Circle.DrawCircle(PlayerPos, Eburrowed.Range, System.Drawing.Color.Yellow, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("EUnburrowedRange").Enabled && !IsBurrowed() && EUnburrowed.IsReady()) Render.Circle.DrawCircle(PlayerPos, Eburrowed.Range, System.Drawing.Color.DarkGreen, 1);
            }
            if (mainMenu["Draw"].GetValue<MenuBool>("Rdmg").Enabled)
            {
                foreach (var enemyVisible in
                        ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget()))
                {
                    Vector2 pos = new Vector2((Drawing.WorldToScreen(enemyVisible.Position).X+50), (Drawing.WorldToScreen(enemyVisible.Position).Y-40));

                    if (R.GetDamage(enemyVisible) > enemyVisible.Health)
                        Drawing.DrawText(pos, Color.Green,"Killable With R");
                    else
                        Drawing.DrawText(pos, Color.Red,"Unkillable with R");
                }
            }
        }
    }
}
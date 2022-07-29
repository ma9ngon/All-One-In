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
    internal class veigar
    {
        private static Spell Q,W,E,R;
        private static Menu mainMenu;
        public static void OnGameLoad(){
            if(GameObjects.Player.CharacterName != "Veigar") return;

            Q = new Spell(SpellSlot.Q, 920f);
            W = new Spell(SpellSlot.W,900f);
            E = new Spell(SpellSlot.E,1050f);
            R = new Spell(SpellSlot.R, 650f);

            Q.SetSkillshot(0.25f,70f,2000f,true,SpellType.Line);
            W.SetSkillshot(1.35f,225f, float.MaxValue, false,SpellType.Circle);
            E.SetSkillshot(0.8f,350f, float.MaxValue, false,SpellType.Circle);
            R.SetTargetted(0.25f,1400f);
           

            mainMenu = new Menu("Veigar ", "simple Veigar", true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuBool("AutoW","Auto w",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            Combo.Add(new MenuBool("Iuse","Use Ignite",true));
            Combo.Add(new MenuBool("Everuse", "Use Evrfrost",true));
            
            mainMenu.Add(Combo);
            var Harass = new Menu("Harass","Harass Settings");
            Harass.Add(new MenuBool("Quse","Use Q",true));
            Harass.Add(new MenuBool("Wuse","Use W",true));
            //Harass.Add(new MenuBool("Euse","Use E",true));
           // Harass.Add(new MenuBool("Ruse","Use R",true));
            Harass.Add(new MenuSlider("mana%","Mana porcent",50,0,100));
            mainMenu.Add(Harass);
            var Misc = new Menu("Misc","Misc Settings");
            //Misc.Add(new MenuSlider("QHeat","Dont use Q if Heat =",79,0,100));
            //Misc.Add(new MenuSlider("EHeat","Dont use E if Heat =",89,0,100));
            Misc.Add(new MenuBool("DamageAfterCombo", "Draw Combo Damage", true));
            Misc.Add(new MenuBool("antiGapCloser", "antiGapCloser", true));
            Misc.Add(new MenuBool("Interrupter", "Interrupte with R", true));
            //Misc.Add(new MenuBool("Wuse","keep Heating",true));
            mainMenu.Add(Misc);
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("rRange","Draw R range",false));

            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;


        }
        public static float GetComboDamage(AIHeroClient enemy)
        {
            float damage = 0;

            if (Q.IsReady())
            {
                damage += Q.GetDamage(enemy);
            }

            if (W.IsReady())
            {
                damage += W.GetDamage(enemy);
            }

            if (R.IsReady())
            {
                damage += R.GetDamage(enemy);
            }

            if (GameObjects.Player.GetSpellSlot("summonerdot") == SpellSlot.Unknown || GameObjects.Player.Spellbook.CanUseSpell(GameObjects.Player.GetSpellSlot("summonerdot")) != SpellState.Ready)
            {
                damage += (float)GameObjects.Player.GetSummonerSpellDamage(enemy,SummonerSpell.Ignite);
            }

            return damage;
        }
        public static void CastE(AIHeroClient target)
        {
            var pred = Prediction.GetPrediction(target, E.Delay);
            var castVec = pred.UnitPosition.ToVector2()
                          - Vector2.Normalize(pred.UnitPosition.ToVector2() - GameObjects.Player.Position.ToVector2())
                          *E.Width;

            if (pred.Hitchance >= HitChance.VeryHigh && E.IsReady())
            {
                E.Cast(castVec);
            }
        }
        public static void CastE(Vector3 pos)
        {
            var castVec = pos.ToVector2() - Vector2.Normalize(pos.ToVector2() - GameObjects.Player.Position.ToVector2()) * E.Width;

            if (E.IsReady())
            {
                E.Cast(castVec);
            }
        }
        public static void CastE(Vector2 pos)
        {
            var castVec = pos;

            if (E.IsReady())
            {
                E.Cast(castVec);
            }
        }

        private static void AutoW()
        {
            if (mainMenu["Combo"].GetValue<MenuBool>("AutoW").Enabled)
            {
                var target =
                    ObjectManager.Get<AIHeroClient>()
                        .FirstOrDefault(
                            h =>
                            h.IsValidTarget(W.Range) && h.Buffs.Where(
                    b =>
                    b.IsActive && Game.Time < b.EndTime
                    && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun
                        || b.Type == BuffType.Suppression || b.Type == BuffType.Snare))
                    .Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay);

                if (target.IsValidTarget() && target != null)
                {
                    W.Cast(target.Position);
                }
            }
        }

        private static bool CanCastSpicialItem()
        {
            if (GameObjects.Player.HasItem(ItemId.Everfrost))
            {
                return GameObjects.Player.CanUseItem((int)ItemId.Everfrost);

            }
            return false;
        }
        private static void ComboLogic()
        {
            var target = TargetSelector.GetTarget(E.Range + 200,DamageType.Magical);
            if (!target.IsValidTarget())
            {
                return;
            }


            if (R.IsReady() && target.IsValidTarget(R.Range) && mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled)
            {
                if (R.GetDamage(target) > target.Health)
                {
                    if (!target.IsInvulnerable)
                    {
                        R.CastOnUnit(target);
                    }
                }
            }

            if (E.IsReady() && mainMenu["Combo"].GetValue<MenuBool>("Euse").Enabled)
            {
                if (GameObjects.Player.Distance(target.Position) <= E.Range + 200)
                {
                    var predE = E.GetPrediction(target);
                    if (predE.Hitchance == HitChance.VeryHigh)
                    {
                        if (!target.IsInvulnerable)
                        {
                            CastE(target);
                        }
                    }
                }
            }

            if (Q.IsReady() && Q.IsInRange(target)
                && mainMenu["Combo"].GetValue<MenuBool>("Quse").Enabled)
            {
                var pred = Q.GetPrediction(target);
                if (pred.Hitchance >= HitChance.VeryHigh && pred.CollisionObjects.Count == 0)
                {
                    Q.Cast(pred.CastPosition);
                }
            }

            var predictionW = W.GetPrediction(target);

            if (W.IsReady() && GameObjects.Player.Distance(target.Position) <= W.Range - 80f
                && mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled)
            {
                if (predictionW.Hitchance >= HitChance.VeryHigh)
                {
                    W.Cast(predictionW.CastPosition);
                }
            }

            if (mainMenu["Combo"].GetValue<MenuBool>("Iuse").Enabled && GameObjects.Player.Distance(target) <= 600
                && GameObjects.Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite) >= target.Health)
            {
                GameObjects.Player.Spellbook.CastSpell(GameObjects.Player.GetSpellSlot("summonerdot"), target);
            }

            if (CanCastSpicialItem()&& mainMenu["Combo"].GetValue<MenuBool>("Everuse").Enabled)
            {
                GameObjects.Player.UseItem(6656, target);
            }
        }

        private static void HarassLogic()
        {
            
        }


        private static void OnGameUpdate(EventArgs args){
            if(GameObjects.Player.IsDead || GameObjects.Player.IsRecalling()) return;
            
            switch (Orbwalker.ActiveMode){
                case OrbwalkerMode.Combo:
                     ComboLogic();
                    break;
                    case OrbwalkerMode.Harass:
                      //  HarassLogic();
                    break;
                case OrbwalkerMode.LaneClear:
                   
                    break;
            }
            AutoW();
        }
         private static void OnDraw(EventArgs args)
        {
            var PlayerPos = GameObjects.Player.Position;
            if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled) if (Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("wRange").Enabled) if (W.IsReady()) Render.Circle.DrawCircle(PlayerPos, W.Range, System.Drawing.Color.Silver, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled) if (E.IsReady()) Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled) if (R.IsReady()) Render.Circle.DrawCircle(PlayerPos, R.Range, System.Drawing.Color.Blue, 1);
                
            }
            
         }

        

    }
}

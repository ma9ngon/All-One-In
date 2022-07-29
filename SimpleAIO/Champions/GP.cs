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

// credit to AramDetFull

namespace SimpleAIO.Champions
{
    internal class gankplank
    {
        private static Spell Q, W, E, R;
        private static Menu mainMenu;
        public  const int BarrelExplosionRange = 345;
        public const int BarrelConnectionRange = 640;
        public static bool justE = false, justQ = false;
        public static List<Barrel> savedBarrels = new List<Barrel>();
        public static Vector3 ePos;
        internal class Barrel
        {
            public AIMinionClient barrel;
            public float time;

            public Barrel(AIMinionClient objAiBase, int tickCount)
            {
                barrel = objAiBase;
                time = tickCount;
            }
        }
        #region OnGameLoad
        public static void OnGameLoad()
        {

            Q = new Spell(SpellSlot.Q, 590f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 5000);

            //Targeting input
            Q.SetTargetted(0.25f, 2200f);
            E.SetSkillshot(0.8f, 50, float.MaxValue, false, SpellType.Circle);
            W.SetSkillshot(.25f, 10f, float.MaxValue, false, SpellType.Circle);
            R.SetSkillshot(1f, 100, float.MaxValue, false, SpellType.Circle);



            mainMenu = new Menu("gp", "gp", true);
            var Combo = new Menu("Combo", "Combo Settings");
            Combo.Add(new MenuBool("Quse", "Use Q", true));
            Combo.Add(new MenuBool("Wuse", "use W", true));
            Combo.Add(new MenuSlider("wBelow", "-> If Hp <", 40, 0, 95));
            Combo.Add(new MenuBool("Euse", "Use E ", true));
            Combo.Add(new MenuSlider("minBarrel", "min e stack to use e",0,0,2));
           // Combo.Add(new MenuBool("Ruse", "Use R ", true));
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
            Draw.Add(new MenuBool("eRange", "Draw E range", true));
            Draw.Add(new MenuBool("lista", "Draw only if spell is ready", true));
            mainMenu.Add(Draw);
            mainMenu.Attach();
            AIHeroClient.OnProcessSpellCast += processSpells;
            //AIHeroClient.OnProcessSpellCast += Game_ProcessSpell;
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
        }
        #endregion
        private static void processSpells(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "GangplankQWrapper")
                {
                    if (!justQ)
                    {
                        justQ = true;
                        DelayAction.Add(200, () => justQ = false);
                    }
                }
                if (args.SData.Name == "GangplankE")
                {
                    ePos = args.End;
                    if (!justE)
                    {
                        justE = true;
                       DelayAction.Add(500, () => justE = false);
                    }
                }
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            for (int i = 0; i < savedBarrels.Count; i++)
            {
                if (savedBarrels[i].barrel.NetworkId == sender.NetworkId)
                {
                    savedBarrels.RemoveAt(i);
                    return;
                }
            }
        }

        private static float getEActivationDelay()
        {  //make it switch
            if (GameObjects.Player.Level >= 13)
            {
                return 0.5f;
            }
            if (GameObjects.Player.Level >= 7)
            {
                return 1f;
            }
            return 2f;
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
            {
                savedBarrels.Add(new Barrel(sender as AIMinionClient, System.Environment.TickCount));
            }
        }

        private static float GetQTime(AIBaseClient targetB)
        {
            return GameObjects.Player.Distance(targetB) / 2800f + Q.Delay;
        }
        private static bool KillableBarrel(AIBaseClient targetB)
        {
            if (targetB.Health < 2)
            {
                return true;
            }
            var barrel = savedBarrels.FirstOrDefault(b => b.barrel.NetworkId == targetB.NetworkId);
            if (barrel != null)
            {
                var time = targetB.Health * getEActivationDelay() * 1000;
                if (System.Environment.TickCount - barrel.time + GetQTime(targetB) * 1000 > time)
                {
                    return true;
                }
            }
            return false;
        }
        public static void useQ(AIBaseClient target)
        {
            if (!Q.IsReady() || target == null)
                return;
            Q.Cast(target);
        }
        public static void useW(AIBaseClient target)
        {
            if (!W.IsReady() || target == null)
                return;
            if (!Q.IsReady(3500) && GameObjects.Player.Mana > 150)
                W.Cast();
        }
        public static  void useE(AIBaseClient target)
        {
            if (!E.IsReady() || target == null)
                return;
            E.CastOnUnit(target);
        }
        public static void useR(AIBaseClient target)
        {
            if (target == null || !R.IsReady())
                return;
            R.CastIfWillHit(target, 3);
        }
        private static IEnumerable<AIMinionClient> GetBarrels()
        {
            return savedBarrels.Select(b => b.barrel).Where(b => b.IsValid);
        }

        public static List<Vector3> PointsAroundTheTargetOuterRing(Vector3 pos, float dist, float width = 15)
        {
            if (!pos.IsValid())
            {
                return new List<Vector3>();
            }
            List<Vector3> list = new List<Vector3>();
            var max = 2 * dist / 2 * Math.PI / width / 2;
            var angle = 360f / max * Math.PI / 180.0f;
            for (int i = 0; i < max; i++)
            {
                list.Add(
                    new Vector3(
                        pos.X + (float)(Math.Cos(angle * i) * dist), pos.Y + (float)(Math.Sin(angle * i) * dist),
                        pos.Z));
            }

            return list;
        }
        private static List<Vector3> GetBarrelPoints(Vector3 point)
        {
            return PointsAroundTheTargetOuterRing(point, BarrelConnectionRange, 20f);
        }
        
        
        private static void Combo()
        {

            AIHeroClient target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (target == null) {return;}
            if (GameObjects.Player.HealthPercent < mainMenu["Combo"].GetValue<MenuSlider>("wBelow").Value && mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled
               && GameObjects.Player.CountEnemyHeroesInRange(500) > 0)
            {
                W.Cast();
            }
            //if (R.IsReady()&& mainMenu["Combo"].GetValue<MenuBool>("Ruse").Enabled)
            //{
            //    var Rtarget =
            //        GameObjects.EnemyHeroes.FirstOrDefault(e => e.HealthPercent < 50 && e.CountAllyHeroesInRange(660) > 0);
            //    if (Rtarget != null)
            //    {
            //        R.CastIfWillHit(Rtarget, 2);
            //    }
            //}
            var barrels =
                GetBarrels()
                    .Where(
                        o =>
                            o.IsValid && !o.IsDead && o.Distance(GameObjects.Player) < 1600 && o.SkinName == "GangplankBarrel" &&
                            o.GetBuff("gangplankebarrellife").Caster.IsMe)
                    .ToList();

            if (Q.IsReady() &&
                E.IsReady())
            {
                var Qbarrels = GetBarrels().Where(o => o.Distance(GameObjects.Player) < Q.Range && KillableBarrel(o));
                foreach (var Qbarrel in Qbarrels)
                {
                    if (Qbarrel.Distance(target) < BarrelExplosionRange)
                    {
                        continue;
                    }
                    var point =
                        GetBarrelPoints(Qbarrel.Position)
                            .Where(
                                p =>
                                    p.IsValid() && !p.IsWall() && p.Distance(GameObjects.Player.Position) < E.Range &&
                                    p.Distance(Prediction.GetPrediction(target, GetQTime(Qbarrel)).UnitPosition) <
                                    BarrelExplosionRange && savedBarrels.Count(b => b.barrel.Position.Distance(p) < BarrelExplosionRange) < 1)
                             .OrderBy(p => p.Distance(target.Position))
                             .FirstOrDefault();
                    if (point != null && !justE)
                    {
                        E.Cast(point);
                        DelayAction.Add(1, () => Q.CastOnUnit(Qbarrel));
                        return;
                    }
                }
            }

            var minBarrel = mainMenu["Combo"].GetValue<MenuSlider>("minBarrel").Value;
            if (E.IsReady() && GameObjects.Player.Distance(target) < E.Range &&
                target.Health > Q.GetDamage(target) + GameObjects.Player.GetAutoAttackDamage(target) && Orbwalker.CanMove() &&
                minBarrel < E.Instance.Ammo)
            {
                CastE(target, barrels);
            }
            

            var meleeRangeBarrel =
                barrels.FirstOrDefault(
                    b =>
                        b.Health < 2 && b.Distance(GameObjects.Player) < GameObjects.Player.GetRealAutoAttackRange(b) &&
                        b.CountEnemyHeroesInRange(BarrelExplosionRange) > 0);
            if (meleeRangeBarrel != null)
            {
                Orbwalker.ForceTarget = meleeRangeBarrel;
            }
            if (Q.IsReady())
            {
                if (barrels.Any())
                {
                    var detoneateTargetBarrels = barrels.Where(b => b.Distance(GameObjects.Player) < Q.Range);

                    if (detoneateTargetBarrels.Any())
                    {
                        foreach (var detoneateTargetBarrel in detoneateTargetBarrels)
                        {
                            if (!KillableBarrel(detoneateTargetBarrel))
                            {
                                continue;
                            }
                            if (
                                detoneateTargetBarrel.Distance(
                                    Prediction.GetPrediction(target, GetQTime(detoneateTargetBarrel)).UnitPosition) <
                                BarrelExplosionRange &&
                                target.Distance(detoneateTargetBarrel.Position) < BarrelExplosionRange)
                            {
                                Q.CastOnUnit(detoneateTargetBarrel);
                                return;
                            }
                            var detoneateTargetBarrelSeconds =
                                barrels.Where(b => b.Distance(detoneateTargetBarrel) < BarrelConnectionRange);
                            if (detoneateTargetBarrelSeconds.Any())
                            {
                                foreach (var detoneateTargetBarrelSecond in detoneateTargetBarrelSeconds)
                                {
                                    if (
                                        detoneateTargetBarrelSecond.Distance(
                                            Prediction.GetPrediction(
                                                target, GetQTime(detoneateTargetBarrel) + 0.15f).UnitPosition) <
                                        BarrelExplosionRange &&
                                        target.Distance(detoneateTargetBarrelSecond.Position) < BarrelExplosionRange)
                                    {
                                        Q.CastOnUnit(detoneateTargetBarrel);
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    if (2 > 1)
                    {
                        var enemies =
                            GameObjects.EnemyHeroes.Where(e => e.IsValidTarget() && e.Distance(GameObjects.Player) < 600)
                                .Select(e => Prediction.GetPrediction(e, 0.25f));
                        var enemies2 =
                            GameObjects.EnemyHeroes.Where(e => e.IsValidTarget() && e.Distance(GameObjects.Player) < 600)
                                .Select(e => Prediction.GetPrediction(e, 0.35f));
                        if (detoneateTargetBarrels.Any())
                        {
                            foreach (var detoneateTargetBarrel in detoneateTargetBarrels)
                            {
                                if (!KillableBarrel(detoneateTargetBarrel))
                                {
                                    continue;
                                }
                                var enemyCount =
                                    enemies.Count(
                                        e =>
                                            e.UnitPosition.Distance(detoneateTargetBarrel.Position) <
                                            BarrelExplosionRange);
                                if (enemyCount >= 1 &&
                                    detoneateTargetBarrel.CountEnemyHeroesInRange(BarrelExplosionRange) >=
                                    1)
                                {
                                    Q.CastOnUnit(detoneateTargetBarrel);
                                    return;
                                }
                                var detoneateTargetBarrelSeconds =
                                    barrels.Where(b => b.Distance(detoneateTargetBarrel) < BarrelConnectionRange);
                                if (detoneateTargetBarrelSeconds.Any())
                                {
                                    foreach (var detoneateTargetBarrelSecond in detoneateTargetBarrelSeconds)
                                    {
                                        if (enemyCount +
                                            enemies2.Count(
                                                e =>
                                                    e.UnitPosition.Distance(detoneateTargetBarrelSecond.Position) <
                                                    BarrelExplosionRange) >=
                                            1 &&
                                            detoneateTargetBarrelSecond.CountEnemyHeroesInRange(BarrelExplosionRange) >=
                                            1)
                                        {
                                            Q.CastOnUnit(
                                                detoneateTargetBarrel);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Q.CanCast(target))
                {
                    CastQonHero(target, barrels);
                }
            }
        }

        private static void CastQonHero(AIHeroClient target, List<AIMinionClient> barrels)
        {
            if (
                barrels.FirstOrDefault(
                    b =>
                        b.Health == 2 &&
                        Prediction.GetPrediction(target, GetQTime(b)).UnitPosition.Distance(b.Position) <
                        BarrelExplosionRange) != null && target.Health > Q.GetDamage(target))
            {
                return;
            }
            Q.CastOnUnit(target);
        }
        private static void CastE(AIHeroClient target, List<AIMinionClient> barrels)
        {
            if (barrels.Count < 1)
            {
                CastEtarget(target);
                return;
            }
            var enemies =
                GameObjects.EnemyHeroes.Where(e => e.IsValidTarget() && e.Distance(GameObjects.Player) < E.Range)
                    .Select(e => Prediction.GetPrediction(e, 0.35f));
            List<Vector3> points = new List<Vector3>();
            foreach (var barrel in
                barrels.Where(b => b.Distance(GameObjects.Player) < Q.Range && KillableBarrel(b)))
            {
                if (barrel != null)
                {
                    var newP = GetBarrelPoints(barrel.Position).Where(p => !p.IsWall());
                    if (newP.Any())
                    {
                        points.AddRange(newP.Where(p => p.Distance(GameObjects.Player.Position) < E.Range));
                    }
                }
            }
            var bestPoint =
                points.Where(b => enemies.Count(e => e.UnitPosition.Distance(b) < BarrelExplosionRange) > 0)
                    .OrderByDescending(b => enemies.Count(e => e.UnitPosition.Distance(b) < BarrelExplosionRange))
                    .FirstOrDefault();
            if (bestPoint.IsValid() &&
                !savedBarrels.Any(b => b.barrel.Position.Distance(bestPoint) < BarrelConnectionRange) && !justE)
            {
                E.Cast(bestPoint);
            }
        }
       
        private static void CastEtarget(AIHeroClient target)
        {
            var ePred = Prediction.GetPrediction(target, 1);
            if (ePred.CastPosition.Distance(ePos) > 400 && !justE)
            {
                E.Cast(
                    target.Position.Extend(ePred.CastPosition, BarrelExplosionRange));
            }
        }
        private static void OnGameUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead) return;

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:

                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    //HarassLogic();
                    if (mainMenu["Harass"].GetValue<MenuSlider>("mana%").Value <= GameObjects.Player.ManaPercent)
                    {
                        Combo();
                    }     
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
                if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled && Q.IsReady()) Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled && E.IsReady()) Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow, 1);
            }
            if (!mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled ) Render.Circle.DrawCircle(PlayerPos, Q.Range, System.Drawing.Color.Cyan, 1);
                if (mainMenu["Draw"].GetValue<MenuBool>("eRange").Enabled ) Render.Circle.DrawCircle(PlayerPos, E.Range, System.Drawing.Color.Yellow, 1);
            }
        }
    }

}
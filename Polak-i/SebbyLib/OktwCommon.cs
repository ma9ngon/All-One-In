using System;
using System.Collections.Generic;
using System.Linq;

namespace SebbyLib
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.Utility;
    using SharpDX;

    public class OktwCommon
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        private static List<UnitIncomingDamage> IncomingDamageList = new List<UnitIncomingDamage>();

        static OktwCommon()
        {
            AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
            AIBaseClient.OnProcessSpellCast += AIBaseClient_OnProcessSpellCast;
        }

        private static void AIBaseClient_OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (args.SData == null || sender.Type != GameObjectType.AIHeroClient)
            {
                return;
            }

            var target = args.Target as AIBaseClient;

            if (target != null)
            {
                if (target.Type == GameObjectType.AIHeroClient && target.Team != sender.Team && (sender.IsMelee || !args.SData.Name.IsAutoAttack()))
                {
                    IncomingDamageList.Add(new UnitIncomingDamage
                    {
                        Damage = (sender as AIHeroClient).GetSpellDamage(target, args.Slot),
                        Skillshot = false,
                        TargetNetworkId = args.Target.NetworkId,
                        Time = Game.Time
                    });
                }
            }
            else
            {
                foreach (var hero in GameObjects.Heroes.Where(e => !e.IsDead && e.IsVisible && e.Team != sender.Team && e.Distance(sender) < 2000))
                {
                    if (hero.HasBuffOfType(BuffType.Slow) || hero.IsWindingUp || !CanMove(hero))
                    {
                        if (CanHitSkillShot(hero, args.Start, args.To, args.SData))
                        {
                            IncomingDamageList.Add(new UnitIncomingDamage
                            {
                                Damage = (sender as AIHeroClient).GetSpellDamage(target, args.Slot),
                                Skillshot = true,
                                TargetNetworkId = hero.NetworkId,
                                Time = Game.Time
                            });
                        }
                    }
                }
            }
        }

        private static void AIBaseClient_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (args.Target != null && args.SData != null && sender.Type == GameObjectType.AIHeroClient)
            {
                if (args.Target.Type == GameObjectType.AIHeroClient && !sender.IsMelee && args.Target.Team != sender.Team)
                {
                    IncomingDamageList.Add(new UnitIncomingDamage
                    {
                        Damage = (sender as AIHeroClient).GetSpellDamage(args.Target as AIBaseClient, args.Slot),
                        Skillshot = false,
                        TargetNetworkId = args.Target.NetworkId,
                        Time = Game.Time
                    });
                }
            }
        }

        public static bool CanMove(AIHeroClient target)
        {
            return !((!target.IsWindingUp && !target.CanMove)
                || target.MoveSpeed < 50
                || target.HaveImmovableBuff());
        }

        public static bool CanHarass()
        {
            return !Player.IsWindingUp && !Player.IsUnderEnemyTurret() && Orbwalker.CanMove(50, false);
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

        public static bool CheckGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!Player.InRange(args.EndPosition, 500, true))
            {
                return false;
            }

            if (args.Type == AntiGapcloser.GapcloserType.UnknowDash && (args.StartPosition == args.EndPosition || args.Speed == 0))
            {
                return false;
            }

            return true;
        }

        public static List<Vector3> CirclePoints(float circleLineSegmentN, float radius, Vector3 position)
        {
            var points = new List<Vector3>();

            for (var i = 1; i <= circleLineSegmentN; i++)
            {
                var angle = i * 2 * Math.PI / circleLineSegmentN;
                var point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z);

                points.Add(point);
            }

            return points;
        }

        public static void DrawLineRectangle(Vector3 start2, Vector3 end2, int radius ,float width, System.Drawing.Color color)
        {
            var start = start2.ToVector2();
            var end = end2.ToVector2();
            var dir = (end - start).Normalized();
            var pDir = dir.Perpendicular();

            var rightStartPos = start + pDir * radius;
            var leftStartPos = start - pDir * radius;
            var rightEndPos = end + pDir * radius;
            var leftEndPos = end - pDir * radius;

            var rStartPos = Drawing.WorldToScreen(new Vector3(rightStartPos.X, rightStartPos.Y, Player.Position.Z));
            var lStartPos = Drawing.WorldToScreen(new Vector3(leftStartPos.X, leftStartPos.Y, Player.Position.Z));
            var rEndPos = Drawing.WorldToScreen(new Vector3(rightEndPos.X, rightEndPos.Y, Player.Position.Z));
            var lEndPos = Drawing.WorldToScreen(new Vector3(leftEndPos.X, leftEndPos.Y, Player.Position.Z));

            Drawing.DrawLine(rStartPos, rEndPos, width, color);
            Drawing.DrawLine(lStartPos, lEndPos, width, color);
            Drawing.DrawLine(rStartPos, lStartPos, width, color);
            Drawing.DrawLine(lEndPos, rEndPos, width, color);
        }

        public static double GetIncomingDamage(AIHeroClient target, float time = 0.5f, bool skillshots = true)
        {
            var totalDamge = 0d;

            foreach (var damage in IncomingDamageList.Where(d => d.TargetNetworkId == target.NetworkId && Game.Time - time < d.Time))
            {
                if (skillshots)
                {
                    totalDamge += damage.Damage;
                }
                else if (!damage.Skillshot)
                {
                    totalDamge += damage.Damage;
                }
            }

            if (target.HasBuffOfType(BuffType.Poison))
            {
                totalDamge += target.Level * 5;
            }

            if (target.HasBuffOfType(BuffType.Damage))
            {
                totalDamge += target.Level * 6;
            }

            return totalDamge;
        }

        public static float GetKsDamage(AIHeroClient target, Spell qwer, bool includeIncomingDamage = true)
        {
            var totalDamage = qwer.GetDamage(target) - target.AllShield - target.HPRegenRate;

            if (totalDamage > target.Health)
            {
                if (target.CharacterName == "Blitzcrank" && !target.HasBuff("manabarrier") && !target.HasBuff("manabarriercooldown"))
                {
                    totalDamage -= 0.3f * target.MaxMana;
                }
            }

            if (includeIncomingDamage)
            {
                totalDamage += (float)GetIncomingDamage(target);
            }

            return totalDamage;
        }

        public static float GetPassiveTime(AIBaseClient target, string buffName)
        {
            return target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                .Where(buff => buff.Name.ToLower() == buffName.ToLower())
                .Select(buff => buff.EndTime)
                .FirstOrDefault() - Game.Time;
        }

        public static Vector3 GetTrapPos(float range)
        {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValid && enemy.Distance(Player.Position) < range && (enemy.HasBuff("bardrstasis") || enemy.HasBuff("zhonyasringshield"))))
            {
                return enemy.Position;
            }

            foreach (var obj in ObjectManager.Get<EffectEmitter>().Where(obj => obj.IsValid && obj.Position.Distance(Player.Position) < range))
            {
                var name = obj.Name.ToLower();

                if (name.Contains("Gatemarker_Red".ToLower()) || name.Contains("global_ss_teleport_target_red.troy".ToLower())
                    || name.Contains("R_Tar_Ground_Enemy".ToLower()) || name.Contains("R_indicator_red".ToLower()))
                {
                    return obj.Position;
                }
            }

            return Vector3.Zero;
        }

        public static bool IsMovingInSameDirection(AIBaseClient source, AIBaseClient target)
        {
            var sourceLW = source.GetWaypoints().Last();

            if (sourceLW == source.Position.ToVector2() || !source.IsMoving)
            {
                return false;
            }

            var targetLW = target.GetWaypoints().Last();

            if (targetLW == target.Position.ToVector2() || !target.IsMoving)
            {
                return false;
            }

            return (sourceLW - source.Position.ToVector2()).AngleBetween(targetLW - target.Position.ToVector2()) < 20;
        }

        public static bool IsSpellHeroCollision(AIHeroClient t, Spell qwer, int extraWidth = 50)
        {
            foreach (var hero in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(qwer.Range + qwer.Width, true, qwer.RangeCheckFrom) && e.NetworkId != t.NetworkId))
            {
                var pred = qwer.GetPrediction(hero);
                var powCalc = Math.Pow(qwer.Width + extraWidth + hero.BoundingRadius, 2);

                if (pred.UnitPosition.ToVector2().DistanceSquared(qwer.From.ToVector2(), qwer.GetPrediction(t).CastPosition.ToVector2(), true) <= powCalc)
                {
                    return true;
                }
                else if (pred.UnitPosition.ToVector2().DistanceSquared(qwer.From.ToVector2(), t.PreviousPosition.ToVector2(), true) <= powCalc)
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
                || target.Health - GetIncomingDamage(target) < 1);
        }

        class UnitIncomingDamage
        {
            public double Damage { get; set; }
            public bool Skillshot { get; set; }
            public int TargetNetworkId { get; set; }
            public float Time { get; set; }
        }
    }
}
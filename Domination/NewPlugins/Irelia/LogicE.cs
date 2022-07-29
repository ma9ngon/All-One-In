using EnsoulSharp;
using EnsoulSharp.SDK;
using FunnySlayerCommon;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominationAIO.NewPlugins
{
    public static class LogicE
    {
        public static void EPrediction(bool Checkbuff = true)
        {
            var target = TargetSelector.GetTarget(775, DamageType.Physical);

            if (!Irelia.E.IsReady() || target == null)
                return;

            {
                if (target != null && (!target.HasBuff("ireliamark") || !Checkbuff))
                {
                    //float ereal = 0.275f + Game.Ping / 1000;

                    if (Irelia.E.IsReady(0))
                    {
                        if (Irelia.E.Name != "IreliaE" && Irelia.E1Pos.IsValid())
                        {
                            if (MenuSettings.ESettings.Emode.Index == 0)
                            {
                                var pos = FSpred.Prediction.Prediction.PredictUnitPosition(target, MenuSettings.ESettings.EDelay.Value).ToVector3();

                                {
                                    if (pos.IsValid())
                                    {
                                        int range = 1000;
                                        if (ObjectManager.Player.CountEnemyHeroesInRange(775) > 2)
                                        {
                                            range = 1000;
                                        }
                                        else
                                        {
                                            range = 350;
                                        }

                                        for (int i = range; i > 50; i--)
                                        {
                                            var poscast = pos.Extend(Irelia.E1Pos, -i);
                                            if (poscast.IsValid() && poscast.Distance(ObjectManager.Player.Position) < 775)
                                            {
                                                Irelia.E.Cast(poscast);
                                                return;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                            if (MenuSettings.ESettings.Emode.Index == 2)
                            {
                                var tempE = new Spell(SpellSlot.Unknown, 775);
                                tempE.SetSkillshot(MenuSettings.ESettings.EDelay.Value / 1000, 1, 2000, false, EnsoulSharp.SDK.SpellType.Line);
                                {
                                    var pred = FSpred.Prediction.Prediction.GetPrediction(tempE, target);
                                    if (pred.Hitchance >= FSpred.Prediction.HitChance.High && pred.CastPosition.IsValid())
                                    {
                                        int range = 0;
                                        if (ObjectManager.Player.CountEnemyHeroesInRange(775) > 2)
                                        {
                                            range = 1000;
                                        }
                                        else
                                        {
                                            range = 350;
                                        }

                                        for (int i = range; i > 50; i--)
                                        {
                                            var pos = pred.CastPosition.Extend(Irelia.E1Pos, -i);
                                            if (pos.IsValid() && pos.Distance(ObjectManager.Player.Position) < 775)
                                            {
                                                Irelia.E.Cast(pos);
                                                return;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                            if (MenuSettings.ESettings.Emode.Index == 1)
                            {
                                var tempE = new Spell(SpellSlot.Unknown, 775);
                                tempE.SetSkillshot(0.25f, 1, 2000, false, EnsoulSharp.SDK.SpellType.Line);
                                {
                                    var pred = FSpred.Prediction.Prediction.GetPrediction(tempE, target);
                                    if (pred.Hitchance >= FSpred.Prediction.HitChance.High && pred.CastPosition.IsValid())
                                    {
                                        int range = 1000;
                                        if (ObjectManager.Player.CountEnemyHeroesInRange(775) > 2)
                                        {
                                            range = 1000;
                                        }
                                        else
                                        {
                                            range = MenuSettings.ESettings.E1vs1Range.Value;
                                        }

                                        var spelldelay = new Spell(SpellSlot.Unknown, 775);
                                        spelldelay.SetSkillshot(0.25f +
                                               0.6f,
                                               1f, 2000, false, EnsoulSharp.SDK.SpellType.Line
                                               );

                                        for (int i = range; i > 50; i--)
                                        {
                                            var pos = pred.CastPosition.Extend(Irelia.E1Pos, -i);

                                            spelldelay.Delay = 0.25f + ((pos.Distance(ObjectManager.Player.Position) - pred.CastPosition.Distance(ObjectManager.Player.Position)) / 2000);

                                            var spelldelaypred = FSpred.Prediction.Prediction.GetPrediction(spelldelay, target);
                                            if (spelldelaypred.Hitchance >= FSpred.Prediction.HitChance.High)
                                            {
                                                var castpos = spelldelaypred.CastPosition.Extend(Irelia.E1Pos, -i + 20);
                                                if (castpos.Distance(ObjectManager.Player.Position) <= 775)
                                                {
                                                    Irelia.E.Cast(castpos);
                                                    return;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            {
                                if (Irelia.E.GetPrediction(target).CastPosition.DistanceToPlayer() <= 675)
                                {
                                    if (ObjectManager.Player.CountEnemyHeroesInRange(775) >= 2)
                                    {
                                        foreach (var gettarget in GameObjects.EnemyHeroes.Where(i => !i.IsAlly && !i.IsDead && i.IsValidTarget(775)).OrderBy(i => i.DistanceToPlayer()))
                                        {
                                            if (gettarget == null)
                                                return;

                                            if (gettarget.NetworkId == target.NetworkId)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (target.DistanceToPlayer() > gettarget.DistanceToPlayer())
                                                {
                                                    var castpos = gettarget.Position.Extend(target.Position, -200);
                                                    if (castpos.DistanceToPlayer() <= 775)
                                                    {
                                                        Irelia.E.Cast(castpos);
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    var castpos = target.Position.Extend(gettarget.Position, -200);
                                                    if (castpos.DistanceToPlayer() <= 775)
                                                    {
                                                        Irelia.E.Cast(castpos);
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Geometry.Circle circle = new Geometry.Circle(ObjectManager.Player.Position, 500, 50);

                                        {
                                            foreach (var onecircle in circle.Points)
                                            {
                                                if (onecircle.Distance(target) > 500)
                                                {
                                                    if (Irelia.E.Cast(onecircle))
                                                    {
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else return;
                            }                           
                        }
                    }
                }
            }
        }
    }
}

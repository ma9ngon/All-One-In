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
    internal class lux{
        private static Spell Q,Q1,W,E,R;
        private static Menu mainMenu;
        class UnitIncomingDamage
        {
            public double Damage { get; set; }
            public bool Skillshot { get; set; }
            public int TargetNetworkId { get; set; }
            public float Time { get; set; }
        }


        private static Vector3 Epos = Vector3.Zero;
        private static List<UnitIncomingDamage> IncomingDamageList = new List<UnitIncomingDamage>();
        private float DragonDmg = 0;
        private static bool infinty = false;
        private double DragonTime = 0;
        public static void OnGameLoad(){
            if (GameObjects.Player.CharacterName != "Lux") return;

            Q = new Spell(SpellSlot.Q, 1175);
            Q1 = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1075);
            R = new Spell(SpellSlot.R, 3000);

            Q1.SetSkillshot(0.25f, 80f, 1200f, true, SpellType.Line);
            Q.SetSkillshot(0.25f, 80f, 1200f, false, SpellType.Line);
            W.SetSkillshot(0.25f, 110f, 1200f, false, SpellType.Line);
            E.SetSkillshot(0.3f, 250f, 1050f, false, SpellType.Circle);
            R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SpellType.Line);

            mainMenu = new Menu("Lux ", "simple Lux", true);
            var Combo = new Menu("Combo","Combo Settings");
            Combo.Add(new MenuBool("Quse","Use Q",true));
            Combo.Add(new MenuBool("Wuse","Use W",true));
            Combo.Add(new MenuBool("Euse","Use E",true));
            Combo.Add(new MenuBool("Euse2", "Use 2nd E", false));
            Combo.Add(new MenuBool("Ruse","Use R",true));
            Combo.Add(new MenuBool("Normalcombo", "Q-E Combo", true));
            Combo.Add(new MenuBool("Misayacombo", "E-Q Combo", false));
            
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
            Misc.Add(new MenuBool("usePackets", "Use Packets", true));
            Misc.Add(new MenuBool("AutoShield", "Auto W", true));
            Misc.Add(new MenuBool("DamageAfterCombo", "Draw Combo Damage", true));
            Misc.Add(new MenuBool("antiGapCloser", "antiGapCloser", true));
            Misc.Add(new MenuBool("Interrupter", "Interrupte with R", true));
            //Misc.Add(new MenuBool("Wuse","keep Heating",true));
            List<String> skinW = new List<String>();
            if (GameObjects.Player.SkinId != 1)
            {
                GameObjects.Player.SetSkin(1);
            }
            string baseskin = GameObjects.Player.SkinName;


            for (int i = 1; i < 50; i++)
            {
                if (infinty && GameObjects.Player.SkinName == baseskin)
                {
                    break;
                }
                Game.Print(i.ToString() + GameObjects.Player.SkinName);

                skinW.Add(GameObjects.Player.SkinName);
                GameObjects.Player.SetSkin(i);
                if (GameObjects.Player.SkinName == baseskin)
                {
                    infinty = true;
                }
            }
            Misc.Add(new MenuList("skins", "skins", skinW.ToArray(), 1));
            

           
            mainMenu.Add(Misc);
            var Draw = new Menu("Draw","Draw Settings");
            Draw.Add(new MenuBool("qRange","Draw Q range",true));
            Draw.Add(new MenuBool("wRange","Draw W range",true));
            Draw.Add(new MenuBool("eRange","Draw E range",true));
            Draw.Add(new MenuBool("rRange","Draw R range",false));
            Draw.Add(new MenuBool("rRangeMini","Draw R range on minimap",false));

            Draw.Add(new MenuBool("lista","Draw only if spell is ready",true));
            mainMenu.Add(Draw);

            mainMenu.Attach();
            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AIBaseClient.OnProcessSpellCast += AIBaseClient_OnProcessSpellCast;
            AntiGapcloser.OnGapcloser += OnGapcloser;
            Drawing.OnEndScene += Drawing_OnEndScene;
            //AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
        }

        //private static void AIBaseClient_OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        //{
        //    if (args.SData == null || sender.Type != GameObjectType.AIHeroClient)
        //    {
        //        return;
        //    }

        //    var target = args.Target as AIBaseClient;
        //    if (target.Type == GameObjectType.AIHeroClient && target.Team != sender.Team && (sender.IsMelee || !args.SData.Name.IsAutoAttack()))
        //    {
        //        IncomingDamageList.Add(new UnitIncomingDamage
        //        {
        //            Damage = (sender as AIHeroClient).GetSpellDamage(target, args.Slot),
        //            Skillshot = false,
        //            TargetNetworkId = args.Target.NetworkId,
        //            Time = Game.Time
        //        });
        //    }
        //    else
        //    {
        //        foreach (var hero in GameObjects.Heroes.Where(e => !e.IsDead && e.IsVisible && e.Team != sender.Team && e.Distance(sender) < 2000))
        //        {
        //            if (hero.HasBuffOfType(BuffType.Slow) || hero.IsWindingUp || !CanMove(hero))
        //            {
        //                if (CanHitSkillShot(hero, args.Start, args.To, args.SData))
        //                {
        //                    IncomingDamageList.Add(new UnitIncomingDamage
        //                    {
        //                        Damage = (sender as AIHeroClient).GetSpellDamage(target, args.Slot),
        //                        Skillshot = true,
        //                        TargetNetworkId = hero.NetworkId,
        //                        Time = Game.Time
        //                    });
        //                }
        //            }
        //        }
        //    }
        //}


        public static bool CanMove(AIHeroClient target)
        {
            return !((!target.IsWindingUp && !target.CanMove)
                || target.MoveSpeed < 50
                || target.HaveImmovableBuff());
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
        private static void AIBaseClient_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsMe || sender == null || !sender.IsEnemy || sender.IsMinion() || W.Level < 1 || !sender.IsValid || !mainMenu["Combo"].GetValue<MenuBool>("Wuse").Enabled || GameObjects.Player.Spellbook.IsAutoAttack) return;
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
            //if (args.Target != null && args.SData != null && sender.Type == GameObjectType.AIHeroClient)
            //{
            //    if (args.Target.Type == GameObjectType.AIHeroClient && !sender.IsMelee && args.Target.Team != sender.Team)
            //    {
            //        IncomingDamageList.Add(new UnitIncomingDamage
            //        {
            //            Damage = (sender as AIHeroClient).GetSpellDamage(args.Target as AIBaseClient, args.Slot),
            //            Skillshot = false,
            //            TargetNetworkId = args.Target.NetworkId,
            //            Time = Game.Time
            //        });
            //    }
            //}
            if (sender.IsMe && args.SData.Name == "LuxLightStrikeKugel")
            {
                Epos = args.End;
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if(mainMenu["Draw"].GetValue<MenuBool>("rRangeMini").Enabled)
            {
                if (mainMenu["Draw"].GetValue<MenuBool>("lista").Enabled)
                {
                    if (R.IsReady())
                      MiniMap. DrawCircle(GameObjects.Player.Position, R.Range, System.Drawing.Color.Aqua, 1);
                }
                else
                    MiniMap.DrawCircle(GameObjects.Player.Position, R.Range, System.Drawing.Color.Aqua, 1);
            }
        }

        private static void CastQ(AIBaseClient t)
        {
            var poutput = Q1.GetPrediction(t);
            var col = poutput.CollisionObjects.Count(ColObj => !ColObj.IsAlly && ColObj.IsMinion() && !ColObj.IsDead);

            if (col < 4 && poutput.Hitchance >= HitChance.VeryHigh)
                Q.Cast(poutput.CastPosition);
        }
        public static bool ValidUlt(AIHeroClient target)
        {
            return !(Invulnerable.Check(target)
                || target.HaveSpellShield()
                || target.HasBuffOfType(BuffType.SpellImmunity)
                || target.HasBuffOfType(BuffType.PhysicalImmunity)
                || target.Health - GetIncomingDamage(target) < 1);
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
        //private static bool HardCC(AIHeroClient target)
        //{

        //    if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
        //        target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
        //        target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
        //        target.IsStunned)
        //    {
        //        return true;

        //    }
        //    else
        //        return false;
        //}
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
        private static float BonusDmg(AIHeroClient target)
        {
            float damage = 10 + (GameObjects.Player.Level) * 8 + 0.2f * GameObjects.Player.FlatMagicDamageMod;
            if (GameObjects.Player.HasBuff("lichbane"))
            {
                damage += (GameObjects.Player.BaseAttackDamage * 0.75f) + ((GameObjects.Player.BaseAbilityDamage + GameObjects.Player.FlatMagicDamageMod) * 0.5f);
            }

            return (float)(GameObjects.Player.GetAutoAttackDamage(target) + GameObjects.Player.CalculateDamage(target,DamageType.Magical, damage));
        }
        private static void ComboLogic()
        {
            //foreach (var ally in GameObjects.AllyHeroes.Where(ally => ally.IsValid && !ally.IsDead  && mainMenu["Misc"].GetValue<MenuBool>("").Enabled && GameObjects.Player.Position.Distance(ally.Position) < W.Range))
            //{
            //    double dmg = GetIncomingDamage(ally);


            //    int nearEnemys = ally.CountEnemyHeroesInRange(800);

            //    if (dmg == 0 && nearEnemys == 0)
            //        continue;

            //    int sensitivity = 20;
                
            //    double HpPercentage = (dmg * 100) / ally.Health;
            //    double shieldValue = 65 + W.Level * 25 + 0.35 * GameObjects.Player.FlatMagicDamageMod;

            //    if (nearEnemys > 0 && HardCC(ally))
            //    {
            //        W.CastOnUnit(ally);
            //    }
            //    else if (ally.HasBuffOfType(BuffType.Poison))
            //    {
            //        W.Cast(W.GetPrediction(ally).CastPosition);
            //    }

            //    nearEnemys = (nearEnemys == 0) ? 1 : nearEnemys;

            //    if (dmg > shieldValue)
            //        W.Cast(W.GetPrediction(ally).CastPosition);
            //    else if (dmg > 100 + GameObjects.Player.Level * sensitivity)
            //        W.Cast(W.GetPrediction(ally).CastPosition);
            //    else if (ally.Health - dmg < nearEnemys * ally.Level * sensitivity)
            //        W.Cast(W.GetPrediction(ally).CastPosition);
            //    else if (HpPercentage >= mainMenu[""].GetValue<MenuSlider>("").Value)
            //        W.Cast(W.GetPrediction(ally).CastPosition);
            //}

            
           foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && E.GetDamage(enemy) + Q.GetDamage(enemy) + BonusDmg(enemy) > enemy.Health))
           {
                    CastQ(enemy);
                    return;
           }

                var t = Orbwalker.GetTarget() as AIHeroClient;
                if (!t.IsValidTarget())
                    t = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
                if (t.IsValidTarget() )
                {
                        CastQ(t);
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && !CanMove(enemy)))
                        CastQ(enemy);
                }
                var te = TargetSelector.GetTarget(E.Range,DamageType.Magical);
            var prete = E.GetPrediction(te);
            if (te.IsValidTarget() && prete.Hitchance >= HitChance.VeryHigh)
            {
                E.Cast(prete.CastPosition);
                
            }
            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(E.Range) && !CanMove(enemy)))
                E.Cast(enemy, true);
            foreach (var target in GameObjects.EnemyHeroes.Where(target => target.IsValidTarget(R.Range) && target.CountAllyHeroesInRange(600) < 2 && ValidUlt(target)))
            {
                float Rdmg = GetKsDamage(target, R);
                if (target.HasBuff("luxilluminatingfraulein"))
                Rdmg += (float)GameObjects.Player.CalculateDamage(target, DamageType.Magical, 10 + (8 * GameObjects.Player.Level) + 0.2 * GameObjects.Player.FlatMagicDamageMod);

              var preR= R.GetPrediction(target);

                if (Rdmg > target.Health && preR.Hitchance >=HitChance.VeryHigh )
                {

                    R.Cast(preR.CastPosition);
                    
                }
                else if (!CanMove(target) && target.IsValidTarget(E.Range))
                {
                    float dmgCombo = Rdmg;

                    if (E.IsReady())
                    {
                        var eDmg = E.GetDamage(target);

                        if (eDmg > target.Health)
                            return;
                        else
                            dmgCombo += eDmg;
                    }

                    if (target.IsValidTarget(800))
                        dmgCombo += BonusDmg(target);

                    if (dmgCombo > target.Health)
                    {
                        R.CastIfWillHit(target, 2);
                        R.Cast(target);
                    }

                }
                else if (1 > 0)
                {
                    R.CastIfWillHit(target, 3);
                }
            }

        }

        
        private static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (sender.IsAlly)
            {
                return;
            }

            if (mainMenu["Misc"].GetValue<MenuBool>("antiGapCloser").Enabled && sender.IsEnemy && Q.IsReady() && sender.IsValidTarget(Q.Range))
            {
               Q.Cast(sender);
            }
        }
       
        //private static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        //{
        //    var unit = args.Sender;
        //    if (args.DangerLevel >= Interrupter.DangerLevel.High && unit.IsEnemy)
        //    {
                
        //        if (mainMenu["Misc"].GetValue<MenuBool>("Interrupter").Enabled && unit.IsValidTarget(R.Range))
        //        {
        //            R.Cast();
        //        }
        //    }
        //}


        //private static float ComboDamage(AIHeroClient hero)
        //{
        //    var dmg = 0d;

        //    if (Q.IsReady())
        //        dmg += GameObjects.Player.GetSpellDamage(hero, SpellSlot.Q) ;
        //    if (W.IsReady())
        //        dmg += GameObjects.Player.GetSpellDamage(hero, SpellSlot.W);
        //    if (E.IsReady())
        //        dmg += GameObjects.Player.GetSpellDamage(hero, SpellSlot.E);
        //    if (R.IsReady())
        //        dmg += GameObjects.Player.GetSpellDamage(hero, SpellSlot.R);
        //    return (float)dmg;
        //}
        
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
            if (GameObjects.Player.HasBuff("LuxLightStrikeKugel"))
            {
                int eBig = Epos.CountEnemyHeroesInRange(350);
                if (1==1)
                {
                    int detonate = eBig - Epos.CountEnemyHeroesInRange(160);

                    if (detonate > 0 || eBig > 1)
                        E.Cast();
                }
                //else if (1==2)
                //{
                //    if (eBig > 0)
                //        E.Cast();
                //}
                //else
                //{
                //    E.Cast();
                //}
            }
            int skinut = mainMenu["Misc"].GetValue<MenuList>("skins").Index;

            if (GameObjects.Player.SkinId != skinut)
                GameObjects.Player.SetSkin(skinut);

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

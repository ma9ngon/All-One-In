using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominationAIO.NewPlugins
{
    public static class Irelia
    {
        public static Spell Q = new Spell(EnsoulSharp.SpellSlot.Q, 600f);
        public static Spell W = new Spell(EnsoulSharp.SpellSlot.W, 800f);
        public static Spell E = new Spell(EnsoulSharp.SpellSlot.E, 775f);
        public static Spell R = new Spell(EnsoulSharp.SpellSlot.R, 900f);

        private static AIHeroClient Player = null;
        public static Vector3 E1Pos = Vector3.Zero;
        private static Menu IreliaMainMenu = null;
        public static float SheenTimer = 0;
        public static float lastQ = 0f;
        public static void NewIre()
        {
            if(ObjectManager.Player == null)
            {
                return;
            }
            else
            {
                Player = ObjectManager.Player;
            }

            Game.Print("Disable block dash spell in EzEvade misc");

            Q.SetTargetted(0, float.MaxValue);
            W.SetSkillshot(0.25f, 70, 20000, false, SpellType.Line);
            W.SetCharged("IreliaW", "ireliawdefense", 800, 800, 3);
            E.SetSkillshot(0.25f, 5f, 2000f, false, SpellType.Line);
            R.SetSkillshot(0.4f, 150, 2000, false, SpellType.Line);

            IreliaMainMenu = new Menu("___Irelia", "FunnySlayer Irelia", true);
            IreliaMainMenu.AttackToMenu();
            IreliaMainMenu.Attach();

            Game.OnUpdate += EventsIrelia.KS;
            AIBaseClient.OnProcessSpellCast += EventsIrelia.AIBaseClient_OnProcessSpellCast;
            GameEvent.OnGameTick += EventsIrelia.Combo;
            AIBaseClient.OnBuffRemove += EventsIrelia.AIBaseClient_OnBuffLose;
            Game.OnUpdate += EventsIrelia.Game_OnUpdate;
            Drawing.OnDraw += EventsIrelia.Drawing_OnDraw;

            Game.OnUpdate += Clear;
        }
       
        private static void Clear(EventArgs args)
        {
            Q.Speed = 1400 + ObjectManager.Player.MoveSpeed;

            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo || !R.IsReady() || !MenuSettings.RSettings.Rcombo.Enabled)
            {
                LogicR.GetRPos1 = null;
                LogicR.GetRPos2 = null;
            }

            if (ObjectManager.Player.IsDead)
                return;
            var minions = new List<AIMinionClient>();
            minions.AddRange(GameObjects.Jungle.Where(i => i != null && !i.IsDead && !i.IsAlly && i.IsValidTarget(600)).OrderBy(i => i.Health));
            minions.AddRange(GameObjects.EnemyMinions.Where(i => i != null && !i.IsDead && !i.IsAlly && i.IsValidTarget(600)).OrderBy(i => i.Health));

            if (minions == null || !Q.IsReady() || !MenuSettings.ClearSettings.QireClear.Enabled || ObjectManager.Player.ManaPercent <= MenuSettings.ClearSettings.QireMana.Value || (!(Orbwalker.ActiveMode == OrbwalkerMode.LaneClear) && !(Orbwalker.ActiveMode == OrbwalkerMode.LastHit) && !MenuSettings.KeysSettings.AutoClearMinions.Active))
                return;

            var min = minions.OrderBy(i => !Helper.UnderTower(i.Position)).FirstOrDefault();
            if (Helper.CanQ(min) && min.DistanceToPlayer() < 600f)
            {
                if (!Helper.UnderTower(min.Position) || MenuSettings.KeysSettings.TurretKey.Active)
                {
                    if (Q.Cast(min) == CastStates.SuccessfullyCasted)
                        return;
                }
            }
        }
    }
}

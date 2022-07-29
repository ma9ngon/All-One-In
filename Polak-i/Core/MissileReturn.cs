using System;

namespace OneKeyToWin_AIO_Sebby.Core
{
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using SebbyLib;
    using SharpDX;

    class MissileReturn
    {
        public AIHeroClient Target;

        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        private string MissileName, MissileReturnName;
        private Spell MissileReturnSpell;
        private MissileClient Missile;
        private Vector3 MissileEndPos;

        private readonly MenuBool aim = new MenuBool("aim", "Auto aim returned missile");
        private readonly MenuBool drawHelper = new MenuBool("drawHelper", "Show helper");

        public MissileReturn(string missile, string missileReturnName, Spell qwer)
        {
            var local = Program.Config[Player.CharacterName] as Menu;

            (local[qwer.Slot.ToString().ToLower() + "Config"] as Menu)
                .Add(
                new Menu("AAOS", "Auto AIM OKTW system")
                {
                    aim
                });

            (local["draw"] as Menu)
                .Add(drawHelper);

            MissileName = missile;
            MissileReturnName = missileReturnName;
            MissileReturnSpell = qwer;

            AIBaseClient.OnDoCast += AIBaseClient_OnDoCast;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private void AIBaseClient_OnDoCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == MissileReturnSpell.Slot)
            {
                MissileEndPos = args.To;
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Missile != null && Missile.IsValid && drawHelper.Enabled)
            {
                OktwCommon.DrawLineRectangle(Missile.Position, Player.Position, (int)MissileReturnSpell.Width, 1, System.Drawing.Color.White);
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (aim.Enabled)
            {
                var posPred = CalculateReturnPos();

                if (posPred != Vector3.Zero)
                {
                    Orbwalker.SetOrbwalkerPosition(posPred);
                }
                else
                {
                    Orbwalker.SetOrbwalkerPosition(Game.CursorPos);
                }
            }
            else
            {
                Orbwalker.SetOrbwalkerPosition(Game.CursorPos);
            }
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid || sender.IsEnemy || sender.Type != GameObjectType.MissileClient)
            {
                return;
            }

            var missile = sender as MissileClient;

            if (missile.SData.Name != null)
            {
                if (missile.SData.Name.ToLower() == MissileName.ToLower() || missile.SData.Name.ToLower() == MissileReturnName.ToLower())
                {
                    Missile = missile;
                }
            }
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid || sender.IsEnemy || sender.Type != GameObjectType.MissileClient)
            {
                return;
            }

            var missile = sender as MissileClient;

            if (missile.SData.Name != null)
            {
                if (missile.SData.Name.ToLower() == MissileReturnName.ToLower())
                {
                    Missile = null;
                }
            }
        }

        private Vector3 CalculateReturnPos()
        {
            if (Missile != null && Missile.IsValid && Target.IsValidTarget())
            {
                var finishPosition = Missile.Position;

                if (Missile.SData.Name.ToLower() == MissileName.ToLower())
                {
                    finishPosition = MissileEndPos;
                }

                var misToPlayer = Player.Distance(finishPosition);
                var tarToPlayer = Player.Distance(Target);

                if (misToPlayer > tarToPlayer)
                {
                    var misToTarget = Target.Distance(finishPosition);

                    if (misToTarget < MissileReturnSpell.Range && misToTarget > 50)
                    {
                        var cursorToTarget = Target.Distance(Player.Position.Extend(Game.CursorPos, 100));
                        var ext = finishPosition.Extend(Target.PreviousPosition, cursorToTarget + misToTarget);

                        if (ext.Distance(Player.Position) < 800 && ext.CountEnemyHeroesInRange(400) < 2)
                        {
                            if (drawHelper.Enabled)
                            {
                                Render.Circle.DrawCircle(ext, 100, System.Drawing.Color.White, 1);
                            }

                            return ext;
                        }
                    }
                }
            }

            return Vector3.Zero;
        }
    }
}
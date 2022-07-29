using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleAIO.Champions{
    internal class buff{
        private static Menu mainMenuB;
        private static Spell Q;
        public static void OnGameLoad(){

            Q = new Spell(SpellSlot.Q, 450f);

            mainMenuB = new Menu("Buffs","Buffs",true);
            
            var EnemyBuffs = new Menu("enemy","enemy");
            EnemyBuffs.Add(new MenuBool("showenemy","enemy",false));
            mainMenuB.Add(EnemyBuffs);
            var myBuffs = new Menu("my","my");
            myBuffs.Add(new MenuBool("showmy","me",false));
            myBuffs.Add(new MenuBool("showob", "OnCreate", false));
            myBuffs.Add(new MenuBool("showob1", "OnDelete", false));
            myBuffs.Add(new MenuBool("gametime", "showgametime", false));
            //myBuffs.Add(new MenuBool("showob2", "objects2", false));
            mainMenuB.Add(myBuffs);
        

            mainMenuB.Attach();
            Drawing.OnDraw += OnDraw;
            GameObject.OnCreate += AiMinionClientOnOnCreate;
            GameObject.OnDelete += AiMinionClientOnOnDelete;

        }
        private static void AiMinionClientOnOnCreate(GameObject sender, EventArgs args)
        {
            if (GameObjectExtensions.DistanceToPlayer (sender) < 1000f && sender.Team == GameObjects.Player.Team && !sender.Name.ToLower().Contains("turret"))
            {
               buff.MinionName = "Minion Name: " + sender.Name;
               buff.MinionType = "Minion Type: " + sender.Type;
               buff.MinionPosition = "Minion Position: " + sender.Position;
            }
        }
        
        private static void AiMinionClientOnOnDelete(GameObject sender, EventArgs args)
        {
            if (GameObjectExtensions.DistanceToPlayer(sender) < 1000f && sender.Team == GameObjects.Player.Team && !sender.Name.ToLower().Contains("turret"))
            {
                buff.MinionName1 = "Minion Name: " + sender.Name;
                buff.MinionType1 = "Minion Type: " + sender.Type;
                buff.MinionPosition1 = "Minion Position: " + sender.Position;
            }
        }
        private static void OnDraw(EventArgs args){ 
            Vector2 posf = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X), (Drawing.WorldToScreen(GameObjects.Player.Position).Y));
            Vector2 poss = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X), (Drawing.WorldToScreen(GameObjects.Player.Position).Y + 120));
            Vector2 poss1 = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X), (Drawing.WorldToScreen(GameObjects.Player.Position).Y + 130));
            Vector2 poss2 = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X), (Drawing.WorldToScreen(GameObjects.Player.Position).Y + 140));

            if (mainMenuB["my"].GetValue<MenuBool>("showob").Enabled)
            {
                
            Drawing.DrawText(posf, Color.White, "MinionCreate");

            if (!string.IsNullOrWhiteSpace(buff.MinionName))
            {
                Drawing.DrawText(poss, Color.White, buff.MinionName);
            }
            if (!string.IsNullOrWhiteSpace(buff.MinionType))
            {
                Drawing.DrawText(poss1, Color.White, buff.MinionType);
            }
            if (!string.IsNullOrWhiteSpace(buff.MinionPosition))
            {
                Drawing.DrawText(poss2, Color.White, buff.MinionPosition);
            }
            }
            if (mainMenuB["my"].GetValue<MenuBool>("showob1").Enabled)
            {

                Drawing.DrawText(posf, Color.White, "MinionDelete");

                if (!string.IsNullOrWhiteSpace(buff.MinionName1))
                {
                    Drawing.DrawText(poss, Color.White, buff.MinionName1);
                }
                if (!string.IsNullOrWhiteSpace(buff.MinionType1))
                {
                    Drawing.DrawText(poss1, Color.White, buff.MinionType1);
                }
                if (!string.IsNullOrWhiteSpace(buff.MinionPosition1))
                {
                    Drawing.DrawText(poss2, Color.White, buff.MinionPosition1);
                }
            }
            //if (mainMenuB["my"].GetValue<MenuBool>("showob2").Enabled)
            //{

            //    Drawing.DrawText(posf, Color.White, "InstanceQ");

            //    if (!string.IsNullOrWhiteSpace(buff.v))
            //    {
            //        Drawing.DrawText(poss, Color.White, buff.v);
            //    }
                
            //}

            if (mainMenuB["my"].GetValue<MenuBool>("showmy").Enabled)
	        {
                Vector2 pos = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X),(Drawing.WorldToScreen(GameObjects.Player.Position).Y));
                var buffs = GameObjects.Player.Buffs;
                if(buffs.Any())Drawing.DrawText(pos,Color.White,"Buffs: ");
                for(var i = 0;i<buffs.Count()*10;i+=10)
                {
                Vector2 pos2 = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X),(Drawing.WorldToScreen(GameObjects.Player.Position).Y+120+i));
                Drawing.DrawText(pos2,Color.White, buffs[i/10].Count+"X "+buffs[i/10].Name);
                }
	        }
               
            if (mainMenuB["enemy"].GetValue<MenuBool>("showenemy").Enabled)
	        {
                var target0 = TargetSelector.GetTarget(1500,DamageType.Mixed);
                Vector2 pos = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X),(Drawing.WorldToScreen(GameObjects.Player.Position).Y));
                var buffs = target0.Buffs;
                if(buffs.Any())Drawing.DrawText(pos,Color.White,"Buffs: ");
                for(var i = 0;i<buffs.Count()*10;i+=10)
                {
                Vector2 pos2 = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X),(Drawing.WorldToScreen(GameObjects.Player.Position).Y+120+i));
                Drawing.DrawText(pos2,Color.White, buffs[i/10].Count+"X "+buffs[i/10].Name);
                }
	        }
            if (mainMenuB["my"].GetValue<MenuBool>("gametime").Enabled)
	        {
                //var target0 = TargetSelector.GetTarget(1500,DamageType.Mixed);
                Vector2 pos = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X),(Drawing.WorldToScreen(GameObjects.Player.Position).Y));
                //var buffs = target0.Buffs;
                //if(buffs.Any())Drawing.DrawText(pos,Color.White,"Buffs: ");
                //for(var i = 0;i<buffs.Count()*10;i+=10)
                //{
                var gametime = Convert.ToString(Game.Time);
                var name = Convert.ToString(GameObjects.Player.PercentBonusPhysicalDamageMod);
                var name2 = Convert.ToString(GameObjects.Player.PercentPhysicalDamageMod);
                var name3 = Convert.ToString(GameObjects.Player.FlatPhysicalDamageMod);
                var name5 = Convert.ToString(GameObjects.Player.GetBonusPhysicalDamage());
                var name4 = Convert.ToString(GameObjects.Player.MoveSpeed * 250 / 100);
                Vector2 pos2 = new Vector2((Drawing.WorldToScreen(GameObjects.Player.Position).X),(Drawing.WorldToScreen(GameObjects.Player.Position).Y+120));
                Drawing.DrawText(pos2, Color.White, gametime);
                Drawing.DrawText(pos2+10, Color.White, "PercentBonusPhysicalDamageMod= " + name);
                Drawing.DrawText(pos2+20, Color.White, "PercentPhysicalDamageMod= " + name2);
                Drawing.DrawText(pos2+30, Color.White, "FlatPhysicalDamageMod= " + name3);
                Drawing.DrawText(pos2+40, Color.White, "GameObjects.Player.MoveSpeed * 250 / 100= " + name4);
                Drawing.DrawText(pos2+50, Color.White, "GetBonusPhysicalDamage()= " + name5);
                
	        }
        }
        private static string MinionName;
        private static string MinionType;
        private static string MinionPosition;
        private static string MinionName1;
        private static string MinionType1;
        private static string MinionPosition1;
       // private static readonly string v = "Minion Name: " + Q.Instance.Name;
        //private static string qName = v;

    }

}

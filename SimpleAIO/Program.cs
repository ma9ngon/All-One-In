using System;
using System.Net;
using System.Diagnostics;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SimpleAIO.Champions;
using System.Threading.Tasks;
using EnsoulSharp.SDK.MenuUI;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using Color = System.Drawing.Color;


namespace SimpleAIO
{
    public class Program
    {
        public static void Main(string[] args)
        {           
            GameEvent.OnGameLoad += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {
            if (ObjectManager.Player == null)
                return;
            buff.OnGameLoad();
            try
            {
                switch (GameObjects.Player.CharacterName)
                {
                    case "Jayce":
                        Jayce.OnGameLoad();
                        break;
                    case "DrMundo":
                        DrMundo.OnGameLoad();
                        break;
                    case "Veigar":
                        veigar.OnGameLoad();
                        break;
                    case "MonkeyKing":
                        Wukong.OnGameLoad();
                        break;
                    case "Lux":
                        lux.OnGameLoad();
                        break;
                    case "MissFortune":
                        MF.OnGameLoad();
                        break;
                    case "Ivern":
                        ivern.OnGameLoad();
                        break;
                    case "Trundle":
                        Trundle.OnGameLoad();
                        break;
                    case "Warwick":
                        Warwick.OnGameLoad();
                        break;
                    case "Ryze":
                        ryze.OnGameLoad();
                        break;
                    case "RekSai":
                        RekSai.OnGameLoad();
                        break;
                    case "Taliyah":
                        Taliyah.OnGameLoad();
                        break;
                    case "Gangplank":
                        gankplank.OnGameLoad();
                        break;
                    case "Tryndamere":
                        Tryndamere.OnGameLoad();
                        break;
                    case "Anivia":
                        Anivia.OnGameLoad();
                        break;
                    case "Garen":
                        Garen.OnGameLoad();
                        break;
                    case "Gwen":
                        Gwen.OnGameLoad();
                        break;
                    case "Illaoi":
                        Illaoi.OnGameLoad();
                        break;
                    case "Mordekaiser":
                        Mordekaiser.OnGameLoad();
                        break;
                    case "Rumble":
                        Rumble.OnGameLoad();
                        break;
                    case "XinZhao":
                        XinZhao.OnGameLoad();
                        break;
                    case "Renekton":
                        Renekton.OnGameLoad();
                        break;
                    case "Chogath":
                        Chogath.OnGameLoad();
                        break;
                    case "Urgot":
                        Urgot.OnGameLoad();
                        break;
                    case "Kayle":
                        Kayle.OnGameLoad();
                        break;
                    default:
                        Game.Print("<font color='#b756c5' size='25'>SimpleAIO Does Not Support :" + ObjectManager.Player.CharacterName+ "</font>");
                        Console.WriteLine("Not Supported " + ObjectManager.Player.CharacterName);
                        break;                   
                }
            }
            catch (Exception ex)
            {
                Game.Print("Error in loading");
                Console.WriteLine("Error in loading :");
                Console.WriteLine(ex);
            }
        }
    }   
}

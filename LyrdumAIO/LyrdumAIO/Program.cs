using EnsoulSharp;
using EnsoulSharp.SDK;
using LyrdumAIO.Champions;
using System;
using System.Net;

namespace LyrdumAIO {

    public class Program {

        public static void Main(string[] args) {
            GameEvent.OnGameLoad += OnLoadingComplete;
        }

        private static void OnLoadingComplete() {
            if (ObjectManager.Player == null)
                return;
            try {
                switch (GameObjects.Player.CharacterName) {
                    case "Lux":
                        Lux.OnGameLoad();
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + ObjectManager.Player.CharacterName + " Loaded");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Discord: discord.gg/KfQFVhdqtz </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Don't forget to  personalize your settings </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        break;


                    case "Cassiopeia":
                        Cassiopeia.OnGameLoad();
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + ObjectManager.Player.CharacterName + " Loaded");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Discord: discord.gg/KfQFVhdqtz </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Don't forget to  personalize your settings! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        break;

                    case "Karthus":
                        Karthus.OnGameLoad();
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + ObjectManager.Player.CharacterName + " Loaded");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Discord: discord.gg/KfQFVhdqtz </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Don't forget to  personalize your settings! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        break;

                    case "Thresh":
                        Thresh.OnGameLoad();
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + ObjectManager.Player.CharacterName + " Loaded");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Discord: discord.gg/KfQFVhdqtz </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Don't forget to  personalize your settings </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        break;


                    case "Brand":
                        Brand.OnGameLoad();
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + ObjectManager.Player.CharacterName + " Loaded");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Discord: discord.gg/KfQFVhdqtz </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Don't forget to  personalize your settings! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  </font>" + "<font color='#F7FF00' size='25'>Bind your semi keys from menu!! </font>");
                        break;


                    default:
                        Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]:  Not compatible with " + ObjectManager.Player.CharacterName + "</font>");
                        Console.WriteLine("[LyrdumAIO] Does Not Support " + ObjectManager.Player.CharacterName);
                        break;
                }
                string stringg;
                string uri = "https://raw.githubusercontent.com/Lyrdum/LyrdumAIO/main/version.txt";
                using (WebClient client = new WebClient()) {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    stringg = client.DownloadString(uri);
                }
                string versionas = "1.3.15\n";
                if (versionas != stringg) {
                    Game.Print("<font color='#ff0000'> [LyrdumAIO]: </font> <font color='#ffe6ff' size='25'>You don't have the current version, please UPDATE !</font>");
                    Game.Print("<font color='#ff0000'> [LyrdumAIO]: </font> <font color='#ffe6ff' size='25'>You don't have the current version, please UPDATE !</font>");
                    Game.Print("<font color='#ff0000'> [LyrdumAIO]: </font> <font color='#ffe6ff' size='25'>You don't have the current version, please UPDATE !</font>");
                    Game.Print("<font color='#ff0000'> [LyrdumAIO]: </font> <font color='#ffe6ff' size='25'>You don't have the current version, please UPDATE !</font>");
                    Game.Print("<font color='#ff0000'> [LyrdumAIO]: </font> <font color='#ffe6ff' size='25'>You don't have the current version, please UPDATE !</font>");
                }
                else if (versionas == stringg) {
                    Game.Print("<font color='#ff0000' size='25'> [LyrdumAIO]: </font> <font color='#ffe6ff' size='25'>Is updated to the latest version!</font>");
                }
            }
            catch (Exception ex) {
                Game.Print("Error in loading");
            }
        }
    }
}

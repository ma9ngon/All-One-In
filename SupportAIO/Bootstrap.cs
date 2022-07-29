using EnsoulSharp.SDK;
using System;

namespace SupportAIO
{
    class Bootstrap
    {
        public static void Init()
        {
            try
            {                      
                GameEvent.OnGameLoad += delegate
                {
                    switch (GameObjects.Player.CharacterName)
                    {
                        case "Lulu":
                            new Champions.Lulu();
                            break;
                        case "Zyra":
                            new Champions.Zyra();
                            break;
                        case "Janna":
                            new Champions.Janna();
                            break;
                        case "Karma":
                            new Champions.Karma();
                            break;
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in loading. Exception:"+ ex);
            }
        }
    }
}

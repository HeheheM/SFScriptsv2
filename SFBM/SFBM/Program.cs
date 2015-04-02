/*
  ______________________________    _____   
 /   _____/\_   _____/\______   \  /     \  
 \_____  \  |    __)   |    |  _/ /  \ /  \ 
 /        \ |     \    |    |   \/    Y    \
/_______  / \___  /    |______  /\____|__  /
        \/      \/            \/         \/ 
 * Credits:
 * Snorflake
 * dasnecko - idea of chat spam
 * 
 * features
 * get chat restriced in like 3 games and maybe banned perma :)
 * */
using LeagueSharp;
using LeagueSharp.Common;
using SFBM.Annotations;
using System;

namespace SFBM
{
    class Program
    {

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        static readonly Menu Config = new Menu("SFBM", "SFBM", true);
        [UsedImplicitly] private static string[] _messages = {"Wow, you're so bad ", "Holy shit your fucking terrible ", "Get fucking better ", "im so better than u ", "smd ", "i fucking hate you ", "wow kill yourself ", "lmao get better ", "holy shit you are the definition of cancer "}; // 9
        static void Game_OnGameLoad(EventArgs args)
        {
            

            Config.AddItem(new MenuItem("Enabled", "Enabled?").SetValue(true));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;

            Game.PrintChat("SFBM by Snorflake loaded!");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Config.Item("Enabled").GetValue<bool>()) return;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsMe) continue;
                var r = new Random();
                var i = r.Next(1, 9);
                var sendmessage = _messages[i];
                Game.Say("/all " + sendmessage + hero.BaseSkinName);

            }
        }
    }
}

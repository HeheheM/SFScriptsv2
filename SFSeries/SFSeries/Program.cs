﻿using LeagueSharp;
using LeagueSharp.Common;
using System;

namespace SFSeries
{
    class Program
    {
        public static string ChampionName;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            ChampionName = ObjectManager.Player.BaseSkinName;

            switch (ChampionName)
            {
                case "Ahri":
                    new Ahri();
                    break;
                case "Katarina":
                    new Katarina();
                    break;
                case "Darius":
                    new Darius();
                    break;
                default:
                    Game.PrintChat("This champion is not supported by SFSeries");
                    break;
            }
        }
    }
}

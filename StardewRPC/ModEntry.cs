using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using DiscordRPC;

namespace StardewRPC
{
    class ModEntry : Mod
    {
        private string ClientID = "496310150995509268";
        private DiscordRpcClient client;
        private string season;

        public override void Entry(IModHelper helper)
        {
            IGameLoopEvents gameLoop = helper.Events.GameLoop;
            gameLoop.GameLaunched += this.OnGameLaunched;
            gameLoop.UpdateTicked += this.OnUpdateTicked;
            gameLoop.SaveLoaded += this.OnSaveLoaded;
            gameLoop.TimeChanged += this.OnTimeChanged;
            gameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.Player.Warped += this.OnWarp;
        }
        
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            client.Invoke();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            client.UpdateDetails("In the Farm House, " + season);
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            client.UpdateDetails("On the main screen");
            client.UpdateState("");
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            client = new DiscordRpcClient(ClientID);
            client.Initialize();
            client.SetPresence(new RichPresence()
            {
                Details = "On the main screen", 
                Assets = new Assets()
                {
                    LargeImageKey = "icon"
                }
            });
           

        }


      

        private void OnWarp(object sender, WarpedEventArgs e)
        {
            client.UpdateDetails("At the " + e.NewLocation + ", " + season);
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            int tme = e.NewTime;
            string time = tme.ToString().PadLeft(4, '0').Insert(2, ":");
            if(tme > 2350)
            {
                string a = time.Split(':')[0];
                string b = time.Split(':')[1].Replace(":", "");
                switch (a)
                {
                    case "24":
                        time = "00:" + b;
                        break;
                    case "25":
                        time = "01:" + b;
                        break;
                    case "26":
                        time = "02:" + b;
                        break;
                }
            }
            client.UpdateState(Game1.player.farmName.Value + " Farm (" + Game1.player.money + "g)");
            switch (Game1.weatherIcon)
            {
                case 2:
                    client.UpdateSmallAsset("sunny", "Sunny - " + time);
                    break;
                case 7:
                    client.UpdateSmallAsset("snow", "Snowing - " + time);
                    break;
                case 0:
                    client.UpdateSmallAsset("wedding", "Wedding - " + time);
                    break;
                case 1:
                    client.UpdateSmallAsset("festival", "Festival - " + time);
                    break;
                case 4:
                    client.UpdateSmallAsset("rainy", "Rainy - " + time);
                    break;
                case 5:
                    client.UpdateSmallAsset("storm", "Stormy - " + time);
                    break;
                case 3:
                    client.UpdateSmallAsset("windy_spring", "Windy - " + time);
                    break;
                case 6:
                    client.UpdateSmallAsset("windy_fall", "Windy - " + time);
                    break;

            }
        }
       

    }
}

using DiscordRPC;
using DiscordRPC.Message;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace StardewRPC
{
    class ModEntry : Mod
    {
        private string ClientID = "496310150995509268";
        private DiscordRpcClient client;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += this.OnTick;
            client = new DiscordRpcClient(ClientID);
            client.Initialize();
            client.SetPresence(new RichPresence()
            {
                Details = helper.Translation.Get("loading"),
                Assets = new Assets()
                {
                    LargeImageKey = "icon"
                },
                Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                }
            });
        }

        private void OnTick(object sender, UpdateTickingEventArgs e)
        {
            if (e.IsMultipleOf(120))
            {
                client.SetPresence(GenerateRichPresence());
            }
        }

        private RichPresence GenerateRichPresence()
        {
            if (Context.IsWorldReady)
            {
                RichPresence rp = new RichPresence()
                {
                    Details = GetCurrentLocation(),
                    State = Helper.Translation.Get("mode.solo"),
                    Assets = new Assets()
                    {
                        LargeImageKey = "icon",
                        LargeImageText = GetFarmInfo(),
                        SmallImageKey = GetWeather(),
                        SmallImageText = GetEnvironmentInfo(),

                    }
                };
                if (Context.IsMultiplayer)
                {
                    rp.Party = new Party()
                    {
                        Max = Game1.getFarm().getNumberBuildingsConstructed("Cabin") + 1,
                        Size = Game1.numberOfPlayers(),
                        ID = Game1.MasterPlayer.UniqueMultiplayerID.ToString()
                    };
                    rp.State = Helper.Translation.Get("mode.multiplayer");
                    rp.Secrets = new Secrets()
                    {
                        JoinSecret = Game1.server.getInviteCode()
                    };
                    client.OnJoin += JoinParty;
                }
                return rp;
            }
            return new RichPresence()
            {
                Assets = new Assets()
                {
                    LargeImageKey = "icon"
                },
                Details = Helper.Translation.Get("location.main-menu")
            };
        }

        private string GetEnvironmentInfo()
        {
            int timeOfDay = Game1.timeOfDay;
            string time = timeOfDay.ToString().PadLeft(4, '0').Insert(2, ":");
            if (timeOfDay > 2350)
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

            string weather;
            if (Game1.isRaining)
            {
                weather = Game1.isLightning ? Helper.Translation.Get("weather.storm") : Helper.Translation.Get("weather.rainy");
            }
            else if (Game1.isDebrisWeather)
            {
                weather = Helper.Translation.Get("weather.windy");
            }
            else if (Game1.isSnowing)
            {
                weather = Helper.Translation.Get("weather.snow");
            }
            else if (Game1.weddingToday)
            {
                weather = Helper.Translation.Get("weather.wedding");
            }
            else if (Game1.isFestival())
            {
                weather = Helper.Translation.Get("weather.festival");
            }
            else
            {
                weather = Helper.Translation.Get("weather.sunny");
            }

            string season = Game1.CurrentSeasonDisplayName;
            season = char.ToUpper(season[0]) + season.Substring(1);

            return Helper.Translation.Get("environment-info", new { time, season, weather });
        }
        private string GetFarmInfo()
        {
            return Helper.Translation.Get("farm-info", new { name = Game1.player.farmName.Value, type = Helper.Translation.Get("farm-type." + Game1.whichFarm.ToString()), money = Game1.player.money.ToString() });
        }
        private string GetWeather()
        {
            if (Game1.isRaining)
            {
                return Game1.isLightning ? "storm" : "rainy";
            }

            if (Game1.isDebrisWeather)
            {
                return "windy_" + Game1.currentSeason;
            }

            if (Game1.isSnowing)
            {
                return "snow";
            }

            if (Game1.weddingToday)
            {
                return "wedding";
            }

            if (Game1.isFestival())
            {
                return "festival";
            }

            return "sunny";
        }
        private string GetCurrentLocation()
        {
            var name = String.Join("-", Regex.Matches(Game1.currentLocation.Name, @"[A-Z]+(?=[A-Z][a-z]+)|\d|[A-Z][a-z]+")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToArray()).ToLower();
            return Helper.Translation.Get("location." + name);
        }

        private void JoinParty(object sender, JoinMessage args)
        {
            object lobby = Program.sdk.Networking.GetLobbyFromInviteCode(args.Secret);
            if (lobby == null)
            {
                return;
            }

            Game1.ExitToTitle(() => { TitleMenu.subMenu = new FarmhandMenu(Program.sdk.Networking.CreateClient(lobby)); });
        }

    }
}

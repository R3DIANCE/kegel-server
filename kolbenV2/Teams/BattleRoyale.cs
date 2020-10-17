using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace kolbenV2
{
    public class BattleRoyale : IScript
    {
        public string CurrentState = "LOBBY";
        public List<Player> Lobby = new List<Player>();
        public List<Player> Spectating = new List<Player>();
        public Random rnd = new Random();
        public Position LobbySpawn = new Position(-783.0755f, 5934.686f, 24.31475f);
        public Position CurrentMap { get; set; }
        public float MarkerScale = 600f;
        public int LobbyDimension { get; set; }
        public int CircleTime = 20000;
        public int TotalItems = 200;
        public int MapIndex { get; set; }
        public IColShape CurrentColshape { get; set; }
        public Dictionary<int, Item> Items = new Dictionary<int, Item>();
        public bool StopCicleMoving = true;
        public System.Timers.Timer LobbyTimer { get; set; }
        public System.Timers.Timer CircleTimer { get; set; }
        public System.Timers.Timer MoveCircleTimer { get; set; }

        public BattleRoyale()
        {

        }
        public BattleRoyale(Player client)
        {
            this.LobbyDimension = client.Id;
            this.CurrentMap = GetRandomMap();
            this.AddPlayer(client);            
            Data.CurrentLobby = this;
            this.StartLobbyTimer();
            Chat.GlobalMessage("Es wurde eine neue Runde Battle Royale gestartet.. /battle um bei zu treten!");
            foreach(Player player in Alt.GetAllPlayers())
            {
                if (player.LoggedIn)
                {
                    player.Emit("sendMessage", $"Es wurde eine neue Runde Battle Royale gestartet.. /battle um bei zu treten!");
                }
            }
            //for(int i = 0; i < this.TotalItems; i++)
            //{
            //    Position pos = GetRandomCoord();
            //    client.Emit("getGround", pos.X, pos.Y, i);
            //    int random = rnd.Next(Data.BattleRoyaleItems.Count);
            //    this.Items.Add(i, new Item(i, pos.X, pos.Y, 0, Data.BattleRoyaleItems.ElementAt(random).Key, Data.BattleRoyaleItems.ElementAt(random).Value));
            //}
            List<Position> list = Data.BattleRoyaleItemSpawns[this.MapIndex];
            while (this.Items.Count < this.TotalItems)
            {
                int random = rnd.Next(list.Count);
                if (!this.Items.ContainsKey(random))
                {                   
                    Position pos = list[random];
                    foreach (KeyValuePair<int, Item> entry in this.Items)
                    {
                        if (new Position(entry.Value.X, entry.Value.Y, entry.Value.X).Distance(pos) <= 10f)
                        {
                            return;
                        }
                    }
                    int randomItem = rnd.Next(Data.BattleRoyaleItems.Count);
                    this.Items.Add(random, new Item(random, pos.X, pos.Y, pos.Z + 0.5f, Data.BattleRoyaleItems.ElementAt(randomItem).Key, Data.BattleRoyaleItems.ElementAt(randomItem).Value));
                }
            }
        }

        public async void AddPlayer(Player player)
        {
            if (this.CurrentState == "INGAME" || this.Lobby.Contains(player))
            {
                return;
            }
            this.SendLobbyMessage($"{player.PlayerName} ist der Lobby beigetreten");
            this.Lobby.Add(player);
            player.Dimension = this.LobbyDimension;
            player.CurrentMode = "BR";
            player.RemoveFromTeams();
            player.Position = new Position(this.LobbySpawn.X, this.LobbySpawn.Y, this.LobbySpawn.Z + 2);
            player.RemoveAllWeapons();
            player.CurrentLobby = this;
            player.GiveWeapon(WeaponModel.Parachute, 0, false);
            IVehicle veh = await AltAsync.Do(() => Alt.CreateVehicle((uint)VehicleModel.Revolter, this.LobbySpawn, player.Rotation));
            veh.Dimension = player.Id + 1000;
            player.Dimension = player.Id + 1000;
            await Task.Delay(200);
            if (player.Exists)
            {
                if (veh.Exists)
                {
                    player.Emit("setPlayerIntoVehicle", veh);
                }
            }

        }

        public void SendLobbyMessage(string message)
        {
            this.Lobby.ForEach(player => player.SendNotificationGreen(message));
            this.Spectating.ForEach(player => player.SendNotificationGreen(message));
        }

        public void StartLobbyTimer()
        {
            LobbyTimer = new System.Timers.Timer();
            LobbyTimer.Interval = 5000;
            LobbyTimer.Elapsed += new ElapsedEventHandler(LobbyTimerElapsed);
            LobbyTimer.Start();
            LobbyTimer.AutoReset = false;
        }

        public void LobbyTimerElapsed(object sender, EventArgs e)
        {
            this.StartGame();
        }

        public void StartCircleTimer()
        {
            CircleTimer = new System.Timers.Timer();
            CircleTimer.Interval = this.CircleTime;
            CircleTimer.Elapsed += new ElapsedEventHandler(CircleTimerElapsed);
            CircleTimer.Start();
            CircleTimer.AutoReset = false;
        }

        public void CircleTimerElapsed(object sender, EventArgs e)
        {
            this.StartMoveCircleTimer();
            this.CheckOutSidePlayers();
        }

        public void StartMoveCircleTimer()
        {
            MoveCircleTimer = new System.Timers.Timer();
            MoveCircleTimer.Interval = 50;
            MoveCircleTimer.Elapsed += new ElapsedEventHandler(MoveCircleTimerElapsed);
            MoveCircleTimer.Start();
            MoveCircleTimer.AutoReset = true;
            this.StopCicleMoving = false;
            this.SendLobbyMessage("Die Zone verkleinert sich!!");
            this.TimeCirleMoving();
        }

        public void MoveCircleTimerElapsed(object sender, EventArgs e)
        {
            this.MoveCircle();           
        }

        public void DamagePlayer(Player player)
        {
            if(player.Armor >= 1)
            {
                player.Armor -= 1;
            }
            else if(player.Health >= 1)
            {
                player.Health -= 1;
            }
        }

        public async void CheckOutSidePlayers()
        {
            if(this == null)
            {
                return;
            }
            this.Lobby.ForEach(p =>
            {
                if(p.Position.Distance(new Position(this.CurrentMap.X, this.CurrentMap.Y, p.Position.Z)) >= this.MarkerScale / 2)
                {
                    this.DamagePlayer(p);
                }
            });
            await Task.Delay(100);
            if(this.Lobby.Count >= 1)
            {
                this.CheckOutSidePlayers();
            }
        }

        public void MoveCircle()
        {
            if (this.StopCicleMoving || this.MarkerScale <= 30)
            {
                MoveCircleTimer.AutoReset = false;
                MoveCircleTimer.Stop();
                this.CreateCircleTimer();
                this.StopCicleMoving = true;
                this.RemoveItemsOutOfCircle();
                return;
            }
            this.MarkerScale -= 0.25f;
            this.Lobby.ForEach(player =>
            {
                //Move the Marker
                player.CreateMarker("br_zone", 1, new Position(this.CurrentMap.X, this.CurrentMap.Y, this.CurrentMap.Z - 50), this.MarkerScale, 600, 255, 112, 112, 200);
            });
        }

        public async void TimeCirleMoving()
        {
            await Task.Delay(20000);
            this.StopCicleMoving = true;
        }

        public async void CreateCircleTimer()
        {
            if (this.MarkerScale <= 30)
            {
                return;
            }
            await Task.Delay(5000);
            this.SendLobbyMessage($"In {TimeSpan.FromMilliseconds(this.CircleTime).TotalSeconds} Sekunden verkleinert sich die Zone!!");
            await Task.Delay(this.CircleTime);
            this.StartCircleTimer();
        }

        public void StartGame()
        {
            this.CurrentState = "INGAME";
            this.CreateCircleTimer();
            this.Lobby.ForEach(player =>
            {
                //player.Position = new Position(this.Items[10].X, this.Items[10].Y, this.Items[10].Z);
                player.Position = new Position(GetRandomCoord().X, GetRandomCoord().Y, GetRandomCoord().Z + 300);
                player.CreateMarker("br_zone", 1, new Position(this.CurrentMap.X, this.CurrentMap.Y, this.CurrentMap.Z - 50), this.MarkerScale, 600, 255, 112, 112, 200);
                this.LoadItems(player);
                player.Dimension = this.LobbyDimension;
                //this.Test(player);
            });
            Alt.Log($"ITEMS: " + this.Items.Count);
        }

        public void LoadItems(Player player)
        {
            Database db = new Database();
            foreach (KeyValuePair<int, Item> entry in this.Items)
            {
                player.Emit("createObj", entry.Key, entry.Value.Hash, entry.Value.X, entry.Value.Y, entry.Value.Z);
                player.CreateMarker($"br_item{entry.Key}", 1, new Position(entry.Value.X, entry.Value.Y, entry.Value.Z), 1, (float)0.1, 0, 255, 0, 200);
                player.Emit("setBlipBattleRoyale", 1, 1, entry.Value.X, entry.Value.Y, entry.Value.Z, "", (entry.Key + 1000));               
                db.Insert($"INSERT INTO br_items_spawns (arena_id, x, y, z) VALUES ('{this.MapIndex}', '{entry.Value.X}', '{entry.Value.Y}', '{entry.Value.Z}')");
            }
        }

        public async void Test(Player player)
        {
            foreach (KeyValuePair<int, Item> entry in this.Items)
            {
                await Task.Delay(2000);
                player.Position = new Position(entry.Value.X, entry.Value.Y, entry.Value.Z);
            }
        }

        public Position GetRandomMap()
        {
            int random = rnd.Next(Data.BattleRoyaleMap.Count);
            //int random = 11;
            this.MapIndex = random;
            return Data.BattleRoyaleMap[random];
        }

        public Position GetRandomCoord()
        {
            var angle = rnd.NextDouble() * Math.PI * 2;
            var radius = Math.Sqrt(rnd.NextDouble()) * this.MarkerScale / 2;
            float x = (float)(this.CurrentMap.X + radius * Math.Cos(angle));
            float y = (float)(this.CurrentMap.Y + radius * Math.Sin(angle));
            return new Position(x, y, this.CurrentMap.Z);           
        }
        
        public void RemoveItemsOutOfCircle()
        {
            foreach (KeyValuePair<int, Item> entry in this.Items)
            {
                if(new Position(entry.Value.X, entry.Value.Y, entry.Value.Z).Distance(new Position(this.CurrentMap.X, this.CurrentMap.Y, entry.Value.Z)) >= this.MarkerScale / 2)
                {
                    this.Lobby.ForEach(l =>
                    {
                        l.Emit("deleteObj", entry.Key);
                        l.DestroyMarker($"br_item{entry.Key}");
                        l.Emit("destroyBlipBattleRoyale", (entry.Key + 1000));
                    });
                    this.Spectating.ForEach(s =>
                    {
                        s.Emit("deleteObj", entry.Key);
                        s.DestroyMarker($"br_item{entry.Key}");
                        s.Emit("destroyBlipBattleRoyale", (entry.Key + 1000));
                    });
                }
            }
        }

        public void EndGame()
        {
            this.Spectating.ForEach(p =>
            {
                this.ResetRound(p);
            });
            this.Lobby.ForEach(p =>
            {
                this.ResetRound(p);
            });
            Data.CurrentLobby = null;           
            this.LobbyTimer.Dispose();
            this.CircleTimer.Dispose();
            this.MoveCircleTimer.Dispose();
        }

        public void ResetRound(Player player)
        {
            this.Lobby.Remove(player);
            this.Spectating.Remove(player);
            player.SetTeamSelect();
            player.CurrentLobby = null;
            player.Emit("endSpectate");
            player.DestroyMarker("br_zone");
            foreach (KeyValuePair<int, Item> entry in this.Items)
            {
                player.Emit("deleteObj", entry.Key);
                player.DestroyMarker($"br_item{entry.Key}");
                player.Emit("destroyBlipBattleRoyale", (entry.Key + 1000));
            }
        }

        public void DeathCheck(Player player, Player killer)
        {          
            this.Lobby.Remove(player);
            this.Spectating.Add(player);
            this.SendLobbyMessage($"{killer.PlayerName} hat {player.PlayerName} getötet!");
            this.SendLobbyMessage($"Spieler übrig: {this.Lobby.Count}");
            player.Spawn(new Position(0, 0, 0), 1);
            player.Dimension = this.LobbyDimension;

            if (this.Lobby.Count <= 1)
            {
                this.EndGame();
                if (killer != player && killer != null)
                {
                    Chat.GlobalMessage($"{this.Lobby[0].PlayerName} hat das Battle Royale gewonnen!");
                }
                return;
            }
            if (killer != player && killer != null)
            {
                player.Emit("battleRoyaleSpec", killer, killer.PlayerName, this.Lobby.IndexOf(killer));
            }
            else
            {
                int random = rnd.Next(this.Lobby.Count);
                player.Emit("battleRoyaleSpec", this.Lobby[random], this.Lobby[random].PlayerName, random);
            }
        }
    }
}

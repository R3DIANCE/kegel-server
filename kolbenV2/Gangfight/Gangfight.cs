using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Timers;

namespace kolbenV2
{
    public class Gangfight : IScript
    {
        public Team Attacker { get; set; }
        public Team Defender { get; set; }
        public List<Team> Teams { get; set; }
        public System.Timers.Timer Timer { get; set; }
        public System.Timers.Timer FlagTimer { get; set; }
        public Position Center { get; set; }
        public int Time = 900000;
        public Stopwatch Stopwatch = new Stopwatch();
        public List<Position> Flags = new List<Position>();
        public Gangfight()
        {

        }
        public Gangfight(Team attacker, Team defender)
        {
            this.Attacker = attacker;
            this.Defender = defender;
            this.Teams = new List<Team>
            {
                this.Attacker,
                this.Defender
            };
            this.SendMessage("Dein Team befindet sich nun im Gangfight! Gehe zum Spawn um bei zu treten");
            this.LoadFlagPositions();
            this.LoadGangfight();
            Data.CurrentGangfight = this;
            this.Center = new Position((this.Attacker.Spawn.X + this.Defender.Spawn.X) / 2, (this.Attacker.Spawn.Y + this.Defender.Spawn.Y) / 2, ((this.Attacker.Spawn.Z + this.Defender.Spawn.Z) / 2) - 40);
        }

        public void SendMessage(string mess)
        {
            this.Teams.ForEach(team => { 
                team.SendTeamMessage(mess);
            });
        }

        public void StartFlagTimer()
        {
            this.FlagTimer = new System.Timers.Timer();
            this.FlagTimer.Interval = 30000;
            this.FlagTimer.Elapsed += new ElapsedEventHandler(CheckPlayer);
            this.FlagTimer.Start();
            this.FlagTimer.AutoReset = true;

        }

        public void AddPlayer(Player player)
        {
            player.InGangfight = true;
            player.Dimension = 1000;
            this.LoadBlips(player);
            this.LoadFlags(player);
            this.LoadHud(player);
            this.LoadZone(player);
            player.SendNotificationGreen("Beigetreten!");
            player.Emit("gangfight:textlabel", 0, 0, 0, "");
            this.Teams.ForEach(team =>
            {
                team.TeamMember.ForEach(player =>
               {
                   player.Emit("update:gangfight:count", this.Attacker.TeamMember.Count, this.Defender.TeamMember.Count);
               });
            });
        }

        public void CheckPlayer(object sender, EventArgs e)
        {
            this.Flags.ForEach(flag =>
            {
                this.Teams.ForEach(team =>
                {
                    team.TeamMember.ForEach(player =>
                    {
                        if (player.Position.Distance(flag) <= 5)
                        {
                            this.AddGangFightPoints(player.PlayerTeam, 5);
                        }
                    });
                });
            });
        }

        public void AddGangFightPoints(Team team, int points)
        {
            team.GangfightPoints += points;
            team.SendTeamMessage("Punkte erhalten - " + team.GangfightPoints);
            this.Teams.ForEach(team =>
            {
                team.TeamMember.ForEach(player =>
                {
                    player.Emit("update:gangfight:points", this.Attacker.GangfightPoints, this.Defender.GangfightPoints);
                });
            });
        }

        public void LoadGangfight()
        {
            this.Timer = new System.Timers.Timer();
            this.Timer.Interval = this.Time;
            this.Timer.Elapsed += new ElapsedEventHandler(DestroyGangfight);
            this.Timer.Start();
            this.StartFlagTimer();
            this.Stopwatch.Start();
            this.Timer.AutoReset = false;
            foreach (Player players in Alt.GetAllPlayers())
            {
                if (players.LoggedIn)
                {
                    players.Emit("admin:message", $"Es findet ein Gangwar statt.. {this.Attacker.Name} - {this.Defender.Name}");
                }
            }
            this.Teams.ForEach(team =>
            {
                team.TeamMember.ForEach(player =>
                {
                    player.Emit("gangfight:textlabel", team.Spawn.X, team.Spawn.Y, team.Spawn.Z, "Gangfight beitreten (E)");
                });
            });
        }

        public void DestroyGangfight(object sender, EventArgs e)
        {
            if(this.Attacker.GangfightPoints > this.Defender.GangfightPoints)
            {
                Chat.GloabAdminMessage($"{this.Attacker.Name} hat den Gangfight gewonnen");
            }
            else if (this.Attacker.GangfightPoints < this.Defender.GangfightPoints)
            {
                Chat.GloabAdminMessage($"{this.Defender.Name} hat den Gangfight gewonnen");
            }
            else if(this.Attacker.GangfightPoints == this.Defender.GangfightPoints)
            {
                Chat.GloabAdminMessage($"Der Gangfight gieng unentschieden aus");
            }
            this.FlagTimer.Stop();
            this.FlagTimer.AutoReset = false;
            foreach (Player player in Alt.GetAllPlayers())
            {
                if (player.CurrentMode == "team")
                {
                    if (player.PlayerTeam.InGangfight)
                    {
                        player.Emit("destroyGangfightBlip");
                        player.DestroyMarker("gangfight");
                        player.DestroyMarker("gangfight:zone");
                        player.Emit("hide:gangfight:HUD");
                        player.SetTeamSelect();
                    }
                }
            }
            this.Teams.ForEach(team => { 
                team.GangfightPoints = 0;
                team.InGangfight = false;
            });           
            Data.CurrentGangfight = null;
        }

        public void LoadFlagPositions()
        {
            if (!GangfightData.GangfightFlags.ContainsKey(this.Attacker.Id) || !GangfightData.GangfightFlags.ContainsKey(this.Defender.Id))
            {
                return;
            }
            foreach(KeyValuePair<int, List<Position>> entry in GangfightData.GangfightFlags)
            {
                if(entry.Key == this.Attacker.Id || entry.Key == this.Defender.Id)
                {
                    this.Flags = entry.Value;
                }
            }
        }

        public void LoadFlags(Player player)
        {
            this.Flags.ForEach(flag => { 
                player.CreateMarker("gangfight" + this.Flags.IndexOf(flag), 1, new Position(flag.X, flag.Y, flag.Z - 2), 10, 3, 255, 51, 51, 100); 
            });
        }

        public void LoadZone(Player player)
        {
            player.CreateMarker("gangfight:zone", 1, new Position(this.Center.X, this.Center.Y, this.Center.Z), this.Attacker.Spawn.Distance(this.Defender.Spawn) * 2, 400, 255, 51, 51, 100);
        }

        public void LoadBlips(Player player)
        {
            this.Flags.ForEach(flag => { player.Emit("setBlip", 379, player.PlayerTeam.BlibColor, flag.X, flag.Y, flag.Z, $"Gangfight", "gangfight"); });
        }

        public void LoadHud(Player player)
        {
            long remaindingTime = this.Time - this.Stopwatch.ElapsedMilliseconds;
            remaindingTime = remaindingTime % 1000 >= 500 ? remaindingTime + 1000 - remaindingTime % 1000 : remaindingTime - remaindingTime % 1000;
            player.Emit("open:gangfight:HUD", this.Attacker.Id, this.Defender.Id, this.Attacker.GangfightPoints, this.Defender.GangfightPoints, remaindingTime);
        }

        public Team OtherTeam(Team team)
        {
            if(team == this.Attacker)
            {
                return this.Defender;
            }
            else
            {
                return this.Attacker;
            }
        }
    }
}

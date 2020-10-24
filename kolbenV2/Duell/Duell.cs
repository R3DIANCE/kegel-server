using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace kolbenV2
{
    public class Duell : IScript
    {
        public DuellArena CurrentArena { get; set; }
        public Player Attacker { get; set; }
        public Player Defender { get; set; }
        public List<Player> Lobby { get; set; }
        public int Rounds { get; set; }
        public bool Pending = true;
        public Random rnd = new Random();
        public System.Timers.Timer AcceptTimer { get; set; }
        public IColShape CurrentColshape { get; set; }
        public Duell()
        {

        }

        public Duell(Player attk, Player def, int rounds)
        {
            this.Attacker = attk;
            this.Defender = def;
            this.Rounds = rounds;
            this.Lobby = new List<Player> { attk, def };
            this.CurrentArena = GetRandomArena();
            this.StartCounter(attk, def);
        }

        public DuellArena GetRandomArena()
        {
            return Data.DuellArenas[rnd.Next(Data.DuellArenas.Count)];
        }

        public void StartCounter(Player attacker, Player defender)
        {
            AcceptTimer = new System.Timers.Timer();
            AcceptTimer.Interval = 15000;
            AcceptTimer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            AcceptTimer.Start();
            AcceptTimer.AutoReset = false;
            attacker.SendNotificationGreen($"Du hast {defender.PlayerName} eine Duell Anfrage geschickt");
            defender.Emit("sendMessage", $"Dir wurde eine Duell Anfrage von {attacker.PlayerName} geschickt, /accept um anzunehmen");
            defender.SendNotificationGreen($"Dir wurde eine Duell Anfrage von {attacker.PlayerName} geschickt, /accept um anzunehmen");
            this.Lobby.ForEach(player => { player.CurrentDuell = this; });
        }

        public void TimerElapsed(object sender, EventArgs e)
        {
            if (this.Pending)
            {
                this.Attacker.SendNotificationRed("Dein Gegner hat das Duell nicht akzeptiert..");
                this.Defender.SendNotificationRed("Du hast das Duell nicht angenommen..");
                this.Lobby.ForEach(player => { player.CurrentDuell = null; });
            }
        }

        public async void SpawnPlayer(Player player, Position pos)
        {
            player.Freeze(true);
            await Task.Delay(2000);
            if (player.Exists)
            {
                player.Position = pos;
            }
            await Task.Delay(1000);
            if (player.Exists)
            {
                player.FullArmor();
                player.SetHealth();
                player.Freeze(false);
            }
        }
    
        public void StartGame()
        {
            this.Pending = false;
            this.Lobby.ForEach(player => { player.FullArmor(); player.SetHealth(); player.CurrentMode = "duell"; player.Dimension = this.Attacker.Id; player.RemoveFromTeams(); player.SetSyncedMetaData("DUELL", true); });
            this.SpawnPlayersRandom();
            this.CreateMarker();
            this.CreateColShape();
            this.SetBlips();
        }

        public void DeleteColShape()
        {
            if (this.CurrentColshape != null)
            {
                this.CurrentColshape.Remove();
            }
        }

        public void CreateColShape()
        {
            IColShape col = Alt.Server.CreateColShapeCircle(new Position(this.CurrentArena.Center.X, this.CurrentArena.Center.Y, this.CurrentArena.Center.Z), (float)(this.CurrentArena.MarkerScale / 2));
            this.CurrentColshape = col;
            col.Dimension = this.Attacker.Id;
        }

        public bool InCircle(Player player)
        {
            if(player.Position.Distance(this.CurrentArena.Center) > (float)(this.CurrentArena.MarkerScale / 2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CreateMarker()
        {
            this.Lobby.ForEach(player => {
                player.Emit("markers:Create", player.Id, 1, this.CurrentArena.Center.X, this.CurrentArena.Center.Y, this.CurrentArena.Center.Z, 0, 0, 0, 0, 0, 0, (float)this.CurrentArena.MarkerScale, (float)this.CurrentArena.MarkerScale, (float)this.CurrentArena.MarkerScale, 255, 112, 112, 200);
                //player.CreateMarker("duell", 1, this.CurrentArena.Center, (float)this.CurrentArena.MarkerScale, (float)this.CurrentArena.MarkerScale, 255, 112, 112, 200);
            });           
        }

        public void DeleteMarker()
        {
            this.Lobby.ForEach(player => { 
                player.DestroyMarker("duell");
                player.Emit("markers:Delete", player.Id);
            }) ;
        }

        public void SetBlips()
        {
            this.Attacker.Emit("createDuellBlip", this.Defender, 5000);
            this.Defender.Emit("createDuellBlip", this.Attacker, 5000);
        }

        public void SpawnPlayersRandom()
        {
            if (rnd.Next(0, 2) != 0)
            {
                this.SpawnPlayer(this.Attacker, this.CurrentArena.Spawn1);
                this.SpawnPlayer(this.Defender, this.CurrentArena.Spawn2);
            }
            else
            {
                this.SpawnPlayer(this.Attacker, this.CurrentArena.Spawn2);
                this.SpawnPlayer(this.Defender, this.CurrentArena.Spawn1);
            }
            this.Lobby.ForEach(p => { p.DeleteData("CHANGING_DUELL"); });
        }

        public void StartNewRound()
        {
            this.Lobby.ForEach(p => { p.SetData("CHANGING_DUELL", p); p.InCircle = true; });
            this.Rounds -= 1;
            this.DeleteColShape();
            this.DeleteMarker();
            this.Lobby.ForEach(p => { p.SendNotificationGreen($"Verbleibende Runden: {this.Rounds}.."); });
            this.Lobby.ForEach(p => { p.SendNotificationGreen($"{this.Attacker.PlayerName}: {this.Attacker.DuellWins} - {this.Defender.PlayerName}: {this.Defender.DuellWins}"); });
            this.CurrentArena = GetRandomArena();
            this.CreateMarker();
            this.SpawnPlayersRandom();
            this.CreateColShape();
        }

        public void EndDuell(Player player1, Player player2)
        {
            if (player1.DuellWins == player2.DuellWins)
            {
                this.Lobby.ForEach(player => { player.SendNotificationGreen($"Das Duell ist zuende.. {player.DuellWins}-{player2.DuellWins}"); });
            }
            if (player1.DuellWins > player2.DuellWins)
            {
                this.Lobby.ForEach(p => { p.SendNotificationGreen($"{player1.PlayerName} hat das Duell gewonnen!.. {player1.DuellWins}-{player2.DuellWins}"); });
                Chat.GlobalMessage($"{player1.PlayerName} hat gegen {player2.PlayerName} in einem Duell gewonnen!");
            }
            if (player2.DuellWins > player1.DuellWins)
            {
                this.Lobby.ForEach(p => { p.SendNotificationGreen($"{player2.PlayerName} hat das Duell gewonnen!.. {player2.DuellWins}-{player1.DuellWins}"); });
                Chat.GlobalMessage($"{player2.PlayerName} hat gegen {player1.PlayerName} in einem Duell gewonnen!");
            }
            this.Lobby.ForEach(p => { p.DuellWins = 0; p.SetTeamSelect(); p.CurrentDuell = null; });
            this.DeleteColShape();
            this.DeleteMarker();
        }

        public async void StartOutOfCircleTimer(Player player, Player otherPlayer)
        {
            player.InCircle = false;
            player.SendNotificationRed("Du bist außerhalb der Arena! In 5 Sekunden hast du die Runde verloren..");
            await Task.Delay(5000);
            if (!player.Exists || player.InCircle == true)
            {
                return;
            }
            if (this.Rounds > 1) // next round
            {
                this.StartNewRound();
            }
            else
            {
                this.EndDuell(player, otherPlayer);
            }
        }

        public void DeathCheck(Player player, Player killer)
        {
            killer.DuellWins += 1;
            if(this.Rounds <= 0)
            {
                this.EndDuell(player, killer);
            }
            else
            {
                this.StartNewRound();
            }
        }
    }
}

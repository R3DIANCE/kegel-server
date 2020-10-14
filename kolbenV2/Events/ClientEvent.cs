using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    class ClientEvent : IScript
    {
        [ClientEvent("startGangfight")]
        public void startGangfight(Player player, int teamId)
        {
            if (player.CurrentMode == "team")
            {
                if (Data.CurrentGangfight != null)
                {
                    player.SendNotificationRed("Es läuft bereits ein Gangwar..");
                    return;
                }
                if (!GangfightData.PossiblePairs[player.TeamId].Contains(teamId))
                {
                    player.SendNotificationRed("Dieses Team ist zu weit entfernt...");
                    return;
                }
                //if (Data.Teams[teamId].TeamMember.Count == 0)
                //{
                //    player.SendNotificationRed("In diesem Team befinden sich keine Spieler..");
                //    return;
                //}
                int difference = 0;
                if(player.PlayerTeam.TeamMember.Count > Data.Teams[teamId].TeamMember.Count)
                {
                    difference = player.PlayerTeam.TeamMember.Count - Data.Teams[teamId].TeamMember.Count;
                }
                else if(Data.Teams[teamId].TeamMember.Count > player.PlayerTeam.TeamMember.Count)
                {
                    difference = Data.Teams[teamId].TeamMember.Count - player.PlayerTeam.TeamMember.Count;
                }
                else if(Data.Teams[teamId].TeamMember.Count == player.PlayerTeam.TeamMember.Count)
                {
                    new Gangfight(player.PlayerTeam, Data.Teams[teamId]);
                    return;
                }
                if(difference > 4)
                {
                    player.SendNotificationRed("Die Teams sind zu unausgelichen..");
                    foreach (Player players in Alt.GetAllPlayers())
                    {
                        if (players.LoggedIn)
                        {
                            players.Emit("admin:message", $"{player.PlayerName} möchte ein Gangfight starten.. Die Teams müssen erst ausgeglichen werden.. {Data.Teams[teamId].Name}({Data.Teams[teamId].TeamMember.Count}) gegen {player.PlayerTeam.Name}({player.PlayerTeam.TeamMember.Count})");
                        }
                    }
                    return;
                }
                new Gangfight(player.PlayerTeam, Data.Teams[teamId]);
            }
        }

        [ClientEvent("parkOut")]
        public void accept(Player player, int id)
        {
            if (player.CurrentMode == "team")
            {
                player.PlayerTeam.SpawnTeamVehicle(player, Data.CarShopVehicles[id].Hash);
            }
        }
        [ClientEvent("Duell")]
        public void accept(Player player, string name, int rounds)
        {
            foreach(Player client in Alt.GetAllPlayers())
            {
                if (client.LoggedIn == false)
                {
                    return;
                }
                if (client.PlayerName == name)
                {
                    if (client == player)
                    {
                        return;
                    }
                    if (client.CurrentDuell != null)
                    {
                        player.SendNotificationRed($"{client.PlayerName} ist bereits in einem Duell!");
                        return;
                    }
                    if (player.CurrentDuell != null)
                    {
                        player.SendNotificationRed($"Du hast bereits eine Anfrage verschickt!");
                        return;
                    }
                    Duell duell = new Duell(player, client, rounds);
                    return;
                }
            }
        }

        [ClientEvent("groundZ")]
        public void groundZ(Player player, float height, int id)
        {
            if(height == 0)
            {
                Data.CurrentLobby.Items.Remove(id);
                Alt.Log("nullig");
            }
            else
            {
                Data.CurrentLobby.Items[id].Z = (height + 0.1f);
            }
        }

        [ClientEvent("PreviousSpec")]
        public void PreviousSpec(Player player, string playerName, int id)
        {
            int newIndex = id--;
            if (id < 0)
            {
                newIndex = player.CurrentLobby.Lobby.Count;
            }
            player.Emit("battleRoyaleSpec", player.CurrentLobby.Lobby[newIndex], player.CurrentLobby.Lobby[newIndex].PlayerName, newIndex);
        }

        [ClientEvent("NextSpec")]
        public void NextSpec(Player player, string playerName, int id)
        {
            int newIndex = id++;
            if(id > player.CurrentLobby.Lobby.Count)
            {
                newIndex = 0;
            }
            player.Emit("battleRoyaleSpec", player.CurrentLobby.Lobby[newIndex], player.CurrentLobby.Lobby[newIndex].PlayerName, newIndex);
        }

        [ClientEvent("endBrSpec")]
        public void endBrSpec(Player player)
        {
            player.SetTeamSelect();
            player.CurrentLobby.Spectating.Remove(player);
            player.Emit("markers:Delete", player.PlayerId);
            foreach (KeyValuePair<int, Item> entry in player.CurrentLobby.Items)
            {
                player.Emit("deleteObj", entry.Key);
                player.Emit("markers:Delete", (entry.Key + 1000));
                player.Emit("destroyBlipBattleRoyale", (entry.Key + 1000));
            }
            player.CurrentLobby = null;
        }       
    }
}

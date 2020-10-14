using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    class PlayerDisconnect : IScript
    {
        [ScriptEvent(ScriptEventType.PlayerDisconnect)]
        public void Disconnect(Player player, string reason)
        {
            if (player.LoggedIn == false)
            {
                return;
            }
            if (player.CurrentDuell != null)
            {
                if (player.CurrentDuell.Attacker != player)
                {
                    player.CurrentDuell.Attacker.SendNotification("Dein Gegner ist Disconnected, du hast gewonnen!");
                    player.CurrentDuell.Attacker.SetTeamSelect();
                    player.CurrentDuell.Attacker.CurrentDuell = null;
                    player.CurrentDuell.Attacker.DuellWins = 0;
                }
                else if (player.CurrentDuell.Defender != player)
                {
                    player.CurrentDuell.Defender.SendNotification("Dein Gegner ist Disconnected, du hast gewonnen!");
                    player.CurrentDuell.Defender.SetTeamSelect();
                    player.CurrentDuell.Defender.CurrentDuell = null;
                    player.CurrentDuell.Defender.DuellWins = 0;
                }
            }
            player.UpdatePlayer();
            player.RemoveFromTeams();
            foreach(Player client in Alt.GetAllPlayers())
            {
                if (client.LoggedIn == false)
                {
                    return;
                }
                client.SendNotification($"{player.PlayerName} hat den Server verlassen");
            }
            Alt.Log("Disconnect - Total: " + Alt.GetAllPlayers().Count);
            Alt.EmitAllClients("playerListRemove", player.PlayerName);
        }
    }
}

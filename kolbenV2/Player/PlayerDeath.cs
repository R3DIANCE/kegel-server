using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    class PlayerDeath : IScript
    {
        [ScriptEvent(ScriptEventType.PlayerDead)]
        public void PlayerDead(Player player, Player killer, uint reason)
        {
            player.KillStreak = 0;
            player.Deaths += 1;
            player.UpdateProfile();

            if (killer != player && killer != null && killer is Player)
            {
                VDMCheck(killer);
                killer.Emit("killFeed", player.PlayerName);
                player.Emit("deathscreen", killer.PlayerName, 4000);
                if (killer.CurrentMode == "BR" && player.CurrentMode == "BR")
                {
                    killer.CurrentLobby.DeathCheck(player, killer);
                }
                if(killer.CurrentDuell != null)
                {
                    killer.CurrentDuell.DeathCheck(player, killer);
                }
                if (killer.CurrentMode == "team")
                {
                    if (player.PlayerTeam.InGangfight && killer.PlayerTeam.InGangfight)
                    {
                        Data.CurrentGangfight.AddGangFightPoints(Data.CurrentGangfight.OtherTeam(player.PlayerTeam), 2);
                    }
                    killer.PlayerTeam.TeamMember.ForEach(member => { member.Emit("createDeathBlip", player.Position.X, player.Position.Y, player.Position.Z); });
                    player.PlayerTeam.TeamMember.ForEach(member => { member.Emit("createDeathBlip", player.Position.X, player.Position.Y, player.Position.Z); });
                }
                if(killer.CurrentMode == "dm")
                {
                    killer.FullArmor();
                    killer.SetHealth();
                }
                killer.Points += 4;
                killer.KillStreak += 1;
                killer.Kills += 1;
                killer.UpdateProfile();

                switch (killer.KillStreak)
                {
                    case 3:
                        Chat.GlobalMessage($"{killer.PlayerName} ist auf einer 3er Killstreak!");
                        break;
                    case 5:
                        Chat.GlobalMessage($"{killer.PlayerName} ist auf einer 5er Killstreak!");
                        break;
                    case 10:
                        Chat.GlobalMessage($"{killer.PlayerName} ist auf einer 10er Killstreak!");
                        break;

                }
            }
            else
            {
                if(player.CurrentMode == "team")
                {
                    if (player.PlayerTeam.InGangfight)
                    {
                        Data.CurrentGangfight.AddGangFightPoints(Data.CurrentGangfight.OtherTeam(player.PlayerTeam), 2);
                    }
                }
                if (player.CurrentDuell != null)
                {
                    player.CurrentDuell.DeathCheck(player, player);
                }
                if (player.CurrentMode == "BR")
                {
                    player.CurrentLobby.DeathCheck(player, player);
                }
                player.Emit("deathscreen", player.PlayerName, 4000);
            }
            player.RespawnPlayer();
        }



        public void VDMCheck(Player killer)
        {
            if (killer.IsInVehicle)
            {
                killer.VehicleKills += 1;
                if (killer.VehicleKills >= 3)
                {
                    killer.SendNotificationRed("3 Mal VDM Haaaaide");
                    killer.Kick("VDM");
                    Chat.GlobalMessage($"{killer.PlayerName} wurde aufgrund von VDM gekickt!");
                }
                else
                {
                    killer.SendNotificationRed($"Kein VDM erlaubt! {killer.VehicleKills}/3 Warns..");
                }
            }
        }
    }
}

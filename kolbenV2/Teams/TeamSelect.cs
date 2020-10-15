using AltV.Net;
using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace kolbenV2
{
    class TeamSelect : IScript
    {
        [ClientEvent("openTeamSelectAttempt")]
        public void openTeamSelectAttempt(Player player)
        {
            if (player.TeamSelectOpen)
            {
                return;
            }
            if(player.CurrentMode == "team")
            {
                if (player.PlayerTeam.InGangfight)
                {
                    if (player.InGangfight)
                    {
                        player.SendNotificationRed("Du bist im Gangfight...");
                    }                 
                    return;
                }
            }
            if(player.CurrentDuell == null)
            {
                player.SetTeamSelect();
            }
        }
  
        [ClientEvent("defaultDmCloth")]
        public void defaultDmCloth(Player player)
        {
            player.SetDmClothes();
            player.TeamId = 0;
            player.UpdateProfile();
        }

        [ClientEvent("defaultTeamCloth")]
        public void defaultTeamCloth(Player player, int teamId)
        {
            player.TeamId = teamId;
            player.UpdateProfile();
            player.PlayerTeam.SetTeamClothes(player);
            if(teamId == 2 || teamId == 4)
            {
                player.Emit("setBlend", 0, 0, 15, 0, 0, 0);
            }
            else
            {
                player.Emit("setBlend", 0, 0, 0, 0, 0, 0);
            }
        }

        [ClientEvent("joinTeam")]
        public void joinTeam(Player player, int teamId)
        {
            Team team = Data.Teams[teamId];
            //if (team.InGangfight)
            //{
            //    if(team.TeamMember.Count > Data.CurrentGangfight.OtherTeam(team).TeamMember.Count)
            //    {
            //        player.SendNotificationRed($"Trete {Data.CurrentGangfight.OtherTeam(team).Name} bei, damit die Teams ausgeglichen sind!");
            //        return;
            //    }                               
            //}
            team.AddPlayer(player);
            player.TeamSelectOpen = false;
            player.Emit("close:teamselect");
        }

        [ClientEvent("joinDeathmatch")]
        public void joinDeathmatch(Player player, int teamId)
        {
            Data.DmTeams[teamId].AddPlayer(player);
            player.TeamSelectOpen = false;
            player.Emit("close:teamselect");
        }
    }
}

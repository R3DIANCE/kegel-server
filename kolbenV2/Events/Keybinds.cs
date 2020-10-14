using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolbenV2
{
    class Keybinds : IScript
    {
        [ClientEvent("pressedE")]
        public async void pressedE(Player player)
        {
            if (player.CurrentMode == "team")
            {
                if (player.Position.Distance(player.PlayerTeam.Garage) <= 2.0f)
                {
                    if(player.PlayerVehicles.Count == 0)
                    {
                        player.PlayerTeam.SpawnTeamVehicle(player, (uint)VehicleModel.Exemplar);
                    }
                    else
                    {
                        player.Emit("openGarage");
                    }                  
                    //player.PlayerTeam.SpawnTeamVehicle(player);
                }
                if (player.Position.Distance(player.PlayerTeam.Spawn) <= 2.0f)
                {
                    if (!player.PlayerTeam.InGangfight)
                    {
                        //player.SendNotificationRed("soon");
                        player.Emit("open:gangfight:menu", player.TeamId);
                    }
                    else
                    {
                        Data.CurrentGangfight.AddPlayer(player);
                    }
                }
            }
            if(player.CurrentMode == "BR")
            {
                foreach (KeyValuePair<int, Item> entry in player.CurrentLobby.Items)
                {
                    if(player.Position.Distance(new Position(entry.Value.X, entry.Value.Y, entry.Value.Z)) <= 3f)
                    {
                        player.PlayAnimation("anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 4000);
                        player.Animating = true;
                        player.Freeze(true);
                        await Task.Delay(4000);
                        if (player.Exists)
                        {
                            player.Freeze(false);
                            player.Animating = false;
                            player.GiveWeapon(entry.Value.Model, 100, true);
                            player.CurrentLobby.Lobby.ForEach(l =>
                            {
                                l.Emit("deleteObj", entry.Key);
                                l.DestroyMarker($"br_item{entry.Key}");
                                l.Emit("destroyBlipBattleRoyale", (entry.Key + 1000));
                            });
                            player.CurrentLobby.Spectating.ForEach(s =>
                            {
                                s.Emit("deleteObj", entry.Key);
                                s.DestroyMarker($"br_item{entry.Key}");
                                s.Emit("destroyBlipBattleRoyale", (entry.Key + 1000));
                            });
                            player.CurrentLobby.Items.Remove(entry.Key);
                        }
                        return;
                    }
                }
            }
        }

        [ClientEvent("health")]
        public async void health(Player player)
        {
            if (!player.Exists)
            {
                player.Kick("bug.. reconnect");
                return;
            }
            if (player.IsInVehicle || player.Animating)
            {
                return;
            }
            player.Emit("EmitWeb", "open:progressbar");
            player.SendNotification("Du benutzt einen Verbandskasten..");
            player.PlayAnimation("anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 4000);
            player.Animating = true;
            player.Freeze(true);
            await Task.Delay(4000);
            if (player.Exists)
            {
                player.Freeze(false);
                player.Animating = false;
                player.SetHealth();
                player.SendNotificationGreen("Verbandskasten benutzt");
            }           
        }

        [ClientEvent("armor")]
        public async void armor(Player player)
        {
            if (!player.Exists)
            {
                player.Kick("bug.. reconnect");
                return;
            }

            if (player.IsInVehicle || player.Animating)
            {
                return;
            }
            player.Emit("EmitWeb", "open:progressbar");
            player.SendNotification("Du benutzt eine Weste..");
            player.PlayAnimation("anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 4000);
            player.Animating = true;
            player.Freeze(true);            
            await Task.Delay(4000);
            if (player.Exists)
            {
                player.Freeze(false);
                player.Animating = false;
                player.FullArmor();
                player.SendNotificationGreen("Weste benutzt");
            }
        }

        [ClientEvent("pressedF4")]
        public void pressedF4(Player player)
        {
            if (!player.TeamSelectOpen)
            {
                player.Emit("openCarshop");
                player.Position = new Position((float)-210.43, (float)-1323.07, (float)30.47);
                player.Dimension = player.Id;
                player.Emit("destroyGangfightBlip");
                player.DestroyMarker("gangfight");
                player.Emit("hide:gangfight:HUD");
                player.RemoveFromTeams();
            }
        }
    }
}

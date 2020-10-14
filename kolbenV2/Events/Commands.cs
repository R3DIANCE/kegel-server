using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.EntitySync;

namespace kolbenV2
{
    class Commands : IScript
    {
        [ClientEvent("test")]
        public void test(Player player)
        {
            if (player.IsAdmin)
            {
                if (Data.CurrentLobby != null)
                {
                    player.SendNotificationRed("Es findet bereits eine Runde statt");
                    return;
                }
                new BattleRoyale(player);
            }
            
        }

        [ClientEvent("battle")]
        public void battle(Player player)
        {
            if (player.IsAdmin)
            {
                if (Data.CurrentLobby != null)
                {
                    Data.CurrentLobby.AddPlayer(player);
                    return;
                }
            }
            
        }

        [ClientEvent("kill")]
        public void kill(Player player)
        {
            player.Health = 0;
        }

        [ClientEvent("adminmessage")]
        public void adminmessage(Player player, string[] args)
        {
            if (player.IsAdmin)
            {
                string message = $"(Admin)";
                for(int i = 0; i < args.Length; i++)
                {
                    message += $"{args[i]} ";
                }                
                foreach (Player players in Alt.GetAllPlayers())
                {
                    if (players.LoggedIn)
                    {
                        players.Emit("admin:message", message);
                    }
                }
            }
        }

        [ClientEvent("go")]
        public void go(Player player, string[] args)
        {
            if (player.IsAdmin)
            {
                foreach(Player players in Alt.GetAllPlayers())
                {
                    if (players.LoggedIn)
                    {
                        if(players.PlayerName == args[0])
                        {
                            player.Position = players.Position;
                        }
                    }
                }
            }
        }

        [ClientEvent("get")]
        public void get(Player player, string[] args)
        {
            if (player.IsAdmin)
            {
                foreach (Player players in Alt.GetAllPlayers())
                {
                    if (players.LoggedIn)
                    {
                        if (players.PlayerName == args[0])
                        {
                            players.Position = player.Position;
                        }
                    }
                }
            }
        }

        [ClientEvent("clothes")]
        public void cloths(Player player, string[] args)
        {
            if (player.IsAdmin)
            {
                player.Emit("customClothes");
            }
        }

        [ClientEvent("discord")]
        public void discord(Player player, string[] args)
        {
            player.Emit("sendMessage", $"Discord: https://discord.io/kolben");
        }
        

        [ClientEvent("prop")]
        public void prop(Player player, string[] args)
        {
            if (player.IsAdmin)
            {
                if (int.TryParse(args[0], out int comp) && int.TryParse(args[1], out int draw) && int.TryParse(args[2], out int tex))
                {
                    player.Emit("prop", comp, draw, tex);
                }
            }                     
        }

        [ClientEvent("ffa")]
        public void ffa(Player player, string[] args)
        {
            if(args.Length == 0)
            {
                Data.DmTeams[1].AddPlayer(player);
                player.SetDmClothes();
                return;
            }
            if (int.TryParse(args[0], out int id))
            {
                if (Data.DmTeams.ContainsKey(id))
                {                   
                    Data.DmTeams[id].AddPlayer(player);
                    player.SetDmClothes();
                }
                else
                {
                    player.SendNotificationRed($"Arena {id} existiert nicht.. 1 - {Data.DmTeams.Count}");
                }
            }
        }

        [ClientEvent("pos")]
        public void pos(Player player)
        {
            player.Emit("sendMessage", $"X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
            player.Emit("sendMessage", $"Rotation - Pitch: {player.Rotation.Pitch} - Roll: {player.Rotation.Roll} - Yaw {player.Rotation.Yaw}");
        }

        [ClientEvent("accept")]
        public void accept(Player player, string [] args)
        {
            int requestedArgs = 0;
            if (args.Length != requestedArgs)
            {
                return;
            }
            if(player.CurrentDuell != null)
            {
                player.CurrentDuell.Lobby.ForEach(player => {
                    if (player.TeamSelectOpen)
                    {
                        player.Emit("beginDuellFromTeamSelect");
                    }
                });
                player.CurrentDuell.StartGame();
            }        
        }

        [ClientEvent("veh")]
        public async void veh(Player player, string[] args)
        {
            if (player.IsAdmin)
            {
                int requestedArgs = 3;
                if (args.Length != requestedArgs)
                {
                    return;
                }
                VehicleModel model;

                if (Enum.TryParse(args[0], true, out model))
                {
                    IVehicle veh = await AltAsync.Do(() => Alt.CreateVehicle(model, player.Position, player.Rotation));
                    veh.Dimension = player.Dimension;
                    if (int.TryParse(args[1], out int prim))
                    {
                        veh.PrimaryColor = (byte)prim;
                    }
                    if (int.TryParse(args[1], out int sec))
                    {
                        veh.SecondaryColor = (byte)sec;
                    }
                }
                else
                {
                    player.SendNotificationRed($"Model: {args[0]} existiert nicht!");
                }
            }
        }

        [ClientEvent("kick")]
        public void test(Player client, string [] args)
        {
            if(args.Length == 0)
            {
                return;
            }
            if (client.IsAdmin)
            {
                foreach(Player player in Alt.GetAllPlayers())
                {
                    if (player.LoggedIn == false)
                    {
                        return;
                    }
                    if (player.PlayerName == args[0])
                    {
                        player.Kick("kicked");
                        return;
                    }
                }
                client.SendNotificationRed($"{args[0]} nicht gefunden!");
            }           
        }

        [ClientEvent("removeveh")]
        public void removeveh(Player player)
        {
            if (player.IsAdmin)
            {
                foreach(IVehicle veh in Alt.GetAllVehicles())
                {
                    if(veh.Position.Distance(player.Position) <= 3f)
                    {
                        veh.Remove();
                    }
                }
            }
        }

        [ClientEvent("spec")]
        public async void spec(Player player, string[] args)
        {        
            if(args.Length != 1)
            {
                return;
            }
            if(player.IsAdmin == false)
            {
                return;
            }
            foreach(Player target in Alt.GetAllPlayers())
            {
                if(target.LoggedIn)
                {
                    if (target.PlayerName == args[0])
                    {
                        player.Dimension = target.Dimension;
                        player.Position = target.Position;
                        player.Emit("nametags:Config", true);
                        await Task.Delay(200);
                        player.Emit("spectatePlayer", target);
                    }
                }
            }           
        }

        [ClientEvent("endspec")]
        public void endSpectate(Player player)
        {
            if (player.IsAdmin == false)
            {
                return;
            }
            player.Emit("endSpectate");
            player.Emit("nametags:Config", false);
            player.SetTeamSelect();
        }

        [ClientEvent("skin")]
        public void skin(Player player, string [] args)
        {
            if (player.IsAdmin == false)
            {
                return;                
            }
            player.Model = Alt.Hash(args[0]);
        }

        [ClientEvent("aduty")]
        public void skin(Player player)
        {
            if (player.IsAdmin == false)
            {
                return;
            }
            if (player.InAduty)
            {
                player.Model = (uint)PedModel.FreemodeMale01;
                if(player.CurrentMode == "team")
                {
                    player.PlayerTeam.SetTeamClothes(player);
                }
                if(player.CurrentMode == "dm")
                {
                    player.SetDmClothes();
                }
                player.GiveWeapons();
                player.FullArmor();
                player.SetHealth();
                //player.SetTeamSelect();
                player.InAduty = false;
                player.Emit("disableGodMode");
                player.Emit("nametags:Config", false);
                player.SetSyncedMetaData("ADUTY", false);
                return;
            }
            player.InAduty = true;
            player.Model = (uint)PedModel.Imporage;
            player.Emit("enableGodMode");
            player.Emit("nametags:Config", true);
            player.SetSyncedMetaData("ADUTY", true);
        }

        [ClientEvent("go")]
        public void go(Player player, string name)
        {

        }
    }
}

using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kolbenV2
{
    class CarShop : IScript
    {
        [ClientEvent("spawnVehicle")]
        public async void spawnVehicle(Player player, int hash)
        {
            if (player.CarShopVehicle != null)
            {
                await AltAsync.Do(() => player.CarShopVehicle.Remove());
            }
            IVehicle veh = await AltAsync.Do(() => Alt.CreateVehicle((uint)hash, new Position((float)-210.43, (float)-1323.07, (float)30.47), new Rotation(0, 0, (float)-28.542)));
            veh.Dimension = player.Id;
            player.CarShopVehicle = veh;
        }

        [ClientEvent("closeCarShop")]
        public void closeCarShop(Player player)
        {
            if (player.CarShopVehicle != null)
            {
                player.CarShopVehicle.Remove();
                player.CarShopVehicle = null;
            }
            player.SetTeamSelect();
        }

        [ClientEvent("buyVehicle")]
        public void buyVehicle(Player player, uint hash)
        {
            foreach(KeyValuePair<int, CarShopVehicle> entry in Data.CarShopVehicles)
            {
                if(entry.Value.Hash == hash)
                {
                    if (entry.Value.Price > player.Points)
                    {
                        player.SendNotificationRed("Nicht genug Geld");
                        return;
                    }
                    if (player.PlayerVehicles.Contains(entry.Value))
                    {
                        player.SendNotificationRed("Dieses Fahrzeug besitzt du bereits!");
                        return;
                    }
                    player.Points -= entry.Value.Price;
                    player.PlayerVehicles.Add(entry.Value);
                    Database db = new Database();
                    db.Insert($"INSERT INTO vehicle_data (owner_id,vehicle_id) VALUES ('{player.PlayerId}', '{entry.Value.Id}')");
                    player.SendNotificationGreen($"{entry.Value.Name} gekauft!");
                    player.Emit("loadVehicle", entry.Value.Name, entry.Value.Id);
                }                
            }
            
        }
        
    }
}

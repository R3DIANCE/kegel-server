using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace kolbenV2
{
    class VehicleEvents : IScript
    {
        public async void StartVehicleTimer(IVehicle veh)
        {
            await Task.Delay(120000);
            if (veh.Exists)
            {
                if(veh.Driver == null)
                {
                    foreach(IPlayer player in Alt.GetAllPlayers())
                    {
                        if(player.Position.Distance(veh.Position) <= 15f)
                        {
                            StartVehicleTimer(veh);
                            return;
                        }
                    }
                    veh.Remove();
                }
            }           
        }

        [ScriptEvent(ScriptEventType.PlayerLeaveVehicle)]
        public void PlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            StartVehicleTimer(vehicle);
        }
    }
}

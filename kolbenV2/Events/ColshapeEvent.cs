using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    class ColshapeEvent : IScript
    {
        [ScriptEvent(ScriptEventType.ColShape)]
        public static void OnEntityColshapeHit(IColShape shape, IEntity entity, bool state)
        {
            if(shape.Exists == false || entity.Exists == false)
            {
                return;
            }
            if (entity is Player player)
            {
                if (player.HasData("CHANGING_DUELL"))
                {
                    return;
                }
                if (player.CurrentDuell != null)
                {
                    if (state) //ENTER
                    {
                        player.SendNotificationGreen("Zone betreten!");
                        player.InCircle = true;
                    }
                    else //EXIT
                    {
                        if (player.CurrentDuell.Attacker == player)
                        {
                            player.CurrentDuell.StartOutOfCircleTimer(player, player.CurrentDuell.Defender);
                        }
                        else
                        {
                            player.CurrentDuell.StartOutOfCircleTimer(player, player.CurrentDuell.Attacker);
                        }
                    }
                }
                if (shape.GetData("TEAM", out int id))
                {
                    if (player.CurrentMode == "team")
                    {
                        if (player.TeamId == id)
                        {
                            if (state)
                            {
                                player.SendNotificationGreen("E um ein Fahrzeug aus zu parken..");
                            }
                        }
                    }
                }
            }
        }
    }
}

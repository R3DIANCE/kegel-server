using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    class WeaponDamage : IScript
    {
        [ScriptEvent(ScriptEventType.WeaponDamage)]
        public static bool IWeaponDamage(Player player, IEntity target, uint weapon, ushort damage, Position offset, BodyPart bodypart)
        {
            if (target is Player targetPlayer)
            {
                if(player.CurrentMode == "team")
                {
                    player.PlayerTeam.TeamMember.ForEach(member => { member.Emit("createTemporaryBlip", targetPlayer, 500); });
                }
                //if (targetPlayer.Armor > damage)
                //{
                //    player.Emit("damage:Armor", damage);
                //}
                //else
                //{
                //    player.Emit("damage:Health", damage);
                //}
            }
            return true;
        }
    }
}

using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolbenV2
{
    class PlayerConnect : IScript
    {
        Database db = new Database();
        [ScriptEvent(ScriptEventType.PlayerConnect)]
        public void OnPlayerConnect(Player player, string reason)
        {
            //test
            if (IsBanned(player))
            {
                player.Kick("banned.. ");
            }
            if (Data.EMPActive)
            {
                player.Emit("toogle:city:lights", true);
            }
            player.MaxArmor = 100;
            player.MaxHealth = 200;
            player.Model = 0x705E61F2;
            player.SetPosition(-182.38f, 851.98f, 232.69f);
            player.SetDateTime(DateTime.Now);
            player.Emit("setPropertiesOnConnect");
            player.LoadTeams();           
            if (AccountExist(player))
            {
                OpenLogin(player);
            }
            else
            {
                OpenRegister(player);
            }         
        }

        public async void OpenLogin(IPlayer player)
        {
            await Task.Delay(5000);
            if (player.Exists)
            {
                player.Emit("open:login", db.SelectString($"SELECT * FROM accounts WHERE socialid ={player.SocialClubId}", "name"));
            }
        }

        public async void OpenRegister(IPlayer player)
        {
            await Task.Delay(5000);
            if (player.Exists)
            {
                player.Emit("open:register");
            }       
        }

        public bool AccountExist(IPlayer player)
        {
            int id = db.SelectInt($"SELECT * FROM accounts WHERE socialid ={player.SocialClubId}", "socialid");
            if(id == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool IsBanned(Player player)
        {
            if (db.SelectInt($"SELECT * FROM banned_accounts WHERE socialclubid ={player.SocialClubId}", "socialclubid") != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

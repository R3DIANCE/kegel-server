using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolbenV2
{
    public class Player : AltV.Net.Elements.Entities.Player
    {
        public bool LoggedIn = false;
        public string PlayerName { get; set; }
        public bool IsAdmin { get; set; }
        public int PlayerId { get; set; }
        public Team PlayerTeam => GetTeam();
        public Deathmatch PlayerDeathMatchLobby => GetDeathmatch();
        public int TeamId { get; set; }
        public int DeathMatchId { get; set; }
        public int KillStreak = 0;
        public int Points { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        Database db = new Database();
        public string CurrentMode = "default";
        public bool TeamSelectOpen = false;
        public bool Animating = false;
        public int VehicleKills = 0;
        public int TeamKills = 0;
        public int DuellWins = 0;
        public bool InCircle = false;
        public bool InAduty = false;
        public Duell CurrentDuell { get; set; }
        public Random rnd = new Random();
        public BattleRoyale CurrentLobby { get; set; }
        public IVehicle CarShopVehicle = null;
        public Dictionary<string, MarkerHandler> MarkerId = new Dictionary<string, MarkerHandler>();
        public List<CarShopVehicle> PlayerVehicles = new List<CarShopVehicle>();

        public Player(IntPtr nativePointer, ushort id) : base(nativePointer, id)
        {
            
        }

        public void LoadPlayer()
        {
            this.PlayerName = db.SelectString($"SELECT * FROM accounts WHERE socialid={this.SocialClubId}", "name");
            this.PlayerId = db.SelectInt($"SELECT * FROM accounts WHERE socialid={this.SocialClubId}", "id");
            this.Points = db.SelectInt($"SELECT * FROM accounts WHERE socialid={this.SocialClubId}", "points");
            this.Kills = db.SelectInt($"SELECT * FROM accounts WHERE socialid={this.SocialClubId}", "kills");
            this.Deaths = db.SelectInt($"SELECT * FROM accounts WHERE socialid={this.SocialClubId}", "deaths");
            db.LoadPlayerVehicles(this);
        }

        public void UpdatePlayer()
        {
            db.Update($"UPDATE accounts SET points ={this.Points} WHERE socialid ={this.SocialClubId}");
            db.Update($"UPDATE accounts SET deaths ={this.Deaths} WHERE socialid ={this.SocialClubId}");
            db.Update($"UPDATE accounts SET kills ={this.Kills} WHERE socialid ={this.SocialClubId}");
        }

        public void SendNotification(string not)
        {
            //player.Emit("sendNotify", not);
            this.Emit("sendNotification", not);
        }

        public void SendNotificationRed(string not)
        {
            //player.Emit("sendNotifyColor", not, "red");
            this.Emit("sendNotificationColor", not, 6);
        }

        public void SendNotificationGreen(string not)
        {
            //player.Emit("sendNotifyColor", not, "green");
            this.Emit("sendNotificationColor", not, 210);
        }

        public void SendNotificationCustom(string not, int color)
        {
            //player.Emit("sendNotifyColor", not, color);
            this.Emit("sendNotificationColor", not, color);
        }

        public Team GetTeam()
        {
            return Data.Teams[this.TeamId];
        }

        public Deathmatch GetDeathmatch()
        {
            return Data.DmTeams[this.DeathMatchId];
        }

        public void ToogleVisible(bool visible)
        {
            this.Emit("visible", visible);
        }

        public void Freeze(bool freeze)
        {
            this.Emit("freeze", freeze);
        }

        public void SetTeamSelect()
        {
            try
            {
                Data.Teams[1].SetTeamClothes(this);
                this.GiveWeapons();
                this.FullArmor();
                this.SetHealth();
                this.TeamKills = 0;
                this.Dimension = this.PlayerId;
                this.TeamSelectOpen = true;
                this.CurrentMode = "default";
                this.TeamId = 1;
                this.DeathMatchId = 0;
                this.RemoveFromTeams();
                this.Position = new Position(402.79f, -996.85f, -99.01f);
                this.Emit("open:teamselect");
                this.UpdateProfile();
            }
            catch (InvalidCastException e)
            {
                Alt.Log(e.ToString());
            }
        }

        public void SetCarShop()
        {
            try
            {
                this.Dimension = this.PlayerId;
                this.CurrentMode = "default";
                this.TeamId = 1;
                this.DeathMatchId = 0;
                this.RemoveFromTeams();
            }
            catch (InvalidCastException e)
            {
                Alt.Log(e.ToString());
            }
        }

        public void RemoveFromTeams()
        {
            foreach (KeyValuePair<int, Team> entry in Data.Teams)
            {
                if (entry.Value.TeamMember.Contains(this))
                {
                    if (entry.Value.InGangfight)
                    {
                        this.Emit("hide:gangfight:HUD");
                        Data.CurrentGangfight.Teams.ForEach(team =>
                        {
                            team.TeamMember.ForEach(player =>
                            {
                                player.Emit("update:gangfight:count", Data.CurrentGangfight.Attacker.TeamMember.Count, Data.CurrentGangfight.Defender.TeamMember.Count);
                            });
                        });
                    }
                    entry.Value.TeamMember.Remove(this);
                    Alt.EmitAllClients("loadTeamMemeberCount", entry.Value.Id, entry.Value.TeamMember.Count);
                }
            }
            foreach (KeyValuePair<int, Deathmatch> entry in Data.DmTeams)
            {
                if (entry.Value.TeamMember.Contains(this))
                {
                    entry.Value.TeamMember.Remove(this);
                    Alt.EmitAllClients("loadDmTeamMemeberCount", entry.Value.Id, entry.Value.TeamMember.Count);
                }
            }
        }

        public void LoadTeams()
        {
            foreach (KeyValuePair<int, Team> entry in Data.Teams)
            {
                this.Emit("setBlip", 225, entry.Value.BlibColor, entry.Value.Garage.X, entry.Value.Garage.Y, entry.Value.Garage.Z, $"{entry.Value.Name}-Garage");
                this.Emit("setBlip", 40, entry.Value.BlibColor, entry.Value.Spawn.X, entry.Value.Spawn.Y, entry.Value.Spawn.Z, entry.Value.Name);
                this.Emit("createPed", entry.Value.GaragePed, entry.Value.Garage.X, entry.Value.Garage.Y, entry.Value.Garage.Z, entry.Value.NPCHeading);
                this.Emit("loadTeamMemeberCount", entry.Value.Id, entry.Value.TeamMember.Count);
            }
            foreach (KeyValuePair<int, Deathmatch> entry in Data.DmTeams)
            {
                this.Emit("loadDmTeamMemeberCount", entry.Value.Id, entry.Value.TeamMember.Count);
            }
        }

        public void SetClothes(int component, int drawable, int texture)
        {
            this.Emit("setPlayerClothes", component, drawable, texture);
        }

        public void UpdateProfile()
        {
            if (TeamId == 0)
            {
                this.Emit("updateProfileServer", this.PlayerName, "Deathmatch", Points, Kills, Deaths, KillStreak, TeamId, "black");
            }
            else
            {
                this.Emit("updateProfileServer", this.PlayerName, this.PlayerTeam.Name, Points, Kills, Deaths, KillStreak, TeamId, this.PlayerTeam.CssColor);
            }
        }

        public void PlayAnimation(string dict, string name, int duration)
        {
            this.Emit("playAnimation", dict, name, duration);
        }

        public async void RespawnPlayer()
        {
            switch (this.CurrentMode)
            {
                case "team":
                    this.Spawn(this.PlayerTeam.Spawn, 4000);
                    break;
                case "dm":
                    this.Spawn(this.PlayerDeathMatchLobby.GetRandomSpawn(), 4000);
                    break;
                case "duell":
                    this.Spawn(new Position(0, 0, 0), 1000);
                    break;
            }
            await Task.Delay(4200);
            this.FullArmor();
            if (this.CurrentMode == "team")
            {
                this.SpawnProtect();
            }
        }

        public void FullArmor()
        {
            if (this.Exists)
            {
                if (this.Armor < 100)
                {
                    this.Armor = 100;
                }
            }
        }

        public void GiveWeapons()
        {
            this.GiveWeapon(WeaponModel.AssaultRifle, 1000, true);
            this.GiveWeapon(WeaponModel.AdvancedRifle, 1000, true);
            this.GiveWeapon(WeaponModel.CarbineRifle, 1000, true);
            this.GiveWeapon(WeaponModel.BullpupRifle, 1000, true);
            this.GiveWeapon(WeaponModel.BullpupShotgun, 1000, true);
            this.GiveWeapon(WeaponModel.GusenbergSweeper, 1000, true);
            this.GiveWeapon(WeaponModel.CombatPDW, 1000, false);
            this.GiveWeapon(WeaponModel.Pistol50, 1000, false);
        }

        public void SetHealth()
        {
            this.Health = 200;
        }

        public void SpawnProtect()
        {
            this.Emit("spawnProtect");
        }

        public void SetDmClothes()
        {
            this.SetClothes(11, 332, rnd.Next(15));
            this.SetClothes(1, 37, 0);
            this.SetClothes(4, 12, 0);
            this.SetClothes(6, 6, 0);
            this.SetClothes(3, 12, 0);
            this.SetClothes(8, 57, 0);
        }

        public void CreateMarker(string name, int type, Position pos, float scale, float height, int r, int g, int b, int alpha)
        {
            this.Emit("markers:Create", name, type, pos.X, pos.Y, pos.Z, 0, 0, 0, 0, 0, 0, scale, scale, height, r, g, b, alpha);
        }

        public void DestroyMarker(string name)
        {
            this.Emit("markers:Delete", name);
        }
    }
}

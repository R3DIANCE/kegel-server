using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kolbenV2
{
    public class Team : Base
    {
        public int Id { get; set; }
        public Position Spawn { get; set; }
        public string Name { get; set; }
        public List<Player> TeamMember = new List<Player>();
        public Position Garage { get; set; }
        public Position GarageSpawn { get; set; }
        public IColShape SpawnColShape { get; set; }
        public float GarageSpawnHeading { get; set; }
        public string GaragePed { get; set; }
        public float NPCHeading { get; set; }
        public string CssColor { get; set; }
        public int NativeColor { get; set; }
        public int BlibColor { get; set; }
        public int GangfightPoints = 0;
        public bool InGangfight = false;
        public int VehicleColor { get; set; }
        public Dictionary<int, List<int>> Clothes = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> Props = new Dictionary<int, List<int>>();

        public Team()
        {

        }

        public Team(int teamId)
        {
            Id = teamId;
            LoadTeam();            
        }

        
        public void AddPlayer(Player player)
        {           
            if (this.InGangfight)
            {
                player.Emit("gangfight:textlabel", this.Spawn.X, this.Spawn.Y, this.Spawn.Z, "Gangfight beitreten (E)");
            }
            else
            {
                player.Emit("gangfight:textlabel", this.Spawn.X, this.Spawn.Y, this.Spawn.Z, "Gangfight starten (E)");
            }
            foreach(KeyValuePair<int, Team> entry in Data.Teams)
            {
                player.DestroyMarker(entry.Value.Name + "SPAWNZONE");
                if(entry.Value != this)
                {                    
                    player.CreateMarker(entry.Value.Name + "SPAWNZONE", 1, new Position(entry.Value.Spawn.X, entry.Value.Spawn.Y, entry.Value.Spawn.Z - 20), 40f, 50f, 255, 0, 0, 50);
                }
            }      
            this.SendTeamMessage($"{player.PlayerName} ist deinem Team beigetreten!");
            player.SendNotificationCustom($"Du bist {this.Name} beigetreten!", this.CssColor);
            this.TeamMember.Add(player);
            Alt.EmitAllClients("loadTeamMemeberCount", this.Id, this.TeamMember.Count);
            player.Position = this.Spawn;
            player.TeamId = this.Id;
            player.CurrentMode = "team";
            player.FullArmor();
            player.GiveWeapons();
            player.Dimension = 0;
            //foreach (Player member in this.TeamMember)
            //{
            //    player.player.Emit("teamBlip", member.player, this.BlibColor);
            //    member.player.Emit("teamBlip", player.player, this.BlibColor);
            //}
        }

        public void SendTeamMessage(string msg)
        {
            this.TeamMember.ForEach(player => { player.SendNotificationCustom(msg, this.CssColor); });
        }

        public void LoadTeam()
        {
            Database db = new Database();
            float spawnX = db.SelectFloat($"SELECT * FROM team WHERE id={this.Id}", "spawn_x");
            float spawnY = db.SelectFloat($"SELECT * FROM team WHERE id={this.Id}", "spawn_y");
            float spawnZ = db.SelectFloat($"SELECT * FROM team WHERE id={this.Id}", "spawn_z");
            this.Spawn = new Position(spawnX, spawnY, spawnZ);
            this.Name = db.SelectString($"SELECT * FROM team WHERE id={this.Id}", "name");
            this.CssColor = db.SelectString($"SELECT * FROM team WHERE id={this.Id}", "css_color");
            this.BlibColor = db.SelectInt($"SELECT * FROM team WHERE id={this.Id}", "blib_color");
            this.NativeColor = db.SelectInt($"SELECT * FROM team WHERE id={this.Id}", "native_color");
            this.VehicleColor = db.SelectInt($"SELECT * FROM garages_spawns WHERE teamid={this.Id}", "color");
            this.LoadGarage();
            this.LoadSpawnColShape();
        }

        public void LoadSpawnColShape()
        {
            this.SpawnColShape = Alt.CreateColShapeCircle(this.Spawn, 20f);
            this.SpawnColShape.SetData("SPAWNZONE", this.Id);
        }

        public void LoadGarage()
        {
            Database db = new Database();
            float garageX = db.SelectFloat($"SELECT * FROM garage WHERE teamid={this.Id}", "pos_x");
            float garageY = db.SelectFloat($"SELECT * FROM garage WHERE teamid={this.Id}", "pos_y");
            float garageZ = db.SelectFloat($"SELECT * FROM garage WHERE teamid={this.Id}", "pos_z");
            this.Garage = new Position(garageX, garageY, garageZ);
            this.GaragePed = db.SelectString($"SELECT * FROM garage WHERE teamid={this.Id}", "ped");
            this.NPCHeading = db.SelectFloat($"SELECT * FROM garage WHERE teamid={this.Id}", "heading");
            float spawnX = db.SelectFloat($"SELECT * FROM garages_spawns WHERE id={this.Id}", "spawn_x");
            float spawnY = db.SelectFloat($"SELECT * FROM garages_spawns WHERE id={this.Id}", "spawn_y");
            float spawnZ = db.SelectFloat($"SELECT * FROM garages_spawns WHERE id={this.Id}", "spawn_z");
            this.GarageSpawn = new Position(spawnX, spawnY, spawnZ);
            this.GarageSpawnHeading = db.SelectFloat($"SELECT * FROM garages_spawns WHERE id={this.Id}", "spawn_heading");
            IColShape shape = Alt.CreateColShapeCircle(this.Garage, 5f);
            shape.SetData("GARAGE", this.Id);
        }

        public async void SpawnTeamVehicle(Player player, uint model)
        {
            foreach (IVehicle veh in Alt.GetAllVehicles())
            {
                if (veh.Position.Distance(this.GarageSpawn) <= 1f)
                {
                    player.SendNotificationRed("Ausparkpunkt belegt!");
                    return;
                }
            }
            IVehicle teamVeh = await AltAsync.Do(() => Alt.CreateVehicle(model, this.GarageSpawn, new Rotation(0, 0, this.GarageSpawnHeading)));
            teamVeh.Dimension = player.Dimension;
            teamVeh.PrimaryColor = (byte)this.VehicleColor;
            teamVeh.SecondaryColor = (byte)this.VehicleColor;
            teamVeh.NumberplateText = player.PlayerName;
            await Task.Delay(200);
            if (player.Exists && teamVeh.Exists)
            {
                player.Emit("setPlayerIntoVehicle", teamVeh);
            }       
        }

        public void SetTeamClothes(Player player)
        {
            foreach (KeyValuePair<int, List<int>> entry in this.Clothes)
            {
                player.SetClothes(entry.Key, entry.Value[0], entry.Value[1]);
            }
            foreach (KeyValuePair<int, List<int>> entry in this.Props)
            {
                player.Emit("prop", entry.Key, entry.Value[0], entry.Value[1]);
            }
        }
    }
}

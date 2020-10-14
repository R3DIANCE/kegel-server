using AltV.Net;
using AltV.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    public class Deathmatch : IScript
    {
        public int Id { get; set; }
        public List<Position> Spawns = new List<Position>();
        public string Name { get; set; }
        public List<Player> TeamMember = new List<Player>();
        Random rnd = new Random();
        Database db = new Database();
        public Deathmatch()
        {

        }

        public Deathmatch(int dmId)
        {
            Id = dmId;
            LoadDeathmatch();
        }

        public void AddPlayer(Player player)
        {
            this.TeamMember.Add(player);
            Alt.EmitAllClients("loadDmTeamMemeberCount", this.Id, this.TeamMember.Count);
            player.Position = this.GetRandomSpawn();
            player.DeathMatchId = this.Id;
            player.CurrentMode = "dm";
            player.FullArmor();
            player.GiveWeapons();
            player.Dimension = this.Id;
        }

        public void LoadDeathmatch()
        {
            this.Name = db.SelectString($"SELECT * FROM deathmatch WHERE id={this.Id}", "name");
            LoadSpawns();
        }

        public void LoadSpawns()
        {
            
            this.Spawns = db.GetPostitionList($"SELECT * FROM deathmatch_spawns WHERE dmid={this.Id}");
        }

        public Position GetRandomSpawn()
        {
            return this.Spawns[rnd.Next(this.Spawns.Count)];
        }
    }
}

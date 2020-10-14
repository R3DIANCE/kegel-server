using AltV.Net;
using AltV.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    public class DuellArena : IScript
    {
        public int Id { get; set; }
        public Position Spawn1 { get; set; }
        public Position Spawn2 { get; set; }
        public Position Center { get; set; }
        public double MarkerScale { get; set; }

        public DuellArena()
        {

        }

        public DuellArena(int id, Position spawn1, Position spawn2)
        {
            this.Id = id;
            this.Spawn1 = spawn1;
            this.Spawn2 = spawn2;
            this.Center = new Position((this.Spawn1.X + this.Spawn2.X) / 2, (this.Spawn1.Y + this.Spawn2.Y) / 2, ((this.Spawn1.Z + this.Spawn2.Z) / 2) - 40);
            this.MarkerScale = (this.Spawn1.Distance(this.Spawn2)) * 1.5;
        }
    }
}

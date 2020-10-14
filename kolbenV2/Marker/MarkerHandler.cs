using AltV.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    public class MarkerHandler
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public Position Position { get; set; }
        public float Scale { get; set; }
        public float Height { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int Alpha { get; set; }

        public MarkerHandler(int id, int type, Position pos, float sclae, float height, int r, int g, int b, int alpha)
        {
            this.Id = id;
            this.Type = type;
            this.Position = pos;
            this.Scale = sclae;
            this.Height = height;
            this.R = r;
            this.G = g;
            this.B = b;
            this.Alpha = alpha;            
        }
    }
}

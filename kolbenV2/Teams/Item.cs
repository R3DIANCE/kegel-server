using AltV.Net;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    public class Item : IScript
    {
        public int Id { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public int Hash { get; set; }

        public WeaponModel Model{ get; set; }

        public Item()
        {

        }

        public Item(int id, float x, float y, float z, int hash, string name)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Hash = hash;
            WeaponModel model;
            if (Enum.TryParse(name, true, out model))
            {
                this.Model = model;
            }
        }
    }
}

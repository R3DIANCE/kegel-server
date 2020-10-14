using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    public class CarShopVehicle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public int Price { get; set; }
        public uint Hash { get; set; }

        public CarShopVehicle(int id, string name, string clas, int price, uint hash)
        {
            this.Id = id;
            this.Name = name;
            this.Class = clas;
            this.Price = price;
            this.Hash = hash;
            Data.CarShopVehicles.Add(this.Id, this);
        }
    }
}

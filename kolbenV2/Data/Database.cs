using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Enums;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    class Database
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public Database()
        {
            Initialize();
        }

        private void Initialize()
        {
            server = "localhost";
            database = "dmkolben";
            uid = "root";
            password = "";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }

        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                Alt.Log("DATABASE-ERROR" + ex.Message);
                return false;
            }
        }

        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Alt.Log("DATABASE-ERROR" + ex.Message);
                return false;
            }
        }

        public void Insert(string query)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, this.connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public void Update(string query)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public void Delete(string query)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public int SelectInt(string query, string column)
        {
            int value = 0;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    value = reader.GetInt32(column);
                }
                reader.Close();
                this.CloseConnection();
            }
            return value;
        }

        public float SelectFloat(string query, string column)
        {
            float value = 0;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    value = reader.GetFloat(column);
                }
                reader.Close();
                this.CloseConnection();
            }
            return value;
        }

        public string SelectString(string query, string column)
        {
            string value = "";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    value = reader.GetString(column);
                }
                reader.Close();
                this.CloseConnection();
            }
            return value;
        }

        public int ColumnCount(string query)
        {
            int count = 0;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    count++;
                }
                reader.Close();
                this.CloseConnection();
            }
            return count;
        }

        public List<Position> GetPostitionList(string query)
        {
            List<Position> list = new List<Position>();
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Position(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z")));
                }
                reader.Close();
                this.CloseConnection();
            }
            return list;
        }

        public void LoadDuell()
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM duell_arenas", connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DuellArena arena = new DuellArena(
                        reader.GetUInt16("id"),
                        new Position(reader.GetFloat("spawn1_x"), reader.GetFloat("spawn1_y"), reader.GetFloat("spawn1_z")),
                        new Position(reader.GetFloat("spawn2_x"), reader.GetFloat("spawn2_y"), reader.GetFloat("spawn2_z"))
                    );
                    Data.DuellArenas.Add(arena);
                }
                reader.Close();
                this.CloseConnection();
            }
        }

        public Item LoadBattleRoyaleItem()
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand("select * from br_items", connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Data.BattleRoyaleItems.Add(reader.GetInt32("hash"), reader.GetString("name"));
                }
                reader.Close();
                this.CloseConnection();
            }
            return null;
        }

        public void LoadTeamClothes(int id)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM clothes WHERE teamid={id}", connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    List<int> list = new List<int>();
                    list.Add(reader.GetInt32("drawable"));
                    list.Add(reader.GetInt32("texture"));
                    Data.Teams[id].Clothes.Add(reader.GetInt32("slot"), list);
                }
                reader.Close();
                this.CloseConnection();
            }
        }

        public void LoadTeamProps(int id)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM prop WHERE teamid={id}", connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    List<int> list = new List<int>();
                    list.Add(reader.GetInt32("drawable"));
                    list.Add(reader.GetInt32("texture"));
                    Data.Teams[id].Props.Add(reader.GetInt32("slot"), list);
                }
                reader.Close();
                this.CloseConnection();
            }
        }

        public void LoadBattleRoyaleMaps()
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM br_arenas", connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Data.BattleRoyaleMap.Add(reader.GetInt16("id"), new Position(reader.GetFloat("x"), reader.GetFloat("y"), reader.GetFloat("z")));
                }
                reader.Close();
                this.CloseConnection();
            }
        }

        public void LoadBattleRoyaleItemSpawns()
        {
            foreach (KeyValuePair<int, Position> entry in Data.BattleRoyaleMap)
            {
                Data.BattleRoyaleItemSpawns.Add(entry.Key, new List<Position>());
            }

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM br_items_spawns", connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {                    
                    Data.BattleRoyaleItemSpawns[reader.GetInt32("arena_id")].Add(new Position(reader.GetFloat("x"), reader.GetFloat("y"), reader.GetFloat("z")));
                }
                reader.Close();
                this.CloseConnection();
            }
        }


        public void LoadVehicleShop()
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM carshop_vehicles", connection);
                MySqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    new CarShopVehicle(reader.GetInt32("id"), reader.GetString("name"), reader.GetString("class"), reader.GetInt32("price"), (uint)reader.GetInt32("hash"));                   
                }
                reader.Close();
                this.CloseConnection();
            }
        }

        public void LoadPlayerVehicles(Player player)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM vehicle_data WHERE owner_id = {player.PlayerId}", connection);
                MySqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    player.PlayerVehicles.Add(Data.CarShopVehicles[reader.GetInt32("vehicle_id")]);
                    player.Emit("loadVehicle", Data.CarShopVehicles[reader.GetInt32("vehicle_id")].Name, Data.CarShopVehicles[reader.GetInt32("vehicle_id")].Id);
                }
                reader.Close();
                this.CloseConnection();
            }
        }
    }
}

using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Elements.Pools;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace kolbenV2
{
    public class Data
    {
        public static Dictionary<int, Team> Teams = new Dictionary<int, Team>();
        public static Dictionary<int, Deathmatch> DmTeams = new Dictionary<int, Deathmatch>();
        public static List<DuellArena> DuellArenas = new List<DuellArena>();
        public static BattleRoyale CurrentLobby { get; set; }
        public static Dictionary<int, string> BattleRoyaleItems = new Dictionary<int, string>();
        public static Dictionary<int, Position> BattleRoyaleMap = new Dictionary<int, Position>();
        public static Dictionary<int, List<Position>> BattleRoyaleItemSpawns = new Dictionary<int, List<Position>>();
        public static Dictionary<int, CarShopVehicle> CarShopVehicles = new Dictionary<int, CarShopVehicle>();
        public static Gangfight CurrentGangfight = null;
        public static bool EMPActive = false;
    }
}

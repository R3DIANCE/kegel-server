using AltV.Net.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    public class GangfightData
    {
        public static Dictionary<int, List<Position>> GangfightFlags = new Dictionary<int, List<Position>>
        {
            {2, new List<Position>{
                new Position(243, -1760, (float)29.2),
                new Position(197, -1671, (float)29.9)
            }},
            {3, new List<Position>{
                new Position(243, -1760, (float)29.2),
                new Position(197, -1671, (float)29.9)
            }},
            {4, new List<Position>{
                new Position(243, -1760, (float)29.2),
                new Position(197, -1671, (float)29.9)
            }},
            {1, new List<Position>{
                new Position(-1024, -1427, (float)5.1),
                new Position(-1130, -1451, (float)4.9)
            }},
            {5, new List<Position>{
                new Position(-1024, -1427, (float)5.1),
                new Position(-1130, -1451, (float)4.9)
            }},
            {7, new List<Position>{
                new Position(-1024, -1427, (float)5.1),
                new Position(-1130, -1451, (float)4.9)
            }},
            {8, new List<Position>{
                new Position(436, 254, (float)103.2),
                new Position(634, 193, (float)96.81)
            }},
            {6, new List<Position>{
                new Position(436, 254, (float)103.2),
                new Position(634, 193, (float)96.81)
            }},
        };

        public static Dictionary<int, List<int>> PossiblePairs = new Dictionary<int, List<int>>
        {
            {2, new List<int>{ 3, 4} },
            {3, new List<int>{ 2, 4} },
            {4, new List<int>{ 2, 3} },
            {1, new List<int>{ 5, 7} },
            {5, new List<int>{ 1, 7} },
            {7, new List<int>{ 5, 1} },
            {8, new List<int>{ 6} },
            {6, new List<int>{ 8} },
        };

        
    }
}

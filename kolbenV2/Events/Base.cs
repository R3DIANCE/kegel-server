using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using AltV.Net.EntitySync.ServerEvent;
using AltV.Net.EntitySync.SpatialPartitions;
using System;
using System.Collections.Generic;
using System.Timers;

namespace kolbenV2
{
    public class Base : AsyncResource
    {       
        Timer DestroyVehTimer = new Timer();
        Timer LostPlayer = new Timer();
        Timer GlobalMessage = new Timer();
        public void StartDestroyedVehicleTimer()
        {
            DestroyVehTimer = new Timer();
            DestroyVehTimer.Interval = 10000;
            DestroyVehTimer.Elapsed += new ElapsedEventHandler(DestroyVehTimerElapsed);
            DestroyVehTimer.Start();
            DestroyVehTimer.AutoReset = true;
        }

        public void DestroyVehTimerElapsed(object sender, EventArgs e)
        {
            foreach(IVehicle veh in Alt.GetAllVehicles())
            {
                if (veh.IsDestroyed)
                {
                    veh.Remove();
                }
            }
        }

        public void StartGlobalMessageTimer()
        {
            GlobalMessage = new Timer();
            GlobalMessage.Interval = 60000;
            GlobalMessage.Elapsed += new ElapsedEventHandler(GlobalMessageElapsed);
            GlobalMessage.Start();
            GlobalMessage.AutoReset = true;
        }

        public void GlobalMessageElapsed(object sender, EventArgs e)
        {
            foreach (Player player in Alt.GetAllPlayers())
            {
                if(player.LoggedIn == false)
                {
                    return;
                }
                player.SendNotificationGreen("Dir gefällt unserer Server? /discord um unserem Discord bei zu treten!");
            }
        }

        public void StartLostPlayerTimer()
        {
            LostPlayer = new Timer();
            LostPlayer.Interval = 10000;
            LostPlayer.Elapsed += new ElapsedEventHandler(LostPlayerElapsed);
            LostPlayer.Start();
            LostPlayer.AutoReset = true;
        }

        public void LostPlayerElapsed(object sender, EventArgs e)
        {
            foreach (Player player in Alt.GetAllPlayers())
            {
                if (!player.Exists)
                {
                    player.Kick("bug - reconnect..");
                }
            }
        }

        public override IEntityFactory<IPlayer> GetPlayerFactory()
        {
            return new MyPlayerFactory();
        }

        public override void OnStart()
        {
            AltEntitySync.Init(1, 100, false,
            (threadCount, repository) => new ServerEventNetworkLayer(threadCount, repository),
            (entity, threadCount) => (entity.Id % threadCount),
            (entityId, entityType, threadCount) => (entityId % threadCount),
            (threadId) => new LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 300),
            new IdProvider());

            StartDestroyedVehicleTimer();
            StartGlobalMessageTimer();
            StartLostPlayerTimer();

            Database db = new Database();

            db.LoadBattleRoyaleItem();
            db.LoadBattleRoyaleMaps();
            db.LoadBattleRoyaleItemSpawns();
            db.LoadVehicleShop();

            for (int i = 0; i < db.ColumnCount("SELECT * FROM `team`"); i++)
            {
                Team team = new Team(i + 1);
                Data.Teams.Add(i + 1, team);
                db.LoadTeamClothes(i + 1);
                db.LoadTeamProps(i + 1);
            }

            for (int i = 0; i < db.ColumnCount("SELECT * FROM `deathmatch`"); i++)
            {
                Deathmatch dmteam = new Deathmatch(i + 1);
                Data.DmTeams.Add(i + 1, dmteam);
            }
            db.LoadDuell();
        }
       
        public override void OnStop()
        {

        }
    }
}

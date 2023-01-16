using AS_Vranicich.Utilities;
using AS_Vranicich.DbContext;
using Microsoft.Data.SqlClient;
using TAP22_23.AlarmClock.Interface;
using TAP22_23.AuctionSite.Interface;
using System;
using Microsoft.EntityFrameworkCore;
using AS_Vranicich.Models;

namespace AS_Vranicich.Logic
{
    public class Logic
    {
        public class HostFactory : IHostFactory
        {
            public void CreateHost(string connectionString)
            {
                MyVerify.ConnectionStringVerify(connectionString);

                using var c = new AsDbContext(connectionString);
                try
                {
                    c.Database.EnsureDeleted();
                    c.Database.EnsureCreated();
                    c.SaveChanges();
                }
                catch (SqlException e)
                {
                    throw new AuctionSiteUnavailableDbException(
                        "Cannot instantiate connection with Database, bad connectionString", e);
                }
            }

            public IHost LoadHost(string connectionString, IAlarmClockFactory alarmClockFactory)
            {
                MyVerify.ConnectionStringVerify(connectionString);

                if (alarmClockFactory == null)
                    throw new AuctionSiteArgumentNullException("Alarm clock can't be null");

                using var c = new AsDbContext(connectionString);
                MyVerify.DB_ContextVerify(c);

                return new Host(connectionString, alarmClockFactory);

            }


            public class Host : IHost
            {
                public string ConnectionString { get; set; }
                public IAlarmClockFactory AlarmClockFactory { get; set; }

                public Host(string connectionString, IAlarmClockFactory alarmClockFactory)
                {
                    ConnectionString = connectionString;
                    AlarmClockFactory = alarmClockFactory;
                }

                public void CreateSite(string name, int timezone, int sessionExpirationTimeInSeconds,
                    double minimumBidIncrement)
                {
                    MyVerify.TimezoneVerify(timezone, sessionExpirationTimeInSeconds, minimumBidIncrement);
                    MyVerify.SiteNameVerify(name);

                    using var c = new AsDbContext(ConnectionString);
                    MyVerify.DB_ContextVerify(c);

                    try
                    {
                        var site = c.Sites.SingleOrDefault(s => s.Name == name);
                        if (site != null)
                        {
                            throw new AuctionSiteNameAlreadyInUseException($"{nameof(name)} already exists");
                        }

                        c.Sites.Add(new Site()
                        {
                            Name = name,
                            Timezone = timezone,
                            SessionExpirationInSeconds = sessionExpirationTimeInSeconds,
                            MinimumBidIncrement = minimumBidIncrement
                        });

                        c.SaveChanges();
                    }
                    catch (DbUpdateException e)
                    {
                        throw new AuctionSiteUnavailableDbException();
                    }
                    
                }

                public IEnumerable<(string Name, int TimeZone)> GetSiteInfos()
                {
                    using var c = new AsDbContext(ConnectionString);
                    List<Site> sites = new List<Site>();

                    try
                    {
                        MyVerify.DB_ContextVerify(c);

                        sites = c.Sites.ToList();
                    }
                    catch (ArgumentNullException e)
                    {
                        throw new AuctionSiteArgumentNullException(e.Message, e);
                    }

                    foreach (var s in sites)
                    {
                        yield return (s.Name, s.Timezone);
                    }
                }

                public ISite LoadSite(string name)
                {
                    MyVerify.SiteNameVerify(name);
                    using var c = new AsDbContext(ConnectionString);
                    MyVerify.DB_ContextVerify(c);

                    try
                    { 
                        var site = c.Sites.Single(n => n.Name == name);
                        return site;
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new AuctionSiteInexistentNameException($"{nameof(name)}", "This name not exist in DbSet",
                            e);
                    }
                }
            }

        }
    }
}

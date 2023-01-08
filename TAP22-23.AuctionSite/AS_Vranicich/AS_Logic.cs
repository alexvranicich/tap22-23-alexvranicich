using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP22_23.AlarmClock.Interface;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich
{
    public class AS_Logic
    {
        public class HostFactory : IHostFactory
        {
            public void CreateHost(string connectionString)
            {
                throw new NotImplementedException();
            }

            public IHost LoadHost(string connectionString, IAlarmClockFactory alarmClockFactory)
            {
                throw new NotImplementedException();
            }
        }

        public class Host : IHost
        {
            public void CreateSite(string name, int timezone, int sessionExpirationTimeInSeconds, double minimumBidIncrement)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<(string Name, int TimeZone)> GetSiteInfos()
            {
                throw new NotImplementedException();
            }

            public ISite LoadSite(string name)
            {
                throw new NotImplementedException();
            }
        }

        public class LogicSite : ISite
        {
            public string Name { get; }
            public int Timezone { get; }
            public int SessionExpirationInSeconds { get; }
            public double MinimumBidIncrement { get; }

            public IEnumerable<IUser> ToyGetUsers()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ISession> ToyGetSessions()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IAuction> ToyGetAuctions(bool onlyNotEnded)
            {
                throw new NotImplementedException();
            }

            public ISession? Login(string username, string password)
            {
                throw new NotImplementedException();
            }

            public void CreateUser(string username, string password)
            {
                throw new NotImplementedException();
            }

            public void Delete()
            {
                throw new NotImplementedException();
            }

            public DateTime Now()
            {
                throw new NotImplementedException();
            }

        }


        public class LogicUser : IUser
        {
            public string Username { get; }

            public IEnumerable<IAuction> WonAuctions()
            {
                throw new NotImplementedException();
            }

            public void Delete()
            {
                throw new NotImplementedException();
            }
        }

        public class LogicSession : ISession
        {
            public string Id { get; }
            public DateTime ValidUntil { get; }
            public IUser User { get; }
            
            public void Logout()
            {
                throw new NotImplementedException();
            }

            public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
            {
                throw new NotImplementedException();
            }
        }

        public class LogicAuction : IAuction
        {
            public int Id { get; }
            public IUser Seller { get; }
            public string Description { get; }
            public DateTime EndsOn { get; }

            public IUser? CurrentWinner()
            {
                throw new NotImplementedException();
            }

            public double CurrentPrice()
            {
                throw new NotImplementedException();
            }

            public void Delete()
            {
                throw new NotImplementedException();
            }

            public bool Bid(ISession session, double offer)
            {
                throw new NotImplementedException();
            }

        }

    }
}

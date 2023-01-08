using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.AS_Models
{
    public class Site : ISite
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

}

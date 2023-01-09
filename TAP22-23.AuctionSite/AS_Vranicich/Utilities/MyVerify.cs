using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Utilities
{
    public class MyVerify
    {
        public static void ConnectionStringVerify(string connectionString)
        {
            if (connectionString == null)
                throw new AuctionSiteArgumentNullException("Connection string must be non null");
        }

        public static void SiteNameVerify(string name)
        {
            if (name == null) 
                throw new AuctionSiteArgumentNullException("Name must be non null");

            if (DomainConstraints.MinSiteName > name.Length || DomainConstraints.MaxSiteName < name.Length)
                throw new AuctionSiteArgumentException("Length name is out of Range");
        }

        public static void TimezoneVerify(int timezone, int sessionExpiration, double bidIncrement)
        {
            if (timezone < DomainConstraints.MinTimeZone || timezone > DomainConstraints.MaxTimeZone)
                throw new AuctionSiteArgumentOutOfRangeException("Timezone is out of range");

            if (sessionExpiration < 0)
                throw new AuctionSiteArgumentOutOfRangeException("Session expiration time must be positive");

            if (bidIncrement < 0)
                throw new AuctionSiteArgumentNullException("Bid incremention must be positive");
        }
    }
}

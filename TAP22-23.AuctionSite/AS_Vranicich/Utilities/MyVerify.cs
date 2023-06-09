﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AS_Vranicich.DbContext;
using AS_Vranicich.Models;
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

        public static void UsernamePasswordVerify(string username, string password)
        {
            if (username == null || password == null)
                throw new AuctionSiteArgumentNullException("Username and Password must be non null");

            if (username.Length < DomainConstraints.MinUserName || username.Length > DomainConstraints.MaxUserName)
                throw new AuctionSiteArgumentException($"{nameof(username)} is Out of Range");

            if (password.Length < DomainConstraints.MinUserPassword)
            {
                throw new AuctionSiteArgumentException("Password is too smaller");
            }
        }

        public static void TimezoneVerify(int timezone, int sessionExpiration, double bidIncrement)
        {
            if (timezone < DomainConstraints.MinTimeZone || timezone > DomainConstraints.MaxTimeZone)
                throw new AuctionSiteArgumentOutOfRangeException("Timezone is out of range");

            if (sessionExpiration < 0)
                throw new AuctionSiteArgumentOutOfRangeException("Session expiration time must be positive");

            if (bidIncrement < 0)
                throw new AuctionSiteArgumentOutOfRangeException("Bid incremention must be positive");
        }

        public static void DB_ContextVerify(AsDbContext context)
        {
            if (!context.Database.CanConnect())
                throw new AuctionSiteUnavailableDbException(
                    "Cannot instantiate connection with Database, bad connectionString");
        }


        public static void BidSessionVerify(ISession session, double offer)
        {
            if (offer < 0)
                throw new AuctionSiteArgumentOutOfRangeException("The offer must be positive");

            if (session == null)
                throw new AuctionSiteArgumentNullException("There is no session");
        }

        public static void AuctionVerify(string description, double startingPrice)
        {
            if (description == null)
                throw new AuctionSiteArgumentNullException("Description must be non null");
            if (description == String.Empty)
                throw new AuctionSiteArgumentException("Description must be no empty");
            if (startingPrice < 0)
                throw new AuctionSiteArgumentOutOfRangeException("Starting price must be positive");
        }
    }
}

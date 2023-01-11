﻿using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    public class Session : ISession
    {
        /*
         * Properties
         */
        public string Id { get; set; }
        public DateTime ValidUntil { get; }
        public IUser User { get; }
        public int UserId { get; set; }
        public int SiteId { get; set; }
        public Site Site { get; set; }

        /*
         * Methods
         */

        public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }
    }
}

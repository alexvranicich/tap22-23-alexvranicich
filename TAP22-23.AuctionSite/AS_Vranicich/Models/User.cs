using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using AS_Vranicich.DbContext;
using AS_Vranicich.Utilities;
using 
using Microsoft.EntityFrameworkCore;
using TAP22_23.AlarmClock.Interface;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User : IUser
    {
        /*
         * Properties 
         */
        public int UserId { get; set; }

        [MaxLength(DomainConstraints.MaxUserName)]
        [MinLength(DomainConstraints.MinUserName)]
        public string Username { get; set; }

        [MinLength(DomainConstraints.MinUserPassword)]
        public string Password { get; set; }

        public IAlarmClock AlarmClock { get; }

        public int SiteId { get; set; }
        public Site? SiteUser { get; set; }

        public Session SessionUser { get; set; }
        public List<Auction>? AuctionsUser { get; set; }

        

        /*
         * Methods
         */

        public IEnumerable<IAuction> WonAuctions()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);


        }

        public void Delete()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);


            var currUser = c.Users.Single(u => u.UserId == UserId && u.SiteId == SiteId);

            var noEndAuction = c.Auctions.All(a =>
                a.Seller.Username == currUser.Username && a.SiteId == currUser.SiteId && a.EndsOn > AlarmClock.Now);

            if (noEndAuction)
                throw new AuctionSiteInvalidOperationException("Can't delete this user, this user has a non ended auction");

            var currWin = c.Auctions.SingleOrDefault(c => c.CurrentWinner() == currUser);

            if (currWin != null)
                throw new AuctionSiteInvalidOperationException("Can't delete this user, this user is winning an auction")

            c.Users.Remove(currUser);
            c.SaveChanges();
        }

    }
}

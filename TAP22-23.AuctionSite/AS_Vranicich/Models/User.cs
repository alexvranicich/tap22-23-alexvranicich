using System.ComponentModel.DataAnnotations;
using AS_Vranicich.DbContext;
using AS_Vranicich.Utilities;
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

        public int SiteId { get; set; }
        public Site SiteUser { get; set; }

        public Session SessionUser { get; set; }
        public List<Auction> AuctionsUser { get; set; }

        

        /*
         * Methods
         */

        public IEnumerable<IAuction> WonAuctions()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            return c.Auctions.Where(a => a.WinningUser == Username).AsEnumerable();
        }

        public void Delete()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);


            var currUser = c.Users.Single(u => u.UserId == UserId && u.SiteId == SiteId);

            var noEndAuction = c.Auctions.SingleOrDefault(a =>SiteId == currUser.SiteId);

            if (noEndAuction == null || noEndAuction.EndsOn > SiteUser.Now())
                throw new AuctionSiteInvalidOperationException($"Can't delete this user, this user has a non ended auction");

            var currWin = c.Auctions.SingleOrDefault(c => c.CurrentWinner() == currUser);

            if (currWin != null)
                throw new AuctionSiteInvalidOperationException(
                    "Can't delete this user, this user is winning an auction");

            c.Users.Remove(currUser);
            c.SaveChanges();
        }

    }
}

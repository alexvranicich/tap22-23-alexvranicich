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
        public override bool Equals(Object u2)
        {
            if (u2 == null) return false;
            if (u2 is User userTwo)
            {
                return SiteId == userTwo.SiteId &&
                       Username == userTwo.Username;
            }
            return false;
        }

        public IEnumerable<IAuction> WonAuctions()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            var allWonAuctions = c.Auctions.Where(a => a.WinningUsername == Username);

            foreach (var auction in allWonAuctions)
            {
                yield return auction;
            }
        }

        public void Delete()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            User currUser;

            try
            {
                currUser = c.Users.Single(u => u.UserId == UserId && u.SiteId == SiteId);

                var noAuction = c.Auctions.Where(a => a.Seller == currUser);
                if (noAuction.Any())
                    throw new AuctionSiteInvalidOperationException($"Can't delete this user, this user not have auction");

                var currWin = c.Auctions.Where(c => c.WinningUsername == currUser.Username);
                if (currWin.Any())
                    throw new AuctionSiteInvalidOperationException(
                        "Can't delete this user, this user is winning an auction");
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException(e.Message, e);
            }

            c.Users.Remove(currUser);
            c.SaveChanges();
            c.Dispose();
        }

    }
}

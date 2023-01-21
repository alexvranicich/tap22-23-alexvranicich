using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AS_Vranicich.DbContext;
using AS_Vranicich.Utilities;
using TAP22_23.AlarmClock.Interface;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    public class Session : ISession
    {
        /*
         * Properties
         */
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public string Id { get; set; }
        public DateTime ValidUntil { get; set; }
        [NotMapped]
        public IUser User { get; set; }
        
        public int UserId { get; set; }
        public Site Site { get; set; }
        public int SiteId { get; set; }
        public List<Auction>? Auctions { get; set; }

        /*
         * Methods
         */

        public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
        {
            MyVerify.AuctionVerify(description, startingPrice);

            if (ValidUntil < Site.Now())
                throw new AuctionSiteInvalidOperationException("Session is expired or not exist");

            if (endsOn < Site.Now())
                throw new AuctionSiteUnavailableTimeMachineException("Auction is expired");

            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            var user = c.Users.Single(u => u.Username == User.Username);
            var site = c.Sites.Single(s => s.SiteId == user.SiteId);

            ValidUntil = Site.SiteClock.Now.AddSeconds(Site.SessionExpirationInSeconds);


            var currSession = c.Sessions.Single(s => s.Id == Id);
            currSession.ValidUntil = ValidUntil;
            c.Sessions.Update(currSession);

            var auction = new Auction()
            {
                EndsOn = endsOn,
                Description = description,
                CurrPrice = startingPrice,
                Seller = User,
                Site = Site,
                SiteId = SiteId,
            };

            c.Auctions.Add(auction);
            c.SaveChanges();
            return auction;
        }

        public void Logout()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            var currSession = c.Sessions.SingleOrDefault(s => s.Id == Id);
            if (currSession != null)
            {
                c.Sessions.Remove(currSession);
                c.SaveChanges();
                c.Dispose();
            }
        }

        public bool IsValid()
        {
            using (var context = new AsDbContext())
            {
                var checkSession = context.Sessions.SingleOrDefault(s => s.Id == Id);
                if (checkSession == null)
                    return false;
                context.Dispose();
            }
            return ValidUntil.Subtract(Site.SiteClock.Now).TotalSeconds > 0;
        }

    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
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
        public override bool Equals(Object s2)
        {
            if (s2 == null) return false;
            if (s2 is Session sessionTwo)
            {
                return sessionTwo.SiteId == SiteId && Id == sessionTwo.Id;
            }
            return false;
        }


        public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
        {
            MyVerify.AuctionVerify(description, startingPrice);

            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            Site currSite;
            Session currSession;
            User currUser;

            try
            {
                currSite = c.Sites.Single(site => site.SiteId == SiteId);
                currSession = c.Sessions.Single(s => s.Id == Id);
                currUser = c.Users.Single(u => u.UserId == UserId);
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException($"{nameof(Site)} not exist", e);
            }

            var currentTime = currSite.Now();
            
            if (ValidUntil < currentTime)
                throw new AuctionSiteInvalidOperationException("Session is expired or not exist");

            if (endsOn < currentTime)
                throw new AuctionSiteUnavailableTimeMachineException("Auction is expired");

            ValidUntil = currentTime.AddSeconds(currSite.SessionExpirationInSeconds);

            currSession.ValidUntil = ValidUntil;
            c.Sessions.Update(currSession);

            var auction = new Auction()
            {
                EndsOn = endsOn,
                Description = description,
                StartingPrice = startingPrice,
                CurrPrice = startingPrice,
                Seller = currUser,
                SellerName = currUser.Username,
                Session = currSession,
                SessionId = currSession.Id,
                Site = currSite,
                SiteId = currSite.SiteId,
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
            return ValidUntil.Subtract(Site.Now()).TotalSeconds > 0;
        }

    }
}

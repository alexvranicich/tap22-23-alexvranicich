using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using AS_Vranicich.DbContext;
using AS_Vranicich.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualBasic;
using TAP22_23.AlarmClock.Interface;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Site : ISite
    {
        /*
         * Properties
         */
        public int SiteId { get; set; }

        [MinLength(DomainConstraints.MinSiteName)]
        [MaxLength(DomainConstraints.MaxSiteName)]
        public string Name { get; set; }

        [Range(DomainConstraints.MinTimeZone,DomainConstraints.MaxTimeZone)]
        public int Timezone { get; set; }

        public int SessionExpirationInSeconds { get; set; }
        public double MinimumBidIncrement { get; set; }

        public static IAlarmClock SiteClock { get; set; }

        public List<Session> Sessions { get; set; }
        public List<User> Users { get; set; }


        /*
         * Methods
         */
        public IEnumerable<IUser> ToyGetUsers()
        {
            using var c = new AsDbContext();
            try
            {
                var currSite = c.Sites.SingleOrDefault(site => site.Name == Name);
                if (currSite == null)
                {
                    throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist");
                }
                return c.Users.Where(u => u.SiteId == currSite.SiteId);

            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException(e.Message, e);
            }
        }

        public IEnumerable<ISession> ToyGetSessions()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ConnectionVerify(c);

            var currSite = c.Sites.SingleOrDefault(s => s.Name == Name);
            if (currSite == null)
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist in DB");

            IQueryable<Session> siteSession;
            List<User> siteUser;
            
            try
            {
                siteSession = c.Sessions.Where(s => s.SiteId == currSite.SiteId && s.ValidUntil > Now());
                siteUser = c.Users.Where(u => u.SiteId == currSite.SiteId).ToList();
            }
            catch (ArgumentNullException e)
            {
                throw new AuctionSiteArgumentNullException(e.Message, e);
            }

            foreach (var session in siteSession)
            {
                session.User = siteUser.First(s => s.UserId == session.UserId);
                yield return session;
            }

        }

        public IEnumerable<IAuction> ToyGetAuctions(bool onlyNotEnded)
        {
            using var c = new AsDbContext();
            MyVerify.DB_ConnectionVerify(c);

            var currSite = c.Sites.SingleOrDefault(s => s.Name == Name);
            if (currSite == null)
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)}: this site not exists");

            IQueryable<Auction> allAuctions;

            try
            {
                allAuctions = c.Auctions.Where(a => a.SiteId == currSite.SiteId);
            }
            catch (ArgumentNullException e)
            {
                throw new AuctionSiteArgumentNullException("No Auction in this Site");
            }


            if (!onlyNotEnded)
            {
                foreach (var singleAuction in allAuctions)
                {
                    yield return singleAuction;
                }
            }
            else
            {
                IQueryable<Auction> notEndAuctions;

                try
                {
                    notEndAuctions = allAuctions.Where(a => a.EndsOn > Now());
                }
                catch (ArgumentNullException e)
                {
                    throw new AuctionSiteArgumentNullException("All auction are ended", e)
                }

                foreach (var singleNotEndAuction in notEndAuctions)
                {
                    yield return singleNotEndAuction;
                }
            }
        }

        public ISession? Login(string username, string password)
        {
            MyVerify.UsernamePasswordVerify(username, password);

            using var c = new AsDbContext();
            MyVerify.DB_ConnectionVerify(c);

            var currSite = c.Sites.SingleOrDefault(s => s.Name == Name);
            if (currSite == null)
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist in DB");

            var currUser = c.Users.SingleOrDefault(u => u.Username == username && u.SiteId == currSite.SiteId);
            if (currUser == null || UtilPassword.DecodePassword(currUser.Password) != password)
                return null;

            try
            {
                DateTime dateSessionExpiration = SiteClock.Now.AddSeconds(SessionExpirationInSeconds);

                var currSession = c.Sessions.SingleOrDefault(s => s.SiteId == currSite.SiteId && s.UserId == currUser.UserId);

                if (currSession != null)
                {
                    currSession.ValidUntil = dateSessionExpiration;
                    c.SaveChanges();
                    return currSession;
                }

                var createSession = new Session()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = currUser.UserId,
                    SiteId = currSite.SiteId,
                    Site = currSite,
                    ValidUntil = dateSessionExpiration
                };

                c.Sessions.Add(createSession);
                c.SaveChanges();
                c.Dispose();
                return createSession;
            }
            catch (DbUpdateException e)
            {
                throw new AuctionSiteUnavailableDbException(e.Message, e);
            }
        }

        public void CreateUser(string username, string password)
        {
            MyVerify.UsernamePasswordVerify(username, password);
            
            using var c = new AsDbContext();
            MyVerify.DB_ConnectionVerify(c);

            try
            {
                c.Users.Add(new User()
                {
                    SiteId = SiteId,
                    Username = username,
                    Password = UtilPassword.EncodePassword(password)
                });

                c.SaveChanges();

            }
            catch (DbUpdateException e)
            {
                throw new AuctionSiteNameAlreadyInUseException(username, "This username already exist in this site");
            }

        }

        public void Delete()
        {
           using var c = new AsDbContext();
           MyVerify.DB_ConnectionVerify(c);

            try
            {
               var site = c.Sites.First(s => s.SiteId == SiteId);

               c.Sites.Remove(site);
               c.SaveChanges();

            }
            catch (ArgumentNullException e)
            {
                throw new AuctionSiteArgumentNullException("Site not found");
            }
        }

        public DateTime Now()
        {
            var currentTime = SiteClock.Now;
            return currentTime;
        }

    }

}

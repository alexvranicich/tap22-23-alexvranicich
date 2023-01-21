using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using AS_Vranicich.DbContext;
using AS_Vranicich.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        [NotMapped]
        public IAlarmClock? SiteClock { get; set; }
       

        public List<Session>? Sessions { get; set; }
        public List<User>? SiteUsers { get; set; }
        public List<Auction>? Auctions { get; set; }


        /*
         * Methods
         */
        public IEnumerable<IUser> ToyGetUsers()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            var currSite = c.Sites.SingleOrDefault(site => site.Name == Name);
            if (currSite == null)
            {
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist");
            }
            
            var allSiteUsers = c.Users.Where(u => u.SiteId == currSite.SiteId);
            if (!allSiteUsers.Any())
            {
                throw new AuctionSiteArgumentNullException("No users in this is site");
            }

            foreach (var user in allSiteUsers)
            {
                yield return user;
            }

        }

        public IEnumerable<ISession?> ToyGetSessions()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            var currSite = c.Sites.SingleOrDefault(site => site.Name == Name);
            if (currSite == null)
            {
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist");
            }

            var allSessions= c.Sessions.Where(s => s.SiteId == currSite.SiteId);

            foreach (var session in allSessions)
            {
                yield return session;
            }
        }

        public IEnumerable<IAuction> ToyGetAuctions(bool onlyNotEnded)
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            var currSite = c.Sites.First(s => s.Name == Name);
            if (currSite == null)
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)}: this site not exists");

            List<Auction> allAuctions;

            try
            {
                allAuctions = c.Auctions.Where(a => a.SiteId == currSite.SiteId).ToList();
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
                List<Auction> notEndAuctions;

                try
                {
                    notEndAuctions = allAuctions.Where(a => a.EndsOn > Now()).ToList();
                }
                catch (ArgumentNullException e)
                {
                    throw new AuctionSiteArgumentNullException("All auction are ended", e);
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
            MyVerify.DB_ContextVerify(c);

            var currSite = c.Sites.SingleOrDefault(s => s.Name == Name);
            if (currSite == null)
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist in DB");

            var currUser = c.Users.SingleOrDefault(u => u.Username == username);
            if (currUser == null || UtilPassword.DecodePassword(currUser.Password) != password)
                return null;

            try
            {
                var currSession =
                    c.Sessions.SingleOrDefault(s => s.UserId == currUser.UserId);

                if (currSession != null)
                {
                    currSession.ValidUntil = Now().AddSeconds(SessionExpirationInSeconds);
                    c.Sessions.Update(currSession);
                    c.SaveChanges();
                    return currSession;
                }

                var createSession = new Session()
                {
                    ValidUntil = Now().AddSeconds(SessionExpirationInSeconds),
                    UserId = currUser.UserId,
                    SiteId = currSite.SiteId,
                    Site = currSite,
                    User = currUser
                };

                c.Sessions.Add(createSession);
                c.SaveChanges();
                return createSession;
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteArgumentException(e.Message, e);
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
            MyVerify.DB_ContextVerify(c);

            var currSite = c.Sites.SingleOrDefault(s => s.SiteId == SiteId);
            if (currSite == null)
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist in DB");

            try
            {
                var currUser
                    = c.Users.SingleOrDefault(u => u.Username == username);
                if (currUser != null)
                    throw new AuctionSiteNameAlreadyInUseException($"{nameof(username)} of an existing site");

                c.Users.Add(new User()
                {
                    SiteId = currSite.SiteId,
                    Username = username,
                    Password = UtilPassword.EncodePassword(password)
                });

                c.SaveChanges();
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException(e.Message, e);
            }
        }

        public void Delete()
        {
           using var c = new AsDbContext();
           MyVerify.DB_ContextVerify(c);

           
           var site = c.Sites.SingleOrDefault(s => s.SiteId == SiteId);
           if (site == null)
               throw new AuctionSiteInvalidOperationException("Site not found");

           c.Sites.Remove(site);
           c.SaveChanges();
        }
          

        public DateTime Now()
        {
            return SiteClock.Now;
        }


    }

}

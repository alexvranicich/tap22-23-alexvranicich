using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public static IAlarmClock SiteClock { get; set; }
        private static IAlarm SiteAlarm { get; set; }

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

            Site currSite;
            
            try
            {
                currSite = c.Sites.Single(site => site.SiteId == SiteId);
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist", e);
            }

            var allSiteUsers = c.Users.Where(u => u.SiteUser.SiteId == currSite.SiteId);

            foreach (var user in allSiteUsers)
            {
                yield return user;
            }

        }

        public IEnumerable<ISession> ToyGetSessions()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            Site currSite;

            try
            { 
                currSite = c.Sites.Single(site => site.SiteId == SiteId);
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist", e);
            }

            var allSessions= c.Sessions.Where(s => s.Site.Name == currSite.Name && s.ValidUntil > Now());

            foreach (var session in allSessions)
            {
                yield return session;
            }
        }

        public IEnumerable<IAuction> ToyGetAuctions(bool onlyNotEnded)
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            Site currSite;

            try
            {
                currSite = c.Sites.Single(site => site.SiteId == SiteId);
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException($"{nameof(Name)} not exist", e);
            }

            var allAuctions = c.Auctions.Where(a => a.Site.Name == currSite.Name);

            if (!onlyNotEnded)
            {
                foreach (var singleAuction in allAuctions)
                {
                    yield return singleAuction;
                }
            }
            else
            {
                var notEndAuctions = allAuctions.Where(a => a.EndsOn > Now()).ToList();

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

            var currSite = c.Sites.SingleOrDefault(s => s.SiteId == SiteId);
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
                    SiteUser = currSite,
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
            using (var c = new AsDbContext())
            {
                MyVerify.DB_ContextVerify(c);

                try
                {
                    var site = c.Sites.Single(s => s.Name == Name);

                    c.Sites.Remove(site);
                    c.SaveChanges();
                    c.Dispose();
                }
                catch (InvalidOperationException e)
                {
                    throw new AuctionSiteInvalidOperationException("Can't remove this", e);
                }
                catch (ArgumentNullException e)
                {
                    throw new AuctionSiteArgumentNullException("No site to deleted", e);
                }
            }
        }

        public DateTime Now()
        {
            try
            {
                return SiteClock.Now;
            }
            catch (ArgumentNullException e)
            {
                throw new AuctionSiteUnavailableTimeMachineException("Problem with now time", e);
            }
        }

        public void SetSessionCleanerAlarm(IAlarmClock alarmClock)
        {
            SiteClock = alarmClock;
            SiteAlarm = alarmClock.InstantiateAlarm(5*60*100);
            SiteAlarm.RingingEvent += CleanupSessions;
        }

        public void CleanupSessions()
        {
            using (var context = new AsDbContext())
            {
                var curSite = context.Sites.SingleOrDefault(s => s.SiteId == SiteId);
                if (curSite == null)
                    throw new InvalidOperationException();

                var siteSessions = context.Sessions.Where(s => s.Site.SiteId == SiteId);
                foreach (var session in siteSessions)
                {
                    if (!session.IsValid())
                    {
                        context.Sessions.Remove(session);
                    }
                }
                context.SaveChanges();
            }
        }

    }

}

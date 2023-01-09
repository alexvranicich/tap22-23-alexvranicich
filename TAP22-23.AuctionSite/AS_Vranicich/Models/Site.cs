using System.ComponentModel.DataAnnotations;
using AS_Vranicich.DbContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TAP22_23.AlarmClock.Interface;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Site : ISite
    {
        public int SiteId { get; set; }

        [MinLength(DomainConstraints.MinSiteName)]
        [MaxLength(DomainConstraints.MaxSiteName)]
        public string Name { get; }

        [Range(DomainConstraints.MinTimeZone,DomainConstraints.MaxTimeZone)]
        public int Timezone { get; }

        public int SessionExpirationInSeconds { get; }
        public double MinimumBidIncrement { get; }

        public static IAlarmClock SiteClock { get; set; }

        public Site(string name, int timezone, int sessionExpirationInSeconds, double minimumBidIncrement)
        {
            Name = name;
            Timezone = timezone;
            SessionExpirationInSeconds = sessionExpirationInSeconds;
            MinimumBidIncrement = minimumBidIncrement;
        }

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
            throw new NotImplementedException();
        }

        public IEnumerable<IAuction> ToyGetAuctions(bool onlyNotEnded)
        {
            throw new NotImplementedException();
        }

        public ISession? Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public void CreateUser(string username, string password)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public DateTime Now()
        {
            throw new NotImplementedException();
        }

    }

}

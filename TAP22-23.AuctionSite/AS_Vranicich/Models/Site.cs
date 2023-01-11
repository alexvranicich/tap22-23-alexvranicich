using System.ComponentModel.DataAnnotations;
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
            throw new NotImplementedException();
        }

    }

}

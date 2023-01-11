using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
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
        public Site? SiteUser { get; set; }

        public Session? SessionUser { get; set; }
        public List<Auction>? AuctionsUser { get; set; }

        /*
         * Methods
         */

        public IEnumerable<IAuction> WonAuctions()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

    }
}

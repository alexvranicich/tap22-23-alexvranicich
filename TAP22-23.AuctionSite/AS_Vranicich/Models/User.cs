using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User : IUser
    {
        public int UserId { get; set; }

        [MaxLength(DomainConstraints.MaxUserName)]
        [MinLength(DomainConstraints.MinUserName)]
        public string Username { get; }

        [MinLength(DomainConstraints.MinUserPassword)]
        public string Password { get; }

        public int SiteId { get; set; }
        public Site Site { get; set; }

        
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

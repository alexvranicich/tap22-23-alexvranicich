using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.AS_Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User : IUser
    {
        [MaxLength(DomainConstraints.MaxUserName)]
        [MinLength(DomainConstraints.MinUserName)]
        public string Username { get; }

        [MinLength(DomainConstraints.MinUserPassword)]
        public string Password { get; }

        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

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

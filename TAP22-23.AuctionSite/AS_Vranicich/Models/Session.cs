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
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

    }
}

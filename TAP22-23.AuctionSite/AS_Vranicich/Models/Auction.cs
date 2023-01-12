using System.ComponentModel.DataAnnotations.Schema;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    public class Auction : IAuction
    {
        /*
         * Properties
         */

        public int Id { get; set; }
        
        public IUser Seller { get; set;  }

        public string Description { get; set; }
        public DateTime EndsOn { get; set; }

        public int SiteId { get; set; }
        public Site Site { get; set; }

        public User UserAuction { get; set; }
        public int UserId { get; set; }
        
        /*
         * Methods
         */
        
        public IUser? CurrentWinner()
        {
            throw new NotImplementedException();
        }

        public double CurrentPrice()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public bool Bid(ISession session, double offer)
        {
            throw new NotImplementedException();
        }
    }
}


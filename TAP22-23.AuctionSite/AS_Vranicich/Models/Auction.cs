using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    public class Auction : IAuction
    {

        public int Id { get; }
        public IUser Seller { get; }

        public string Description { get; }
        public DateTime EndsOn { get; }

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


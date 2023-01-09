using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    public class Session : ISession
    {
        public string Id { get; }
        public DateTime ValidUntil { get; }
        public IUser User { get; }
        
        
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

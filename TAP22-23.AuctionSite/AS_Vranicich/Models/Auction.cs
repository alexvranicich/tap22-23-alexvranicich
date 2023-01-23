using System;
using System.ComponentModel.DataAnnotations.Schema;
using AS_Vranicich.DbContext;
using AS_Vranicich.Utilities;
using TAP22_23.AlarmClock.Interface;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich.Models
{
    public class Auction : IAuction
    {
        /*
         * Properties
         */

        public int Id { get; set; }
        [NotMapped]
        public IUser Seller { get; set; }
        public string SellerName { get; set; }
        public string Description { get; set; }
        public DateTime EndsOn { get; set; }

        public Site Site { get; set; }
        public int SiteId { get; set; }
        public Session Session { get; set; }
        public string SessionId { get; set; }

        public User? WinningUser { get; set; }
        public string? WinningUsername { get; set; }
        public double MaximumOffer { get; set; } = 0.0;
        public double CurrPrice { get; set; } = 0.0;
        public double StartingPrice { get; set; }

        /*
         * Methods
         */

        public override bool Equals(Object a2)
        {
            if (a2 == null) return false;
            if (a2 is Auction auctionTwo)
            {
                return SiteId == auctionTwo.SiteId && Id == auctionTwo.Id;
            }
            return false;
        }

        public IUser? CurrentWinner()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            var currAuction = c.Auctions.Single(a => a.Id == Id && a.SiteId == SiteId);     
            return c.Users.SingleOrDefault(u => u.Username == currAuction.WinningUsername);
        }

        public double CurrentPrice()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            try
            {
                var currentAuction = c.Auctions.SingleOrDefault(a => a.Id == Id);
                if (currentAuction == null)
                    throw new AuctionSiteArgumentOutOfRangeException($"{nameof(Id)} auction expired");

                return currentAuction.CurrPrice;
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException(e.Message);
            }
        }

        public void Delete()
        {
            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            try
            {
                var currAuction = c.Auctions.SingleOrDefault(a => a.Id == Id);
                if (currAuction == null)
                    throw new AuctionSiteInvalidOperationException($"{nameof(currAuction)} is null");

                c.Auctions.Remove(currAuction);
                c.SaveChanges();
                c.Dispose();
            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException(e.Message);
            }
        }
        
        public bool Bid(ISession session, double offer)
        {
            MyVerify.BidSessionVerify(session, offer); // Only verify; bid positive and session non null //

            using var c = new AsDbContext();
            MyVerify.DB_ContextVerify(c);

            Site currSite;
            DateTime currTimeClock;

            try
            {
                currSite = c.Sites.Single(s => s.SiteId == SiteId);
                currTimeClock = currSite.Now();

            }
            catch (InvalidOperationException e)
            {
                throw new AuctionSiteInvalidOperationException("Site not exist", e);
            }

            var currAuction = c.Auctions.SingleOrDefault(a => a.SiteId == currSite.SiteId && a.Id == Id);
            if (currAuction == null)
                throw new AuctionSiteInvalidOperationException($"Auction: {nameof(Id)} is not exist");
            if (currAuction.EndsOn < currTimeClock)
                throw new AuctionSiteInvalidOperationException($"{nameof(EndsOn)} auction expired");

            var currSession = c.Sessions.SingleOrDefault(s => s.Id == session.Id);
            if (currSession == null || currSession.ValidUntil < currTimeClock)
                throw new AuctionSiteArgumentException($"{nameof(Id)} isn't a valid session");

            var currUser = c.Users.SingleOrDefault(u => u.SessionUser.Id == session.Id);
            if (currUser == null)
                throw new AuctionSiteInvalidOperationException($"{nameof(currUser)} is not exist in this session");
            if (currUser.Username == Seller.Username)
                throw new AuctionSiteArgumentException($"The logged user is also the Seller of this auction");

            var seller = c.Users.SingleOrDefault(u => u.Username == Seller.Username);
            if (seller == null)
                throw new AuctionSiteInvalidOperationException($"{nameof(seller)} is not exist");
            if (currUser.SiteId != seller.SiteId)
                throw new AuctionSiteArgumentException(
                    $"Logged user is a user of a site different from the site of the Seller");

            double minBid = currSite.MinimumBidIncrement;

            /*
             * List of not valid bid:
             * 1) Bidder = current winner && offer is lower than the maximum offer increased by minimumBidIncrement
             * 2) Bidder != current winner && offer is lower than the current price
             * 3) Bidder != current winner && offer is lower than the current price increased by minimumBid Increment
             *                              && This is not the first bid
             */

            if (currUser.Username == currAuction.WinningUsername &&
                offer < currAuction.MaximumOffer + currSite.MinimumBidIncrement)
                return false;

            if (currUser.Username != currAuction.WinningUsername && offer < currAuction.CurrPrice)
                return false;

            if (currUser.Username != currAuction.WinningUsername &&
                offer < currAuction.CurrPrice + currSite.MinimumBidIncrement && currAuction.WinningUsername != null)
                return false;

            /*
             * IF this is the first bid,
             * THEN the maximum offer is set to offer,
             * the current price is not changed
             * and the bidder becomes the current winner
             */

            if (currAuction.MaximumOffer == 0)
            {
                currAuction.MaximumOffer = offer;
                currAuction.WinningUser = currUser;
                currAuction.WinningUsername = currUser.Username;
            }

            /*
             * IF the bidder was already winning this auction
             * THEN maximum offer is set to offer and
             * current price and current winner are unchanged
             */

            else if (currAuction.WinningUsername == currUser.Username)
            {
                currAuction.MaximumOffer = offer;
            }

            /*
             * IF this is NOT the first bid, the bidder is NOT the current winner,
             * and offer is higher than the maximum offer.
             * THEN the current price is set to the minimum between offer and (CMO + minimumBidIncrement),
             * the maximum offer is set to offer 
             * and the bidder becomes the current winner
             */

            else if (currAuction.MaximumOffer > 0 && currUser.Username != currAuction.WinningUsername && offer > currAuction.MaximumOffer)
            {
                if (currAuction.MaximumOffer + minBid > offer)
                    currAuction.CurrPrice = offer;
                else
                    currAuction.CurrPrice = currAuction.MaximumOffer + minBid;

                currAuction.MaximumOffer = offer;
                currAuction.WinningUser = currUser;
                currAuction.WinningUsername = currUser.Username;
            }

            /*
             * IF this is NOT the first bid, the bidder is NOT the current winner,
             * and offer is NOT higher than the current maximum offer.
             * THEN the current price is set to the minimum between CMO and (offer + minimumBidIncrement)
             * and the current winner does not change
             */

            else if (currAuction.MaximumOffer > 0 && currUser.Username != currAuction.WinningUsername && offer < currAuction.MaximumOffer)
            {
                if (currAuction.MaximumOffer > offer + minBid)
                    currAuction.CurrPrice = offer + minBid;
                else
                    currAuction.CurrPrice = currAuction.MaximumOffer;
            }

            currSession.ValidUntil = currTimeClock.AddSeconds(currSite.SessionExpirationInSeconds);
            c.Sessions.Update(currSession);
            c.Auctions.Update(currAuction);
            c.SaveChanges();
            return true;
        }
    }
}


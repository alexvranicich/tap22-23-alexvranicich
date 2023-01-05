using AS_Vranicich.AS_Models;
using Microsoft.EntityFrameworkCore;
using TAP22_23.AuctionSite.Interface;

namespace AS_Vranicich
{
    public class AS_DbContext : TapDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }

    }
}
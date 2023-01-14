using AS_Vranicich.Models;
using Microsoft.EntityFrameworkCore;
using TAP22_23.AuctionSite.Interface;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;

namespace AS_Vranicich.DbContext
{
    public class AsDbContext : TapDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Auction> Auctions { get; set; } 
        
        private static string? ConnectionString { get; set; }

        public AsDbContext(string connectionString) : base(new DbContextOptionsBuilder<AsDbContext>()
            .UseSqlServer(connectionString).Options)
        {
            ConnectionString = connectionString;
        }

        public AsDbContext() : base(new DbContextOptionsBuilder<AsDbContext>()
            .UseSqlServer(ConnectionString).Options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var user = modelBuilder.Entity<User>();
            user.HasOne(user => user.SiteUser).WithMany(s => s.Users).OnDelete(DeleteBehavior.ClientCascade);
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (SqlException e)
            {
                throw new AuctionSiteUnavailableDbException("The Database in unavailable", e);
            }
            catch (DbUpdateException e)
            {
                var sqlException = e.InnerException as SqlException;

                if (sqlException == null)
                    throw new AuctionSiteArgumentNullException($"{sqlException} is null", e);

                switch (sqlException.ErrorCode)
                {
                    case 2601:
                        throw new AuctionSiteUnavailableDbException("Cannot insert duplicate key row in object", e);

                    case 2627:
                        throw new AuctionSiteNameAlreadyInUseException("This name is already in use");

                    default: throw new AuctionSiteUnavailableDbException(e.Message, e);
                }
            }
        }

    }
}
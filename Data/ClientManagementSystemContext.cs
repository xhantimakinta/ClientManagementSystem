using ClientManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementSystem.Data
{
    public class ClientManagementSystemContext : DbContext
    {
        public ClientManagementSystemContext(DbContextOptions<ClientManagementSystemContext> options)
            : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ClientContact> ClientContacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================
            // CLIENT
            // =============================
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.ClientCode)
                .IsUnique();   // Ensure ClientCode is unique

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Name); // Index for performance

            // =============================
            // CONTACT
            // =============================
            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique();   // Ensure unique emails

            modelBuilder.Entity<Contact>()
                .HasIndex(c => new { c.Surname, c.Name }); // Composite index for ordering

                        // =============================
            // CLIENTCONTACT (Join Table)
            // =============================
            // Use an identity PK (ClientContactId) and enforce uniqueness on (ClientId, ContactId)
            modelBuilder.Entity<ClientContact>()
                .HasKey(cc => cc.ClientContactId);

            modelBuilder.Entity<ClientContact>()
                .HasIndex(cc => new { cc.ClientId, cc.ContactId })
                .IsUnique();

            modelBuilder.Entity<ClientContact>()
                .HasOne(cc => cc.Client)
                .WithMany(c => c.ClientContacts)
                .HasForeignKey(cc => cc.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClientContact>()
                .HasOne(cc => cc.Contact)
                .WithMany(c => c.ClientContacts)
                .HasForeignKey(cc => cc.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
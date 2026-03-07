using Empath_AI.Migrations;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Empath_AI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
                              
        public DbSet<User> Users { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Devices> Devices { get; set; }
        public DbSet<HeartRateRecord> Hearts { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Medical_Report> Medical_Reports { get; set; }
        public DbSet<Accelerometer> Accelerometer { get; set; }
        public DbSet<GSRRecord> GSRRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversation → User
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.user)
                .WithMany()
                .HasForeignKey(c => c.User_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // Devices → User (User deleted → Devices deleted)
            modelBuilder.Entity<Devices>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Medical_Report → User
            modelBuilder.Entity<Medical_Report>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // HeartRateRecord → User (NoAction - cascades through Device instead)
            modelBuilder.Entity<HeartRateRecord>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // GSRRecord → User (NoAction - cascades through Device instead)
            modelBuilder.Entity<GSRRecord>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Accelerometer → User (NoAction - cascades through Device instead)
            modelBuilder.Entity<Accelerometer>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // GSRRecord → Device (Device deleted → GSR records deleted)
            modelBuilder.Entity<GSRRecord>()
                .HasOne(g => g.Device)
                .WithMany()
                .HasForeignKey(g => g.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Accelerometer → Device (Device deleted → Accelerometer records deleted)
            modelBuilder.Entity<Accelerometer>()
                .HasOne(a => a.Device)
                .WithMany()
                .HasForeignKey(a => a.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // HeartRateRecord → Device (Device deleted → HeartRate records deleted)
            modelBuilder.Entity<HeartRateRecord>()
                .HasOne(h => h.Device)
                .WithMany()
                .HasForeignKey(h => h.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);


            ////Message -> Conversation
            //modelBuilder.Entity<Message>()
            //    .HasOne(m => m.conversation)
            //    .WithMany(c => c.messages)
            //    .HasForeignKey(m => m.Conversation_ID)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

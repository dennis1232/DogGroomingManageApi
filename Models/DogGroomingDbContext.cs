using Microsoft.EntityFrameworkCore;

namespace DogGroomingAPI.Models
{
    public partial class DogGroomingDbContext : DbContext
    {
        public DogGroomingDbContext(DbContextOptions<DogGroomingDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Appointments_Id");

                entity.Property(e => e.AppointmentTime).HasColumnType("datetime");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                // PetSize is now a simple string
                entity.Property(e => e.PetSize)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.HasOne(d => d.Customer).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Appointments_Customers");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Customers_Id");

                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Username).HasMaxLength(50);
            });
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

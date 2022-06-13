using Domain;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    public DbSet<House> Houses { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Room> Rooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<House>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Room>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Address>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<House>()
            .HasOne(x => x.Address)
            .WithMany()
            .IsRequired(false);

        modelBuilder.Entity<House>()
            .HasMany(x => x.Rooms);

        modelBuilder.Entity<Address>().HasData(Seeding.Addresses.GetSeeded());
        modelBuilder.Entity<House>().HasData(Seeding.Houses.GetSeededHousesWithoutNavigations());
        modelBuilder.Entity<Room>().HasData(Seeding.Houses.GetSeededRoomsFromHouses());
    }
}
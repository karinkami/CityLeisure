using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<EventCategory> EventCategories { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<FavoriteEvent> FavoriteEvents { get; set; }
    public DbSet<BookedSeat> BookedSeats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserName).HasColumnName("user_name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(32).HasDefaultValue("User");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.ToTable("event_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.ToTable("venues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.MapUrl).HasColumnName("map_url");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.VenueId).HasColumnName("venue_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.EventDate).HasColumnName("event_date");
            entity.Property(e => e.EventTime).HasColumnName("event_time");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
            entity.Property(e => e.AvailableTickets).HasColumnName("available_tickets");
            entity.Property(e => e.AgeRating).HasColumnName("age_rating");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.SeatingType).HasColumnName("seating_type").HasMaxLength(32).HasDefaultValue("general");
            entity.Property(e => e.SeatLayoutJson).HasColumnName("seat_layout_json");

            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId);

            entity.HasOne(e => e.Venue)
                  .WithMany()
                  .HasForeignKey(e => e.VenueId);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SelectedSeatsJson).HasColumnName("selected_seats_json");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Event)
                  .WithMany()
                  .HasForeignKey(e => e.EventId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(10,2)");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SeatLabelsJson).HasColumnName("seat_labels_json");

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Event)
                  .WithMany()
                  .HasForeignKey(e => e.EventId);
        });

        modelBuilder.Entity<BookedSeat>(entity =>
        {
            entity.ToTable("booked_seats");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.SeatLabel).HasColumnName("seat_label").HasMaxLength(64);
            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity.HasIndex(e => new { e.EventId, e.SeatLabel }).IsUnique();

            entity.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FavoriteEvent>(entity =>
        {
            entity.ToTable("favorite_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Event)
                  .WithMany()
                  .HasForeignKey(e => e.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}



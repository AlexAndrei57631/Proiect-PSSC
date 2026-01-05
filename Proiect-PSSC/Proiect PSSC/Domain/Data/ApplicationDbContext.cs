using Microsoft.EntityFrameworkCore;
using Domain.Models.Entities;
using Domain.Models.ValueObjects;

namespace Proiect_PSSC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for persisted entity states
        public DbSet<DeliveredOrder> DeliveredOrders { get; set; }
        public DbSet<SentInvoice> SentInvoices { get; set; }
        public DbSet<DeliveredShipment> DeliveredShipments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Order entity
            modelBuilder.Entity<DeliveredOrder>(entity =>
            {
                entity.ToTable("DeliveredOrders");
                
                // Use a generated ID as primary key
                entity.Property<int>("Id");
                entity.HasKey("Id");
                
                // Configure the primary constructor for EF Core
                entity.Metadata.SetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
                
                entity.Property(e => e.CustomerId)
                    .HasConversion(
                        v => v.Value,
                        v => CustomerId.Parse(v))
                    .HasColumnName("CustomerId");
                
                entity.Property(e => e.ReservationId).HasColumnName("ReservationId");
                entity.Property(e => e.ReservedAt).HasColumnName("ReservedAt");
                entity.Property(e => e.PreparedAt).HasColumnName("PreparedAt");
                entity.Property(e => e.WarehouseLocation).HasColumnName("WarehouseLocation");
                entity.Property(e => e.DeliveredAt).HasColumnName("DeliveredAt");
                entity.Property(e => e.DeliverySignature).HasColumnName("DeliverySignature");
                
                // Configure owned Money type for TotalAmount with precision
                entity.OwnsOne(e => e.TotalAmount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("TotalAmount")
                        .HasPrecision(18, 2)
                        .IsRequired();
                    money.Property(m => m.Currency).HasColumnName("Currency").IsRequired();
                });
                
                // Configure owned Address type
                entity.OwnsOne(e => e.DeliveryAddress, address =>
                {
                    address.Property(a => a.Street).HasColumnName("Street").IsRequired();
                    address.Property(a => a.City).HasColumnName("City").IsRequired();
                    address.Property(a => a.PostalCode).HasColumnName("PostalCode").IsRequired();
                    address.Property(a => a.Country).HasColumnName("Country").IsRequired();
                });
                
                // Configure owned collection of OrderItems
                entity.OwnsMany(e => e.Items, item =>
                {
                    item.ToTable("OrderItems");
                    item.WithOwner().HasForeignKey("DeliveredOrderId");
                    item.Property<int>("Id");
                    item.HasKey("Id");
                    
                    item.Property(i => i.ProductId)
                        .HasConversion(
                            v => v.Value,
                            v => ProductId.Parse(v))
                        .HasColumnName("ProductId")
                        .IsRequired();
                    
                    item.Property(i => i.Quantity).HasColumnName("Quantity").IsRequired();
                    
                    item.OwnsOne(i => i.UnitPrice, money =>
                    {
                        money.Property(m => m.Amount)
                            .HasColumnName("UnitPriceAmount")
                            .HasPrecision(18, 2)
                            .IsRequired();
                        money.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").IsRequired();
                    });
                });
            });

            // Configure Invoice entity
            modelBuilder.Entity<SentInvoice>(entity =>
            {
                entity.ToTable("SentInvoices");
                
                entity.Metadata.SetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
                
                entity.Property(e => e.InvoiceId)
                    .HasConversion(
                        v => v.Value,
                        v => InvoiceId.Parse(v))
                    .HasColumnName("InvoiceId");
                
                entity.HasKey(e => e.InvoiceId);
                
                entity.Property(e => e.OrderId)
                    .HasConversion(
                        v => v.Value,
                        v => OrderId.Parse(v))
                    .HasColumnName("OrderId");
                
                entity.Property(e => e.CustomerId)
                    .HasConversion(
                        v => v.Value,
                        v => CustomerId.Parse(v))
                    .HasColumnName("CustomerId");
                
                entity.Property(e => e.InvoiceNumber).HasColumnName("InvoiceNumber");
                entity.Property(e => e.GeneratedAt).HasColumnName("GeneratedAt");
                entity.Property(e => e.SentAt).HasColumnName("SentAt");
                entity.Property(e => e.SentTo).HasColumnName("SentTo");
                entity.Property(e => e.DeliveryMethod).HasColumnName("DeliveryMethod");
                
                // Configure owned Money type for TotalAmount with precision
                entity.OwnsOne(e => e.TotalAmount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("TotalAmount")
                        .HasPrecision(18, 2)
                        .IsRequired();
                    money.Property(m => m.Currency).HasColumnName("Currency").IsRequired();
                });
                
                // Configure owned Address type
                entity.OwnsOne(e => e.BillingAddress, address =>
                {
                    address.Property(a => a.Street).HasColumnName("Street").IsRequired();
                    address.Property(a => a.City).HasColumnName("City").IsRequired();
                    address.Property(a => a.PostalCode).HasColumnName("PostalCode").IsRequired();
                    address.Property(a => a.Country).HasColumnName("Country").IsRequired();
                });
                
                // Configure owned collection of InvoiceItems
                entity.OwnsMany(e => e.Items, item =>
                {
                    item.ToTable("InvoiceItems");
                    item.WithOwner().HasForeignKey("SentInvoiceId");
                    item.Property<int>("Id");
                    item.HasKey("Id");
                    
                    item.Property(i => i.ProductId)
                        .HasConversion(
                            v => v.Value,
                            v => ProductId.Parse(v))
                        .HasColumnName("ProductId")
                        .IsRequired();
                    
                    item.Property(i => i.Quantity).HasColumnName("Quantity").IsRequired();
                    
                    item.OwnsOne(i => i.UnitPrice, money =>
                    {
                        money.Property(m => m.Amount)
                            .HasColumnName("UnitPriceAmount")
                            .HasPrecision(18, 2)
                            .IsRequired();
                        money.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").IsRequired();
                    });
                    
                    item.OwnsOne(i => i.LineTotal, money =>
                    {
                        money.Property(m => m.Amount)
                            .HasColumnName("LineTotalAmount")
                            .HasPrecision(18, 2)
                            .IsRequired();
                        money.Property(m => m.Currency).HasColumnName("LineTotalCurrency").IsRequired();
                    });
                });
            });

            // Configure Shipment entity
            modelBuilder.Entity<DeliveredShipment>(entity =>
            {
                entity.ToTable("DeliveredShipments");
                
                entity.Metadata.SetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
                
                // Use a generated ID as primary key
                entity.Property<int>("Id");
                entity.HasKey("Id");
                
                entity.Property(e => e.OrderId)
                    .HasConversion(
                        v => v.Value,
                        v => OrderId.Parse(v))
                    .HasColumnName("OrderId");
                
                entity.Property(e => e.CustomerId)
                    .HasConversion(
                        v => v.Value,
                        v => CustomerId.Parse(v))
                    .HasColumnName("CustomerId");
                
                entity.Property(e => e.TrackingNumber).HasColumnName("TrackingNumber");
                entity.Property(e => e.PreparedAt).HasColumnName("PreparedAt");
                entity.Property(e => e.Carrier).HasColumnName("Carrier");
                entity.Property(e => e.DeliveredAt).HasColumnName("DeliveredAt");
                entity.Property(e => e.RecipientName).HasColumnName("RecipientName");
                entity.Property(e => e.DeliverySignature).HasColumnName("DeliverySignature");
                
                // Configure owned Address type
                entity.OwnsOne(e => e.DeliveryAddress, address =>
                {
                    address.Property(a => a.Street).HasColumnName("Street").IsRequired();
                    address.Property(a => a.City).HasColumnName("City").IsRequired();
                    address.Property(a => a.PostalCode).HasColumnName("PostalCode").IsRequired();
                    address.Property(a => a.Country).HasColumnName("Country").IsRequired();
                });
                
                // Configure owned collection of ShipmentItems
                entity.OwnsMany(e => e.Items, item =>
                {
                    item.ToTable("ShipmentItems");
                    item.WithOwner().HasForeignKey("DeliveredShipmentId");
                    item.Property<int>("Id");
                    item.HasKey("Id");
                    
                    item.Property(i => i.ProductId)
                        .HasConversion(
                            v => v.Value,
                            v => ProductId.Parse(v))
                        .HasColumnName("ProductId")
                        .IsRequired();
                    
                    item.Property(i => i.Quantity).HasColumnName("Quantity").IsRequired();
                });
            });
        }
    }
}
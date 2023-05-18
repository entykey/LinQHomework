﻿using LinQHomework.Models;
using Microsoft.EntityFrameworkCore;

namespace LinQHomework.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.; Database=LinQHomework; Trusted_Connection=True; MultipleActiveResultSets=True; TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Configure entities, relationships, etc.

            //modelBuilder.Entity<Customer>()
            //    .HasMany(c => c.Orders)
            //    .WithOne(o => o.Customer)
            //    .HasForeignKey(o => o.CustomerId);

            //modelBuilder.Entity<Product>()
            //    .HasMany(p => p.OrderItems)
            //    .WithOne(oi => oi.Product)
            //    .HasForeignKey(oi => oi.ProductId);

            //modelBuilder.Entity<Order>()
            //    .HasMany(o => o.OrderItems)
            //    .WithOne(oi => oi.Order)
            //    .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<Order>()
            .HasOne<Customer>(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.ProductId });

            modelBuilder.Entity<OrderItem>()
                .HasOne<Order>(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne<Product>(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region seed data
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Name = "John Smith", Email = "john.smith@example.com" },
                new Customer { CustomerId = 2, Name = "Jane Doe", Email = "jane.doe@example.com" },
                new Customer { CustomerId = 3, Name = "Bob Johnson", Email = "bob.johnson@example.com" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Product 1", Price = 9.99m },
                new Product { ProductId = 2, Name = "Product 2", Price = 14.99m },
                new Product { ProductId = 3, Name = "Product 3", Price = 19.99m }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order { OrderId = 1, OrderDate = new DateTime(2022, 01, 01), CustomerId = 1 },
                new Order { OrderId = 2, OrderDate = new DateTime(2022, 02, 01), CustomerId = 2 },
                new Order { OrderId = 3, OrderDate = new DateTime(2022, 03, 01), CustomerId = 3 }
            );

            modelBuilder.Entity<OrderItem>().HasData(
                new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 2 },
                new OrderItem { OrderItemId = 2, OrderId = 1, ProductId = 2, Quantity = 1 },
                new OrderItem { OrderItemId = 3, OrderId = 2, ProductId = 1, Quantity = 3 },
                new OrderItem { OrderItemId = 4, OrderId = 2, ProductId = 3, Quantity = 1 },
                new OrderItem { OrderItemId = 5, OrderId = 3, ProductId = 2, Quantity = 2 },
                new OrderItem { OrderItemId = 6, OrderId = 3, ProductId = 3, Quantity = 2 }
            );
            #endregion

            // always have this line at bottom:
            base.OnModelCreating(modelBuilder);
        }
    }
}

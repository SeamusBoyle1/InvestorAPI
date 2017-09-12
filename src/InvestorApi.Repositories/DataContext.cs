﻿using InvestorApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestorApi.Repositories
{
    public sealed class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var userEntity = modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Id).IsRequired();
                entity.Property(user => user.DisplayName).IsRequired();
                entity.Property(user => user.Email).IsRequired();
                entity.Property(user => user.HashedPassword).IsRequired();
                entity.Property(user => user.Level).IsRequired();
            });
        }
    }
}
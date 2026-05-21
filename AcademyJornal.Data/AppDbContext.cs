using AcademyJournal.Core.Interfaces;
using AcademyJournal.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyJornal.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public DbSet<Student> Student  { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Review> Review { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=AcademyJournal;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Review>(entity =>
            {
                entity.Property(r => r.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(r => r.IsEdited)
                      .HasDefaultValue(false);
            });
        }
    }
}

using AcademyJournal.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyJournal.Core.Interfaces
{
    public interface IAppDbContext // для реализации связи между Проектом "Data" и "Core"
    {
        DbSet<Student> Student { get; set; }
        DbSet<Module> Modules { get; set; }
        DbSet<Grade> Grades { get; set; }
        DbSet<Review> Review { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}

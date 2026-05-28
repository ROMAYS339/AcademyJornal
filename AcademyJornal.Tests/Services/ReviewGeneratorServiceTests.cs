using AcademyJournal.Core.Interfaces;
using AcademyJournal.Core.Models;
using AcademyJournal.Core.Models.Enums;
using AcademyJournal.Core.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AcademyJornal.Tests.Services
{
    public class ReviewGeneratorServiceTests
    {
        private readonly Mock<IAppDbContext> _mockContext;
        private readonly ReviewGeneratorService _service;

        public ReviewGeneratorServiceTests()
        {
            _mockContext = new Mock<IAppDbContext>();
            _service = new ReviewGeneratorService(_mockContext.Object);
        }

        [Fact]
        public void GenerateReview_StudentWithGoodGrades_ReturnsValidReview()
        {
            // Arrange
            var student = new Student { Id = 1, FullName = "Иван Петров" };
            var module = new Module { Id = 1, Name = "Программирование", Description = "разработка" };
            var grades = new List<Grade>
            {
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.lesson, Value = 10 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.exam, Value = 11 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.homework, Value = 9 }
            };
            _mockContext.Setup(c => c.Grades).Returns(MockDbSet(grades));
            _mockContext.Setup(c => c.Student).Returns(MockDbSet(new List<Student> { student }));
            _mockContext.Setup(c => c.Modules).Returns(MockDbSet(new List<Module> { module }));

            // Act
            var review = _service.GenerateReview(1, 1);

            // Assert
            Assert.NotNull(review);
            Assert.Contains("Петров", review.Text);
            Assert.Equal(student, review.Student);
        }

        [Fact]
        public void GenerateReview_NoGrades_ReturnsNull()
        {
            // Arrange
            _mockContext.Setup(c => c.Grades).Returns(MockDbSet(new List<Grade>()));
            _mockContext.Setup(c => c.Student).Returns(MockDbSet(new List<Student>()));
            _mockContext.Setup(c => c.Modules).Returns(MockDbSet(new List<Module>()));

            // Act
            var review = _service.GenerateReview(1, 1);

            // Assert
            Assert.Null(review);
        }

        private static DbSet<T> MockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return dbSet.Object;
        }
    }
}
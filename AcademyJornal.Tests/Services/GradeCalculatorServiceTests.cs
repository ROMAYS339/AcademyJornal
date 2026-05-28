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
    public class GradeCalculatorServiceTests
    {
        private readonly Mock<IAppDbContext> _mockContext;
        private readonly GradeCalculatorService _service;

        public GradeCalculatorServiceTests()
        {
            _mockContext = new Mock<IAppDbContext>();
            _service = new GradeCalculatorService(_mockContext.Object);
        }

        [Fact]
        public void CalculateModuleAverage_NoGrades_ReturnsZero()
        {
            // Arrange
            _mockContext.Setup(c => c.Grades).Returns(MockDbSet(new List<Grade>()));

            // Act
            var result = _service.CalculateModuleAverage(1, 1);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateModuleAverage_ExamBad_ActiveStudent_ResultLimited()
        {
            // Arrange
            var grades = new List<Grade>
            {
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.lesson, Value = 10 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.lesson, Value = 9 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.exam, Value = 3 }
            };
            _mockContext.Setup(c => c.Grades).Returns(MockDbSet(grades));

            // Act
            var result = _service.CalculateModuleAverage(1, 1);

            // Assert
            Assert.InRange(result, 0m, 8m);
        }

        [Fact]
        public void CalculateModuleAverage_HomeworkBelow70Percent_ReturnsZero()
        {
            // Arrange
            var grades = new List<Grade>
            {
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.lesson, Value = 10 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.homework, Value = 8 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.homework, Value = 0 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.homework, Value = 0 }
            };
            _mockContext.Setup(c => c.Grades).Returns(MockDbSet(grades));

            // Act
            var result = _service.CalculateModuleAverage(1, 1);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateModuleAverage_AllPerfect_ReturnsHighScore()
        {
            // Arrange
            var grades = new List<Grade>
            {
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.lesson, Value = 12 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.exam, Value = 12 },
                new() { StudentId = 1, ModuleId = 1, Type = GradeType.homework, Value = 10 }
            };
            _mockContext.Setup(c => c.Grades).Returns(MockDbSet(grades));

            // Act
            var result = _service.CalculateModuleAverage(1, 1);

            // Assert
            Assert.True(result >= 8m);
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
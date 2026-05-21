using AcademyJournal.Core.Interfaces;
using AcademyJournal.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyJournal.Core.Services
{
    public class GradeCalculatorService
    {
        private readonly IAppDbContext _context;

        public GradeCalculatorService(IAppDbContext context)
        {
            _context = context;
        }

        public decimal CalculateModuleAverage(int studentId, int moduleId) // здесь надо уточнить, что значит высокая, низкая оценка?
        {
            // подготовка к рассчитанию средней оценки
            var studentsGradesModule = _context.Grades.Where(g => g.StudentId == studentId && g.ModuleId == moduleId).ToList();

            if (!studentsGradesModule.Any()) return 0; // если нет оценок за модуль

            var totalLessons = studentsGradesModule.Where(g => g.Type == GradeType.lesson).ToList();
            var skippedLesson = totalLessons.Where(g => g.Value == 0).ToList();
            var lessonGrades = totalLessons.Where(g => g.Value > 0).ToList();

            var totalHwGrades = studentsGradesModule.Where(g => g.Type == GradeType.homework).ToList();
            var skippedHWGrades = totalHwGrades.Where(g => g.Value == 0).ToList();
            var hwGrades = totalHwGrades.Where(g => g.Value > 0).ToList();

            var examGrade = studentsGradesModule.Where(g => g.Type == GradeType.exam).Select(g => g.Value).FirstOrDefault();

            // средние оценки (ДЗ и Уроки)
            decimal avglessonGrades = 0m;
            decimal avgHWGrades = 0m;

            if (lessonGrades.Count > 0 && lessonGrades.Sum(g => g.Value) > 0)
            {
                avglessonGrades = (decimal)lessonGrades.Sum(g => g.Value) / lessonGrades.Count;
            }

            if (totalHwGrades.Count > 0 && hwGrades.Sum(g => g.Value) > 0)
            {
                avgHWGrades = (decimal)hwGrades.Sum(g => g.Value) / totalHwGrades.Count;
            }

            // проверка на условия
            bool isHWNotPassedOverSeventy = false;
            bool isAvgLessonGradeBad = false;
            bool isSkippedLessons = false;
            bool isExamNotPassed = false;
            bool isExamBad = false;

            double hwComplition = 0;
            if (totalHwGrades.Count > 0)
            {
                hwComplition = (double)hwGrades.Count / totalHwGrades.Count;
            }

            if (hwComplition < 0.70)
            {
                isHWNotPassedOverSeventy = true;
            }

            if (avglessonGrades < 8)
            {
                isAvgLessonGradeBad = true;
            }

            if (examGrade > 0 && examGrade <= 4)
            {
                isExamBad = true;
            }
            else if (examGrade == 0)
            {
                isExamNotPassed = true;
            }

            if (skippedLesson.Count > 0)
            {
                isSkippedLessons = true;
            }

            //средняя за модуль, штрафы и финальная проверка
            decimal finalavgGrade = (decimal)((avglessonGrades + avgHWGrades + examGrade) / 3) - skippedHWGrades.Count;

            if (isAvgLessonGradeBad != true && skippedHWGrades.Count < 7 && isSkippedLessons != true)
            {
                if (finalavgGrade < 8m)
                {
                    finalavgGrade = 8m;
                }
            }

            if (isExamBad)
            {
                if (finalavgGrade > 9m)
                {
                    finalavgGrade = 8m;
                }
            }

            if (isExamNotPassed)
            {
                if (finalavgGrade > 6m)
                {
                    finalavgGrade = 6m;
                }
            }

            if (isHWNotPassedOverSeventy)
            {
                if (finalavgGrade != 0m)
                {
                    finalavgGrade = 0m;
                }
            }

            // корректировка
            if (finalavgGrade < 0m) finalavgGrade = 0m;
            if (finalavgGrade > 12m) finalavgGrade = 12m;

            return Math.Round(finalavgGrade, 2);
        }
    }
}

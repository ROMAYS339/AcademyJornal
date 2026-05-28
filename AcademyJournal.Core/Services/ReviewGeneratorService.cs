using AcademyJournal.Core.Interfaces;
using AcademyJournal.Core.Models;
using AcademyJournal.Core.Models.Enums;
using AcademyJournal.Core.Services.Samples;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AcademyJournal.Core.Services
{
    public class ReviewGeneratorService
    {
        private readonly IAppDbContext _context;
        private readonly Random _rand;

        public ReviewGeneratorService(IAppDbContext context)
        {
            _context = context;
            _rand = new Random();
        }

        public Review GenerateReview(int studentId, int moduleId)
        {
            var grades = _context.Grades
                .Where(g => g.StudentId == studentId && g.ModuleId == moduleId).ToList();

            var data = (from s in _context.Student
                        join m in _context.Modules on moduleId equals m.Id
                        where s.Id == studentId
                        select new { Student = s, Module = m }).FirstOrDefault();

            if (data == null || !grades.Any()) return null;

            var student = data.Student;
            var module = data.Module;

            var totalLessons = grades.Where(g => g.Type == GradeType.lesson).ToList();
            var lessonGrades = totalLessons.Where(g => g.Value > 0).ToList();
            var totalHw = grades.Where(g => g.Type == GradeType.homework).ToList();
            var hwGrades = totalHw.Where(g => g.Value > 0).ToList();
            var examGrade = grades.Where(g => g.Type == GradeType.exam)
                                  .Select(g => g.Value).FirstOrDefault();

            // Расчёт показателей
            // строка 45 (после исправления):
            decimal activity = lessonGrades.Count > 0 ? (decimal)lessonGrades.Average(g => g.Value) : 0;
            decimal attendance = totalLessons.Count > 0 ? (decimal)lessonGrades.Count / totalLessons.Count : 0;
            decimal hwCompletion = totalHw.Count > 0 ? (decimal)hwGrades.Count / totalHw.Count : 0;

            // Определение тематики модуля
            bool isDevelopment = DetermineIsDevelopment(module.Description);

            // Ключи для словарей
            int keyHw = GetKeyByRange(hwCompletion, 0.20m, 0.40m, 0.60m, 0.80m, 1);
            int keyExam = GetKeyByRange(examGrade, 3, 6, 9, 12, 2, 5);
            int keyAtt = GetKeyByRange(attendance, 0.20m, 0.40m, 0.60m, 0.80m, 1);
            int keyAct = activity switch { <= 3 => 2, <= 6 => 3, <= 9 => 4, _ => 5 };

            string keyAdvice = DetermineAdviceKey(keyHw, keyExam, keyAtt, keyAct);

            // Генерация блоков текста
            string moduleIntro = GetRandomPhrase(ReviewSamples.ModuleIntros).Replace("{ModuleName}", module.Name);
            string success = isDevelopment
                ? string.Format(GetRandomPhrase(ReviewSamples.DevSuccessPhrases), student.FullName)
                : string.Format(GetRandomPhrase(ReviewSamples.DesignSuccessPhrases), student.FullName);
            string activityPhrase = GetRandomPhrase(ReviewSamples.ActivityComments[keyAct]);
            string attendancePhrase = GetRandomPhrase(ReviewSamples.AttendanceComments[keyAtt]);
            string hwPhrase = GetRandomPhrase(ReviewSamples.PercentComments[keyHw]);
            string examPhrase = GetRandomPhrase(ReviewSamples.GradeComments[keyExam]);
            string advice = GetRandomPhrase(ReviewSamples.AdviceComments[keyAdvice]);

            // Сборка итогового текста со случайным украшением
            string decoration = GetRandomDecoration();
            string fullText = $"{moduleIntro}\n\n{success}\n{activityPhrase}\n{attendancePhrase}\n{hwPhrase}\n{examPhrase}\n\n{advice}\n{decoration}";

            return new Review
            {
                Text = fullText,
                Student = student,
                Module = module,
                CreatedAt = DateTime.Now,
                IsEdited = false
            };
        }

        private bool DetermineIsDevelopment(string moduleDesc)
        {
            var designWords = new[] {
                "дизайн", "ui", "ux", "css", "html", "figma", "sketch",
                "photoshop", "illustrator", "макет", "прототип", "типографика",
                "цвет", "шрифт", "анимация", "верстка", "layout", "интерфейс",
                "interface", "wireframe", "user interface", "user experience"
            };
            return !designWords.Any(w => moduleDesc.ToLower().Contains(w));
        }

        private int GetKeyByRange(decimal value, params decimal[] thresholds)
        {
            // thresholds: пороги включительно по возрастанию, последний элемент - максимальный ключ (по умолчанию)
            int maxKey = (int)thresholds.Last();
            for (int i = 0; i < thresholds.Length - 1; i++)
                if (value <= thresholds[i]) return i + 1;
            return maxKey;
        }

        private string DetermineAdviceKey(int hw, int exam, int att, int act)
        {
            if (hw >= 4 && exam >= 4 && att >= 4 && act >= 4) return "Perfect";
            if (hw < 4 && exam >= 4 && att >= 4 && act >= 4) return "FixHomework";
            if (hw >= 4 && exam >= 4 && att < 4 && act >= 4) return "FixAttendance";
            if (hw >= 4 && exam < 4 && att >= 4 && act >= 4) return "FixExam";
            if (hw < 4 && exam >= 4 && att < 4 && act >= 4) return "FixProcess";
            return "FixEverything";
        }

        private string GetRandomPhrase(List<string> list) => list[_rand.Next(list.Count)];
        private string GetRandomDecoration()
        {
            var decorations = new[] { "✨", "🌟", "🎓", "📚", "" };
            return decorations[_rand.Next(decorations.Length)];
        }
    }
}
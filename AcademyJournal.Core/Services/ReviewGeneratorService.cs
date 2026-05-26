using AcademyJournal.Core.Interfaces;
using AcademyJournal.Core.Models;
using AcademyJournal.Core.Models.Enums;
using AcademyJournal.Core.Services.Samples;
using System;
using System.Collections.Generic;
using System.Text;

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
            // подготовка

            var studentsGradesModule = _context.Grades.Where(g => g.StudentId == studentId && g.ModuleId == moduleId).ToList();

            var data = (from s in _context.Student
                        join m in _context.Modules on moduleId equals m.Id
                        where s.Id == studentId
                        select new { Student = s, Module = m })
                        .FirstOrDefault();

            if (data == null) return null; // если нет студента или модуля

            if (!studentsGradesModule.Any()) return null; // если нет оценок за модуль   

            var student = data.Student;
            var module = data.Module;

            var totalLessons = studentsGradesModule.Where(g => g.Type == GradeType.lesson).ToList();
            var lessonGrades = totalLessons.Where(g => g.Value > 0).ToList();

            var totalHwGrades = studentsGradesModule.Where(g => g.Type == GradeType.homework).ToList();
            var hwGrades = totalHwGrades.Where(g => g.Value > 0).ToList();

            var examGrade = studentsGradesModule.Where(g => g.Type == GradeType.exam).Select(g => g.Value).FirstOrDefault();

            //Процент выполнение дз, активность на уроках,
            decimal HwPercentComplition = 0m;
            decimal activity = 0m;
            decimal attendance = 0m;

            if (totalLessons.Count == 0 || lessonGrades.Count == 0 || lessonGrades.Sum(g => g.Value) == 0)
            {
                activity = 0m;
                attendance = 0m;
            }
            else
            {
                activity = Math.Round((decimal)lessonGrades.Sum(l => l.Value) / lessonGrades.Count, 2);
                attendance = Math.Round((decimal)lessonGrades.Count / totalLessons.Count, 2);
            }

            if (totalHwGrades.Count == 0 || hwGrades.Count == 0)
            {
                HwPercentComplition = 0m;
            }
            else
            {
                HwPercentComplition = Math.Round((decimal)hwGrades.Count / totalHwGrades.Count, 2);
                
            }


            //условия для выборки фраз

            bool isDevelopment = true;
            int keyHwPercentComplition = 0;
            int keyGradeExam = 0;
            int keyAttendance = 0;
            int keyActivity = 0;
            string keyAdvice = "";

            List<string> designWords = new List<string>
        {
            "design", "дизайн", "ui", "ux", "css", "html", "sass", "scss",
            "figma", "sketch", "photoshop", "illustrator", "tilda", "webflow",
            "макет", "прототип", "типографика", "цвет", "шрифт", "анимация",
            "юзабилити", "usability", "адаптив", "responsive", "верстка", "layout",
            "интерфейс", "interface", "wireframe", "user interface", "user experience"
        };

            //проверка на тему модуля
            string moduleDesc = module.Description.ToLower();
            foreach (var designItem in designWords)
            {
                if (moduleDesc.Contains(designItem))
                {
                    isDevelopment = false;
                    break;
                }
            }

            //проверка на процент выполнения дз
            if (HwPercentComplition >= 0m && HwPercentComplition <= 0.20m)
            {
                keyHwPercentComplition = 1;
            }
            else if (HwPercentComplition >= 0.21m && HwPercentComplition <= 0.40m)
            {
                keyHwPercentComplition = 2;
            }
            else if (HwPercentComplition >= 0.41m && HwPercentComplition <= 0.60m)
            {
                keyHwPercentComplition = 3;
            }
            else if (HwPercentComplition >= 0.61m && HwPercentComplition <= 0.80m)
            {
                keyHwPercentComplition = 4;
            }
            else
            {
                keyHwPercentComplition = 5;
            }

            //проверка на написание экзамена
            if (examGrade >= 0 && examGrade <= 3)
            {
                keyGradeExam = 2;
            }
            else if (examGrade > 3 && examGrade <= 6)
            {
                keyGradeExam = 3;
            }
            else if (examGrade > 6 && examGrade <= 9)
            {
                keyGradeExam = 4;
            }
            else
            {
                keyGradeExam = 5;
            }

            //проверка на посещаемость
            if (attendance >= 0 && attendance <= 0.20m)
            {
                keyAttendance = 1;
            }
            else if (attendance >= 0.21m && attendance <= 0.40m)
            {
                keyAttendance = 2;
            }
            else if (attendance >= 0.41m && attendance <= 0.60m)
            {
                keyAttendance = 3;
            }
            else if (attendance >= 0.61m && attendance <= 0.80m)
            {
                keyAttendance = 4;
            }
            else
            {
                keyAttendance = 5;
            }

            // активность
            if (activity >= 0m && activity <= 3m)
            {
                keyActivity = 2;
            }
            else if (activity > 3m && activity <= 6m)
            {
                keyActivity = 3;
            }
            else if (activity > 6m && activity <= 9m)
            {
                keyActivity = 4;
            }
            else
            {
                keyActivity = 5;
            }

            //совет ученику
            if (keyHwPercentComplition >= 4 && keyGradeExam >= 4 && keyAttendance >= 4 && keyActivity >= 4)
            {
                keyAdvice = "Perfect";
            }
            else if (keyHwPercentComplition < 4 && keyGradeExam >= 4 && keyAttendance >= 4 && keyActivity >= 4)
            {
                keyAdvice = "FixHomework";
            }
            else if (keyHwPercentComplition >= 4 && keyGradeExam >= 4 && keyAttendance < 4 && keyActivity >= 4)
            {
                keyAdvice = "FixAttendance";
            }
            else if (keyHwPercentComplition >= 4 && keyGradeExam < 4 && keyAttendance >= 4 && keyActivity >= 4)
            {
                keyAdvice = "FixExam";
            }
            else if (keyHwPercentComplition < 4 && keyGradeExam >= 4 && keyAttendance < 4 && keyActivity >= 4)
            {
                keyAdvice = "FixProcess";
            }
            else
            {
                keyAdvice = "FixEverything";
            }

            // выборка

            string successPhrase = "";
            if (isDevelopment)
            {
                successPhrase = ReviewSamples.DevSuccessPhrases[_rand.Next(ReviewSamples.DevSuccessPhrases.Count)];
            }
            else
            {
                successPhrase = ReviewSamples.DesignSuccessPhrases[_rand.Next(ReviewSamples.DesignSuccessPhrases.Count)];
            }
            successPhrase = string.Format(successPhrase, student.FullName);

            List<string> attendancePhrases = ReviewSamples.AttendanceComments[keyAttendance];
            string attendancePhrase = attendancePhrases[_rand.Next(attendancePhrases.Count)];

            List<string> hwPercentComplitionPhrases = ReviewSamples.PercentComments[keyHwPercentComplition];
            string hwPercentComplitionPhrase = hwPercentComplitionPhrases[_rand.Next(hwPercentComplitionPhrases.Count)];

            List<string> gradeExamPhrases = ReviewSamples.GradeComments[keyGradeExam];
            string gradeExamPhrase = gradeExamPhrases[_rand.Next(gradeExamPhrases.Count)];

            List<string> advicePhrases = ReviewSamples.AdviceComments[keyAdvice];
            string advice = advicePhrases[_rand.Next(advicePhrases.Count)];

            List<string> activityPhrases = ReviewSamples.ActivityComments[keyActivity];
            string activityPhrase = activityPhrases[_rand.Next(activityPhrases.Count)];

            //составление объекта Review

            string fullText = $"{module.Name} - {moduleDesc} \n" +
                $"{successPhrase} \n" +
                $"{activityPhrase} \n" +
                $"{attendancePhrase} \n" +
                $"{hwPercentComplitionPhrase} \n" +
                $"{gradeExamPhrase} \n" +
                $"{advice} \n";

            return new Review { Text = fullText, Student = student, Module = module, CreatedAt = DateTime.Now, IsEdited = false };
        }
    }
}

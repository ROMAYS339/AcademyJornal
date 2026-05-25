using AcademyJournal.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyJournal.Core.Services
{
    public class ReviewGeneratorService
    {
        private readonly Random _rng = new();

        private static readonly Dictionary<string, string[]> ModuleDescriptions = new()
        {
            ["Тестирование"] = new[]
            {
            "В рамках данного модуля студенты изучали методологию тестирования программного обеспечения, написание автоматических тестов и принципы обеспечения качества.",
            "Модуль посвящён теории и практике тестирования ПО: unit-тесты, интеграционные тесты, анализ покрытия кода.",
            "Курс охватывал основы QA-инженерии, включая ручное и автоматизированное тестирование веб-приложений.",
        },
            ["БД"] = new[]
            {
            "Модуль охватывал проектирование реляционных баз данных, язык SQL и работу с СУБД MS SQL Server.",
            "В ходе модуля студенты освоили нормализацию данных, построение ER-диаграмм и написание сложных SQL-запросов.",
            "Курс посвящён базам данных: от концептуального моделирования до оптимизации запросов и индексирования.",
        },
            ["Курсовой"] = new[]
            {
            "Курсовой модуль предполагал самостоятельную разработку полноценного программного продукта с применением изученных технологий.",
            "В рамках курсового проекта студенты реализовывали собственное приложение, демонстрируя навыки проектирования и программирования.",
            "Итоговый модуль направлен на создание и защиту индивидуального проекта, охватывающего весь стек изученных технологий.",
        },
        };

        private static readonly string[][] PerformanceTemplates =
        {
        new[]
        {
            "{name} испытывал(а) значительные трудности с усвоением материала модуля.",
            "{name} в целом справлялся(ась) с заданиями ниже среднего уровня группы.",
        },
        new[]
        {
            "{name} показал(а) удовлетворительные результаты и усвоил(а) базовый объём материала.",
            "{name} выполнял(а) задания на достаточном уровне, демонстрируя понимание ключевых концепций.",
        },
        new[]
        {
            "{name} продемонстрировал(а) хорошее понимание материала и уверенное владение практическими навыками.",
            "{name} стабильно справлялся(ась) с заданиями повышенной сложности и проявлял(а) инициативу на занятиях.",
        },
        new[]
        {
            "{name} показал(а) отличные результаты, демонстрируя глубокое понимание предмета и высокий уровень самостоятельности.",
            "{name} является одним из сильнейших студентов группы — уверенно решал(а) нестандартные задачи и помогал(а) коллегам.",
        },
    };

        private static readonly string[][] AdviceTemplates =
        {
        new[]
        {
            "Настоятельно рекомендуется уделять домашним заданиям значительно больше времени: именно систематическая практика формирует устойчивые навыки.",
            "Для повышения результатов необходимо кардинально улучшить дисциплину выполнения домашних работ — без регулярной практики прогресс невозможен.",
        },
        new[]
        {
            "Рекомендуется повысить ответственность при выполнении домашних заданий и не откладывать их на последний момент.",
            "Советуем уделить особое внимание домашней работе: регулярное выполнение заданий значительно ускорит освоение материала.",
        },
     
        new[]
        {
            "Рекомендуется поддерживать текущий темп выполнения домашних заданий и стремиться к 100% результату.",
            "Хорошая дисциплина при работе дома. Для достижения отличных результатов стоит сохранить эту тенденцию.",
        },
      
        new[]
        {
            "Отличная самодисциплина! Рекомендуется участие в дополнительных проектах и олимпиадах для дальнейшего профессионального роста.",
            "Студент проявил(а) образцовое отношение к домашней работе. Рекомендуется продолжать в том же духе и расширять кругозор через углублённое изучение смежных технологий.",
        },
    };

        
        /// <summary>
        /// Генерирует отзыв на студента.
        /// </summary>
        /// <param name="student">Студент.</param>
        /// <param name="module">Название модуля (из справочника).</param>
        /// <param name="finalScore">Итоговый балл (1-10).</param>
        /// <param name="hwCompletionPercent">Процент выполненных ДЗ (0-100).</param>
        public string Generate(
            Student student,
            string moduleName,
            double finalScore,
            double hwCompletionPercent)
        {
            string moduleDesc = PickRandom(GetModuleDescriptions(moduleName));
            string performance = PickRandom(GetPerformanceTemplates(finalScore))
                                       .Replace("{name}", student.FullName);
            string hwLine = FormatHomework(hwCompletionPercent);
            string advice = PickRandom(GetAdviceTemplates(hwCompletionPercent));

            return $"{moduleDesc}\n\n{performance} " +
                   $"Итоговый балл за модуль составил {finalScore:F1}. " +
                   $"{hwLine}\n\n{advice}";
        }

        private string[] GetModuleDescriptions(string moduleName)
            => ModuleDescriptions.TryGetValue(moduleName, out var arr) ? arr
               : new[] { $"В рамках модуля «{moduleName}» студенты изучали актуальные темы курса." };

        private string[] GetPerformanceTemplates(double score) =>
            score switch
            {
                < 5 => PerformanceTemplates[0],
                < 7 => PerformanceTemplates[1],
                < 9 => PerformanceTemplates[2],
                _ => PerformanceTemplates[3],
            };

        private string[] GetAdviceTemplates(double hwPercent) =>
            hwPercent switch
            {
                < 50 => AdviceTemplates[0],
                < 70 => AdviceTemplates[1],
                < 90 => AdviceTemplates[2],
                _ => AdviceTemplates[3],
            };

        private static string FormatHomework(double hwPercent)
        {
            string rate = hwPercent switch
            {
                >= 90 => "отличным",
                >= 70 => "хорошим",
                >= 50 => "удовлетворительным",
                _ => "неудовлетворительным",
            };
            return $"Процент выполнения домашних заданий составил {hwPercent:F0}%, что является {rate} показателем.";
        }

        private T PickRandom<T>(T[] arr) => arr[_rng.Next(arr.Length)];
    }

}

using AcademyJournal.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademyJournal.Core.Models
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0, 12)] //0 - это отсутствие на уроке или экзамене/ не сделанная, но не просроченная дз
        [Display(Name = "Оценка")]
        public int Value { get; set; }

        [Required]
        [Display(Name = "Тип")]
        public GradeType Type { get; set; }

        [Required]
        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }

        [Required]
        public int ModuleId { get; set; }
        [ForeignKey(nameof(ModuleId))]
        public Module Module { get; set; }
    }
}

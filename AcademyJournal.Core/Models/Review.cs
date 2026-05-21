using AcademyJournal.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AcademyJournal.Core.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Отзыв")]
        public string Text { get; set; }

        [Required]
        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }

        [Required]
        public int ModuleId { get; set; }
        [ForeignKey(nameof(ModuleId))]
        public Module Module { get; set; }

        [Display(Name = "Дата написания")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Изменен")]
        public bool IsEdited { get; set; }
    }
}

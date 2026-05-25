using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace AcademyJournal.Core.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Группа")]
        public string Group { get; set; }

        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public char Name { get; internal set; }
    }
}

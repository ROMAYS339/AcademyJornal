using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AcademyJournal.Core.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DesignPatternsRepositoryPattern.Models
{
    public class Employee
    {
        [Key]
        public int? Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$",ErrorMessage = "Invalid Email Format")]
        public string Email { get; set; }
        public int? DeptId { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ACES.Models
{
    public class Course
    {
        public int Id { get; set; }
        [DisplayName("Course Name")]
        public string CourseName { get; set; }
        public int InstructorId { get; set; }
        public bool IsCourseActive { get; set; }

        #region For Views
        [NotMapped]
        public int? NumAssignments { get; set; }
        [NotMapped]
        public int? NumStudents { get; set; }
        #endregion
    }
}

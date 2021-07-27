using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ACES.Models
{
    public class StudentAssignment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int AssignmentId { get; set; }
        public string RepositoryUrl { get; set; }
        public string JSONCode { get; set; }

        #region For Views
        [NotMapped]
        public string StudentName { get; set; }

        [NotMapped]
        public string Files { get; set; }

        #endregion
    }
}

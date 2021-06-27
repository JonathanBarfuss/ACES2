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
        /// <summary>
        /// This represents the number of watermarks a students personal assignment should have. 
        /// It is compared with Commit.NumWatermarks to see if they have tampered with the code.
        /// </summary>
        public string JSONCode { get; set; }

        #region For Views
        [NotMapped]
        public int? NumCommits { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        [NotMapped]
        public string PointsRatio { get; set; }
        [NotMapped]
        public string WatermarksRatio { get; set; }
        [NotMapped]
        public double LinesModifiedAvg { get; set; }
        [NotMapped]
        public double TimeBetweenAvg { get; set; }
        #endregion
    }
}

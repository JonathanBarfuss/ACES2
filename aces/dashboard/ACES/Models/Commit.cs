using System;

namespace ACES.Models
{
    public class Commit
    {
        public int Id { get; set; }
        public int StudentAssignmentId { get; set; }
        public int PointsEarned { get; set; }
        public DateTime DateCommitted { get; set; }
        public int LinesAdded { get; set; }
        public int LinesDeleted { get; set; }
        /// <summary>
        /// Compared with StudentAssignment.NumWatermarks to see if they have been tampered with.
        /// </summary>
        public int NumWatermarks { get; set; }
        public string JSONCode { get; set; }
    }
}

using System;

namespace ACES.Models
{
    public class Results
    {
        public int Id { get; set; }
        public int StudentAssignmentId { get; set; }
        public DateTime DateCommitted { get; set; }
        public string JSONCode { get; set; }
    }
}

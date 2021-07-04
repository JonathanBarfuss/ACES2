using System;

namespace ACES.Models
{
    public class Commit
    {
        public int Id { get; set; }
        public int StudentAssignmentId { get; set; }
        public DateTime DateCommitted { get; set; }
        public string JSONCode { get; set; }
    }
}

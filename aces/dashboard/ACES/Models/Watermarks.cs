using System;

namespace ACES.Models
{
    public class Watermarks
    {

        public int Id { get; set; }
        public string Watermark { get; set; }
        public int StudentID { get; set; }
        public int AssignmentID { get; set; }
        public string FileName { get; set; }
        public string StudentRepoName { get; set; }

    }
}

using System.Collections.Generic;

namespace ACES.Models.ViewModels
{
    public class AssignmentStudentCommitsVM
    {
        public int StudentAssignmentId { get; set; }
        public string StudentName { get; set; }
        //public int NumWatermarks { get; set; }
        public Assignment Assignment { get; set; }
        public List<Results> Commits { get; set; }
    }
}

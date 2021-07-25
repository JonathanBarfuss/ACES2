using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACES.Models.ViewModels
{
    public class CompareResultsVM
    {
        public string StudentName {get; set;}
        public DateTime CommitDate { get; set; }
        public string Watermarks { get; set; }
        public string Whitespaces { get; set; }
        public int NumberOfCommits { get; set; }
        public int LinesAdded { get; set; }
        public int LinesDeleted { get; set; }
        public TimeSpan AverageTime { get; set; }
        public string OtherWatermark { get; set; }
        public string WatermarkHighlight { get; set; }
        public string WhitespaceHighlight { get; set; }
        public string DueDateHighlight { get; set; }
    }
}

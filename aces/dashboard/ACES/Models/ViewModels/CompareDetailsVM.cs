using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACES.Models.ViewModels
{
    public class CompareDetailsVM
    {
        public DateTime CommitDate { get; set; }
        public String Message { get; set; }
        public String FileName { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ACES.Models.ViewModels
{
    public class AssignmentJsonVM
    {
        public int AssignmentId { get; set; }
        public string AssignmentName { get; set; } 
        
        public int CourseId { get; set; }

        public List<JSON> Json { get; set; }
        
    }
}

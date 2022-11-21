using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace ACES.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [DisplayName("Repository URL")]
        public string RepositoryUrl { get; set; }        

        [DisplayName("JSON Criteria")]
        public string JSONCode { get; set; }
        
        public string JSONFiles { get; set; }

        public int CourseId { get; set; }

        public DateTime DueDate { get; set; }

        [DisplayName("Canvas Link")]
        public string CanvasLink { get; set; }

    }
     
}

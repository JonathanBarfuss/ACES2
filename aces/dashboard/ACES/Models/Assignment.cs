using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

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

        public int CourseId { get; set; }

    }
}

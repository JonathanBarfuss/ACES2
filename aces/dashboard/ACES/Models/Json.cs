using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace ACES.Models
{
    public class JSON 
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }       

        [DisplayName("File Name")]
        public string FileName { get; set; }

        [DisplayName("# Line's in File")]
        public string LineNumbers { get; set; }

        [DisplayName("Replace Watermark")]
        public bool ReplaceWatermark { get; set; }

        [DisplayName("Comment for Watermark")]
        public string Comment { get; set; }

        [DisplayName("Whitespace Line #'s")]
        public string WhitespaceLines { get; set; }

        [DisplayName("Random String Line #'s")]
        public string RandomStringLines { get; set; }
        
    }    
}
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACES.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ACES.Data;
using System.Net.Http;
using System.IO;

namespace ACES.Controllers
{
    [ApiController]
    public class FactoryController : Controller
    {
        private string watermark;

        [HttpPost("factory")]
        public ActionResult<IEnumerable<PostAddWatermark>> factoryCreate([FromBody] PostAddWatermark data)
        {
            if(data.existing_watermark != "")
            {
                watermark = data.existing_watermark;
            } else
            {
                //GenerateWatermark
            }
            WatermarkFile(data.directory);
            return View();
        }

        private int WatermarkFile(string path)
        {
            int watermarks = 0;
            string line;

            // Read the file and display it line by line.  
            StreamReader file = new StreamReader(path);
            StreamWriter newFile = new StreamWriter("../../assignments/temp.cs");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("aw:watermark"))
                {
                    newFile.WriteLine("//aw:" + watermark);
                    watermarks++;
                } else
                {
                    newFile.WriteLine(line);
                }       
            }
            newFile.Close();
            return watermarks;
        }
    }
}

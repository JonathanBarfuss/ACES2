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
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace ACES.Controllers
{
    [ApiController]
    public class FactoryController : Controller
    {

        #region Globals

        private string watermark;

        #endregion

        [HttpPost("factory")]
        public HttpResponseMessage factoryCreate([FromBody] PostAddWatermark data)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            var emailAddress = new System.Net.Mail.MailAddress(data.email);

            if (emailAddress.Address != data.email)
            {
                //return View("Error: email provided is not a valid email.");
            }

            if (data.asn_no.Length! > 2)
            {
                //return View("Error: asn_no must be greater than 2 characters.");
            }

            if (data.existing_watermark == "")
            {
                watermark = Generate_Watermark(data.email, data.asn_no);
            }
            else
            {
                watermark = data.existing_watermark;
            }

            using (StreamReader r = new StreamReader(data.directory + "/.acesconfig.json"))
            {
                string jsonnn = r.ReadToEnd();
                Dictionary<string, string> files = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonnn);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(new GetWatermarkedAssignment()
            {
                watermark = watermark,
                watermark_count = WatermarkFile(data.directory),
                zipped_directory = data.directory
            });

            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
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
                }
                else
                {
                    newFile.WriteLine(line);
                }
            }
            newFile.Close();
            return watermarks;
        }

        private string Generate_Watermark(string email, string asn_no)
        {

            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            byte[] salt = new byte[20];
            for (int i = 0; i < 20; i++)
            {

                rngCsp.GetBytes(salt);

            }

            return "";

        }

    }
}


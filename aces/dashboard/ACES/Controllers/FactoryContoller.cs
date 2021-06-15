using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACES.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
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
        public HttpResponseMessage FactoryCreate([FromBody] PostAddWatermark data)
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

            var markableFiles = GenerateFileList(data.jsonCode);

            int totalMarks = 0;
            //string copiedPath = "out/" + watermark.Substring(0, 16) + "/" + data.asn_no;


            foreach (var f in markableFiles)
            {

                totalMarks += WatermarkFile(data.directory);  //Change the directory to the files retrieved.

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

        private List<string> GenerateFileList(string jsonCode)
        {

            JObject json = JObject.Parse(jsonCode);

            var files = json["namedfiles"].Children();

            var fileList = new List<string>();

            fileList.AddRange(files.Select(file => file.Value<string>()));

            return fileList;

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

            rngCsp.GetBytes(salt);

            byte[] enEmail = Encoding.UTF8.GetBytes(email);
            byte[] enAsn_no = Encoding.UTF8.GetBytes(asn_no);

            List<byte> watermarkBytes = new List<byte>();
            watermarkBytes.AddRange(enAsn_no);
            watermarkBytes.AddRange(enEmail);
            watermarkBytes.AddRange(salt);

            byte[] bytes = SHA256.Create().ComputeHash(watermarkBytes.ToArray());

            StringBuilder fullWatermark = new StringBuilder();
            for(int i = 0; i < bytes.Length; i++)
            {

                fullWatermark.Append(bytes[i].ToString("x2"));

            }

            return fullWatermark.ToString();

        }

    }
}




using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ACES.Controllers
{
    [ApiController]
    public class FactoryController : Controller
    {

        #region Globals

        private string watermark;
        private int numWhitespaces;
        string whiteString = "";
        private List<string> whitespaces = new List<string>();
        private List<string> randomstring = new List<string>();

        #endregion

        #region FactoryCreate
        [HttpPost("factory")]
        public string FactoryCreate([FromBody] PostAddWatermark data)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            var emailAddress = new System.Net.Mail.MailAddress(data.email);
            numWhitespaces = data.whitespaces;

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
                watermark = GenerateWatermark(data.email, data.asn_no);
            }
            else
            {
                watermark = data.existing_watermark;
            }

            var markableFiles = GenerateListsFromJSON(data.jsonCode);

            int totalMarks = 0;
            //string copiedPath = "out/" + watermark.Substring(0, 16) + "/" + data.asn_no;


            foreach (var f in markableFiles)
            {

                totalMarks += WatermarkFile(data.directory, f);  //Change the directory to the files retrieved. "copiedPath + "/" + f" or something that gets the correct files each time.

            }

            // can change GetWatermarkedAssignment to have whatever variables you need it to return. It is at the bottom of the StudentInterfaceController
            var json = System.Text.Json.JsonSerializer.Serialize(new GetWatermarkedAssignment()
            {
                // add field for repo once we get it working
                watermark = watermark,
                watermark_count = totalMarks,
                whitespace_count = numWhitespaces
            });

            return json;
        }
        #endregion

        #region GenerateFileList
        private List<string> GenerateListsFromJSON(string jsonCode)
        {

            //Parse the json code provided in the string parameter
            JObject json = JObject.Parse(jsonCode);

            //get the data from the json string in the database
            var files = json["files"].Children();
            var lines = json["whitespaces"].Children();
            var moreLines = json["randomstring"].Children();

            var fileList = new List<string>();

            //Put the file names obtained in files variable into this list object
            fileList.AddRange(files.Select(file => file.Value<string>()));
            whitespaces.AddRange(lines.Select(lines => lines.Value<string>()));
            randomstring.AddRange(moreLines.Select(moreLines => moreLines.Value<string>()));

            return fileList;

        }
        #endregion

        #region WatermarkFile
        private int WatermarkFile(string dir, string filename)
        {
            int watermarks = 0;
            string line;
            int lineCount = 1;
            Random rnd = new Random();

            if (numWhitespaces == -1) // -1 if it doesn't exist so it has to generate a number
            {
                numWhitespaces = rnd.Next(25);
            }

            for (int i = 0; i < numWhitespaces; i++)
            {
                whiteString += " ";
            }


            // Read the file and display it line by line.  
            StreamReader file = new StreamReader(dir + "/" + filename);
            StreamWriter newFile = new StreamWriter("../../assignments/temp2/" + filename); // this file is the watermarked one, should be put in the new repository after the while loop
            while ((line = file.ReadLine()) != null)
            {
                if (whitespaces.Contains(lineCount.ToString()))
                {
                    newFile.WriteLine(whiteString);
                    watermarks++;
                }
                else if (randomstring.Contains(lineCount.ToString()))
                {
                    newFile.WriteLine("//" + watermark);
                    watermarks++;
                }
                else
                {
                    newFile.WriteLine(line);
                }
                lineCount++;
            }
            newFile.Close();
            return watermarks;
        }
        #endregion

        #region GenerateWatermark
        private string GenerateWatermark(string email, string asn_no)
        {

            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

            byte[] salt = new byte[20];
            //Fill array with random bytes to be salt value
            rngCsp.GetBytes(salt);

            //Change email and asn_no to bytes
            byte[] enEmail = Encoding.UTF8.GetBytes(email);
            byte[] enAsn_no = Encoding.UTF8.GetBytes(asn_no);

            //Combine salt value with encoded values for email and asn_no
            List<byte> watermarkBytes = new List<byte>();
            watermarkBytes.AddRange(enAsn_no);
            watermarkBytes.AddRange(enEmail);
            watermarkBytes.AddRange(salt);

            //Hash full array to get the full watermark
            byte[] bytes = SHA256.Create().ComputeHash(watermarkBytes.ToArray());

            //Put hashed value into one string
            StringBuilder fullWatermark = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {

                fullWatermark.Append(bytes[i].ToString("x2"));

            }

            return fullWatermark.ToString();

        }
        #endregion

    }
}




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
        private int numberOfWhitespaces;
        string whiteString = "";
        bool replaceInsteadOfInsert = false;
        string comment;
        int[] whitespacesLineNumbers;
        int[] randomStringLineNumbers;
        private List<string> whitespaces = new List<string>();
        private List<string> randomstring = new List<string>();

        #endregion

        #region FactoryCreate
        [HttpPost("factory")]
        public string FactoryCreate([FromBody] PostAddWatermark data) // data variable carries over from the StudentInterfaceController, can add fields if needed in that file in PostAddWatermark struct
        {
            HttpResponseMessage response = new HttpResponseMessage();
            var emailAddress = new System.Net.Mail.MailAddress(data.email);
            whitespacesLineNumbers = data.whitespacesLineNumbers;
            randomStringLineNumbers = data.randomStringLineNumbers;
            replaceInsteadOfInsert = data.replaceInsteadOfInsert;
            if (data.comment == "no comment") // assign comment if the professor provided one
            {
                comment = "DO NOT DELETE THIS LINE";
            } else
            {
                comment = data.comment;
            }

            if(data.watermark == "no watermark") // assign/create watermark if one existed or not
            {
                watermark = GenerateWatermark(data.email, data.assignmentName);
            }
            else
            {
                watermark = data.watermark;
            }
            

            Random rnd = new Random();
            numberOfWhitespaces = rnd.Next(10,30); // generating a random number of whitespaces to put after the comment
            for (int i = 0; i < numberOfWhitespaces; i++){ whiteString += " "; }

            data.fileContent = WatermarkFile(data.fileContent);

            // can change GetWatermarkedAssignment to have whatever variables you need it to return. It is at the bottom of the StudentInterfaceController
            var json = System.Text.Json.JsonSerializer.Serialize(new GetWatermarkedAssignment()
            {
                comment = comment,
                numberOfWhitespaceCharacters = numberOfWhitespaces,
                watermark = watermark,
                markedFileContent = data.fileContent
            });

            return json;
        }
        #endregion       

        #region WatermarkFile
        private string WatermarkFile(string fileContent)
        {   
            string[] contentLines = fileContent.Split("\n");

            // Add generated whitestring
            for (int i = 0; i < whitespacesLineNumbers.Length; i++)
            {
                if (replaceInsteadOfInsert)
                {
                    contentLines[whitespacesLineNumbers[i] - 1] = "// "+ comment + whiteString; // adds whitespace watermark after this comment
                }
                else
                {
                    contentLines[whitespacesLineNumbers[i] - 1] = "\n// " + comment + whiteString; // adds whitespace watermark after this comment
                }
            }

            // Add generated watermark
            for (int i = 0; i < randomStringLineNumbers.Length; i++)
            {
                if (replaceInsteadOfInsert)
                {
                    contentLines[randomStringLineNumbers[i] - 1] = "//wm" + watermark;
                }
                else
                {
                    contentLines[randomStringLineNumbers[i] - 1] = "\n//wm" + watermark;
                }
            } 
            return string.Join("\n", contentLines);
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




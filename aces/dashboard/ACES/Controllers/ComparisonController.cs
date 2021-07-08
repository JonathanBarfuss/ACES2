using ACES.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ACES.Controllers
{
    public class ComparisonController : Controller
    {
        private string watermark;
        private List<string> fileList = new List<string>();
        private int numWatermarks;
        private int numWhitespaces;
        private string whitestring;

        private readonly ACESContext _context;
        private readonly IConfiguration _configuration;

        public ComparisonController(ACESContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()//List<StudentAssignment> students)
        {
            Request.Cookies.TryGetValue("StudentId", out string stringid);
            Int32.TryParse(stringid, out int studentId);

            var studAssign = _context.StudentAssignment.FirstOrDefault(x => x.StudentId == studentId);
            string token = _configuration["GithubToken"];


            using (var httpClient = new HttpClient())
            {
                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                

                // for each to loop through students
                dynamic json = JsonConvert.DeserializeObject(studAssign.JSONCode);

                string studentRepoContents = $"{studAssign.RepositoryUrl}/contents".Replace("//github.com", "//api.github.com/repos");
                var objStudentRepoRequest = new HttpRequestMessage(HttpMethod.Get, studentRepoContents);

                using (HttpResponseMessage objStudentRepoResponse = httpClient.SendAsync(objStudentRepoRequest).Result)
                {
                    if (objStudentRepoResponse.IsSuccessStatusCode)
                    {
                        FileInfo[] contents = JsonConvert.DeserializeObject<FileInfo[]>(objStudentRepoResponse.Content.ReadAsStringAsync().Result);
                        foreach (var file in contents)
                        {
                            if (file.type == "file")
                            {
                                // Check if file is part of JSON instructions for watermarking.
                                foreach (var fileInJson in json.files)
                                {
                                    if (fileInJson.name.Value == file.name)
                                    {
                                        //content var has file contents
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var file in fileList)
            {
                CompareFile("../../assignments", file);
            }
            return View();
        }

        private void CompareFile(string dir, string filename)
        {
            string line;
            int white = 0;
            int strung = 0;
            int lineCount = 1;
            //For reading file to be compared  
            StreamReader file = new StreamReader(dir + "/" + filename);

            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains(whitestring))
                {
                    white++;
                }
                else if (line.Contains(watermark))
                {
                    strung++;
                }
                lineCount++;
            }

            Calculate(white / numWhitespaces, strung / numWatermarks);

            //Close stream reader and writer
            file.Close();
        }

        private void Calculate(float whiteRatio, float stringRatio)
        {

        }
    }
}


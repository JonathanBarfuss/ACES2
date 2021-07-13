using ACES.Data;
using ACES.Models;
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
        private List<string> fileList = new List<string>();
        private string watermark;
        private string whitestring;
        private int ogWatermarkCount;
        private int ogWhitespaceCount;
        private int curWatermarkCount;
        private int curWhitespaceCount;
        int[] whiteLines;
        int[] stringLines;

        private readonly ACESContext _context;
        private readonly IConfiguration _configuration;

        public ComparisonController(ACESContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index(int assignmentID)
        {
            //var studentList = assignmentID;

            var studentSubmissions = _context.StudentAssignment.Where(i => i.AssignmentId == assignmentID);
            List<StudentAssignment> studentSubmissionsList = studentSubmissions.ToList(); 

            //Request.Cookies.TryGetValue("StudentId", out string stringid);
            //Int32.TryParse(stringid, out int studentId);

            //var studAssign = _context.StudentAssignment.FirstOrDefault(x => x.StudentId == studentId);
            string token = _configuration["GithubToken"];


            using (var httpClient = new HttpClient())
            {
                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                foreach (var student in studentSubmissionsList)
                {
                    dynamic json = JsonConvert.DeserializeObject(student.JSONCode);

                    string studentRepoContents = $"{student.RepositoryUrl}/contents".Replace("//github.com", "//api.github.com/repos");
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
                                        if (fileInJson.fileName.Value == file.name)
                                        {
                                            // get file from github
                                            HttpRequestMessage fileGetRequest = new HttpRequestMessage(HttpMethod.Get, file.download_url);
                                            fileGetRequest.Headers.Add("Authorization", "Bearer " + token);
                                            HttpResponseMessage fileGetResponse = httpClient.SendAsync(fileGetRequest).Result;
                                            string fileContent = fileGetResponse.Content.ReadAsStringAsync().Result;
                                            fileGetResponse.Dispose();

                                            whitestring = ""; // reset for use of new file watermark
                                            for (int i = 0; i < fileInJson.numberOfWhitespaceCharacters.Value; i++)
                                            {
                                                whitestring += " ";
                                            }
                                            watermark = fileInJson.watermark.Value;
                                            whiteLines = fileInJson.whitespacesLineNumbers.ToObject<int[]>();
                                            stringLines = fileInJson.randomStringLineNumbers.ToObject<int[]>();
                                            ogWhitespaceCount += whiteLines.Count(); // increment total whitespaces count
                                            ogWatermarkCount += stringLines.Count(); // increment total watermark count
                                            CompareFile(fileContent);
                                            PopulateCommitDB(student.Id, "2020-02-20 12:00:00.0000000"); // change date to whatever we pull from github api
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {

                        }
                    }


                }
                
            }

            return View();
        }

        private void CompareFile(string fileContent)
        {
            string[] contentLines = fileContent.Split("\n");

            foreach (var line in contentLines)
            {
                if (whitestring != "" && line.Contains(whitestring))
                {
                    curWhitespaceCount++;
                }
                else if (line.Contains(watermark))
                {
                    curWatermarkCount++;
                }
            }
        }

        private void PopulateCommitDB(int id, string dateCommit)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new CommitJSON()
            {
                watermarks = curWatermarkCount + "/" + ogWatermarkCount,
                whitespaces = curWhitespaceCount + "/" + ogWhitespaceCount
            });
            var newCommit = new Commit()
            {
                StudentAssignmentId = id,
                DateCommitted = Convert.ToDateTime(dateCommit),
                JSONCode = json
            };
            _context.Commit.Add(newCommit);
            _context.SaveChanges();
        }
    }
}

public struct CommitJSON
{
    public string watermarks { get; set; }
    public string whitespaces { get; set; }
}


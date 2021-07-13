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
        private int numberOfCommits;
        private TimeSpan averageTimeBetweenCommits;
        private int linesAdded;
        private int linesDeleted;
        private DateTime finalCommit;

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
                    ogWatermarkCount = 0;
                    ogWhitespaceCount = 0;
                    curWatermarkCount = 0;
                    curWhitespaceCount = 0;
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
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                    PopulateCommitDB(student.Id, "2020-02-20 12:00:00.0000000"); // change date to whatever we pull from github api
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

        private void GatherGithubInfo(string studentURL)
        {
            List<String> shas = new List<string>();
            List<DateTime> times = new List<DateTime>();
            string token = _configuration["GithubToken"];

            using (var httpClient = new HttpClient())
            {
                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                string studentRepoCommits = $"{studentURL}".Replace("/contents", "/commits");  //**********************verify correct URL***************
                var objRepoRequest = new HttpRequestMessage(HttpMethod.Get, studentRepoCommits);

                using (HttpResponseMessage objRepoResponse = httpClient.SendAsync(objRepoRequest).Result)
                {
                    if (objRepoResponse.IsSuccessStatusCode)
                    {
                        var jsonInfo = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(objRepoResponse.Content.ReadAsStringAsync().Result);
                        numberOfCommits = jsonInfo.Count;  //count the number of commits
                        finalCommit = (DateTime)jsonInfo[0]["commit"]["committer"]["date"];  //get the datetime of the most recent commit*********************verify this is the last commit

                        for (int i = 0; i < jsonInfo.Count; i++)
                        {

                            shas.Add(jsonInfo[i]["sha"].ToString());
                            var tempDate = jsonInfo[i]["commit"]["committer"]["date"];
                            times.Add((DateTime)tempDate);
                        }
                        averageTimeBetweenCommits = calculateAverageTime(times);
                    }
                }
                foreach (String sha in shas)
                {
                    string studentRepoCommitURL = String.Format($"{studentRepoCommits}/{0}", sha);  //**************verify correct url**************
                    objRepoRequest = new HttpRequestMessage(HttpMethod.Get, studentRepoCommitURL);

                    using (HttpResponseMessage objRepoResponse = httpClient.SendAsync(objRepoRequest).Result)
                    {
                        if (objRepoResponse.IsSuccessStatusCode)
                        {
                            var jsonStuff = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(objRepoResponse.Content.ReadAsStringAsync().Result);
                            int tempAdded = (int)jsonStuff["stats"]["additions"];
                            linesAdded += tempAdded;
                            linesDeleted += (int)jsonStuff["stats"]["deletions"];
                        }
                    }
                }
            }
        }

        //helper method to calculate the average amount of time between commits
        private TimeSpan calculateAverageTime(List<DateTime> times)
        {
            TimeSpan timeTotal;
            timeTotal = TimeSpan.Zero;

            for (int i = 0; i < times.Count - 1; i++)  //calculate all the differences in time
            {
                var tempTime = times[i].Subtract(times[i + 1]);
                timeTotal = timeTotal.Add(tempTime);
            }
            return timeTotal / times.Count;
        }
    }
}

public struct CommitJSON
{
    public string watermarks { get; set; }
    public string whitespaces { get; set; }
}

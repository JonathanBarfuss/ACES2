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
        private long averageTimespanTicks;
        private int linesAdded;
        private int linesDeleted;
        private DateTime finalCommitTime;
        private string otherWatermark = "none";

        private readonly ACESContext _context;
        private readonly IConfiguration _configuration;

        public ComparisonController(ACESContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index(int assignmentID)
        {    
            var studentSubmissions = _context.StudentAssignment.Where(i => i.AssignmentId == assignmentID);
            List<StudentAssignment> studentSubmissionsList = studentSubmissions.ToList(); 

            string token = _configuration["GithubToken"];


            using (var httpClient = new HttpClient())
            {
                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                foreach (var student in studentSubmissionsList)
                {
                    ogWatermarkCount = 0;  //initialize variables
                    ogWhitespaceCount = 0;
                    curWatermarkCount = 0;
                    curWhitespaceCount = 0;
                    otherWatermark = "none";
                    linesAdded = 0;
                    linesDeleted = 0;

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
                    GatherGithubInfo(studentRepoContents);
                    PopulateCommitDB(student.Id);
                    
                }
                
            }
            
            return RedirectToAction("ComparisonResults", "Assignments", new { id = assignmentID });
        }

        private void CompareFile(string fileContent)
        {
            string[] contentLines = fileContent.Split("\n");

            foreach (var line in contentLines)
            {
                if (line.Contains("// DO NOT REMOVE THIS LINE")) // whitespace watermark is after this comment 
                {
                    if (line.Contains(whitestring))
                    {
                        curWhitespaceCount++;
                    }
                }
                else if (line.Contains("//wm")) // searches for //wm cause that is where the watermark would be to find if there is a wrong watermark there
                {
                    if (line.Contains(watermark))
                    {
                        curWatermarkCount++;
                    }
                    else
                    {
                        // THEY HAVE A WRONG WATERMARK IF IT GETS TO THIS CODE
                        otherWatermark = line.ToString();
                        otherWatermark = otherWatermark.Replace("//wm", "");
                    }                    
                }
            }
        }

        private void PopulateCommitDB(int id)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new CommitJSON()
            {
                watermarks = curWatermarkCount,
                ogWatermarks = ogWatermarkCount,
                whitespaces = curWhitespaceCount,
                ogWhitespaces = ogWhitespaceCount,
                NumberOfCommits = numberOfCommits,
                LinesAdded = linesAdded,
                LinesDeleted = linesDeleted,
                AverageTimespanTicks = averageTimespanTicks,
                OtherWatermark = otherWatermark
            });
            var studentCommit = _context.Results.Where(i => i.StudentAssignmentId == id).FirstOrDefault();
            if (studentCommit == null)
            {
                var newCommit = new Results()
                {
                    StudentAssignmentId = id,
                    DateCommitted = finalCommitTime,
                    JSONCode = json
                };
                _context.Results.Add(newCommit);
            } 
            else
            {
                studentCommit.DateCommitted = finalCommitTime;
                studentCommit.JSONCode = json;
            }
            
            _context.SaveChanges();
        }

        //method to get github information for a specific students URL and save results in the class variables
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

                string studentRepoCommits = $"{studentURL}".Replace("/contents", "/commits");
                string studentRepoCommitsLimit = studentRepoCommits + "?per_page=40";   //?per_page parameter sets the number of results, default 30 max 100
                var objRepoRequest = new HttpRequestMessage(HttpMethod.Get, studentRepoCommitsLimit);

                using (HttpResponseMessage objRepoResponse = httpClient.SendAsync(objRepoRequest).Result)
                {
                    if (objRepoResponse.IsSuccessStatusCode)
                    {
                        var jsonInfo = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(objRepoResponse.Content.ReadAsStringAsync().Result);
                        numberOfCommits = jsonInfo.Count;  //count the number of commits
                        finalCommitTime = (DateTime)jsonInfo[0]["commit"]["committer"]["date"];  //get the datetime of the most recent commit
                        finalCommitTime = finalCommitTime.ToLocalTime();  //convert from UTC time to local time  

                        for (int i = 0; i < jsonInfo.Count; i++)  //get the sha for each commit
                        {
                            //**TODO: for now all commits are saved for testing, in the future remove all that were added by the "anticheatbot" to only keep the student's commits** 
                            shas.Add(jsonInfo[i]["sha"].ToString()); 
                            var tempDate = jsonInfo[i]["commit"]["committer"]["date"];
                            times.Add((DateTime)tempDate);
                        }
                        if (numberOfCommits > 1)  //calculate the average time between commits if there are at least 2 commits
                        {
                            averageTimespanTicks = calculateAverageTime(times);
                        }
                        else
                        {
                            averageTimespanTicks = 0;
                        }
                        
                    }
                }
                //INFO: This will loop through each commit, doing an api call for each which takes time, if determined too long use the api /compare call 
                //to compare differences from start to finish in one call at the cost of loosing detail.  Use /compare/{starting sha}...{ending sha} or
                //move the lines added and lines deleted code to the details, or have the values appear asynchronously as they are calculated
                foreach (String sha in shas)  //for each individual commit get the lines added and deleted
                {
                    string studentRepoCommitURL = String.Format($"{studentRepoCommits}/{sha}");
                    objRepoRequest = new HttpRequestMessage(HttpMethod.Get, studentRepoCommitURL);

                    using (HttpResponseMessage objRepoResponse = httpClient.SendAsync(objRepoRequest).Result)
                    {
                        if (objRepoResponse.IsSuccessStatusCode)
                        {
                            var jsonStuff = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(objRepoResponse.Content.ReadAsStringAsync().Result);
                            linesAdded += (int)jsonStuff["stats"]["additions"];
                            linesDeleted += (int)jsonStuff["stats"]["deletions"];
                        }
                    }
                }
            }
        }

        //helper method to calculate the average amount of time between commits measured in Timespan Ticks
        private long calculateAverageTime(List<DateTime> times)
        {
            TimeSpan timeTotal;
            timeTotal = TimeSpan.Zero;

            for (int i = 0; i < times.Count - 1; i++)  //calculate all the differences in time
            {
                var tempTime = times[i].Subtract(times[i + 1]);
                timeTotal = timeTotal.Add(tempTime);
            }
            timeTotal /= (times.Count - 1);  //divide by number of time differences == count - 1
            return (long)timeTotal.Ticks;
        }
    }
}

public struct CommitJSON
{
    public int watermarks { get; set; }
    public int ogWatermarks { get; set; }
    public int whitespaces { get; set; }
    public int ogWhitespaces { get; set; }
    public int NumberOfCommits { get; set; }
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
    public long AverageTimespanTicks { get; set; }
    public string OtherWatermark { get; set; }
}

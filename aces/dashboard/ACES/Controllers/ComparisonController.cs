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

                //foreach(var student in students) {}
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
                                    if (fileInJson.fileName.Value == file.name)
                                    {
                                        // get file from github
                                        HttpRequestMessage fileGetRequest = new HttpRequestMessage(HttpMethod.Get, file.download_url);
                                        fileGetRequest.Headers.Add("Authorization", "Bearer " + token);
                                        HttpResponseMessage fileGetResponse = httpClient.SendAsync(fileGetRequest).Result;
                                        string fileContent = fileGetResponse.Content.ReadAsStringAsync().Result;
                                        fileGetResponse.Dispose();

                                        whitestring = ""; // reset for use of new file watermark
                                        for(int i = 0; i < fileInJson.numberOfWhitespaceCharacters.Value; i++)
                                        {
                                            whitestring += " ";
                                        }
                                        watermark = fileInJson.watermark.Value;
                                        whiteLines = fileInJson.whitespacesLineNumbers.ToObject<int[]>();
                                        stringLines = fileInJson.randomStringLineNumbers.ToObject<int[]>();
                                        ogWhitespaceCount += whiteLines.Count(); // increment total whitespaces count
                                        ogWatermarkCount += stringLines.Count(); // increment total watermark count
                                        CompareFile(fileContent);
                                        GatherGithubInfo();
                                        PopulateCommitDB(studAssign.Id, "2020-02-20 12:00:00.0000000"); // change date to whatever we pull from github api
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

        private void GatherGithubInfo()
        {
            List<String> shas = new List<string>();
            List<DateTime> times = new List<DateTime>();

            using (var httpClient = new HttpClient())
            {
                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + "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");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                string studentRepoContents = $"{studAssign.RepositoryUrl}/contents".Replace("//github.com", "//api.github.com/repos");

                string RepoContents = @"https://api.github.com/repos/AntiCheatSummer2021/Template1/commits";  //currently hard coded
                var objRepoRequest = new HttpRequestMessage(HttpMethod.Get, RepoContents);

            }
    }
}

public struct CommitJSON
{
    public string watermarks { get; set; }
    public string whitespaces { get; set; }
}

// delete all past this point
//initialize required variables
var info = new GoTimeVM();
List<String> shas = new List<string>();
List<DateTime> times = new List<DateTime>();
int linesAdded = 0;
int linesDeleted = 0;
List<int> listAdded = new List<int>(); //*****************************for testing only*******************

            using (var httpClient = new HttpClient())
            {
                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + "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");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                string RepoContents = @"https://api.github.com/repos/AntiCheatSummer2021/Template1/commits";  //currently hard coded
var objRepoRequest = new HttpRequestMessage(HttpMethod.Get, RepoContents);

                // Get response
                using (HttpResponseMessage objRepoResponse = httpClient.SendAsync(objRepoRequest).Result)
                {
                    if (objRepoResponse.IsSuccessStatusCode)
                    {
                        var jsonStuff = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(objRepoResponse.Content.ReadAsStringAsync().Result);
info.NumCommits = jsonStuff.Count;

                        for (int i = 0; i<jsonStuff.Count; i++)
                        {

                            shas.Add(jsonStuff[i]["sha"].ToString());
                            var tempDate = jsonStuff[i]["commit"]["committer"]["date"];
times.Add((DateTime) tempDate);
                        }
                        info.AverageTime = calculateAverageTime(times);

                    }
                }

                int tempAdded = 0;  //***********************for testing********************************************
                foreach (String sha in shas)
                {
                    RepoContents = String.Format(@"https://api.github.com/repos/AntiCheatSummer2021/Template1/commits/{0}", sha);  //hard coded plus sha
                    objRepoRequest = new HttpRequestMessage(HttpMethod.Get, RepoContents);

                    using (HttpResponseMessage objRepoResponse = httpClient.SendAsync(objRepoRequest).Result)
                    {
                        if (objRepoResponse.IsSuccessStatusCode)
                        {
                            var jsonStuff = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(objRepoResponse.Content.ReadAsStringAsync().Result);
tempAdded = (int) jsonStuff["stats"]["additions"];
                            listAdded.Add(tempAdded); //for testing only**********************************************
                            linesAdded += tempAdded;
                            linesDeleted += (int) jsonStuff["stats"]["deletions"];
                        }
                    }
                }
                info.LinesAdded = listAdded;  //****************************testing only*******************
                info.TotalAdded = linesAdded;
                info.TotalDeleted = linesDeleted;
                return View(info);
            }
            
            info.NumCommits = 6969;
            return View(info);
                        
        }

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
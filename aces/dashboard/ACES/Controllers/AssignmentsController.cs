using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACES.Data;
using ACES.Models;
using ACES.Models.ViewModels;
using System.Collections.Immutable;
using System.Drawing;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace ACES.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly ACESContext _context;
        private readonly IConfiguration _configuration;

        public AssignmentsController(ACESContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Assignments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Assignment.ToListAsync());
        }

        // GET: Assignments/AssignmentStudents/5
        public async Task<IActionResult> AssignmentStudents(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment.FirstOrDefaultAsync(m => m.Id == id);
            if (assignment == null)
            {
                return NotFound();
            }

            var studentAssignments = await _context.StudentAssignment.Where(x => x.AssignmentId == id).ToListAsync();
            foreach (var sAssignment in studentAssignments)
            {
                var student = await _context.Student.FirstOrDefaultAsync(x => x.Id == sAssignment.StudentId);
                sAssignment.StudentName = student.FullName;

                var jsonInfo = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(sAssignment.JSONCode);  //use the jsonCode to get the file names
                var lstFiles = (Newtonsoft.Json.Linq.JArray)jsonInfo["files"];
                for (int i = 0; i < lstFiles.Count; i++)  //loop through each file and get the name
                {
                    sAssignment.Files += "<div>" + (string)lstFiles[i]["fileName"] + "</div>";  //add each name to the files string along with some html
                }
            }

            var vm = new AssignmentStudentsVM()
            {
                CourseId = assignment.CourseId,
                AssignmentId = id.Value,
                AssignmentName = assignment.Name,
                StudentAssignments = studentAssignments
            };

            return View(vm);
        }

        // GET: Assignments/ComparisonResults/int
        public async Task<IActionResult> ComparisonResults(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment.FirstOrDefaultAsync(m => m.Id == id);  //make sure the assignment exists
            if (assignment == null)
            {
                return NotFound();
            }
            ViewBag.AssignmentName = assignment.Name;  //store the assignment name to display in the view
            ViewBag.AssignmentID = id;

            //join the student assignment table with the student table and the commit table
            var joinCommits = (from sa in _context.StudentAssignment
                               join s in _context.Student on sa.StudentId equals s.Id
                               join c in _context.Results on sa.Id equals c.StudentAssignmentId
                               select new
                               {
                                   studentAssignmentId = sa.Id,
                                   assignmentId = sa.AssignmentId,
                                   studentName = s.FullName,
                                   jsonCode = c.JSONCode,
                                   dateCommited = c.DateCommitted
                               }).ToListAsync();

            var compareResults = joinCommits.Result.Where(x => x.assignmentId == id);  //select only those for the desired assignment

            List<CompareResultsVM> listResults = new List<CompareResultsVM>();  //create list to hold the results

            foreach (var result in compareResults)  //create the view model for each result
            {
                var vm = new CompareResultsVM();
                var jsonInfo = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(result.jsonCode);
                vm.StudentName = result.studentName;
                vm.StudentAssignmentId = result.studentAssignmentId;
                vm.CommitDate = result.dateCommited;
                int watermarks = (int)jsonInfo["watermarks"];
                int ogWatermarks = (int)jsonInfo["ogWatermarks"];
                vm.Watermarks = watermarks.ToString() + "/" + ogWatermarks.ToString();  //display as a fraction of original watermarks
                int whitespaces = (int)jsonInfo["whitespaces"];
                int ogWhitespaces = (int)jsonInfo["ogWhitespaces"];
                vm.Whitespaces = whitespaces.ToString() + "/" + ogWhitespaces.ToString();  //display as a fraction of original white spaces
                vm.NumberOfCommits = (int)jsonInfo["NumberOfCommits"];
                vm.LinesAdded = (int)jsonInfo["LinesAdded"];
                vm.LinesDeleted = (int)jsonInfo["LinesDeleted"];
                vm.AverageTime = new TimeSpan((long)jsonInfo["AverageTimespanTicks"]);  //convert ticks back into timespan
                vm.WatermarkHighlight = determineHighlight(watermarks, ogWatermarks);
                vm.WhitespaceHighlight = determineHighlight(whitespaces, ogWhitespaces);
                vm.OtherWatermark = (string)jsonInfo["OtherWatermark"];
                vm.OtherWatermarkID = (int)jsonInfo["WatermarkMatchID"];
                if(result.dateCommited > assignment.DueDate)
                {
                    vm.DueDateHighlight = "caution";
                }
                else
                {
                    vm.DueDateHighlight = "none";
                }

                listResults.Add(vm);  //and the newly created view model to the list
            }

            return View(listResults);
        }

        public async Task<IActionResult> OtherWatermarkDetails(int? id)
        {
            var result = await _context.Watermarks.FirstOrDefaultAsync(m => m.Id == id);
            if (result == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FirstOrDefaultAsync(s => s.Id == result.StudentID);
            var assignment = await _context.Assignment.FirstOrDefaultAsync(s => s.Id == result.AssignmentID);
            var course = await _context.Course.FirstOrDefaultAsync(c => c.Id == assignment.CourseId);

            List<OtherWatermarkDetailsVM> listdetail = new List<OtherWatermarkDetailsVM>();
            var vm = new OtherWatermarkDetailsVM();

            vm.Watermark = result.Watermark;
            vm.StudentID = result.StudentID;
            vm.AssignmentID = result.AssignmentID;
            vm.FileName = result.FileName;
            vm.StudentRepoName = result.StudentRepoName;
            vm.StudentName = student.FirstName + " " + student.LastName;
            vm.AssignmentName = assignment.Name;
            vm.Course = course.CourseName;
            


            listdetail.Add(vm);

            return View(listdetail);
        }

        //helper method to determine the highlight for a value
        public string determineHighlight( int numerator, int denominator)
        {
            string highlight = "none";
            if( denominator != 0)
            {
                if( numerator / denominator != 1) { highlight = "caution"; }
                if ( (double)numerator / (double)denominator < 0.5 ) { highlight = "danger"; }                
            }
            return highlight;
        }

        // GET: Assignments/ComparisonDetails/
        public async Task<IActionResult> ComparisonDetails(int id, int studentAssignmentId)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var assignment = await _context.Assignment.FirstOrDefaultAsync(m => m.Id == id);
            var studentAssignment = await _context.StudentAssignment.FirstOrDefaultAsync(x => x.Id == studentAssignmentId);
            var student = await _context.Student.FirstOrDefaultAsync(x => x.Id == studentAssignment.StudentId);

            ViewBag.AssignmentName = assignment.Name;
            ViewBag.StudentName = student.FullName;
            string studentURL = studentAssignment.RepositoryUrl;
            ViewBag.RepoUrl = studentURL;

            List<String> shas = new List<string>();
            List<CompareDetailsVM> details = new List<CompareDetailsVM>();
            string token = _configuration["GithubToken"];

            using (var httpClient = new HttpClient())
            {
                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                string studentRepoCommits = $"{studentURL}/commits".Replace("//github.com", "//api.github.com/repos");
                string studentRepoCommitsLimit = studentRepoCommits + "?per_page=40";   //?per_page parameter sets the number of results, default 30 max 100
                var objRepoRequest = new HttpRequestMessage(HttpMethod.Get, studentRepoCommitsLimit);  //if you want to use default limit of 30 just use studentRepoCommits as the second parameter

                using (HttpResponseMessage objRepoResponse = httpClient.SendAsync(objRepoRequest).Result)
                {
                    if (objRepoResponse.IsSuccessStatusCode)
                    {
                        var jsonInfo = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(objRepoResponse.Content.ReadAsStringAsync().Result);

                        for (int i = 0; i < jsonInfo.Count; i++)  //get the sha for each commit
                        {
                            shas.Add(jsonInfo[i]["sha"].ToString());
                        }
                    }
                }

                foreach (String sha in shas)  //for each individual commit get the lines added and deleted
                {
                    string studentRepoCommitURL = String.Format($"{studentRepoCommits}/{sha}");
                    objRepoRequest = new HttpRequestMessage(HttpMethod.Get, studentRepoCommitURL);

                    using (HttpResponseMessage objRepoResponse = httpClient.SendAsync(objRepoRequest).Result)
                    {
                        if (objRepoResponse.IsSuccessStatusCode)
                        {                            
                            var jsonResponse = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(objRepoResponse.Content.ReadAsStringAsync().Result);
                            var files = (Newtonsoft.Json.Linq.JArray)jsonResponse["files"];
                            if (files.Count == 0)  //if there are no files use this to gather the info
                            {
                                CompareDetailsVM detail = new CompareDetailsVM();
                                DateTime tempDate = (DateTime)jsonResponse["commit"]["committer"]["date"];
                                detail.CommitDate = tempDate.ToLocalTime();
                                detail.Message = (string)jsonResponse["commit"]["message"];
                                detail.FileName = "No Files";
                                detail.Additions = (int)jsonResponse["stats"]["additions"];
                                detail.Deletions = (int)jsonResponse["stats"]["deletions"];
                                details.Add(detail);
                            }
                            for (int i = 0; i < files.Count; i++)  //if there are files use this to gather the info
                            {
                                CompareDetailsVM detail = new CompareDetailsVM();
                                DateTime tempDate = (DateTime)jsonResponse["commit"]["committer"]["date"];
                                detail.CommitDate = tempDate.ToLocalTime();
                                detail.Message = (string)jsonResponse["commit"]["message"];
                                detail.FileName = (string)files[i]["filename"];
                                detail.Additions = (int)files[i]["additions"];
                                detail.Deletions = (int)files[i]["deletions"];
                                details.Add(detail);
                            }
                        }
                    }
                }
            }

            return View(details);
        }

        // GET: Assignments/Create
        public IActionResult Create(int? courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        // POST: Assignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,RepositoryUrl,CourseId,JSONCode,DueDate,CanvasLink")] Assignment assignment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync(); var newId = (from x in _context.Assignment //get the generated assignmentId
                                                                select x.Id).Max();

                string canvasLink = String.Format(@"http://{0}/?aID={1}", Request.Host ,newId);  //TODO: when hosted on the server change the url to the correct
                assignment.CanvasLink = canvasLink;
                _context.Update(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction("CourseAssignments", "Courses", new { id = assignment.CourseId });
            }
            return View(assignment);
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id, string from = "")
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }


            assignment.JSONCode = JValue.Parse(assignment.JSONCode).ToString(Formatting.Indented);
            ViewBag.CourseId = assignment.CourseId;
            ViewBag.From = from; // This helps take us back to CourseAssignments if that's where we came from
            return View(assignment);
        }

        // POST: Assignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,RepositoryUrl,CourseId,JSONCode,DueDate,CanvasLink")] Assignment assignment)
        {
            if (id != assignment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(Index));  //use this if you would rather go to the list of all assignments
                return RedirectToAction("CourseAssignments", "Courses", new { id = assignment.CourseId });
            }
            return View(assignment);
        }

        // GET: Assignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignment.FindAsync(id);
            _context.Assignment.Remove(assignment);
            await _context.SaveChangesAsync();
            return RedirectToAction("CourseAssignments", "Courses", new { id = assignment.CourseId });
        }

        // Get: Assignments/StudentRepoForm
        [HttpGet]
        public async Task<IActionResult> StudentRepoForm(int assignmentId, string repoURL)
        {
            var vm = new StudentRepoVM()
            {
                assignmentId = assignmentId,
                RepoURL = "",
                Agreed = ""
            };
            ViewBag.assignmentId = assignmentId;
            ViewBag.repoURL = repoURL;

            return View(vm);
        }

        // Post: Assignments/StudentRepoForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentRepoForm([Bind("assignmentId,RepoURL,Agreed")] StudentRepoVM vm)
        {
            //get the required data from form and cookie
            string studentID;
            int assignmentID = vm.assignmentId;
            if (Request.Cookies.ContainsKey("StudentID"))
            {
                studentID = Request.Cookies["StudentID"];
            }

            // validate student repo format
            string studentRepoURL = vm.RepoURL;
            if (studentRepoURL.EndsWith(".git"))  //if the student has the .git prefix just remove it
            {
                studentRepoURL = studentRepoURL.Replace(".git", "");
            }
            if (String.IsNullOrWhiteSpace(studentRepoURL))  //if no data was entered return an error message
            {
                TempData["error"] = "Error: Please enter your repository";
                return RedirectToAction("StudentRepoForm", "Assignments", new { assignmentId = assignmentID });
            }

            string agreeToRepoRemake = vm.Agreed; 

            //find the assignment and get the sectionID
            var assignment = _context.Assignment.Where(x => x.Id == vm.assignmentId).FirstOrDefault();
            string courseID = assignment.CourseId.ToString();

            //redirect to the studentAssignments function
            return RedirectToAction("StudentAssignments", "StudentInterface", new { assignmentId = assignmentID, courseId = courseID, studentRepoURL = studentRepoURL, agreeToRepoRemake = agreeToRepoRemake });
        }

        public async Task<IActionResult> DownloadAssignment(int courseId)
        {
            var assignments = await _context.Assignment.Where(x => x.CourseId == courseId).ToListAsync();
            return View(assignments);
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignment.Any(e => e.Id == id);
        }
    }
}
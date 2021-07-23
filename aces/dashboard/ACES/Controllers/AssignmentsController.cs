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

namespace ACES.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly ACESContext _context;

        public AssignmentsController(ACESContext context)
        {
            _context = context;
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
                var commits = await _context.Results.Where(x => x.StudentAssignmentId == sAssignment.Id).ToListAsync();
                sAssignment.NumCommits = commits.Count();
                sAssignment.StudentName = student.FullName;                
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

            //join the student assignment table with the student table and the commit table
            var joinCommits = (from sa in _context.StudentAssignment
                               join s in _context.Student on sa.StudentId equals s.Id
                               join c in _context.Commit on sa.Id equals c.StudentAssignmentId
                               select new
                               {
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


        // ************************************************************verify not needed*****************************************
        // GET: Assignments/AssignmentStudentCommits/5
        public async Task<IActionResult> AssignmentStudentCommits(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commits = await _context.Results.Where(x => x.StudentAssignmentId == id).ToListAsync();
            var sAssingment = await _context.StudentAssignment.FirstOrDefaultAsync(x => x.Id == id);
            var assignment = await _context.Assignment.FirstOrDefaultAsync(x => x.Id == sAssingment.AssignmentId);

            var studentName = (await _context.Student.FirstOrDefaultAsync(x => x.Id == sAssingment.StudentId)).FullName;

            var vm = new AssignmentStudentCommitsVM()
            {
                StudentAssignmentId = (int)id,
                StudentName = studentName,
                //NumWatermarks = sAssingment.NumWatermarks,
                Assignment = assignment,
                Commits = commits
            };

            return View(vm);
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

                string canvasLink = String.Format(@"http://localhost:61946/?aID={0}", newId);
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
            if (String.IsNullOrWhiteSpace(studentRepoURL))
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
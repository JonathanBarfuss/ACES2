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
                var commits = await _context.Commit.Where(x => x.StudentAssignmentId == sAssignment.Id).ToListAsync();
                sAssignment.NumCommits = commits.Count();
                sAssignment.StudentName = student.FullName;

                // Get rations
                var watermarkAvg = 0.0;
                var linesModifiedAvg = 0.0;
                var timeBetweenAvg = 0.0;
                var pointsAvg = 0;

                if (commits.Count() > 0)
                {
                    for (var i = 0; i < commits.Count(); i++)
                    {
                        watermarkAvg += commits[i].NumWatermarks;
                        linesModifiedAvg += (commits[i].LinesAdded + commits[i].LinesDeleted);
                        if ((i - 1) > -1)
                        {
                            timeBetweenAvg += (commits[i].DateCommitted - commits[i - 1].DateCommitted).TotalHours;
                        }
                    }
                    watermarkAvg /= commits.Count();
                    linesModifiedAvg /= commits.Count();
                    timeBetweenAvg /= commits.Count();
                    pointsAvg /= commits.Count();
                }

                // Otherwise they are all 0
                //sAssignment.WatermarksRatio = watermarkAvg + "/" + sAssignment.NumWatermarks;
                sAssignment.LinesModifiedAvg = linesModifiedAvg;
                sAssignment.TimeBetweenAvg = timeBetweenAvg;
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

        // GET: Assignments/AssignmentStudentCommits/5
        public async Task<IActionResult> AssignmentStudentCommits(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commits = await _context.Commit.Where(x => x.StudentAssignmentId == id).ToListAsync();
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
        public async Task<IActionResult> StudentRepoForm(int assignmentId)
        {
            var vm = new StudentRepoVM()
            {
                assignmentId = assignmentId,
                RepoURL = ""
            };
            ViewBag.assignmentId = assignmentId;

            return View(vm);
        }

        // Post: Assignments/StudentRepoForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentRepoForm([Bind("assignmentId,RepoURL")] StudentRepoVM vm)
        {
            //get the required data from form and cookie
            string studentID;
            int assignmentID = vm.assignmentId;
            string studentRepoURL = vm.RepoURL;
            if (Request.Cookies.ContainsKey("StudentID"))
            {
                studentID = Request.Cookies["StudentID"];
            }

            //find the assignment and get the sectionID
            var assignment = _context.Assignment.Where(x => x.Id == vm.assignmentId).FirstOrDefault();
            string courseID = assignment.CourseId.ToString();

            //redirect to the studentAssignments function
            return RedirectToAction("StudentAssignments", "StudentInterface", new { assignmentId = assignmentID, courseId = courseID, studentRepoURL = studentRepoURL });
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
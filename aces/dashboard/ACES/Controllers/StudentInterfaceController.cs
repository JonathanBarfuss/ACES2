using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACES.Data;
using ACES.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

namespace ACES.Controllers
{
    public class StudentInterfaceController : Controller
    {
        private readonly ACESContext _context;

        public StudentInterfaceController(ACESContext context)
        {
            _context = context;
        }

        // GET: StudentInterface
        public async Task<IActionResult> Index()
        {
            if (!Request.Cookies.ContainsKey("StudentID"))
            {
                return RedirectToAction("Index", "Login");
            }

            var studentId = int.Parse(Request.Cookies["StudentID"]);
            var enrollments = await _context.Enrollment.Where(x => x.StudentId == studentId).ToListAsync();
            enrollments = enrollments.Where(x => x.Active == true).ToList();    //filter based on active enrollments
            List<Course> coursesList = new List<Course>();
            foreach (var enrollment in enrollments)
            {
                List<Course> temp = await _context.Course.Where(x => x.Id == enrollment.CourseId).ToListAsync();
                foreach (var course in temp) 
                {
                    coursesList.Add(course);
                }
            }
            return View(coursesList);
        }

        // GET: StudentInterface/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: StudentInterface/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: StudentInterface/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseName,InstructorId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: StudentInterface/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: StudentInterface/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseName,InstructorId")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: StudentInterface/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: StudentInterface/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> StudentAssignments(int courseId)
        {
            var assignments = await _context.Assignment.Where(x => x.CourseId == courseId).ToListAsync();
            return View(assignments);
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }


        [HttpGet]
        public IActionResult DownloadAssignment(int assignmentId) //TODO: assignmentId
        {
            // Get assignment's url and name from Assignments table:
            var assignment = _context.Assignment.Where(x => x.Id == assignmentId).FirstOrDefault();
            string assignmentUrl = assignment.RepositoryUrl.ToString(); // To test, update Assignment table in DB with relevant Url
            string assignmentName = $"{assignment.Name.Replace(" ", "_")}_";

            // Get student's email and Id:
            Request.Cookies.TryGetValue("StudentEmail", out string studentEmail);
            Request.Cookies.TryGetValue("StudentId", out string strStudentId);
            Int32.TryParse(strStudentId, out int studentId);
            string student_assignment_watermark = String.Empty;
            // Get existing watermark if student already downloaded this assignment earlier
            var studentAssignment = _context.StudentAssignment.Where(x => x.StudentId == studentId && x.AssignmentId == assignmentId).FirstOrDefault();
            if (studentAssignment != null)
            {
                student_assignment_watermark = studentAssignment.Watermark;
            }

            // Initiate Post API Call to uniquely watermark the downloading assignment for a student
            using (var httpClient = new HttpClient())
            {
                string path = "http://localhost:8080"; //TODO: replace with Brad's link to cs website, e.g. cs.weber.bradley...
                var objRequest = new HttpRequestMessage(HttpMethod.Post, path);

                // Populate request content to pass to the server
                objRequest.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new PostAddWatermark()
                {
                    directory = assignmentUrl,
                    email = studentEmail,
                    asn_no = assignmentName,
                    existing_watermark = student_assignment_watermark
                }));

                // Check response
                using (HttpResponseMessage objResponse = httpClient.SendAsync(objRequest).Result)
                {
                    if (objResponse.IsSuccessStatusCode)
                    {
                        // Parse response content
                        var deserializedObject = JsonConvert.DeserializeObject<GetWatermarkedAssignment>(objResponse.Content.ReadAsStringAsync().Result);
                        if (deserializedObject == null)
                        {
                            //TODO: pop-up user message
                        }
                        string assignment_watermark = deserializedObject.watermark;
                        int assignment_watermark_count = deserializedObject.watermark_count;
                        string zipped_directory = deserializedObject.zipped_directory;

                        // If first download, store the downloading assignment data in StudentAssignment table in the DB:
                        if (student_assignment_watermark == "")
                        {
                            var newStudentAssignment = new StudentAssignment()
                            {
                                StudentId = studentId,
                                AssignmentId = assignmentId,
                                Watermark = assignment_watermark,
                                RepositoryUrl = zipped_directory,
                                NumWatermarks = assignment_watermark_count
                            }; // don't store repositoryUrl on download
                            _context.StudentAssignment.Add(newStudentAssignment);
                        }
                        else
                        {
                            studentAssignment.RepositoryUrl = zipped_directory;
                            studentAssignment.NumWatermarks = assignment_watermark_count;
                        }
                        _context.SaveChanges(); //save changes should only be done once

                        // Download zipped file to the student's browser
                        var net = new System.Net.WebClient();
                        var data = net.DownloadData($"{path}/{deserializedObject.zipped_directory}");
                        var content = new System.IO.MemoryStream(data);
                        var contentType = "APPLICATION/octet-stream";
                        var fileName = $"{assignmentName}prepared.zip";
                        return File(content, contentType, fileName);
                    }
                    else
                    {
                        //TODO: pop-up user message
                        //ViewBag.Message = "Unsuccessful download attempt";
                        var assignments = _context.Assignment.Where(x => x.CourseId == assignment.CourseId).ToListAsync();
                        return View(assignments);
                    }
                }
            }
        }     
    }  

    public class GetWatermarkedAssignment
    {
        public string zipped_directory { get; set; }
        public string watermark { get; set; }
        public int watermark_count { get; set; }
    }

    public class PostAddWatermark
    {
        public string directory { get; set; }
        public string email { get; set; }
        public string asn_no { get; set; }
        public string existing_watermark { get; set; }
    }
}

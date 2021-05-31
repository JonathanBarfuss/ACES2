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
        public IActionResult DownloadAssignment(int courseId) //TODO: assignmentId
        { 
            // get assignment's url and name from Assignments table:
            var assignment = _context.Assignment.Where(x => x.Id == courseId).FirstOrDefault();
            string assignmentUrl = assignment.RepositoryUrl.ToString();
            string assignmentName = $"{assignment.Name.Replace(" ", "_")}_";

            // get student email:
            Request.Cookies.TryGetValue("StudentEmail", out string studentEmail);

            using (var httpClient = new HttpClient())
            {               

                var objRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080");//cs.weber.bradley...
                
                //string strDirectory = "C:/Users/diliy/source/repos/ACES/aces/assignments/samples/c_asn";
                objRequest.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new PostAddWatermark() 
                {
                    directory = assignmentUrl, email = studentEmail, asn_no = assignmentName 
                }));
                using (HttpResponseMessage objResponse = httpClient.SendAsync(objRequest).Result)
                {
                    if (objResponse.IsSuccessStatusCode)
                    {
                        var deserializedObject = JsonConvert.DeserializeObject<GetWatermarkedAssignment>(objResponse.Content.ReadAsStringAsync().Result);
                        string assignment_watermark = deserializedObject.watermark; //TODO: store in the DB
                        int assignment_watermark_count = deserializedObject.watermark_count; //TODO: store in the DB

                        var net = new System.Net.WebClient();
                        var data = net.DownloadData($"http://localhost:8080/{deserializedObject.zipped_directory}");
                        var content = new System.IO.MemoryStream(data);
                        var contentType = "APPLICATION/octet-stream";
                        var fileName = $"{assignmentName}prepared.zip";
                        return File(content, contentType, fileName);
                    }
                    else
                    {
                        return null;

                        //TODO: pop-up user message
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
    }
}

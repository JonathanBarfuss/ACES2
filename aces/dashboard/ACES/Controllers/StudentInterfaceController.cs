using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACES.Data;
using ACES.Models;
using System.Runtime.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json; //Remove
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;

namespace ACES.Controllers
{
    public class StudentInterfaceController : Controller
    {
        private static readonly HttpClient client = new HttpClient(); //TODO
        private readonly ACESContext _context;

        public StudentInterfaceController(ACESContext context)
        {
            _context = context;
        }

        // GET: StudentInterface
        public async Task<IActionResult> Index()
        {
            // request.Cookies["UserID"]
            var studentId = int.Parse(Request.Cookies["UserID"]);
            var enrollments = await _context.Enrollment.Where(x => x.StudentId == studentId).ToListAsync();
            List<Course> coursesList = new List<Course>();
            foreach (var enrollment in enrollments)
            {
                List<Course> temp = await _context.Course.Where(x => x.Id == enrollment.CourseId).ToListAsync();
                foreach (var course in temp) // should we filter on active enrollement only? Why student needs to download inactive enr course assignment?
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
        public IActionResult DownloadAssignment(int id)
        { 
            //TODO: grab url for assign from Assignments table in db

            using (var httpClient = new HttpClient())
            {               

                var objRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080");//cs.weber.bradley...
                
                string strDirectory = "C:/Users/diliy/source/repos/ACES/aces/assignments/samples/c_asn";
                objRequest.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new PostContent() { directory = strDirectory, email = "joshuaabbott@mail.weber.edu", asn_no = "test_asn" }));
                using (HttpResponseMessage objResponse = httpClient.SendAsync(objRequest).Result)
                {
                    if (objResponse.IsSuccessStatusCode)
                    {
                        var deserializedObject = JsonConvert.DeserializeObject<Root>(objResponse.Content.ReadAsStringAsync().Result);

                        var net = new System.Net.WebClient();
                        var data = net.DownloadData($"http://localhost:8080/{deserializedObject.zipped_directory}");
                        var content = new System.IO.MemoryStream(data);
                        var contentType = "APPLICATION/octet-stream";
                        var fileName = "test_asnprepared.zip";
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

    public class Root
    {
        public string zipped_directory { get; set; }
    }

    public class PostContent
    {
        public string directory { get; set; }
        public string email { get; set; }
        public string asn_no { get; set; }
    }
}

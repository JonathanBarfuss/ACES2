using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACES.Data;
using ACES.Models;
using System.Net.Http;
//using System.Web.Http;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

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
                List<Course> temp = await _context.Course.Where(x => x.Id == enrollment.SectionId).ToListAsync();
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

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> StudentAssignments(int assignmentId, int sectionId) 
        {

            if (assignmentId == 0)
            {
                var courseAssignments = await _context.Assignment.Where(x => x.SectionId == sectionId).ToListAsync();
                return View(courseAssignments);
            }
            
            // Get assignment's url and name from Assignments table:
            var assignment = _context.Assignment.Where(x => x.Id == assignmentId).FirstOrDefault();
            string assignmentUrl = assignment.RepositoryUrl.ToString(); // To test, update Assignment table in DB with relevant Url
            string assignmentName = $"{assignment.Name.Replace(" ", "_")}_";

            // Get student's email and Id:
            Request.Cookies.TryGetValue("StudentEmail", out string studentEmail);
            Request.Cookies.TryGetValue("StudentId", out string strStudentId);
            Int32.TryParse(strStudentId, out int studentId);
            string student_assignment_watermark = String.Empty;
            int whitespace_watermark = -1;

            // Get existing watermark if student already downloaded this assignment earlier
            var studentAssignment = _context.StudentAssignment.Where(x => x.StudentId == studentId && x.AssignmentId == assignmentId).FirstOrDefault();
            if (studentAssignment != null)
            {
                JObject json = JObject.Parse(studentAssignment.JSONCode);
                student_assignment_watermark = json.SelectToken("watermark").Value<string>();
                whitespace_watermark = json.SelectToken("whitespace_count").Value<int>();
            }

            //TODO: Add code to clone instructor's repo for a student
            var contentsRepoUrl = $"https://api.github.com/repos/AntiCheatSummer2021/assignment4-ShaneyPooh/contents";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer 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");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App"); // TODO: name of appl: ACES
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                var objRequest1 = new HttpRequestMessage(HttpMethod.Get, contentsRepoUrl);

                // Check response
                using (HttpResponseMessage objResponse = httpClient.SendAsync(objRequest1).Result)
                {
                    if (objResponse.IsSuccessStatusCode)
                    {
                        FileInfo[] contents = JsonConvert.DeserializeObject<FileInfo[]>(objResponse.Content.ReadAsStringAsync().Result);
                        foreach (var file in contents)
                        {
                            var fileType = file.type;

                            if (file.type == "dir")
                            {
                                var directoryContentsUrl = file.url;
                                // use this URL to list the contents of the folder
                            }
                            else if (file.type == "file")
                            {

                                HttpRequestMessage fileGetRequest = new HttpRequestMessage(HttpMethod.Get, file.download_url);
                                fileGetRequest.Headers.Add("Authorization", "Bearer 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");
                                HttpResponseMessage fileGetResponse = httpClient.SendAsync(fileGetRequest).Result;
                                string content = fileGetResponse.Content.ReadAsStringAsync().Result;
                                fileGetResponse.Dispose();

                                StreamWriter sr = new StreamWriter("../../assignments/temp/" + file.name);
                                sr.Write(content);
                                sr.Close();

                            }
                        }


                        string path = "http://localhost:61946/factory"; //TODO: replace with Brad's link to cs website, e.g. cs.weber.bradley...
                        var objRequest2 = new HttpRequestMessage(HttpMethod.Post, path);

                        //StreamReader r = new StreamReader("../../assignments/samples/c_asn/.acesconfig.json");
                        //var jsonFile = r.ReadToEnd();

                        var json = System.Text.Json.JsonSerializer.Serialize(new PostAddWatermark()
                        {
                            directory = "../../assignments/temp",
                            email = studentEmail,
                            asn_no = assignmentName,
                            existing_watermark = student_assignment_watermark,
                            whitespaces = whitespace_watermark,
                            jsonCode = assignment.JSONCode
                            //jsonCode = jsonFile

                        });

                        // Populate request content to pass to the server
                        objRequest2.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        // Check response
                        using (HttpResponseMessage objResponse2 = httpClient.SendAsync(objRequest2).Result)
                        {
                            if (objResponse.IsSuccessStatusCode)
                            {
                                // Parse response content
                                var deserializedObject = JsonConvert.DeserializeObject<GetWatermarkedAssignment>(objResponse2.Content.ReadAsStringAsync().Result);

                                // formats watermark data in a json to store in the database
                                var assignmentJSON = System.Text.Json.JsonSerializer.Serialize(new GetWatermarkedAssignment()
                                {
                                    whitespace_count = deserializedObject.whitespace_count,
                                    watermark = deserializedObject.watermark,
                                    watermark_count = deserializedObject.watermark_count,
                                    fileNames = deserializedObject.fileNames
                                });

                                // If first download, store the downloading assignment data in StudentAssignment table in the DB:
                                if (studentAssignment == null)
                                {
                                    var newStudentAssignment = new StudentAssignment()
                                    {
                                        StudentId = studentId,
                                        AssignmentId = assignmentId,
                                        RepositoryUrl = contentsRepoUrl,  //Change this to the actual student repo url
                                        JSONCode = assignmentJSON
                                    };
                                    _context.StudentAssignment.Add(newStudentAssignment);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    studentAssignment.RepositoryUrl = contentsRepoUrl;
                                    studentAssignment.JSONCode = assignmentJSON;
                                    _context.SaveChanges();
                                }

                                string[] tempFiles = System.IO.Directory.GetFiles("../../assignments/temp");
                                string[] temp2Files = System.IO.Directory.GetFiles("../../assignments/temp2");
                                List<string> fileContents = new List<string>();

                                foreach(var file in temp2Files)
                                {

                                    StreamReader sr = new StreamReader(file);
                                    var newFile = sr.ReadToEnd();
                                    fileContents.Add(newFile);
                                    sr.Close();

                                }

                                var count = 0;

                                foreach(var file in contents) {

                                    if (deserializedObject.fileNames[count] == file.name)
                                    {
                                        //call api to put file to student's repo
                                        HttpRequestMessage filePutRequest = new HttpRequestMessage(HttpMethod.Put, file.url);
                                        filePutRequest.Headers.Add("Authorization", "Bearer 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");
                                        filePutRequest.Content = new StringContent(JsonConvert.SerializeObject(new PutBody { message = "Added watermark", content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContents[count])), sha = file.sha }), Encoding.UTF8, "application/json");
                                        HttpResponseMessage filePutResponse = httpClient.SendAsync(filePutRequest).Result;
                                        if (filePutResponse.IsSuccessStatusCode)
                                        {
                                            //TODO: display confirmation to a student?
                                        }
                                        else
                                        {
                                            //TODO: display an error message to a student
                                        }
                                        filePutResponse.Dispose();
                                        count++;
                                    }

                                }

                                foreach(var file in tempFiles)
                                {

                                    System.IO.File.Delete(file);

                                }
                                foreach (var file in temp2Files)
                                {

                                    System.IO.File.Delete(file);

                                }

                            }
                            else
                            {

                                //Give error

                            }

                        }

                    }
                }
            }

            var assignments = await _context.Assignment.Where(x => x.SectionId == sectionId).ToListAsync();
            return View(assignments);
     
        }
    }

    //JSON structures
    struct PutBody
    {
        public String message;
        public String content;
        public String sha;
    }

    struct LinkFields
    {
        public String self;
    }
    struct FileInfo
    {
        public String name;
        public String type;
        public String sha;
        public String download_url;
        public String url;
    }

    public struct FileData
    {
        public String name;
        public String contents;
    }
    public struct Directory
    {
        public String name;
        public List<Directory> subDirs;
        public List<FileData> files;
    }

    public struct GetWatermarkedAssignment
    {
        // add field for repo name
        public int whitespace_count { get; set; }
        public string watermark { get; set; }
        public int watermark_count { get; set; }
        public List<string> fileNames { get; set; }
    }

    public struct PostAddWatermark
    {
        public string directory { get; set; }
        public string email { get; set; }
        public string asn_no { get; set; }
        public string existing_watermark { get; set; }
        public int whitespaces { get; set; }
        public string jsonCode { get; set; }
    }
}

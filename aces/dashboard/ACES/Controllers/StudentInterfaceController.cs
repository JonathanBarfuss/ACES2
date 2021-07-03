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
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace ACES.Controllers
{
    public class StudentInterfaceController : Controller
    {

        private readonly ACESContext _context;

        public StudentInterfaceController(ACESContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        // GET: StudentInterface/Create
        public IActionResult Create()
        {
            return View();
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }

        #region GET: StudentInterface
        public async Task<IActionResult> Index(string? message)
        {
            if (!Request.Cookies.ContainsKey("StudentID"))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!String.IsNullOrEmpty(message))  //check if an alert message is being passed
            {
                ViewBag.IsMessage = "true";
                ViewBag.Message = message;
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
        #endregion

        #region GET: StudentInterface/Details/5
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
        #endregion

        #region POST: StudentInterface/Create
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
        #endregion

        #region GET: StudentInterface/Edit/5
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
        #endregion

        #region POST: StudentInterface/Edit/5
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
        #endregion

        #region Delete & DeleteConfirmed
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
        #endregion        

        #region StudentAssignments
        [HttpGet]
        public async Task<IActionResult> StudentAssignments(int assignmentId, int courseId, string studentRepoURL) 
        {

            if(studentRepoURL == null)
            {

                studentRepoURL = "https://github.com/AntiCheatSummer2021/assignment4-ShaneyPooh";

            }


            if (assignmentId == 0)
            {
                var courseAssignments = await _context.Assignment.Where(x => x.CourseId == courseId).ToListAsync();
                return View(courseAssignments);
            }

            // Get assignment's url and name from Assignments table:
            var assignment = _context.Assignment.Where(x => x.Id == assignmentId).FirstOrDefault();
            string insructorAssignmentRepoUrl = assignment.RepositoryUrl.ToString(); 
            string assignmentName = $"{assignment.Name.Replace(" ", "_")}_";
            string token = _configuration["GithubToken"];

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

            //TODO: get student url from the field the students enters the repo name into
            //var studentRepoURL = "https://github.com/AntiCheatSummer2021/brad-assignment-ShaneyPooh";
            using (var httpClient = new HttpClient())
            {

                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                string instructorRepoContents = $"{insructorAssignmentRepoUrl}/contents".Replace("//github.com", "//api.github.com/repos");
                var objRequest1 = new HttpRequestMessage(HttpMethod.Get, instructorRepoContents);

                // Check response
                using (HttpResponseMessage objResponse = httpClient.SendAsync(objRequest1).Result)
                {

                    if (objResponse.IsSuccessStatusCode)
                    {

                        #region Get Files From Repository/Put them in temp files
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
                                //Get file from repository contents
                                HttpRequestMessage fileGetRequest = new HttpRequestMessage(HttpMethod.Get, file.download_url);
                                fileGetRequest.Headers.Add("Authorization", "Bearer " + token);
                                HttpResponseMessage fileGetResponse = httpClient.SendAsync(fileGetRequest).Result;
                                string content = fileGetResponse.Content.ReadAsStringAsync().Result;
                                fileGetResponse.Dispose();

                                System.IO.Directory.CreateDirectory("../../assignments/temp/");
                                System.IO.Directory.CreateDirectory("../../assignments/temp2/");

                                //Write the content from GitHub file to a temp file in project temp folder
                                StreamWriter sr = new StreamWriter("../../assignments/temp/" + file.name);
                                sr.Write(content);
                                sr.Close();

                            }
                        }
                        #endregion

                        #region Gather Needed Info to Pass to Factory
                        var objRequest2 = new HttpRequestMessage(HttpMethod.Post, "http://localhost:61946/factory"); //TODO: replace path with Brad's link to cs website, e.g. cs.weber.bradley... // should still use localhost, otherwise needs to do dns routing, going to internet, call does not go out

                        //Gather JSON info to pass to Factory
                        var json = System.Text.Json.JsonSerializer.Serialize(new PostAddWatermark()
                        {

                            directory = "../../assignments/temp",
                            email = studentEmail,
                            asn_no = assignmentName,
                            existing_watermark = student_assignment_watermark,
                            whitespaces = whitespace_watermark,
                            jsonCode = assignment.JSONCode

                        });

                        // Populate request content to pass to the server
                        objRequest2.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        #endregion

                        #region Pass Gathered Info to Factory/Check Response Content
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
                                        RepositoryUrl = studentRepoURL,  
                                        JSONCode = assignmentJSON
                                    };
                                    _context.StudentAssignment.Add(newStudentAssignment);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    studentAssignment.RepositoryUrl = studentRepoURL;
                                    studentAssignment.JSONCode = assignmentJSON;
                                    _context.SaveChanges();
                                }


                                DirectoryInfo di = new DirectoryInfo("../../assignments/temp/");
                                StreamReader sr = null;
                                var firstFile = "";

                                var count = 0; //index variable to access items from List

                                //This loop adds all watermarked files stored in List back to GitHub
                                foreach(var file in contents) {                                   

                                    if (file.name == "README.md")
                                    {

                                        firstFile = "README.md";
                                        sr = new StreamReader("../../assignments/temp/" + firstFile);

                                    }
                                    else
                                    {

                                        firstFile = di.EnumerateFiles()
                                                      .Select(f => f.Name)
                                                      .FirstOrDefault();
                                        sr = new StreamReader("../../assignments/temp/" + firstFile);

                                    }
                                    
                                    var fileContent = sr.ReadToEnd();
                                    sr.Close();

                                    //call api to put file to student's repo
                                    string studentPartName = studentRepoURL.Substring(studentRepoURL.LastIndexOf("/") + 1);
                                    string studentAssignmentRepoUrl = file.url.Replace("Template1", studentPartName);
                                    HttpRequestMessage filePutRequest = new HttpRequestMessage(HttpMethod.Put, studentAssignmentRepoUrl);

                                    filePutRequest.Headers.Add("Authorization", "Bearer " + token);
                                    filePutRequest.Content = new StringContent(JsonConvert.SerializeObject(new PutBody { message = "Add assignment file(s)", content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent)), sha = file.sha }), Encoding.UTF8, "application/json");
                                    HttpResponseMessage filePutResponse = httpClient.SendAsync(filePutRequest).Result;

                                    if (filePutResponse.IsSuccessStatusCode)
                                    {
                                        //TODO: display confirmation to a student?
                                        filePutResponse.Dispose();
                                        count++;
                                        System.IO.File.Delete("../../assignments/temp/" + firstFile);

                                    }
                                    else
                                    {
                                        //TODO: display an error message to a student
                                        string error = String.Format("Error: Repository for assignment {0} has not been updated", assignment.Name);
                                        return RedirectToAction("Index", "StudentInterface", new { message = error });
                                    }                                  
                                
                                }

                                string confirm = String.Format("Your Repository for assignment {0} has been updated", assignment.Name);
                                return RedirectToAction("Index", "StudentInterface", new { message = confirm });

                            }
                            else
                            {
                                //Give error
                                string error = String.Format("Error: Repository for assignment {0} has not been updated", assignment.Name);
                                return RedirectToAction("Index", "StudentInterface", new { message = error });
                            }

                        }
                        #endregion

                    }
                    else
                    {
                        //Give error
                        string error = String.Format("Error: Repository for assignment {0} has not been updated", assignment.Name);
                        return RedirectToAction("Index", "StudentInterface", new { message = error });
                    }

                }

            }

            var assignments = await _context.Assignment.Where(x => x.CourseId == courseId).ToListAsync();
            return View(assignments);
     
        }
        #endregion

    }

    #region JSON Structures
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
    #endregion

}

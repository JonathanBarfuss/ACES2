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
using System.Text;
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
            var enrollments = await _context.Enrollment.Where(x => x.StudentId == studentId).ToListAsync();  //get the active student's enrollments

            List < Course > coursesList = new List<Course>();
            foreach (var enrollment in enrollments)
            {
                List<Course> temp = await _context.Course.Where(x => x.Id == enrollment.CourseId).ToListAsync();
                temp = temp.Where(x => x.IsCourseActive == true).ToList();  //filter based on active courses
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
        public async Task<IActionResult> StudentAssignments(int assignmentId, int courseId, string studentRepoURL, string agreeToRepoRemake) 
        {          
            if (assignmentId == 0)
            {
                var courseAssignments = await _context.Assignment.Where(x => x.CourseId == courseId).ToListAsync();
                return View(courseAssignments);
            }

            //TODO: modify to validate (first check for empty repo entry, second check for invalid repo)
            //TODO: and to display error to a student to enter the valid repo string, also provide an example above the field!
            if (studentRepoURL == null)
            {
                studentRepoURL = "https://github.com/AntiCheatSummer2021/assignment4-ShaneyPooh";
                //studentRepoURL = "https://github.com/AntiCheatSummer2021/brad-assignment-ShaneyPooh";
            }

            // Get assignment's url and name from Assignments table:
            var assignment = _context.Assignment.Where(x => x.Id == assignmentId).FirstOrDefault();
            string insructorAssignmentRepoUrl = assignment.RepositoryUrl.ToString();
            string assignmentName = $"{assignment.Name.Replace(" ", "_")}_";
            string token = _configuration["GithubToken"];
            bool isRemake = false;
            dynamic objAssignmentJson = Newtonsoft.Json.JsonConvert.DeserializeObject(assignment.JSONCode);
       
            // Get student's email and Id:
            Request.Cookies.TryGetValue("StudentEmail", out string studentEmail);
            Request.Cookies.TryGetValue("StudentId", out string strStudentId);
            Int32.TryParse(strStudentId, out int studentId);

            // Stop downloading assignment if student already downloaded this assignment earlier
            var studentAssignment = _context.StudentAssignment.Where(x => x.StudentId == studentId && x.AssignmentId == assignmentId).FirstOrDefault();
            if (studentAssignment != null)
            {
                if (agreeToRepoRemake != "Agreed" && agreeToRepoRemake != "Override")
                {
                    TempData["error"] = "This Repo already has the assignment files. Click Home to exit.";
                    TempData["warning"] = "Click the button below to confirm. Warning: it will clear your repo from all files, including any prior code you committed:";
                    return RedirectToAction("StudentRepoForm", "Assignments", new { assignmentId = studentAssignment.AssignmentId, repoURL = studentRepoURL });
                }
                else
                {
                    isRemake = true;
                    if (agreeToRepoRemake != "Override")
                    {
                        using (var httpClient = new HttpClient())
                        {
                            //Set up Header info to request files for deletion from the student's GitHub repo
                            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                            httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                            string studentRepoContents = $"{studentAssignment.RepositoryUrl}/contents".Replace("//github.com", "//api.github.com/repos");
                            var objStudentRepoRequest = new HttpRequestMessage(HttpMethod.Get, studentRepoContents);

                            // Get response
                            using (HttpResponseMessage objStudentRepoResponse = httpClient.SendAsync(objStudentRepoRequest).Result)
                            {
                                if (objStudentRepoResponse.IsSuccessStatusCode)
                                {
                                    FileInfo[] contents = JsonConvert.DeserializeObject<FileInfo[]>(objStudentRepoResponse.Content.ReadAsStringAsync().Result);
                                    foreach (var file in contents)
                                    {
                                        var fileType = file.type;

                                        if (file.type == "dir")
                                        {
                                            var directoryContentsUrl = file.url;
                                            // future enhancement: use this URL to get the contents of the folder if there are folders in addition to files
                                        }
                                        else if (file.type == "file")
                                        {
                                            // Get file's sha from student's repository contents, and delete the file
                                            HttpRequestMessage fileDeleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"{studentRepoContents}/{file.name}");
                                            fileDeleteRequest.Headers.Add("Authorization", "Bearer " + token);

                                            var bodyDeleteFile = System.Text.Json.JsonSerializer.Serialize(new DeleteFile()
                                            {
                                                message = $"Deleting the file {file.name} per the student's request from their repo: {studentAssignment.RepositoryUrl}",
                                                sha = file.sha
                                            });

                                            fileDeleteRequest.Content = new StringContent(bodyDeleteFile, Encoding.UTF8, "application/json");

                                            HttpResponseMessage fileDeleteResponse = httpClient.SendAsync(fileDeleteRequest).Result;
                                            if (!fileDeleteResponse.IsSuccessStatusCode)
                                            {
                                                //Display error message? Retry again?
                                                TempData["errorDeletingFiles"] = $"Error: unable to delete files from repository {studentAssignment.RepositoryUrl}";
                                                return RedirectToAction("StudentRepoForm", "Assignments", new { assignmentId = studentAssignment.AssignmentId, repoURL = studentRepoURL });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Declare studentMarkedfiles list to start adding marked files data that later will go to StudentAssignmentJson object for storing in the database in the StudentAssignment table in the JSONCode column           
            List<StudentMarkedFile> studentMarkedfiles = new List<StudentMarkedFile>();
            var studentAssignmentJson = new StudentAssignmentJson() { };            

            // Start GitHub API call to get files from instructor's repo
            using (var httpClient = new HttpClient())
            {
                //Set up Header info to request files from GitHub
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ACES");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                string instructorRepoContents = $"{insructorAssignmentRepoUrl}/contents".Replace("//github.com", "//api.github.com/repos");
                var objInstructorRepoRequest = new HttpRequestMessage(HttpMethod.Get, instructorRepoContents);

                // Get response
                using (HttpResponseMessage objInstructorRepoResponse = httpClient.SendAsync(objInstructorRepoRequest).Result)
                {
                    if (objInstructorRepoResponse.IsSuccessStatusCode)
                    {
                        #region Get Files From Instructor's Repository/Prep for sending to Factory
                        string whiteComment;
                        FileInfo[] contents = JsonConvert.DeserializeObject<FileInfo[]>(objInstructorRepoResponse.Content.ReadAsStringAsync().Result);
                        foreach (var file in contents)
                        {
                            var fileType = file.type;

                            if (file.type == "dir")
                            {
                                var directoryContentsUrl = file.url;
                                // future enhancement: use this URL to get the contents of the folder if instructor uses folders in addition to files
                            }
                            else if (file.type == "file")
                            {
                                // Get file from instructor's repository contents, customize it if needed
                                // and put it into student's repo
                                HttpRequestMessage fileGetRequest = new HttpRequestMessage(HttpMethod.Get, file.download_url);
                                fileGetRequest.Headers.Add("Authorization", "Bearer " + token);
                                HttpResponseMessage fileGetResponse = httpClient.SendAsync(fileGetRequest).Result;
                                string content = fileGetResponse.Content.ReadAsStringAsync().Result;
                                fileGetResponse.Dispose();

                                // Check if file is part of JSON instructions for watermarking.
                                foreach (var fileInJson in objAssignmentJson.files)
                                {
                                    if (fileInJson.name.Value == file.name)
                                    {
                                        // If yes, send the file to factory with Json object.
                                        #region Gather Needed Info to Pass to Factory
                                        bool replaceInsteadOfInsert = fileInJson.replaceInsteadOfInsert != null && fileInJson.replaceInsteadOfInsert.Value == 1;
                                        if(fileInJson.comment == null)
                                        {
                                            whiteComment = "no comment";
                                        } else
                                        {
                                            whiteComment = fileInJson.comment.Value;
                                        }
                                        var fileInstructions = System.Text.Json.JsonSerializer.Serialize(new PostAddWatermark()
                                        {
                                            email = studentEmail,
                                            assignmentName = assignmentName,
                                            fileName = fileInJson.name.Value,
                                            replaceInsteadOfInsert = replaceInsteadOfInsert,
                                            comment = whiteComment,
                                            whitespacesLineNumbers = fileInJson.whitespaces.ToObject<int[]>(),
                                            randomStringLineNumbers = fileInJson.randomstring.ToObject<int[]>(),
                                            fileContent = content
                                        });
                                        #endregion

                                        #region Pass Gathered Info to Factory/Check Response Content
                                        var objFactoryRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:61946/factory"); //TODO: replace path with Brad's link to cs website, e.g. cs.weber.bradley...// should still use localhost, otherwise needs to do dns routing, going to internet, vs call does not go out? If used outside, that app will put a full path
                                         objFactoryRequest.Content = new StringContent(fileInstructions, Encoding.UTF8, "application/json");
                                        // Check response
                                        using (HttpResponseMessage objFactoryResponse = httpClient.SendAsync(objFactoryRequest).Result)
                                        {
                                            if (objInstructorRepoResponse.IsSuccessStatusCode)
                                            {
                                                // Parse response content
                                                var deserializedObject = JsonConvert.DeserializeObject<GetWatermarkedAssignment>(objFactoryResponse.Content.ReadAsStringAsync().Result);
                                                content = deserializedObject.markedFileContent; // update content for relevant file                                             

                                                // Add a new StudentMarkedFile object to the studentMarkedfiles list to be added to the database later
                                                studentMarkedfiles.Add(new StudentMarkedFile
                                                {
                                                    fileName = fileInJson.name.Value,
                                                    numberOfLinesInFile = (int)fileInJson.lines.Value,
                                                    watermark = deserializedObject.watermark,
                                                    comment = deserializedObject.comment,
                                                    numberOfWhitespaceCharacters = deserializedObject.numberOfWhitespaceCharacters,
                                                    whitespacesLineNumbers = fileInJson.whitespaces.ToObject<int[]>(),
                                                    randomStringLineNumbers = fileInJson.randomstring.ToObject<int[]>()
                                                });

                                                // Add each watermark to the database
                                                var newWatermark = new Watermarks
                                                {

                                                    Watermark = deserializedObject.watermark,
                                                    StudentID = studentId,
                                                    AssignmentID = assignmentId,
                                                    FileName = fileInJson.name.Value,
                                                    StudentRepoName = studentRepoURL

                                                };

                                                _context.Watermarks.Add(newWatermark);
                                                _context.SaveChanges();

                                            }
                                            else
                                            {
                                                string error = String.Format("Error: Repository for assignment {0} has not been updated", assignment.Name);
                                                return RedirectToAction("Index", "StudentInterface", new { message = error });
                                            }
                                        }
                                        #endregion

                                        break;
                                    }
                                }                                

                                #region Upload file to student's repo
                                // Upload each file (we are still in the files loop) to student's repo,
                                // regardless of whether this file is customized or not requiring customization, e.g. README.md
                                string studentPartName = studentRepoURL.Substring(studentRepoURL.LastIndexOf("github.com/") + 11);
                                int startReplaceIndex = file.url.LastIndexOf("repos/") + 6;
                                int lengthToReplace = file.url.LastIndexOf("/contents") - startReplaceIndex;
                                string studentAssignmentRepoUrl = file.url.Replace(file.url.Substring(startReplaceIndex, lengthToReplace), studentPartName);
                                HttpRequestMessage filePutToStudentRepoRequest = new HttpRequestMessage(HttpMethod.Put, studentAssignmentRepoUrl);

                                filePutToStudentRepoRequest.Headers.Add("Authorization", "Bearer " + token);
                                filePutToStudentRepoRequest.Content = new StringContent(JsonConvert.SerializeObject(new PutBody { message = "Add assignment file(s)", content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)), sha = file.sha }), Encoding.UTF8, "application/json");
                                HttpResponseMessage filePutToStudentRepoResponse = httpClient.SendAsync(filePutToStudentRepoRequest).Result;

                                if (filePutToStudentRepoResponse.IsSuccessStatusCode)
                                {
                                    filePutToStudentRepoResponse.Dispose();                                    
                                }
                                else
                                {
                                    string error = String.Format("Error: Repository for assignment {0} has not been updated, status code: {1}", assignment.Name, filePutToStudentRepoResponse.StatusCode);
                                    return RedirectToAction("Index", "StudentInterface", new { message = error });
                                }
                                #endregion
                            }
                        }

                        //Finalize studentAssignmentJson object before storing it in the database                        
                        studentAssignmentJson.files = studentMarkedfiles;
                        var jsonString = System.Text.Json.JsonSerializer.Serialize(studentAssignmentJson);

                        // If first download, store the downloading assignment data in StudentAssignment table in the DB:
                        if (studentAssignment == null)
                        {
                            var newStudentAssignment = new StudentAssignment()
                            {
                                StudentId = studentId,
                                AssignmentId = assignmentId,
                                RepositoryUrl = studentRepoURL,
                                JSONCode = jsonString
                            };
                            _context.StudentAssignment.Add(newStudentAssignment);
                            _context.SaveChanges();
                        }
                        // if Remake, just update the JSONCode, all other columns should be the same
                        else if (isRemake)
                        {
                            studentAssignment.JSONCode = jsonString;
                            _context.SaveChanges();
                        }

                        string confirm = String.Format("Your Repository for assignment {0} has been updated", assignment.Name);
                        return RedirectToAction("Index", "StudentInterface", new { message = confirm });

                        #endregion                     
                    }
                    else
                    {
                        string error = String.Format("Error: Repository for assignment {0} has not been updated", assignment.Name);
                        return RedirectToAction("Index", "StudentInterface", new { message = error });
                    }
                }
            }
  
        }
        #endregion
    }

    #region JSON/Other Structures
    struct PutBody
    {
        public String message;
        public String content;
        public String sha;
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
    public struct DeleteFile
    {
        public string message { get; set; }
        public string sha { get; set; }
    }
    public struct GetWatermarkedAssignment
    {
        public string comment { get; set; }
        public int numberOfWhitespaceCharacters { get; set; }
        public string watermark { get; set; }
        public string markedFileContent { get; set; }
    }

    public struct PostAddWatermark
    {
        public string email { get; set; }
        public string assignmentName { get; set; }
        public string fileName { get; set; }
        public bool replaceInsteadOfInsert { get; set; }
        public string comment { get; set; }
        public int[] whitespacesLineNumbers { get; set; }
        public int[] randomStringLineNumbers { get; set; }
        public string fileContent { get; set; }
    }

    public struct StudentMarkedFile
    { 
        public string fileName { get; set; }
        public int numberOfLinesInFile { get; set; }
        public string watermark { get; set; }
        public string comment { get; set; }
        public int numberOfWhitespaceCharacters { get; set; }
        public int[] whitespacesLineNumbers { get; set; }
        public int[] randomStringLineNumbers { get; set; }
    }

    public struct StudentAssignmentJson
    {
        public List<StudentMarkedFile> files { get; set; }
    }

    #endregion
}

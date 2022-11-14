using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACES.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ACES.Controllers
{
    public class LoginController : Controller
    {
        private readonly ACESContext _context;
        private int loginError = 0;

        public LoginController(ACESContext context)
        {
            _context = context;
        }

        public IActionResult Index(string aID, int lError) //aID is for directing students to an assignment, lError is for login errors (incorrect username or password)
        {
            loginError = lError;
            if (loginError == 1)
            {

                ViewBag.lblLoginError = "Invalid Email or Password";
                //Response.Cookies.Append("IsLoggedIn", 0.ToString());
                return View();

            }

            ViewBag.assignmentID = aID;
            return View();
        }

        public IActionResult AttemptLogin(string username, string password, string assignmentID)
        {
            //Validate there is input
            if (username == null || password == null)
            {
                return RedirectToAction("index", new { lError = 1 });
            }

            // Get lists of students and instructors
            var instructors = _context.Instructor.ToList();
            var students = _context.Student.ToList();

            // Choose student or instructor based on the email
            var instructor = instructors.Where(x => x.Email == username).FirstOrDefault();
            var student = students.Where(x => x.Email == username).FirstOrDefault();
            if (instructor != null)
            {
                Response.Cookies.Append("InstructorEmail", instructor.Email.ToString());
            }
            else if (instructor == null)
            {

                if (student != null)
                {
                    Response.Cookies.Append("StudentEmail", student.Email.ToString());
                }

            }
            else
            {

                //Put logic here for if user doesn't exist

            }

            // Authenticate
            if (instructor != null)
            {
                //NOTE: CURRENTLY, ALL PRE-ENTERED DATA HAS mypass111 AS THE PASSWORD
                var hashedPass = Services.Cryptographer.ComputeSha256Hash(password + instructor.Salt);

                if (hashedPass.ToUpper() == instructor.Password.ToUpper())
                {
                    // figure out cookies and all that jazz...
                    if (Request.Cookies.ContainsKey("StudentID"))
                    {
                        Response.Cookies.Delete("StudentID");
                        Response.Cookies.Delete("StudentEmail");
                    }
                    Response.Cookies.Append("InstructorID", instructor.Id.ToString());

                    // take user to two factor authentication page if they have enabled two factor authentication
                    if (false) //TODO: change this to be "if instructor.TwoFactorEnabled" once that is implemented
                    {
                        return RedirectToAction("Authorize", "TwoFactorAuthentication");
                    }
                    // take user to designated landing page if they have not enabled two factor authentication
                    Response.Cookies.Append("IsLoggedIn", 1.ToString());
                    if (instructor.IsLoggedIn == false)
                    {
                        instructor.IsLoggedIn = true;
                        _context.SaveChanges();
                    }
                    return RedirectToAction("Index", "Courses", new { instructorId = instructor.Id });
                }
            }
            else if (student != null)
            {
                var hashedPass = Services.Cryptographer.ComputeSha256Hash(password + student.Salt);

                if (hashedPass.ToUpper() == student.Password.ToUpper())
                {
                    // figure out cookies and all that jazz...
                    if (Request.Cookies.ContainsKey("InstructorID"))
                    {
                        Response.Cookies.Delete("InstructorID");
                        Response.Cookies.Delete("InstructorEmail");
                    }
                    Response.Cookies.Append("StudentID", student.Id.ToString());

                    // take user to two factor authentication page if they have enabled two factor authentication
                    if (false) //TODO: change this to be "if student.TwoFactorEnabled" once that is implemented
                    {
                        if (!String.IsNullOrEmpty(assignmentID))
                        {
                            Response.Cookies.Append("assignmentID", assignmentID);
                        }
                        return RedirectToAction("Authorize", "TwoFactorAuthentication");
                    }
                    // take user to designated landing page if they have not enabled two factor authentication
                    Response.Cookies.Append("IsLoggedIn", 1.ToString());
                    student.IsLoggedIn = true;
                    _context.SaveChanges();

                    if (!String.IsNullOrEmpty(assignmentID))  //if an assignment ID is provided go to that specific assignment
                    {
                        //If student is not in the course of the specific assignment add them to that course
                        var courseID = _context.Assignment.Find(Int32.Parse(assignmentID)).CourseId; //Gets CourseID from AssignmentID
                        var enrolled = _context.Enrollment.Where(c => c.CourseId == courseID); //Gets all rows with this courseID
                        var isEnrolled = enrolled.FirstOrDefault(s => s.StudentId == student.Id); //Gets row with with studentID

                        if (isEnrolled == null) //Student is not in the course
                        {
                            Models.Enrollment enrollment = new Models.Enrollment
                            {
                                StudentId = student.Id,
                                CourseId = courseID
                            };
                            _context.Enrollment.Add(enrollment);
                            _context.SaveChanges();
                        }

                        string tempurl = String.Format("/Assignments/StudentRepoForm?assignmentId={0}", assignmentID);
                        return Redirect(tempurl);


                        /*
                        Int32.TryParse(assignmentID, out int intID);
                        var assignment = _context.Assignment.Where(x => x.Id == intID).FirstOrDefault();  //get the specific assignment
                        string sectionID = assignment.CourseId.ToString();  //get the assignment's courseId
                        string tempurl = String.Format("/StudentInterface/StudentAssignments?assignmentId={0}&courseId={1}", assignmentID, sectionID);  //create the url
                        return Redirect(tempurl);
                        */
                    }
                    return RedirectToAction("Index", "StudentInterface");
                }
            }


            return RedirectToAction("index", new { lError = 1 });
        }
    }
}

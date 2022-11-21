using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACES.Data;
using Google.Authenticator;
using ACES.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace ACES.Controllers
{
    public class TwoFactorAuthenticationController : Controller
    {
        private readonly ACESContext _context;
        private int authError = 0;

        public TwoFactorAuthenticationController(ACESContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult Enable(int aError = 0)
        {
            authError = aError;
            if (authError == 1)
            {
                ViewBag.lblAuthError = "Invalid auth token";
            }
            CombinedUsers user = new CombinedUsers();
            Instructor instructor = new Instructor();
            Student student = new Student();
            if (Request.Cookies["StudentEmail"] != null)
            {
                var students = _context.Student.ToList();
                student = students.Where(x => x.Email == Request.Cookies["StudentEmail"]).FirstOrDefault();
                user.Id = student.Id;
                user.Email = student.Email;
            }
            else
            {
                if (Request.Cookies["InstructorEmail"] != null)
                {
                    var instructors = _context.Instructor.ToList();
                    instructor = instructors.Where(x => x.Email == Request.Cookies["InstructorEmail"]).FirstOrDefault();
                    user.Id = instructor.Id;
                    user.Email = instructor.Email;
                }
                else // the code should never get here, but if it does, log them out just to be safe
                {
                    return RedirectToAction("Index", "Logout");
                }
            }

            TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
            var setupInfo = twoFactor.GenerateSetupCode("ACES", user.Email, TwoFactorKey(user), false, 3);
            ViewBag.SetupCode = setupInfo.ManualEntryKey;
            ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Enable(string inputCode)
        {
            CombinedUsers user = new CombinedUsers();
            Instructor instructor = new Instructor();
            Student student = new Student();
            if (Request.Cookies["StudentEmail"] != null)
            {
                var students = _context.Student.ToList();
                student = students.Where(x => x.Email == Request.Cookies["StudentEmail"]).FirstOrDefault();
                user.Id = student.Id;
                user.Email = student.Email;
            }
            else
            {
                if (Request.Cookies["InstructorEmail"] != null)
                {
                    var instructors = _context.Instructor.ToList();
                    instructor = instructors.Where(x => x.Email == Request.Cookies["InstructorEmail"]).FirstOrDefault();
                    user.Id = instructor.Id;
                    user.Email = instructor.Email;
                }
                else // the code should never get here, but if it does, log them out just to be safe
                {
                    return RedirectToAction("Index", "Logout");
                }
            }

            TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
            bool isValid = twoFactor.ValidateTwoFactorPIN(TwoFactorKey(user), inputCode);
            if (!isValid) // if they enter the key wrong, redirect them to the same page so they can try again
            {
                return RedirectToAction("Enable", "TwoFactorAuthentication", new { aError = 1 });
            }

            Response.Cookies.Append("TwoFactorEnabled", "True");
            //user.TwoFactorEnabled = true; //TODO: this needs to reflect in the database
            return RedirectToAction("Index", "UserSettings");
        }

        [HttpGet]
        public IActionResult Disable(int aError = 0)
        {
            authError = aError;
            if (authError == 1)
            {
                ViewBag.lblAuthError = "Invalid auth token";
            }
            return View();
        }

        [HttpPost]
        public IActionResult Disable(string inputCode)
        {
            CombinedUsers user = new CombinedUsers();
            Instructor instructor = new Instructor();
            Student student = new Student();
            if (Request.Cookies["StudentEmail"] != null)
            {
                var students = _context.Student.ToList();
                student = students.Where(x => x.Email == Request.Cookies["StudentEmail"]).FirstOrDefault();
                user.Id = student.Id;
                user.Email = student.Email;
            }
            else if (Request.Cookies["InstructorEmail"] != null)
            {
                var instructors = _context.Instructor.ToList();
                instructor = instructors.Where(x => x.Email == Request.Cookies["InstructorEmail"]).FirstOrDefault();
                user.Id = instructor.Id;
                user.Email = instructor.Email;
            }
            else // the code should never get here, but if it does, log them out just to be safe
            {
                return RedirectToAction("Index", "Logout");
            }


            TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
            bool isValid = twoFactor.ValidateTwoFactorPIN(TwoFactorKey(user), inputCode);
            if (!isValid) // if they enter the key wrong, redirect them to the same page so they can try again 
            {
                return RedirectToAction("Disable", "TwoFactorAuthentication", new { aError = 1 });
            }


            Response.Cookies.Delete("TwoFactorEnabled");
            //user.TwoFactorEnabled = false; //TODO: this needs to reflect in the database
            return RedirectToAction("Index", "UserSettings");
        }

        [HttpGet]
        public IActionResult Authorize(int aError = 0)
        {
            authError = aError;
            if (authError == 1)
            {
                ViewBag.lblAuthError = "Invalid auth token, try again";
            }
            return View();
        }

        [HttpPost]
        public IActionResult Authorize(string inputCode)
        {
            CombinedUsers user = new CombinedUsers();
            Instructor instructor = new Instructor();
            Student student = new Student();
            if (Request.Cookies["StudentEmail"] != null)
            {
                var students = _context.Student.ToList();
                student = students.Where(x => x.Email == Request.Cookies["StudentEmail"]).FirstOrDefault();
                user.Id = student.Id;
                user.Email = student.Email;
            }
            else if (Request.Cookies["InstructorEmail"] != null)
            {
                var instructors = _context.Instructor.ToList();
                instructor = instructors.Where(x => x.Email == Request.Cookies["InstructorEmail"]).FirstOrDefault();
                user.Id = instructor.Id;
                user.Email = instructor.Email;
            }
            else // the code should never get here, but if it does, log them out just to be safe
            {
                return RedirectToAction("Index", "Logout");
            }

            TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
            bool isValid = twoFactor.ValidateTwoFactorPIN(TwoFactorKey(user), inputCode);
            if (!isValid) // if they enter the key wrong, redirect them to the same page so they can try again
            {
                return RedirectToAction("Authorize", "TwoFactorAuthentication", new { aError = 1 });
            }


            Response.Cookies.Append("TwoFactorEnabled", "True"); // currently, this cookie is how we are tracking how the enable/disable two factor authentication button in user settings will behave

            // now that the user has completed their authentication, take them to the landing page they are supposed to go to
            if (instructor != null)
            {
                if (instructor.IsLoggedIn == false)
                {
                    Response.Cookies.Append("IsLoggedIn", 1.ToString());
                    instructor.IsLoggedIn = true;
                    _context.SaveChanges();
                    // if instructor is set up as an admin, set a cookie that will allow them to access the admin pages
                    if (false)
                    {
                        Response.Cookies.Append("IsAdmin", "true");
                    }
                }
                return RedirectToAction("Index", "Courses", new { instructorId = instructor.Id });
            }
            else if (student != null)
            {
                Response.Cookies.Append("IsLoggedIn", 1.ToString());
                student.IsLoggedIn = true;
                _context.SaveChanges();

                if (Request.Cookies["assignmentID"] != null)  //if an assignment ID is provided go to that specific assignment
                {
                    var assignmentID = Request.Cookies["assignmentID"];
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
                    Response.Cookies.Delete("assignmentID");
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
            else // the code should never get here, but if it does, log them out just to be safe
            {
                return RedirectToAction("Index", "Logout");
            }


        }

        private static string TwoFactorKey(CombinedUsers user)
        {
            //TODO: should we implement our cryptography and/or salt in the generation of this key?
            return $"{user.Id}+{user.Email}";
        }
    }
}

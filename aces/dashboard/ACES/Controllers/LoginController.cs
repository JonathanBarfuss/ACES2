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

        public LoginController(ACESContext context)
        {
            _context = context;
        }

        public IActionResult Index(string aID)
        {
            ViewBag.assignmentID = aID;
            return View();
        }

        public IActionResult AttemptLogin(string username, string password, string assignmentID)
        {
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
                    if (instructor.IsLoggedIn == false)
                    {
                        instructor.IsLoggedIn = true;
                        _context.SaveChanges();
                    }
                    return RedirectToAction("Index", "Courses", new { instructorId = instructor.Id });
                }
            } else if (student != null)
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
                    student.IsLoggedIn = true;
                    _context.SaveChanges();

                    if(!String.IsNullOrEmpty(assignmentID))  //if an assignment ID is provided go to that specific assignment
                    {
                        Int32.TryParse(assignmentID, out int intID);
                        var assignment = _context.Assignment.Where(x => x.Id == intID).FirstOrDefault();  //get the specific assignment
                        string sectionID = assignment.SectionId.ToString();  //get the assignment's sectionId
                        string tempurl = String.Format("/StudentInterface/StudentAssignments?assignmentId={0}&sectionId={1}", assignmentID, sectionID);  //create the url
                        return Redirect(tempurl);
                    }
                    return RedirectToAction("Index", "StudentInterface");
                }
            }

            return View();
        }
    }
}

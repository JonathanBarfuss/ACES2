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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AttemptLogin(string username, string password)
        {
            // Get lists of students and instructors
            var instructors = _context.Instructor.AsNoTracking().ToList();
            var students = _context.Student.AsNoTracking().ToList();

            // Choose student or instructor based on the email
            var instructor = instructors.Where(x => x.Email == username).FirstOrDefault();
            Models.Student student = null;
            if (instructor == null)
                student = students.Where(x => x.Email == username).FirstOrDefault();

            // Authenticate
            if(instructor != null)
            {
                //NOTE: CURRENTLY, ALL PRE-ENTERED DATA HAS mypass111 AS THE PASSWORD
                var hashedPass = Services.Cryptographer.ComputeSha256Hash(password + instructor.Salt);

                if (hashedPass.ToUpper() == instructor.Password.ToUpper())
                {
                    // figure out cookies and all that jazz...
                    if (Request.Cookies.ContainsKey("StudentID"))
                    {
                        Response.Cookies.Delete("StudentID");
                    }
                    Response.Cookies.Append("InstructorID", instructor.Id.ToString());
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
                    }
                    Response.Cookies.Append("StudentID", student.Id.ToString());
                    return RedirectToAction("Index", "StudentInterface");
                }
            }

            return View();
        }
    }
}

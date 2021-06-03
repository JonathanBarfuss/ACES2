using ACES.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACES.Controllers
{
    public class SignupController : Controller
    {
        private readonly ACESContext _context;

        public SignupController(ACESContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AttemptSignupAsync(string email, string password, string firstname, string lastname, bool isProfessor)
        {
            string salty = RandomString(36);
            if (isProfessor)
            {
                var instructors = _context.Instructor.AsNoTracking().ToList(); // get list of instructors
                var newInstructor = instructors.Where(x => x.Email == email).FirstOrDefault();  // see if the email already exists in instructors
                if (newInstructor != null)
                {
                    return RedirectToAction("Index", "Signup");
                }

                Models.Instructor instructor = new Models.Instructor() // create new instrctor if it is a new email
                {
                    FirstName = firstname,
                    LastName = lastname,
                    Email = email,
                    Password = Services.Cryptographer.ComputeSha256Hash(password + salty),
                    Salt = salty,
                    IsLoggedIn = true
                };

                _context.Instructor.Add(instructor); // add to database
                await _context.SaveChangesAsync();

                instructors = _context.Instructor.AsNoTracking().ToList();     // get new list of instructors
                newInstructor = instructors.Where(x => x.Email == email).FirstOrDefault();  // get newly entered instructor for cookie

                if (Request.Cookies.ContainsKey("StudentID"))
                {
                    Response.Cookies.Delete("StudentID");
                    Response.Cookies.Delete("StudentEmail");
                }
                Response.Cookies.Append("InstructorID", newInstructor.Id.ToString());
                return RedirectToAction("Index", "Courses", new { instructorId = newInstructor.Id });
            }
            else
            {
                var students = _context.Student.AsNoTracking().ToList();
                var newStudent = students.Where(x => x.Email == email).FirstOrDefault();
                if (newStudent != null)
                {
                    return RedirectToAction("Index", "Signup");
                }

                Models.Student student = new Models.Student()
                {
                    FirstName = firstname,
                    LastName = lastname,
                    Email = email,
                    Password = Services.Cryptographer.ComputeSha256Hash(password + salty),
                    Salt = salty,
                    IsLoggedIn = true
                };

                _context.Student.Add(student);
                await _context.SaveChangesAsync();

                students = _context.Student.AsNoTracking().ToList();
                newStudent = students.Where(x => x.Email == email).FirstOrDefault();

                if (Request.Cookies.ContainsKey("InstructorID"))
                {
                    Response.Cookies.Delete("InstructorID");
                    Response.Cookies.Delete("InstructorEmail");
                }
                Response.Cookies.Append("StudentID", newStudent.Id.ToString());
                return RedirectToAction("Index", "StudentInterface");
            }
        }

        // creates a random string for salt
        private static string RandomString(int length)
        {
            Random random = new Random();
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789-";
            var builder = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                var c = pool[random.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}

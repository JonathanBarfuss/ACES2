using System.Linq;
using ACES.Data;
using Microsoft.AspNetCore.Mvc;


namespace ACES.Controllers
{
    public class LogoutController : Controller
    {

        private readonly ACESContext _context;

        public LogoutController(ACESContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var instructors = _context.Instructor.ToList();
            var students = _context.Student.ToList();

            var IEmail = string.Empty;
            var SEmail = string.Empty;
                       

            Models.Instructor instructor = null;
            Models.Student student = null;
            if (Request.Cookies["InstructorEmail"] != null)
            {
                IEmail = Request.Cookies["InstructorEmail"].ToString();
                instructor = instructors.Where(x => x.Email == IEmail).FirstOrDefault();
            }
            else if (Request.Cookies["StudentEmail"] != null)
            {
                SEmail = Request.Cookies["StudentEmail"].ToString();
                student = students.Where(x => x.Email == SEmail).FirstOrDefault();
            }
      
            if (Request.Cookies.ContainsKey("TwoFactorEnabled"))
            {
                Response.Cookies.Delete("TwoFactorEnabled");
            }

            if (instructor != null)
            {

                instructor.IsLoggedIn = false;
                Response.Cookies.Delete("InstructorID");
                Response.Cookies.Delete("InstructorEmail");
                Response.Cookies.Delete("IsLoggedIn");
                var cookie = Request.Cookies["StudentEmail"];
                if (cookie != null)
                    Response.Cookies.Delete("StudentEmail");

            }
            else if (student != null)
            {

                student.IsLoggedIn = false;
                Response.Cookies.Delete("StudentID");
                Response.Cookies.Delete("StudentEmail");
                Response.Cookies.Delete("IsLoggedIn");
                var cookie = Request.Cookies["InstructorEmail"];
                if (cookie != null)
                    Response.Cookies.Delete("InstructorEmail");

            }

            _context.SaveChanges();

            return RedirectToAction("Index", "Login");
        }

    }
}

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
    public class AdminInterfaceController : Controller
    {

        private readonly ACESContext _context;

        public AdminInterfaceController(ACESContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructor.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> Disable2FA(string? userEmail, string? userType, int? userId)
        {
            //if (!Request.Cookies.ContainsKey("StudentID"))
            //{
            //    return RedirectToAction("Index", "Login");
            //}


            var instructors = await _context.Instructor.ToListAsync(); // get the list of instructor accounts
            var students = await _context.Student.ToListAsync();  //get the list of student accounts

            List<CombinedUsers> userList = new List<CombinedUsers>();
            CombinedUsers temp = new CombinedUsers();
            foreach (var instructor in instructors)
            {
                temp.Id = instructor.Id;
                temp.Email = instructor.Email;
                temp.UserType = "Instructor";
                userList.Add(new CombinedUsers(instructor.Id, instructor.Email, "Instructor"));
            }
            foreach (var student in students)
            {
                temp.Id = student.Id;
                temp.Email = student.Email;
                temp.UserType = "Student";
                userList.Add(new CombinedUsers(student.Id, student.Email, "Student"));
            }

            if (userEmail != null)
            {
                ViewBag.selectedUserEmail = userEmail;
            }
            if (userType != null)
            {
                ViewBag.selectedUserType = userType;
            }
            if (userId != null)
            {
                ViewBag.selectedUserId = userId; 
            }


            return View(userList);
        }

        public async Task<IActionResult> ApproveProfessor(string? message)
        {
            //if (!Request.Cookies.ContainsKey("StudentID"))
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            var instructors = await _context.Instructor.ToListAsync(); // get the list of instructor accounts (to be changed to list of accounts that signed up as instructors and need to be confirmed)

            List<Instructor> instructorList = new List<Instructor>();
            foreach (var instructor in instructors)
            {
                instructorList.Add(instructor);
            }
            return View(instructorList);
        }


    }

}

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

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Disable2FA(string? userEmail, string? userType, string? message)
        {
            //if (!Request.Cookies.ContainsKey("StudentID"))
            //{
            //    return RedirectToAction("Index", "Login");
            //}


            var instructors = await _context.Instructor.ToListAsync(); // get the list of instructor accounts
            var students = await _context.Student.ToListAsync();  //get the list of student accounts
            if (userEmail != null && userType != null)
            {
                if (userType == "Instructor")
                {
                    var instructor = instructors.Where(x => x.Email == userEmail).FirstOrDefault();
                    instructor.TwoFactorEnabled = false;
                    _context.SaveChanges();
                }
                else if (userType == "Student")
                {
                    var student = students.Where(x => x.Email == userEmail).FirstOrDefault();
                    student.TwoFactorEnabled = false;
                    _context.SaveChanges();
                }
            }

            List<CombinedUsers> userList = new List<CombinedUsers>();
            foreach (var instructor in instructors)
            {
                userList.Add(new CombinedUsers(instructor.Id, instructor.Email, "Instructor", instructor.TwoFactorEnabled));
            }
            foreach (var student in students)
            {
                userList.Add(new CombinedUsers(student.Id, student.Email, "Student", student.TwoFactorEnabled));
            }


            if (message != null)
            {
                ViewBag.Message = message;
            }


            return View(userList);
        }

        public async Task<IActionResult> ApproveProfessor(string? userEmail, bool? deny, string? message)
        {
            //if (!Request.Cookies.ContainsKey("StudentID"))
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            var instructors = await _context.Instructor.ToListAsync(); // get the list of instructor accounts (to be changed to list of accounts that signed up as instructors and need to be confirmed)

            if (userEmail != null)
            {
                var instructor = instructors.Where(x => x.Email == userEmail).FirstOrDefault();
                if (deny == true) // if the deny button was pressed instead of the approve button, delete the entry
                {
                    _context.Remove(instructor);
                }
                instructor.IsApproved = true; // always set instructor to approved, even if it was deleted, so that the list properly updates
                _context.SaveChanges();

            }

            List<Instructor> instructorList = new List<Instructor>();
            foreach (var instructor in instructors)
            {
                if (!instructor.IsApproved)
                {
                    instructorList.Add(instructor);
                }
            }

            
            if (message != null)
            {
                ViewBag.Message = message;
                if (instructorList.Count == 0) // making sure the sucess message is not overwritten if the last instructor is approved
                {
                    ViewBag.message += ", There are no instructors awaiting approval";
                }
            }
            else if (instructorList.Count == 0)
            {
                ViewBag.message = "There are no instructors awaiting approval";
            }
            return View(instructorList);
        }


    }

}

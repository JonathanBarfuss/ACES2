using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACES.Data;
using ACES.Models;
using ACES.Models.ViewModels;
using ACES.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ACES.Controllers
{
    public class PasswordResetController : Controller
    {
        private readonly ACESContext _context;
        private int loginError = 0;
        private readonly EmailService mailService;

        public PasswordResetController(ACESContext context, IOptions<EmailSettings> mailSettings)
        {
            _context = context;
            this.mailService = new EmailService(mailSettings);
        }

        public IActionResult Index(int lError) //lError is for login errors (incorrect username or password)
        {
            loginError = lError;
            if (loginError == 1)
            {

                ViewBag.lblLoginError = "Invalid Email or Password";
                return View();

            }

            if (loginError == 2)
            {

                ViewBag.lblLoginError = "Passwords Don't Match";
                return View();

            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword(int lError)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (ModelState.IsValid)
            {


                var instructors = _context.Instructor.ToList();
                var students = _context.Student.ToList();

                var instructor = instructors.Where(x => x.Email == model.Email).FirstOrDefault();
                var student = students.Where(x => x.Email == model.Email).FirstOrDefault();
                if (instructor != null)
                {
                    //var token = await userManager.GeneratePasswordResetTokenAsync(instructor);
                    var token = "CoolTokenName" + instructor.FirstName;

                    var passwordResetLink = Url.Action("AttemptPasswordReset", "PasswordReset",
                        new { email = model.Email, token = token }, Request.Scheme);

                    //Send Email
                    try
                    {
                        //EmailService mailService = new EmailService(new EmailSettings());
                        Email request = new Email();
                        request.ToEmail = instructor.Email;
                        request.Subject = "ACES Password Reset";
                        request.Body = passwordResetLink;
                        await mailService.SendEmailAsync(request);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    return View("ForgotPasswordConfirmation");
                }
                else if (student != null)
                {

                    //var token = await userManager.GeneratePasswordResetTokenAsync(student);
                    var token = "CoolTokenName" + student.FirstName;

                    var passwordResetLink = Url.Action("AttemptPasswordReset", "PasswordReset",
                        new { email = model.Email, token = token }, Request.Scheme);

                    //Send Email
                    try
                    {
                        //EmailService mailService = new EmailService(new EmailSettings());
                        Email request = new Email();
                        request.ToEmail = student.Email;
                        request.Subject = "ACES Password Reset";
                        request.Body = passwordResetLink;
                        await mailService.SendEmailAsync(request);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    return View("ForgotPasswordConfirmation");

                }
                else
                {
                    return View("ForgotPasswordConfirmation");
                }

            }

            return View(model);
        }

        public IActionResult AttemptPasswordReset(string username, string newPassword, string repeatPassword)
        {
            //Validate there is input
            if (username == null || newPassword == null || repeatPassword == null)
            {
                return RedirectToAction("Index", "PasswordReset", new { lError = 1 });
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

            //Reset Password
            if (instructor != null)
            {
                //Validate the passwords are equivalent
                if (newPassword != repeatPassword)
                {
                    return RedirectToAction("Index", "PasswordReset", new { lError = 2 });
                }

                string salty = RandomString(36);
                instructor.Password = Services.Cryptographer.ComputeSha256Hash(newPassword + salty);
                instructor.Salt = salty;
                _context.SaveChanges();

                return RedirectToAction("Index", "Login");
            }
            else if (student != null)
            {
                //Validate the passwords are equivalent
                if (newPassword != repeatPassword)
                {
                    return RedirectToAction("Index", "PasswordReset", new { lError = 2 });
                }

                string salty = RandomString(36);
                student.Password = Services.Cryptographer.ComputeSha256Hash(newPassword + salty);
                student.Salt = salty;
                _context.SaveChanges();

                return RedirectToAction("Index", "Login");
            }

            return RedirectToAction("Index", "PasswordReset", new { lError = 1 });
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

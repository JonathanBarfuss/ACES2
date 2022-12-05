﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACES.Data;
using ACES.Models;
using ACES.Models.ViewModels;

namespace ACES.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ACESContext _context;

        public CoursesController(ACESContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            if (!Request.Cookies.ContainsKey("InstructorID"))
            {
                return RedirectToAction("Index", "Login");
            }

            var instructorId = int.Parse(Request.Cookies["InstructorID"]);
            var courses = await _context.Course.Where(x => x.InstructorId == instructorId).ToListAsync();
            courses = courses.Where(x => x.IsCourseActive == true).ToList();  //only show the active courses

            // Get more info about courses (total number of assignments and students):
            foreach (var course in courses)
            {
                course.NumAssignments = _context.Assignment.Where(x => x.CourseId == course.Id).ToList().Count();
                course.NumStudents = _context.Enrollment.Where(x => x.CourseId == course.Id).ToList().Count();
            }
            return View(courses);
        }

        public async Task<IActionResult> CourseAssignments(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseName = _context.Course.FirstOrDefault(m => m.Id == id).CourseName;
            if (string.IsNullOrEmpty(courseName))
            {
                return NotFound();
            }

            var assignments = await _context.Assignment.Where(x => x.CourseId == id).ToListAsync();
            var courseNumber = _context.Course.FirstOrDefault(y => y.Id == id).CourseNumber;
            var academicYear = _context.Course.FirstOrDefault(z => z.Id == id).AcademicYear;
            var cRN = _context.Course.FirstOrDefault(a => a.Id == id).CRN;
            var vm = new CourseAssignmentsVM()
            {
                CourseId = id,
                CouseName = courseName,
                CourseNumber = courseNumber,
                AcademicYear = academicYear,                                
                CRN = cRN,                               
                Assignments = assignments
            };

            return View(vm);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseName,CourseNumber,AcademicYear,CRN")] Course course)
        {
            if (ModelState.IsValid)
            {
                course.InstructorId = int.Parse(Request.Cookies["InstructorID"]);
                course.IsCourseActive = true;
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Edit/5
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

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseName,CourseNumber,AcademicYear,CRN,InstructorId,IsCourseActive")] Course course)
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

        // GET: Courses/Delete/5
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

        public async Task<IActionResult> Archive(int? id)
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

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            course.IsCourseActive = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }
    }
}

using ACES.Data;
using ACES.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ACES.Controllers
{
    public class JSONController : Controller
    {

        private readonly ACESContext _context;
        private readonly IConfiguration _configuration;        

        public JSONController(ACESContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.JSON.ToListAsync());
        }

        public IActionResult Create(int assignmentId)
        {
            ViewBag.AssignmentId = assignmentId;    
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FileName,LineNumbers,ReplaceWatermark,Comment,WhitespaceLines,RandomStringLines,AssignmentId")] JSON json)
        {            
            if (ModelState.IsValid)
            {
                _context.Add(json);                                                             
                var assignment = await _context.Assignment.FirstOrDefaultAsync(m => m.Id == json.AssignmentId);

                var fileName = json.FileName;
                var lineNumbers = json.LineNumbers;
                var replaceWatermark = json.ReplaceWatermark ? 1 : 0;
                var comment = json.Comment;
                var whitespaceLines = json.WhitespaceLines;
                var randomStringLines = json.RandomStringLines;

                List<String> whiteLines = new List<String>();
                var builder = new StringBuilder();
                for (var i = 0; i <= whitespaceLines.Length; i++)
                {
                    string entry;
                    if (i == whitespaceLines.Length || whitespaceLines[i] == ' ')
                    {
                        entry = builder.ToString();
                        builder.Remove(0, builder.Length);
                        whiteLines.Add(entry);
                    }
                    else
                    {
                        builder.Append(whitespaceLines[i].ToString());
                    }
                }

                builder.Remove(0, builder.Length);

                List<String> randomLines = new List<String>();
                for (var i = 0; i <= randomStringLines.Length; i++)
                {
                    string entry;
                    if (i == randomStringLines.Length || randomStringLines[i] == ' ')
                    {
                        entry = builder.ToString();
                        builder.Remove(0, builder.Length);
                        randomLines.Add(entry);
                    }
                    else
                    {
                        builder.Append(randomStringLines[i].ToString());
                    }

                }

                JObject fileJson = new JObject(
                    new JProperty("name", fileName),
                    new JProperty("lines", Int32.Parse(lineNumbers)),
                    new JProperty("replaceInsteadOfInsert", replaceWatermark),
                    new JProperty("comment", comment),
                    new JProperty("whitespaces",
                        new JArray(from l in whiteLines select Int32.Parse(l))),
                    new JProperty("randomstring",
                        new JArray(from r in randomLines select Int32.Parse(r))));   
                Console.WriteLine("fileJson {0}", fileJson);
                
                assignment.JSONFiles = assignment.JSONFiles + fileJson;               

                JObject jsonObjects = new JObject(
                    new JProperty("files",
                    new JArray(assignment.JSONFiles)));

                assignment.JSONCode = (string)JsonConvert.SerializeObject(jsonObjects);               

                await _context.SaveChangesAsync();
                _context.Update(json);
                _context.Update(assignment);
                return RedirectToAction("CourseAssignments", "Courses", new { id = assignment.CourseId });
            }
            return View(json);
        }

        // GET: JSON/Edit/5
        public async Task<IActionResult> Edit(int? id, string from = "")
        {
            if (id == null)
            {
                return NotFound();
            }

            var json = await _context.JSON.FindAsync(id);
            if (json == null)
            {
                return NotFound();
            }

            ViewBag.AssignmentId = json.AssignmentId;
            ViewBag.From = from; // This helps take us back to CourseAssignments if that's where we came from
            return View(json);
        }

        // POST: JSON/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FileName,LineNumbers,ReplaceWatermark,Comment,WhitespaceLines,RandomStringLines,AssignmentId")] JSON json)
        {
            if (id != json.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(json);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JsonExists(json.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(Index));  //use this if you would rather go to the list of all assignments
                return RedirectToAction("AssignmentsJson", "Assignments", new { id = json.AssignmentId });
            }
            return View(json);
        }

        // GET: JSON/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var json = await _context.JSON
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (json == null)
            {
                return NotFound();
            }

            return View(json);
        }

        // POST: Json/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var json = await _context.JSON.FindAsync(id);            
            var jsonToDelete = await _context.JSON.Where(x => x.AssignmentId == id).ToListAsync();
            var assignment = await _context.Assignment.FirstOrDefaultAsync(m => m.Id == id);

            foreach (var item in jsonToDelete)
            {
                _context.JSON.Remove(item);
            }
            assignment.JSONCode = "";
            assignment.JSONFiles = "";

            await _context.SaveChangesAsync();
            return RedirectToAction("AssignmentsJson", "Assignments", new { id = id });
        }

        private bool JsonExists(int id)
        {
            return _context.JSON.Any(e => e.Id == id);
        }
    }

}


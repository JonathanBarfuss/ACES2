using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACES.Models;
using Microsoft.EntityFrameworkCore;

namespace ACES.Data
{
    public class ACESContext : DbContext
    {
        public ACESContext(DbContextOptions<ACESContext> options) : base(options)
        {
        }

        public DbSet<Assignment> Assignment { get; set; }
        public DbSet<Course> Course { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<Instructor> Instructor { get; set; }
        public DbSet<Enrollment> Enrollment { get; set; }
        public DbSet<StudentAssignment> StudentAssignment { get; set; }
        public DbSet<Results> Results { get; set; }
        public DbSet<Watermarks> Watermarks { get; set; }

    }
}

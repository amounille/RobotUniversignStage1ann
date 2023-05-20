using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RobotUniversign.DAO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobotUniversign.DAO
{
    public class DatabaseContext : DbContext
    {
        

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
            {
              
            }


        public DbSet<Collect> Collecte { get; set; }

        public DbSet<Batch> Batch { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableDetailedErrors();
            base.OnConfiguring(optionsBuilder);
        }

    }
}

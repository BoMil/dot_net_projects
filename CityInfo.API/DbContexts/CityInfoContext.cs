using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.DbContexts
{
    public class CityInfoContext : DbContext
    {

        public DbSet<City> Cities { get; set; }
        public DbSet<PointOfInterest> PointsOfInterest { get; set; }

        /// <summary>
        ///  This allow us to set the connection string in the Program.cs file
        /// </summary>
        /// <param name="options"></param>
        public CityInfoContext(DbContextOptions<CityInfoContext> options) : base(options)
        {
        }


        // !This is an example of how we can add data to the database
        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     modelBuilder.Entity<City>().HasData(
        //         new City("Paris")
        //         {
        //             Id = 1,
        //             Description = "The capital of France"
        //         },
        //         new City("London")
        //         {
        //             Id = 2,
        //             Description = "The capital of England"
        //         },
        //         new City("New York")
        //         {
        //             Id = 3,
        //             Description = "The capital of the United States"
        //         }
        //     );
            
        //     modelBuilder.Entity<PointOfInterest>().HasData(
        //         new PointOfInterest("Eiffel Tower")
        //         {
        //             Id = 1,
        //             CityId = 1,
        //             Description = "The Eiffel Tower is a wrought iron lattice tower in Paris, France. It is the world's oldest existing structure."
        //         },
        //         new PointOfInterest("London Eye")
        //         {
        //             Id = 2,
        //             CityId = 2,
        //             Description = "The London Eye is a giant Ferris wheel in London, England. It is the world's tallest structure."
        //         },
        //         new PointOfInterest("Statue of Liberty")
        //         {
        //             Id = 3,
        //             CityId = 3,
        //             Description = "The Statue of Liberty is a colossal neoclassical sculpture in New York, United States. It is the world's largest statue."
        //         }
        //     );
        //     base.OnModelCreating(modelBuilder);
        // }
        
    }
}
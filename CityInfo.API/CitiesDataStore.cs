using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;

namespace CityInfo.API
{
    public class CitiesDataStore
    {
        public List<CityDto> Cities { get; set; } = new List<CityDto>();

        public static CitiesDataStore Instance { get; } = new CitiesDataStore();

        public CitiesDataStore()
        {
            Cities.Add(new CityDto {
                Id = 1, Name = "London", Description = "London is the capital city of England.",
                 PointsOfInterest = new List<PointsOfInterestDto>() { new PointsOfInterestDto { Id = 1, Name = "Tower of London" }, new PointsOfInterestDto { Id = 2, Name = "Buckingham Palace" } }});
            Cities.Add(new CityDto { Id = 2, Name = "Paris", Description = "Paris is the capital of France.", PointsOfInterest = new List<PointsOfInterestDto>() { new PointsOfInterestDto { Id = 1, Name = "Eiffel Tower" }, new PointsOfInterestDto { Id = 2, Name = "Louvre Museum" } } });
            Cities.Add(new CityDto { Id = 3, Name = "Berlin", Description = "Berlin is the capital of Germany.", PointsOfInterest = new List<PointsOfInterestDto>() { new PointsOfInterestDto { Id = 1, Name = "Brandenburg Gate" }, new PointsOfInterestDto { Id = 2, Name = "Berlin Wall" } } });
        }
    }
}
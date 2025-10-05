using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    // [Route("api/[controller]")] - this will automatically prefix the route with the base route of the app (https://localhost:7156/api/Cities/)
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {

        // [HttpGet("api/cities")]
        [HttpGet()]
        public ActionResult<IEnumerable<CityDto>> GetCities()
        {
            return  Ok(CitiesDataStore.Instance.Cities);
        }

        [HttpGet("{id}")]
        public ActionResult<CityDto> GetCity(int id)
        {
            var city = CitiesDataStore.Instance.Cities.FirstOrDefault(c => c.Id == id);
            if (city == null)
            {
                return NotFound();
            }
            return  Ok(city);
        }   
    }
}
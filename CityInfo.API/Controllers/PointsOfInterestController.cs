using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities/{cityId}/pointsOfInterest")]
    public class PointsOfInterestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<PointsOfInterestDto>> GetPointsOfInterestByCityId(int cityId)
        {
            var pointsOfInterest = CitiesDataStore.Instance.Cities.FirstOrDefault(c => c.Id == cityId)?.PointsOfInterest;
            if (pointsOfInterest == null)
            {
                return NotFound();
            }
            return Ok(pointsOfInterest);
        }

        [HttpGet("{pointsOfInterestId}")]
        public ActionResult<PointsOfInterestDto> GetPointOfInterest(int cityId, int pointsOfInterestId)
        {
            // First, get the points of interest for the city
            var pointsOfInterestByCityId = CitiesDataStore.Instance.Cities.FirstOrDefault(c => c.Id == cityId)?.PointsOfInterest;
            if (pointsOfInterestByCityId == null)
            {
                return NotFound();
            }

            // Then, get the point of interest
            var pointOfInterest = pointsOfInterestByCityId.FirstOrDefault(p => p.Id == pointsOfInterestId);
            if (pointOfInterest == null)
            {
                return NotFound();
            }
            return Ok(pointOfInterest);
        }
    }
}
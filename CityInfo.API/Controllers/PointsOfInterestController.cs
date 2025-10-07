using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpGet("{pointsOfInterestId}", Name = "GetPointOfInterest")]
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

        [HttpPost]
        public ActionResult<PointsOfInterestDto> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterestForCreation)
        {

            // This way we can check the model state, but this is not necessary because we have the [ApiController] attribute
            // if (!ModelState.IsValid)
            // {
            //     return BadRequest(ModelState);
            // }

            // First check if the city exists
            var city = CitiesDataStore.Instance.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            // demo purposes - to be improved
            var maxPointOfInterestId = CitiesDataStore.Instance.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

            // Then, create the point of interest
            var finalPointOfInterest = new PointsOfInterestDto
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterestForCreation.Name,
                Description = pointOfInterestForCreation.Description
            };

            city.PointsOfInterest.Add(finalPointOfInterest);
            return CreatedAtRoute("GetPointOfInterest", new { cityId = cityId, pointsOfInterestId = finalPointOfInterest.Id }, finalPointOfInterest);
        }

        [HttpPut("{pointsOfInterestId}")]
        public ActionResult<PointsOfInterestDto> UpdatePointOfInterest(int cityId, int pointsOfInterestId, [FromBody] PointOfInterestUpdateDto pointOfInterestUpdate)
        {
            // First check if the city exists
            var city = CitiesDataStore.Instance.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }


            // Then find the point of interest
            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointsOfInterestId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            pointOfInterestFromStore.Name = pointOfInterestUpdate.Name;
            pointOfInterestFromStore.Description = pointOfInterestUpdate.Description;

            return NoContent();
        }

        [HttpPatch("{pointsOfInterestId}")]
        public ActionResult<PointsOfInterestDto> PartiallyUpdatePointOfInterest(int cityId, int pointsOfInterestId, [FromBody] JsonPatchDocument<PointOfInterestUpdateDto> patchDocument)
        {
            // First check if the city exists
            var city = CitiesDataStore.Instance.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            // Then find the point of interest
            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointsOfInterestId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = new PointOfInterestUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description
            };

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;




            return NoContent();
        }

        [HttpDelete("{pointsOfInterestId}")]
        public ActionResult<PointsOfInterestDto> DeletePointOfInterest(int cityId, int pointsOfInterestId)
        {
            // First check if the city exists
            var city = CitiesDataStore.Instance.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            // Then find the point of interest
            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointsOfInterestId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            city.PointsOfInterest.Remove(pointOfInterestFromStore);
            return NoContent();
        }
        
    }
}
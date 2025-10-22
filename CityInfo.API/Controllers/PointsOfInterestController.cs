using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities/{cityId}/pointsOfInterest")]
    public class PointsOfInterestController : ControllerBase
    {
        private ILogger<PointsOfInterestController> _logger;
        private IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;


        public PointsOfInterestController(
            ILogger<PointsOfInterestController> logger,
            IMailService mailService,
            IMapper mapper,
            ICityInfoRepository cityInfoRepository
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointsOfInterestDto>>> GetPointsOfInterestByCityId(int cityId)
        {

            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogError($"City with id {cityId} not found");
                return NotFound("City not found");
            }

            var pointsOfInterest = await _cityInfoRepository.GetPointsOfInterestByCityIdAsync(cityId);
            if (pointsOfInterest == null)
            {
                _logger.LogError($"Point of interests for city with id {cityId} not found");
                return NotFound("Points of interest not found");
            }

            return Ok(_mapper.Map<IEnumerable<PointsOfInterestDto>>(pointsOfInterest));
        }

        [HttpGet("{pointsOfInterestId}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointsOfInterestDto>> GetPointOfInterest(int cityId, int pointsOfInterestId)
        {

            try
            {
                if (!await _cityInfoRepository.CityExistsAsync(cityId))
                {
                    _logger.LogError($"City with id {cityId} not found");
                    return NotFound("City not found");
                }

                var pointOfItnerest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointsOfInterestId);
                if (pointOfItnerest == null)
                {
                    _logger.LogError($"Point of interest with id {pointsOfInterestId} not found");
                    return NotFound("Point of interest not found");
                }

                return Ok(_mapper.Map<PointsOfInterestDto>(pointOfItnerest));

            }
            catch (System.Exception ex)
            {
                _logger.LogCritical($"Error while getting point of interest for city with the id {cityId}", ex);
                return StatusCode(500, "A problem occurred while getting the point of interest");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PointsOfInterestDto>> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterestForCreation)
        {
            try
            {

                // This way we can check the model state, but this is not necessary because we have the [ApiController] attribute
                // if (!ModelState.IsValid)
                // {
                //     return BadRequest(ModelState);
                // }

                // First check if the city exists
                if (!await _cityInfoRepository.CityExistsAsync(cityId))
                {
                    return NotFound("City not found");
                }

                var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointOfInterestForCreation);

                await _cityInfoRepository.AddPointOfInterestToCityAsync(cityId, finalPointOfInterest);

                // After this call, the finalPointOfInterest will be saved and the id will be auto generated
                await _cityInfoRepository.SaveChangesAsync();

                // This will
                return CreatedAtRoute("GetPointOfInterest", new { cityId = cityId, pointsOfInterestId = finalPointOfInterest.Id }, finalPointOfInterest);
                
            }
            catch (System.Exception ex)
            {
                _logger.LogCritical($"Error while creating point of interest for city with the id {cityId}", ex);
                return StatusCode(500, "A problem occurred while creating the point of interest");
            }


        }

        [HttpPut("{pointsOfInterestId}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointsOfInterestId, [FromBody] PointOfInterestUpdateDto pointOfInterestUpdate)
        {
            // First check if the city exists
         if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound("City not found");
            }


            // Then find the point of interest
            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointsOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound("Point of interest not found");
            }

            // The mapper will override the values
            _mapper.Map(pointOfInterestUpdate, pointOfInterestEntity);

            // And then changes will be saved to the database
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{pointsOfInterestId}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointsOfInterestId, [FromBody] JsonPatchDocument<PointOfInterestUpdateDto> patchDocument)
        {
            // First check if the city exists
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound("City not found");
            }

            // Then find the point of interest
            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointsOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound("Point of interest not found");
            }

            var pointOfInterestToPatch = _mapper.Map<PointOfInterestUpdateDto>(pointOfInterestEntity);

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            // pointOfInterestEntity.Name = pointOfInterestToPatch.Name;
            // pointOfInterestEntity.Description = pointOfInterestToPatch.Description;


            // And then changes will be saved to the database
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointsOfInterestId}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointsOfInterestId)
        {
            // First check if the city exists
            var city = CitiesDataStore.Instance.Cities.FirstOrDefault(c => c.Id == cityId);
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            // Then find the point of interest
            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointsOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound("Point of interest not found");
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);

            _mailService.SendMail("Point of interest deleted", $"Point of interest with id {pointsOfInterestId} has been deleted");
            return NoContent();
        }
        
    }
}
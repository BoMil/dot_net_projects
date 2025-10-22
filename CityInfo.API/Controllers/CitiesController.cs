using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    // [Route("api/[controller]")] - this will automatically prefix the route with the base route of the app (https://localhost:7156/api/Cities/)
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {

        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        const int maxPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // [HttpGet("api/cities")]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<CityWthoutPointOfInterestDto>>> GetCities([FromQuery] string? name, [FromQuery] string? searchQuery, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageSize > maxPageSize)
            {
                pageSize = maxPageSize;
            }
            var (cityEntities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            // ? Manually map the entities to the DTO
            // var cities = cityEntities.Select(c => new CityWthoutPointOfInterestDto
            // {
            //     Id = c.Id,
            //     Name = c.Name,
            //     Description = c.Description,
            // });

            return Ok(_mapper.Map<IEnumerable<CityWthoutPointOfInterestDto>>(cityEntities));
        }

        // ? The url example is: https://localhost:{{portNumber}}/api/cities/2?includePointsOfInterest=true
        [HttpGet("{id}")]
        // Here we are using IActionResult because we can now return a CityDto or CityWthoutPointOfInterestDto
        public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
        {
            var city =  await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                return  Ok(_mapper.Map<CityDto>(city));
            }
            return  Ok(_mapper.Map<CityWthoutPointOfInterestDto>(city));
        }   
    }
}
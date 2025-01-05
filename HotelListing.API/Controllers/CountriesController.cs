using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Query;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Core.Models;

namespace HotelListing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class CountriesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ICountriesRepository _countriesRepository;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesController> logger)
    {
        _mapper = mapper;
        _countriesRepository = countriesRepository;
        _logger = logger;
    }

    [HttpGet("GetAll")]
    [EnableQuery]
    public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
    {
        var countries = await _countriesRepository.GetAllAsync<GetCountryDto>();
        return Ok(countries);
    }

    [HttpGet]
    [EnableQuery]
    public async Task<ActionResult<PagedResult<GetCountryDto>>> GetPagedCountries([FromQuery] QueryParameters queryParameters)
    {
        var pagedCountriesResult = await _countriesRepository.GetAllAsync<GetCountryDto>(queryParameters);
        return Ok(pagedCountriesResult);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDetailsDto>> GetCountry(int id)
    {
        var country = await _countriesRepository.GetCountryDetails(id);
        return Ok(country);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
    {
        if (id != updateCountryDto.Id)
        {
            return BadRequest("Invalid Id");
        }

        try
        {
            await _countriesRepository.UpdateAsync(id, updateCountryDto);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CountryExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto createCountryDto)
    {
        var country = await _countriesRepository.AddAsync<CreateCountryDto, GetCountryDto>(createCountryDto);
        return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        await _countriesRepository.DeleteAsync(id);
        return NoContent();
    }

    private async Task<bool> CountryExists(int id)
    {
        return await _countriesRepository.Exists(id);
    }
}

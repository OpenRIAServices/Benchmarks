using Microsoft.AspNetCore.Mvc;
using OpenRiaServices.Client.Benchmarks.Server.Cities;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        public CitiesController()
        {
        }

        [HttpGet]
        public IEnumerable<City> Get()
            => OpenRiaServices.Client.Benchmarks.Server.Cities.CityDomainService.GetCitiesResult;
    }
}
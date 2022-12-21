using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("0.0")]
    [ApiController]
    public class LocalizerController : ControllerBase
    {
        private readonly IStringLocalizer<LocalizerController> _stringLocalizer;

        public LocalizerController(IStringLocalizer<LocalizerController> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var message = _stringLocalizer["hi"].ToString();
            return Ok(message);
        }

        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {
            var message = string.Format(_stringLocalizer["welcome"], name);
            return Ok(message);
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var message = _stringLocalizer.GetAllStrings();
            return Ok(message);
        }
    }
}

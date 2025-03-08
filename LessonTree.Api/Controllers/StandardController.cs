using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LessonTree.BLL.Service;
using LessonTree.Models.DTO;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StandardController : ControllerBase
    {
        private readonly IStandardService _service;
        private readonly ILogger<StandardController> _logger;

        public StandardController(IStandardService service, ILogger<StandardController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetStandards()
        {
            var standards = _service.GetAll();
            return Ok(standards);
        }

        
    }
}
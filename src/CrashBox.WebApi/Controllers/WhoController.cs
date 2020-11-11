using System;
using Microsoft.AspNetCore.Mvc;

//This is a test controller 

namespace CrashBox.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class WhoController : ControllerBase
    {
        public IActionResult Get()
        {
            return Ok(Environment.MachineName);
        }
      
    }// class
} // ns

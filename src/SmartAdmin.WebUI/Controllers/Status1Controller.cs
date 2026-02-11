// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common.Interfaces;

using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartAdmin.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Status1Controller(IConfiguration configuration, IApplicationDbContext context) : ControllerBase
    {
        // GET: api/<Status1Controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[]
            {
                "DefaultConnection: " + configuration.GetConnectionString("DefaultConnection"),
                "SourceConnection: " + configuration.GetConnectionString("DefaultConnection"),
                "SquarePayConnection: " + configuration.GetConnectionString("SquarePayConnection")
            };
        }

        // GET api/<Status1Controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<Status1Controller>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Status1Controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Status1Controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

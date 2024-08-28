using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Utility;
using System.Runtime.CompilerServices;

namespace RedMango_API.Controllers;

[Route("api/AuthTest")]
[ApiController]
public class AuthTestController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<string>> GetSomething()
    {
        return "You are authenticated";
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles =SD.Role_Admin)]
    public async Task<ActionResult<string>> GetSomething(int someIntValue)
    {
        return "You are authorized with Role of Admin";
    }
}

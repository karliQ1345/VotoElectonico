using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace VotoElectonico.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected string? GetCedula() => User.FindFirst("cedula")?.Value;

        protected Guid? GetUserId()
        {
            var id =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(id, out var g) ? g : null;
        }

        protected bool IsInRole(string role) => User.IsInRole(role);
    }

}

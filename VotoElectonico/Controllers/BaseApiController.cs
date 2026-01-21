using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VotoElectonico.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected string? GetCedula() => User.FindFirst("cedula")?.Value;

        protected Guid? GetUserId()
        {
            var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        protected bool IsInRole(string role) => User.IsInRole(role);
    }
}

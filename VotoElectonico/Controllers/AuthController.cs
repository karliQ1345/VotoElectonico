using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotoElectonico.DTOs.Auth;
using VotoElectonico.DTOs.Common;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        public AuthController() { }

        [HttpPost("login")]
        public ActionResult<ApiResponse<LoginResponseDto>> Login([FromBody] LoginRequestDto req)
        {
            // Validar cedula, consultar usuario/roles, decidir 2FA.
            var resp = new LoginResponseDto
            {
                RequiereTwoFactor = true,
                TwoFactorSessionId = null,
                RolPrincipal = "JefeJunta",
                Redirect = "/twofactor",
                EmailEnmascarado = "jua****@gmail.com"
            };

            return Ok(ApiResponse<LoginResponseDto>.Success(resp));
        }
    }
}

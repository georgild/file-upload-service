using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FileUploadService.DTOs;
using FileUploadService.Exceptions;
using FileUploadService.Models;
using FileUploadService.Repositories;
using FileUploadService.Services;

namespace FileUploadService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase {

        private readonly string jwtKey;

        private readonly string jwtIssuer;

        public AuthController(IConfiguration config, ILogger<FilesController> logger) {

            string? jwtKeyConfig = config.GetSection("Jwt:Key").Get<string>();
            string? jwtIssuerConfig = config.GetSection("Jwt:Issuer").Get<string>();

            if (string.IsNullOrWhiteSpace(jwtKeyConfig) || string.IsNullOrWhiteSpace(jwtIssuerConfig)) {
                throw new ApplicationException("Invalid JWT config");
            }

            jwtKey = jwtKeyConfig;
            jwtIssuer = jwtIssuerConfig;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Post([FromBody] LoginDto loginRequest) {

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

 
            long sampleUid = Math.Abs((loginRequest.Username + loginRequest.Password).GetHashCode());
            IEnumerable<Claim> claims = [
                new Claim("uid", sampleUid.ToString())
            ];

            JwtSecurityToken Sectoken = new JwtSecurityToken(jwtIssuer,
              jwtIssuer,
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            string token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

            return Ok(token);
        }
    }

}

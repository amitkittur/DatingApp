using System.Threading.Tasks;
using DattingApp.API.Data;
using DattingApp.API.Dtos;
using DattingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace DattingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;

        }

        [HttpPost("register")]

        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {

            userForRegisterDto.username = userForRegisterDto.username.ToLower();

            if (await _repo.UserExists(userForRegisterDto.username))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                Username = userForRegisterDto.username
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.password);
            return StatusCode(201);

        }

        [HttpPost("login")]
        
        public async Task<IActionResult> login(UserForLoginDto userForLoginDto)
        {
            var userFromrepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromrepo == null)
                return Unauthorized();

            //Create Token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,userFromrepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromrepo.Username.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                        .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });          
        }
    }
}
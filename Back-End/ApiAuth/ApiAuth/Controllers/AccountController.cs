using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiAuth.Data;
using ApiAuth.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            DataContext context,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, 
            SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> PostUser([FromBody] UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response
                {
                    IsSuccess = false,
                    Message = "Bad request",
                    Result = ModelState
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user != null)
            {
                return BadRequest(new Response
                {
                    IsSuccess = false,
                    Message = "Error003"
                });
            }

            user = new IdentityUser
            {
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = true
                //PhoneNumber = request.Phone,
            };

            IdentityResult result = await _userManager.CreateAsync(user, request.Password);
            if (result != IdentityResult.Success)
            {
                return BadRequest(
                    new Response
                    {
                        IsSuccess = false,
                        Message = result.Errors.FirstOrDefault().Description
                    });
            }

            await CheckRoleAsync("ROLE_MODERATOR");
            await CheckRoleAsync("ROLE_ADMIN");

            await _userManager.AddToRoleAsync(user, "ROLE_MODERATOR");

            return Ok(new Response { IsSuccess = true });
        }

        [HttpPost]
        [Route("CreateToken")]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Username);
                if (user != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result =  await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                    if (result.Succeeded)
                    {
                        object results = GetToken(user);
                        return Created(string.Empty, results);
                    }
                }
            }

            return BadRequest();
        }

        // GET: api/Account
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IEnumerable<object>> GetUsers()
        {
            return await (from u in _context.Users
                          from r in _context.Roles
                          from ur in _context.UserRoles
                          where (u.Id == ur.UserId && ur.RoleId == r.Id)
                          && (u.Email == u.Email)
                          select new
                          {
                              id = u.Id,
                              username = u.UserName,
                              Email = u.Email,
                              emailConfirmed = u.EmailConfirmed,
                              roles =  r.Name
                          }).ToListAsync();
        }

        private object GetToken(IdentityUser user)
        {
            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken token = new JwtSecurityToken(
                _configuration["Tokens:Issuer"],
                _configuration["Tokens:Audience"],
                claims,
                expires: DateTime.UtcNow.AddDays(99),
                signingCredentials: credentials);

            return new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                roles = (from r in _context.Roles
                         from ur in _context.UserRoles
                         where (ur.UserId == user.Id && ur.RoleId == r.Id)
                         select r.Name)
            };
        }

        private async Task CheckRoleAsync(string roleName)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        // GET: 
        [HttpGet]
        [Route("all")]
        public  string all()
        {
            return "Este es el contenido puclico";
        }

        // GET: 
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("user")]
        public async Task<IEnumerable<object>> user()
        {
            return await (from u in _context.Users
                          from r in _context.Roles
                          from ur in _context.UserRoles
                          where (u.Id == ur.UserId && ur.RoleId == r.Id)
                          && (u.Email == u.Email)
                          select new
                          {
                              id = u.Id,
                              username = u.UserName,
                              Email = u.Email,
                              emailConfirmed = u.EmailConfirmed,
                              roles = r.Name
                          }).ToListAsync();
        }

        // GET api/<AccountController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AccountController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT api/<AccountController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<AccountController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}

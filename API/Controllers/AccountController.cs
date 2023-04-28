using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly DataContext context;
        public ITokenService TokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.TokenService = tokenService;
            this.context = context;
            
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if(await UserExists(registerDTO.Username))
                return BadRequest("User already exists.");

            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDTO.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };

            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();
             return new UserDTO(){
              Username = user.UserName,
              token = TokenService.CreateToken(user)  
            };
        }

        private async Task<bool> UserExists(string username){
            return await this.context.Users.AnyAsync(user=> user.UserName.ToLower() == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login (LoginDTO login)
        {
            var user = await this.context.Users.FirstOrDefaultAsync(f=>f.UserName == login.Username.ToLower());
            return IsValidPasswordForLogin(user, login);
        }

        private ActionResult<UserDTO>  IsValidPasswordForLogin(AppUser appUser, LoginDTO login)
        {
            if(appUser == null) return Unauthorized("Unvalid Username");           

            using var hmac = new HMACSHA512(appUser.PasswordSalt);
            var ComputeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

            for(int bytes = 0; bytes < ComputeHash.Length; bytes++)
            {
                if(ComputeHash[bytes] != appUser.PasswordHash[bytes])
                    return Unauthorized("Unvalid password");
            }
            return new UserDTO(){
              Username = appUser.UserName,
              token = TokenService.CreateToken(appUser)  
            };
        }
    }
}
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseAPIController
    {
        public IUserRepository UserRepository { get; }
        public IMapper Mapper { get; }
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            this.Mapper = mapper;
            this.UserRepository = userRepository;
            
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task <ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            var users = await this.UserRepository.GetUsersAsync();
            var usersToReturn = this.Mapper.Map<IEnumerable<MemberDTO>>(users);
            return Ok(usersToReturn);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            var user = await this.UserRepository.GetUserByUsernameAsync(username);
             var userToReturn = this.Mapper.Map<MemberDTO>(user);
           
            return Ok(userToReturn);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO member){
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await this.UserRepository.GetUserByUsernameAsync(username);

            if(user == null) return NotFound();

            this.Mapper.Map(member, user);
            if(await this.UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user.");

        }
        
    }
}
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseAPIController
    {
        public IUserRepository UserRepository { get; }
        public IMapper Mapper { get; }
        public IPhotoService PhotoService { get; }
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            this.PhotoService = photoService;
            this.Mapper = mapper;
            this.UserRepository = userRepository;
            
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task <ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUser = await this.UserRepository.GetUserByUsernameAsync(User.GetUserNAme());

            userParams.CurrentUserName = currentUser.UserName;
           
            var users = await this.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, 
            users.TotalCount, users.TotalCount));

            return Ok(users);
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
            var user = await this.UserRepository.GetUserByUsernameAsync(User.GetUserNAme());

            if(user == null) return NotFound();

            this.Mapper.Map(member, user);
            if(await this.UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user.");

        }
        
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await this.UserRepository.GetUserByUsernameAsync(User.GetUserNAme());

            if(user == null) return NotFound();

            var result = await this.PhotoService.AddPhotoAsync(file);
            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo(){
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if(user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if(await this.UserRepository.SaveAllAsync())
            {   
                return CreatedAtAction(nameof(GetUser), new {username = user.UserName},
                     this.Mapper.Map<PhotoDTO>(photo));
            }
            return BadRequest("Error adding photo");
        }   

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId){
            var user = await this.UserRepository.GetUserByUsernameAsync(User.GetUserNAme());
            if(user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x=> x.Id == photoId);

            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest ("this is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x=>x.IsMain);

            if(currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if(await this.UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting main photo");

        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId){
            var user = await this.UserRepository.GetUserByUsernameAsync(User.GetUserNAme());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId != null)
            {
                var result = await this.PhotoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null ) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            if( await this.UserRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photos");

        }
    }
}
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        public DataContext Context { get; }

        public LikesRepository(DataContext context)
        {
            this.Context = context;
        }


        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await this.Context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            string predicate = likesParams.Predicate;
            int userId = likesParams.UserId;

            var users = this.Context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = this.Context.Likes.AsQueryable();
            
            if(predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == userId);
                users = likes.Select(like => like.TargetUser);
            }

            if(predicate == "likedBy")
            {
                likes = likes.Where(like => like.TargetUserId == userId);
                users = likes.Select(like => like.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                City = user.City,
                PhotoUrl = user.Photos.FirstOrDefault(f=>f.IsMain).Url,
                Id = user.Id
            });

            return await PagedList<LikeDto>
                .CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);

        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await this.Context.Users
                .Include(x => x.LikedByUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

      
    }
}
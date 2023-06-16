using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        public DataContext Context { get; }
        private readonly IMapper mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.Context = context;
        }
        
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
           return await this.Context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
           return await this.Context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(f => f.UserName == username.ToLower());
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await this.Context.Users.Include(user => user.Photos).ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.Context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            this.Context.Entry(user).State = EntityState.Modified;
        }

        public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {


            var query = this.Context.Users.AsQueryable();
            
            query = query.Where(u => u.UserName != userParams.CurrentUserName);
            query = query.Where(u => u.Gender == userParams.Gender);
            
            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1 )); 
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge + 1 )); 
            
            query = query.Where( u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch{
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };


            return await PagedList<MemberDTO>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDTO>(this.mapper.ConfigurationProvider)
                , userParams.PageNumber
                , userParams.PageSize);

        }

        public async Task<MemberDTO> GetMemberAsync(string username)
        {
              return await this.Context.Users
                .Where(x=> x.UserName == username)
                .ProjectTo<MemberDTO>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }
    }
}
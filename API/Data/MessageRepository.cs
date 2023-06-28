using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        public IMapper Mapper { get; }
        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.Mapper = mapper;
            this.context = context;
            
        }
        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);        
        }

        public void DeleteMessage(Message message)
        {
            this.context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = this.context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where( u => u.RecipientUsername == messageParams.Username 
                                            && ! u.RecipientDeleted ),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username 
                                            && ! u.SenderDeleted ),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && !u.RecipientDeleted &&u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(this.Mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.
                CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await this.context.Messages
                .Include(s => s.Sender).ThenInclude(p => p.Photos)
                .Include(s => s.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.RecipientUsername == currentUsername && !m.RecipientDeleted  && 
                    m.SenderUsername == recipientUsername || 
                    m.RecipientUsername == recipientUsername && !m.SenderDeleted  && 
                    m.SenderUsername == currentUsername
                    
                )
                .OrderBy( m => m.MessageSent)
                .ToListAsync();
                
            var unreadMessages = messages.Where(m => m.DateRead == null  && m.RecipientUsername == currentUsername);

            if(unreadMessages.Any()){
                foreach(var message in unreadMessages){
                    message.DateRead = DateTime.UtcNow;
                }
                await this.context.SaveChangesAsync();
            }

            return this.Mapper.Map<IEnumerable<MessageDto>>(messages);

        }
        
        public async Task<bool> SaveAllAsync()
        {
            return await this.context.SaveChangesAsync() > 0;
        }
    }
}
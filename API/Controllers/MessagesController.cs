using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseAPIController
    {
        public IMapper Mapper { get; }
        public IMessageRepository MessageRepository { get; }
        public IUserRepository UserRepository { get; }
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            this.UserRepository = userRepository;
            this.MessageRepository = messageRepository;
            this.Mapper = mapper;
            
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto messageToCreate)
        {
            var username = User.GetUserNAme();
            if(username == messageToCreate.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself.");
            var sender = await this.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await this.UserRepository.GetUserByUsernameAsync(messageToCreate.RecipientUsername);

            if(recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = messageToCreate.Content
            };

            this.MessageRepository.AddMessage(message);

            if(await this.MessageRepository.SaveAllAsync())
                return Ok(this.Mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser
            ([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserNAme();
            var messages = await this.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(
                messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
            
            return messages;

        }
        
        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> Thread (string username)
        {
            var currentUsername = User.GetUserNAme();

            return Ok(await this.MessageRepository.GetMessageThread(currentUsername, username));

        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserNAme();
            var message = await this.MessageRepository.GetMessage(id);

            if(message.SenderUsername != username && message.RecipientUsername != username)
            {
                return Unauthorized();
            }

            if(message.SenderUsername == username) message.SenderDeleted = true;

            if(message.RecipientUsername == username) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted)
            {
                this.MessageRepository.DeleteMessage(message);
            }
            if(await this.MessageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem when deleting the message");

        }
    }
}
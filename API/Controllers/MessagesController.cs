using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class MessagesController : BaseAPIController 
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messagesRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messagesRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _messagesRepository = messagesRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDTO createMessageDto)
        {
            var username = User.GetUsername();

             if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You can't  send a  message to yourself");

            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient =  await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();


            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messagesRepository.AddMessage(message);

            if (await _messagesRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
            return BadRequest("Failed to send");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser ([FromQuery]
         MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await _messagesRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize,
                messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();

            return Ok(await _messagesRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await _messagesRepository.GetMessage(id);

            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();

            if (message.Sender.UserName == username) message.SenderDeleted = true;

            if (message.Recipient.UserName == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _messagesRepository.DeleteMessage(message);

            if (await _messagesRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }
        
    }
}
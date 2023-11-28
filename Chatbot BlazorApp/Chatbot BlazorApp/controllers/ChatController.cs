using Chatbot_BlazorApp_Share.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Chatbot_BlazorApp.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        private readonly IHandleChatService _handleChatService;
        public ChatController(IHandleChatService handleChatService)
        {
            _handleChatService = handleChatService;
        }

        // POST api/<ChatController>
        [HttpPost("{message}")]
        public async Task<ActionResult<string>> PostChat(string message)
        {
            string chat = await _handleChatService.GenerateChat(message);

            if( !string.IsNullOrEmpty(chat)) 
            {
                return Ok(chat);
            }
            return BadRequest();
            
        }
    }
}

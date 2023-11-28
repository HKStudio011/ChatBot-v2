using Chatbot_BlazorApp_Share.Entity;
using Chatbot_BlazorApp_Share.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Chatbot_BlazorApp.Controllers
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
        [HttpPost]
        public async Task<ActionResult<string>> PostChat(ChatMessage chatMessage)
        {
            string chat = await _handleChatService.GenerateChat(chatMessage.Message);

            if( !string.IsNullOrEmpty(chat)) 
            {
                return Ok(chat);
            }
            return BadRequest();
            
        }
    }
}

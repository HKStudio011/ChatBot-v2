using Chatbot_BlazorApp_Share.DBContext;
using Chatbot_BlazorApp_Share.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot_BlazorApp_Share.Services
{
    public class HandleChatClientService : IHandleChatService
    {
        private readonly HttpClient _httpClient;
        public HandleChatClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateChat(string message)
        {
           ChatMessage chatMessage = new ChatMessage() { Message = message };
           var result = await _httpClient.PostAsJsonAsync("/api/Chat", chatMessage);
           if (result.IsSuccessStatusCode) 
           {
                return await result.Content.ReadAsStringAsync();
           }
           else
           {
               return result.StatusCode.ToString();
           }
        }
    }
}

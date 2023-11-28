using Azure.Core;
using Chatbot_BlazorApp_Share.Entity;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot_BlazorApp_Share.Services
{
    public class HandleContentClientService : IHandleContentService
    {
        private readonly HttpClient _httpClient;
        public HandleContentClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateContentsAsync(Contents contents)
        {
            await _httpClient.PostAsJsonAsync("/api/Content", contents);
        }

        public async Task<List<Contents>> GetAllContentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Contents>>("/api/Content");
        }

        public Contents GetContentById(int id)
        {
            var task = _httpClient.GetFromJsonAsync<Contents>($"/api/Content/{id}");
            task.Wait();
            return task.Result;
        }

        public async Task UpdateContentsAsync(Contents contents)
        {
            await _httpClient.PutAsJsonAsync("/api/Content", contents);
        }

        public async Task DeleteContentsAsync(int id)
        {
            await _httpClient.DeleteAsync($"/api/Content/{id}");
        }
    }
}

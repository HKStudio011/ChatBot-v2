using Chatbot_BlazorApp_Share.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot_BlazorApp_Share.Services
{
    public interface IHandleContentService
    {
        Task<List<Contents>> GetAllContentsAsync();
        Task CreateContentsAsync(Contents contents);
        Task UpdateContentsAsync(Contents contents);
        Task DeleteContentsAsync(int id);
        Contents GetContentById(int id);
    }
}

using Chatbot_BlazorApp_Share.Entity;
using Chatbot_BlazorApp_Share.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Chatbot_BlazorApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IHandleContentService _handleContentService;
        public ContentController(IHandleContentService handleContentService)
        {
            _handleContentService = handleContentService;
        }

        // GET: api/<ContentController>
        [HttpGet]
        public IEnumerable<Contents> GetAllContents()
        {
            var task = _handleContentService.GetAllContentsAsync();
            task.Wait();
            return task.Result;
        }

        // GET api/<ContentController>/5
        [HttpGet("{id}")]
        public Contents GetContentById(int id)
        {
            return _handleContentService.GetContentById(id);
        }

        // POST api/<ContentController>
        [HttpPost]
        public async Task PostContent(Contents contents)
        {
            await _handleContentService.CreateContentsAsync(contents);
        }

        // PUT api/<ContentController>/
        [HttpPut]
        public async Task PutContent(Contents contents)
        {
            await _handleContentService.UpdateContentsAsync(contents);
        }

        // DELETE api/<ContentController>/
        [HttpDelete("{id}")]
        public async Task DeleteContentById(int id)
        {
            await _handleContentService.DeleteContentsAsync(id);
        }
    }
}

using Chatbot_BlazorApp_Share.DBContext;
using Chatbot_BlazorApp_Share.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot_BlazorApp_Share.Services
{
    public class HandleChatService : IHandleChatService
    {
        private readonly ChatbotContext _chatbotContext;
        private readonly IHandleContentService _handleContentService;
        public HandleChatService(ChatbotContext chatbotContext, IHandleContentService handleContentService)
        {
            _chatbotContext = chatbotContext;
            _handleContentService = handleContentService;
        }
        public async Task<string> GenerateChat(string message)
        {
            var contents = new Contents()
            {
                Content = message
            };
            var result = ((HandleContentService)_handleContentService).HandelContent(contents).ToList();
            ConcurrentBag<List<Keywords>> list_Keywords = result[1] as ConcurrentBag<List<Keywords>>;

            List<string> keywords = new List<string>();

            foreach (var list in list_Keywords)
            {
                foreach (var keyword in list)
                {
                    keywords.Add(keyword.KeywordNotToneMarks);
                }
            }
            return SelectContent(keywords);
        }
        class KeywordMarked
        {
            public string Keyword { get; set; }
            public int point { get; set; }
            public ConcurrentBag<Contents> ListContent { get; set; } = new();
        }

        private string SelectContent(List<string> keywords)
        {
            if (keywords.Count == 0)
            {
                return "Xin lỗi, không có thông tin liên quan đến câu hỏi của bạn =((";
            }
            ConcurrentBag<KeywordMarked> keywordMarkeds = new ConcurrentBag<KeywordMarked>();

            var temp = _chatbotContext.Keywords.ToList();
            var temp1 = _chatbotContext.SplitContents.ToList();
            var temp2 = _chatbotContext.Contents.ToList();



            //mask keyword chat vs keyword trong database
            keywords.AsParallel().ForAll(k =>
            {

                int poin = k.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length;
                poin *= poin;
                var search = temp.AsParallel().Where(i => i.KeywordNotToneMarks == k).ToList();
                if (search.Count != 0)
                {
                    KeywordMarked keywordMarked = new();
                    keywordMarked.Keyword = k;
                    keywordMarked.point = poin;
                    var content = (from kw in temp.AsParallel()
                                   join slc in temp1.AsParallel()
                                      on kw.SplitContentID equals slc.SplitContentID into gr
                                   from kwslc in gr.AsParallel()
                                   join c in temp2.AsParallel()
                                      on kwslc.ContentID equals c.ContentID
                                   where kw.KeywordNotToneMarks == k
                                   select c).Distinct().ToList();
                    if (content.Count != 0)
                    {
                        content.AsParallel().ForAll(c =>
                        {
                            keywordMarked.ListContent.Add(c);

                        });
                    }
                    keywordMarkeds.Add(keywordMarked);
                }

            });

            //giai phong ram
            temp = null;
            temp1 = null;
            temp2 = null;


            //nhom cac content giong nhau
            var contentAndPoint = new ConcurrentBag<KeyValuePair<Contents,int>>();
            keywordMarkeds.AsParallel().ForAll(k =>
            {
                k.ListContent.AsParallel().ForAll(item =>
                {
                    contentAndPoint.Add(new KeyValuePair<Contents, int>(item,k.point));
                });
            });

                            
            var grContent = contentAndPoint.AsParallel()
                                .GroupBy(x=> x.Key.ContentID).Select(x => x.ToList()).ToList();
            contentAndPoint.Clear();
            //tinh tong diem cua cac content
            grContent.AsParallel().ForAll(l_keypair =>
            {
                if (l_keypair.Count > 1)
                {
                    int total_point = l_keypair[0].Value;

                    for (int i = 1; i < l_keypair.Count; i++)
                    {
                        total_point += l_keypair[i].Value;
                    }
                    contentAndPoint.Add(new KeyValuePair<Contents, int>(l_keypair[0].Key, total_point));
                }
                else
                {
                    contentAndPoint.Add(l_keypair[0]);
                }
            });

            //tim max point
            int max = contentAndPoint.AsParallel().OrderByDescending(k => k.Value).FirstOrDefault().Value;

            if (max <= 1)
            {
                return "Xin lỗi, không có thông tin liên quan đến câu hỏi của bạn =((";
            }

            var result = contentAndPoint.AsParallel().Where(i => i.Value == max).ToList();


            if (result.Count > 3)
            {
                return "Xin lỗi, không thể cung cấp một câu trả lời chính xác vì có quá nhiều thông tin liên quan đến câu hỏi của bạn =((";
            }
            else if (result.Count > 1)
            {
                string str = "Đây là thông tin liên quan đến câu hỏi của bạn:\n\n";
                for (int i = 0; i < result.Count; i++)
                {
                    if (i == result.Count - 1)
                    {
                        str += result[i].Key.Content;
                    }
                    else
                    {
                        str += result[i].Key.Content + "\n\n";
                    }

                }

                return str;
            }
            else
            {
                return result[0].Key.Content;
            }
        }
    }
}

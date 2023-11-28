using Chatbot_BlazorApp_Share.DBContext;
using Chatbot_BlazorApp_Share.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Chatbot_BlazorApp_Share.Services
{
     public class KeywordsComparer : IEqualityComparer<Keywords>
    {
        // Keywords are equal if their names and product numbers are equal.
        public bool Equals(Keywords x, Keywords y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the Keywords' properties are equal.
            return x.KeywordNotToneMarks == y.KeywordNotToneMarks || x.Keyword == y.Keyword;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(Keywords keywords)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(keywords, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashkeywordsName = keywords.Keyword == null ? 0 : keywords.Keyword.GetHashCode();

            //Get hash code for the Code field.
            int hashPkeywordsCode = keywords.Keyword.GetHashCode();

            //Calculate the hash code for the product.
            return hashkeywordsName ^ hashPkeywordsCode;
        }
    }

    public class HandleContentService : IHandleContentService
    {
        private readonly object _locker = new object();
        private readonly ChatbotContext _chatbotContext;
        private readonly ChatbotModel _chatbotModel;
        public HandleContentService(ChatbotContext chatbotContext, ChatbotModel chatbotModel)
        {
            _chatbotContext = chatbotContext;
            _chatbotModel = chatbotModel;
        }


        public async Task CreateContentsAsync(Contents contents)
        {
            var reuslt = _chatbotContext.Contents.AsParallel().Where(c => c.Content == contents.Content).FirstOrDefault();
            if (reuslt != null) 
            {
                return;
            }
            await AddContent(contents);
        }

        public async Task CreateContentsFromFileAsync(string path)
        {
            List<string> contents = new List<string>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (File.Exists(path))
            {
                using var package = new ExcelPackage(path);

                // Tạo đối tượng Worksheet để truy cập vào trang tính cần đọc
                var sheet = package.Workbook.Worksheets[0];

                // Sử dụng vòng lặp để duyệt qua các hàng và cột của trang tính
                for (int row = 2; row <= sheet.Dimension.End.Row; row++)
                {
                    // Lấy giá trị của ô tại vị trí hàng và cột hiện tại
                    string content = sheet.Cells[row, 1].Value.ToString();
                    if(!string.IsNullOrEmpty(content))
                    {
                        contents.Add(content);
                    }
                }

                foreach (var row in contents)
                {
                    try
                    {
                        var reuslt = _chatbotContext.Contents.AsParallel().Where(c => c.Content == row).FirstOrDefault();
                        if (reuslt != null)
                        {
                            continue;
                        }
                        var content = new Contents()
                        {
                            Content = row,
                        };
                        await AddContent(content);
                    }
                    catch (Exception ex) { }
                }
            }
        }

        public async Task<List<Contents>> GetAllContentsAsync() 
        {
            return await _chatbotContext.Contents.ToListAsync();
        }

        public Contents GetContentById(int id)
        {
            return _chatbotContext.Contents.AsParallel().Where(c => c.ContentID == id).FirstOrDefault();
        }

        public async Task UpdateContentsAsync(Contents contents)
        {
            var item = await _chatbotContext.Contents.FindAsync(contents.ContentID);
            if (item != null)
            {
                await AddContent(item, isUpdate: true);
                await _chatbotContext.SaveChangesAsync();
            }
        }

        public async Task DeleteContentsAsync(int id)
        {
            var item = _chatbotContext.Contents.AsParallel().Where(c => c.ContentID == id).FirstOrDefault();
            if (item != null)
            {
                _chatbotContext.Contents.Remove(item);
                await _chatbotContext.SaveChangesAsync();
            }
        }

        private async Task AddContent(Contents contents,bool isUpdate = false)
        {
            var result = HandelContent(contents).ToList();
            ConcurrentBag<SplitContents> splitContents = result[0] as ConcurrentBag<SplitContents>;
            ConcurrentBag<List<Keywords>> list_Keywords = result[1] as ConcurrentBag<List<Keywords>>;

            lock(_locker)
            {
                if(isUpdate)
                {
                    DeleteBeforUpdate(contents.ContentID);
                    _chatbotContext.Contents.Update(contents);
                }
                else
                {
                    _chatbotContext.Contents.Add(contents);
                }
                
                foreach(var item in splitContents)
                {
                    _chatbotContext.SplitContents.Add(item);
                }
                foreach (var list in list_Keywords)
                {

                    foreach (var item in list)
                    {
                        _chatbotContext.Keywords.Add(item);
                    }
                }
            }
            await _chatbotContext.SaveChangesAsync();
        }

        public IEnumerable<object> HandelContent(Contents contents)
        {
            ConcurrentBag<SplitContents> splitContents = new ConcurrentBag<SplitContents>();
            ConcurrentBag<List<Keywords>> list_Keywords = new ConcurrentBag<List<Keywords>>();
            var list_split = SplitContent(contents.Content);
            list_split.AsParallel().ForAll(item =>
            {
                try
                {
                    SplitContents splitContent = new SplitContents()
                    {
                        SplitContent = item,
                        Content = contents,
                    };
                    splitContents.Add(splitContent);
                    list_Keywords.Add(GenerateKeyword(splitContent));
                }
                catch (Exception ex) { }
            });
            return new object[]
            {
                splitContents,list_Keywords
            };
        }

        private void DeleteBeforUpdate(int id)
        {
            var result = _chatbotContext.SplitContents.AsParallel().Where(s => s.ContentID == id).ToList();
            foreach(var item in result)
            {
                _chatbotContext.SplitContents.Remove(item);
            }
        }

        private List<Keywords> GenerateKeyword(SplitContents SplitContent)
        {
           List<Keywords> keywords = new List<Keywords>();  
           string result = _chatbotModel.GenerateKeyword(SplitContent.SplitContent);
            if(string.IsNullOrEmpty(result) )
            {
                throw new Exception("Key word null");
            }
            var split_keyword = result.Split("\n",StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);
            split_keyword =string.Join( ',',split_keyword)
                                    .Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in split_keyword) 
            {
                if(item.Length> 50)
                {
                    continue;
                }
                Keywords keyword = new Keywords()
                {
                    Keyword = item,
                    KeywordNotToneMarks = RemoveUnicode(item),
                    SplitContents = SplitContent
                };
                keywords.Add(keyword);
            }

            keywords = keywords.AsParallel().Distinct(new KeywordsComparer()).ToList();

            return keywords;

        }
        private string RemoveUnicode(string input)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ", "đ", "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ", "í", "ì", "ỉ", "ĩ", "ị", "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ", "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự", "ý", "ỳ", "ỷ", "ỹ", "ỵ", };
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "d", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "i", "i", "i", "i", "i", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "y", "y", "y", "y", "y", };
            for (int i = 0; i < arr1.Length; i++)
            {
                input = input.Replace(arr1[i], arr2[i]);
                input = input.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return input;
        }
        private List<string> SplitContent(string content)
        {
            ConcurrentBag<string> sentence = new ConcurrentBag<string>();
            var split_paragramp = content.Split("\n",StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);

            split_paragramp.AsParallel().ForAll(p =>
            {
                var split_sentence = p.Split(".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                for(int i = 0;i< split_sentence.Count-1;i++)
                {
                    if (char.IsDigit(split_sentence[i][^1]) && char.IsDigit(split_sentence[i+1][0]))
                    {
                        split_sentence[i] = string.Concat(split_sentence[i], split_sentence[i + 1]);
                        split_sentence.RemoveAt(i+1);
                        i--;
                    }
                }
                split_sentence.AsParallel().ForAll(s => sentence.Add(s));
            });

            return sentence.ToList();
        }
    }
}

using ChatBot_Generate_Data.Models;
using OfficeOpenXml;
using OfficeOpenXml.Attributes;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ChatBot_Generate_Data
{
    public class MultiTask
    {
        string path;
        string pathKeyword;
        string pathErrorVariations;
        List<string> contents;
        List<Account> accounts;
        Config config;

        class Respomse
        {
            public Respomse(string content)
            {
                Content = content;
                Result = new List<string>();
            }

            public string Content { get; set; }
            public List<string> Result { get; set; }


        }
        public MultiTask()
        {
            path = "Temp";
            pathKeyword =path + "/Keyword";
            pathErrorVariations = path + "/Error Variations";
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 
            contents = new List<string>();
            accounts = new List<Account>();
        }

        private async Task ReadConfig()
        {
            Console.WriteLine("Reading config...");
            if (File.Exists("Data/config.json"))
            {
                using var stream =new FileStream("Data/config.json",FileMode.Open);
                config = await JsonSerializer.DeserializeAsync<Config>(stream);
            }
            else
            {
                await Console.Out.WriteLineAsync("Error: config.json not exits");
                await Console.Out.WriteLineAsync("Creating new config...");
            }

            if(config == null) 
            {
                config = new Config()
                {
                    NumberTasks = accounts.Count,
                    Url = @"https://www.bing.com/search?q=Bing+AI&showconv=1"
                };
                using var stream = new FileStream("Data/config.json", FileMode.Create);
                await JsonSerializer.SerializeAsync<Config>(stream,config);
            }
        }

        private bool ReadAccount()
        {
            Console.WriteLine("Reading list account...");
            if (File.Exists("Data/account.xlsx"))
            {
                using var package = new ExcelPackage("Data/account.xlsx");

                // Tạo đối tượng Worksheet để truy cập vào trang tính cần đọc
                var sheet = package.Workbook.Worksheets[0];

                // Sử dụng vòng lặp để duyệt qua các hàng và cột của trang tính
                for (int row = 2; row <= sheet.Dimension.End.Row; row++)
                {
                    // Lấy giá trị của ô tại vị trí hàng và cột hiện tại
                    string email = sheet.Cells[row, 1].Value.ToString();
                    string password = sheet.Cells[row, 2].Value.ToString();
                    accounts.Add(new Account() { Email = email, Password = password });
                }
                return true;
            }
            Console.WriteLine("Error: cannot read account");
            return false;
        }

        private bool ReadContent()
        {
            Console.WriteLine("Reading list content...");
            if (File.Exists("Data/content.xlsx"))
            {
                using var package = new ExcelPackage("Data/content.xlsx");

                // Tạo đối tượng Worksheet để truy cập vào trang tính cần đọc
                var sheet = package.Workbook.Worksheets[0];

                // Sử dụng vòng lặp để duyệt qua các hàng và cột của trang tính
                for (int row = 2; row <= sheet.Dimension.End.Row; row++)
                {
                    // Lấy giá trị của ô tại vị trí hàng và cột hiện tại
                    string content = sheet.Cells[row, 1].Value.ToString();
                    contents.Add(content);
                }
                Console.WriteLine("Reading list content: Done");
                return true;
            }
            Console.WriteLine("Error: cannot read content");
            return false;
        }

        private List<string> SplitContent(string content)
        {
            //split paragraph in content
            //split sentence in paragraph
            List<string> result = new List<string>();
            foreach (var paragraph in content.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                foreach (var sentence in paragraph.Split(".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    result.Add(sentence);
                }
            }
            return result;
        }

        private async Task WriteTempFile(Respomse response , string pathFile)
        {
            while(true)
            {
                string fileName = Guid.NewGuid().ToString()+".json";
                if(File.Exists(Path.Combine(pathFile,fileName))) 
                {
                    continue;
                }
                using var stream = new FileStream(Path.Combine(pathFile, fileName), FileMode.Create);
                await JsonSerializer.SerializeAsync<Respomse>(stream, response);
                break;
            }
        }

        private async Task Handle(BingChat bingChat,string content,string chatPrompt,string pathTemp)
        {
            int index = 0;
            while(true)
            {
                try
                {
                    string result = await bingChat.ChatWithBingAI(chatPrompt);
                    var temp = Regex.Matches(result, @"(?<=@).*(?=@)");
                    if(temp.Count > 0)
                    {
                        Respomse respomse = new Respomse(content);
                        foreach (Match item in temp)
                        {
                            respomse.Result.Add(item.Value);
                        }

                        await WriteTempFile(respomse, pathKeyword);
                        index = 0;
                        break;
                    }
                    else
                    {
                        index++;
                    }
                    if(index >=10)
                    {
                        bingChat.Restart();
                        await bingChat.SignIn();
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Error: {ex.Message}");
                    index++;
                }
            }
        }

        private void ManageTemp()
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                else
                {
                    Directory.CreateDirectory(pathKeyword);
                    Directory.CreateDirectory(pathErrorVariations);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }

        private async Task<bool> CheckSignIn(BingChat bingchat,int index)
        {
            while (true)
            {
                //kiem tra sign in
                if (!await bingchat.SignIn())
                {
                    await Console.Out.WriteLineAsync($"Warning: Account {accounts[index].Email} cannot sign in");

                    //remove accout cannot sign in
                    accounts.RemoveAt(index);
                    // kiem tra lai so luong accout neu >= NumberTasks thi doi accout   
                    if (accounts.Count >= config.NumberTasks)
                    {
                        bingchat.Email = accounts[index].Email;
                        bingchat.Password = accounts[index].Password;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
        private async Task<List<BingChat>> InitializationBingChat()
        {
            List<BingChat> bingChats = new List<BingChat>();
            for (int i = 0; i<config.NumberTasks;i++)
            {   
                var bingchat = new BingChat(config.Url, accounts[i].Email, accounts[i].Password, 5, 10);
                bingchat.Start();
                if (await CheckSignIn(bingchat, i))
                {
                    bingChats.Add(bingchat);
                }
                else
                {
                    bingchat.Close();
                    break;
                }
            }
            return bingChats;
        }

        private void CloseBingChat(List<BingChat> bingChats)
        {
            foreach (var item in bingChats)
            {
                item.Close();
            }
            bingChats.Clear();
        }
        private async Task GenerateKeyword(List<string> splitContents)
        {
            ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
            await Console.Out.WriteLineAsync("Generating Keyword content...");
            var bingChats = await InitializationBingChat();

            for(int i =0;i< splitContents.Count;i+=bingChats.Count)
            {
                await Console.Out.WriteLineAsync($"Progess : {i}/{splitContents.Count}");
                for (int j = 0;j<bingChats.Count;j++)
                {
                    int index = i + j;
                    // index > so luong splitContents -> break
                    if (index > splitContents.Count-1)
                    {
                        break;
                    }
                    string prompt = $"""
                        Bạn là một chuyên gia phân tích văn bản và hoàn thành xuất sắc các yêu cầu.
                        Hãy phân tích văn bản  sau "{splitContents[index]}" và hoàn thành các yêu cầu:
                        1 Tạo danh sách liệt kê các từ khoá quan trọng.
                        2 Kết quả đầu ra chỉ chứa các từ khoá.
                        3 Chuyển về chữ in thường.
                        4 Tôi chỉ cần danh sách kết quả có dạng [@<từ khoá>@] ví dụ @từ khoá@
                        """;

                    tasks.Add(Handle(bingChats[j], splitContents[index], prompt, pathKeyword));
                }

                // wait task finish
                Task.WaitAll(tasks.ToArray());
                // clear list task
                tasks.Clear();
            }

            CloseBingChat(bingChats);
            await Console.Out.WriteLineAsync("Generating Keyword content: Done");
        }

        private async Task<List<string>> SplitContent()
        {
            await Console.Out.WriteLineAsync("Splitting content...");
            ConcurrentBag<List<string>> temp = new ConcurrentBag<List<string>>();
            ConcurrentBag<string > result = new ConcurrentBag<string>();

            contents.AsParallel().ForAll(content =>
            {
                try
                {
                    temp.Add(SplitContent(content));
                }
                catch(Exception ex) 
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            if(temp.Count == 0) 
            {
                await Console.Out.WriteLineAsync("Error : Cannot split content");
                return result.ToList();
            }
            else
            {
                temp.AsParallel().ForAll(content =>
                {
                    content.AsParallel().ForAll(sentence =>
                    {
                        result.Add(sentence);
                    });
                });
            }
            await Console.Out.WriteLineAsync("Splitting content: Done");
            return result.ToList();
        }

        public async Task Run()
        {
            if(!ReadAccount()) return;
            await ReadConfig();
            await Console.Out.WriteLineAsync($"Info : Url = {config.Url}");
            await Console.Out.WriteLineAsync($"Info : Account = {accounts.Count}");
            if (config.NumberTasks > accounts.Count)
            {
                await Console.Out.WriteLineAsync("Warning : Set NumberTasks = Account because NumberTasks > Account");
                config.NumberTasks = accounts.Count;
            }
            await Console.Out.WriteLineAsync($"Info : NumberTasks = {config.NumberTasks}");

            if (!ReadContent()) return;
            //create folder temp
            ManageTemp();

            await GenerateKeyword(await SplitContent());

            //cleaning
            //ManageTemp();
            Console.WriteLine("cleaning...");
            Console.WriteLine("!!!Finish!!!");
        }
    }
}

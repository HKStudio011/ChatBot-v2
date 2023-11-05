using ChatBot_Generate_Data.Models;
using OfficeOpenXml;
using OfficeOpenXml.Attributes;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChatBot_Generate_Data
{
    public class MultiTask
    {
        string path;
        string pathResult;
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

        class Result
        {
            public string Input { get; set; }
            public string Label { get; set; }
            public string Task { get; set; }
        }
        public MultiTask()
        {
            path = "Temp";
            pathResult = "Result";
            pathKeyword = path + "/Keyword";
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
                using var stream = new FileStream("Data/config.json", FileMode.Open);
                config = await JsonSerializer.DeserializeAsync<Config>(stream);
            }
            else
            {
                await Console.Out.WriteLineAsync("Error: config.json not exits");
                await Console.Out.WriteLineAsync("Creating new config...");
            }

            if (config == null)
            {
                config = new Config()
                {
                    NumberTasks = accounts.Count,
                    Url = @"https://www.bing.com/search?q=Bing+AI&showconv=1"
                };
                using var stream = new FileStream("Data/config.json", FileMode.Create);
                await JsonSerializer.SerializeAsync<Config>(stream, config);
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
                    if(sentence.Length>=6 && sentence.Contains(" "))
                    {
                        result.Add(sentence);
                    }                  
                }
            }
            return result;
        }

        private async Task WriteTempFile(Respomse response, string pathFile)
        {
            while (true)
            {
                string fileName = Guid.NewGuid().ToString() + ".json";
                if (File.Exists(Path.Combine(pathFile, fileName)))
                {
                    continue;
                }
                using var stream = new FileStream(Path.Combine(pathFile, fileName), FileMode.Create);
                await JsonSerializer.SerializeAsync<Respomse>(stream, response);
                break;
            }
        }

        private async Task HandleChat(BingChat bingChat, string content, string chatPrompt, string pathTemp)
        {
            int index = 0;
            int count = 0;
            while (true)
            {
                try
                {
                    if (index >= 5)
                    {
                        if (count >= 2) break;
                        count++;
                        bingChat.Restart();
                        await bingChat.SignIn();
                    }
                    string result = await bingChat.ChatWithBingAI(chatPrompt);
                    result = result.Replace(",", "\n");
                    var temp = Regex.Matches(result, @"@([^@]*)@");
                    if (temp.Count > 0)
                    {
                        Respomse respomse = new Respomse(content);
                        foreach (Match item in temp)
                        {
                            respomse.Result.Add(item.Groups[1].Value);
                        }

                        await WriteTempFile(respomse, pathKeyword);
                        index = 0;
                        break;
                    }
                    else
                    {
                        index++;
                        continue;
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

                Directory.CreateDirectory(pathKeyword);
                Directory.CreateDirectory(pathErrorVariations);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }

        private async Task<bool> CheckSignIn(BingChat bingchat, int index)
        {
            while (true)
            {
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

                //kiem tra sign in
                if (!await bingchat.SignIn())
                {
                    await Console.Out.WriteLineAsync($"Warning: Account {accounts[index].Email} cannot sign in");

                    //remove accout cannot sign in
                    accounts.RemoveAt(index);

                }
                else
                {
                    return true;
                }
            }
        }
        private async Task<List<BingChat>> InitializationBingChat()
        {
            ConcurrentBag<BingChat> bingChats = new ConcurrentBag<BingChat>();
            Parallel.For(0, config.NumberTasks ,i=>
            {
                var bingchat = new BingChat(config.Url, accounts[i].Email, accounts[i].Password, 5, 10);
                bingchat.Start();
                var task = CheckSignIn(bingchat, i);
                task.Wait();
                if (task.Result)
                {
                    bingChats.Add(bingchat);
                }
                else
                {
                    bingchat.Close();
                }
            });

            return bingChats.ToList();
        }

        private void CloseBingChat(List<BingChat> bingChats)
        {
            bingChats.AsParallel().ForAll(item =>
            {
                item.Close();
            });
            bingChats.Clear();
        }
        private async Task GenerateKeyword(List<string> splitContents, List<BingChat> bingChats)
        {
            ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
            await Console.Out.WriteLineAsync("Generating Keyword content...");

            for (int i = 0; i < splitContents.Count; i += bingChats.Count)
            {
                await Console.Out.WriteLineAsync($"Progess : {i}/{splitContents.Count}");
                for (int j = 0; j < bingChats.Count; j++)
                {
                    int index = i + j;
                    // index > so luong splitContents -> break
                    if (index > splitContents.Count - 1)
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

                    tasks.Add(HandleChat(bingChats[j], splitContents[index], prompt, pathKeyword));
                }
                // wait task finish
                Task.WaitAll(tasks.ToArray());
                // clear list task
                tasks.Clear();
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }

            await Console.Out.WriteLineAsync($"Progess : {splitContents.Count}/{splitContents.Count}");
            await Console.Out.WriteLineAsync("Generating Keyword content: Done");
        }

        private async Task<List<Respomse>> ReadResponeFile(string pathFolder)
        {
            ConcurrentBag<Respomse> responeses = new ConcurrentBag<Respomse>();
            if (Directory.Exists(pathFolder))
            {
                await Console.Out.WriteLineAsync($"Reading file in {pathFolder}...");
                try
                {
                    var listFile = Directory.GetFiles(pathFolder);
                    listFile.AsParallel().ForAll(file =>
                    {
                        using var stream = new FileStream(file, FileMode.Open);
                        var task = JsonSerializer.DeserializeAsync<Respomse>(stream);
                        task.AsTask().Wait();
                        var temp = task.Result;
                        if(temp != null)
                        {
                            responeses.Add(temp);
                        }
                    });
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Error: {ex.Message}");
                }
                await Console.Out.WriteLineAsync($"Reading file in {pathFolder}: Done");
            }
            else
            {
                await Console.Out.WriteLineAsync($"Error: {pathFolder} not exits");
            }
            return responeses.ToList();
        }

        private void EndProgram()
        {
            Console.WriteLine("!!!Close program after 5s!!!");
            var task = Task.Delay(TimeSpan.FromSeconds(5));
            task.Wait();
            Environment.Exit(0);
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

        private async Task GenerateErrorVariation(List<string> splitKeywords)
        {
            ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
            await Console.Out.WriteLineAsync("Generating error variation...");
            List<string> character = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            var random = new Random();

            splitKeywords.AsParallel().ForAll(keyword =>
            {
                ConcurrentBag<string> result = new ConcurrentBag<string>();
                Respomse respomse = new Respomse(keyword);
                keyword = RemoveUnicode(keyword).ToLower();
                result.Add(keyword);
                // tao  NumberErrorVariations bien the
                Parallel.For(0, config.NumberErrorVariations, i =>
                {
                    var temp = keyword.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);                
                    // moi bien the thi co random so lan thay doi theo so tu cua bien the
                    for (int j = 0; j < random.Next(temp.Length); j++)
                    {
                        var index = random.Next(temp.Length);
                        // vi tri chen = random theo do dai +1
                        temp[index] = temp[index].Insert(random.Next(temp[index].Length + 1), character[random.Next(character.Count)]);
                        result.Add(string.Join(" ", temp));
                    }
                });
                respomse.Result = result.ToList();
                tasks.Add(WriteTempFile(respomse, pathErrorVariations));
                result.Clear();
            });

            // wait task finish
            Task.WaitAll(tasks.ToArray());
            // clear list task
            tasks.Clear();
            await Console.Out.WriteLineAsync("Generating error variation: Done");
        }

        private async Task<List<string>> SplitKeywords(List<Respomse> respomses)
        {
            await Console.Out.WriteLineAsync("Splitting keyword...");
            ConcurrentBag<string> result = new ConcurrentBag<string>();

            respomses.AsParallel().ForAll(respomse =>
            {
                respomse.Result.AsParallel().ForAll(item =>
                {
                    result.Add(item);
                });
            });

            if (result.Count == 0)
            {
                await Console.Out.WriteLineAsync("Error : Cannot split content");
                return result.ToList();
            }

            await Console.Out.WriteLineAsync("Splitting keyword: Done");
            return result.ToList();
        }

        private async Task<List<string>> SplitContents()
        {
            await Console.Out.WriteLineAsync("Splitting content...");
            ConcurrentBag<List<string>> temp = new ConcurrentBag<List<string>>();
            ConcurrentBag<string> result = new ConcurrentBag<string>();

            contents.AsParallel().ForAll(content =>
            {
                try
                {
                    temp.Add(SplitContent(content));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            if (temp.Count == 0)
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

        private async Task<List<Result>> DataSynthesis()
        {
            ConcurrentBag<Result> results = new ConcurrentBag<Result>();
            var data1 = await ReadResponeFile(pathKeyword);
            var data2 = await ReadResponeFile(pathErrorVariations);

            await Console.Out.WriteLineAsync("Data synthesis...");

            if (data1.Count == 0 || data2.Count == 0)
            {
                Console.WriteLine("Error: Temp is null");
                EndProgram();
            }

            //content keyword sinh từ khoá từ nội dung
            //Danh sách keyword content sinh nội dung từ khoá
            data1.AsParallel().ForAll(item =>
            {
                item.Result.AsParallel().ForAll(keyword =>
                {
                    Result result = new Result()
                    {
                        Input = item.Content,
                        Label = keyword,
                        Task = "Sinh từ khoá từ nội dung: "
                    };

                    results.Add(result);
                });

                Result result = new Result()
                {
                    Input = string.Join(",",item.Result),
                    Label = item.Content,
                    Task = "Sinh nội dung từ khoá: "
                };

                results.Add(result);
            });

            //biến thể keyword sửa lỗi chính tả hoặc lỗi đánh máy

            data2.AsParallel().ForAll(item =>
            {
                item.Result.AsParallel().ForAll(errorCariation =>
                {
                    Result result = new Result()
                    {
                        Input = errorCariation,
                        Label = item.Content,
                        Task = "Sửa lỗi chính tả hoặc lỗi đánh máy: "
                    };

                    results.Add(result);
                });

            });
            await Console.Out.WriteLineAsync("Data synthesis: Done");
            return results.ToList();
        }

        private void WriteData(List<Result> results)
        {
            Console.WriteLine($"Writing data in {pathResult}...");
            if (!Directory.Exists(pathResult))
            {
                Directory.CreateDirectory(pathResult);
            }

            // Tạo đối tượng ExcelPackage
            using ExcelPackage excelPackage = new ExcelPackage();

            // Tạo trang tính
            ExcelWorksheet sheet = excelPackage.Workbook.Worksheets.Add("data");

            // Thêm dữ liệu vào trang tính
            sheet.Cells["A1"].Value = "input";
            sheet.Cells["B1"].Value = "label";
            sheet.Cells["C1"].Value = "task";

            // Sử dụng vòng lặp để duyệt qua các hàng và cột của trang tính
            for (int i = 0; i < results.Count; i++)
            {
                //excel bat dau tu 1 va da ghi 1 dong tieu de cot => 2
                int row = i + 2;
                // Lấy giá trị của ô tại vị trí hàng và cột hiện tại
                sheet.Cells[row, 1].Value = results[i].Input;
                sheet.Cells[row, 2].Value = results[i].Label;
                sheet.Cells[row, 3].Value = results[i].Task;

            } 

            // Lưu file Excel
            excelPackage.SaveAs(Path.Combine(pathResult,"data.xlsx"));
            Console.WriteLine($"Writing data in {pathResult}: Done");
        }

        public async Task Run()
        {
            if (!ReadAccount()) return;
            await ReadConfig();
            await Console.Out.WriteLineAsync($"Info : Url = {config.Url}");
            await Console.Out.WriteLineAsync($"Info : Account = {accounts.Count}");
            if (config.NumberTasks > accounts.Count)
            {
                await Console.Out.WriteLineAsync("Warning : Set Number Tasks = Account because NumberTasks > Account");
                config.NumberTasks = accounts.Count;
            }
            await Console.Out.WriteLineAsync($"Info : Number tasks = {config.NumberTasks}");
            await Console.Out.WriteLineAsync($"Info : Number error variation = {config.NumberErrorVariations}");

            if (!ReadContent()) return;
            //create folder temp
            ManageTemp();

            //generate data

            Console.WriteLine("Starting selenium...");
            var bingChats = await InitializationBingChat();
            Console.WriteLine("Starting selenium: Done");


            var spiltContents = await SplitContents();
            if (spiltContents.Count != 0)
            {
                await GenerateKeyword(spiltContents, bingChats);
                spiltContents.Clear();
            }
            else
            {
                Console.WriteLine("Error: Spilt Contents is null");
                EndProgram();
            }

            Console.WriteLine("Closing selenium...");
            CloseBingChat(bingChats);
            Console.WriteLine("Closing selenium: Done");

            var responeses = await ReadResponeFile(pathKeyword);
            if (responeses.Count != 0)
            {
                var splitKeywords = await SplitKeywords(responeses);
                await GenerateErrorVariation(splitKeywords);
                responeses.Clear();
            }
            else
            {
                Console.WriteLine("Error: Responeses is null");
                EndProgram();
            }

            var data = await DataSynthesis();

            WriteData(data);
            //cleaning
            Console.WriteLine("Cleaning...");
            ManageTemp();
            Console.WriteLine("!!!Finish!!!");
        }
    }
}

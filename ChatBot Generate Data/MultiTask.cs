using ChatBot_Generate_Data.Models;
using OfficeOpenXml;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatBot_Generate_Data
{
    public class MultiTask
    {   
        List<string> contents;
        List<Account> accounts;
        Config config;
        public MultiTask()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 
            contents = new List<string>();
            accounts = new List<Account>();
        }

        public async Task ReadConfig()
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
                    Url = @"https://www.bing.com/search?form=NTPCHB&q=Bing+AI&showconv=1"
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

        public bool ReadContent()
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
                return true;
            }
            Console.WriteLine("Error: cannot read content");
            return false;
        }

        public async Task Run()
        {
            if(!ReadAccount()) return;
            if(!ReadContent()) return;
            await ReadConfig();
            await Console.Out.WriteLineAsync($"Info : NumberTasks = {config.NumberTasks}");
            await Console.Out.WriteLineAsync($"Info : Url = {config.Url}");
            await Console.Out.WriteLineAsync($"Info : Account = {accounts.Count}");


        }
    }
}

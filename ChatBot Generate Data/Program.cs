using ChatBot_Generate_Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

string email = "";
string password = "";
string url = "";

email = "trucngonghinh2002@gmail.com";
password = "aA170220021593578800Aa";
url = @"https://www.bing.com/search?form=NTPCHB&q=Bing+AI&showconv=1";

GenerateData generateData = new GenerateData(url);
if (!(await generateData.SignIn(email, password)))
{
    Console.ReadKey();
    generateData.chromeDriver.Quit();
    return;
}

await generateData.ChatWithBingAI("Việt nam nằm ở đâu");

await Task.Delay(TimeSpan.FromSeconds(20));
generateData.chromeDriver.Quit();

using ChatBot_Generate_Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

MultiTask multiTask = new MultiTask();
await multiTask.Run();
Console.ReadKey();


//Console.WriteLine(await chat.ChatWithBingAI(content));

//content = """

//    """;

//Console.WriteLine(await chat.ChatWithBingAI(content));


//await Task.Delay(TimeSpan.FromSeconds(20));
//chat.chromeDriver.Quit();

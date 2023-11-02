using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using static System.Net.Mime.MediaTypeNames;

namespace ChatBot_Generate_Data
{
    public class GenerateData
    {
        public readonly ChromeDriver chromeDriver; 
        public GenerateData(string url) 
        {
            chromeDriver = new ChromeDriver();
            chromeDriver.Url = url;
        }

        public async Task<bool> SignIn(string email, string password)
        {
            int index = 0;
            while (index < 10)
            {
                try
                {
                    //login
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    var loginbutton = chromeDriver.FindElement(By.XPath("//*[@id=\"id_a\"]"));
                    loginbutton.Click();

                    await Task.Delay(TimeSpan.FromSeconds(2));
                    var emailinput = chromeDriver.FindElement(By.XPath("//*[@id=\"i0116\"]"));
                    emailinput.SendKeys(email);

                    var netxbutton = chromeDriver.FindElement(By.XPath("//*[@id=\"idSIButton9\"]"));
                    netxbutton.Click();

                    await Task.Delay(TimeSpan.FromSeconds(2));
                    var passinput = chromeDriver.FindElement(By.XPath("//*[@id=\"i0118\"]"));
                    passinput.SendKeys(password);

                    netxbutton = chromeDriver.FindElement(By.XPath("//*[@id=\"idSIButton9\"]"));
                    netxbutton.Click();

                    await Task.Delay(TimeSpan.FromSeconds(2));
                    netxbutton = chromeDriver.FindElement(By.XPath("//*[@id=\"idSIButton9\"]"));
                    netxbutton.Click();

                    await Task.Delay(TimeSpan.FromSeconds(2));

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    index++;
                }
            }

            return false;
        }

        private bool Verify(ISearchContext conversationSD)
        {
            while(true)
            {
                try
                {
                    var checkbox = conversationSD.FindElement(By.CssSelector("#cib-conversation-main")).GetShadowRoot()
                                        .FindElements(By.CssSelector("#cib-chat-main > cib-chat-turn"))[^1].GetShadowRoot()
                                        .FindElement(By.CssSelector("cib-message-group.response-message-group")).GetShadowRoot()
                                        .FindElement(By.CssSelector("iframe")).GetShadowRoot()
                                        .FindElement(By.CssSelector("#cf-chl-widget-ox1rb")).GetShadowRoot()
                                        .FindElement(By.CssSelector("#challenge-stage > div > label > input[type=checkbox]"));
                    checkbox.Click();
                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }

        public async Task<string> ChatWithBingAI(string content)
        {

            int index = 0;
            while(index<10)
            {
                try
                {
                    //chat
                    chromeDriver.Navigate().Refresh();
                    
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    var conversation = chromeDriver.FindElement(By.XPath("//*[@id=\"b_sydConvCont\"]/cib-serp"));
                    
                    //get shawdow-root
                    var conversationSD = conversation.GetShadowRoot();

                    if (!Verify(conversationSD))
                    {
                        index++;
                        continue;
                    }

                    var seletmode = conversationSD.FindElement(By.CssSelector("#cib-conversation-main")).GetShadowRoot()
                                        .FindElement(By.CssSelector("#cib-chat-main > cib-welcome-container")).GetShadowRoot()
                                        .FindElement(By.CssSelector("div.controls > cib-tone-selector")).GetShadowRoot()
                                        .FindElement(By.CssSelector("#tone-options > li:nth-child(3) > button"));
                    seletmode.Click();

                    var input = conversationSD.FindElement(By.CssSelector("#cib-action-bar-main")).GetShadowRoot()
                                        .FindElement(By.CssSelector("div > div.main-container > div > div.input-row > cib-text-input")).GetShadowRoot()
                                        .FindElement(By.CssSelector("#searchboxform"))
                                        .FindElement(By.CssSelector("#searchbox"));
                    input.SendKeys(content);

                    var sumitbutton = conversationSD.FindElement(By.CssSelector("#cib-action-bar-main")).GetShadowRoot()
                                        .FindElement(By.CssSelector("div > div.main-container > div > div.bottom-controls > div.bottom-right-controls > div.control.submit > button"));
                    sumitbutton.Click();
                    
                    await Task.Delay(TimeSpan.FromSeconds(10));

                    if (!Verify(conversationSD))
                    {
                        index++;
                        continue;
                    }

                    var stopResponding = conversationSD.FindElement(By.CssSelector("#cib-action-bar-main")).GetShadowRoot()
                          .FindElement(By.CssSelector("div > cib-typing-indicator"));

                    var stopRespondingButton = conversationSD.FindElement(By.CssSelector("#cib-action-bar-main")).GetShadowRoot()
                                        .FindElement(By.CssSelector("div > cib-typing-indicator")).GetShadowRoot()
                                        .FindElement(By.CssSelector("#stop-responding-button"));

                    while (stopResponding.GetAttribute("aria-hidden")=="false")
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        stopResponding = conversationSD.FindElement(By.CssSelector("#cib-action-bar-main")).GetShadowRoot()
                          .FindElement(By.CssSelector("div > cib-typing-indicator"));
                    }

                    var rewult = conversationSD.FindElement(By.CssSelector("#cib-conversation-main")).GetShadowRoot()
                                        .FindElements(By.CssSelector("#cib-chat-main > cib-chat-turn"));
                    string messageStr = "";
                    if (rewult.Count>0)
                    {
                        var message = rewult[^1].GetShadowRoot()
                                   .FindElement(By.CssSelector("cib-message-group.response-message-group"))
                                   .FindElement(By.CssSelector("cib-message:nth-child(3)"))
                                   .FindElement(By.CssSelector("cib-shared"))
                                   .FindElement(By.CssSelector("#entity-image-top > div.ac-textBlock"));
                        messageStr = message.Text;
                    }              



                    return messageStr;
                }
                catch
                {
                    index++;
                }
            }
            return "";


        }
    }
}

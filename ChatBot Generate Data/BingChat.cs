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
    public class BingChat
    {
        public readonly ChromeDriver chromeDriver;
        public readonly string url;
        public readonly int timeWaitLong;
        public readonly int timeWaitShort;
        public BingChat(string url,int timeWaitShort=2, int timeWaitLong=5) 
        {
            chromeDriver = new ChromeDriver();
            chromeDriver.Url = url;
            this.timeWaitLong = timeWaitLong;
            this.timeWaitShort = timeWaitShort;
            this.url = url;
        }

        public void Restart()
        {
            chromeDriver.Quit();
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
                    await Task.Delay(TimeSpan.FromSeconds(timeWaitLong));
                    var loginbutton = chromeDriver.FindElement(By.XPath("//*[@id=\"id_a\"]"));
                    loginbutton.Click();

                    await Task.Delay(TimeSpan.FromSeconds(timeWaitShort));
                    var emailinput = chromeDriver.FindElement(By.XPath("//*[@id=\"i0116\"]"));
                    emailinput.SendKeys(email);

                    var netxbutton = chromeDriver.FindElement(By.XPath("//*[@id=\"idSIButton9\"]"));
                    netxbutton.Click();

                    await Task.Delay(TimeSpan.FromSeconds(timeWaitShort));
                    var passinput = chromeDriver.FindElement(By.XPath("//*[@id=\"i0118\"]"));
                    passinput.SendKeys(password);

                    netxbutton = chromeDriver.FindElement(By.XPath("//*[@id=\"idSIButton9\"]"));
                    netxbutton.Click();

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(timeWaitShort));
                        netxbutton = chromeDriver.FindElement(By.CssSelector("#btnAskLater"));
                        netxbutton.Click();
                    }
                    catch { }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(timeWaitShort));
                        netxbutton = chromeDriver.FindElement(By.CssSelector("#btnAskLater"));
                        netxbutton.Click();
                    }
                    catch { }

                    await Task.Delay(TimeSpan.FromSeconds(timeWaitShort));
                    netxbutton = chromeDriver.FindElement(By.XPath("//*[@id=\"idSIButton9\"]"));
                    netxbutton.Click();

                    await Task.Delay(TimeSpan.FromSeconds(timeWaitShort));

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Re sigin in");
                    chromeDriver.Navigate().GoToUrl(url);
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
                    
                    await Task.Delay(TimeSpan.FromSeconds(timeWaitLong));
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
                    
                    foreach(var item in content.Split('\n'))
                    {
                        input.SendKeys(item);
                        input.SendKeys(Keys.Shift + Keys.Enter);
                    }
                    input.SendKeys(Keys.Shift+Keys.Enter);

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
                        var messageGruop = rewult[^1].GetShadowRoot()
                                   .FindElement(By.CssSelector("cib-message-group.response-message-group")).GetShadowRoot()
                                   .FindElements(By.CssSelector("cib-message"));
                        int indexMessage = 0;
                        int indexRole = 1;

                        if(messageGruop.Count>=3 && messageGruop.Count < 5)
                        {
                            indexMessage = 2;
                            indexRole = 3;
                        }
                        else if(messageGruop.Count >= 1 && messageGruop.Count < 3)
                        {
                            indexMessage = 0;
                            indexRole = 1;
                        }

                        try
                        {
                            var message = messageGruop[indexMessage].GetShadowRoot()
                                .FindElement(By.CssSelector("cib-shared > div > div"));
                            messageStr = message.Text;
                        }
                        catch
                        {
                            var message = messageGruop[indexMessage].GetShadowRoot()
                                .FindElement(By.CssSelector("#entity-image-top > div.ac-textBlock"));
                            messageStr = message.Text;
                        }


                        try
                        {
                            messageGruop[indexRole].GetShadowRoot()
                                   .FindElement(By.CssSelector("cib-shared > div > cib-muid-consent")).GetShadowRoot()
                                   .FindElement(By.CssSelector("div.get-started-btn-wrapper-inline > button ")).Click();
                        }
                        catch { }
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

using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Chatbot_BlazorApp.Services
{
    public class ChatbotModel
    {
        private Py.GILState gil;
        public string PathModel { get; set; }
        public int NumberThread { get; private set; }
        public string Version { get; private set; }
        public dynamic Model { get; private set; }
        private int count = 0;
        private readonly object lock_in = new object();
        private readonly object lock_out = new object();
        public ChatbotModel(string PathModel = "../Model/mt5-base-fireturning-three-task", int NumberThread = 4, string Version = "python311.dll")
        {
            this.PathModel = PathModel;
            // so luong luong chat cung luc
            this.NumberThread = NumberThread;
            // dll python
            this.Version = Version;
        }

        public void ClosePythonEndine()
        {
            try
            {
                Console.WriteLine("Close python engine");
                PythonEngine.Shutdown();
            }
            catch
            { }
        }

        public async Task Init()
        {
            string dll_name = File.ReadAllText("Python/version.txt");
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", dll_name, EnvironmentVariableTarget.Process);

            await Console.Out.WriteLineAsync("Start python engine...");
            PythonEngine.Initialize();
            gil = Py.GIL();

            dynamic sys = Py.Import("sys");
            await Console.Out.WriteLineAsync("Python version: " + sys.version);

            await Console.Out.WriteLineAsync("Init model...");
            dynamic module = Py.Import("Python.Model");

            Model = module.ChatBot(data_path: "", model_name_ouput: PathModel);
            Model.InitGenerate();
            Model.InitGenerates();

            await Console.Out.WriteLineAsync("Init model: Done ");

            PythonEngine.BeginAllowThreads();
        }

        public string GenerateKeyword(string request)
        {
            lock (lock_in)
            {
                while (count >= NumberThread)
                {
                    var task = Task.Delay(1000);
                    task.Wait();
                }
                count++;
            }
            var result = "";
            try
            {
                result = Model.GenerateKeyword(request);
                result = "\n" + Model.GenerateKeywords(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                lock (lock_out)
                {
                    count--;
                    if (count < 0) count = 0;
                }
            }
            return result;
        }
    }
}

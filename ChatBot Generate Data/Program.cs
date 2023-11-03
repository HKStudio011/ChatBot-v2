using ChatBot_Generate_Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

MultiTask multiTask = new MultiTask();
await multiTask.Run();
Console.WriteLine("!!!Finish!!!");
Console.ReadKey();

//string email = "";
//string password = "";
//string url = "";


//BingChat chat = new BingChat(url,5,10);
//if (!(await chat.SignIn(email, password)))
//{
//    Console.ReadKey();
//    chat.chromeDriver.Quit();
//    return;
//}

//string content = """
//    Bạn là một chuyên gia phân tích văn bản và hoàn thành xuất sắc các yêu cầu.
//    Hãy phân tích văn bản  sau "Phố đi bộ TP HCM đông nghịt người hóa trang đêm Halloween" và hoàn thành các yêu cầu:
//    1 Tạo danh sách liệt kê các từ khoá quan trọng.
//    2 Kết quả đầu ra chỉ chứa các từ khoá.
//    3 Chuyển về chữ in thường.
//    4 Tôi chỉ cần danh sách kết quả có dạng [@<từ khoá>@] ví dụ @từ khoá@
//    """;

//Console.WriteLine(await chat.ChatWithBingAI(content));

//content = """
//    Bạn là một chuyên gia ngôn ngữ, bạn thường tạo ra các biến thể là các lỗi chỉnh tả hoặc lỗi đánh máy của các từ hoặc cụm từ với mục tiêu là giúp mọi người dùng nắm bắt và không sử dụng những biến thể lỗi mà bạn tạo ra. Các biến thể do bạn tạo ra rất quan trọng trong việc xâu dựng một hệ thống phát hiện lỗi chính tả hoặc lỗi đánh máy. Nhiệm vụ của bạn là hoàn thành các bước dưới đây và chỉ đưa ra kết quả của bước 3:
//    l Bỏ dấu tiếng việt của các từ hoặc cụm từ sau "ngày sinh nhật".
//    2 Tạo ra 25 biến thể là các lỗi chính tả hoặc lỗi đánh máy ở dạng chữ in thường từ kết quả đã bỏ dấu ở bước 1.
//    3 Đâu ra có dạng [@<từ đã bỏ dấu>:<các biến thể>@] ví dụ @từ khoá:các biến thể@
//    """;

//Console.WriteLine(await chat.ChatWithBingAI(content));


//await Task.Delay(TimeSpan.FromSeconds(20));
//chat.chromeDriver.Quit();

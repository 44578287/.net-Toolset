using System.Data.SQLite;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;
using Spectre.Console;
using 小工具集;
using static Azure.Core.HttpHeader;
using static 小工具集.Windows;
using static 小工具集.Windows.SQL;

//Console.WriteLine(LnkToPath(@"C:\Users\g9964\Desktop\阿里巴巴DNS检测工具.lnk"));
/*foreach (var Data in GetStartMenuAppArray()!)
{
    Console.WriteLine(Data);
}*/
//Console.WriteLine(小工具集.Windows.Path.ThisAppPath);
//Console.WriteLine(await Network.DownloadFileWithProgress("https://d-ssl.dtstatic.com/uploads/blog/202109/24/20210924081215_63853.thumb.300_0.jpeg_webp"));
//Console.WriteLine(await Network.DownloadFileWithProgress("https://az764295.vo.msecnd.net/stable/660393deaaa6d1996740ff4880f1bad43768c814/VSCodeUserSetup-x64-1.80.0.exe"));

/*foreach (var Data in Network.GetIpAddress())
{ 
    Console.WriteLine($"{Data.Key} {Data.Value.IPV4} {Data.Value.IPV6}");
}*/

//Console.WriteLine(Network.GetPublicIP());
//Unzip(小工具集.Windows.Path.ThisAppPath+ "/快易享-Webp.rar", 小工具集.Windows.Path.ThisAppPath+"/A");
//Unzip(小工具集.Windows.Path.ThisAppPath + "/快易享-Webp.rar");

//Windows.User.GetAdminMode();

//Console.WriteLine(小工具集.Windows.Encrypted.GetTextGuid(@"C:\Users\g9964\Downloads\SW_DVD9_Win_Server_STD_CORE_2022_2108.17_64Bit_ChnSimp_DC_STD_MLF_X23-35655.ISO"));
//Console.WriteLine(小工具集.Windows.Encrypted.GetFilexxHash(@"C:\Users\g9964\Desktop\1\1.txt"));
//Console.WriteLine(小工具集.Windows.Encrypted.GetFilexxHash(@"C:\Users\g9964\Desktop\1 - 副本.txt"));
/*
//string data = "你好";
string data = @"C:\Users\g9964\Downloads\virtio-win-0.1.229.iso";
//string data = @"C:\Users\g9964\Desktop\1.txt";
Console.WriteLine("原文: "+data);
byte[] key = 小工具集.Windows.Encrypted.GenerateRandomKey(32);
string Key = 小工具集.Windows.Encrypted.KeyToStringBase64(key);
Console.WriteLine("密钥: "+Key);
byte[] Skey;
string edata = 小工具集.Windows.Encrypted.EncryptedText(data, key,out Skey);
//小工具集.Windows.Encrypted.EncryptFile(data, data+"E", key,out Skey);
//小工具集.Windows.Encrypted.EncryptFileAsync(data, data + "E", key);
Console.WriteLine("一次密钥: " + 小工具集.Windows.Encrypted.KeyToStringBase64(Skey));
//Console.WriteLine("454454654564546");
//Console.WriteLine("密文: "+edata);
//string Edata = 小工具集.Windows.Encrypted.DeclassifyText(edata, 小工具集.Windows.Encrypted.StringToKeyBase64(Key), Skey);
//Console.WriteLine("原文: "+ Edata);
//小工具集.Windows.Encrypted.DecryptFile(data + "E",data+"D", key,Skey);
*/
/*
byte[] mySimpleTextAsBytes = Encoding.UTF8.GetBytes("你好");

// Do not use these key and nonce values in your own code!
byte[] key = 小工具集.Windows.Encrypted.GenerateRandomKey(32);
byte[] nonce = 小工具集.Windows.Encrypted.GenerateRandomKey(12);
uint counter = 1;

// Encrypt
ChaCha20 forEncrypting = new ChaCha20(key, nonce, counter);
byte[] encryptedContent = new byte[mySimpleTextAsBytes.Length];
forEncrypting.EncryptBytes(encryptedContent, mySimpleTextAsBytes);
string Temp = Convert.ToBase64String(encryptedContent);
Console.WriteLine(Temp);
byte[] Tempb = Convert.FromBase64String(Temp);
// Decrypt
ChaCha20 forDecrypting = new ChaCha20(key, nonce, counter);
byte[] decryptedContent = new byte[encryptedContent.Length];
forDecrypting.DecryptBytes(decryptedContent, Tempb);
Console.WriteLine(Encoding.UTF8.GetString(decryptedContent));*/
/*var response = await Send(new("http://127.0.0.1:5250/API/GetUserListData"), "{\r\n  \"object\": \"ID\",\r\n  \"conditions\": \"2\"\r\n}", HttpMode:HttpMode.POST,SendDataType: SendDataType.Json);
// 检查响应是否成功
if (response.IsSuccessStatusCode)
{
    // 读取响应内容
    var responseContent = await response.Content.ReadAsStringAsync();
    Console.WriteLine(responseContent);
}
else
{
    Console.WriteLine("请求失败，状态码：" + response.StatusCode);
}*/
/*using var client = new HttpClient();
var Data = await client.GetAsync("https://www.google.com");
Console.WriteLine(await Data.Content.ReadAsStringAsync());*/

// 创建 Stopwatch 实例
Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
//Task T= Network.Download.DownloadFileProgressBar("https://cloud.445720.xyz/f/35oSV/w64devkit.rar", "D:\\Projects\\vs2022\\小工具集\\工具测试\\bin\\Debug\\net7.0\\w64devkit.rar");
Network.Download download = new Network.Download();
//Task<bool> T = download.DownloadFileBase("https://cloud.445720.xyz/f/35oSV/w64devkit.rar", "D:\\Projects\\vs2022\\小工具集\\工具测试\\bin\\Debug\\net7.0\\w64devkit.rar");
Task T=  download.DownloadFileLine(new() 
{
    new(){Url = "https://cloud.445720.xyz/f/35oSV/w64devkit.rar",Name = "D:\\Projects\\vs2022\\小工具集\\工具测试\\bin\\Debug\\net7.0\\w64devkit1.rar"},
    new(){Url = "https://cloud.445720.xyz/f/35oSV/w64devkit.rar",Name = "D:\\Projects\\vs2022\\小工具集\\工具测试\\bin\\Debug\\net7.0\\w64devkit2.rar"}
});
while (T.Status != TaskStatus.RanToCompletion)
{
    AnsiConsole.WriteLine(小工具集.Windows.Text.ConvertByteUnits(download.DownloadFileDatas[0].CurrentLength));
    Thread.Sleep(500);
}
T.Wait();

stopwatch.Stop();
// 获取经过的时间
TimeSpan elapsed = stopwatch.Elapsed;
// 输出执行时间
Console.WriteLine($"代码执行时间: {elapsed.TotalMilliseconds} 毫秒");
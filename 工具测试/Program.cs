using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using 小工具集;
using static 小工具集.Windows;
using static 小工具集.Windows.File;

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

Windows.User.GetAdminMode();
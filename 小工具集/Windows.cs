using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using CSChaCha20;
using LoongEgg.LoongLogger;
using SharpCompress.Archives;
using SharpCompress.Common;
using Spectre.Console;
using Standart.Hash.xxHash;
using static 小工具集.Windows.Code;
using static 小工具集.Windows.Network.HttpEnum;

namespace 小工具集
{
    /// <summary>
    /// Windows用
    /// </summary>
    public static class Windows
    {
        /// <summary>
        /// 路径
        /// </summary>
        public static class Path
        {
            /// <summary>
            /// 当前用户文件夹路径
            /// </summary>
            public static string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            /// <summary>
            /// 当前用户下载文件夹路径
            /// </summary>
            public static string DownloadPath = System.IO.Path.Combine(UserPath, "Downloads");
            /// <summary>
            /// 当前用户下桌面文件夹路径
            /// </summary>
            public static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            /// <summary>
            /// 当前用户文件文件夹路径
            /// </summary>
            public static string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            /// <summary>
            /// 当前用户下视频文件夹路径
            /// </summary>
            public static string VideosPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            /// <summary>
            /// 当前用户下图片文件夹路径
            /// </summary>
            public static string PicturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            /// <summary>
            /// 当前用户下音频文件夹路径
            /// </summary>
            public static string MusicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            /// <summary>
            /// x64安装位置
            /// </summary>
            public static string ProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            /// <summary>
            /// x86安装位置
            /// </summary>
            public static string ProgramFilesPathX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            /// <summary>
            /// 开始菜单位置
            /// </summary>
            public static string StartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            /// <summary>
            /// 系统开机自起文件位置
            /// </summary>
            public static string StartupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            /// <summary>
            /// 自身文件位置
            /// </summary>
            public static string ThisAppFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;
            /// <summary>
            /// 自身文件位置路径
            /// </summary>
            public static string ThisAppPath = AppDomain.CurrentDomain.BaseDirectory;
        }
        /// <summary>
        /// 文件相关
        /// </summary>
        public static class File
        {
            /// <summary>
            /// 从快捷方式中获取目标路径
            /// </summary>
            /// <param name="Path">快捷方式文件路径</param>
            /// <returns>目标文件路径</returns>
            /// <exception cref="Exception">内部错误</exception>
            public static string? LnkToPath(string Path)
            {
                string? targetPath = null;
                try
                {
                    if (System.IO.File.Exists(Path))
                    {
                        IWshRuntimeLibrary.WshShell shell = new();
                        IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(Path);
                        targetPath = shortcut.TargetPath;
                    }
                    else
                    {
                        throw new Exception("文件不存在!");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return targetPath;
            }
            /// <summary>
            /// 获取开始菜单或指定目录的所有快捷方式
            /// </summary>
            /// <param name="Path">目录位置(可选)</param>
            /// <returns>所有快捷方式</returns>
            /// <exception cref="Exception">内部错误</exception>
            public static string[]? GetStartMenuAppArray(string? Path = null)
            {
                Path ??= @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";
                string[]? AppArray = null;
                try
                {
                    string searchPattern = "*.lnk"; // 要匹配的文件模式
                    AppArray = Directory.GetFiles(Path, searchPattern, SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return AppArray;
            }
            /// <summary>
            /// 文件夹探针(监控变化)
            /// </summary>
            /// <param name="Path">监控目标</param>
            /// <param name="Filter">目标类型</param>
            /// <param name="NotifyFilter">监视变化类型</param>
            /// <param name="IncludeSubdirectories">是否包含子文件夹</param>
            /// <returns>文件探针</returns>
            public static FileSystemWatcher FileProbes(
                string Path,
                string Filter = "*.*",
                NotifyFilters NotifyFilter = NotifyFilters.LastWrite
                                            | NotifyFilters.FileName
                                            | NotifyFilters.DirectoryName,
                bool IncludeSubdirectories = true
                )
            {
                // 创建一个新的FileSystemWatcher对象
                FileSystemWatcher watcher = new FileSystemWatcher();
                // 设置要监视的文件夹路径
                watcher.Path = Path;
                // 设置要监视的文件类型（可选）
                watcher.Filter = Filter;
                // 设置要监视文件的更改类型
                watcher.NotifyFilter = NotifyFilter;
                // 设置是否包括子文件夹
                watcher.IncludeSubdirectories = IncludeSubdirectories;
                // 开始监视
                watcher.EnableRaisingEvents = true;

                return watcher;
            }
            /// <summary>
            /// 万能解压
            /// </summary>
            /// <param name="ZipPath">压缩包位置</param>
            /// <param name="ObjectivePath">解压位置(未指定则仅检视压缩包内容)</param>
            /// <param name="Pwd">压缩包密码</param>
            /// <param name="Overwrite">覆盖目标文件</param>
            /// <returns>文件列表</returns>
            public static Dictionary<string, IArchiveEntry> Unzip(
                string ZipPath,
                string? ObjectivePath = null,
                string? Pwd = null,
                bool Overwrite = true
                )
            {
                Dictionary<string, IArchiveEntry> TempData = new();
                if (!System.IO.Directory.Exists(ObjectivePath) & ObjectivePath != null)//自动创建解压文件夹
                {
                    Directory.CreateDirectory(ObjectivePath!);
                }
                using (var stream = System.IO.File.OpenRead(ZipPath))//读取压缩文件
                {
                    SharpCompress.Readers.ReaderOptions? ReaderOptions = new()
                    {
                        LeaveStreamOpen = true, // 保持流打开，以便在解压缩后删除源文件
                        Password = Pwd // 设置密码
                    };
                    using (SharpCompress.Archives.IArchive? archive = SharpCompress.Archives.ArchiveFactory.Open(stream, ReaderOptions))
                    {
                        if (ObjectivePath != null)
                        {
                            foreach (SharpCompress.Archives.IArchiveEntry entry in archive.Entries)
                            {
                                if (!entry.IsDirectory)
                                {
                                    entry.WriteToDirectory(ObjectivePath, new ExtractionOptions
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = Overwrite
                                    });
                                }
                                TempData.Add(entry.Key, entry);
                            }
                        }
                        else
                        {
                            foreach (SharpCompress.Archives.IArchiveEntry entry in archive.Entries)
                            {
                                TempData.Add(entry.Key, entry);
                            }
                        }
                    }
                    return TempData;
                }
            }
            /// <summary>
            /// 异步运行应用
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="Args">启动指令</param>
            /// <param name="ExternalRuns">外部运行</param>
            /// <param name="Hide">隐藏外部运行</param>
            /// <returns>进程</returns>
            public static async Task<Process?> StartAppAsync(string Path, string? Args = null, bool ExternalRuns = false, bool Hide = false)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = Path;
                startInfo.Arguments = Args;
                startInfo.CreateNoWindow = Hide; // 设置为 false，使应用程序显示控制台窗口

                if (ExternalRuns)
                {
                    startInfo.CreateNoWindow = Hide; // 设置为 false，使应用程序显示控制台窗口
                    startInfo.UseShellExecute = true; // 设置为 true，使用操作系统的外壳程序启动应用程序
                }

                startInfo.UseShellExecute = ExternalRuns; // 设置为 true，使用操作系统的外壳程序启动应用程序
                Process? process = await Task.Run(() => Process.Start(startInfo));
                return process;
            }
            /// <summary>
            /// 异步运行应用
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="StartInfo">启动信息</param>
            /// <returns>进程</returns>
            public static async Task<Process?> StartAppAsync(string Path, ProcessStartInfo StartInfo)
            {
                ProcessStartInfo _StartInfo = StartInfo;
                _StartInfo.FileName = Path;

                Process? process = await Task.Run(() => Process.Start(_StartInfo));
                return process;
            }
            /// <summary>
            /// 异步运行应用
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="Args">启动指令</param>
            /// <param name="StartInfo">启动信息</param>
            /// <returns>进程</returns>
            public static async Task<Process?> StartAppAsync(string Path, string Args, ProcessStartInfo StartInfo)
            {
                ProcessStartInfo _StartInfo = StartInfo;
                _StartInfo.FileName = Path;
                _StartInfo.Arguments = Args;

                Process? process = await Task.Run(() => Process.Start(_StartInfo));
                return process;
            }
            /// <summary>
            /// 异步运行应用
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="Args">启动指令</param>
            /// <returns>进程</returns>
            public static async Task<Process?> StartAppAsyncNoShow(string Path, string? Args = null)
            {
                ProcessStartInfo _StartInfo = new();
                _StartInfo.FileName = Path;
                _StartInfo.Arguments = Args;

                // 隐藏外部程序的窗口
                _StartInfo.CreateNoWindow = true;
                _StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // 禁用外部程序的标准输出和错误输出
                _StartInfo.RedirectStandardOutput = true;
                _StartInfo.RedirectStandardError = true;
                _StartInfo.UseShellExecute = false;

                Process? process = await Task.Run(() => Process.Start(_StartInfo));
                return process;
            }
            /// <summary>
            /// 异步运行应用
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="Args">启动指令</param>
            /// <returns>进程</returns>
            public static string StartAppAsyncNoShowRData(string Path, string? Args = null)
            {
                // 创建一个新的进程实例
                Process process = new Process();

                // 设置要运行的外部程序的路径和参数
                process.StartInfo.FileName = Path;
                process.StartInfo.Arguments = Args;

                // 隐藏外部程序的窗口
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // 禁用外部程序的标准输出和错误输出
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;

                // 启动外部程序
                process.Start();

                // 等待外部程序完成
                process.WaitForExit();

                // 获取外部程序的输出
                string output = process.StandardOutput.ReadToEnd();
                //string error = process.StandardError.ReadToEnd();

                // 处理输出和错误信息
                // ...

                // 关闭进程
                process.Close();

                return output;
            }

            /// <summary>
            /// 运行并监控应用
            /// </summary>
            public class AppProbes
            {
                string _Path = "";
                string? _Args = null;
                /// <summary>
                /// 初始化
                /// </summary>
                /// <param name="Path">文件路径</param>
                /// <param name="Args">启动信息</param>
                public AppProbes(string Path, string? Args = null)
                {
                    _Path = Path;
                    _Args = Args;
                }
                /// <summary>
                /// 日志输出监控
                /// </summary>
                public event Action<string?>? LogOut;
                /// <summary>
                /// 进程
                /// </summary>
                public Process _Process = new();
                /// <summary>
                /// 开始运行并监控
                /// </summary>
                public void Start()
                {
                    // 创建一个新的进程
                    _Process = new Process();

                    // 设置进程启动信息
                    _Process.StartInfo.FileName = _Path;
                    _Process.StartInfo.Arguments = _Args;
                    _Process.StartInfo.UseShellExecute = false;
                    _Process.StartInfo.RedirectStandardOutput = true;  // 重定向输出

                    // 创建一个线程来实时读取输出数据
                    Thread readOutputThread = new Thread(() =>
                    {
                        while (!_Process.StandardOutput.EndOfStream)
                        {
                            string? output = _Process.StandardOutput.ReadLine();

                            // 处理输出数据
                            //Console.WriteLine(output);
                            LogOut?.Invoke(output);//丢到外部
                                                   // 根据输出数据判断当前运行到哪一步
                                                   // ...
                        }
                    });

                    // 启动进程
                    _Process.Start();

                    // 启动读取输出数据的线程
                    readOutputThread.Start();

                    // 等待进程执行完成
                    _Process.WaitForExit();
                }
            }
            /// <summary>
            /// 运行并监控应用
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="Args">启动信息</param>
            /// <param name="action">信息捕获</param>
            public static void StartAppProbes(string Path, string? Args = null, Action<string?>? action = null)
            {
                // 创建一个新的进程
                Process process = new Process();

                // 设置进程启动信息
                process.StartInfo.FileName = Path;
                process.StartInfo.Arguments = Args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;  // 重定向输出
                // 创建一个线程来实时读取输出数据
                Thread readOutputThread = new Thread(() =>
                {
                    while (true)
                    {
                        string? output = process.StandardOutput.ReadLine();

                        // 处理输出数据
                        //Console.WriteLine(output);
                        action?.Invoke(output);//丢到外部
                        // 根据输出数据判断当前运行到哪一步
                        // ...
                    }
                });

                // 启动进程
                process.Start();

                // 启动读取输出数据的线程
                readOutputThread.Start();

                // 等待进程执行完成
                process.WaitForExit();
            }
        }
        /// <summary>
        /// 网络相关
        /// </summary>
        public static class Network
        {
            /// <summary>
            /// 下载文件
            /// </summary>
            /// <param name="Url">地址</param>
            /// <param name="Path">保存路径</param>
            /// <param name="Name">保存文件名</param>
            /// <param name="progress">下载进度</param>
            /// <returns>文件路径</returns>
            public static async Task<string?> DownloadFile(string Url, string? Path = null, string? Name = null, IProgress<double>? progress = null)
            {
                Path ??= Windows.Path.ThisAppPath;
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Url))
                        using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                        {
                            // 确保响应成功
                            response.EnsureSuccessStatusCode();

                            // 从响应中获取文件名
                            Name ??= response.Content.Headers.ContentDisposition?.FileName ?? System.IO.Path.GetFileName(new Uri(Url).LocalPath);

                            // 过滤文件名
                            string invalidChars = new string(System.IO.Path.GetInvalidFileNameChars());
                            Name = string.Concat(Name.Split(invalidChars.ToCharArray()));

                            Name = System.IO.Path.Combine(Path, Name);

                            // 获取响应内容流
                            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                            {
                                // 获取文件总大小
                                long? totalBytes = response.Content.Headers.ContentLength;

                                // 创建文件流
                                using (FileStream fileStream = new FileStream(Name, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    byte[] buffer = new byte[4096];
                                    long bytesCopied = 0;
                                    int bytesRead;
                                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                    {
                                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                                        bytesCopied += bytesRead;
                                        if (totalBytes.HasValue && progress != null)
                                        {
                                            // 使用已下载的大小计算进度
                                            double percentage = (double)bytesCopied / totalBytes.Value * 100;
                                            progress.Report(percentage);
                                        }
                                    }
                                }
                            }

                            return Name;
                        }
                    }
                    catch (Exception ex)
                    {
                        Spectre.Console.AnsiConsole.WriteLine(ex.Message);
                        // 可以根据实际需求进行异常处理，返回更详细的错误信息给调用者
                        throw new Exception("下载文件失败：" + ex.Message);
                    }
                }
            }
            /// <summary>
            /// 带有进度条的下在文件
            /// </summary>
            /// <param name="Url">文件地址</param>
            /// <param name="Path">保存路径</param>
            /// <param name="Name">保存文件名</param>
            /// <returns>保存文件路径</returns>
            public static async Task<string?> DownloadFileWithProgress(string Url, string? Path = null, string? Name = null)
            {
                Path ??= Windows.Path.ThisAppPath;

                // 发送HTTP请求并获取文件总大小
                long? totalBytes = await GetFileSizeAsync(Url);

                // 创建进度报告对象
                ProgressTracker progressTracker = new ProgressTracker(totalBytes);

                // 创建进度报告回调函数
                IProgress<double> progress = new Progress<double>(percentage =>
                {
                    progressTracker.UpdateProgress(percentage);
                    long? downloadedBytes = progressTracker.DownloadedBytes;
                    double speed = progressTracker.DownloadSpeed;

                    // 显示下载进度、已下载大小和下载速度
                    Spectre.Console.AnsiConsole.WriteLine($"下载进度：{percentage.ToString("0.0")}% - {GetSizeString(downloadedBytes)}/{GetSizeString(totalBytes)} - {GetSizeString((long?)speed)}/s");
                });

                // 下载文件并返回文件路径
                return await DownloadFile(Url, Path, Name, progress);
            }
            private static async Task<long?> GetFileSizeAsync(string Url)
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, Url))
                    using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        return response.Content.Headers.ContentLength;
                    }
                }
            }
            private static string GetSizeString(long? size)
            {
                if (size == null || size <= 0)
                    return "0B";

                string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
                int suffixIndex = 0;
                double bytes = (double)size.Value;

                while (bytes >= 1024 && suffixIndex < suffixes.Length - 1)
                {
                    bytes /= 1024;
                    suffixIndex++;
                }

                return $"{bytes:F1}{suffixes[suffixIndex]}";
            }
            class ProgressTracker
            {
                private readonly long? totalBytes;
                private long downloadedBytes;
                private Stopwatch stopwatch;

                public ProgressTracker(long? totalBytes)
                {
                    this.totalBytes = totalBytes;
                    downloadedBytes = 0;
                    stopwatch = Stopwatch.StartNew();
                }

                public long? DownloadedBytes => downloadedBytes;

                public double DownloadSpeed
                {
                    get
                    {
                        double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        if (elapsedSeconds > 0 && downloadedBytes > 0)
                        {
                            double speed = downloadedBytes / elapsedSeconds;
                            return speed;
                        }

                        return 0;
                    }
                }

                public void UpdateProgress(double percentage)
                {
#pragma warning disable CS8629 // 可为 null 的值类型可为 null。
                    downloadedBytes = (long)(totalBytes * (percentage / 100));
#pragma warning restore CS8629 // 可为 null 的值类型可为 null。
                }
            }

            /// <summary>
            /// Http请求
            /// </summary>
            public static class HttpRequest
            {
                /// <summary>
                /// 发送请求
                /// </summary>
                /// <param name="Uri">地址</param>
                /// <param name="Data">发送数据</param>
                /// <param name="HttpMode">请求模式</param>
                /// <param name="SendDataType">发送目标内容</param>
                /// <param name="HeaderType">发送请求头设置</param>
                /// <returns>Http响应</returns>
                /// <exception cref="Exception">WTF</exception>
                public static async Task<HttpResponseMessage> Send(Uri Uri, object? Data = null, HttpMode HttpMode = HttpMode.GET, SendDataType SendDataType = SendDataType.String, HeaderType HeaderType = HeaderType.Text)
                {
                    // 创建HttpClient实例
                    using var client = new HttpClient();
                    HttpResponseMessage response;
                    object? content = null;
                    //设置请求头发送数据类型
                    if (Data != null || HttpMode != HttpMode.GET)
                    {
                        try
                        {
                            switch (SendDataType)
                            {
                                case SendDataType.String:
                                    // 设置要发送的JSON数据
                                    HeaderType = HeaderType.Text;
                                    content = new StringContent((string)Data!, Encoding.UTF8, HeaderType.GetDescription());
                                    break;
                                case SendDataType.Json:
                                    HeaderType = HeaderType.Json;
                                    content = new StringContent((string)Data!, Encoding.UTF8, HeaderType.GetDescription());
                                    break;
                                case SendDataType.File_Bytes:
                                    HeaderType = HeaderType.Stream;
                                    content = new ByteArrayContent((byte[])Data!);
                                    break;
                                case SendDataType.File_Stream:
                                    HeaderType = HeaderType.Stream;
                                    content = new StreamContent((Stream)Data!);
                                    break;
                                case SendDataType.File_Path:
                                    HeaderType = HeaderType.Stream;
                                    FileStream fileStream = System.IO.File.OpenRead((string)Data!);
                                    content = new StreamContent(fileStream);
                                    // 设置Content-Disposition头部，指定文件名
                                    client.DefaultRequestHeaders.Add("Content-Disposition", $"attachment; filename=\"{System.IO.Path.GetFileName(Data as string)}\"");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Spectre.Console.AnsiConsole.WriteLine($"Error----{ex.Message}-----大概率是Data传值类型错了");
                        }
                        //设置请求头发送数据类型
                        //client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(HeaderType.GetDescription()));
                    }
                    //选择发送模式
                    switch (HttpMode)
                    {
                        case HttpMode.GET:
                            response = await client.GetAsync(Uri);
                            break;
                        case HttpMode.POST:
                            response = await client.PostAsync(Uri, ConvertObj(content!, AsType(SendDataType)!));
                            break;
                        case HttpMode.PUT:
                            response = await client.PutAsync(Uri, ConvertObj(content!, AsType(SendDataType)!));
                            break;
                        case HttpMode.DELETE:
                            response = await client.DeleteAsync(Uri);
                            break;
                        default: throw new Exception("大哥牛皮无中生有");
                    }
                    return response;
                }
                /// <summary>
                /// 通过类型指定输出对象类型
                /// </summary>
                /// <param name="SendDataType">指定类型</param>
                /// <returns>该对象转换后类型</returns>
                private static Type AsType(SendDataType SendDataType)
                {
                    switch (SendDataType)
                    {
                        case SendDataType.String: return typeof(StringContent);
                        case SendDataType.Json: return typeof(StringContent);
                        case SendDataType.File_Bytes: return typeof(byte[]);
                        case SendDataType.File_Stream: return typeof(Stream);
                    }
                    return typeof(object);
                }
            }
            /// <summary>
            /// 跟Http有关的枚举
            /// </summary>
            public static class HttpEnum
            {
                /// <summary>
                /// Http请求模式
                /// </summary>
                public enum HttpMode
                {
                    /// <summary>
                    /// GET请求
                    /// </summary>
                    GET,
                    /// <summary>
                    /// POST请求
                    /// </summary>
                    POST,
                    /// <summary>
                    /// PUT请求
                    /// </summary>
                    PUT,
                    /// <summary>
                    /// DELETE请求
                    /// </summary>
                    DELETE
                }
                /// <summary>
                /// 发送内容类型
                /// </summary>
                public enum SendDataType
                {
                    /// <summary>
                    /// 字符串
                    /// </summary>
                    String,
                    /// <summary>
                    /// Json
                    /// </summary>
                    Json,
                    /// <summary>
                    /// 文件流
                    /// </summary>
                    File_Stream,
                    /// <summary>
                    /// 文件Bytes
                    /// </summary>
                    File_Bytes,
                    /// <summary>
                    /// 文件路径
                    /// </summary>
                    File_Path
                }
                /// <summary>
                /// 请求头类型
                /// </summary>
                public enum HeaderType
                {
                    /// <summary>
                    /// application/json
                    /// </summary>
                    [Description("application/json")]
                    Json,
                    /// <summary>
                    /// text/plain
                    /// </summary>
                    [Description("text/plain")]
                    Text,
                    /// <summary>
                    /// text/xml
                    /// </summary>
                    [Description("text/xml")]
                    Xml,
                    /// <summary>
                    /// text/html
                    /// </summary>
                    [Description("text/html")]
                    Html,
                    /// <summary>
                    /// application/octet-stream
                    /// </summary>
                    [Description("application/octet-stream")]
                    Stream,
                    /// <summary>
                    /// application/x-www-form-urlencoded
                    /// </summary>
                    [Description("application/x-www-form-urlencoded")]
                    Form,
                }
            }

            /// <summary>
            /// 获取所有网卡的IP
            /// </summary>
            /// <returns>网卡名+IP的字典</returns>
            public static Dictionary<string, IpAddress> GetIpAddress()
            {
                Dictionary<string, IpAddress> TempData = new();
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                        foreach (UnicastIPAddressInformation ipInfo in ipProperties.UnicastAddresses)
                        {
                            if (!TempData.ContainsKey(networkInterface.Name))
                            {
                                TempData.Add(networkInterface.Name, new());
                            }
                            if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                TempData[networkInterface.Name].IPV4 = ipInfo.Address.ToString();
                            }
                            if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                            {
                                TempData[networkInterface.Name].IPV6 = ipInfo.Address.ToString();
                            }
                        }
                    }
                }
                return TempData;
            }
            /// <summary>
            /// IP地址
            /// </summary>
            public class IpAddress
            {
                /// <summary>
                /// IPV4
                /// </summary>
                public string? IPV4 { get; set; }
                /// <summary>
                /// IPV6
                /// </summary>
                public string? IPV6 { get; set; }
            }
            /// <summary>
            /// 获取公网IP
            /// </summary>
            /// <param name="Url">API地址</param>
            /// <returns>公网IP地址</returns>
            public static string? GetPublicIP(string? Url = null)
            {
                Url ??= "https://api.ipify.org";
                HttpClient httpClient = new HttpClient();
                return httpClient.GetStringAsync(Url).Result;
            }
        }
        /// <summary>
        /// 用户相关
        /// </summary>
        public static class User
        {

            /// <summary>
            /// 获取管理员权限
            /// </summary>
            /// <param name="Close"></param>
            /// <returns>是否成功</returns>
            public static bool GetAdminMode(bool Close = true)
            {
#pragma warning disable CA1416 // 验证平台兼容性
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (!isAdmin)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = Path.ThisAppFilePath;
                    startInfo.UseShellExecute = true;
                    startInfo.Verb = "runas"; // 请求管理员权限

                    try
                    {
                        // 启动新的进程
                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        Spectre.Console.AnsiConsole.WriteLine("无法请求管理员权限：" + ex.Message);
                        return false;
                    }
                    if (Close)
                        Environment.Exit(0);
                }
                return true;
#pragma warning restore CA1416 // 验证平台兼容性
            }
        }
        /// <summary>
        /// 文本相关
        /// </summary>
        public static class Text
        {
            /// <summary>
            /// 自动转换文件大小单位
            /// </summary>
            /// <param name="bytes">字节</param>
            /// <param name="manualUnit">指定单位(未指定自动转换)</param>
            /// <param name="decimalPlaces">小数点位数</param>
            /// <returns>转换后字符串</returns>
            public static string ConvertByteUnits(double bytes, string manualUnit = "", int decimalPlaces = 2)
            {
                double convertedValue = bytes;
                string convertedUnit = "B";

                // 根据手动指定的单位进行转换
                if (!string.IsNullOrEmpty(manualUnit))
                {
                    switch (manualUnit.ToLower())
                    {
                        case "kb":
                            convertedValue /= 1024;
                            convertedUnit = "KB";
                            break;
                        case "mb":
                            convertedValue /= Math.Pow(1024, 2);
                            convertedUnit = "MB";
                            break;
                        case "gb":
                            convertedValue /= Math.Pow(1024, 3);
                            convertedUnit = "GB";
                            break;
                        case "tb":
                            convertedValue /= Math.Pow(1024, 4);
                            convertedUnit = "TB";
                            break;
                        default:
                            throw new ArgumentException("无效的单位");
                    }

                    return Math.Round(convertedValue, decimalPlaces).ToString() + " " + convertedUnit;
                }

                // 自动转换为推荐单位
                string[] units = { "B", "KB", "MB", "GB", "TB" };
                int unitIndex = 0;
                while (convertedValue >= 1024 && unitIndex < units.Length - 1)
                {
                    convertedValue /= 1024;
                    unitIndex++;
                }

                if (convertedValue < 0.01 && unitIndex > 0)
                {
                    // 当转换后的值小于 0.01 且不是最小单位时，将单位回退一个级别
                    convertedValue *= 1024;
                    unitIndex--;
                }

                return Math.Round(convertedValue, decimalPlaces).ToString() + " " + units[unitIndex];
            }
            /// <summary>
            /// 转换单位并输出时间字符串
            /// </summary>
            /// <param name="milliseconds">毫秒</param>
            /// <param name="manualUnit">指定单位(未指定自动转换)</param>
            /// <param name="decimalPlaces">小数点位数</param>
            /// <returns>转换后的时间字符串</returns>
            public static string ConvertTimeUnits(double milliseconds, string manualUnit = "", int decimalPlaces = 2)
            {
                double convertedValue = milliseconds;
                string convertedUnit = "ms";

                // 根据手动指定的单位进行转换
                if (!string.IsNullOrEmpty(manualUnit))
                {
                    switch (manualUnit.ToLower())
                    {
                        case "s":
                            convertedValue /= 1000;
                            convertedUnit = "s";
                            break;
                        case "us":
                            convertedValue *= 1000;
                            convertedUnit = "us";
                            break;
                        case "min":
                            convertedValue /= 60000;
                            convertedUnit = "min";
                            break;
                        case "hr":
                            convertedValue /= 3600000;
                            convertedUnit = "hr";
                            break;
                        case "day":
                            convertedValue /= 86400000;
                            convertedUnit = "day";
                            break;
                        default:
                            throw new ArgumentException("无效的单位");
                    }

                    return Math.Round(convertedValue, decimalPlaces).ToString() + " " + convertedUnit;
                }

                // 自动转换为推荐单位
                string[] units = { "ms", "s", "us", "min", "hr", "day" };
                int unitIndex = 0;
                while (convertedValue >= 1 && unitIndex < units.Length - 1)
                {
                    convertedValue /= 1000;
                    unitIndex++;
                }

                if (convertedValue < 0.01 && unitIndex > 0)
                {
                    // 当转换后的值小于 0.01 且不是最小单位时，将单位回退一个级别
                    convertedValue *= 1000;
                    unitIndex--;
                }

                return Math.Round(convertedValue, decimalPlaces).ToString() + " " + units[unitIndex];
            }
            /// <summary>
            /// 转换单位并输出时间字中文符串
            /// </summary>
            /// <param name="milliseconds">毫秒</param>
            /// <param name="manualUnit">指定单位(未指定自动转换-中文)</param>
            /// <param name="decimalPlaces">小数点位数</param>
            /// <returns>转换后的时间字符串</returns>
            public static string ConvertTimeUnitsCh(double milliseconds, string manualUnit = "", int decimalPlaces = 2)
            {
                double convertedValue = milliseconds;
                string convertedUnit = "毫秒";

                // 根据手动指定的单位进行转换
                if (!string.IsNullOrEmpty(manualUnit))
                {
                    switch (manualUnit.ToLower())
                    {
                        case "秒":
                            convertedValue /= 1000;
                            convertedUnit = "秒";
                            break;
                        case "微秒":
                            convertedValue *= 1000;
                            convertedUnit = "微秒";
                            break;
                        case "分钟":
                            convertedValue /= 60000;
                            convertedUnit = "分钟";
                            break;
                        case "小时":
                            convertedValue /= 3600000;
                            convertedUnit = "小时";
                            break;
                        case "天":
                            convertedValue /= 86400000;
                            convertedUnit = "天";
                            break;
                        default:
                            throw new ArgumentException("无效的单位");
                    }

                    return Math.Round(convertedValue, decimalPlaces).ToString() + " " + convertedUnit;
                }

                // 自动转换为推荐单位
                string[] units = { "毫秒", "秒", "微秒", "分钟", "小时", "天" };
                int unitIndex = 0;
                while (convertedValue >= 1 && unitIndex < units.Length - 1)
                {
                    convertedValue /= 1000;
                    unitIndex++;
                }

                if (convertedValue < 0.01 && unitIndex > 0)
                {
                    // 当转换后的值小于 0.01 且不是最小单位时，将单位回退一个级别
                    convertedValue *= 1000;
                    unitIndex--;
                }

                return Math.Round(convertedValue, decimalPlaces).ToString() + " " + units[unitIndex];
            }
            /// <summary>
            /// 转换时间单位并输出时间字符串
            /// 0天 0小时 3分钟 40秒 111毫秒
            /// </summary>
            /// <param name="milliseconds">毫秒</param>
            /// <returns>转换后的时间字符串</returns>
            public static string ConvertTimeUnitsStr(double milliseconds)
            {
                double totalSeconds = milliseconds / 1000;

                int days = (int)(totalSeconds / 86400);
                int hours = (int)((totalSeconds % 86400) / 3600);
                int minutes = (int)(((totalSeconds % 86400) % 3600) / 60);
                int seconds = (int)(((totalSeconds % 86400) % 3600) % 60);
                int ms = (int)(milliseconds % 1000);

                string timeString = $"总运行时长: {days}天 {hours}小时 {minutes}分钟 {seconds}秒 {ms}毫秒";

                return timeString;
            }

        }
        /// <summary>
        /// 加密相关
        /// </summary>
        public static class Encrypted
        {
            /// <summary>
            /// 获取文本的xxHash
            /// </summary>
            /// <param name="Data">文本内容</param>
            /// <param name="Seed">种籽</param>
            /// <returns>值</returns>
            public static ulong GetTextxxHash(string Data, ulong Seed = 0)
            {
                byte[] data = Encoding.UTF8.GetBytes(Data);
                return xxHash3.ComputeHash(data, data.Length, Seed);
            }
            /// <summary>
            /// 获取文本的Guid
            /// </summary>
            /// <param name="Data">文本内容</param>
            /// <param name="Seed">种籽</param>
            /// <returns>值</returns>
            public static Guid GetTextGuid(string Data, ulong Seed = 0)
            {
                byte[] data = Encoding.UTF8.GetBytes(Data);
                return xxHash128.ComputeHash(data, data.Length, Seed).ToGuid();
            }
            /// <summary>
            /// 获取文件的xxHash
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="Seed">种籽</param>
            /// <returns>值</returns>
            public static ulong GetFilexxHash(string Path, ulong Seed = 0)
            {
                // 使用FileStream读取文件内容并转换为byte[]
                byte[] data;
                using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        data = br.ReadBytes((int)fs.Length);
                    }
                }
                return xxHash3.ComputeHash(data, data.Length, Seed);
            }
            /// <summary>
            /// 获取文件的Guid
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="Seed">种籽</param>
            /// <returns>值</returns>
            public static Guid GetFileGuid(string Path, ulong Seed = 0)
            {
                // 使用FileStream读取文件内容并转换为byte[]
                byte[] data;
                using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        data = br.ReadBytes((int)fs.Length);
                    }
                }
                return xxHash128.ComputeHash(data, data.Length, Seed).ToGuid();
            }
            /// <summary>
            /// 加密文本
            /// </summary>
            /// <param name="Data">文本内容</param>
            /// <param name="Key">密钥</param>
            /// <param name="Nonce">一次性密钥</param>
            /// <param name="Counter">计数器</param>
            /// <returns>加密文本</returns>
            public static string EncryptedText(string Data, byte[] Key, out byte[] Nonce, uint Counter = 1)
            {
                Nonce = GenerateRandomKey(12);//若没指定则自动生成一次性密钥
                byte[] data = Encoding.UTF8.GetBytes(Data);
                ChaCha20 Encrypting = new ChaCha20(Key, Nonce, Counter);
                byte[] reData = new byte[data.Length];
                Encrypting.EncryptBytes(reData, data);
                return Convert.ToBase64String(reData);
            }
            /// <summary>
            /// 解密文本
            /// </summary>
            /// <param name="Data">加密内容</param>
            /// <param name="Key">密钥</param>
            /// <param name="Nonce">一次性密钥</param>
            /// <param name="Counter">计数器</param>
            /// <returns>原文</returns>
            public static string DeclassifyText(string Data, byte[] Key, byte[] Nonce, uint Counter = 1)
            {
                byte[] data = Convert.FromBase64String(Data);
                ChaCha20 Decrypting = new ChaCha20(Key, Nonce, Counter);
                byte[] reData = new byte[data.Length];
                Decrypting.DecryptBytes(reData, data);
                return Encoding.UTF8.GetString(reData);
            }
            /// <summary>
            /// 加密文件
            /// </summary>
            /// <param name="filePath">文件路径</param>
            /// <param name="encryptedFilePath">加密后的文件路径</param>
            /// <param name="key">密钥</param>
            /// <param name="nonce">一次性密钥</param>
            /// <param name="counter">计数器</param>
            /// <param name="chunking">分块</param>
            public static void EncryptFile(string filePath, string encryptedFilePath, byte[] key, out byte[] nonce, uint counter = 1, int chunking = 1024)
            {
                nonce = GenerateRandomKey(12); // 若没指定则自动生成一次性密钥

                using (var inputStream = System.IO.File.OpenRead(filePath))
                using (var outputStream = System.IO.File.Create(encryptedFilePath))
                {
                    ChaCha20 Encrypt = new ChaCha20(key, nonce, counter);
                    Encrypt.EncryptStream(outputStream, inputStream, chunking);
                }

            }
            /// <summary>
            /// 解密文件
            /// </summary>
            /// <param name="encryptedFilePath">加密文件路径</param>
            /// <param name="decryptedFilePath">解密后的文件路径</param>
            /// <param name="key">密钥</param>
            /// <param name="nonce">一次性密钥</param>
            /// <param name="counter">计数器</param>
            /// <param name="chunking">分块</param>
            public static void DecryptFile(string encryptedFilePath, string decryptedFilePath, byte[] key, byte[] nonce, uint counter = 1, int chunking = 1024)
            {
                using (var inputStream = System.IO.File.OpenRead(encryptedFilePath))
                using (var outputStream = System.IO.File.Create(decryptedFilePath))
                {
                    ChaCha20 Decrypting = new ChaCha20(key, nonce, counter);
                    Decrypting.DecryptStream(outputStream, inputStream, chunking);
                }
            }
            /// <summary>
            /// 异步加密文件
            /// </summary>
            /// <param name="filePath">文件路径</param>
            /// <param name="encryptedFilePath">加密后的文件路径</param>
            /// <param name="key">密钥</param>
            /// <param name="counter">计数器</param>
            /// <param name="chunking">分块</param>
            /// <returns>一次性密钥</returns>
            public static async Task<byte[]> EncryptFileAsync(string filePath, string encryptedFilePath, byte[] key, uint counter = 1, int chunking = 1024)
            {
                byte[] nonce = GenerateRandomKey(12); // 若没指定则自动生成一次性密钥

                using (var inputStream = System.IO.File.OpenRead(filePath))
                using (var outputStream = System.IO.File.Create(encryptedFilePath))
                {
                    ChaCha20 Encrypt = new ChaCha20(key, nonce, counter);
                    await Encrypt.EncryptStreamAsync(outputStream, inputStream, chunking);
                }
                return nonce;
            }
            /// <summary>
            /// 异步解密文件
            /// </summary>
            /// <param name="encryptedFilePath">加密文件路径</param>
            /// <param name="decryptedFilePath">解密后的文件路径</param>
            /// <param name="key">密钥</param>
            /// <param name="nonce">一次性密钥</param>
            /// <param name="counter">计数器</param>
            /// <param name="chunking">分块</param>
            public static async Task DecryptFileAsync(string encryptedFilePath, string decryptedFilePath, byte[] key, byte[] nonce, uint counter = 1, int chunking = 1024)
            {
                using (var inputStream = System.IO.File.OpenRead(encryptedFilePath))
                using (var outputStream = System.IO.File.Create(decryptedFilePath))
                {
                    ChaCha20 Decrypting = new ChaCha20(key, nonce, counter);
                    await Decrypting.DecryptStreamAsync(outputStream, inputStream, chunking);
                }
            }


            /// <summary>
            /// 生成指定位数密钥
            /// </summary>
            /// <param name="sizeInBytes">Byte数(要/8)</param>
            /// <returns>密钥byte[]</returns>
            public static byte[] GenerateRandomKey(int sizeInBytes)
            {
                byte[] key = new byte[sizeInBytes];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(key);
                }
                return key;
            }
            /// <summary>
            /// 将密钥转成字符串(十六进制)
            /// </summary>
            /// <param name="key">密钥</param>
            /// <returns>密钥字符串</returns>
            public static string KeyToStringHex(byte[] key)
            {
                StringBuilder reData = new StringBuilder();
                foreach (byte b in key)
                {
                    reData.AppendFormat("{0:X2} ", b);
                }
                return reData.ToString().TrimEnd();
            }
            /// <summary>
            /// 将密钥转成字符串(十六进制)没有美化
            /// </summary>
            /// <param name="key">密钥</param>
            /// <returns>密钥字符串</returns>
            public static string KeyToStringHexNoFormat(byte[] key)
            {
                StringBuilder reData = new StringBuilder();
                foreach (byte b in key)
                {
                    reData.AppendFormat("{0:X2}", b);
                }
                return reData.ToString().TrimEnd();
            }
            /// <summary>
            /// 将密钥转成字符串(Base64)
            /// </summary>
            /// <param name="key">密钥</param>
            /// <returns>密钥字符串</returns>
            public static string KeyToStringBase64(byte[] key)
            {
                return Convert.ToBase64String(key);
            }
            /// <summary>
            /// 将字符串转换回密钥(Base64)
            /// </summary>
            /// <param name="keyAsString">密钥字符串</param>
            /// <returns>密钥</returns>
            public static byte[] StringToKeyBase64(string keyAsString)
            {
                return Convert.FromBase64String(keyAsString);
            }
            /// <summary>
            /// 将字符串转换回密钥(十六进制)
            /// </summary>
            /// <param name="keyAsString">密钥字符串</param>
            /// <returns>密钥</returns>
            public static byte[] StringToKeyHex(string keyAsString)
            {
                bool hasSpaces = keyAsString.Contains(" ");

                if (hasSpaces)
                {
                    keyAsString = keyAsString.Replace(" ", "");
                }

                int keySizeInBytes = keyAsString.Length / 2;
                byte[] key = new byte[keySizeInBytes];

                for (int i = 0; i < keySizeInBytes; i++)
                {
                    string byteString = keyAsString.Substring(i * 2, 2);
                    key[i] = byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber);
                }

                return key;
            }


        }
        /// <summary>
        /// 代码相关
        /// </summary>
        public static class Code
        {
            /// <summary>
            /// 将OBJ转换成指定类型
            /// </summary>
            /// <param name="Data">OBJ内容</param>
            /// <param name="Type">指定类型</param>
            /// <returns>转换成指定类型后的内容</returns>
            public static dynamic ConvertObj(object Data, Type Type)
            {
                return Convert.ChangeType(Data, Type);
            }
        }
        /// <summary>
        /// SQL相关
        /// </summary>
        public static class SQL
        {

            /// <summary>
            /// SQLit操作类
            /// </summary>
            public class SQLite
            {

                /// <summary>
                /// 连接信息
                /// </summary>
                public ConnData _ConnData { get; }
                /// <summary>
                /// 连接字符串
                /// </summary>
                public string _ConnStr { get; }
                /// <summary>
                /// 连接
                /// </summary>
                public SQLiteConnection _Conn { get; }
                /// <summary>
                /// 初始化
                /// </summary>
                /// <param name="ConnData">连接信息</param>
                /// <exception cref="InvalidOperationException"></exception>
                public SQLite(ConnData ConnData)
                {
                    Logger.WriteDebug($"初始化数据库 位置:{ConnData.dbPath}");
                    _ConnData = ConnData;
                    _ConnStr = ConnData.dbPath;
                    if (NewDbFile(_ConnData.dbPath) == null)
                    {
                        Logger.WriteError($"初始化(创建)数据库失败!");
                        throw new InvalidOperationException("初始化(创建)数据库失败!");
                    }
                    _Conn = new SQLiteConnection("data source=" + _ConnData.dbPath);
                    Logger.WriteInfor($"初始化数据库完成 位置:{ConnData.dbPath}");
                    ConneTest();
                }
                /// <summary>
                /// 新建数据库文件
                /// </summary>
                /// <param name="dbPath">数据库文件路径及名称</param>
                /// <returns>新建成功，返回db路径，否则返回null</returns>
                static public string? NewDbFile(string dbPath)
                {
                    Logger.WriteDebug("执行创建数据库 位置:" + System.IO.Path.GetFullPath(dbPath));
                    if (!System.IO.File.Exists(dbPath))
                    {
                        Logger.WriteInfor("未在:" + System.IO.Path.GetFullPath(dbPath) + " 找到数据库即将执行创建");
                        try
                        {
                            SQLiteConnection.CreateFile(dbPath);
                            Logger.WriteInfor("创建成功!位置:" + System.IO.Path.GetFullPath(dbPath));
                            return dbPath;
                        }
                        catch (Exception Message)
                        {
                            Logger.WriteError("错误!" + Message);
                            return null;
                        }
                    }
                    Logger.WriteInfor("数据库存在!位置:" + System.IO.Path.GetFullPath(dbPath));
                    return dbPath;
                }
                /// <summary>
                /// 数据库连接测试
                /// </summary>
                /// <returns>初始化成功OR失败</returns>
                public bool ConneTest()
                {
                    try
                    {
                        _Conn.Open();
                    }
                    catch
                    {
                        Logger.WriteError("数据库连接失败!");
                        return false;
                    }
                    finally
                    {
                        _Conn.Cancel();
                        _Conn.Close();
                    }
                    Logger.WriteInfor("数据库连接成功!");
                    return true;
                }

                /// <summary>
                /// Sql命令执行
                /// </summary>
                /// <param name="Sql">Sql命令</param>
                /// <returns>执行结果</returns>
                public SQLiteCommand? Command(string Sql)
                {
                    SQLiteCommand Cmd;
                    try
                    {
                        Cmd = new(Sql, _Conn);
                        Logger.WriteDebug($"执行命令成功! 影响行数:{Cmd.ExecuteNonQuery()} 命令:{Sql}");
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"执行命令失败! 因为: {ex.Message} 命令:{Sql}");
                        return null;
                    }
                    return Cmd;
                }
                /// <summary>
                /// Sql命令执行(单次)
                /// </summary>
                /// <param name="Sql">Sql命令</param>
                /// <returns>执行结果</returns>
                public SQLiteCommand? CommandSingle(string Sql)
                {
                    SQLiteCommand Cmd;
                    try
                    {
                        Open();
                        Cmd = new(Sql, _Conn);
                        Logger.WriteDebug($"执行命令成功! 影响行数:{Cmd.ExecuteNonQuery()} 命令:{Sql}");
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"执行命令失败! 因为: {ex.Message} 命令:{Sql}");
                        return null;
                    }
                    finally
                    {
                        Close();
                    }
                    return Cmd;
                }

                /// <summary>
                /// 备份数据库到指定路径。
                /// </summary>
                /// <param name="backupPath">备份文件的路径。</param>
                public void BackupDatabase(string backupPath)
                {
                    Logger.WriteInfor($"备份数据库至:{backupPath}");
                    try
                    {
                        if (System.IO.File.Exists(_ConnStr))
                        {
                            System.IO.File.Copy(_ConnStr, backupPath, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"备份数据库时出错!因为:{ex.Message}");
                        throw;
                    }
                }

                /// <summary>
                /// 还原数据库从指定的备份文件。
                /// </summary>
                /// <param name="backupPath">备份文件的路径。</param>
                public void RestoreDatabase(string backupPath)
                {
                    Logger.WriteInfor($"还原数据库:{backupPath}");
                    try
                    {
                        if (System.IO.File.Exists(backupPath))
                        {
                            System.IO.File.Copy(backupPath, _ConnStr, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"还原数据库时出错!因为:{ex.Message}");
                        throw;
                    }
                }

                /// <summary>
                /// 向指定表中插入数据。
                /// </summary>
                /// <param name="tableName">要插入数据的表名。</param>
                /// <param name="columns">要插入的列名数组。</param>
                /// <param name="values">对应的值数组。</param>
                /// <exception cref="ArgumentException">当列数与值数不匹配时引发。</exception>
                public void InsertData(string tableName, string[] columns, object[] values)
                {
                    Logger.WriteDebug($"插入表'{tableName}' 插入项目{string.Join(",", columns)}");
                    try
                    {
                        if (columns.Length != values.Length)
                        {
                            throw new ArgumentException("列数与值数必须相等。");
                        }

                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            command.CommandText = GenerateInsertQuery(tableName, columns);

                            for (int i = 0; i < columns.Length; i++)
                            {
                                command.Parameters.AddWithValue($"@{columns[i]}", values[i]);
                            }

                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"插入数据时出错!因为:{ex.Message}");
                        throw; // 将异常继续抛出
                    }
                }

                /// <summary>
                /// 执行批量插入操作，向指定表中插入多行数据。
                /// </summary>
                /// <param name="tableName">要插入数据的表名。</param>
                /// <param name="columns">要插入的列名数组。</param>
                /// <param name="batchValues">包含多个数据行的批量值列表。</param>
                /// <exception cref="ArgumentException">当列数和批量值为空，或批量值的列数与指定列数不匹配时引发。</exception>
                public void BulkInsertData(string tableName, string[] columns, List<object[]> batchValues)
                {
                    Logger.WriteDebug($"批量插入表'{tableName}' 插入项目{string.Join(",", columns)} 插入数量{batchValues.Count}");
                    try
                    {
                        if (columns.Length == 0 || batchValues.Count == 0)
                        {
                            throw new ArgumentException("列数和批量值不能为空。");
                        }

                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            string insertQuery = GenerateInsertQuery(tableName, columns);

                            List<int> mismatchedRows = new List<int>();

                            for (int i = 0; i < batchValues.Count; i++)
                            {
                                object[] values = batchValues[i];

                                if (values.Length != columns.Length)
                                {
                                    mismatchedRows.Add(i + 1);
                                }
                                else
                                {
                                    string parameterNames = string.Join(", ", columns.Select((_, index) => $"@{columns[index]}_{i}"));

                                    string valuesQuery = string.Join(", ", parameterNames.Split(',').Select((_, index) => $"@{columns[index]}_{i}"));

                                    string fullInsertQuery = $"{insertQuery} VALUES ({valuesQuery})";

                                    command.CommandText = fullInsertQuery;

                                    for (int j = 0; j < columns.Length; j++)
                                    {
                                        command.Parameters.AddWithValue($"@{columns[j]}_{i}", values[j]);
                                    }

                                    command.ExecuteNonQuery();
                                }
                            }

                            if (mismatchedRows.Count > 0)
                            {
                                string errorDetails = string.Join(", ", mismatchedRows.Select(row => $"第 {row} 行"));
                                throw new ArgumentException($"批量插入时出错，{errorDetails}的列数与值数不匹配。");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"批量插入数据时出错!因为:{ex.Message}");
                        throw; // 将异常继续抛出
                    }
                }

                /// <summary>
                /// 生成用于插入数据的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要插入数据的表名。</param>
                /// <param name="columns">要插入的列名数组。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateInsertQuery(string tableName, string[] columns)
                {
                    string columnNames = string.Join(", ", columns);
                    string parameterNames = string.Join(", ", columns.Select(c => $"@{c}"));
                    return $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";
                }

                /// <summary>
                /// 更新指定表中的数据。
                /// </summary>
                /// <param name="tableName">要更新的表名。</param>
                /// <param name="columns">要更新的列名数组。</param>
                /// <param name="values">对应的新值数组。</param>
                /// <param name="condition">更新数据的条件。</param>
                /// <exception cref="ArgumentException">当列数与值数不匹配时引发。</exception>
                public void UpdateData(string tableName, string[] columns, object[] values, string condition)
                {
                    Logger.WriteDebug($"更新表'{tableName}' 更新项目{string.Join(",", columns)} 条件 {condition}");
                    try
                    {
                        if (columns.Length != values.Length)
                        {
                            throw new ArgumentException("列数与值数必须相等。");
                        }

                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            command.CommandText = GenerateUpdateQuery(tableName, columns, condition);

                            for (int i = 0; i < columns.Length; i++)
                            {
                                command.Parameters.AddWithValue($"@{columns[i]}", values[i]);
                            }

                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"更新数据时出错!因为:{ex.Message}");
                        throw;
                    }
                }
                /// <summary>
                /// 更新指定表中的数据。
                /// </summary>
                /// <param name="tableName">要更新的表名。</param>
                /// <param name="data">包含要更新的列名及对应的新值的字典。</param>
                /// <param name="condition">更新数据的条件。</param>
                /// <exception cref="ArgumentException">当列数与值数不匹配时引发。</exception>
                public void UpdateData(string tableName, Dictionary<string, object> data, string condition)
                {
                    Logger.WriteDebug($"更新表'{tableName}' 更新项目{string.Join(",", data.Keys)} 条件 {condition}");
                    try
                    {
                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            List<string> updateExpressions = new List<string>();
                            foreach (var kvp in data)
                            {
                                command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
                                updateExpressions.Add($"{kvp.Key} = @{kvp.Key}");
                            }

                            command.CommandText = GenerateUpdateQuery(tableName, updateExpressions, condition);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"更新数据时出错!因为:{ex.Message}");
                        throw;
                    }
                }

                /// <summary>
                /// 生成用于更新数据的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要更新的表名。</param>
                /// <param name="columns">要更新的列名数组。</param>
                /// <param name="condition">更新数据的条件。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateUpdateQuery(string tableName, string[] columns, string condition)
                {
                    string setClause = string.Join(", ", columns.Select(c => c + " = @" + c));
                    return $"UPDATE {tableName} SET {setClause} WHERE {condition}";
                }
                /// <summary>
                /// 生成用于更新数据的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要更新的表名。</param>
                /// <param name="updateExpressions">数据。</param>
                /// <param name="condition">更新数据的条件。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateUpdateQuery(string tableName, List<string> updateExpressions, string condition)
                {
                    string setClause = string.Join(", ", updateExpressions);
                    return $"UPDATE {tableName} SET {setClause} WHERE {condition}";
                }

                /// <summary>
                /// 查询指定表中满足条件的数据。
                /// </summary>
                /// <param name="tableName">要查询的表名。</param>
                /// <param name="columns">要查询的列名数组。为 null 或空数组时表示查询所有列。</param>
                /// <param name="condition">查询条件。</param>
                /// <param name="orderBy">排序条件。例如： "ColumnName ASC"。</param>
                /// <returns>满足条件的数据表。</returns>
                public DataTable QueryData(string tableName, string[]? columns, string condition, string? orderBy = null)
                {
                    Logger.WriteDebug($"查询表'{tableName}' 查询项目{string.Join(",", columns ?? new string[] { "*" })} 条件 {condition} 排序 {orderBy}");
                    try
                    {
                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            command.CommandText = GenerateQueryQuery(tableName, columns, condition, orderBy);

                            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);
                                return dataTable;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"查询数据时出错!因为:{ex.Message}");
                        throw;
                    }
                }

                /// <summary>
                /// 生成用于查询数据的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要查询的表名。</param>
                /// <param name="columns">要查询的列名数组。</param>
                /// <param name="condition">查询条件。</param>
                /// <param name="orderBy">排序条件。例如： "ColumnName ASC"。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateQueryQuery(string tableName, string[]? columns, string condition, string? orderBy = null)
                {
                    string columnNames;

                    if (columns == null || columns.Length == 0)
                    {
                        columnNames = "*"; // 查询所有列
                    }
                    else
                    {
                        columnNames = string.Join(", ", columns);
                    }
                    if (orderBy != null)
                        return $"SELECT {columnNames} FROM {tableName} WHERE {condition} ORDER BY {orderBy}";
                    else
                        return $"SELECT {columnNames} FROM {tableName} WHERE {condition}";
                }

                /// <summary>
                /// 分页查询指定表中的数据。
                /// </summary>
                /// <param name="tableName">要查询的表名。</param>
                /// <param name="columns">要查询的列名数组。</param>
                /// <param name="condition">查询条件。</param>
                /// <param name="orderBy">排序条件。例如： "ColumnName ASC"。</param>
                /// <param name="pageNumber">页码（从1开始）。</param>
                /// <param name="pageSize">每页的条目数。</param>
                /// <returns>指定页的数据表。</returns>
                public DataTable QueryDataWithPagination(string tableName, string[]? columns, string condition, int pageNumber, int pageSize, string? orderBy = null)
                {
                    Logger.WriteDebug($"分页查询表'{tableName}' 查询项目{string.Join(",", columns ?? new string[] { "*" })} 条件 {condition} 排序 {orderBy} 第 {pageNumber} 页 每页 {pageSize} 条");

                    try
                    {
                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            command.CommandText = GenerateQueryWithPaginationQuery(tableName, columns, condition, pageNumber, pageSize, orderBy);

                            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);
                                return dataTable;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"分页查询数据时出错!因为:{ex.Message}");
                        throw;
                    }
                }

                /// <summary>
                /// 生成用于分页查询数据的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要查询的表名。</param>
                /// <param name="columns">要查询的列名数组。</param>
                /// <param name="condition">查询条件。</param>
                /// <param name="pageNumber">页码（从1开始）。</param>
                /// <param name="pageSize">每页的条目数。</param>
                /// <param name="orderBy">排序条件。例如： "ColumnName ASC"。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateQueryWithPaginationQuery(string tableName, string[]? columns, string condition, int pageNumber, int pageSize, string? orderBy = null)
                {
                    string columnNames;
                    if (columns == null || columns.Length == 0)
                    {
                        columnNames = "*"; // 查询所有列
                    }
                    else
                    {
                        columnNames = string.Join(", ", columns);
                    }
                    int offset = (pageNumber - 1) * pageSize;
                    if (orderBy != null)
                        return $"SELECT {columnNames} FROM {tableName} WHERE {condition} ORDER BY {orderBy} LIMIT {pageSize} OFFSET {offset}";
                    else
                        return $"SELECT {columnNames} FROM {tableName} WHERE {condition} LIMIT {pageSize} OFFSET {offset}";
                }

                /// <summary>
                /// 获取指定查询条件下的数据总行数。
                /// </summary>
                /// <param name="tableName">要查询的表名。</param>
                /// <param name="condition">查询条件。</param>
                /// <returns>数据总行数。</returns>
                public int GetTotalRowCount(string tableName, string condition)
                {
                    Logger.WriteDebug($"询表项目数量'{tableName}' 条件{condition}");
                    try
                    {
                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            command.CommandText = GenerateTotalRowCountQuery(tableName, condition);
                            int rowCount = Convert.ToInt32(command.ExecuteScalar());
                            return rowCount;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"获取数据总行数时出错!因为:{ex.Message}");
                        throw;
                    }
                }

                /// <summary>
                /// 生成用于查询数据总行数的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要查询的表名。</param>
                /// <param name="condition">查询条件。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateTotalRowCountQuery(string tableName, string condition)
                {
                    return $"SELECT COUNT(*) FROM {tableName} WHERE {condition}";
                }

                /// <summary>
                /// 删除指定表中满足条件的数据。
                /// </summary>
                /// <param name="tableName">要删除数据的表名。</param>
                /// <param name="condition">删除数据的条件。</param>
                public void DeleteData(string tableName, string condition)
                {
                    Logger.WriteDebug($"删除表'{tableName}' 条件 {condition}");
                    try
                    {
                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            command.CommandText = GenerateDeleteQuery(tableName, condition);

                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"删除数据时出错!因为:{ex.Message}");
                        throw;
                    }
                }

                /// <summary>
                /// 生成用于删除数据的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要删除数据的表名。</param>
                /// <param name="condition">删除数据的条件。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateDeleteQuery(string tableName, string condition)
                {
                    return $"DELETE FROM {tableName} WHERE {condition}";
                }

                /// <summary>
                /// 创建表。
                /// </summary>
                /// <param name="tableName">要创建的表名。</param>
                /// <param name="columns">要创建的列定义数组。</param>
                public void CreateTable(string tableName, string[] columns)
                {
                    Logger.WriteDebug($"创建表'{tableName}' 创建项目{string.Join(",", columns)}");
                    try
                    {
                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            command.CommandText = GenerateCreateTableQuery(tableName, columns);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError("创建表时出错：" + ex.Message);
                        throw; // 将异常继续抛出
                    }
                }

                /// <summary>
                /// 生成用于创建表的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要创建的表名。</param>
                /// <param name="columns">要创建的列定义数组。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateCreateTableQuery(string tableName, string[] columns)
                {
                    string columnDefinitions = string.Join(", ", columns);
                    return $"CREATE TABLE {tableName} ({columnDefinitions})";
                }

                /// <summary>
                /// 删除表。
                /// </summary>
                /// <param name="tableName">要删除的表名。</param>
                public void DropTable(string tableName)
                {
                    Logger.WriteDebug($"删除表'{tableName}'");
                    try
                    {
                        using (SQLiteCommand command = _Conn.CreateCommand())
                        {
                            command.CommandText = GenerateDropTableQuery(tableName);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError("删除表时出错：" + ex.Message);
                        throw; // 将异常继续抛出
                    }
                }

                /// <summary>
                /// 生成用于删除表的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要删除的表名。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateDropTableQuery(string tableName)
                {
                    return $"DROP TABLE IF EXISTS {tableName}";
                }

                /// <summary>
                /// 打开连接
                /// </summary>
                public bool Open()
                {
                    try
                    {
                        _Conn.Open();
                    }
                    catch (Exception ex)//报错处理
                    {
                        //Console.WriteLine(ex.Message);
                        Logger.WriteError(ex.Message);
                        return false;
                    }
                    return true;
                }
                /// <summary>
                /// 关闭连接
                /// </summary>
                public void Close()
                {
                    _Conn.Close();
                }
                /// <summary>
                /// 获取状态
                /// </summary>
                /// <returns>状态</returns>
                public ConnectionState Status()
                {
                    return _Conn.State;
                }

                /// <summary>
                /// 连接信息
                /// </summary>
                public class ConnData
                {
                    /// <summary>
                    /// 数据库路径
                    /// </summary>
                    public string dbPath { get; set; } = "db.db";
                }
            }
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="enumValue">枚举值</param>
        /// <returns>枚举叙述</returns>
        public static string GetDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = field?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            return attributes != null && attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
        }
    }
}
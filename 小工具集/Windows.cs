﻿using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using CSChaCha20;
using Downloader;
using LoongEgg.LoongLogger;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SharpCompress.Archives;
using SharpCompress.Common;
using Spectre.Console;
using xxHashSharp;
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
            [Obsolete("有新的方法可以用")]
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
            [Obsolete("有新的方法可以用")]
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
            [Obsolete("有新的方法可以用")]
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
            [Obsolete("有新的方法可以用")]
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
            [Obsolete("有新的方法可以用")]
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
            [Obsolete("有新的方法可以用")]
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
            [Obsolete("有新的方法可以用")]
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

            /// <summary>
            /// 进程助手
            /// </summary>
            class ProcessHelper
            {
                // 枚举用于指定启动方式
                public enum ProcessStartMode
                {
                    /// <summary>
                    /// 开启新窗口并运行
                    /// </summary>
                    ShowWindow,
                    /// <summary>
                    /// 不开启窗口但打印输出
                    /// </summary>
                    NoShowWindow,
                    /// <summary>
                    /// 不开启窗口且不打印输出
                    /// </summary>
                    NoShowNoOutput
                }
                /// <summary>
                /// 进程启动模式
                /// </summary>
                private ProcessStartMode startMode;
                /// <summary>
                /// 输出接收
                /// </summary>
                public event Action<string?>? OutputReceived;
                /// <summary>
                /// 错误输出接收
                /// </summary>
                public event Action<string?>? OutputError;
                /// <summary>
                /// 输入流
                /// </summary>
                private event Action<string?>? InputRequired;
                /// <summary>
                /// 构造函数
                /// </summary>
                /// <param name="startMode">启动模式</param>
                public ProcessHelper(ProcessStartMode startMode = ProcessStartMode.NoShowNoOutput)
                {
                    this.startMode = startMode;
                }

                public async Task<string> StartAppAsyncNoShowRData(string Path, string? Args = null)
                {
                    return await Task.Run(async () =>
                    {
                        try
                        {
                            // 创建一个新的进程实例
                            Process process = new Process();

                            // 设置要运行的外部程序的路径和参数
                            process.StartInfo.FileName = Path;
                            process.StartInfo.Arguments = Args;

                            // 根据启动方式设置窗口和输出
                            switch (startMode)
                            {
                                case ProcessStartMode.ShowWindow:
                                    //process.StartInfo.CreateNoWindow = true;
                                    //process.StartInfo.CreateNoWindow = true;
                                    //process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                                    //process.StartInfo.RedirectStandardOutput = true;
                                    //process.StartInfo.RedirectStandardError = true;
                                    //process.StartInfo.UseShellExecute = true;
                                    break;
                                case ProcessStartMode.NoShowWindow:
                                    //process.StartInfo.CreateNoWindow = true;
                                    //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    //process.StartInfo.RedirectStandardOutput = true;
                                    //process.StartInfo.RedirectStandardError = true;
                                    break;
                                case ProcessStartMode.NoShowNoOutput:
                                    process.StartInfo.RedirectStandardInput = true; // 启用标准输入流
                                    process.StartInfo.CreateNoWindow = true;
                                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    process.StartInfo.RedirectStandardOutput = true;
                                    process.StartInfo.RedirectStandardError = true;
                                    break;
                            }


                            // 创建一个StringBuilder来保存输出
                            StringBuilder outputBuilder = new StringBuilder();

                            // 定义一个异步方法来读取标准输出流
                            async Task ReadOutputAsync(StreamReader reader)
                            {
                                while (!reader.EndOfStream)
                                {
                                    string? line = await reader.ReadLineAsync();
                                    outputBuilder.AppendLine(line);

                                    // 触发输出事件
                                    OutputReceived?.Invoke(line);
                                }
                            }

                            // 定义一个异步方法来读取标准错误输出流
                            async Task ReadOutputErrorAsync(StreamReader reader)
                            {
                                while (!reader.EndOfStream)
                                {
                                    string? line = await reader.ReadLineAsync();
                                    outputBuilder.AppendLine(line);

                                    // 触发输出事件
                                    OutputError?.Invoke(line);
                                }
                            }

                            //定义发送数据
                            InputRequired += (e) =>
                            {
                                // 获取标准输入流
                                StreamWriter standardInput = process.StandardInput;
                                // 例如，向CMD发送命令
                                standardInput.WriteLine(e);
                                standardInput.Flush(); // 刷新输入流
                            };

                            // 启动外部程序
                            process.Start();

                            // 异步读取标准输出和标准错误
                            Task outputTask = ReadOutputAsync(process.StandardOutput);
                            Task errorTask = ReadOutputErrorAsync(process.StandardError);

                            // 等待外部程序完成
                            await Task.WhenAll(outputTask, errorTask);
                            process.WaitForExit();

                            // 关闭进程
                            process.Close();

                            // 返回完整的输出
                            return outputBuilder.ToString();
                        }
                        catch (Exception e)
                        {
                            OutputError?.Invoke(e.Message);
                            return e.Message;
                        }
                    });
                }
                /// <summary>
                /// 发送命令
                /// </summary>
                /// <param name="Command">命令</param>
                public void Send(string Command)
                {
                    InputRequired!.Invoke(Command);
                }
            }
        }
        /// <summary>
        /// 网络相关
        /// </summary>
        public static class Network
        {
            /// <summary>
            /// 系统UG
            /// </summary>
            public static readonly string UserAgent = GetNativeUserAgent();

            /// <summary>
            /// 下载相关
            /// </summary>
            public class Download
            {
                /// <summary>
                /// 下载文件
                /// </summary>
                /// <param name="Url">地址</param>
                /// <param name="Path">保存路径</param>
                /// <param name="Name">保存文件名</param>
                /// <param name="progress">下载进度</param>
                /// <returns>文件路径</returns>
                [Obsolete("有新的方法可以用")]
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
                /// 下载配置
                /// </summary>
                private static DownloadConfiguration DownloadOpt = new()
                {
                    // 通常，主机支持的最大字节数为8000，默认值为8000
                    BufferBlockSize = 10240,  // (缓冲块大小，用于优化下载性能)

                    // 要下载的文件部分数，默认值为1
                    //ChunkCount = 8,  // (要下载的文件分块数)

                    // 限制下载速度为2MB/s，默认值为零或无限制
                    //MaximumBytesPerSecond = 1024 * 1024 * 2,  // (下载速度限制，以字节/秒为单位)

                    // 失败时的最大重试次数
                    MaxTryAgainOnFailover = 2,  // (失败时的最大重试次数)

                    // 是否并行下载文件的部分，默认值为false
                    ParallelDownload = true,  // (是否并行下载文件的不同部分)

                    // 并行下载的数量。默认值与分块数相同
                    ParallelCount = 8,  // (并行下载的数量)

                    // 当下载完成但失败时，清除包块数据， 默认值为false
                    ClearPackageOnCompletionWithFailure = true,  // (在下载失败时是否清除包块数据)

                    // 在开始下载之前，预留文件大小的存储空间，默认值为false
                    ReserveStorageSpaceBeforeStartingDownload = true,  // (在开始下载之前是否为文件预留存储空间)
                };
                /// <summary>
                /// 
                /// </summary>
                /// <param name="Url"></param>
                /// <param name="Name"></param>
                /// <param name="RequestConfiguration"></param>
                /// <returns></returns>
                public static async Task<DownloadFileData> DownloadFileProgressBar(string Url, string Name, RequestConfiguration? RequestConfiguration = null)
                {
                    if (RequestConfiguration != null)
                        DownloadOpt.RequestConfiguration = RequestConfiguration;

                    DownloadFileData downloadFileData = new();
                    var downloader = new DownloadService(DownloadOpt);
                    await AnsiConsole.Progress()
                        .Columns(new ProgressColumn[]
                        {
                            new TaskDescriptionColumn(),    // 任务描述
                            new ProgressBarColumn(),        // 进度栏
                            new PercentageColumn(),         // 百分比
                            new RemainingTimeColumn(),      // 余下的时间
                            new SpinnerColumn(),            // 旋转器
                        })
                        .StartAsync(async ctx =>
                        {
                            ProgressTask? task = null;
                            // 在每次下载开始时提供`FileName`和`TotalBytesToReceive`信息
                            downloader.DownloadStarted += (sender, e) =>
                            {
                                downloadFileData.Name = e.FileName;
                                downloadFileData.MaxLength = e.TotalBytesToReceive;
                                AnsiConsole.MarkupLine($"[yellow]开始下载文件[/] [dodgerblue2]{Name}[/] ({Text.ConvertByteUnits(e.TotalBytesToReceive)})");
                                //创建进度条
                                task = ctx.AddTask($"[green]{downloadFileData.Name}[/]");
                                task.MaxValue = downloadFileData.MaxLength;
                            };
                            // (下载开始事件：当下载开始时触发此事件，提供下载文件名和总字节数)
                            // 提供有关分块下载的任何信息，如每个分块的进度百分比、速度、总接收字节数和接收字节数组以进行实时流传输
                            /*downloader.ChunkDownloadProgressChanged += (sender, e) =>
                            {
                                AnsiConsole.WriteLine($"{e.ActiveChunks}---{Text.ConvertByteUnits(e.AverageBytesPerSecondSpeed)}");
                            };*/
                            // (分块下载进度更改事件：提供分块下载的进度信息，包括每个分块的进度百分比、速度等)
                            // 提供有关下载进度的任何信息，如所有分块进度的总进度百分比、总速度、平均速度、总接收字节数和接收字节数组以进行实时流传输
                            downloader.DownloadProgressChanged += (sender, e) =>
                            {
                                task!.Value = e.ReceivedBytesSize;
                                task.Description = $"[green]{downloadFileData.Name}[/] [yellow]{Text.ConvertByteUnits(e.BytesPerSecondSpeed)}[/]";
                                //Console.WriteLine(e.ProgressPercentage);
                            };
                            // (下载进度更改事件：提供下载的总体进度信息，包括总进度百分比、速度等)
                            // 下载完成事件，可以包括发生的错误、取消或成功完成的下载
                            downloader.DownloadFileCompleted += (sender, e) =>
                            {
                                if (e.Error != null)
                                {
                                    downloadFileData.State = false;
                                    task!.Description = $"[red]{Name} 下载失败![/]";
                                }
                                else
                                {
                                    downloadFileData.State = true;
                                    task!.Description = $"[green]{Name} 下载成功![/]";
                                }
                            };

                            // (下载文件完成事件：在下载完成时触发此事件，可能包括错误信息、取消或成功完成的下载)
                            await downloader.DownloadFileTaskAsync(Url, Name);
                        });
                    return downloadFileData;
                }

                /// <summary>
                /// 文件下载数据集
                /// </summary>
                public List<DownloadFileData?> DownloadFileDatas { get; } = new();
                /// <summary>
                /// 文件下载任务集
                /// </summary>
                private List<Task<bool>?>? DownloadFileTasks;
                /// <summary>
                /// 文件下载 基类
                /// </summary>
                /// <param name="Url">文件地址</param>
                /// <param name="Name">文件名称</param>
                /// <param name="RequestConfiguration">请求配置</param>
                /// <returns>任务成功与否</returns>
                public async Task<bool> DownloadFileBase(string Url, string Name, RequestConfiguration? RequestConfiguration = null)
                {
                    if (RequestConfiguration != null)
                        DownloadOpt.RequestConfiguration = RequestConfiguration;

                    DownloadFileData downloadFileData = new();
                    DownloadFileDatas.Add(downloadFileData);
                    var downloader = new DownloadService(DownloadOpt);
                    downloader.DownloadStarted += (sender, e) =>
                    {
                        downloadFileData.Name = e.FileName;
                        downloadFileData.MaxLength = e.TotalBytesToReceive;
                    };
                    downloader.DownloadProgressChanged += (sender, e) =>
                    {
                        downloadFileData.CurrentLength = e.ReceivedBytesSize;
                    };
                    downloader.DownloadFileCompleted += (sender, e) =>
                    {
                        if (e.Error != null)
                        {
                            downloadFileData.State = false;
                            downloadFileData.Error = e.Error;
                        }
                        else
                            downloadFileData.State = true;
                    };
                    await downloader.DownloadFileTaskAsync(Url, Name);
                    return downloadFileData.State;
                }

                /// <summary>
                /// 文件下载 基类
                /// </summary>
                /// <param name="DownloadInfo">下载信息</param>
                /// <returns>任务成功与否</returns>
                public async Task<bool> DownloadFileBase(DownloadInfo DownloadInfo)
                {
                    return await DownloadFileBase(DownloadInfo.Url, DownloadInfo.Name, DownloadInfo.RequestConfiguration);
                }

                /// <summary>
                /// 文件下载 基类
                /// </summary>
                /// <param name="DownloadInfo">下载信息</param>
                /// <param name="Task">进步条信息</param>
                /// <returns>任务成功与否</returns>
                private async Task<bool> DownloadFileLineBase(DownloadInfo DownloadInfo, ProgressTask Task)
                {
                    if (DownloadInfo.RequestConfiguration != null)
                        DownloadOpt.RequestConfiguration = DownloadInfo.RequestConfiguration;

                    DownloadFileData downloadFileData = new();
                    DownloadFileDatas.Add(downloadFileData);
                    var downloader = new DownloadService(DownloadOpt);
                    downloader.DownloadStarted += (sender, e) =>
                    {
                        downloadFileData.Name = e.FileName;
                        downloadFileData.MaxLength = e.TotalBytesToReceive;
                        Task.MaxValue = e.TotalBytesToReceive;
                        AnsiConsole.MarkupLine($"[yellow]开始下载文件[/] [dodgerblue2]{downloadFileData.Name}[/] ({Text.ConvertByteUnits(downloadFileData.MaxLength)})");
                    };
                    downloader.DownloadProgressChanged += (sender, e) =>
                    {
                        downloadFileData.CurrentLength = e.ReceivedBytesSize;
                        Task.Value = e.ReceivedBytesSize;
                        Task.Description = $"[green]{downloadFileData.Name}[/] [yellow]{Text.ConvertByteUnits(e.BytesPerSecondSpeed)}[/]";
                    };
                    downloader.DownloadFileCompleted += (sender, e) =>
                    {
                        if (e.Error != null)
                        {
                            downloadFileData.State = false;
                            downloadFileData.Error = e.Error;
                            Task!.Description = $"[red]{downloadFileData.Name} 下载失败![/]";
                        }
                        else
                        {
                            downloadFileData.State = true;
                            Task!.Description = $"[green]{downloadFileData.Name} 下载成功![/]";
                        }
                    };
                    await downloader.DownloadFileTaskAsync(DownloadInfo.Url, DownloadInfo.Name);
                    return downloadFileData.State;
                }

                /// <summary>
                /// 下载列队
                /// </summary>
                /// <param name="DownloadInfos">下载信息列表</param>
                /// <returns>任务</returns>
                public async Task DownloadFileLine(List<DownloadInfo> DownloadInfos)
                {
                    await AnsiConsole.Progress()
                        .Columns(new ProgressColumn[]
                        {
                            new TaskDescriptionColumn(),    // 任务描述
                            new ProgressBarColumn(),        // 进度栏
                            new PercentageColumn(),         // 百分比
                            new RemainingTimeColumn(),      // 余下的时间
                            new SpinnerColumn(),            // 旋转器
                        })
                        .StartAsync(async ctx =>
                        {
                            //遍历添加任务
                            DownloadFileTasks = new();
                            for (int i = 0; i < DownloadInfos.Count; i++)
                            {
                                var Task = ctx.AddTask($"[green]{DownloadInfos[i].Name}[/]");
                                DownloadFileTasks.Add(DownloadFileLineBase(DownloadInfos[i], Task));
                            }
                            //等待所有下载任务结束
                            Task.WaitAll(DownloadFileTasks.ToArray()!);
                        });
                }
                /// <summary>
                /// 下载信息
                /// </summary>
                public class DownloadInfo
                {
                    /// <summary>
                    /// 文件地址
                    /// </summary>
                    public required string Url { get; set; }
                    /// <summary>
                    /// 文件名称
                    /// </summary>
                    public required string Name { get; set; }
                    /// <summary>
                    /// 请求配置
                    /// </summary>
                    public RequestConfiguration? RequestConfiguration { get; set; } = null;
                }

                /// <summary>
                /// 下载数据内容
                /// </summary>
                public class DownloadFileData
                {
                    /// <summary>
                    /// 文件名
                    /// </summary>
                    public string? Name { get; set; }
                    /// <summary>
                    /// 文件大小
                    /// </summary>
                    public long MaxLength { get; set; }
                    /// <summary>
                    /// 目前大小
                    /// </summary>
                    public long CurrentLength { get; set; }
                    /// <summary>
                    /// 状态
                    /// </summary>
                    public bool State { get; set; }
                    /// <summary>
                    /// 错误
                    /// </summary>
                    public Exception? Error { get; set; }
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

            /// <summary>
            /// 获取本机User-Agent
            /// </summary>
            /// <returns>UG</returns>
            private static string GetNativeUserAgent()
            {
                // 获取操作系统信息
                string osInfo = Environment.OSVersion.ToString();

                // 获取当前应用程序的版本信息
                string? appVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

                // 获取当前 .NET Framework 版本
                string dotnetVersion = Environment.Version.ToString();

                // 获取当前进程的信息
                Process currentProcess = Process.GetCurrentProcess();

                // 从进程信息中获取应用程序名称
                string applicationName = currentProcess.ProcessName;

                // 构建 UserAgent 字符串
                string userAgent = $"{applicationName ??= "小工具集"}/{appVersion ??= "0"} ({osInfo}; .NET CLR {dotnetVersion})";

                return userAgent;
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
            public static string GetTextxxHash(string Data, uint Seed = 0)
            {
                byte[] data = Encoding.UTF8.GetBytes(Data);
                xxHash hash = new xxHash();
                hash.Init(Seed);
                hash.Update(data, data.Count());
                return hash.Digest().ToString("X");
            }
            /// <summary>
            /// 获取文件的xxHash
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <param name="Seed">种籽</param>
            /// <returns>值</returns>
            public static string GetFilexxHash(string Path, uint Seed = 0)
            {
                xxHash hash = new xxHash();
                // 初始化哈希值
                hash.Init(Seed);
                // 打开文件流
                using (FileStream fs = System.IO.File.OpenRead(Path))
                {
                    // 创建一个缓冲区，每次读取 4KB 的数据
                    byte[] buffer = new byte[102400];
                    int bytesRead;
                    // 循环读取文件流，直到结束
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // 更新哈希值
                        hash.Update(buffer, bytesRead);
                    }
                }
                // 获取最终的哈希值
                uint result = hash.Digest();
                // 输出十六进制格式的哈希值
                return result.ToString("X");
            }

            /// <summary>
            /// 获取文件的SHA256
            /// </summary>
            /// <param name="Path">文件路径</param>
            /// <returns></returns>
            public static string GetFilexxSHA256(string Path)
            {
                using (FileStream stream = System.IO.File.OpenRead(Path))
                {
                    SHA256 sha256 = SHA256.Create();
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    return hashHex;
                }
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

        }
        /// <summary>
        /// SQL相关
        /// </summary>
        public static class SQL
        {
            /// <summary>
            /// 数据库连接池。
            /// </summary>
            public class SQLiteDatabasePool
            {
                private readonly string _connectionString;
                private readonly ConcurrentQueue<SQLiteConnection> _connectionPool = new ConcurrentQueue<SQLiteConnection>();
                private readonly SemaphoreSlim _connectionSemaphore;
                private readonly Timer _connectionCleanupTimer;
                private readonly object _lock = new object();

                private int _currentPoolSize = 0;

                private int MaxPoolSize = 200;
                private int MinPoolSize = 10;
                private int ConnectionTimeout = 30; // 秒
                private int IdleTimeout = 2; // 秒

                /// <summary>
                /// 初始化一个新的 SQLiteDatabasePool 实例。
                /// </summary>
                /// <param name="connectionString">数据库连接字符串。</param>
                /// <param name="config">连接池配置。</param>
                public SQLiteDatabasePool(string connectionString, SQLiteDatabasePool_Config config)
                {
                    MaxPoolSize = config.MaxPoolSize;
                    MinPoolSize = config.MinPoolSize;
                    ConnectionTimeout = config.ConnectionTimeout;
                    IdleTimeout = config.IdleTimeout;

                    _connectionString = connectionString;
                    _connectionSemaphore = new SemaphoreSlim(MaxPoolSize, MaxPoolSize);

                    // 创建一个定时器，定期清理闲置连接
                    _connectionCleanupTimer = new Timer(CleanupIdleConnections!, null, IdleTimeout * 1000, IdleTimeout * 1000);

                    InitializePool();
                }

                // 初始化连接池，创建最小连接数的连接并添加到池中
                private void InitializePool()
                {
                    lock (_lock)
                    {
                        for (int i = 0; i < MinPoolSize; i++)
                        {
                            SQLiteConnection connection = CreateNewConnection();
                            _connectionPool.Enqueue(connection);
                            _currentPoolSize++;
                        }
                        Logger.WriteInfor("连接池初始化大小：" + _currentPoolSize);
                    }
                }

                /// <summary>
                /// 获取一个数据库连接（异步版本）
                /// </summary>
                /// <returns></returns>
                /// <exception cref="TimeoutException"></exception>
                public async Task<SQLiteConnection> GetConnectionAsync()
                {
                    if (await _connectionSemaphore.WaitAsync(ConnectionTimeout * 1000))
                    {
                        if (_connectionPool.TryDequeue(out SQLiteConnection? connection))
                        {
                            if (connection.State == ConnectionState.Closed)
                            {
                                await connection.OpenAsync();
                            }
                            return connection;
                        }
                        else
                        {
                            SQLiteConnection newConnection = CreateNewConnection();
                            _currentPoolSize++; // 增加连接池大小
                            return newConnection;
                        }
                    }

                    throw new TimeoutException("在超时时间内无法获取数据库连接。");
                }
                /// <summary>
                /// 获取一个数据库连接
                /// </summary>
                /// <returns></returns>
                /// <exception cref="TimeoutException"></exception>
                public SQLiteConnection GetConnection()
                {
                    if (_connectionSemaphore.Wait(ConnectionTimeout * 1000))
                    {
                        if (_connectionPool.TryDequeue(out SQLiteConnection? connection))
                        {
                            if (connection.State == ConnectionState.Closed)
                            {
                                connection.Open();
                            }
                            return connection;
                        }
                        else
                        {
                            SQLiteConnection newConnection = CreateNewConnection();
                            _currentPoolSize++; // 增加连接池大小
                            return newConnection;
                        }
                    }

                    throw new TimeoutException("在超时时间内无法获取数据库连接。");
                }

                /// <summary>
                /// 释放数据库连接回连接池
                /// </summary>
                /// <param name="connection">连接</param>
                public void ReleaseConnection(SQLiteConnection connection)
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        _connectionPool.Enqueue(connection);
                    }
                    _connectionSemaphore.Release();
                }

                // 创建新的数据库连接
                private SQLiteConnection CreateNewConnection()
                {
                    SQLiteConnection connection = new SQLiteConnection(_connectionString);
                    connection.Open();
                    return connection;
                }

                // 清理闲置连接，自动调整连接池大小（异步版本）
                private void CleanupIdleConnections(object state)
                {
                    lock (_lock)
                    {
                        Logger.WriteDebug($"当前连接池大小:{_currentPoolSize}");
                        int targetPoolSize = Math.Max(MinPoolSize, _currentPoolSize - 5); // 自动减小到最小连接数
                        int connectionsToClose = _currentPoolSize - targetPoolSize;

                        while (connectionsToClose > 0)
                        {
                            if (_connectionPool.TryDequeue(out SQLiteConnection? connection))
                            {
                                connection.Close();
                                _currentPoolSize--; // 减少连接池大小
                                connectionsToClose--;
                            }
                        }
                    }
                }

                /// <summary>
                /// 获取当前连接池大小
                /// </summary>
                /// <returns>当前连接池大小</returns>
                public int GetCurrentPoolSize()
                {
                    return _currentPoolSize;
                }
            }
            /// <summary>
            /// 连接池配置类。
            /// </summary>
            public class SQLiteDatabasePool_Config
            {
                /// <summary>
                /// 最大连接数。
                /// </summary>
                public int MaxPoolSize { get; set; } = 200;

                /// <summary>
                /// 最小连接数。
                /// </summary>
                public int MinPoolSize { get; set; } = 10;

                /// <summary>
                /// 连接超时时间（秒）。
                /// </summary>
                public int ConnectionTimeout { get; set; } = 30;

                /// <summary>
                /// 闲置连接超时时间（秒）。
                /// </summary>
                public int IdleTimeout { get; set; } = 2;
            }
            /// <summary>
            /// SQLit操作类
            /// </summary>
            public class SQLite
            {
                /// <summary>
                /// 连接池
                /// </summary>
                private readonly SQLiteDatabasePool _dbPool;
                /// <summary>
                /// 连接信息
                /// </summary>
                private ConnData _ConnData { get; }
                /// <summary>
                /// 连接字符串
                /// </summary>
                private string _ConnStr { get; }
                /// <summary>
                /// 初始化
                /// </summary>
                /// <param name="ConnData">连接信息</param>
                /// <param name="Config">连接池配置</param>
                /// <exception cref="InvalidOperationException"></exception>
                public SQLite(ConnData ConnData, SQLiteDatabasePool_Config Config)
                {
                    Logger.WriteDebug($"初始化数据库 位置:{ConnData.dbPath}");
                    _ConnData = ConnData;
                    _ConnStr = ConnData.dbPath;

                    if (NewDbFile(_ConnData.dbPath) == null)
                    {
                        Logger.WriteError($"初始化(创建)数据库失败!");
                        throw new InvalidOperationException("初始化(创建)数据库失败!");
                    }
                    _dbPool = new SQLiteDatabasePool($"data source={ConnData.dbPath}", Config);
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
                /// 备份数据库到指定路径。
                /// </summary>
                /// <param name="backupPath">备份文件的路径。</param>
                public void BackupDb(string backupPath)
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
                public void RestoreDb(string backupPath)
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
                /// 数据库连接测试
                /// </summary>
                /// <returns>初始化成功OR失败</returns>
                public bool ConneTest()
                {
                    SQLiteConnection connection;
                    try
                    {
                        connection = Open()!;
                        Close(connection);
                    }
                    catch
                    {
                        Logger.WriteError("数据库连接失败!");
                        return false;
                    }
                    Logger.WriteInfor("数据库连接成功!");
                    return true;
                }

                /// <summary>
                /// Sql命令执行
                /// </summary>
                /// <param name="Sql">Sql命令</param>
                /// <param name="connection">连接</param>
                /// <returns>执行结果</returns>
                [Obsolete("不好管理资源啊!")]
                public SQLiteCommand? Command(string Sql, SQLiteConnection? connection)
                {
                    SQLiteCommand Cmd;
                    try
                    {
                        Cmd = new(Sql, connection);
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
                [Obsolete("不好管理资源啊!")]
                public SQLiteDataReader? CommandSingle(string Sql)
                {
                    SQLiteDataReader? ReData = null;
                    SQLiteConnection? connection = null;
                    try
                    {
                        connection = Open();
                        SQLiteCommand Cmd = new(Sql, connection);
                        ReData = Cmd.ExecuteReader();
                        Logger.WriteDebug($"执行命令成功! 影响行数:{ReData.StepCount} 命令:{Sql}");
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"执行命令失败! 因为: {ex.Message} 命令:{Sql}");
                        return null;
                    }
                    finally
                    {
                        //ReData?.Close();
                        Close(connection);
                    }
                    return ReData;
                }
                /// <summary>
                /// 打开连接
                /// </summary>
                public SQLiteConnection? Open()
                {
                    SQLiteConnection connection;
                    try
                    {
                        connection = _dbPool.GetConnection();
                    }
                    catch (Exception ex)//报错处理
                    {
                        //Console.WriteLine(ex.Message);
                        Logger.WriteError(ex.Message);
                        return null;
                    }
                    return connection;
                }
                /// <summary>
                /// 关闭连接
                /// </summary>
                public void Close(SQLiteConnection? Input_Data)
                {
                    if (Input_Data != null)
                        _dbPool.ReleaseConnection(Input_Data);
                }
                /// <summary>
                /// 获取当前连接池大小
                /// </summary>
                /// <returns>连接池数量</returns>
                public int GetPoolSize()
                {
                    return _dbPool.GetCurrentPoolSize();
                }

                /// <summary>
                /// 向指定表中插入数据。
                /// </summary>
                /// <param name="tableName">要插入数据的表名。</param>
                /// <param name="columns">要插入的列名数组。</param>
                /// <param name="values">对应的值数组。</param>
                /// <exception cref="ArgumentException">当列数与值数不匹配时引发。</exception>
                public void InsertData(string tableName, string[] columns, object?[] values)
                {
                    Logger.WriteDebug($"插入表'{tableName}' 插入项目{string.Join(",", columns)}");
                    SQLiteConnection? connection = null;
                    try
                    {
                        if (columns.Length != values.Length)
                        {
                            throw new ArgumentException("列数与值数必须相等。");
                        }
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
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
                    finally
                    {
                        Close(connection);
                    }
                }
                /// <summary>
                /// 向指定表中插入数据。
                /// </summary>
                /// <param name="tableName">要插入数据的表名。</param>
                /// <param name="data">包含要插入的列名及其对应值的字典。</param>
                /// <exception cref="ArgumentException">当列数与值数不匹配时引发。</exception>
                public void InsertData(string tableName, Dictionary<string, object?>? data)
                {
                    if (data == null)
                        return;
                    Logger.WriteDebug($"插入表'{tableName}' 插入项目{string.Join(",", data.Keys)}");
                    SQLiteConnection? connection = null;
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
                        {
                            List<string> columns = new List<string>(data.Keys);
                            List<object?> values = new List<object?>(data.Values);

                            if (columns.Count != values.Count)
                            {
                                throw new ArgumentException("列数与值数必须相等。");
                            }

                            command.CommandText = GenerateInsertQuery(tableName, columns.ToArray());

                            for (int i = 0; i < columns.Count; i++)
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
                    finally
                    {
                        Close(connection);
                    }
                }
                /// <summary>
                /// 执行批量插入操作，向指定表中插入多行数据。
                /// </summary>
                /// <param name="tableName">要插入数据的表名。</param>
                /// <param name="columns">要插入的列名数组。</param>
                /// <param name="batchValues">包含多个数据行的批量值列表。</param>
                /// <exception cref="ArgumentException">当列数和批量值为空，或批量值的列数与指定列数不匹配时引发。</exception>
                public void BulkInsertData(string tableName, string[] columns, List<object?[]> batchValues)
                {
                    Logger.WriteDebug($"批量插入表'{tableName}' 插入项目{string.Join(",", columns)} 插入数量{batchValues.Count}");
                    SQLiteConnection? connection = null;
                    try
                    {
                        if (columns.Length == 0 || batchValues.Count == 0)
                        {
                            throw new ArgumentException("列数和批量值不能为空。");
                        }
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
                        {
                            string insertQuery = GenerateInsertQuery(tableName, columns);

                            for (int rowIndex = 0; rowIndex < batchValues.Count; rowIndex++)
                            {
                                object?[] values = batchValues[rowIndex];

                                if (values.Length != columns.Length)
                                {
                                    throw new ArgumentException($"批量插入时出错，第 {rowIndex + 1} 行的列数与值数不匹配。");
                                }

                                string parameterNames = string.Join(", ", columns.Select((col, index) => $"@{col}_{rowIndex}"));
                                string valuesQuery = string.Join(", ", parameterNames.Split(',').Select(_ => $"@{_.TrimStart('@')}"));

                                string fullInsertQuery = $"{insertQuery} VALUES ({valuesQuery})";

                                command.CommandText = fullInsertQuery;

                                for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                                {
                                    command.Parameters.AddWithValue($"@{columns[columnIndex]}_{rowIndex}", values[columnIndex]);
                                }

                                command.ExecuteNonQuery();
                                command.Parameters.Clear(); // Clear parameters for the next iteration
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"批量插入数据时出错!因为:{ex.Message}");
                        throw;
                    }
                    finally
                    {
                        Close(connection);
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
                public void UpdateData(string tableName, string[] columns, object?[] values, string condition)
                {
                    Logger.WriteDebug($"更新表'{tableName}' 更新项目{string.Join(",", columns)} 条件 {condition}");
                    SQLiteConnection? connection = null; // 获取连接
                    try
                    {
                        if (columns.Length != values.Length)
                        {
                            throw new ArgumentException("列数与值数必须相等。");
                        }
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
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
                    finally
                    {
                        Close(connection);
                    }
                }
                /// <summary>
                /// 更新指定表中的数据。
                /// </summary>
                /// <param name="tableName">要更新的表名。</param>
                /// <param name="data">包含要更新的列名及对应的新值的字典。</param>
                /// <param name="condition">更新数据的条件。</param>
                /// <exception cref="ArgumentException">当列数与值数不匹配时引发。</exception>
                public void UpdateData(string tableName, Dictionary<string, object?>? data, string condition)
                {
                    if (data == null)
                        return;
                    Logger.WriteDebug($"更新表'{tableName}' 更新项目{string.Join(",", data.Keys)} 条件 {condition}");
                    SQLiteConnection? connection = null; // 获取连接
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
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
                    finally
                    {
                        Close(connection);
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
                    SQLiteConnection? connection = null; // 获取连接
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
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
                    finally
                    {
                        Close(connection);
                    }
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
                    SQLiteConnection? connection = null; // 获取连接
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
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
                    finally
                    {
                        Close(connection);
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
                    SQLiteConnection? connection = null; // 获取连接
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
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
                    finally
                    {
                        Close(connection);
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
                    SQLiteConnection? connection = null; // 获取连接
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
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
                    finally
                    {
                        Close(connection);
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
                public bool CreateTable(string tableName, string[] columns)
                {
                    Logger.WriteDebug($"创建表'{tableName}' 创建项目{string.Join(",", columns)}");
                    SQLiteConnection? connection = null; // 获取连接
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
                        {
                            command.CommandText = GenerateCreateTableQuery(tableName, columns);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("table app already exists"))
                        {
                            Logger.WriteDebug($"{tableName} 表已存在 跳过创建");
                            return false;
                        }
                        Logger.WriteError("创建表时出错：" + ex.Message);
                        throw; // 将异常继续抛出
                    }
                    finally
                    {
                        Close(connection);
                    }
                    return true;
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
                    SQLiteConnection? connection = null; // 获取连接
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
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
                    finally
                    {
                        Close(connection);
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
                /// 判断指定名称的表是否存在。
                /// </summary>
                /// <param name="tableName">要判断的表名。</param>
                /// <returns>如果表存在，返回true；否则返回false。</returns>
                public bool TableExists(string tableName)
                {
                    Logger.WriteDebug($"判断表是否存在：'{tableName}'");
                    SQLiteConnection? connection = null;
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
                        {
                            command.CommandText = GenerateTableExistsQuery(tableName);
                            int tableCount = Convert.ToInt32(command.ExecuteScalar());
                            return tableCount > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"判断表是否存在时出错：{ex.Message}");
                        throw;
                    }
                    finally
                    {
                        Close(connection);
                    }
                }
                /// <summary>
                /// 生成用于判断指定表是否存在的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要判断的表名。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateTableExistsQuery(string tableName)
                {
                    return $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
                }

                /// <summary>
                /// 获取数据库中所有表的名称。
                /// </summary>
                /// <returns>包含所有表名的字符串数组。</returns>
                public string[] GetAllTableNames()
                {
                    Logger.WriteDebug("获取所有表名");
                    SQLiteConnection? connection = null;
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
                        {
                            command.CommandText = GenerateGetAllTableNamesQuery();
                            List<string> tableNames = new List<string>();
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    tableNames.Add(reader.GetString(0));
                                }
                            }
                            return tableNames.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"获取表名时出错：{ex.Message}");
                        throw;
                    }
                    finally
                    {
                        Close(connection);
                    }
                }
                /// <summary>
                /// 生成用于获取数据库中所有表的名称的SQLite查询语句。
                /// </summary>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateGetAllTableNamesQuery()
                {
                    return "SELECT name FROM sqlite_master WHERE type='table'";
                }
                /*
                /// <summary>
                /// 获取指定表的列信息。
                /// </summary>
                /// <param name="tableName">要获取列信息的表名。</param>
                /// <returns>包含列信息的List。</returns>
                public List<TableColumnInfo> GetTableColumns(string tableName)
                {
                    Logger.WriteDebug($"获取表'{tableName}'的列信息");
                    SQLiteConnection? connection = null;
                    try
                    {
                        connection = Open();
                        using (SQLiteCommand command = connection!.CreateCommand())
                        {
                            command.CommandText = GenerateGetTableColumnsQuery(tableName);
                            List<TableColumnInfo> columnInfoList = new List<TableColumnInfo>();
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    TableColumnInfo columnInfo = new TableColumnInfo
                                    {
                                        ColumnId = reader.GetInt32(0),
                                        ColumnName = reader.GetString(1),
                                        DataType = reader.GetString(2),
                                        IsPrimaryKey = reader.GetInt32(5) == 1,
                                        ColumnConstraints = reader.GetString(4) // 添加约束信息的获取
                                    };
                                    columnInfoList.Add(columnInfo);
                                }
                            }
                            return columnInfoList;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"获取表列信息时出错：{ex.Message}");
                        throw;
                    }
                    finally
                    {
                        Close(connection);
                    }
                }
                /// <summary>
                /// 生成用于获取指定表的列信息的SQLite查询语句。
                /// </summary>
                /// <param name="tableName">要获取列信息的表名。</param>
                /// <returns>生成的SQLite查询语句。</returns>
                private string GenerateGetTableColumnsQuery(string tableName)
                {
                    return $"PRAGMA table_info({tableName})";
                }
                /// <summary>
                /// 表列信息的数据结构。
                /// </summary>
                public class TableColumnInfo
                {
                    /// <summary>
                    /// 列的序号。
                    /// </summary>
                    public int ColumnId { get; set; }

                    /// <summary>
                    /// 列名。
                    /// </summary>
                    public string? ColumnName { get; set; }

                    /// <summary>
                    /// 数据类型。
                    /// </summary>
                    public string? DataType { get; set; }

                    /// <summary>
                    /// 是否为主键。
                    /// </summary>
                    public bool IsPrimaryKey { get; set; }

                    /// <summary>
                    /// 列的约束。
                    /// </summary>
                    public string? ColumnConstraints { get; set; }

                    // 添加其他列信息的属性，如默认值等
                }
                */

                /// <summary>
                /// 将 DataTable 中的数据填充到指定的数据类对象列表中。
                /// </summary>
                /// <typeparam name="T">要填充的数据类类型。</typeparam>
                /// <param name="dataTable">包含数据的 DataTable。</param>
                /// <returns>填充了数据的数据类对象列表。</returns>
                public static List<T> FillDataClassList<T>(DataTable dataTable) where T : new()
                {
                    List<T> dataList = new List<T>();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        T dataItem = new T();

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            PropertyInfo? property = typeof(T).GetProperty(column.ColumnName);
                            if (property != null)
                            {
                                object value = row[column];
                                if (value != DBNull.Value)
                                {
                                    property.SetValue(dataItem, value, null);
                                }
                            }
                        }

                        dataList.Add(dataItem);
                    }

                    return dataList;
                }

                /// <summary>
                /// 将 DataTable 中的数据填充到指定的数据类对象列表中（忽略大小写）。
                /// </summary>
                /// <typeparam name="T">要填充的数据类类型。</typeparam>
                /// <param name="dataTable">包含数据的 DataTable。</param>
                /// <returns>填充了数据的数据类对象列表。</returns>
                public static List<T> FillDataClassListIgnoreCase<T>(DataTable dataTable) where T : new()
                {
                    List<T> dataList = new List<T>();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        T dataItem = new T();

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            PropertyInfo? property = typeof(T).GetProperty(column.ColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (property != null)
                            {
                                object value = row[column];
                                if (value != DBNull.Value)
                                {
                                    property.SetValue(dataItem, value, null);
                                }
                            }
                        }

                        dataList.Add(dataItem);
                    }

                    return dataList;
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


            /// <summary>
            /// 数据库类型
            /// </summary>
            public static class SqlServerClass
            {
                /// <summary>
                /// Sqlite
                /// </summary>
                public class Sqlite
                {
                    /// <summary>
                    /// 数据库文件物理位置
                    /// </summary>
                    public string Server { get; set; } = "DB.db";

                    /// <summary>
                    /// 连接模式
                    /// </summary>
                    public SqliteOpenMode Mode { get; set; }

                    /// <summary>
                    /// 缓存方式
                    /// </summary>
                    public SqliteCacheMode Cache { get; set; }

                    /// <summary>
                    /// 获取连接字符串
                    /// </summary>
                    /// <returns>连接字符串</returns>
                    public string GetConnectionString()
                    {
                        return $"Data Source={Server};Mode={Mode};Cache={Cache};";
                    }

                    /// <summary>
                    /// 设置连接
                    /// </summary>
                    /// <param name="optionsBuilder">选项生成器</param>
                    public void SetOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
                    {
                        optionsBuilder.UseSqlite(GetConnectionString());
                    }
                }

                /// <summary>
                /// MySql
                /// </summary>
                public class MySql
                {
                    /// <summary>
                    /// 服务器地址
                    /// </summary>
                    public IPAddress Server { get; set; } = IPAddress.Parse("127.0.0.1");

                    /// <summary>
                    /// 端口号
                    /// </summary>
                    public int Port { get; set; } = 3306;

                    /// <summary>
                    /// 账号
                    /// </summary>
                    public string User { get; set; } = "root";

                    /// <summary>
                    /// 密码
                    /// </summary>
                    public string? Password { get; set; }

                    /// <summary>
                    /// 数据库名
                    /// </summary>
                    public string? Database { get; set; }

                    /// <summary>
                    /// SSL模式
                    /// </summary>
                    public string? SslMode { get; set; }

                    /// <summary>
                    /// 获取连接字符串
                    /// </summary>
                    /// <returns>连接字符串</returns>
                    public string GetConnectionString()
                    {
                        return $"Server={Server};Port={Port};User={User};Password={Password};Database={Database};SslMode={SslMode};";
                    }

                    /// <summary>
                    /// 设置连接
                    /// </summary>
                    /// <param name="optionsBuilder">选项生成器</param>
                    public void SetOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
                    {
                        optionsBuilder.UseMySQL(GetConnectionString());
                    }
                }

                /// <summary>
                /// SqlServer
                /// </summary>
                public class SqlServer
                {
                    /// <summary>
                    /// 服务器地址
                    /// </summary>
                    public IPAddress Server { get; set; } = IPAddress.Parse("127.0.0.1");

                    /// <summary>
                    /// 端口号
                    /// </summary>
                    public int Port { get; set; } = 3306;

                    /// <summary>
                    /// 账号
                    /// </summary>
                    public string User { get; set; } = "root";

                    /// <summary>
                    /// 密码
                    /// </summary>
                    public string? Password { get; set; }

                    /// <summary>
                    /// 数据库名
                    /// </summary>
                    public string? Database { get; set; }

                    /// <summary>
                    /// 获取连接字符串
                    /// </summary>
                    /// <returns>连接字符串</returns>
                    public string GetConnectionString()
                    {
                        return $"Server={Server};Port={Port};User={User};Password={Password};Database={Database};";
                    }

                    /// <summary>
                    /// 设置连接
                    /// </summary>
                    /// <param name="optionsBuilder">选项生成器</param>
                    public void SetOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
                    {
                        optionsBuilder.UseSqlServer(GetConnectionString());
                    }
                }
            }
        }

        /// <summary>
        /// 将OBJ转换成指定类型
        /// </summary>
        /// <param name="Data">OBJ内容</param>
        /// <param name="Type">指定类型</param>
        /// <returns>转换成指定类型后的内容</returns>
        public static dynamic ConvertObj(this object Data, Type Type)
        {
            return Convert.ChangeType(Data, Type);
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

        /// <summary>
        /// 将数据类对象列表转换为字典。
        /// </summary>
        /// <typeparam name="T">数据类类型。</typeparam>
        /// <param name="dataList">数据类对象列表。</param>
        /// <param name="NoNull">不要Null值。</param>
        /// <returns>字典，键为数据类属性，值为数据类对象列表中相应属性的值列表。</returns>
        public static Dictionary<string, List<object?>?> ConvertToDictionary<T>(this List<T> dataList, bool NoNull = false) where T : class
        {
            Dictionary<string, List<object?>?> dictionary = new();

            PropertyInfo[] properties = typeof(T).GetProperties();
            if (NoNull)
                properties = properties.Where(item => item != null).ToArray();
            foreach (PropertyInfo property in properties)
            {
                List<object?>? values = dataList.Select(item => property.GetValue(item)).ToList();
                if (values != null && NoNull || !NoNull)
                    dictionary[property.Name] = values;
            }

            return dictionary;
        }

        /// <summary>
        /// 将数据类对象转换为字典。
        /// </summary>
        /// <typeparam name="T">数据类类型。</typeparam>
        /// <param name="dataObject">数据类对象。</param>
        /// <param name="NoNull">不要Null值。</param>
        /// <returns>字典，键为数据类属性，值为数据类对象的属性值。</returns>
        public static Dictionary<string, object?>? ConvertToDictionary<T>(this T dataObject, bool NoNull = false) where T : class
        {
            Dictionary<string, object?>? dictionary = new();

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(dataObject);
                if (value != null && NoNull || !NoNull)
                    dictionary[property.Name] = value;
            }

            return dictionary;
        }

        /// <summary>
        /// 将任意数据转换成Json
        /// </summary>
        /// <typeparam name="T">源数据类型</typeparam>
        /// <param name="Data">转换目标</param>
        /// <param name="Option">转换配置</param>
        /// <returns>Json文本</returns>
        public static string? ToJsonString<T>(this T Data, JsonSerializerOptions? Option = null)
        {
            return System.Text.Json.JsonSerializer.Serialize(Data, Option);
        }

        /// <summary>
        /// 将任意数据转换成Json(异步)
        /// </summary>
        /// <typeparam name="T">源数据类型</typeparam>
        /// <param name="Data">转换目标</param>
        /// <param name="Option">转换配置</param>
        /// <returns>Json文本</returns>
        public static async Task<string> ToJsonStringAsync<T>(this T Data, JsonSerializerOptions? Option = null)
        {
            using var stream = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, Data, Data!.GetType(), Option).ConfigureAwait(false);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 将字符串保存到文本文件。
        /// </summary>
        /// <param name="String">要保存的字符串。</param>
        /// <param name="filePath">要保存到的文件路径。</param>
        /// <returns>如果成功保存则返回true，否则返回false。</returns>
        public static async Task<bool> SaveTextToFileAsync(this string String, string filePath)
        {
            try
            {
                // 使用异步操作写入JSON字符串到文件
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    byte[] Bytes = Encoding.UTF8.GetBytes(String);
                    await fs.WriteAsync(Bytes, 0, Bytes.Length);
                }

                Logger.WriteDebug($"字符串已成功保存到文件: {filePath}");
                return true;
            }
            catch (IOException e)
            {
                Logger.WriteError($"发生IO错误: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 异步从文件中逐行读取文本并返回整个文件内容作为字符串。
        /// </summary>
        /// <param name="filePath">要读取的文件路径。</param>
        /// <returns>文件内容的字符串。</returns>
        public static async Task<string?> ReadLargeFileAsync(string filePath)
        {
            StringBuilder fileContent = new StringBuilder();

            try
            {
                // 使用异步操作逐行读取大文件中的文本
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                using (StreamReader reader = new StreamReader(fs))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        // 将每一行文本追加到文件内容字符串中
                        fileContent.AppendLine(line);
                    }
                }

                return fileContent.ToString();
            }
            catch (IOException e)
            {
                Logger.WriteError($"发生IO错误: {e.Message}");
                return null; // 返回null或适当的错误处理
            }
        }

        /// <summary>
        /// Json字符串转回Class数据类
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="Data">Json字符串</param>
        /// <returns>目标数据类</returns>
        public static T? ToClassData<T>(this string? Data)
        {
            if (Data is null)
            {
                return default;
            }
            else
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(Data);
            }
        }

        /// <summary>
        /// Json字符串转回Class数据类(异步)
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="Data">Json字符串</param>
        /// <returns>目标数据类</returns>
        public static async Task<T?> ToClassDataAsync<T>(this string? Data)
        {
            if (Data is null)
            {
                return default;
            }
            else
            {
                using MemoryStream Steam = new MemoryStream((Encoding.UTF8.GetBytes(Data)));
                return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(Steam);
            }
        }

        /// <summary>
        /// 类转换
        /// </summary>
        /// <param name="source">原数据类</param>
        /// <param name="destination">目标数据类</param>
        public static void CopyPropertiesWithSameName(this object source, object destination)
        {
            Type sourceType = source.GetType();
            Type destinationType = destination.GetType();

            PropertyInfo[] sourceProperties = sourceType.GetProperties();
            PropertyInfo[] destinationProperties = destinationType.GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                foreach (var destinationProperty in destinationProperties)
                {
                    if (sourceProperty.Name == destinationProperty.Name &&
                        sourceProperty.PropertyType == destinationProperty.PropertyType &&
                        destinationProperty.CanWrite)
                    {
                        destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
                        break;
                    }
                }
            }
        }
    }
}
using System.Diagnostics;
using System.Net.NetworkInformation;
using SharpCompress.Archives;
using SharpCompress.Common;

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
                        Console.WriteLine(ex.Message);
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
                    Console.WriteLine($"下载进度：{percentage.ToString("0.0")}% - {GetSizeString(downloadedBytes)}/{GetSizeString(totalBytes)} - {GetSizeString((long?)speed)}/s");
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

        }
        /// <summary>
        /// 文本相关
        /// </summary>
        public static class Text
        {
            /// <summary>
            /// 自动转换单位
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
        }
    }
}
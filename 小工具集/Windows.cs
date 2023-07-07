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
            /// <returns>文件路径</returns>
            public static async Task<string?> DownloadFile(string Url, string? Path = null, string? Name = null)
            {
                Path ??= Windows.Path.ThisAppPath;
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        // 发送GET请求并获取响应
                        HttpResponseMessage response = await client.GetAsync(Url);

                        // 确保响应成功
                        response.EnsureSuccessStatusCode();

                        // 从响应中获取文件名
                        Name ??= response.Content.Headers.ContentDisposition?.FileName ?? System.IO.Path.GetFileName(new Uri(Url).LocalPath);

                        //过滤文件名
                        string invalidChars = new string(System.IO.Path.GetInvalidFileNameChars());
                        Name = string.Concat(Name.Split(invalidChars.ToCharArray()));

                        Name = System.IO.Path.Combine(Path, Name);
                        // 读取响应内容并保存到文件
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (FileStream fileStream = new FileStream(Name, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await contentStream.CopyToAsync(fileStream);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                    return Name;
                }
            }
        }
    }
}
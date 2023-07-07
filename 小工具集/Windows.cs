namespace 小工具集
{
    public class Windows
    {
        /// <summary>
        /// 从捷径中获取目标路径
        /// </summary>
        /// <param name="Path">捷径文件路径</param>
        /// <returns>目标文件路径</returns>
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
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return targetPath;
        }
    }
}
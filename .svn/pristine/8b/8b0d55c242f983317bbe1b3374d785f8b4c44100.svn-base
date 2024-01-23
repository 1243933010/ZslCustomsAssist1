using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public class FileHelper
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");

        public static void DeleteFolder(string path) => Directory.Delete(path, true);

        public static void DeleteAllFileByDirectoryPath(string path)
        {
            foreach (FileSystemInfo file in new DirectoryInfo(path).GetFiles())
                file.Delete();
        }

        public static void DeleteDirectory(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                return;
            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                directory.Delete(true);
            directoryInfo.Delete(true);
        }

        public static List<string> GetObjectByDirectoryPath(
            string path,
            string searchPattern = "*.*",
            bool isAllDirectories = false)
        {
            List<string> objectByDirectoryPath = new List<string>();
            if (!Directory.Exists(path))
                return objectByDirectoryPath;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] fileInfoArray = !isAllDirectories ? directoryInfo.GetFiles(searchPattern) : directoryInfo.GetFiles(searchPattern, SearchOption.AllDirectories);
            if (fileInfoArray == null || fileInfoArray.Length == 0)
                return objectByDirectoryPath;
            StreamReader streamReader = (StreamReader)null;
            string str = "";
            foreach (FileInfo fileInfo in fileInfoArray)
            {
                try
                {
                    streamReader = fileInfo.OpenText();
                    str = streamReader.ReadToEnd();
                }
                finally
                {
                    streamReader?.Close();
                }
                if (!(str == ""))
                    objectByDirectoryPath.Add(str);
            }
            return objectByDirectoryPath;
        }

        public static List<T> GetObjectByDirectoryPath<T>(
            string path,
            string searchPattern = "*.*",
            bool isAllDirectories = false)
        {
            FileHelper.logger.Debug((object)("从目录" + path + "中搜索" + searchPattern + "文件"));
            List<string> objectByDirectoryPath1 = FileHelper.GetObjectByDirectoryPath(path, searchPattern, isAllDirectories);
            List<T> objectByDirectoryPath2 = new List<T>();
            try
            {
                T obj1 = default(T);
                foreach (string str in objectByDirectoryPath1)
                {
                    T obj2 = JsonConvert.DeserializeObject<T>(str);
                    objectByDirectoryPath2.Add(obj2);
                }
            }
            catch (Exception ex)
            {
                FileHelper.logger.Debug((object)("从目录" + path + "中搜索" + searchPattern + "文件，执行异常" + ex.ToString()));
            }
            return objectByDirectoryPath2;
        }

        public static bool SaveAsFile(
            string directoryPath,
            string fileName,
            string fileContent,
            bool deleteBefore = true)
        {
            string str = (string)null;
            try
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                str = Path.Combine(directoryPath, fileName);
                if (deleteBefore && File.Exists(str))
                {
                    FileHelper.logger.Info((object)("覆盖已有文件：" + str));
                    File.Delete(str);
                }
            }
            catch (Exception ex)
            {
                FileHelper.logger.Error((object)("覆盖文件【" + str + "】异常！"), ex);
                return false;
            }
            FileStream fileStream = (FileStream)null;
            try
            {
                fileStream = new FileStream(str, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] bytes = Encoding.UTF8.GetBytes(fileContent);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
            }
            catch (PathTooLongException ex)
            {
                FileHelper.logger.Error((object)("原文件【" + str + "】命名过长！"), (Exception)ex);
                string newFilePath = FileHelper.GetNewFilePath(str);
                fileContent = ex.Message + "\r\n原命名过长文件路径：" + str + "\r\n\r\n" + fileContent;
                File.AppendAllText(newFilePath, fileContent);
            }
            catch (Exception ex)
            {
                FileHelper.logger.Error((object)("写入文件【" + str + "】异常！"), ex);
                return false;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
            return true;
        }

        private static string GetNewFilePath(string saveFileFullPath)
        {
            string withoutExtension = Path.GetFileNameWithoutExtension(saveFileFullPath);
            string extension = Path.GetExtension(saveFileFullPath);
            string shortName = FileHelper.GetShortName(saveFileFullPath);
            string path = Path.GetDirectoryName(saveFileFullPath.Replace(withoutExtension, shortName + "//" + DateTime.Now.ToString("yyyyMMddHHmmssfff"))) + extension;
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }

        private static string GetShortName(string saveFileFullPath)
        {
            string shortName = "";
            string[] strArray = Path.GetFileNameWithoutExtension(saveFileFullPath).Split('_');
            for (int index = 0; index < 3; ++index)
                shortName = shortName + strArray[index] + "_";
            return shortName;
        }

        public static bool SaveOrApppendContent(
            string directoryPath,
            string fileName,
            string fileContent)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                string path = Path.Combine(directoryPath, fileName);
                if (!File.Exists(path))
                    File.Create(path).Dispose();
                File.AppendAllText(path, fileContent);
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.logger.Info((object)ex.Message);
                return false;
            }
        }

        public static void CopyOldLabFilesToNewLab(string sourcePath, string savePath)
        {
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            string[] directories = Directory.GetDirectories(sourcePath);
            string[] files = Directory.GetFiles(sourcePath);
            if (files.Length != 0)
            {
                for (int index = 0; index < files.Length; ++index)
                    File.Copy(sourcePath + "\\" + Path.GetFileName(files[index]), savePath + "\\" + Path.GetFileName(files[index]), true);
            }
            if (directories.Length == 0)
                return;
            for (int index = 0; index < directories.Length; ++index)
            {
                Directory.GetDirectories(sourcePath + "\\" + Path.GetFileName(directories[index]));
                FileHelper.CopyOldLabFilesToNewLab(sourcePath + "\\" + Path.GetFileName(directories[index]), savePath + "\\" + Path.GetFileName(directories[index]));
            }
        }

        public static void MoveAndReplace(string fullPath, string targetDirectory)
        {
            if (!File.Exists(fullPath))
                return;
            string fileName = Path.GetFileName(fullPath);
            string str = Path.Combine(targetDirectory, fileName);
            if (File.Exists(str))
            {
                try
                {
                    File.Delete(str);
                }
                catch (Exception ex)
                {
                    FileHelper.logger.Debug((object)ex.Message);
                    return;
                }
            }
            try
            {
                File.Move(fullPath, str);
            }
            catch (PathTooLongException ex)
            {
                FileHelper.logger.Error((object)ex);
                FileHelper.DealWithPathTooLongDuringMove(fullPath, str);
            }
            catch (Exception ex)
            {
                FileHelper.logger.Error((object)ex);
            }
        }

        public static string readLastLine(string filePath, int lastLinesSum, Encoding encode)
        {
            if (encode == null)
                encode = Encoding.UTF8;
            List<string> stringList = new List<string>();
            StreamReader streamReader = (StreamReader)null;
            FileStream fileStream = (FileStream)null;
            string str1 = "";
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                streamReader = new StreamReader((Stream)fileStream, encode);
                while (!streamReader.EndOfStream)
                    stringList.Add(streamReader.ReadLine());
            }
            catch (FileNotFoundException ex)
            {
                FileHelper.logger.Error((object)("文件" + filePath + "不存在"), (Exception)ex);
                fileStream?.Close();
                streamReader?.Close();
                return str1;
            }
            finally
            {
                fileStream?.Close();
                streamReader?.Close();
            }
            int count = stringList.Count;
            if (count < lastLinesSum)
            {
                foreach (string str2 in stringList)
                    str1 = str1 + str2 + "\r\n";
            }
            else
            {
                for (int index = count - lastLinesSum; index < count; ++index)
                    str1 = str1 + stringList[index] + "\r\n";
            }
            return str1;
        }

        public static string InitSaveReportDirectory(string directoryName, string mainDirectoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
                if (Directory.Exists(directoryName))
                    FileHelper.logger.Info((object)(mainDirectoryName + directoryName + "创建成功！"));
                else
                    FileHelper.logger.Info((object)(mainDirectoryName + directoryName + "创建失败！"));
            }
            return directoryName;
        }

        public static void WipeFile(string filename, int timesToWrite)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    double num = Math.Ceiling((double)new FileInfo(filename).Length / 512.0);
                    byte[] numArray = new byte[512];
                    RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider();
                    FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                    for (int index1 = 0; index1 < timesToWrite; ++index1)
                    {
                        fileStream.Position = 0L;
                        for (int index2 = 0; (double)index2 < num; ++index2)
                        {
                            cryptoServiceProvider.GetBytes(numArray);
                            fileStream.Write(numArray, 0, numArray.Length);
                        }
                    }
                    fileStream.SetLength(0L);
                    fileStream.Close();
                    DateTime dateTime = new DateTime(2037, 1, 1, 0, 0, 0);
                    File.SetCreationTime(filename, dateTime);
                    File.SetLastAccessTime(filename, dateTime);
                    File.SetLastWriteTime(filename, dateTime);
                    File.Delete(filename);
                }
            }
            catch (Exception ex)
            {
                FileHelper.logger.Info((object)("解除" + filename + "强制删除异常：" + ex.Message + "\n" + ex.StackTrace));
            }
            if (File.Exists(filename))
                return;
            FileHelper.logger.Info((object)("\t删除成功:" + filename));
        }

        public static void DealWithPathTooLongDuringMove(string fileFullName, string targetFullPath)
        {
            byte[] bytes = File.ReadAllBytes(fileFullName);
            File.AppendAllText(FileHelper.GetNewFilePath(targetFullPath), "指定的路径或文件名太长，或者两者都太长。完全限定文件名必须少于 260 个字符，并且目录名必须少于 248 个字符。\r\n原命名过长文件路径：" + fileFullName + "\r\n" + Encoding.UTF8.GetString(bytes));
            File.Delete(fileFullName);
        }

        public static byte[] GetBytes(string path)
        {
            byte[] buffer = (byte[])null;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }
    }
}

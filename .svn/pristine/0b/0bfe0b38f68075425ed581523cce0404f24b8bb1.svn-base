using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using Crc32 = ICSharpCode.SharpZipLib.Checksums.Crc32;
using Org.BouncyCastle.Apache.Bzip2;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public static class ZipUtil
    {
        public static string Compress(string param)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(param);
            MemoryStream memoryStream = new MemoryStream();
            Stream stream = (Stream)new BZip2OutputStream((Stream)memoryStream);
            try
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                stream.Close();
                memoryStream.Close();
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static string Decompress(string param)
        {
            string str = "";
            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(param));
            Stream stream = (Stream)new BZip2InputStream((Stream)memoryStream);
            StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
            try
            {
                str = streamReader.ReadToEnd();
            }
            finally
            {
                stream.Close();
                memoryStream.Close();
            }
            return str;
        }

        public static void ZipFile(
          string FileToZip,
          string ZipedPath,
          string ZipedFileName = "",
          int CompressionLevel = 5,
          int BlockSize = 2048,
          string password = "")
        {
            if (!File.Exists(FileToZip))
                throw new FileNotFoundException("指定要压缩的文件: " + FileToZip + " 不存在!");
            using (FileStream fileStream1 = File.Create(string.IsNullOrEmpty(ZipedFileName) ? ZipedPath + "\\" + new FileInfo(FileToZip).Name.Substring(0, new FileInfo(FileToZip).Name.LastIndexOf('.')) + ".zip" : ZipedPath + "\\" + ZipedFileName + ".zip"))
            {
                using (ZipOutputStream zipOutputStream = new ZipOutputStream((Stream)fileStream1))
                {
                    using (FileStream fileStream2 = new FileStream(FileToZip, FileMode.Open, FileAccess.Read))
                    {
                        ZipEntry entry = new ZipEntry(FileToZip.Substring(FileToZip.LastIndexOf("\\") + 1));
                        if (!string.IsNullOrEmpty(password))
                            zipOutputStream.Password = "123";
                        zipOutputStream.PutNextEntry(entry);
                        zipOutputStream.SetLevel(CompressionLevel);
                        byte[] buffer = new byte[BlockSize];
                        try
                        {
                            int count;
                            do
                            {
                                count = fileStream2.Read(buffer, 0, buffer.Length);
                                ((Stream)zipOutputStream).Write(buffer, 0, count);
                            }
                            while (count > 0);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        fileStream2.Close();
                    }
                    zipOutputStream.Finish();
                    ((Stream)zipOutputStream).Close();
                }
                fileStream1.Close();
            }
        }

        public static void ZipDirectory(
          string DirectoryToZip,
          string ZipedPath,
          string ZipedFileName = "",
          string password = "")
        {
            if (!Directory.Exists(DirectoryToZip))
                throw new FileNotFoundException("指定的目录: " + DirectoryToZip + " 不存在!");
            using (FileStream fileStream = File.Create(string.IsNullOrEmpty(ZipedFileName) ? ZipedPath + "\\" + new DirectoryInfo(DirectoryToZip).Name + ".zip" : ZipedPath + "\\" + ZipedFileName + ".zip"))
            {
                using (ZipOutputStream s = new ZipOutputStream((Stream)fileStream))
                {
                    if (!string.IsNullOrEmpty(password))
                        s.Password = "123";
                    ZipUtil.ZipSetp(DirectoryToZip, s, "");
                }
            }
        }

        private static void ZipSetp(string strDirectory, ZipOutputStream s, string parentPath)
        {
            if ((int)strDirectory[strDirectory.Length - 1] != (int)Path.DirectorySeparatorChar)
                strDirectory += Path.DirectorySeparatorChar.ToString();
            Crc32 crc32 = new Crc32();
            foreach (string fileSystemEntry in Directory.GetFileSystemEntries(strDirectory))
            {
                if (Directory.Exists(fileSystemEntry))
                {
                    string parentPath1 = parentPath + fileSystemEntry.Substring(fileSystemEntry.LastIndexOf("\\") + 1) + "\\";
                    ZipUtil.ZipSetp(fileSystemEntry, s, parentPath1);
                }
                else
                {
                    using (FileStream fileStream = File.OpenRead(fileSystemEntry))
                    {
                        byte[] buffer = new byte[fileStream.Length];
                        fileStream.Read(buffer, 0, buffer.Length);
                        ZipEntry entry = new ZipEntry(parentPath + fileSystemEntry.Substring(fileSystemEntry.LastIndexOf("\\") + 1));
                        entry.DateTime = DateTime.Now;
                        entry.Size = fileStream.Length;
                        fileStream.Close();
                        crc32.Reset();
                        crc32.Update(buffer);
                        entry.Crc = crc32.Value;
                        s.PutNextEntry(entry);
                        ((Stream)s).Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        public static void ExtractZip(
          string ZipFile,
          string TargetDirectory,
          string Password = null,
          bool OverWrite = true)
        {
            if (!Directory.Exists(TargetDirectory))
                throw new FileNotFoundException("指定的目录: " + TargetDirectory + " 不存在!");
            if (!TargetDirectory.EndsWith("\\"))
                TargetDirectory += "\\";
            using (ZipInputStream zipInputStream = new ZipInputStream((Stream)File.OpenRead(ZipFile)))
            {
                zipInputStream.Password = Password;
                ZipEntry nextEntry;
                while ((nextEntry = zipInputStream.GetNextEntry()) != null)
                {
                    string str = "";
                    string name = nextEntry.Name;
                    if (name != "")
                        str = Path.GetDirectoryName(name) + "\\";
                    string fileName = Path.GetFileName(name);
                    Directory.CreateDirectory(TargetDirectory + str);
                    if (string.IsNullOrWhiteSpace(fileName) && (File.Exists(TargetDirectory + str + fileName) & OverWrite || !File.Exists(TargetDirectory + str + fileName)))
                    {
                        using (FileStream fileStream = File.Create(TargetDirectory + str + fileName))
                        {
                            byte[] buffer = new byte[2048];
                            while (true)
                            {
                                int count = ((Stream)zipInputStream).Read(buffer, 0, buffer.Length);
                                if (count > 0)
                                    fileStream.Write(buffer, 0, count);
                                else
                                    break;
                            }
                            fileStream.Close();
                        }
                    }
                }
              ((Stream)zipInputStream).Close();
            }
        }

        public static bool UnZip(string fileToUnZip, string zipedFolder, string password = null)
        {
            bool flag1 = true;
            if (!File.Exists(fileToUnZip))
                throw new FileNotFoundException("文件【" + fileToUnZip + "】不存在！");
            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);
            try
            {
                using (FileStream fileStream1 = File.OpenRead(fileToUnZip))
                {
                    using (ZipInputStream zipInputStream = new ZipInputStream((Stream)fileStream1))
                    {
                        if (!string.IsNullOrEmpty(password))
                            zipInputStream.Password = password;
                        ZipEntry nextEntry;
                        while ((nextEntry = zipInputStream.GetNextEntry()) != null)
                        {
                            if (!string.IsNullOrEmpty(nextEntry.Name))
                            {
                                bool flag2 = false;
                                string path = Path.Combine(zipedFolder, nextEntry.Name).Replace('/', '\\');
                                if (path.EndsWith("\\"))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                else
                                {
                                    long length;
                                    try
                                    {
                                        length = ((Stream)zipInputStream).Length;
                                    }
                                    catch (ZipException ex)
                                    {
                                        length = fileStream1.Length;
                                        flag2 = true;
                                    }
                                    byte[] numArray = new byte[length];
                                    while (length > 0L)
                                    {
                                        length = (long)((Stream)zipInputStream).Read(numArray, 0, numArray.Length);
                                        if (flag2)
                                            numArray = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(numArray).TrimEnd(new char[1]));
                                        using (FileStream fileStream2 = File.Create(path))
                                            fileStream2.Write(numArray, 0, numArray.Length);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                GC.Collect();
            }
            return flag1;
        }
    }
}

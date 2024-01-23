using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.SwagentProxy
{
    internal class SwxaAPI : AbstractLog
    {
        private static unsafe void* G_phDeviceHandle = (void*)null;
        private static object openDeviceLock = new object();

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_OpenDevice(void** phDeviceHandle);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_OpenDeviceWithPath(byte[] name, void** phDeviceHandle);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_CloseDevice(void* phDeviceHandle);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_OpenSession(void* phDeviceHandle, void** phSessionHandle);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_CloseSession(void* phSessionHandle);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_GenerateRandom(
          void* phDeviceHandle,
          int uiLength,
          byte[] pucRandom);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_HashInit(
          void* phDeviceHandle,
          int uiAlgID,
          byte[] pucPublicKey,
          byte[] pucID,
          int uiIDLength);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_HashUpdate(
          void* hSessionHandle,
          byte[] pucData,
          int uiDataLength);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_HashFinal(
          void* hSessionHandle,
          byte[] pucHash,
          int* puiHashLength);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_GetSymmKeyHandle(
          void* hSessionHandle,
          int keyIndex,
          void* hKeyHandle);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_Encrypt(
          void* hSessionHandle,
          void* hKeyHandle,
          int puiAlg,
          byte[] pIv,
          byte[] pucData,
          int uiDataLength,
          byte[] pucEncData,
          int* puiEncDataLength);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_Decrypt(
          void* hSessionHandle,
          void* hKeyHandle,
          int puiAlg,
          byte[] pIv,
          byte[] pucData,
          int uiDataLength,
          byte[] pucEncData,
          int* puiEncDataLength);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_InternalPrivateKeyOperation_RSA(
          void* hSessionHandle,
          int keyIndex,
          int RSA_Type,
          byte[] inData,
          int unDataLen,
          byte[] tmpData,
          int* tmpDataLen);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_InternalPublicKeyOperation_RSA(
          void* hSessionHandle,
          int keyIndex,
          int RSA_Type,
          byte[] inData,
          int unDataLen,
          byte[] tmpData,
          int* tmpDataLen);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_ExportSignPublicKey_ECC(
          void* hSessionHandle,
          int uiKeyIndex,
          byte[] pucPublicKey);

        [DllImport("Libs\\swsds\\swsds.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SDF_InternalSign_ECC(
          void* hSessionHandle,
          int uiISKIndex,
          byte[] pucData,
          int uiDataLength,
          byte[] pucSignature);

        public static unsafe bool OpenDevice(out string message)
        {
            fixed (void** phDeviceHandle = &SwxaAPI.G_phDeviceHandle)
            {
                try
                {
                    int num = SwxaAPI.SDF_OpenDeviceWithPath(Encoding.Default.GetBytes(Environment.CurrentDirectory + "\\Libs\\swsds"), phDeviceHandle);
                    AbstractLog.logger.Info((object)"调用SDF_OpenDevice方法...");
                    if (num != 0)
                    {
                        message = "打开设备失败，错误码:" + num.ToString("X8");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    message = "打开设备失败" + (object)ex;
                    return false;
                }
                message = "";
                return true;
            }
        }

        public static unsafe bool Opensession(
          void* phDeviceHandle,
          void** phSessionHandle,
          out string message)
        {
            try
            {
                int num = SwxaAPI.SDF_OpenSession(phDeviceHandle, phSessionHandle);
                if (num != 0)
                {
                    message = "建立会话失败，错误码:" + num.ToString("X8");
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = "建立会话失败" + (object)ex;
                return false;
            }
            message = "";
            return true;
        }

        public static unsafe byte[] Genrandom(int randLen, out string message)
        {
            byte[] pucRandom = new byte[randLen];
            void* voidPtr;
            if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero && !SwxaAPI.OpenDevice(out message) || !SwxaAPI.Opensession(SwxaAPI.G_phDeviceHandle, &voidPtr, out message))
                return (byte[])null;
            int random = SwxaAPI.SDF_GenerateRandom(voidPtr, randLen, pucRandom);
            if (random != 0)
            {
                message = "生成指定长度的随机数失败, 错误码:" + random.ToString("X8");
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            SwxaAPI.SDF_CloseSession(voidPtr);
            return pucRandom;
        }

        public static unsafe byte[] SignSha1WithRSA1024(
          byte[] pucDataInput,
          int uiInputLength,
          int keyIndex)
        {
            string message1 = "";
            byte[] numArray1 = new byte[32];
            int count = 32;
            if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero)
            {
                lock (SwxaAPI.openDeviceLock)
                {
                    if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero)
                    {
                        if (!SwxaAPI.OpenDevice(out message1))
                        {
                            AbstractLog.logger.Error((object)message1);
                            return (byte[])null;
                        }
                    }
                }
            }
            void* voidPtr;
            int num1 = SwxaAPI.SDF_OpenSession(SwxaAPI.G_phDeviceHandle, &voidPtr);
            if (num1 != 0)
            {
                string message2 = "SDF_OpenSession error:" + num1.ToString("X8");
                AbstractLog.logger.Error((object)message2);
                return (byte[])null;
            }
            int num2 = SwxaAPI.SDF_HashInit(voidPtr, 2, (byte[])null, (byte[])null, 0);
            if (num2 != 0)
            {
                string message3 = "SDF_HashInit error:" + num2.ToString("X8");
                AbstractLog.logger.Error((object)message3);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            if (uiInputLength / 4096 > 0)
            {
                for (int index = 0; index < uiInputLength / 4096; ++index)
                {
                    byte[] numArray2 = new byte[4096];
                    Buffer.BlockCopy((Array)pucDataInput, index * 4096, (Array)numArray2, 0, 4096);
                    int num3 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray2, 4096);
                    if (num3 != 0)
                    {
                        string message4 = "SDF_HashUpdate error:" + num3.ToString("X8");
                        AbstractLog.logger.Error((object)message4);
                        SwxaAPI.SDF_CloseSession(voidPtr);
                        return (byte[])null;
                    }
                }
            }
            int length1 = uiInputLength % 4096;
            if (length1 != 0)
            {
                byte[] numArray3 = new byte[length1];
                Buffer.BlockCopy((Array)pucDataInput, uiInputLength - length1, (Array)numArray3, 0, length1);
                int num4 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray3, length1);
                if (num4 != 0)
                {
                    string message5 = "SDF_HashUpdate error:" + num4.ToString("X8");
                    AbstractLog.logger.Error((object)message5);
                    SwxaAPI.SDF_CloseSession(voidPtr);
                    return (byte[])null;
                }
            }
            int num5 = SwxaAPI.SDF_HashFinal(voidPtr, numArray1, &count);
            if (num5 != 0)
            {
                string message6 = "SDF_HashFinal error:" + num5.ToString("X8");
                AbstractLog.logger.Error((object)message6);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            byte[] numArray4 = new byte[128];
            byte[] sourceArray = new byte[108]
            {
        (byte) 0,
        (byte) 1,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        (byte) 0,
        (byte) 48,
        (byte) 33,
        (byte) 48,
        (byte) 9,
        (byte) 6,
        (byte) 5,
        (byte) 43,
        (byte) 14,
        (byte) 3,
        (byte) 2,
        (byte) 26,
        (byte) 5,
        (byte) 0,
        (byte) 4,
        (byte) 20
            };
            Array.Copy((Array)sourceArray, (Array)numArray4, sourceArray.Length);
            Buffer.BlockCopy((Array)numArray1, 0, (Array)numArray4, sourceArray.Length, count);
            byte[] tmpData = new byte[numArray4.Length];
            int length2 = numArray4.Length;
            int num6 = SwxaAPI.SDF_InternalPrivateKeyOperation_RSA(voidPtr, keyIndex, 65792, numArray4, numArray4.Length, tmpData, &length2);
            if (num6 != 0)
            {
                string message7 = "SDF_InternalPrivateKeyOperation_RSA error:" + num6.ToString("X8");
                AbstractLog.logger.Error((object)message7);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            SwxaAPI.SDF_CloseSession(voidPtr);
            return tmpData;
        }

        public static unsafe byte[] SignSM3WithSM2(
          byte[] pucDataInput,
          int uiInputLength,
          int keyindex)
        {
            string message1 = "";
            byte[] dst = new byte[72];
            int index1 = 0;
            byte[] numArray1 = new byte[32];
            int uiDataLength = 32;
            byte[] bytes = Encoding.UTF8.GetBytes("1234567812345678");
            byte[] pucPublicKey = new byte[68];
            byte[] numArray2 = new byte[64];
            if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero)
            {
                lock (SwxaAPI.openDeviceLock)
                {
                    if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero)
                    {
                        if (!SwxaAPI.OpenDevice(out message1))
                        {
                            AbstractLog.logger.Error((object)message1);
                            return (byte[])null;
                        }
                    }
                }
            }
            void* voidPtr;
            int num1 = SwxaAPI.SDF_OpenSession(SwxaAPI.G_phDeviceHandle, &voidPtr);
            if (num1 != 0)
            {
                string message2 = "SDF_OpenSession error:" + num1.ToString("X8");
                AbstractLog.logger.Error((object)message2);
                return (byte[])null;
            }
            int num2 = SwxaAPI.SDF_ExportSignPublicKey_ECC(voidPtr, keyindex, pucPublicKey);
            if (num2 != 0)
            {
                string message3 = "SDF_ExportSignPublicKey_ECC error:" + num2.ToString("X8");
                AbstractLog.logger.Error((object)message3);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            int num3 = SwxaAPI.SDF_HashInit(voidPtr, 1, pucPublicKey, bytes, 16);
            if (num3 != 0)
            {
                string message4 = "SDF_HashInit error:" + num3.ToString("X8");
                AbstractLog.logger.Error((object)message4);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            if (uiInputLength / 4096 > 0)
            {
                for (int index2 = 0; index2 < uiInputLength / 4096; ++index2)
                {
                    byte[] numArray3 = new byte[4096];
                    Buffer.BlockCopy((Array)pucDataInput, index2 * 4096, (Array)numArray3, 0, 4096);
                    int num4 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray3, 4096);
                    if (num4 != 0)
                    {
                        string message5 = "SDF_HashUpdate error:" + num4.ToString("X8");
                        AbstractLog.logger.Error((object)message5);
                        SwxaAPI.SDF_CloseSession(voidPtr);
                        return (byte[])null;
                    }
                }
            }
            int length = uiInputLength % 4096;
            if (length != 0)
            {
                byte[] numArray4 = new byte[length];
                Buffer.BlockCopy((Array)pucDataInput, uiInputLength - length, (Array)numArray4, 0, length);
                int num5 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray4, length);
                if (num5 != 0)
                {
                    string message6 = "SDF_HashUpdate error:" + num5.ToString("X8");
                    AbstractLog.logger.Error((object)message6);
                    SwxaAPI.SDF_CloseSession(voidPtr);
                    return (byte[])null;
                }
            }
            int num6 = SwxaAPI.SDF_HashFinal(voidPtr, numArray1, &uiDataLength);
            if (num6 != 0)
            {
                string message7 = "SDF_HashFinal error:" + num6.ToString("X8");
                AbstractLog.logger.Error((object)message7);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            int num7 = SwxaAPI.SDF_InternalSign_ECC(voidPtr, keyindex, numArray1, uiDataLength, numArray2);
            if (num7 != 0)
            {
                string message8 = "SDF_InternalSign_ECC error:" + num7.ToString("X8");
                AbstractLog.logger.Error((object)message8);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            dst[index1] = (byte)48;
            int index3 = index1 + 1;
            dst[index3] = (byte)70;
            int index4 = index3 + 1;
            dst[index4] = (byte)2;
            int index5 = index4 + 1;
            dst[index5] = (byte)33;
            int index6 = index5 + 1;
            dst[index6] = (byte)0;
            int dstOffset1 = index6 + 1;
            Buffer.BlockCopy((Array)numArray2, 0, (Array)dst, dstOffset1, 32);
            int index7 = dstOffset1 + 32;
            dst[index7] = (byte)2;
            int index8 = index7 + 1;
            dst[index8] = (byte)33;
            int index9 = index8 + 1;
            dst[index9] = (byte)0;
            int dstOffset2 = index9 + 1;
            Buffer.BlockCopy((Array)numArray2, 32, (Array)dst, dstOffset2, 32);
            SwxaAPI.SDF_CloseSession(voidPtr);
            return dst;
        }

        public static unsafe string SignSha1WithRSA2048(
          byte[] pucDataInput,
          int uiInputLength,
          int keyIndex)
        {
            string message = "";
            byte[] numArray1 = new byte[32];
            int count = 32;
            if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero && !SwxaAPI.OpenDevice(out message))
                return (string)null;
            void* voidPtr;
            int num1 = SwxaAPI.SDF_OpenSession(SwxaAPI.G_phDeviceHandle, &voidPtr);
            if (num1 != 0)
            {
                string str = "SDF_OpenSession error:" + num1.ToString("X8");
                SwxaAPI.SDF_CloseSession(voidPtr);
                return str;
            }
            int num2 = SwxaAPI.SDF_HashInit(voidPtr, 2, (byte[])null, (byte[])null, 0);
            if (num2 != 0)
            {
                string str = "SDF_HashInit error:" + num2.ToString("X8");
                SwxaAPI.SDF_CloseSession(voidPtr);
                return str;
            }
            if (uiInputLength / 4096 > 0)
            {
                for (int index = 0; index < uiInputLength / 4096; ++index)
                {
                    byte[] numArray2 = new byte[4096];
                    Buffer.BlockCopy((Array)pucDataInput, index * 4096, (Array)numArray2, 0, 4096);
                    int num3 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray2, 4096);
                    if (num3 != 0)
                    {
                        string str = "SDF_HashUpdate error:" + num3.ToString("X8");
                        SwxaAPI.SDF_CloseSession(voidPtr);
                        return str;
                    }
                }
            }
            int length1 = uiInputLength % 4096;
            if (length1 != 0)
            {
                byte[] numArray3 = new byte[length1];
                Buffer.BlockCopy((Array)pucDataInput, uiInputLength - length1, (Array)numArray3, 0, length1);
                int num4 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray3, length1);
                if (num4 != 0)
                {
                    string str = "SDF_HashUpdate error:" + num4.ToString("X8");
                    SwxaAPI.SDF_CloseSession(voidPtr);
                    return str;
                }
            }
            int num5 = SwxaAPI.SDF_HashFinal(voidPtr, numArray1, &count);
            if (num5 != 0)
            {
                string str = "SDF_HashFinal error:" + num5.ToString("X8");
                SwxaAPI.SDF_CloseSession(voidPtr);
                return str;
            }
            byte[] numArray4 = new byte[256];
            byte[] sourceArray = new byte[236]
            {
        (byte) 0,
        (byte) 1,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        byte.MaxValue,
        (byte) 0,
        (byte) 48,
        (byte) 33,
        (byte) 48,
        (byte) 9,
        (byte) 6,
        (byte) 5,
        (byte) 43,
        (byte) 14,
        (byte) 3,
        (byte) 2,
        (byte) 26,
        (byte) 5,
        (byte) 0,
        (byte) 4,
        (byte) 20
            };
            Array.Copy((Array)sourceArray, (Array)numArray4, sourceArray.Length);
            Buffer.BlockCopy((Array)numArray1, 0, (Array)numArray4, sourceArray.Length, count);
            byte[] numArray5 = new byte[numArray4.Length];
            int length2 = numArray4.Length;
            int num6 = SwxaAPI.SDF_InternalPrivateKeyOperation_RSA(voidPtr, keyIndex, 65792, numArray4, numArray4.Length, numArray5, &length2);
            if (num6 != 0)
            {
                string str = "SDF_InternalPrivateKeyOperation_RSA error:" + num6.ToString("X8");
                SwxaAPI.SDF_CloseSession(voidPtr);
                return str;
            }
            SwxaAPI.SDF_CloseSession(voidPtr);
            return Convert.ToBase64String(numArray5);
        }

        public static unsafe byte[] InternalPublicKeyOperation_RSA(
          int keyIndex,
          byte[] inData,
          out string message)
        {
            int length = inData.Length;
            byte[] tmpData = new byte[length];
            int num1 = 0;
            inData[0] = (byte)0;
            void* voidPtr;
            if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero && !SwxaAPI.OpenDevice(out message) || !SwxaAPI.Opensession(SwxaAPI.G_phDeviceHandle, &voidPtr, out message))
                return (byte[])null;
            int num2 = SwxaAPI.SDF_InternalPublicKeyOperation_RSA(voidPtr, keyIndex, 65792, inData, length, tmpData, &num1);
            if (num2 != 0)
            {
                message = "内部公钥验签失败, 错误码:" + num2.ToString("X8");
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            SwxaAPI.SDF_CloseSession(voidPtr);
            return tmpData;
        }

        public static unsafe byte[] HashDigest(byte[] inData)
        {
            byte[] pucHash = new byte[20];
            string message1 = "";
            int length1 = inData.Length;
            int num1 = 0;
            if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero)
            {
                lock (SwxaAPI.openDeviceLock)
                {
                    if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero)
                    {
                        if (!SwxaAPI.OpenDevice(out message1))
                        {
                            AbstractLog.logger.Error((object)message1);
                            return (byte[])null;
                        }
                    }
                }
            }
            void* voidPtr;
            int num2 = SwxaAPI.SDF_OpenSession(SwxaAPI.G_phDeviceHandle, &voidPtr);
            if (num2 != 0)
            {
                string message2 = "SDF_OpenSession error:" + num2.ToString("X8");
                AbstractLog.logger.Error((object)message2);
                return (byte[])null;
            }
            int num3 = SwxaAPI.SDF_HashInit(voidPtr, 2, (byte[])null, (byte[])null, 0);
            if (num3 != 0)
            {
                string message3 = "SDF_HashInit error:" + num3.ToString("X8");
                AbstractLog.logger.Error((object)message3);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            if (length1 / 4096 > 0)
            {
                for (int index = 0; index < length1 / 4096; ++index)
                {
                    byte[] numArray = new byte[4096];
                    Buffer.BlockCopy((Array)inData, index * 4096, (Array)numArray, 0, 4096);
                    int num4 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray, 4096);
                    if (num4 != 0)
                    {
                        string message4 = "SDF_HashUpdate error:" + num4.ToString("X8");
                        AbstractLog.logger.Error((object)message4);
                        SwxaAPI.SDF_CloseSession(voidPtr);
                        return (byte[])null;
                    }
                }
            }
            int length2 = length1 % 4096;
            if (length2 != 0)
            {
                byte[] numArray = new byte[length2];
                Buffer.BlockCopy((Array)inData, length1 - length2, (Array)numArray, 0, length2);
                int num5 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray, length2);
                if (num5 != 0)
                {
                    string message5 = "SDF_HashUpdate error:" + num5.ToString("X8");
                    AbstractLog.logger.Error((object)message5);
                    SwxaAPI.SDF_CloseSession(voidPtr);
                    return (byte[])null;
                }
            }
            int num6 = SwxaAPI.SDF_HashFinal(voidPtr, pucHash, &num1);
            if (num6 != 0)
            {
                string message6 = "SDF_HashFinal error:" + num6.ToString("X8");
                AbstractLog.logger.Error((object)message6);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            SwxaAPI.SDF_CloseSession(voidPtr);
            return pucHash;
        }

        public static unsafe byte[] HashDigestSM3(byte[] inData)
        {
            byte[] pucHash = new byte[32];
            string message1 = "";
            int length1 = inData.Length;
            int num1 = 0;
            if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero)
            {
                lock (SwxaAPI.openDeviceLock)
                {
                    if ((IntPtr)SwxaAPI.G_phDeviceHandle == IntPtr.Zero)
                    {
                        if (!SwxaAPI.OpenDevice(out message1))
                        {
                            AbstractLog.logger.Error((object)message1);
                            return (byte[])null;
                        }
                    }
                }
            }
            void* voidPtr;
            int num2 = SwxaAPI.SDF_OpenSession(SwxaAPI.G_phDeviceHandle, &voidPtr);
            if (num2 != 0)
            {
                string message2 = "SDF_OpenSession error:" + num2.ToString("X8");
                AbstractLog.logger.Error((object)message2);
                return (byte[])null;
            }
            int num3 = SwxaAPI.SDF_HashInit(voidPtr, 1, (byte[])null, (byte[])null, 0);
            if (num3 != 0)
            {
                string message3 = "SDF_HashInit error:" + num3.ToString("X8");
                AbstractLog.logger.Error((object)message3);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            if (length1 / 4096 > 0)
            {
                for (int index = 0; index < length1 / 4096; ++index)
                {
                    byte[] numArray = new byte[4096];
                    Buffer.BlockCopy((Array)inData, index * 4096, (Array)numArray, 0, 4096);
                    int num4 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray, 4096);
                    if (num4 != 0)
                    {
                        string message4 = "SDF_HashUpdate error:" + num4.ToString("X8");
                        AbstractLog.logger.Error((object)message4);
                        SwxaAPI.SDF_CloseSession(voidPtr);
                        return (byte[])null;
                    }
                }
            }
            int length2 = length1 % 4096;
            if (length2 != 0)
            {
                byte[] numArray = new byte[length2];
                Buffer.BlockCopy((Array)inData, length1 - length2, (Array)numArray, 0, length2);
                int num5 = SwxaAPI.SDF_HashUpdate(voidPtr, numArray, length2);
                if (num5 != 0)
                {
                    string message5 = "SDF_HashUpdate error:" + num5.ToString("X8");
                    AbstractLog.logger.Error((object)message5);
                    SwxaAPI.SDF_CloseSession(voidPtr);
                    return (byte[])null;
                }
            }
            int num6 = SwxaAPI.SDF_HashFinal(voidPtr, pucHash, &num1);
            if (num6 != 0)
            {
                string message6 = "SDF_HashFinal error:" + num6.ToString("X8");
                AbstractLog.logger.Error((object)message6);
                SwxaAPI.SDF_CloseSession(voidPtr);
                return (byte[])null;
            }
            SwxaAPI.SDF_CloseSession(voidPtr);
            return pucHash;
        }

        public static bool PasswordEquals(byte[] b1, byte[] b2, int len) => b1.Length == len && b2 != null && !((IEnumerable<byte>)b1).Where<byte>((Func<byte, int, bool>)((t, i) => (int)t != (int)b2[i])).Any<byte>();
    }
}

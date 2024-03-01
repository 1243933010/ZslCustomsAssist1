﻿using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Utils.sm;
using ZslCustomsAssist.Utils.Log;
using Newtonsoft.Json;

namespace ZslCustomsAssist.Utils
{
    public class SignXmlUtil
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");

        public static string GetDigestValue(XmlDocument signdXmlDoc)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(signdXmlDoc.NameTable);
            nsmgr.AddNamespace("zsl", "http://www.w3.org/2000/09/xmldsig#");
            return signdXmlDoc.SelectSingleNode("//zsl:Signature//zsl:DigestValue", nsmgr).InnerText;
        }

        public static string GetSignedInfoStr(XmlDocument signdXmlDoc) => signdXmlDoc.GetElementsByTagName("ds:Signature").Item(0).OuterXml;

        public static void SignXmlDoc(XmlDocument xmlDoc, string digestValue)
        {
            AbstractLog.logger.Error((object)(ServerCore.userData.szAlgoFlag != 65536UL).ToString() + "请求摘要数据==");
            string xml1 = (ServerCore.userData.szAlgoFlag != 65536UL ? XmlHelp.GetXmlContent("./tpl/SM3_SignedInfo_tpl.xml", (Encoding)null) : XmlHelp.GetXmlContent("./tpl/SignedInfo_tpl.xml", (Encoding)null)).Replace("${ds:DigestValue}", digestValue).Replace("${ds:RootElementAttrStr}", SignXmlUtil.EditXmlRootEleAttr(xmlDoc));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml1);
            string newValue = SPSecureAPI.SpcSignData(DeclareMessageXmlSign.getC14NTranXml(doc));
            string xml2 = (ServerCore.userData.szAlgoFlag != 65536UL ? XmlHelp.GetXmlContent("./tpl/SM2_Signature_tpl_temp_bak.xml", (Encoding)null) : XmlHelp.GetXmlContent("./tpl/Signature_tpl_temp_bak.xml", (Encoding)null)).Replace("${ds:DigestValue}", digestValue).Replace("${ds:SignatureValue}", newValue).Replace("${ds:KeyName}", ServerCore.userData.szCertNo).Replace("${ds:X509Certificate}", ServerCore.userData.szSignCert);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(xml2);
            XmlElement documentElement = xmlDocument.DocumentElement;
            XmlNode newChild = xmlDoc.ImportNode((XmlNode)documentElement, true);
            xmlDoc.DocumentElement.AppendChild(newChild);
        }

        //测试函数，测试数据是否完成
        public static void SignXmlDoc1(XmlDocument xmlDoc, string digestValue)
        {
            AbstractLog.logger.Error((object)(ServerCore.userData.szAlgoFlag != 65536UL).ToString() + "请求摘要数据==");
            string xml1 = (ServerCore.userData.szAlgoFlag != 65536UL ? XmlHelp.GetXmlContent("./tpl/SM3_SignedInfo_tpl.xml", (Encoding)null) : XmlHelp.GetXmlContent("./tpl/SignedInfo_tpl.xml", (Encoding)null)).Replace("${ds:DigestValue}", digestValue).Replace("${ds:RootElementAttrStr}", SignXmlUtil.EditXmlRootEleAttr(xmlDoc));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml1);
           // AbstractLog.logger.Error((object)JsonConvert.SerializeObject(doc, Newtonsoft.Json.Formatting.Indented) + "请求摘要数据======333");
            string newValue = SPSecureAPI.SpcSignData(DeclareMessageXmlSign.getC14NTranXml(doc));
            string xml2 = (ServerCore.userData.szAlgoFlag != 65536UL ? XmlHelp.GetXmlContent("./tpl/SM2_Signature_tpl_temp_bak.xml", (Encoding)null) : XmlHelp.GetXmlContent("./tpl/Signature_tpl_temp_bak.xml", (Encoding)null)).Replace("${ds:DigestValue}", digestValue).Replace("${ds:SignatureValue}", newValue).Replace("${ds:KeyName}", ServerCore.userData.szCertNo).Replace("${ds:X509Certificate}", ServerCore.userData.szSignCert);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(xml2);
            XmlElement documentElement = xmlDocument.DocumentElement;
            XmlNode newChild = xmlDoc.ImportNode((XmlNode)documentElement, true);
            xmlDoc.DocumentElement.AppendChild(newChild);
        }

        public static string SignXmlAbstractDoc(XmlDocument xmlDoc)
        {
            
            string newValue = SPSecureAPI.SpcSignData(DeclareMessageXmlSign.getC14NTranXml(xmlDoc));
            return newValue;
        }

        public static string SignXmlDocTran(string report)
        {
            string data = SignXmlUtil.Canonicalize(report);
            XElement xelement = XElement.Parse(report, LoadOptions.PreserveWhitespace);
            bool flag = false;
            StringBuilder stringBuilder = new StringBuilder("");
            foreach (object attribute in xelement.Attributes())
            {
                string str = attribute.ToString();
                if (str.ToLower().StartsWith("xmlns:ds"))
                    flag = true;
                if (str.ToLower().StartsWith("xmlns"))
                    stringBuilder.Append(string.Format(" {0}", (object)str));
            }
            if (!flag)
                return report;
            string digest = ServerCore.userData.szAlgoFlag != 65536UL ? Sm3Crypto.ToSM3Base64Str(data) : SignXmlUtil.GetDigest(data);
            string signedInfo = ServerCore.userData.szAlgoFlag != 65536UL ? SignXmlUtil.GetSignedSM3InfoContent(digest) : SignXmlUtil.GetSignedSHA1InfoContent(digest);
            string xmlData = ServerCore.userData.szAlgoFlag != 65536UL ? SignXmlUtil.GetSignedSM3InfoContentPlusWithNameSpace(digest, stringBuilder.ToString()) : SignXmlUtil.GetSignedSHA1InfoContentPlusWithNameSpace(digest, stringBuilder.ToString());
            string szCertNo = ServerCore.userData.szCertNo;
            string signatureValue = SPSecureAPI.SpcSignData(SignXmlUtil.Canonicalize(xmlData));
            report = SignXmlUtil.ComposeW3cEnvelopedXmlEx(SignXmlUtil.ComposeW3cEnvelopedSignedInfoXml(signedInfo, signatureValue, szCertNo), report);
            return report;
        }

        private static string Canonicalize(string xmlData)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(xmlData);
            using (MemoryStream output1 = new MemoryStream())
            {
                using (XmlWriter w = XmlWriter.Create((Stream)output1))
                {
                    xmlDocument.WriteTo(w);
                    w.Flush();
                }
                output1.Position = 0L;
                XmlDsigC14NTransform dsigC14Ntransform = new XmlDsigC14NTransform();
                dsigC14Ntransform.LoadInput((object)output1);
                using (MemoryStream output2 = (MemoryStream)dsigC14Ntransform.GetOutput(typeof(Stream)))
                    return Encoding.UTF8.GetString(output2.ToArray());
            }
        }

        public static string GetDigest(string data)
        {
            AbstractLog.logger.Info((string)("【" + "】byte1-------！"));

            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (SHA1 shA1 = (SHA1)new SHA1CryptoServiceProvider())
                return Convert.ToBase64String(shA1.ComputeHash(bytes));
        }

        private static string GetSignedSHA1InfoContent(string digest)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<ds:SignedInfo>");
            stringBuilder.Append("<ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/>");
            stringBuilder.Append("<ds:SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/>");
            stringBuilder.Append("<ds:Reference URI=\"\">");
            stringBuilder.Append("<ds:Transforms><ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></ds:Transforms>");
            stringBuilder.Append("<ds:DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/>");
            stringBuilder.Append("<ds:DigestValue>");
            stringBuilder.Append(digest);
            stringBuilder.Append("</ds:DigestValue></ds:Reference></ds:SignedInfo>");
            return stringBuilder.ToString();
        }

        private static string GetSignedSM3InfoContent(string digest)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<ds:SignedInfo>");
            stringBuilder.Append("<ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/>");
            stringBuilder.Append("<ds:SignatureMethod Algorithm=\"http://www.chinaport.gov.cn/2022/04/xmldsig#sm2-sm3\"/>");
            stringBuilder.Append("<ds:Reference URI=\"\">");
            stringBuilder.Append("<ds:Transforms><ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></ds:Transforms>");
            stringBuilder.Append("<ds:DigestMethod Algorithm=\"http://www.chinaport.gov.cn/2022/04/xmldsig#sm3\"/>");
            stringBuilder.Append("<ds:DigestValue>");
            stringBuilder.Append(digest);
            stringBuilder.Append("</ds:DigestValue></ds:Reference></ds:SignedInfo>");
            return stringBuilder.ToString();
        }

        private static string GetSignedSHA1InfoContentPlusWithNameSpace(
          string digest,
          string nameSpaceAttrs)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<ds:SignedInfo ").Append(nameSpaceAttrs).Append(">");
            stringBuilder.Append("<ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/>");
            stringBuilder.Append("<ds:SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/>");
            stringBuilder.Append("<ds:Reference URI=\"\">");
            stringBuilder.Append("<ds:Transforms><ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></ds:Transforms>");
            stringBuilder.Append("<ds:DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/>");
            stringBuilder.Append("<ds:DigestValue>");
            stringBuilder.Append(digest);
            stringBuilder.Append("</ds:DigestValue></ds:Reference></ds:SignedInfo>");
            return stringBuilder.ToString();
        }

        private static string GetSignedSM3InfoContentPlusWithNameSpace(
          string digest,
          string nameSpaceAttrs)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<ds:SignedInfo ").Append(nameSpaceAttrs).Append(">");
            stringBuilder.Append("<ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/>");
            stringBuilder.Append("<ds:SignatureMethod Algorithm=\"http://www.chinaport.gov.cn/2022/04/xmldsig#sm2-sm3\"/>");
            stringBuilder.Append("<ds:Reference URI=\"\">");
            stringBuilder.Append("<ds:Transforms><ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></ds:Transforms>");
            stringBuilder.Append("<ds:DigestMethod Algorithm=\"http://www.chinaport.gov.cn/2022/04/xmldsig#sm3\"/>");
            stringBuilder.Append("<ds:DigestValue>");
            stringBuilder.Append(digest);
            stringBuilder.Append("</ds:DigestValue></ds:Reference></ds:SignedInfo>");
            return stringBuilder.ToString();
        }

        private static string ComposeW3cEnvelopedSignedInfoXml(
          string signedInfo,
          string signatureValue,
          string keyName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<ds:Signature>");
            stringBuilder.Append(signedInfo);
            stringBuilder.Append("<ds:SignatureValue>");
            stringBuilder.Append(signatureValue);
            stringBuilder.Append("</ds:SignatureValue>");
            stringBuilder.Append("<ds:KeyInfo><ds:KeyName>");
            stringBuilder.Append(keyName);
            stringBuilder.Append("</ds:KeyName>");
            stringBuilder.Append("</ds:KeyInfo>");
            stringBuilder.Append("</ds:Signature>");
            return stringBuilder.ToString();
        }

        private static string ComposeW3cEnvelopedXmlEx(string signatureXml, string srcData)
        {
            int num = srcData.LastIndexOf("</");
            if (num < 0)
            {
                Console.WriteLine("密码不能为空,请传入密码!");
                throw new Exception("密码不能为空.");
            }
            return string.Format("{0}{1}{2}", (object)srcData.Substring(0, num), (object)signatureXml, (object)srcData.Substring(num));
        }

        public static void SignXmlDoc(
          XmlDocument xmlDoc,
          string digestValue,
          string signatureValue,
          string certNo,
          string signCert,
          string keyAlgorithm)
        {
            string xml = (!"1.2.840.113549.1.1.1".Equals(keyAlgorithm) ? XmlHelp.GetXmlContent("./tpl/SM2_Signature_tpl.xml", (Encoding)null) : XmlHelp.GetXmlContent("./tpl/Signature_tpl.xml", (Encoding)null)).Replace("${ds:DigestValue}", digestValue).Replace("${ds:SignatureValue}", signatureValue).Replace("${ds:KeyName}", certNo).Replace("${ds:X509Certificate}", signCert);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlElement documentElement = xmlDocument.DocumentElement;
            XmlNode newChild = xmlDoc.ImportNode((XmlNode)documentElement, true);
            xmlDoc.DocumentElement.AppendChild(newChild);
        }

        public static void SignXmlDocTran(
          XmlDocument xmlDoc,
          string digestValue,
          string signatureValue,
          string certNo,
          string keyAlgorithm)
        {
            string xml = (!"1.2.840.113549.1.1.1".Equals(keyAlgorithm) ? XmlHelp.GetXmlContent("./tpl/SM2_Signature_tpl_tran.xml", (Encoding)null) : XmlHelp.GetXmlContent("./tpl/Signature_tpl_tran.xml", (Encoding)null)).Replace("${ds:DigestValue}", digestValue).Replace("${ds:SignatureValue}", signatureValue).Replace("${ds:KeyName}", certNo);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlElement documentElement = xmlDocument.DocumentElement;
            XmlNode newChild = xmlDoc.ImportNode((XmlNode)documentElement, true);
            xmlDoc.DocumentElement.AppendChild(newChild);
        }

        public static string EditXmlRootEleAttr(XmlDocument xmlDoc)
        {
            List<string> stringList = new List<string>()
      {
        "xmlns:ds"
      };
            Hashtable hashtable = new Hashtable()
      {
        {
          (object) "xmlns:ds",
          (object) "http://www.w3.org/2000/09/xmldsig#"
        }
      };
            foreach (XmlAttribute attribute in (XmlNamedNodeMap)xmlDoc.DocumentElement.Attributes)
            {
                if (attribute.Name.ToLower().StartsWith("xmlns") && !(attribute.Name.ToLower() == "xmlns:ds"))
                {
                    stringList.Add(attribute.Name);
                    hashtable.Add((object)attribute.Name, (object)attribute.Value);
                }
            }
            stringList.Sort();
            string str = "";
            foreach (string key in stringList)
                str = str + " " + key + "=\"" + hashtable[(object)key].ToString() + "\"";
            return str;
        }

        private static string SyncXmlRootEleAttr(XmlDocument xmlDoc)
        {
            XmlAttributeCollection attributes = xmlDoc.DocumentElement.Attributes;
            string str1 = "";
            string str2 = "";
            string str3 = "";
            string str4 = " xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"";
            string str5 = "";
            string str6 = "";
            foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap)attributes)
            {
                string name = xmlAttribute.Name;
                string str7 = xmlAttribute.Value;
                if (!(name.ToLower() == "guid") && !(name.ToLower() == "version") && !(name.ToLower() == "xmlns:ds"))
                {
                    if ("xmlns" == name.ToLower())
                        str2 = " " + name + "=\"" + str7 + "\"";
                    else if ("xmlns:ceb" == name.ToLower())
                        str3 = str3 + " " + name + "=\"" + str7 + "\"";
                    else if ("xmlns:ns2" == name.ToLower())
                        str5 = str5 + " " + name + "=\"" + str7 + "\"";
                    else if ("xmlns:xsi" == name.ToLower())
                        str6 = str6 + " " + name + "=\"" + str7 + "\"";
                    else
                        str1 = str1 + " " + name + "=\"" + str7 + "\"";
                }
            }
            return str2 + str3 + str4 + str5 + str6 + str1;
        }

        public string HashAndSignString(string plaintext, string privateKey)
        {
            byte[] bytes = new UnicodeEncoding().GetBytes(plaintext);
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                cryptoServiceProvider.FromXmlString(privateKey);
                return Convert.ToBase64String(cryptoServiceProvider.SignData(bytes, (object)new SHA1CryptoServiceProvider()));
            }
        }

        public bool VerifySigned(string plaintext, string SignedData, string publicKey)
        {
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                cryptoServiceProvider.FromXmlString(publicKey);
                byte[] bytes = new UnicodeEncoding().GetBytes(plaintext);
                byte[] signature = Convert.FromBase64String(SignedData);
                return cryptoServiceProvider.VerifyData(bytes, (object)new SHA1CryptoServiceProvider(), signature);
            }
        }

        public void TestSign()
        {
            string plaintext = "测试签名内容！";
            Console.WriteLine("签名数为：{0}", (object)plaintext);
            KeyValuePair<string, string> rsaKey = Encrypter.CreateRSAKey();
            string privateKey = rsaKey.Value;
            string key = rsaKey.Key;
            string SignedData = Encrypter.HashAndSignString(plaintext, privateKey);
            Console.WriteLine("数字签名:{0}", (object)SignedData);
            Console.WriteLine("签名验证结果：{0}", (object)Encrypter.VerifySigned(plaintext, SignedData, key));
        }

        private void SignXmlDoc(string xmlKeyPair, XmlDocument xmlDoc, XmlTextWriter writer)
        {
            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
            cryptoServiceProvider.FromXmlString(xmlKeyPair);
            SignedXml signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = (AsymmetricAlgorithm)cryptoServiceProvider
            };
            signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
            Reference reference = new Reference("");
            reference.AddTransform((Transform)new XmlDsigEnvelopedSignatureTransform(false));
            signedXml.AddReference(reference);
            signedXml.ComputeSignature();
            XmlElement xml = signedXml.GetXml();
            xmlDoc.DocumentElement.AppendChild((XmlNode)xml);
            xmlDoc.WriteTo((XmlWriter)writer);
            writer.Flush();
        }

        public static bool VerifyXmlDoc(string xmlPubKey, XmlDocument xmlDoc)
        {
            RSACryptoServiceProvider key = new RSACryptoServiceProvider();
            key.FromXmlString(xmlPubKey);
            SignedXml signedXml = new SignedXml(xmlDoc);
            XmlNode xmlNode = xmlDoc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[0];
            signedXml.LoadXml((XmlElement)xmlNode);
            return signedXml.CheckSignature((AsymmetricAlgorithm)key);
        }

        public static string WarpLinePer76(string orgString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < orgString.Length; ++index)
            {
                stringBuilder.Append(orgString[index]);
                if (index != 0 && (index + 1) % 76 == 0)
                    stringBuilder.Append("\r\n");
            }
            return stringBuilder.ToString();
        }

        public static void TestTransSign(string directory, string successDir)
        {
            FileInfo[] files = new DirectoryInfo(directory).GetFiles();
            StringBuilder stringBuilder1 = new StringBuilder(XmlHelp.GetXmlContent("./tpl/DxpMsg_tpl.xml", Encoding.UTF8));
            StringBuilder stringBuilder2 = new StringBuilder(XmlHelp.GetXmlContent("./tpl/SendReport_tpl.xml", Encoding.UTF8));
            foreach (FileInfo fileInfo in files)
            {
                string newValue = "";
                try
                {
                    string xmlKeyPair = FileHelper.readLastLine(fileInfo.FullName, 1, Encoding.UTF8);
                    newValue = Base64Helper.Base64Encode(stringBuilder1.ToString());
                    stringBuilder2 = stringBuilder2.Replace("${Data:EncryptData}", newValue);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(stringBuilder2.ToString());
                    XmlHelp.SignXmlDoc(xmlKeyPair, xmlDoc, "");
                    FileHelper.MoveAndReplace(fileInfo.FullName, successDir);
                }
                catch (Exception ex)
                {
                    SignXmlUtil.logger.Error((object)("传输加签异常!\npoint_tpl：\n" + LogHelper.CutLongValue(stringBuilder1.ToString()) + "\nencryptedReport：\n" + LogHelper.CutLongValue(newValue) + "\nxmlDoc：\n" + LogHelper.CutLongValue(stringBuilder2.ToString())), ex);
                }
            }
        }
    }
}
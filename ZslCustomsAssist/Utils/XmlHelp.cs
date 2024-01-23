using log4net.Util;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Security.Cryptography.Xml;
using Transform = System.Security.Cryptography.Xml.Transform;

namespace ZslCustomsAssist.Utils
{
    public class XmlHelp
    {
        public static string XmlTextValidateByXsd(string xmlText, string schemaFile)
        {
            StringBuilder sb = new StringBuilder();
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                ValidationType = ValidationType.Schema
            };
            settings.Schemas.Add((string)null, schemaFile);
            settings.ValidationEventHandler += (ValidationEventHandler)((x, y) => sb.AppendFormat("{0}\n", (object)y.Message));
            using (XmlReader xmlReader = XmlReader.Create((TextReader)new StringReader(xmlText), settings))
            {
                try
                {
                    while (xmlReader.Read())
                        ;
                }
                catch (XmlException ex)
                {
                    sb.AppendFormat("{0}\n", (object)ex.Message);
                }
                finally
                {
                    xmlReader?.Close();
                }
            }
            return sb.ToString();
        }

        public static string XmlValidationByXsd(string xmlFile, string xsdFile, string namespaceUrl = null)
        {
            StringBuilder sb = new StringBuilder();
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                ValidationType = ValidationType.Schema
            };
            settings.Schemas.Add(namespaceUrl, xsdFile);
            settings.ValidationEventHandler += (ValidationEventHandler)((x, y) => sb.AppendFormat("{0}|", (object)y.Message));
            using (XmlReader xmlReader = XmlReader.Create(xmlFile, settings))
            {
                try
                {
                    while (xmlReader.Read())
                        ;
                }
                catch (XmlException ex)
                {
                    sb.AppendFormat("{0}|", (object)ex.Message);
                }
            }
            return sb.ToString();
        }

        public static string XmlValidateByXsd(string xmlText, string schemaFile)
        {
            StringBuilder sb = new StringBuilder();
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                ValidationType = ValidationType.Schema
            };
            settings.Schemas.Add((string)null, schemaFile);
            settings.ValidationEventHandler += (ValidationEventHandler)((x, y) => sb.AppendFormat("{0}\n", (object)y.Message));
            using (XmlReader xmlReader = XmlReader.Create((TextReader)new StringReader(xmlText), settings))
            {
                try
                {
                    while (xmlReader.Read())
                        ;
                }
                catch (XmlException ex)
                {
                    sb.AppendFormat("{0}\n", (object)ex.Message);
                }
            }
            return sb.ToString();
        }

        public static string SerializeToXml<T>(T myObject)
        {
            if ((object)myObject == null)
                return string.Empty;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream w = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter((Stream)w, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.None;
            xmlSerializer.Serialize((XmlWriter)xmlTextWriter, (object)myObject);
            w.Position = 0L;
            StringBuilder stringBuilder = new StringBuilder();
            using (StreamReader streamReader = new StreamReader((Stream)w, Encoding.UTF8))
            {
                string str;
                while ((str = streamReader.ReadLine()) != null)
                    stringBuilder.Append(str);
                streamReader.Close();
            }
            xmlTextWriter.Close();
            return stringBuilder.ToString();
        }

        public static T DeserializeToObject<T>(string xml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(xml);
            T obj = (T)xmlSerializer.Deserialize((TextReader)stringReader);
            stringReader.Close();
            return obj;
        }

        public static string GetXmlContent(string xmlPath, Encoding encode)
        {
            if (encode == null)
                encode = Encoding.UTF8;
            StreamReader streamReader = (StreamReader)null;
            FileStream fileStream = (FileStream)null;
            string xmlContent = "";
            try
            {
                fileStream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                streamReader = new StreamReader((Stream)fileStream, encode);
                xmlContent = streamReader.ReadToEnd();
            }
            finally
            {
                fileStream?.Close();
                streamReader?.Close();
            }
            return xmlContent;
        }

        public static XmlDocument ToXmlDocument(string xmlContent)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(xmlContent);
            return xmlDocument;
        }

        public static string XmlToString(XmlDocument xmlDoc)
        {
            MemoryStream w1 = (MemoryStream)null;
            string str = (string)null;
            StreamReader streamReader = (StreamReader)null;
            try
            {
                w1 = new MemoryStream();
                XmlTextWriter w2 = new XmlTextWriter((Stream)w1, (Encoding)null)
                {
                    Formatting = Formatting.Indented
                };
                xmlDoc.Save((XmlWriter)w2);
                streamReader = new StreamReader((Stream)w1, Encoding.UTF8);
                w1.Position = 0L;
                str = streamReader.ReadToEnd();
            }
            finally
            {
                streamReader?.Close();
                w1?.Close();
            }
            return str;
        }

        public static string SignXmlDoc(string xmlKeyPair, XmlDocument xmlDoc, string prefix)
        {
            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
            cryptoServiceProvider.FromXmlString(xmlKeyPair);
            SignedXml signedXml = new SignedXml(xmlDoc);
            signedXml.SigningKey = (AsymmetricAlgorithm)cryptoServiceProvider;
            signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
            Reference reference = new Reference("");
            reference.AddTransform((Transform)new XmlDsigEnvelopedSignatureTransform(false));
            signedXml.AddReference(reference);
            signedXml.ComputeSignature();
            XmlElement xml = signedXml.GetXml();
            if (prefix != null && prefix != "")
                xml.Prefix = prefix;
            xmlDoc.DocumentElement.AppendChild((XmlNode)xml);
            return xmlDoc.OuterXml;
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

        public static string FilterXmlErrorCode(string xml)
        {
            foreach (char character in xml)
            {
                if (!XmlHelp.IsLegalXmlChar((int)character))
                    xml = xml.Replace(character.ToString(), "");
            }
            return xml;
        }

        private static bool IsLegalXmlChar(int character) => character == 9 || character == 10 || character == 13 || character >= 32 && character <= 55295 || character >= 57344 && character <= 65533 || character >= 65536 && character <= 1114111;

        public static byte[] AsByteArray(XmlDocument xmlDoc)
        {
            using (StringWriter w1 = new StringWriter())
            {
                using (XmlTextWriter w2 = new XmlTextWriter((TextWriter)w1))
                {
                    xmlDoc.WriteTo((XmlWriter)w2);
                    return Encoding.UTF8.GetBytes(w1.ToString());
                }
            }
        }
    }
}

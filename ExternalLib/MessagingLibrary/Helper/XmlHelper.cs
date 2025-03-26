using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System;

namespace MessagingLibrary.Helper
{
    /*
	 * XmlHelper 클래스에서 XML을 저장하는 방법은 **객체 직렬화(Serialization)**를 이용하여 객체를 XML로 변환한 후 파일로 저장하는 방식입니다.
이를 담당하는 핵심 메서드는 SaveXml<T>(string fileName, T obj) 입니다.

	XmlSerializer는 .NET의 XML 직렬화/역직렬화를 담당하는 클래스입니다.

	직렬화 (Serialization): 객체를 XML 형식으로 변환
	역직렬화 (Deserialization): XML을 객체로 변환

	 */

    public class XmlHelper
    {
        /// <summary>
        /// Loads a XML file from a specified path
        /// </summary>
        /// <param name="path">File path to load</param>
        /// <returns>XML element</returns>
        public static XElement LoadLinqXml(string path)
        {
            XElement doc = null;

            // Make sure existance of the file
            if (File.Exists(path) == false)
            {
                throw new Exception("No xml file found.");
            }

            try
            {
                using (StreamReader xr = new StreamReader(path))
                {
                    doc = XElement.Load(xr);
                }
            }
            catch (FileNotFoundException ffe)
            {
                throw ffe;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                throw dnfe;
            }
            catch (IOException ioe)
            {
                throw ioe;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static XmlDocument LoadReadOnlyXml(string path)
        {
            XmlDocument doc;

            if (File.Exists(path) == false)
                throw new Exception("No xml file found.");

            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                doc = new XmlDocument();
                doc.Load(stream);
            }

            return doc;
        }

        /// <summary>
        /// Loads XmlDocument from a xml string
        /// </summary>
        /// <param name="xml">xml string</param>
        /// <returns></returns>
        public static XmlDocument LoadXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc;
        }

        /// <summary>
        /// Converts the content of XmlDocument to string
        /// </summary>
        /// <param name="doc">XmlDocument to convert</param>
        /// <returns>String content of XmlDocument</returns>
        public static string XmlToString(XmlDocument doc)
        {
            string txt = null;

            if (doc == null)
                return null;

            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter xtw = new XmlTextWriter(sw))
                {
                    doc.WriteTo(xtw);
                    txt = sw.ToString();
                }
            }

            return txt;
        }

        /// <summary>
        /// Indicates whether a node has a specified attribute.
        /// </summary>
        /// <param name="xelmt"></param>
        /// <param name="attrName"></param>
        /// <returns></returns>
        public static bool HasAttribute(XElement xelmt, string attrName)
        {
            if (xelmt.Attribute(attrName) != null)
                return true;

            return false;
        }

        /// <summary>
        /// Indicates whether a node contains a specified node.
        /// </summary>
        /// <param name="xelmt"></param>
        /// <param name="elmtName"></param>
        /// <returns></returns>
        public static bool HasElement(XElement xelmt, string elmtName)
        {
            if (xelmt.Element(elmtName) != null)
                return true;

            return false;
        }

        #region Serialization

        /// <summary>
        /// Loads serialized xml file as an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>

        public static T LoadXml<T>(string fileName)
        {
            // In case of non-existence of the file, create a new file
            if (File.Exists(fileName) == false)
                SaveXml<T>(fileName, (T)Activator.CreateInstance(typeof(T)));

            // In case of a zero size file, delete it first and recreate it
            FileInfo file = new FileInfo(fileName);
            if (file.Length <= 0)
            {
                file.Delete();
                SaveXml<T>(fileName, (T)Activator.CreateInstance(typeof(T)));
            }

            //LoadXML<T>는 **XML 파일 → 객체 변환 (역직렬화)**하여 데이터를 복원
            XmlSerializer deserializer = XmlSerializer.FromTypes(new[] { typeof(T) })[0];
            // Restore data from a XML document
            //System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return (T)deserializer.Deserialize(stream);
                //return (T)serializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// Saves an object to a serialized xml file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="obj"></param>
        public static void SaveXml<T>(string fileName, T obj)
        {
            //XmlSerializer를 사용하여 객체 → XML 변환(직렬화) 후, FileStream을 통해 파일로 저장

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                try
                {
                    //	XmlSerializer serializer = new XmlSerializer(obj.GetType());
                    XmlSerializer serializer = XmlSerializer.FromTypes(new[] { typeof(T) })[0];
                    serializer.Serialize(stream, obj);
                }
                catch
                {
                    throw;
                }
            }
        }
        #endregion

        /// <summary>
        /// 객체를 Linq XML문서형태로 변환하여 반환한다.
        /// </summary>
        /// <typeparam name="T">변환하고 자하는 객체의 타입</typeparam>
        /// <param name="obj">변환하고 자하는 객체</param>
        /// <returns>Linq XML문서</returns>
        public static XDocument ObjectToLinqXmlDoc<T>(T obj)
        {
            string xmlString = ObjectToXmlString<T>(obj);

            return XDocument.Parse(xmlString);
        }

        /// <summary>
        /// 객체를 XML형태로 변환하여 반환한다.
        /// </summary>
        /// <typeparam name="T">변환하고 자하는 객체의 타입</typeparam>
        /// <param name="obj">변환하고 자하는 객체</param>
        /// <returns>XML</returns>
        public static string ObjectToXmlString<T>(T obj)
        {
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(obj.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, obj, emptyNamespaces);
                return stream.ToString();
            }
            // 			string xmlString = null;
            // 
            // 			XmlSerializer serializer = XmlSerializer.FromTypes(new[] { typeof(T) })[0];
            // 			//XmlSerializer serializer = new XmlSerializer(typeof(T));
            // 			using (StringWriter writer = new StringWriter())
            // 			{
            // 				serializer.Serialize(writer, obj);
            // 				writer.Close();
            // 
            // 				xmlString = writer.ToString();
            // 			}
            // 
            // 			return xmlString;
        }

        /// <summary>
        /// XML 이 주어진 스키마 규칙에 부합하는 지 여부를 판단한다.
        /// </summary>
        /// <param name="xmlString">XML</param>
        /// <param name="schemaString">스키마</param>
        /// <param name="targetNamespace">XML 네임스페이스</param>
        /// <returns>부합하면 <c>true</c>, 그렇지 않으면 <c>false</c></returns>
        public static bool ValidateXml(string xmlString, string schemaString, string targetNamespace = null)
        {
            bool isValid = true;

            var stringReader = new StringReader(schemaString);
            XmlReader xmlReader = XmlReader.Create(stringReader);

            var schemas = new XmlSchemaSet();
            schemas.Add("tempuri", xmlReader);

            var xmlDoc = XDocument.Parse(xmlString);

            xmlDoc.Validate(schemas, (s, e) =>
            {
                isValid = false;
            });

            return isValid;
        }

        public static T XmlDeserialize<T>(string toDeserialize)
        {
            XmlSerializer xmlSerializer = XmlSerializerFactory.GetSerializer<T>();
            using (StringReader textReader = new StringReader(toDeserialize))
            {
                return (T)xmlSerializer.Deserialize(textReader);
            }

            //XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            //using (StringReader textReader = new StringReader(toDeserialize))
            //{
            //	return (T)xmlSerializer.Deserialize(textReader);
            //}

            //XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(T) })[0];
            //using (StringReader textReader = new StringReader(toDeserialize))
            //{
            //	return (T)xmlSerializer.Deserialize(textReader);
            //}
        }
        public static class XmlSerializerFactory
        {
            private static readonly Dictionary<Type, XmlSerializer> cache = new Dictionary<Type, XmlSerializer>();

            public static XmlSerializer GetSerializer<T>()
            {
                Type type = typeof(T);
                if (!cache.ContainsKey(type))
                {
                    cache[type] = new XmlSerializer(type);
                }

                return cache[type];
            }
        }
    }
}

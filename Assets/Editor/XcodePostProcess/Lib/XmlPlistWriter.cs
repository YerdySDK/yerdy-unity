using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Text;

namespace XcodePostProcessLib
{
	public class XmlPlistWriter
	{
		public static void Write(PlistObject plistObject, Stream stream)
		{
			// XmlWriter writes out the encoding as 'utf-8' in the XML declaration, which Xcode
			// barfs on :(  So, we write out the declaration ourselves (and tell XmlWriter to skip
			// the declaration)
			using (StreamWriter streamWriter = new StreamWriter(stream, new UTF8Encoding()))
			{
				streamWriter.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

				XmlWriterSettings settings = new XmlWriterSettings();
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				settings.OmitXmlDeclaration = true;

				using (XmlWriter xmlWriter = XmlWriter.Create(streamWriter)) {
					XmlPlistWriter writer = new XmlPlistWriter(xmlWriter);
					writer.WritePlistDoc(plistObject);
				}
			}
		}


		private XmlWriter xml;

		private XmlPlistWriter(XmlWriter writer)
		{
			xml = writer;
		}

		private void WritePlistDoc(PlistObject rootObject)
		{
			xml.WriteDocType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);

			xml.WriteStartElement("plist");
			xml.WriteAttributeString("version", "1.0");
			WriteObject(rootObject);
			xml.WriteEndElement();
		}

		private void WriteObject(PlistObject plistObject)
		{
			if (plistObject.Raw is List<PlistObject>) {
				WriteArray(plistObject.Array);
			} else if (plistObject.Raw is Dictionary<string, PlistObject>) {
				WriteDictionary(plistObject.Dict);
			} else if (plistObject.Raw is string) {
				xml.WriteElementString("string", plistObject.String);
			} else if (plistObject.Raw is float) {
				xml.WriteElementString("real", plistObject.Real.ToString("r"));
			} else if (plistObject.Raw is int) {
				xml.WriteElementString("integer", plistObject.Integer.ToString());
			} else if (plistObject.Raw is DateTime) {
				string dateStr = plistObject.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", DateTimeFormatInfo.InvariantInfo);
				xml.WriteElementString("date", dateStr);
			} else if (plistObject.Raw is bool) {
				if (plistObject.Bool) {
					xml.WriteStartElement("true"); 
					xml.WriteEndElement();
				} else {
					xml.WriteStartElement("false"); 
					xml.WriteEndElement();
				}
			} else if (plistObject.Raw is byte[]) {
				string base64 = Convert.ToBase64String(plistObject.Data);
				xml.WriteElementString("data", base64);
			}
		}

		private void WriteArray(List<PlistObject> array)
		{
			xml.WriteStartElement("array");

			foreach (PlistObject plistObject in array) {
				WriteObject(plistObject);
			}

			xml.WriteEndElement();
		}

		private void WriteDictionary(Dictionary<string, PlistObject> dict)
		{
			xml.WriteStartElement("dict");
			
			foreach (KeyValuePair<string, PlistObject> pair in dict) {
				xml.WriteElementString("key", pair.Key);
				WriteObject(pair.Value);
			}
			
			xml.WriteEndElement();
		}
	}
}

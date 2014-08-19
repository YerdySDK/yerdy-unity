using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace XcodePostProcessLib
{
	public class XmlPlistParser
	{
		/// <summary>
		/// Parses the XML plist at the specified path.
		/// </summary>
		public static PlistObject Parse(string path)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(path);
			return ParseDocument(xmlDoc);
		}
		
		/// <summary>
		/// Parses the provided XML plist string.
		/// </summary>
		public static PlistObject ParseString(string xml)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			return ParseDocument(xmlDoc);
		}
		
		private static PlistObject ParseDocument(XmlDocument xmlDoc)
		{
			if (xmlDoc.DocumentElement.Name != "plist")
				throw new Exception("Invalid XML plist, root element isn't <plist>");
			
			XmlNodeList children = xmlDoc.DocumentElement.SelectNodes("*");
			
			if (children.Count == 0)
				return null; // no object in plist, just return null
			else if (children.Count != 1)
				throw new Exception("Invalid XML plist, <plist> must have only 1 child");
			
			
			XmlNode child = children[0];
			if (child.Name == "array" || child.Name == "dict")
				return ProcessNode(child);
			else
				throw new Exception("Invalid XML plist, <plist> must have a <dict> or <array> child");
		}
		
		private static PlistObject ProcessNode(XmlNode node)
		{
			if (node.Name == "array")
				return ProcessArray(node);
			else if (node.Name == "dict")
				return ProcessDict(node);
			else if (node.Name == "string")
				return ProcessLeaf(node, n => n.InnerText);
			else if (node.Name == "real")
				return ProcessLeaf(node, n => float.Parse(n.InnerText, CultureInfo.InvariantCulture));
			else if (node.Name == "integer")
				return ProcessLeaf(node, n => int.Parse(n.InnerText, CultureInfo.InvariantCulture));
			else if (node.Name == "date")
				return ProcessLeaf(node, n => DateTime.Parse(n.InnerText, CultureInfo.InvariantCulture));
			else if (node.Name == "true")
				return ProcessLeaf(node, n => true);
			else if (node.Name == "false")
				return ProcessLeaf(node, n => false);
			else if (node.Name == "data")
				return ProcessLeaf(node, n => Convert.FromBase64String(n.InnerText));
			else if (node.Name == "key")
				throw new Exception("Invalid XML plist, encountered unexpected <key>");
			else
				throw new Exception("Invalid XML plist, invalid tag: <" + node.Name + ">");
		}
		
		private static PlistObject ProcessArray(XmlNode array)
		{
			List<PlistObject> retVal = new List<PlistObject>();
			
			foreach (XmlNode child in array.SelectNodes("*"))
				retVal.Add(ProcessNode(child));
			
			return new PlistObject(retVal);
		}
		
		private static PlistObject ProcessDict(XmlNode dict)
		{
			// each key-value is a pair of <key>/<____> nodes, so divide by 2
			Dictionary<string, PlistObject> retVal = new Dictionary<string, PlistObject>(dict.SelectNodes("*").Count / 2);
			
			IEnumerator childEnumerator = dict.SelectNodes("*").GetEnumerator();
			while (childEnumerator.MoveNext())
			{
				XmlNode keyNode = (XmlNode)childEnumerator.Current;
				if (keyNode.Name != "key")
					throw new Exception("Invalid XML plist, expecting <key>, was <" + keyNode.Name + ">");
				
				string key = ProcessLeaf(keyNode, n => n.InnerText).String;
				
				if (!childEnumerator.MoveNext())
					throw new Exception("Invalid XML plist, <key> doesn't have matching value tag");
				
				retVal[key] = ProcessNode((XmlNode)childEnumerator.Current);
			}
			
			return new PlistObject(retVal);
		}
		
		private static PlistObject ProcessLeaf<T>(XmlNode leaf, Func<XmlNode, T> conversionFunc)
		{
			if (leaf.SelectNodes("*").Count > 0)
				throw new Exception("Invalid XML plist, <" + leaf.Name + "> must not have child elements");
			return new PlistObject(conversionFunc(leaf));
		}

	}
}
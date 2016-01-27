﻿using System.IO;
using NUnit.Framework;
using PListNet.Nodes;
using System.Text;

namespace PListNet.Tests
{
	[TestFixture]
	public class XmlWriterTests
	{
		[Test]
		public void WhenXmlFormatIsResavedAndOpened_ThenParsedDocumentMatchesTheOriginal()
		{
			using (var stream = TestFileHelper.GetTestFileStream("TestFiles/utf8-Info.plist"))
			{
				// test for <ustring> elements
				bool containsUStrings;
				using (var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
				{
					var text = reader.ReadToEnd();
					containsUStrings = text.Contains("<ustring>");
					stream.Seek(0, SeekOrigin.Begin);
				}

				var node = PList.Load(stream);

				using (var outStream = new MemoryStream())
				{
					PList.Save(node, outStream, PListFormat.Xml);

					// rewind and reload
					outStream.Seek(0, SeekOrigin.Begin);
					var newNode = PList.Load(outStream);

					// compare
					Assert.AreEqual(node.GetType().Name, newNode.GetType().Name);

					var oldDict = node as DictionaryNode;
					var newDict = newNode as DictionaryNode;

					Assert.AreEqual(oldDict.Count, newDict.Count);

					foreach (var key in oldDict.Keys)
					{
						Assert.IsTrue(newDict.ContainsKey(key));

						var oldValue = oldDict[key];
						var newValue = newDict[key];

						Assert.AreEqual(oldValue.GetType().Name, newValue.GetType().Name);
						Assert.AreEqual(oldValue, newValue);
					}

					// lastly, confirm <ustring> contents have not changed
					outStream.Seek(0, SeekOrigin.Begin);
					using (var reader = new StreamReader(outStream))
					{
						var text = reader.ReadToEnd();
						var outContainsUStrings = text.Contains("<ustring>");

						Assert.AreEqual(containsUStrings, outContainsUStrings);
					}
				}
			}
		}
	}
}

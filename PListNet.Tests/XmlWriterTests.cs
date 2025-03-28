﻿using System.Text;
using PListNet.Nodes;

namespace PListNet.Tests;

public class XmlWriterTests
{
	[Fact]
	public void WhenXmlFormatIsSavedAndOpened_ThenParsedDocumentMatchesTheOriginal()
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
				Assert.Equal(node.GetType().Name, newNode.GetType().Name);

				var oldDict = node as DictionaryNode;
				var newDict = newNode as DictionaryNode;

				Assert.NotNull(oldDict);
				Assert.NotNull(newDict);
				Assert.Equal(oldDict.Count, newDict.Count);

				foreach (var key in oldDict.Keys)
				{
					Assert.True(newDict.ContainsKey(key));

					var oldValue = oldDict[key];
					var newValue = newDict[key];

					Assert.Equal(oldValue.GetType().Name, newValue.GetType().Name);
					Assert.Equal(oldValue, newValue);
				}

				// lastly, confirm <ustring> contents have not changed
				outStream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(outStream))
				{
					var text = reader.ReadToEnd();
					var outContainsUStrings = text.Contains("<ustring>");

					Assert.Equal(containsUStrings, outContainsUStrings);
				}
			}
		}
	}

	[Fact]
	public void WhenBooleanValueIsSaved_ThenThereIsNoWhiteSpace()
	{
		using (var outStream = new MemoryStream())
		{
			// create basic PList containing a boolean value
			var node = new DictionaryNode { { "Test", new BooleanNode(true) } };

			// save and reset stream
			PList.Save(node, outStream, PListFormat.Xml);
			outStream.Seek(0, SeekOrigin.Begin);

			// check that boolean was written out without a space per spec (see also issue #11)
			using (var reader = new StreamReader(outStream))
			{
				var contents = reader.ReadToEnd();

				Assert.Contains("<true/>", contents);
			}
		}
	}

	[Fact]
	public void WhenXmlIsSaved_ThenDoctypeIsWritten()
	{
		using (var outStream = new MemoryStream())
		{
			// create basic PList containing a string value
			var node = new DictionaryNode { { "Test", new StringNode("value") } };

			// save and reset stream
			PList.Save(node, outStream, PListFormat.Xml);
			outStream.Seek(0, SeekOrigin.Begin);

			// check that the doctype is written
			using (var reader = new StreamReader(outStream))
			{
				var contents = reader.ReadToEnd();

				Assert.Contains(@"<!DOCTYPE plist PUBLIC ""-//Apple Computer//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">", contents);
			}
		}
	}

	[Fact]
	public void WhenDisabled_ThenDoctypeIsNotWritten()
	{
		using (var outStream = new MemoryStream())
		{
			// create basic PList containing a string value
			var node = new DictionaryNode { { "Test", new StringNode("value") } };

			// save and reset stream
			var options = new PListOptions(PListFormat.Xml, WriteDocType: false);
			PList.Save(node, outStream, options);
			outStream.Seek(0, SeekOrigin.Begin);

			// check that the doctype is written
			using (var reader = new StreamReader(outStream))
			{
				var contents = reader.ReadToEnd();

				Assert.DoesNotContain(@"<!DOCTYPE", contents);
			}
		}
	}

	[Fact]
	public void WhenXmlPlistWithBooleanValueIsLoadedAndSaved_ThenWhiteSpaceMatches()
	{
		using (var stream = TestFileHelper.GetTestFileStream("TestFiles/github-20.plist"))
		{
			// read in the source file and reset the stream so we can parse from it
			string source;
			using (var reader = new StreamReader(stream, Encoding.Default, true, 2048, true))
			{
				source = reader.ReadToEnd();
				// Replace the line endings from the source file with the Apple standard line ending
				source = source.Replace("\r\n", "\n");
			}
			stream.Seek(0, SeekOrigin.Begin);

			var root = PList.Load(stream) as DictionaryNode;
			Assert.NotNull(root);

			// verify that we parsed expected content
			var node = root["ABool"] as BooleanNode;
			Assert.NotNull(node);
			Assert.True(node.Value);

			// write the file out to memory and check that there is still no space
			// in the written out boolean node
			using (var outStream = new MemoryStream())
			{
				// save and reset stream
				PList.Save(root, outStream, PListFormat.Xml);
				outStream.Seek(0, SeekOrigin.Begin);

				// check that boolean was written out without a space per spec (see also issue #11)
				using (var reader = new StreamReader(outStream))
				{
					var contents = reader.ReadToEnd();

					Assert.Equal(source, contents);
				}
			}

		}
	}

	[Fact]
	public void WhenStringContainsUnicode_ThenStringIsWrappedInUstringTag()
	{
		using (var outStream = new MemoryStream())
		{
			var utf16value = "😂test";

			// create basic PList containing a boolean value
			var node = new DictionaryNode { { "Test", new StringNode(utf16value) } };

			// save and reset stream
			PList.Save(node, outStream, PListFormat.Xml);
			outStream.Seek(0, SeekOrigin.Begin);

			// check that boolean was written out without a space per spec (see also issue #11)
			using (var reader = new StreamReader(outStream))
			{
				var contents = reader.ReadToEnd();

				Assert.Contains($"<ustring>{utf16value}</ustring>", contents);
			}
		}
	}
}

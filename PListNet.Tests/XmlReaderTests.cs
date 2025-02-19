using PListNet.Nodes;

namespace PListNet.Tests;

public class XmlReaderTests
{
	[Fact]
	public void WhenParsingXmlDocumentWithSingleDictionary_ThenItIsParsedCorrectly()
	{
		using (var stream = TestFileHelper.GetTestFileStream("TestFiles/asdf-Info.plist"))
		{
			var node = PList.Load(stream);

			Assert.NotNull(node);

			var dictionary = node as DictionaryNode;
			Assert.NotNull(dictionary);

			Assert.Equal(14, dictionary.Count);
		}
	}

	[Fact]
	public void WhenDocumentContainsNestedCollections_ThenDocumentIsParsedCorrectly()
	{
		using (var stream = TestFileHelper.GetTestFileStream("TestFiles/dict-inside-array.plist"))
		{
			var node = PList.Load(stream);

			Assert.NotNull(node);
			Assert.IsType<DictionaryNode>(node);

			var array = ((DictionaryNode)node).Values.First() as ArrayNode;
			Assert.NotNull(array);
			Assert.Single(array);

			var dictionary = array[0] as DictionaryNode;
			Assert.NotNull(dictionary);

			Assert.Equal(4, dictionary.Count);
		}
	}

	[Fact]
	public void WhenDocumentContainsNestedCollectionsAndComplexText_ThenDocumentIsParsedCorrectly()
	{
		using (var stream = TestFileHelper.GetTestFileStream("TestFiles/Pods-acknowledgements.plist"))
		{
			var root = PList.Load(stream) as DictionaryNode;

			Assert.NotNull(root);
			Assert.Equal(3, root.Count);

			Assert.IsType<StringNode>(root["StringsTable"]);
			Assert.IsType<StringNode>(root["Title"]);

			var array = root["PreferenceSpecifiers"] as ArrayNode;
			Assert.NotNull(array);
			Assert.Equal(15, array.Count);

			foreach (var node in array)
			{
				Assert.IsType<DictionaryNode>(node);

				var dictionary = (DictionaryNode)node;
				Assert.Equal(3, dictionary.Count);
			}
		}
	}

	[Fact]
	public void WhenDocumentContainsEmptyArray_ThenDocumentIsParsedCorrectly()
	{
		using (var stream = TestFileHelper.GetTestFileStream("TestFiles/empty-array.plist"))
		{
			var root = PList.Load(stream) as DictionaryNode;

			Assert.NotNull(root);
			Assert.Single(root);

			Assert.IsType<DictionaryNode>(root["Entitlements"]);
			var dict = root["Entitlements"] as DictionaryNode;

			var array = dict?["com.apple.developer.icloud-container-identifiers"] as ArrayNode;
			Assert.NotNull(array);
			Assert.Empty(array);
		}
	}

	[Fact]
	public void WhenReadingUid_UidNodeIsParsed()
	{
		using (var stream = TestFileHelper.GetTestFileStream("TestFiles/github-7-xml.plist"))
		{
			try
			{
				var node = PList.Load(stream);
				Assert.True(true);
			}
			catch (PListFormatException ex)
			{
				Assert.Fail(ex.Message);
			}
		}
	}
}

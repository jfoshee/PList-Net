using PListNet.Internal;
using PListNet.Nodes;

namespace PListNet.Tests;

class CustomRootNode : DictionaryNode
{
	// The top level element will be written as <Custom> instead of <dict>
	public override string XmlTag => "Custom";
	// The inner keys in this dictionary will be written as <k> tags
	// instead of the default <key> tags
	public override string XmlKeyTag => "k";
	public override byte BinaryTag => 199;
}

class ShortIntNode(int value) : IntegerNode(value)
{
	public override string XmlTag => "i";
	public override byte BinaryTag => 200;
	public ShortIntNode() : this(default) { }
}

class ShortRealNode(double value) : RealNode(value)
{
	public override string XmlTag => "r";
	public override byte BinaryTag => 201;
	public ShortRealNode() : this(default) { }
}

class ShortStringNode(string value) : StringNode(value)
{
	public override string XmlTag => "s";
	public override byte BinaryTag => 202;
	public ShortStringNode() : this("") { }
}

public sealed class CustomTagsTests : IDisposable
{
	[Fact(DisplayName = "Writing custom tags")]
	public void Writing()
	{
		var root = new DictionaryNode
		{
			["int-value"] = new ShortIntNode(42),
			["real-value"] = new ShortRealNode(3.14),
			["string-value"] = new ShortStringNode("Hello, World!")
		};
		using var stream = new MemoryStream();

		PList.Save(root, stream, PListFormat.Xml);

		stream.Seek(0, SeekOrigin.Begin);
		using var reader = new StreamReader(stream);
		var contents = reader.ReadToEnd();
		var expected = @"
		<key>int-value</key>
		<i>42</i>
		<key>real-value</key>
		<r>3.14</r>
		<key>string-value</key>
		<s>Hello, World!</s>
		".Replace("\r\n", "\n").Trim();
		Assert.Contains(expected, contents);
	}

	[Fact(DisplayName = "Reading custom tags (round trip)")]
	public void Reading()
	{
		// Register the nodes in the factory
		NodeFactory.Register(new ShortIntNode());
		NodeFactory.Register(new ShortRealNode());
		NodeFactory.Register(new ShortStringNode());

		// Write the PList
		var saved = new DictionaryNode
		{
			["int-value"] = new ShortIntNode(42),
			["real-value"] = new ShortRealNode(3.14),
			["string-value"] = new ShortStringNode("Hello, World!")
		};
		using var stream = new MemoryStream();
		PList.Save(saved, stream, PListFormat.Xml);

		// Act: Read the PList
		stream.Seek(0, SeekOrigin.Begin);
		var loaded = PList.Load(stream) as DictionaryNode;

		// Assert: The loaded PList is the same as the saved PList
		Assert.NotNull(loaded);
		Assert.Equal(saved, loaded);
	}

	[Fact(DisplayName = "Writing custom root node key tags")]
	public void WritingCustomRootNode()
	{
		var root = new CustomRootNode
		{
			["int-value"] = new ShortIntNode(42),
			["real-value"] = new ShortRealNode(3.14),
			["string-value"] = new ShortStringNode("Hello, World!")
		};
		using var stream = new MemoryStream();

		PList.Save(root, stream, PListFormat.Xml);

		stream.Seek(0, SeekOrigin.Begin);
		using var reader = new StreamReader(stream);
		var contents = reader.ReadToEnd();
		var expected = @"
		<k>int-value</k>
		<i>42</i>
		<k>real-value</k>
		<r>3.14</r>
		<k>string-value</k>
		<s>Hello, World!</s>
		".Replace("\r\n", "\n").Trim();
		Assert.Contains(expected, contents);
	}

	[Fact(DisplayName = "Reading custom root node key tags (round trip)")]
	public void ReadingCustomRootNode()
	{
		// Register the nodes in the factory
		NodeFactory.Register(new CustomRootNode());
		NodeFactory.Register(new ShortIntNode());
		NodeFactory.Register(new ShortRealNode());
		NodeFactory.Register(new ShortStringNode());

		// Write the PList
		var saved = new CustomRootNode
		{
			["int-value"] = new ShortIntNode(42),
			["real-value"] = new ShortRealNode(3.14),
			["string-value"] = new ShortStringNode("Hello, World!")
		};
		using var stream = new MemoryStream();
		PList.Save(saved, stream, PListFormat.Xml);

		// Act: Read the PList
		stream.Seek(0, SeekOrigin.Begin);
		var loaded = PList.Load(stream) as CustomRootNode;

		// Assert: The loaded PList is the same as the saved PList
		Assert.NotNull(loaded);
		Assert.Equal(saved, loaded);
	}

	public void Dispose()
	{
		// Cleanup: Reset the Node factory
		// so it does not have the custom tags for other tests
		NodeFactory.Reset();
	}
}

using PListNet.Internal;
using PListNet.Nodes;

namespace PListNet.Tests;

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

public class CustomTagsTests
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

		// Cleanup: Reset the factory
		NodeFactory.Reset();
	}
}

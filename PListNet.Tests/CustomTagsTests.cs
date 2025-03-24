using PListNet.Nodes;

namespace PListNet.Tests;

class ShortIntNode(int value) : IntegerNode(value)
{
	public override string XmlTag => "i";
}

class ShortRealNode(double value) : RealNode(value)
{
	public override string XmlTag => "r";
}

class ShortStringNode(string value) : StringNode(value)
{
	public override string XmlTag => "s";
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
}

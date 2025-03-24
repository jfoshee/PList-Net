using PListNet.Nodes;

namespace PListNet.Internal;

/// <summary>
/// Singleton class which generates concrete <see cref="T:PListNet.PNode"/> from the Tag or TypeCode
/// </summary>
public static class NodeFactory
{
	private static readonly Dictionary<string, Type> _xmlTags = new();
	private static readonly Dictionary<byte, Type> _binaryTags = new();
	private static readonly object _lock = new();

	/// <summary>
	/// Initializes the <see cref="NodeFactory"/> class.
	/// </summary>
	static NodeFactory()
	{
		Reset();
	}

	/// <summary>
	/// Resets the factory to default tags.
	/// </summary>
	internal static void Reset()
	{
		lock (_lock)
		{
			_xmlTags.Clear();
			_binaryTags.Clear();
			RegisterInternal(new DictionaryNode());
			RegisterInternal(new IntegerNode());
			RegisterInternal(new RealNode());
			RegisterInternal(new StringNode());
			RegisterInternal(new ArrayNode());
			RegisterInternal(new DataNode());
			RegisterInternal(new DateNode());
			RegisterInternal(new UidNode());

			RegisterInternal("string", 5, new StringNode());
			RegisterInternal("ustring", 6, new StringNode());

			RegisterInternal("true", 0, new BooleanNode());
			RegisterInternal("false", 0, new BooleanNode());
		}
	}

	public static void Register<T>(T node) where T : PNode, new()
	{
		lock (_lock)
		{
			RegisterInternal(node.XmlTag, node.BinaryTag, node);
		}
	}

	public static void Register<T>(string xmlTag, byte binaryTag, T node) where T : PNode, new()
	{
		lock (_lock)
		{
			RegisterInternal(xmlTag, binaryTag, node);
		}
	}

	private static void RegisterInternal<T>(T node) where T : PNode, new()
		=> RegisterInternal(node.XmlTag, node.BinaryTag, node);

	private static void RegisterInternal<T>(string xmlTag, byte binaryTag, T node) where T : PNode, new()
	{
		_xmlTags[xmlTag] = node.GetType();
		_binaryTags[binaryTag] = node.GetType();
	}

	public static PNode Create(byte binaryTag, int length)
	{
		lock (_lock)
		{
			if (binaryTag == 0 && length == 0x00) return new NullNode();
			if (binaryTag == 0 && length == 0x0F) return new FillNode();

			if (binaryTag == 6) return new StringNode { IsUtf16 = true };

			if (_binaryTags.TryGetValue(binaryTag, out var type))
				return (PNode)Activator.CreateInstance(type)!;

			throw new PListFormatException($"Unknown node - binary tag {binaryTag}");
		}
	}

	public static PNode Create(string tag)
	{
		lock (_lock)
		{
			if (_xmlTags.TryGetValue(tag, out var type))
				return (PNode)Activator.CreateInstance(type)!;

			throw new PListFormatException($"Unknown node - XML tag \"{tag}\"");
		}
	}

	public static PNode CreateLengthElement(int length)
		=> new IntegerNode(length);

	public static PNode CreateKeyElement(string key)
		=> new StringNode(key);
}

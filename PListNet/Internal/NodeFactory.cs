using PListNet.Nodes;

namespace PListNet.Internal;

/// <summary>
/// Singleton class which generates concrete <see cref="T:PListNet.PNode"/> from the Tag or TypeCode
/// </summary>
public static class NodeFactory
{
	private static readonly Dictionary<string, Type> _xmlTags = new Dictionary<string, Type>();
	private static readonly Dictionary<byte, Type> _binaryTags = new Dictionary<byte, Type>();

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
		_xmlTags.Clear();
		_binaryTags.Clear();
		Register(new DictionaryNode());
		Register(new IntegerNode());
		Register(new RealNode());
		Register(new StringNode());
		Register(new ArrayNode());
		Register(new DataNode());
		Register(new DateNode());
		Register(new UidNode());

		Register("string", 5, new StringNode());
		Register("ustring", 6, new StringNode());

		Register("true", 0, new BooleanNode());
		Register("false", 0, new BooleanNode());
	}

    /// <summary>
    /// Registers the specified element type by its own XmlTag and BinaryTag.
    /// If the tag already exists, it will be overwritten.
    /// </summary>
    /// <typeparam name="T">The node type to register.</typeparam>
    /// <param name="node">An instance of the node used to extract tags.</param>
    public static void Register<T>(T node) where T : PNode, new()
	{
		Register(node.XmlTag, node.BinaryTag, node);
	}

	/// <summary>
	/// Registers the specified element type with custom XML and binary tags.
	/// If the tag already exists, it will be overwritten.
	/// </summary>
	/// <typeparam name="T">The node type to register.</typeparam>
	/// <param name="xmlTag">The XML tag to associate with the node type.</param>
	/// <param name="binaryTag">The binary tag to associate with the node type.</param>
	/// <param name="node">An instance of the node to get the type from.</param>
	public static void Register<T>(string xmlTag, byte binaryTag, T node) where T : PNode, new()
	{
		_xmlTags[xmlTag] = node.GetType();
		_binaryTags[binaryTag] = node.GetType();
	}

	/// <summary>
	/// Creates a concrete <see cref="T:PListNet.PNode"/> object secified specified by it's typecode.
	/// </summary>
	/// <param name="binaryTag">The typecode of the element.</param>
	/// <param name="length">The length of the element 
	/// (required only for <see cref="T:PListNet.Primitives.PListBool"/>, <see cref="T:PListNet.Primitives.PListNull"/>
	/// and <see cref="T:PListNet.Primitives.PListFill"/>).</param>
	/// <returns>The created <see cref="T:PListNet.PNode"/> object</returns>
	public static PNode Create(byte binaryTag, int length)
	{
		if (binaryTag == 0 && length == 0x00) return new NullNode();
		if (binaryTag == 0 && length == 0x0F) return new FillNode();

		if (binaryTag == 6) return new StringNode { IsUtf16 = true };

		if (_binaryTags.ContainsKey(binaryTag))
		{
			return (PNode)Activator.CreateInstance(_binaryTags[binaryTag]);
		}

		throw new PListFormatException($"Unknown node - binary tag {binaryTag}");
	}

	/// <summary>        
	/// Creates a concrete <see cref="T:PListNet.PNode"/> object secified specified by it's tag.
	/// </summary>
	/// <param name="tag">The tag of the element.</param>
	/// <returns>The created <see cref="T:PListNet.PNode"/> object</returns>
	public static PNode Create(string tag)
	{
		if (_xmlTags.ContainsKey(tag))
		{
			return (PNode)Activator.CreateInstance(_xmlTags[tag]);
		}

		throw new PListFormatException($"Unknown node - XML tag \"{tag}\"");
	}

	/// <summary>
	/// Creates a <see cref="T:PListNet.PNode"/> object used for exteded length information.
	/// </summary>
	/// <param name="length">The exteded length information.</param>
	/// <returns>The <see cref="T:PListNet.PNode"/> object used for exteded length information.</returns>
	public static PNode CreateLengthElement(int length)
	{
		return new IntegerNode(length);
	}

	/// <summary>
	/// Creates a <see cref="T:PListNet.PNode"/> object used for dictionary keys.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <returns>The <see cref="T:PListNet.PNode"/> object used for dictionary keys.</returns>
	public static PNode CreateKeyElement(string key)
	{
		return new StringNode(key);
	}
}

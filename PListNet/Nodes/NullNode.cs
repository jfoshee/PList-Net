namespace PListNet.Nodes;

/// <summary>
/// Represents a null element in a PList
/// </summary>
/// <remarks>Is skipped in Xml-Serialization</remarks>
public class NullNode : PNode
{
	/// <summary>
	/// Gets the Xml tag of this element.
	/// </summary>
	/// <value>The Xml tag of this element.</value>
	public override string XmlTag => "null";

	/// <summary>
	/// Gets the binary typecode of this element.
	/// </summary>
	/// <value>The binary typecode of this element.</value>
	public override byte BinaryTag => 0;

	public override int BinaryLength => 0;

	/// <summary>
	/// Gets a value indicating whether this instance is written only once in binary mode.
	/// </summary>
	/// <value>
	/// 	<c>true</c> this instance is written only once in binary mode; otherwise, <c>false</c>.
	/// </value>
	public override bool IsBinaryUnique => false;

	/// <summary>
	/// Reads this element binary from the reader.
	/// </summary>
	public override void ReadBinary(Stream stream, int nodeLength)
	{
		if (nodeLength != 0x00)
		{
			throw new PListFormatException();
		}
	}

	/// <summary>
	/// Writes this element binary to the writer.
	/// </summary>
	public override void WriteBinary(Stream stream)
	{
	}

	/// <summary>
	/// Generates an object from its XML representation.
	/// </summary>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
	public override void ReadXml(System.Xml.XmlReader reader)
	{
		reader.ReadStartElement(XmlTag);
	}

	/// <summary>
	/// Converts an object into its XML representation.
	/// </summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
	public override void WriteXml(System.Xml.XmlWriter writer)
	{
		writer.WriteStartElement(XmlTag);
		writer.WriteEndElement();
	}
}

namespace PListNet;

public record PListOptions(PListFormat PListFormat = PListFormat.Xml,
                           bool WriteDocType = true);

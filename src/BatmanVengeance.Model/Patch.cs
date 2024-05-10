namespace BatmanVengeance.Model;

public class Patch
{
    /// <summary>
    /// cpu architecture
    /// </summary>
    public string? arch { get; set; }

    /// <summary>
    /// rom type
    /// </summary>
    public string? endian { get; set; }

    /// <summary>
    /// outputpath
    /// </summary>
    public string? output { get; set; } //,create

    /// <summary>
    /// defines the offset to insert a command
    /// </summary>
    public string? origin { get; set; }

    /// <summary>
    /// command that specifies insertion of a binary
    /// </summary>
    public string? insert { get; set; }

    /// <summary>
    /// character for the end of text
    /// </summary>
    public string? endText { get; set; }

    /// <summary>
    /// character for the calc the pointer
    /// </summary>
    public string? dbPointer { get; set; }

    public List<PatchLabel>? PathLabels { get; set; }
}
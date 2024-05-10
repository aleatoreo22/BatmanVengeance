namespace BatmanVengeance.Model;

public class Map
{

    /// <summary>
    ///  defines the offset to insert the pointer if needs
    /// </summary>
    public string? origin { get; set; }

    /// <summary>
    /// Start character
    /// </summary>
    public string? character { get; set; }

    /// <summary>
    /// Start hex
    /// </summary>
    public string? hex { get; set; }

    /// <summary>
    /// Number of characters
    /// </summary>
    public int number { get; set; }
}

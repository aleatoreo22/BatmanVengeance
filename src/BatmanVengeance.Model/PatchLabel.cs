namespace BatmanVengeance.Model;

public class PatchLabel
{
    /// <summary>
    /// defines the offset to insert the text
    /// </summary>
    public string? originText { get; set; }

    /// <summary>
    ///  defines the offset to insert the pointer if needs
    /// </summary>
    public string? originPointer { get; set; }

    /// <summary>
    /// labels to identify
    /// </summary>
    public string? label { get; set; }

    /// <summary>
    /// changed text
    /// </summary>
    public string? db { get; set; }

    public List<Map>? Maps { get; set; }
}

namespace BatmanVengeance.Model;

public class PatchLabel
{
    /// <summary>
    /// defines the offset to insert the new text
    /// </summary>
    public string? newOrigin { get; set; }

    /// <summary>
    /// defines the offset of the old text
    /// </summary>
    public string? oldOrigin { get; set; }

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

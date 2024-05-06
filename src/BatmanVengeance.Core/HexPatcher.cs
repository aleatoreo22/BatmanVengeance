using BatmanVengeance.Model;

namespace BatmanVengeance.Core;

public class HexPatcher(Patch patch, string fileOutput)
{
    public Patch Patch { get; set; } = patch;

    public string FileOutput { get; set; } = fileOutput;

    private string WritePatch(string? value, string prefix = "", string sufix = "")
    {
        return string.IsNullOrEmpty(value) ? "" : prefix + value + sufix + "\n";
    }

    public void Run()
    {
        var patchFile = "";

        patchFile += WritePatch(Patch.arch, "arch");
        patchFile += WritePatch(Patch.endian, "endian ");
        patchFile += WritePatch(Patch.output, "output \"", "\" , create");
        patchFile += WritePatch(Patch.origin, "origin ");
        patchFile += WritePatch(Patch.insert, "insert \"", "\"");

        foreach (var item in Patch.PathLabels ?? [])
        {
            patchFile += WritePatch($"{item?.originText?.PadLeft(8, '0')}", "origin $");
            patchFile += WritePatch(item?.label, sufix: ":");
            patchFile += WritePatch(item?.db, "db \"", "\"");
            patchFile += WritePatch(Patch.dwText, "dw $");
        }

        File.WriteAllText(FileOutput, patchFile);
    }
}

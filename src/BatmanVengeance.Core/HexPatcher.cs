using BatmanVengeance.Model;

namespace BatmanVengeance.Core;

public class HexPatcher(Patch patch, string fileOutput, string nextFreePosition, string fileFullPath)
{
    public Patch Patch { get; set; } = patch;

    public string FileOutput { get; set; } = fileOutput;

    public long NextFreePosition { get; set; } = Convert.ToInt64(nextFreePosition, 16);

    public string FileFullPath { get; set; } = fileFullPath;

    private string WritePatch(string? value, string prefix = "", string sufix = "")
    {
        return string.IsNullOrEmpty(value) ? "" : prefix + value + sufix + "\n";
    }

    public void Run()
    {
        var patchFile = "";
        var patchFilePrefix = "";
        patchFilePrefix += WritePatch(Patch.arch, "arch");
        patchFilePrefix += WritePatch(Patch.endian, "endian ");
        patchFilePrefix += WritePatch(Patch.output, "output \"", "\" , create");
        patchFilePrefix += WritePatch(Patch.origin, "origin ");
        patchFilePrefix += WritePatch(Patch.insert, "insert \"", "\"");


        var patchLabels = Patch.PathLabels?.FindAll(x => string.IsNullOrEmpty(x.newOrigin)).ToList();
        Patch?.PathLabels?.RemoveAll(x => string.IsNullOrEmpty(x.newOrigin));
        patchLabels = CalcNextFreeMemory(patchLabels);

        foreach (var item in patchLabels ?? [])
        {
            foreach (var map in item.Maps ?? [])
            {
                patchFile += WritePatch(map.origin, prefix: "origin $");
                patchFile += WritePatch(item.label, prefix: "dl ");
                patchFile += WritePatch(Patch?.dbPointer, prefix: "db ");
            }
            patchFile += WritePatch($"{item?.newOrigin?.PadLeft(8, '0')}", "origin $");
            patchFile += WritePatch(item?.label, sufix: ":");
            patchFile += WritePatch(item?.db, "db \"", "\"");
            patchFile += WritePatch(Patch?.dwText, "dw $");
        }

        foreach (var item in Patch?.PathLabels ?? [])
        {
            patchFile += WritePatch($"{item?.newOrigin?.PadLeft(8, '0')}", "origin $");
            patchFile += WritePatch(item?.label, sufix: ":");
            patchFile += WritePatch(item?.db, "db \"", "\"");
            patchFile += WritePatch(Patch?.dwText, "dw $");
        }
        var table = GetTable();
        foreach (var item in table)
        {
            Console.WriteLine(item.Value);
            if (patchFile.Contains(item.Value))
                patchFile = patchFile.Replace(item.Value, "\"," + item.Key + ",\"");
        }
        File.WriteAllText(FileOutput, patchFilePrefix + patchFile);
    }

    private Dictionary<string, string> GetTable()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("https://raw.githubusercontent.com/aleatoreo22/BatmanVengeance/master/tabela.tbl.txt").Result;
                response.EnsureSuccessStatusCode();
                string conteudo = response.Content.ReadAsStringAsync().Result;
                return conteudo
                    .Split("\n")
                    .ToList()
                    .Distinct()
                    .ToDictionary(x => x.Substring(0, 2), x => x.Substring(3));
            }
            catch (HttpRequestException e)
            {
                Console.Write("An error occurred while loading the file: " + e.Message);
                throw;
            }
        }
    }

    private List<PatchLabel>? CalcNextFreeMemory(List<PatchLabel>? patchLabels)
    {
        if (patchLabels == null)
            return null;
        Dictionary<string, PatchLabel> pointers = [];
        foreach (var item in patchLabels)
        {
            if (string.IsNullOrEmpty(item.oldOrigin))
                continue;
            item.newOrigin = NextFreePosition.ToString("x");
            pointers.Add(CalculateHexPointer(item.oldOrigin), item);
            NextFreePosition += (item.db?.Length ?? 0) + 4;
        }
        ReadNewPointers(ref pointers);
        return pointers.Select(x => x.Value).ToList();
    }

    private string CalculateHexPointer(string position)
    {

        var hexCalc = "08" + position.PadLeft(6, '0');
        List<string> character = [];
        for (int i = 0; i < hexCalc.Length; i++)
        {
            character.Add(hexCalc.Substring(i, 2));
            i++;
        }
        var characterArray = character.ToArray();
        Array.Reverse(characterArray);
        Console.WriteLine(string.Join("", characterArray));
        return string.Join("", characterArray);
    }

    private void ReadNewPointers(ref Dictionary<string, PatchLabel> pointers)
    {
        if (string.IsNullOrEmpty(FileFullPath) || !File.Exists(FileFullPath))
            throw new Exception("File not found");
        using var fileStream = new FileStream(FileFullPath, FileMode.Open);
        int hexIn;
        List<HexInfo> hexInfos = [];
        for (int i = 0; (hexIn = fileStream.ReadByte()) != -1; i++)
        {
            hexInfos.Add(new HexInfo
            {
                Position = (fileStream.Position - 1).ToString("X"),
                ValueHex = string.Format("{0:X2}", hexIn)
            });
            if (hexInfos.Count > 10)
                hexInfos.RemoveAt(0);
            var searchText = string.Join("", hexInfos.Select(x => x.ValueHex));
            foreach (var item in pointers.Keys)
            {
                if (searchText.Contains(item))
                {
                    var position = hexInfos?.Find(x => x.ValueHex == item.Substring(0, 2)).Position;
                    if (pointers[item].Maps == null)
                        pointers[item].Maps = [];
                    pointers[item]?.Maps?.Add(new Map
                    {
                        origin = position,
                    });
                    Console.WriteLine("find!!!");
                    Console.WriteLine(position);
                    hexInfos?.Clear();
                }
            }
        }
        Console.WriteLine("Finish!!!!");
    }
}

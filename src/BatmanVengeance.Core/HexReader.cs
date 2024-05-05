using System.ComponentModel;
using BatmanVengeance.Model;

namespace BatmanVengeance.Core;

public class HexReader
{
    private string FullPath { get; set; }
    private List<string>? WhiteList { get; set; }
    private List<string>? BlackList { get; set; }

    public HexReader(string fullPath, List<string>? whiteList = null, List<string>? blackList = null)
    {
        FullPath = fullPath;
        WhiteList = whiteList?.Select(x => x.ToLower()).ToList();
        BlackList = blackList?.Select(x => x.ToLower()).ToList();
    }

    public List<HexInfo> Read()
    {
        if (string.IsNullOrEmpty(FullPath) || !File.Exists(FullPath))
            throw new Exception("File not found");

        using var fileStream = new FileStream(FullPath, FileMode.Open);
        int hexIn;
        string founded = "";
        List<HexInfo> hexList = [];
        List<HexInfo> hexListReturn = [];
        var ok = false;
        for (int i = 0; (hexIn = fileStream.ReadByte()) != -1; i++)
        {
            var hexStringValue = string.Format("{0:X2}", hexIn);
            var value = char.ConvertFromUtf32(Convert.ToInt32(hexIn));
            if ((!WhiteList?.Contains(hexStringValue.ToLower()) ?? false) ||
                (BlackList?.Contains(hexStringValue.ToLower()) ?? false))
            {
                if (founded == "")
                    continue;
                if (founded.ToLower().Contains("mean"))
                    ok = true;
                if (!ok)
                {
                    founded = "";
                    hexList.Clear();
                    continue;
                }

                hexListReturn.Add(new HexInfo
                {
                    ValueHex = string.Join(" ", hexList.Select(x => x.ValueHex)),
                    Position = hexList.FirstOrDefault()?.Position ?? "",
                    ValueString = founded,
                });
                founded = "";
                hexList.Clear();
                continue;
            }

            founded += value;
            hexList.Add(new HexInfo
            {
                ValueHex = hexStringValue,
                Position = (fileStream.Position - 1).ToString("X"),
            });
        }

        return hexListReturn;
    }
}

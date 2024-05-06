using System.Text.RegularExpressions;
using BatmanVengeance.Model;

namespace BatmanVengeance.Core;

public class HexReader
{
    private string FullPath { get; set; }
    private List<string>? WhiteList { get; set; }
    private List<string>? BlackList { get; set; }
    private int MinLenString { get; set; }
    private List<PositionDelimiter>? PositionDelimiters { get; set; }
    private string removePunctuationRegex = "[^a-zA-Z0-9\\s]";

    public HexReader(string fullPath, int minLenString, List<string>? whiteList = null, List<string>? blackList = null, List<PositionDelimiter>? positionDelimiters = null)
    {
        FullPath = fullPath;
        MinLenString = minLenString;
        WhiteList = whiteList?.Select(x => x.ToLower()).ToList();
        BlackList = blackList?.Select(x => x.ToLower()).ToList();
        PositionDelimiters = positionDelimiters;
    }

    public List<HexInfo> Read()
    {
        if (string.IsNullOrEmpty(FullPath) || !File.Exists(FullPath))
            throw new Exception("File not found");

        using var fileStream = new FileStream(FullPath, FileMode.Open);
        int hexIn;
        string founded = "";
        List<HexInfo> hexList = [];
        List<HexInfo> hexExportInformation = [];
        List<HexInfo> hexListMatch = [];
        for (int i = 0; (hexIn = fileStream.ReadByte()) != -1; i++)
        {

            if (PositionDelimiters == null || !PositionDelimiters.Exists(x => x.InitialPosition <= fileStream.Position &&
            x.EndPosition >= fileStream.Position))
                continue;

            var hexStringValue = string.Format("{0:X2}", hexIn);
            var value = char.ConvertFromUtf32(Convert.ToInt32(hexIn));
            if ((!WhiteList?.Contains(hexStringValue.ToLower()) ?? false) ||
                (BlackList?.Contains(hexStringValue.ToLower()) ?? false))
            {
                if (!(founded == "" || founded.Replace(" ", "").Replace("\n", "").Length <= MinLenString))
                {
                    hexListMatch.Add(new HexInfo
                    {
                        ValueHex = string.Join(" ", hexList.Select(x => x.ValueHex)),
                        Position = hexList.FirstOrDefault()?.Position ?? "",
                        ValueString = founded,
                        Len = System.Text.Encoding.UTF8.GetByteCount(founded),
                    });
                }
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

            if (!hexExportInformation.Exists(x => x.ValueHex == hexStringValue))
            {
                hexExportInformation.Add(new HexInfo
                {
                    ValueHex = hexStringValue,
                    ValueString = value
                });
            }
        }
        File.WriteAllText("text.txt", string.Join("\n", hexExportInformation.Select(x => x.ValueHex + ": " + x.ValueString)));
        Dictionary<string, string> englishDic = [];
        try
        {
            englishDic = GetEnglishDoc();
        }
        catch (Exception)
        {
            return hexListMatch;
        }

        List<List<HexInfo>> packages = [[]];
        for (int i = 0; i < hexListMatch.Count; i += 5000)
        {
            List<HexInfo> package = hexListMatch.Skip(i).Take(5000).ToList();
            packages.Add(package);

        }
        List<HexInfo> hexReturn = [];
        Parallel.ForEach(packages, package =>
        {
            hexReturn.AddRange(TryWords(package, englishDic));
        });
        return hexReturn;
    }

    private List<HexInfo> TryWords(List<HexInfo> hexList, Dictionary<string, string> englishDic)
    {
        List<HexInfo> hexListReturn = [];
        foreach (var item in hexList)
        {
            try
            {
                var words = Regex.Replace(item.ValueString ?? "", removePunctuationRegex, " ").ToLower().Split(" ").ToList();
                words.RemoveAll(x => x == "" || x == " " || x == "\n");
                if (words.Count == 0)
                    continue;
                if (!englishDic.ContainsKey(words[0]))
                    continue;
                if (words.Count > 1 && !englishDic.ContainsKey(words[1]))
                    continue;
                Console.WriteLine(item.ValueString);
                hexListReturn.Add(item);
            }
            catch (Exception) { }
        }
        return hexListReturn;
    }

    private Dictionary<string, string> GetEnglishDoc()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("https://raw.githubusercontent.com/dwyl/english-words/master/words.txt").Result;
                response.EnsureSuccessStatusCode();
                string conteudo = response.Content.ReadAsStringAsync().Result;
                return conteudo.ToLower().Split("\n").ToList().Distinct().ToDictionary(x => x, x => x);
            }
            catch (HttpRequestException e)
            {
                Console.Write("An error occurred while loading the file: " + e.Message);
                throw;
            }
        }
    }
}

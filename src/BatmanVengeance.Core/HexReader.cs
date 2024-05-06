using System.ComponentModel;
using BatmanVengeance.Model;

namespace BatmanVengeance.Core;

public class HexReader
{
    private string FullPath { get; set; }
    private List<string>? WhiteList { get; set; }
    private List<string>? BlackList { get; set; }
    private int MinLenString { get; set; }

    public HexReader(string fullPath, int minLenString, List<string>? whiteList = null, List<string>? blackList = null)
    {
        FullPath = fullPath;
        MinLenString = minLenString;
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
        List<HexInfo> hexExportInformation = [];
        List<HexInfo> hexListMatch = [];
        for (int i = 0; (hexIn = fileStream.ReadByte()) != -1; i++)
        {
            var hexStringValue = string.Format("{0:X2}", hexIn);
            var value = char.ConvertFromUtf32(Convert.ToInt32(hexIn));
            if ((!WhiteList?.Contains(hexStringValue.ToLower()) ?? false) ||
                (BlackList?.Contains(hexStringValue.ToLower()) ?? false))
            {
                if (founded == "" || founded.Length <= MinLenString)
                    continue;
                hexListMatch.Add(new HexInfo
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
        List<string> englishDoc = [];
        try
        {
            englishDoc = GetEnglishDoc();
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
        int cont = 0;
        Console.WriteLine("Total de pacotes: " + packages.Count);
        Parallel.ForEach(packages, package =>
        {
            cont++;
            Console.WriteLine("Pacote Iniciado: " + cont);
            hexReturn.AddRange(TryWords(package, englishDoc));
            Console.WriteLine("Pacote Finalizado: " + cont);
        });
        return hexReturn;
    }

    private List<HexInfo> TryWords(List<HexInfo> hexList, List<string> englishDic)
    {

        List<HexInfo> hexListReturn = [];
        foreach (var item in hexList)
        {
            try
            {
                var words = item.ValueString.Split(" ").ToList();
                words.RemoveAll(x => x == "" || x == " " || x == "\n");
                if (words.Count == 0)
                    continue;
                if (!englishDic.Contains(words[0]))
                    continue;
                if (words.Count > 1 && !englishDic.Contains(words[1]))
                    continue;
                hexListReturn.Add(item);
            }
            catch (Exception) { }
        }
        return hexListReturn;
    }

    private List<string> GetEnglishDoc()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("https://raw.githubusercontent.com/dwyl/english-words/master/words.txt").Result;
                response.EnsureSuccessStatusCode();
                string conteudo = response.Content.ReadAsStringAsync().Result;
                return conteudo.Split("\n").ToList();
            }
            catch (HttpRequestException e)
            {
                Console.Write("An error occurred while loading the file: " + e.Message);
                throw;
            }
        }
    }
}

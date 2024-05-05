Console.WriteLine("Hello, World!\n\n");
var romPath = "";
var find = "";
var keepSearching = false;

var delimitator = new List<string>
{
    "00",
    "2e"
};

#if DEBUG
romPath = "/home/anemonas/Downloads/Batman - Vengeance (USA) (En,Fr,Es) NEW.gba";
find = "mean";
#else
Console.WriteLine("Rom path:");
romPath = Console.ReadLine();
Console.WriteLine("Find:");
romPath = Console.ReadLine();
#endif

if (romPath == null)
    return;
if (!File.Exists(romPath))
    return;

string found = "";

var hexList = new List<HexInfo>();

using (var fs = new FileStream(romPath, FileMode.Open))
{
    int hexIn;
    for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
    {
        var hexStringValue = string.Format("{0:X2}", hexIn);
        var value = char.ConvertFromUtf32(Convert.ToInt32(hexIn));

        if (!keepSearching)
        {

            if (value.ToLower() == find.ToLower().Substring(found.Length, 1))
            {
                found += value;
                hexList.Add(new HexInfo
                {
                    hexValue = hexStringValue,
                    hexPosition = (fs.Position - 1).ToString("X"),
                });
            }
            else
            {
                hexList.Clear();
                found = "";
            }

            if (found.ToLower() == find)
            {
                Console.WriteLine("Found " + found + " in:\n" + string.Join("\n", hexList.Select(x => x.hexPosition)));
                Console.WriteLine("Keep search util the last char? [y,N]");
#if DEBUG
                keepSearching = true;
#else
            keepSearching = Console.Read().ToLower() == "y";
#endif
                if (!keepSearching)
                    break;
            }
        }
        else
        {
            if (delimitator.Contains(hexStringValue.ToLower()))
            {
                Console.WriteLine("Found " + found + " in:\n" + string.Join("\n", hexList.Select(x => x.hexPosition)));
                break;
            }

            found += value;
            hexList.Add(new HexInfo
            {
                hexValue = hexStringValue,
                hexPosition = (fs.Position - 1).ToString("X"),
            });
        }
    }
}

public class HexInfo
{
    public string hexValue { get; set; }
    public string hexPosition { get; set; }
}
using System.Globalization;
using CsvHelper;
using CUE4Parse.Compression;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MoriaDig;
    
internal static class Util
{
    // Dependencies download and initialisation for CUE4Parse
    internal static void IntitaliseCue4Parse()
    {
        string zlibPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ZlibHelper.DLL_NAME);
        ZlibHelper.DownloadDll(zlibPath);
        ZlibHelper.Initialize(zlibPath);
    }

    // Convert CUE4Parse object into Json for easier navigation
    internal static JArray JsonRoundtrip(IEnumerable<UObject> exports)
    {
        string data = JsonConvert.SerializeObject(exports, Formatting.Indented);
        return (JArray)JsonConvert.DeserializeObject(data)!;
    }

    // Game asset file cache
    private static readonly Dictionary<string, JArray> Items = [];
    // Look up a game object by type (Tool) and name (Torch)
    // In majority of the cases this is a lookup from recipe to the resulting object
    internal static JObject GetItemProperty(this DefaultFileProvider provider, string type, string name)
    {
        // Armor file for some reason does not end with "s" in the assets
        string s = type == "Armor" ? type : type + "s";
        // Constructions (AKA buildings) are located in a different folder
        s = s.StartsWith("Construction") ? $"Moria/Content/Tech/Data/Building/DT_{s}" : $"Moria/Content/Tech/Data/Items/DT_{s}";
        if (!Items.TryGetValue(type, out JArray? value))
        {
            value = JsonRoundtrip(provider.LoadAllObjects(s));
            Items[type] = value;
        }
        return (JObject)value[0]["Rows"]!
            .Select(x => (JProperty)x)
            .Single(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            .Value;
    }

    // Since we are putting this into a csv, there is no point to try and preserve types
    // we can simplify things a lot by converting everything to strings (which is the only thing csv supports)
    internal static string StringFromJsonPath(this JObject o, string jsonPath)
    {
        return o.SelectToken(jsonPath)?.Value<string>() ?? "";
    }

    // Get item's name by item id (e.g. Tool.Torch), by getting it from the game asset corresponding to the item type
    internal static string LookupName(this DefaultFileProvider provider, string id)
    {
        int index = id.IndexOf('.');
        string type;
        if (index == -1)
        {
            type = "Construction";
        }
        else
        {
            type = id[..index];
            id = id[(index+1)..];
        }

        return provider.GetItemProperty(type, id).StringFromJsonPath("$.DisplayName.SourceString");
    }

    // Unfortunately CsvHelper does not support writing dictionaries directly,
    // but we can do it easily enough.
    internal static void WriteCsv(List<OrderedDictionary<string, string>> recipes, string filename)
    {
        using StreamWriter writer = new(filename);
        using CsvWriter csv = new(writer, CultureInfo.InvariantCulture);
        bool hasHeaderBeenWritten = false;
        foreach (OrderedDictionary<string, string> row in recipes)
        {
            if (!hasHeaderBeenWritten)
            {
                foreach (KeyValuePair<string, string> pair in row)
                {
                    csv.WriteField(pair.Key);
                }
                hasHeaderBeenWritten = true;
                csv.NextRecord();
            }
            foreach (KeyValuePair<string, string> pair in row)
            {
                csv.WriteField(pair.Value);
            }
            csv.NextRecord();
        }
    }
}
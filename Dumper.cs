using System.Linq.Dynamic.Core;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports;
using Newtonsoft.Json.Linq;

namespace MoriaDig;

public static class Dumper
{
    internal static void DumpRecipesToCsv(DefaultFileProvider provider, string objectPath, List<Mapping> itemsMapping, string outputCsvFilename)
    {
        // Read the object and convert it to json based object model
        IEnumerable<UObject> exports = provider.LoadAllObjects(objectPath);
        JArray c = Util.JsonRoundtrip(exports);
            
        
        // This will accumulate output rows for the csv
        List<OrderedDictionary<string, string>> recipes = [];

        foreach (JToken jToken in c[0]["Rows"]!)
        {
            JProperty row = (JProperty)jToken;
            JObject o = (JObject)row.Value;

            // Apparently both Live and Test appear in the actual game
            if (o.StringFromJsonPath("$.EnabledState") != "ERowEnabledState::Live" &&
                o.StringFromJsonPath("$.EnabledState") != "ERowEnabledState::Test")
            {
                continue; // Skip entries that are not enabled in the game files
            }

            // We will keep columns for each output row in this dictionary
            OrderedDictionary<string, string> r = [];
            recipes.Add(r);

            // for each csv column
            foreach (Mapping mapping in itemsMapping)
            {
                // mark the field empty, if mapping condition is present and is not fulfilled
                if (mapping.Condition == "" || new[] { r }.AsQueryable().Any(mapping.Condition))
                {
                    // Either our current recipe, or the item the recipe produces
                    JObject context = mapping.Linked ? provider.GetItemProperty(r["Type"], r["Id"]) : o;
                    // Get the field data from the assets and add it to the output
                    r.Add(mapping.Field, mapping.Dig(provider, context, mapping.JsonPath));
                }
                else
                {
                    r.Add(mapping.Field, "");
                }
            }
            // There are some items there that do not have a name, they are likely invalid, so exclude them
            if (string.IsNullOrWhiteSpace(r["Name"]))
            {
                recipes.Remove(r);
                continue;
            }
            Console.WriteLine(r["Name"]);
            // Some fields in mapping are for conditions only, we do not want to retain them in the output, so we remove them
            foreach (Mapping mapping in itemsMapping)
            {
                if (mapping.Excluded)
                {
                    r.Remove(mapping.Field);
                }
            }
        }

        recipes = [.. recipes.OrderBy(x => x["Name"])];
        Util.WriteCsv(recipes, outputCsvFilename);
    }
}
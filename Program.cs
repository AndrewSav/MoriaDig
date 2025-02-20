using System.Diagnostics;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;

namespace MoriaDig;

internal class Program
{
    static void Main(string[] args)
    {

        try
        {
            Stopwatch sw = Stopwatch.StartNew();

            // Buildings

            // Show unicode ™ (below) correctly in the console
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(65001);
            
            if (args.Length != 1)
            {
                Console.WriteLine("Example Usage: MoriaDig \"C:\\Program Files (x86)\\steam\\steamapps\\common\\The Lord of the Rings Return to Moria™\\Moria\\Content\\Paks\"");
                return;
            }

            // Download and initialise dependencies if missing
            Util.IntitaliseCue4Parse();

            DefaultFileProvider provider = new(args[0], SearchOption.TopDirectoryOnly, true, new VersionContainer(EGame.GAME_UE4_27));
            provider.Initialize();
            provider.Mount();

            // Item Recipes
            Dumper.DumpRecipesToCsv(provider, "Moria/Content/Tech/Data/Items/DT_ItemRecipes", Mappings.ItemsMapping, "items.csv");

            // Building Recipes
            Dumper.DumpRecipesToCsv(provider, "Moria/Content/Tech/Data/Building/DT_ConstructionRecipes", Mappings.BuildingsMapping, "buildings.csv");

            Console.WriteLine(sw);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString()); 
        }
    }
}
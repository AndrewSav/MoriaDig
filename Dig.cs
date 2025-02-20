using CUE4Parse_Conversion.Textures;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports.Texture;
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace MoriaDig;

// This class represent mapping functions that retrieve different type of fields from the assets
internal static class Dig
{
    private static string ItemSplit(JObject context, string jsonPath, int index)
    {
        return context.StringFromJsonPath(jsonPath).Split(".")[index];
    }

#pragma warning disable IDE0060
    // Get "Tool" part of "Tool.Torch"
    public static string ItemSplit1(DefaultFileProvider provider, JObject context, string jsonPath)
    {
        return ItemSplit(context, jsonPath, 0);
    }

    // Get "Torch" part of "Tool.Torch"
    public static string ItemSplit2(DefaultFileProvider provider, JObject context, string jsonPath)
    {
        return ItemSplit(context, jsonPath, 1);
    }

    // Just get the value as is
    public static string Simple(DefaultFileProvider provider, JObject context, string jsonPath)
    {
        return context.StringFromJsonPath(jsonPath);
    }

    // Concatenate values as comma-separate from an array
    public static string SimpleArray(DefaultFileProvider provider, JObject context, string jsonPath)
    {
        return string.Join(", ", context.SelectTokens(jsonPath).Select(x => x.Value<string>()));
    }

    public static string Constant(DefaultFileProvider provider, JObject context, string constant)
    {
        return constant;
    }
#pragma warning restore IDE0060

    // Lookup object name in the object asset
    public static string NameLookup(DefaultFileProvider provider, JObject context, string jsonPath)
    {
        return string.Join(", ", context.SelectTokens(jsonPath).Select(x => provider.LookupName(x.Value<string>()!)));
    }

    // Parse and produce comma-separate list of materials with quantities in parentheses that follow each
    public static string MaterialsLookup(DefaultFileProvider provider, JObject context, string jsonPath)
    {
        JArray? array = (JArray?)context.SelectToken(jsonPath);
        if (array == null) return "";
        IEnumerable<string> materials = array.Select(x => (JObject)x)
            .Select(x => new
            {
                mats = provider.LookupName(x.StringFromJsonPath("$.MaterialHandle.RowName")),
                count = x.StringFromJsonPath("$.Count")
            })
            .Select(x => $"{x.mats} ({x.count})");
        return string.Join(", ",materials);
    }

    // Returns icon name, and also writes icon to a PNG file in the current folder
    public static string ExtractIcon(DefaultFileProvider provider, JObject context, string jsonPath)
    {
        string path = context.StringFromJsonPath(jsonPath);
        if (path == "") return "";

        path = path[..path.LastIndexOf('.')];
        UTexture2D icon = (UTexture2D)provider.LoadAllObjects(path).First();
        byte[]? png = icon.Decode()!.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        string pngPath = Path.ChangeExtension(Path.GetFileName(path),".png");
        File.WriteAllBytes(pngPath, png);

        return pngPath;
    }
}

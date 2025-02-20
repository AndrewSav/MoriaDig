using CUE4Parse.FileProvider;
using Newtonsoft.Json.Linq;

namespace MoriaDig;

// Represent a single field mapping from the game assets to csv
internal record Mapping(
    string Field, // The name of the field in the csv output
    string JsonPath, // JsonPath reference to the target field
    Func<DefaultFileProvider, JObject, string, string> Dig, // the function to extract the field from the asset
    bool Linked = false, // false, if JsonPath refers to this entity (recipe), and true if it refers to the items that the recipe produces
    bool Excluded = false, // if true excluded from resulting csv
    // See https://dynamic-linq.net/expression-language for the condition syntax
    string Condition = "" // if not empty, gives a condition, which causes this field not to be read, if the condition does not match
);

# MoriaDig

MoriaDig dumps information from `The Lord of the Rings: Return To Moria™` about in-game recipes for items and building into csv files, and extracts their icons.

## Running

Run MoriaDig.exe and provide path to the game asset paks, e.g.:

```
MoriaDig.exe "C:\Program Files (x86)\steam\steamapps\common\The Lord of the Rings Return to Moria™\Moria\Content\Paks"
```

This will create the following in the current folder:

- items.csv - information about crafting recipes for items
- buildings.csv - information about crafting recipes for buildings
- *.png - icons for the recipes about. Each csv has an "Icon" column, with corresponding file names

## Very brief code walkthrough

- `Program.cs` - the main program which calls Dumper for 1) items and 2) building
- `Dumper.cs` - dumps the data from the assets according to mappings (below)
- `Mapping.cs` - defines what a mapping for single csv field looks like
- `Mappings.cs` - lists mappings for all fields (based on the above) for both items and buildings
- `Util.cs`  - a few helper methods that did not fit anywhere else

## Goals

- Simplicity - should be easy to follow and tweak in case the asset format changes / breaks
- Flexibility - good foundation for extracting data from this game assets, easy to add more fields or write new extraction methods
- Comprehensiveness - provide most useful data around areas it is designed to provide them for

## Non-Goals

- User friendliness - this is a developer's tool, so you are expected to be a developer
- Error handling - the tool is likely to break if the in-game format changes. If so it will need to be fixed in code, not attempt is made to handle any type of issues gracefully
- Beautiful spreadsheet - the output while comprehensive is very rough. It's expected that you will need to massage it to fit for a particular purpose
- 100% percent accurate - this is something not achievable in the timeframe I'm willing to spend on this. There will be discrepancies, due to how the game interprets the data.

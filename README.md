# PartySizeReunited

## War Sails options

When the War Sails DLC is active, the MCM menu provides the following settings:

- **Deployment bonus boat** (0-8): Increases the number of ships that can be deployed in battle.
- **AI fleet safety limit** (3-500, default: 250): Caps the ideal fleet size returned for AI-controlled parties and clans. AI fleet targets are calculated dynamically from troop count and ship capacity, with a 10% capacity margin.

For each AI party, the mod keeps at least the game's original target while ensuring that the fleet can carry its troops. Clan targets are calculated by adding together the individual targets of their war parties. The player's party and the player's clan are never affected by these AI fleet distribution adjustments.

## Steam Workshop update

`Scripts\UpdatePartySizeReunited.ps1` updates the existing Steam Workshop item. It creates the XML expected by the TaleWorlds tool in the temporary directory and deletes it after execution, so no machine-specific path is stored in the repository.

Define the Bannerlord installation directory once in the user environment:

```powershell
[Environment]::SetEnvironmentVariable(
    "BANNERLORD_GAME_DIR",
    "D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord",
    "User"
)
```

Open a new terminal after setting the variable. By default, the published module is read from `%BANNERLORD_GAME_DIR%\Modules\PartySizeReunited`. To publish a module from another location, define the optional `PARTY_SIZE_REUNITED_MODULE_DIR` environment variable.

Run the update from the repository root:

```powershell
.\Scripts\UpdatePartySizeReunited.ps1 `
    -GameVersion "1.4.8" `
    -ChangeNotes "v2.3.0`nDescribe the changes here"
```

`GameVersion` is the compatible Bannerlord version used as a Steam Workshop tag. Both `1.4.8` and `v1.4.8` are accepted.


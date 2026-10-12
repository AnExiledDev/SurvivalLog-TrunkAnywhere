# Trunk Anywhere

A BepInEx 6 (IL2CPP) plugin for Survival Log: one key opens the backpack and trunk view anywhere before the disaster, the same view the cart's organize trunk action opens, and unpaid shop items are never let into the trunk.

## What it does

- `F6` opens your backpack beside your vehicle's trunk, wherever you are, through the game's own trunk open path (the event the cart's organize trunk action raises), so the game's own checks apply.
- If your own backpack is open, it is closed first and the trunk view opens in its place.
- An unpaid item dragged into the trunk stays where it was, and a short message says to pay for it first. The check sits where the game moves an item from one inventory to another, so it holds for every drag anywhere.

## Controls

`F6` in a loaded game before the disaster. It does nothing, apart from one line in the log, when:

- you have not bought a vehicle;
- any inventory other than your own backpack is open: a storage, a station, the trunk itself;
- any other window is open: a shop, trade, a menu;
- a text field has focus, or the game has turned its own hotkeys off;
- the disaster has happened, or no game is loaded.

## Config

`BepInEx/config/com.dev.survivallog.trunkanywhere.cfg`:

| Setting | Default | What it does |
| --- | --- | --- |
| `Controls/TrunkKey` | `F6` | Key that opens the backpack and trunk view. `None` turns the key off; unpaid items are kept out of the trunk either way. |

Log: `BepInEx/trunk-anywhere.log`.

## Known limits

- Any window the game counts as modal blocks the key, the settings menu included.
- The shop's trunk tab still buys unpaid items straight into the trunk, as the game always has. The game creates those items in the trunk directly and this mod only guards moves. Leaving the shop without paying takes them back out, as before.
- An unpaid item already in the trunk can be rearranged there or taken out.
- Door boxes are not the trunk and are not guarded.
- If the mod could not watch the bag screen (its hooks failed, see the log), the key only works while the bag screen is closed.

## Install

Copy `TrunkAnywhere.dll` into `BepInEx/plugins`. Needs BepInEx 6 (IL2CPP build).

## Build and test

```sh
dotnet build src/TrunkAnywhere -c Release -p:GameDir="D:\path\to\Survival Log"
dotnet test tests/TrunkAnywhere.Tests
```

The build references BepInEx and the game's interop assemblies under `GameDir`, so BepInEx must be installed and the game launched once. The tests compile only the pure `Core` code and need nothing from the game.

## License

MIT, see [LICENSE](LICENSE).

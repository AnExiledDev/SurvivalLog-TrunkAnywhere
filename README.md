# Trunk Anywhere

One key opens your vehicle's trunk from anywhere before the disaster.

Before the disaster, the only way to sort your trunk is the cart's organize trunk action, and you can't always reach it. Trunk Anywhere opens the same backpack and trunk view with a single key, wherever you are. It also keeps unpaid shop items out of the trunk.

## Features

- **Trunk key**: press `F6` to open your backpack beside your vehicle's trunk, wherever you are. It uses the game's own trunk view, the same one the cart's organize trunk action opens, so the game's own checks apply.
- **Works from your backpack**: if your own backpack is already open, it closes and the trunk view opens in its place.
- **Unpaid items stay out of the trunk**: dragging an unpaid item into the trunk does nothing. The item stays where it was and a message says "Pay for it first: unpaid items can't go in the trunk." This check sits where the game moves any item between inventories, so it holds for every drag.
- **No buying straight into the trunk**: buying on the shop's trunk tab does nothing. No item is created, nothing goes on the bill and no money is taken, and a message says "Buy into your bag: unpaid items can't go in the trunk." Buy into your bag as usual, pay, then move the item into the trunk.
- **Log file**: every key press and what came of it, and every unpaid item or shop buy kept out of the trunk, is written to `BepInEx\trunk-anywhere.log`.

Trunk Anywhere adds no data of its own to your save. Items you move with the trunk view are moved in your save the same way the game's own trunk view moves them. You can remove the mod at any time; the trunk and everything in it stay as they are.

## Hotkeys

| Key | What it does |
| --- | --- |
| `F6` | Opens the backpack and trunk view (before the disaster, once you own a vehicle). |

The key does nothing (apart from one line in the log) when:

- you have not bought a vehicle;
- any inventory other than your own backpack is open: a storage, a station, or the trunk itself;
- any other window is open: a shop, a trade, a menu;
- you are typing in a text field, or the game has turned its own hotkeys off;
- the disaster has already happened, or no game is loaded.

The key can be changed or turned off in the config.

## Known issues and limits

This mod moves items between your bag and your trunk. Back up your save before trying it for the first time.

- Any window the game treats as blocking stops the key, the settings menu included.
- An unpaid item that is already in the trunk (for example, bought there before you installed the mod) can still be moved around inside the trunk or taken out.
- Door boxes are not the trunk and are not guarded against unpaid items.
- If the mod could not hook the bag screen (the log says so), the key only works while the bag screen is closed.

If an item goes missing, an unpaid item gets into the trunk, or the key does something unexpected, please tell me (see [Feedback](#feedback-and-bug-reports)).

## Requirements

- Survival Log 1.1.18317. Built and tested on this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Download `TrunkAnywhere.dll` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-TrunkAnywhere/releases/latest) and put it in the `BepInEx\plugins` folder inside the game folder (the folder with the game .exe). Full path example: `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\TrunkAnywhere.dll`

## Uninstall

Delete `TrunkAnywhere.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.trunkanywhere.cfg` and `BepInEx\trunk-anywhere.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.trunkanywhere.cfg`, created the first time the game starts with the mod. Edit it while the game is closed; changes apply at the next game start. See [CONFIG.md](CONFIG.md) for every setting.

## Troubleshooting

Tested on Survival Log 1.1.18317 with BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for `===== Trunk Anywhere 0.1.1 started, key F6`. The same line, plus one line for every press of the key saying what happened and why, is in `BepInEx\trunk-anywhere.log`, which survives restarts. If the key seems to do nothing, that file says which of the reasons above stopped it.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also leave a comment on the Nexus page or [open an issue](https://github.com/AnExiledDev/SurvivalLog-TrunkAnywhere/issues). When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\trunk-anywhere.log`.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/TrunkAnywhere -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\TrunkAnywhere\bin\Release\TrunkAnywhere.dll`.

The unit tests cover the key and unpaid-item rules and need nothing from the game:

```
dotnet test tests/TrunkAnywhere.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).

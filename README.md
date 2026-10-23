# Trunk Anywhere

One key opens your vehicle's trunk from anywhere before the disaster.

Before the disaster, the only way to sort your trunk is the cart's organize trunk action, and you can't always reach it. Trunk Anywhere opens the same backpack and trunk view with a single key, wherever you are. It also stops unpaid shop items being dragged into the trunk.

## Features

- **Trunk key**: press `F6` to open your backpack beside your vehicle's trunk, wherever you are. It uses the game's own trunk view, the same one the cart's organize trunk action opens, so the game's own checks apply.
- **Works from your backpack**: if your own backpack is already open, it closes and the trunk view opens in its place.
- **No dragging unpaid items in**: dragging an unpaid item into the trunk does nothing. The item stays where it was and a message says "Pay for it first: unpaid items can't be moved into the trunk." This check sits where the game moves any item between inventories, so it holds for every drag.
- **Buy straight into the trunk**: buying on the shop's trunk tab works as it does in the game. The item goes into the trunk and onto the same bill as your bag, and you pay for both together.
- **Take All to Trunk**: when you loot a storage or container and your trunk is one of the tabs on the left, a Take All to Trunk button sits right under Take by Type. It moves everything in the container straight into the trunk, without switching tabs, using the game's own Take All move. It is greyed out when the container is empty and hidden while the trunk tab is the one shown, where the game's own Take All already fills the trunk. If the trunk fills up, the rest stays in the container and the game says so. If the container holds an unpaid item, nothing moves and the "Pay for it first" message shows.
- **Log file**: every key press and Take All to Trunk press and what came of it, and every unpaid item kept out of the trunk, is written to `BepInEx\trunk-anywhere.log`.

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

- Any window the game treats as blocking stops the key, the settings menu included.
- Only drags into the trunk are checked. An item from the shop's trunk tab that is not yet paid for is not stopped from being moved inside the trunk or out of it.
- Door boxes are not the trunk and are not guarded against unpaid items.

## Requirements

- Survival Log 1.1.19611. Built and tested on this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- Optional: [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Optional, to change settings without a restart: install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `TrunkAnywhere-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-TrunkAnywhere/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/64).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\TrunkAnywhere.dll`

## Uninstall

Delete `TrunkAnywhere.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.trunkanywhere.cfg` and `BepInEx\trunk-anywhere.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.trunkanywhere.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Each setting in the cfg file is explained in English and Simplified Chinese. See [CONFIG.md](CONFIG.md) for every setting. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

The Take All to Trunk button and the unpaid-item message follow the game's language: English or Simplified Chinese.

## Troubleshooting

Tested on Survival Log 1.1.19611 with BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for `===== Trunk Anywhere 0.4.2 started`, followed by `trunk key F6`. The same lines, plus one line for every press of the key saying what happened and why, is in `BepInEx\trunk-anywhere.log`, which survives restarts. If the key seems to do nothing, that file says which of the reasons above stopped it.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\trunk-anywhere.log`.

To reach me directly, message `exiled.dev` on Discord; you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/TrunkAnywhere -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\TrunkAnywhere\bin\Release\TrunkAnywhere.dll`.

The unit tests cover the key and unpaid-item rules and the Take All to Trunk button, and need nothing from the game:

```
dotnet test tests/TrunkAnywhere.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).

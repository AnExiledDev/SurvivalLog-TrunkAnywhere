# Trunk Anywhere

One key opens your vehicle's trunk from anywhere before the disaster.

Before the disaster, the only way to sort your trunk is the cart's Organize Trunk option, and the cart isn't always nearby. Trunk Anywhere opens the same backpack and trunk view with a single key, wherever you are. It also stops you from dragging unpaid shop items into the trunk.

## Features

- **Trunk key**: press `F6` to open your backpack next to your vehicle's trunk, wherever you are. It's the same trunk view the cart's Organize Trunk option opens, so everything in it works the way it normally does.
- **Works from your backpack**: if your backpack is already open, it closes and the trunk view opens in its place.
- **No dragging unpaid items in**: dragging an unpaid item into the trunk does nothing. The item stays where it was and a message says "Pay for it first: unpaid items can't be moved into the trunk." This works for every drag into the trunk.
- **Buy straight into the trunk**: buying on the shop's trunk tab works like it does without the mod. The item goes into the trunk and onto the same bill as your bag, and you pay for both together.
- **Take All to Trunk**: when you loot a storage or container and your trunk is one of the tabs on the left, a Take All to Trunk button shows up right under Take by Type. It moves everything in the container straight into the trunk, so you don't have to switch tabs. The button is greyed out when the container is empty. It's hidden while the trunk tab is the one showing, since the game's Take All already puts things in the trunk there. If the trunk fills up, the rest stays in the container and the game tells you. If the container holds an unpaid item, nothing moves and the "Pay for it first" message shows. You can turn the button off with `TakeAllToTrunk` in the config.
- **Log file**: `BepInEx\trunk-anywhere.log` gets a line for every press of the key or the Take All to Trunk button, saying what happened. It also notes every unpaid item the mod kept out of the trunk.

Trunk Anywhere doesn't add anything to your save. Items you move in the trunk view are saved the same way they are when you use the cart. You can remove the mod at any time, and the trunk and everything in it stay as they are.

## Hotkeys

| Key | What it does |
| --- | --- |
| `F6` | Opens the backpack and trunk view (before the disaster, once you own a vehicle). |

The key does nothing (apart from one line in the log) when:

- You haven't bought a vehicle.
- Any inventory other than your backpack is open, like a storage, a station or the trunk itself.
- Any other window is open, like a shop, a trade or a menu.
- You're typing in a text field, or the game has its hotkeys switched off at that moment.
- The disaster has already happened, or no game is loaded.

The key can be changed or turned off in the config.

## Known issues and limits

- The key does nothing while any other window is open, the settings menu included.
- Only drags into the trunk are checked. An item you picked on the shop's trunk tab but haven't paid for yet can still be moved around inside the trunk or out of it.
- Doorway Storage doesn't count as the trunk, so the unpaid-item check doesn't cover it.

## Requirements

- Survival Log 1.1.20719. It's built and tested on this version, and a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.19.0 or newer. Trunk Anywhere does not load without it. It applies settings changed on the Mod Manager's Mods screen without restarting the game.
- Optional: [Mod Manager](https://www.nexusmods.com/survivallog/mods/73). With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `TrunkAnywhere-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-TrunkAnywhere/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/64).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\TrunkAnywhere.dll`

## Uninstall

Delete `TrunkAnywhere.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.trunkanywhere.cfg` and `BepInEx\trunk-anywhere.log` (with `trunk-anywhere.log.1` if it is there).

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.trunkanywhere.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Each setting in the cfg file is explained in English and Simplified Chinese. See [CONFIG.md](CONFIG.md) for every setting.

The Take All to Trunk button and the unpaid-item message follow the game's language: English or Simplified Chinese.

## Troubleshooting

Tested on Survival Log 1.1.20719 with BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for `===== Trunk Anywhere 0.4.7 started`, followed by `trunk key F6`. `BepInEx\trunk-anywhere.log` has the same lines, plus one line for every press of the key saying what happened and why, and it's kept between restarts. If the key seems to do nothing, that file says which of the reasons above stopped it.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\trunk-anywhere.log`.

To reach me directly, message `exiled.dev` on Discord. You can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx, the game's IL2CPP interop assemblies and Exiled's Toolkit, so a game install with BepInEx set up and started once is required. Those assemblies are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/TrunkAnywhere -c Release -p:GameDir="C:\path\to\Survival Log" -p:ToolkitDll="C:\path\to\ExiledToolkit.dll"
```

`GameDir` is the game folder and `ToolkitDll` is a built `ExiledToolkit.dll`. The defaults in the project file are local paths you will almost certainly need to override. The output DLL is at `src\TrunkAnywhere\bin\Release\TrunkAnywhere.dll`.

The unit tests cover the key and unpaid-item rules and the Take All to Trunk button, and need nothing from the game:

```
dotnet test tests/TrunkAnywhere.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).

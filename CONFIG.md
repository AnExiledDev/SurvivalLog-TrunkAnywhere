# Trunk Anywhere settings

Settings live in `BepInEx\config\com.dev.survivallog.trunkanywhere.cfg`, created the first time the game starts with the mod. Edit the file while the game is closed; changes apply at the next game start. Delete the file to get the defaults back.

## Controls

| Setting | Default | What it does |
| --- | --- | --- |
| `TrunkKey` | `F6` | Key that opens the backpack and trunk view anywhere before the disaster, once you own a vehicle. It does nothing while a storage, shop or any other window is open, or while you type. |

The value is a Unity key name, for example `F6`, `F7`, `T` or `Keypad5`. Set it to `None` to turn the key off. Unpaid items are kept out of the trunk either way.

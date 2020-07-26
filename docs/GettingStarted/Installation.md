# Installation

## SideLoader

Installing BepInEx:
* See <b>[Outward Wiki: Installing Mods](https://outward.gamepedia.com/Installing_Mods#BepInEx_Loader)</b> for instructions on installing BepInEx

Installing SideLoader:
* Download the latest SideLoader from the <b>[SL Releases page](https://github.com/sinaioutlander/Outward-SideLoader/releases)</b>
* Put this zip file in your Outward directory, so it looks like `Outward\SideLoader.zip`
* Right-click the file and <b>"Extract Here"</b>, so it merges with the existing folders.
* Done!

## SideLoader Mods
Generally, you don't need to worry about this, but if you have trouble installing a SideLoader mod this might be helpful.

Some mods which use the SideLoader still might only use a single `.dll` file, and you don't need to do anything special for those.

However, some mods which use the SideLoader will use something called a "SideLoader Pack" or "SL Pack". This is simply a folder. Each pack uses it's own folder to keep the assets separate from other mods. SideLoader looks for these folders, and will automatically load and apply the assets from them.

They can be placed in the `Mods\SideLoader\` folder, for example: `Outward\Mods\SideLoader\{PackName}\...`.

They can also be placed in a structure such as `Outward\BepInEx\plugins\{ModName}\SideLoader\...`. This is the recommended structure for BepInEx mods to keep things organized.
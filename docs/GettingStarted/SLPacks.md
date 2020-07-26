# SL Packs

## Overview

An <b>SL Pack</b> (or <b>SideLoader Pack</b>) is simply a <b>Folder</b>. SL Packs are used to load and configure assets.

SideLoader supports two structures for the SL Pack folder:
* `Outward\Mods\SideLoader\{Name}\`
* `Outward\BepInEx\plugins\{Name}\SideLoader\`

The second format is recommended if you are making a BepInEx mod, as it means you can store everything in one folder.

The `{Name}` in the path is the unique name of your Mod or Asset Pack. It can be anything, but try to be original to avoid conflicts.

For example, `Outward\Mods\SideLoader\MyCoolPack\` is an SL Pack folder called MyCoolPack. For the BepInEx structure, it would look like `Outward\BepInEx\plugins\MyCoolPack\SideLoader\`

## SL Pack Structure
Other than the Name, the only special thing about your SL Pack is the name of the sub-folders you create inside of it.

The sub-folders you can create are:
* `AudioClip\`
* `AssetBundles\`
* `Characters\`
* `Enchantments\`
* `Items\`
* `Recipes\`
* `StatusEffects\`
* `Texture2D\`

### AudioClip

The `AudioClip\` folder is used for <b>.wav</b> files, either for replacing existing game sounds/music or for custom audio for your own mods.

Your .wav files simply need to be placed inside the AudioClip folder. To replace Audio, use the same name that the game uses. 

See also: <B>TODO</b> [Replacing Audio]() for more details.

### AssetBundles

The `AssetBundles\` folder is simply used for loading Unity AssetBundles.

You <b>only need to place the actual AssetBundle</b> in here, you do <b>not</b> need to include the ".meta" or ".manifest" files, nor do you need to include the generic "AssetBundles" file.

AssetBundles which you place in this folder can be referenced by the <b>file name</b>.

See also: [Unity AssetBundles Guide](https://docs.unity3d.com/Manual/AssetBundles-Workflow.html)

<b>Important note:</b> 
* You must use the same version of the Unity Editor that Outward uses to build your asset bundles.
* Currently, this is <b>[Unity 2018.4.8](https://download.unity3d.com/download_unity/9bc9d983d803/Windows64EditorInstaller/UnitySetup64-2018.4.8f1.exe)</b>.

### Characters

The `Characters\` folder is used for defining custom Characters.

See <B>TODO</b> [Custom Characters]().

### Enchantments

The `Enchantments\` folder is used for defining custom Enchantments.

See <B>TODO</b> [Custom Enchantments]().

### Items

The `Items\` folder is used for defining custom Items and Skills.

You can place custom item xmls directly in this folder if you want (eg. `Items\MySimpleItem.xml`).

You can also create a <b>sub-folder for each item</b>. This is used if you are defining any <b>custom icons or textures</b> for the item (explained in more detail on the Custom Items page).

See also: <B>TODO</b> [Custom Items]().

### Recipes

The `Recipes\` folder is obviously used to define custom recipes. Simply place the xml file(s) in this folder.

See also: <B>TODO</b> [Custom Recipes]().

### StatusEffects

The `StatusEffects\` folder is for custom Status Effects and Imbues. Similar to custom items, you can put the XMLs directly in this folder or you can create a folder for each status.

See also: <B>TODO</b> [Custom Status Effects]().

### Texture2D

The `Texture2D` folder is used for replacing textures. The SideLoader will find currently loaded textures which have the same name and replace them.

See also: <B>TODO</b> [Replacing Textures]().
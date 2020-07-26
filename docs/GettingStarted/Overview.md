# SL Overview

This page covers some important basics about SideLoader, and clears up some common questions.

## Editing Etiquette

When using SideLoader to <b>edit existing assets</b>, you should be courteous of other modders and only change what you need to.

Generally, you <b>only need to include what you are changing</b>. This goes for the textures and icons sometimes found in the exported files too. Let's say you are only changing the Description of an Item - you can remove everything from the XML template other than the Description field (and the Target / New ID fields), as well as deleting the icons and textures.

By only changing what you need to, this allows your mod to be compatible with other edits to the same asset.

## XML vs C#

There are two ways you can use SideLoader:
1. Using <b>SL Packs</b> and <b>XML</b>
2. Using <b>C#</b> directly from your own mod

The SL Documentation focuses on SLPacks / XML, however all of the same rules apply to C# as well.

* Any XML file can also be applied from C# as well, since these XML files are simply wrappers for the C# class.
* The SL Pack approach is used for loading non-XML Assets, such as Textures, Audio and AssetBundles.
* You can use either or both of these approaches, simply use whatever is most convenient for you.

## F.A.Q ##
### Can I use this to edit existing Items/Skills/etc? {docsify-ignore}

Yes. When editing an existing item, you should remove (delete or comment out) all other fields on the template that you don't want to change, to avoid conflicts with other SideLoader edits to the same item. Some Skills may not be fully supported yet.

### How can I know the possible values for X setting on a template? {docsify-ignore}

Check the wiki page for that kind of template, or the Enums / google doc resources.

### Does this work in multiplayer? {docsify-ignore}

Yes, but it <b>doesn't</b> "sync" your changes or custom items, you need to make sure all players have the same SideLoader mods enabled if you want it to be synced. 

### Can I load custom Levels using this? {docsify-ignore}

There is some limited support for it from C# with the `CustomScenes` class, but it's not well tested yet.
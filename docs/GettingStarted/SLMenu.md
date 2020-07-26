# SL Menu

## Overview
The <b>SideLoader Menu</b> is currently used for generating XML Templates from existing Items, Status Effects and Enchantment recipes. It is also capable of exporting icons and textures from the prefab.

## Using The Menu

Press <b>Control + Alt + F6</b> to show or hide the SideLoader menu when in-game.

<i>Note: F6 must be pressed last in the combo.</i>

### Items

* <b>Item ID:</b> this is the ID of the Item you are cloning FROM. This sets "Target_ItemID" in the template.
* <b>New ID:</b> the ID of the Item your template will be applied to. It can be an existing ID, or a new one. This sets "New_ItemID" in the template.

When you press the button to generate a template, it will be generated to <b>`Mods/SideLoader/_GENERATED/Items/`</b>. It will automatically create a sub-folder template for this item including the Textures and Icon(s).

To use this generated template, simply place the item sub-folder in your `SideLoader/MyPack/Items/` folder.
* eg, if the generated item folder was "2000010_IronSword", take that folder and put it in your pack so it looks like "MyPack/Items/2000010_IronSword".
* You can rename this item sub-folder if you want. The name of that folder is not important.

The Menu also lets you set the `EffectBehaviour` setting on the `SL_Item` template.

* <b>Replace Effects</b>: `DestroyEffects`
* <b>Ovveride by Transform</b>: `OverrideEffects`
* <b>Add on top</b>: `NONE`

### Status Effects

The Status Effects tab allows you to dump a status effect, in a similar way to items. The templates will be generated to the folder `Outward\Mods\SideLoader\_GENERATED\StatusEffects\`.

To find the ID of the status you want to dump, use [this Google Sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1969601658).

### Enchantments

Pick any Enchantment ID off [this list](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Enchantments.txt).

The template will be generated to `Mods\SideLoader\_GENERATED\Enchantments\`.
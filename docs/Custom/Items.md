# Custom Items

<b>Custom Items</b> can be used to make completely new items or modify existing ones. This includes generic items, consumables, equipment, weapons, and skills. You can do this either from an SL Pack, or directly from your own C# mod.

The SL Pack sub-folder for custom items is `Items\`, and this page will focus on working with XML templates from this folder.

## Creating a template

Use the <b>[SideLoader Menu](GettingStarted/SLMenu.md)</b> to generate templates for you from existing items or skills. It will also dump all the textures as well as the icons, and set it up in an SL pack structure ready to go for you.

Pick an item similar to what you want to make (or pick the item you want to edit), and generate a template for it.

Your custom item XML can be placed in the `Items\` sub-folder of your [SL Pack](GettingStarted/SLPacks.md). The XML can be named anything.
* Eg, `[SLPack Folder]\Items\MyItem.xml`

If you want to use custom Item Visuals (including changing the icons or the textures), you need to create a <b>sub-folder for each item</b>.
* Eg, `[SLPack Folder]\Items\MyItem\MyItem.xml`

## Editing Etiquette

When editing <b>existing items</b> with SideLoader, you should be courteous of other Modders and only change what you need to.

SideLoader allows you to <b>only include the things you want to change</b>. The only two things that are required in an SL_Item template are the `Target_ItemID` and the `New_ItemID`, everything else is <b>optional</b>.

<b>Only include the things you need to change.</b>

For the Effects (SL_EffectTransform), it's slightly more complicated than that, see more in the related section.

See also: [SL Overview](GettingStarted/Overview)

## SL_Item

All custom items have these fields.

`Target_ItemID` (integer) <b>[REQUIRED]</b>
* The Item ID of the Item you are cloning <b>from</b>, or the item you want to edit. If you leave out fields on the template, it will default to the value from this item.

`New_ItemID` (integer) <b>[REQUIRED]</b>
* The Item ID you want to apply the template <b>to</b>. It can be a new or existing ID.

`Name` (text)
* The item name.
* [Unity Rich Text](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html) is supported. In Xml however, you must use `&lt;` for "<" and `&gt;` for ">", so for example it would look like `&lt;b&gt;` for `<b>`.

`Description` (text)
* Type out the full description as it should appear in game.
* If you want to overwrite to a blank description, set the value to `<Description />` in Xml, or `Description = ""` in C#.
* [Unity Rich Text](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html) is supported. See note about Xml Rich Text above.
* In Xml, don't use "\n" for new lines, put an actual new line.

`LegacyItemID` (integer)
* If you want a Legacy Chest upgrade for this item, enter that item ID here. That will be the result when this item is placed in a chest.
* Set to `-1` for no upgrade.

`IsPickable` (true/false)
* Can the item be picked up? This affects whether it shows in the game's F1 Debug Menu too.

`IsUsable` (true/false)
* Can you "Use" the item?

`QtyRemovedOnUse` (integer)
* If usable, how many get removed on use?

`GroupItemInDisplay` (true/false)
* Should this item stack in displays? This means it can be an item stack.

`HasPhysicsWhenWorld` (true/false)
* Determines if the item should behave as a rigid body when in world space

`RepairedInRest` (true/false)
* Does this item get repaired when resting? (If equipped)

`BehaviorOnNoDurability` (enum)
* What happens when this item reaches 0 durability
* Must be exactly one of: `NotSet`, `DoNothing`, `Destroy`, or `Replace`

`CastType` (enum)
* The animation when this item is "Used", or if it's a skill the animation when it is used
* See [this reference](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/Character.SpellCastType.txt) for the possible values you can set (there are a lot)

`CastModifier` (enum)
* A modifier on the character when you use or activate this item/skill.
* Must be exactly one of: `Immobilized`, `Mobile`, `Attack` or `NONE`

`CastLocomotionEnabled` (true/false)
* Can you move while casting or using this item/skill?

`MobileCastMovementMult` (float)
* If you can move while casting, this is the modifier placed on the character's movement speed.
* It's a "float" value, meaning you can set it to "0.3" or "1.52374", etc.

`CastSheatheRequired` (integer)
* Sets the type of  weapon sheathing behavior when you use/cast this item.
* Either `0`, `1` or `2`. I'm not totally sure how this one works, haven't looked into it.

`OverrideSellModifier` (integer)
* Overrides the Sell Modifier (at ALL merchants) for the item compared to the Buy Price. By default it is 0.3 for most items.
* Set to `-1` for no override
* Generally the only time this is used is for items with 1:1 Buy:Sell, eg Gold Ingot.

### Tags
The `Tags` is a list of string (text) values. Tags are used by the game for lots of various things.
* You can use this for an item to count as a type of ingredient, eg `Water`, `Meat`, `Vegetable`
* Also used for item behaviour, eg `Lexicon`
* See [this Google Sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1840819680) for a list of tags.
* Do not include the tag number. Eg, put `Weapon`, not `1-Weapon`.

Your tags XML should look like this, for example:
```xml
<Tags>
  <string>Misc</string>
  <string>Item</string>
  <string>Ammo</string>
  <string>Consummable</string>
</Tags>
```

Some tags are added by default. For example, if you create an Axe, it will automatically get "Axe" and "Weapon", you don't have to set those.

### StatsHolder
The `StatsHolder` contains the item's <b>Stats</b> values. Not all items have stats, but most do.

A base SL_ItemStats XML should look like this:
```xml
<StatsHolder>
  <BaseValue>0</BaseValue>
  <RawWeight>0</RawWeight>
  <MaxDurability>0</MaxDurability>
</StatsHolder>
```

These stats are:

`BaseValue` (integer)
* The base <b>buy price</b> of the item. The sell price is 0.3x this value, by default.

`RawWeight` (float)
* The weight of this item. Can include decimal.

`MaxDurability` (integer)
* The max and starting durability of this item. No decimals.
* Set to -1 for indestructable

Note that Equipment and Weapons have their own custom stats which override the StatsHolder field. More details below in the Equipment and Weapons sections.

### EffectTransforms

`EffectTransforms` is a more complex part of custom items. This value is a <b>list of `SL_EffectTransform` objects.</b>

See the [Effects](Effects/EffectTransforms) page for documentation on all SL_Effect classes, and more details on how to set up EffectTransforms.

This is used for <b>Effects</b> for many purposes such as <b>On-Hit Effects</b> for weapon and skills, <b>Passive</b> effects, <b>Consumable</b> effects, etc.

The EffectTransforms value is actually a <b>list of Transforms</b> (a container for objects in a Unity game), which can each contain any number of custom Effects, as well as further nested child transforms.

`EffectBehaviour` (enum)
* This setting relates to your `EffectTransforms`, and it controls the template behaviour in regards to the Effects on the item or skill.
* The options are: `DestroyEffects`, `OverrideEffects` and `NONE`.
* <b>`DestroyEffects`</b> will destroy all the existing effects on the item or skill before it applies your new ones. This means everything will be overwritten with your new effects.
* <b>`OverrideEffects`</b> will only destroy existing transforms if you have defined one of the same name in your EffectTransforms list. Use this to replace specific transforms, but not all of them.
* <b>`NONE`</b> will add your effects on top (if you defined any) and not remove anything.

### ItemExtensions

The `ItemExtensions` and the `ExtensionsEditBehaviour` fields are used for various additional behaviour components added to Items, such as Deployable, Ephemeral, etc.

See [Item Extensions](Custom/ItemExtensions) for more information.

## Visuals and Icons

There are three fields on the SL_Item template which relate to custom visuals.

`SL_Item.ItemVisuals`, `SL_Item.SpecialItemVisuals` and `SL_Item.SpecialFemaleItemVisuals`.

See [Custom Item Visuals](Custom/ItemVisuals) for instructions on setting up custom visuals, and also for details on changing textures or icons with your own .png files.

Even if you are not defining custom visuals at all, you can still use some of these settings to align and rotate the existing visuals.

# SL_Item Sub-classes

<u><b><i>The sections below are specific to certain types of custom items.</i></b></u>

## SL_Skill
SL_Skill is a rather involved topic, with many derived classes. See [Custom Skills](Custom/Skills.md) for more information.

## SL_Equipment

The `SL_Equipment` template is used for base Equipment, and it inherits all the base fields on SL_Item. The classes SL_Bag and SL_Weapon inherit from SL_Equipment.

Along with all the SL_Item fields, you have the following:

`EquipSlot` (enum)
* Sets the Equipment Slot this item equips to
* Must be exactly one of: `Helmet`, `Chest`, `Foot`, `RightHand`, `LeftHand`, `Back`, or `Quiver`

`TwoHandType` (enum)
* Sets the two-handed behaviour for this item, if it equips to the hand-slot
* Must be exactly one of: `None`, `TwoHandedRight` (melee), or `TwoHandedLeft` (bow)

`IKType` (enum)
* For off-hand equipment, the IK (hand/arm animation) when the character holds the item.
* Must be exactly one of: `None`, `Lantern`, `Torch`, or `Lexicon`

### SL_EquipmentStats

The <b>Equipment Stats</b> overrides the base `StatsHolder` field. It will look like:
```xml
<StatsHolder xsi:type="SL_EquipmentStats">
```

`Damage_Resistance` (list of floats)
* This is a list of float number values which sets the Damage Resistance of the equipment.
* Normal values are between -100 and 100.
* Define a `<float></float>` for each entry. There must be at least 6 entries, though technically the game uses 9.
* The order is: <b>`Physical`, `Ethereal`, `Decay`, `Lightning`, `Frost`, `Fire`,</b> and the unused `DarkOLD`, `LightOLD` and `Raw`

For example:

```xml
<Damage_Resistance>
  <float>0</float> <!-- Physical -->
  <float>0</float> <!-- Ethereal -->
  <float>0</float> <!-- Decay -->
  <float>0</float> <!-- Electric (lightning) -->
  <float>0</float> <!-- Frost -->
  <float>0</float> <!-- Fire -->
  <float>0</float> <!-- [UNUSED] DarkOLD -->
  <float>0</float> <!-- [UNUSED] LightOLD -->
  <float>0</float> <!-- [UNUSED] Raw -->
</Damage_Resistance>
```

`Impact_Resistance` (float)
* Impact resistance stat, 0-100

`Damage_Protection` (float)
* Physical protection stat, 0-100

`Damage_Bonus` (list of float)
* Same as Damage_Resistance, but for damage bonuses instead.
* Normal values are between -100 and 100.

`Stamina_Use_Penalty` (float)
* Positive values mean it increases stamina costs, negative values decreases stamina costs.

`Mana_Use_Modifier` (float)
* Works the same way as stamina use penalty, but for mana.

`Movement_Penalty` (float)
* A positive value will reduce movement speed, a negative value increases it.

`Pouch_Bonus` (float)
* Increases capacity of pouch

`Heat_Protection` (float)
* Hot Weather Def stat

`Cold_Protection` (float)
* Cold Weather Def stat

`Corruption_Protection` (float)
* For Soroboreans DLC, Corruption protection stat.

`Health_Regen` (float)
* Health regen per second

`Cooldown_Reduction` (float)
* Cooldown reduction %, between 0 and 100.

## SL_Armor
`SL_Armor` derives from `SL_Equipment`, and contains a couple extra fields.

`Class` (enum)
* The Armor Class. Not sure if this actually has an effect on anything.
* Must be exactly one of: `Cloth`, `Light`, `Medium` or `Heavy`.

`GearSoundMaterial` (enum)
* Sets the general movement sound of the equipment
* Must be exactly one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSoundMaterials.txt).

`ImpactSoundMaterial` (enum)
* The sound when the armor is hit
* Must be exactly one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSoundMaterials.txt).

## SL_Weapon

As long as all the fields on SL_Item and SL_Equipment, weapons also have a few more values and their own stats.

`WeaponType` (enum)
* Sets the type of weapon.
* Must be one of these values: [Weapon.WeaponType](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/Weapon.WeaponType.txt)

`Unblockable` (true/false)
* Sets the unblockable behaviour. Is this weapon <b>unblockable</b>? Currently no weapons in the live game have this as true, but it does work.

`SwingSound` (enum)
* The sound when you swing the weapon.
* Must be exactly one of: `Default`, `Weapon_1H`, `WeaponHvy_1H`, `Weapon_2H`, or `WeaponHvy_2H`

`SpecialIsZoom` (true/false)
* If true, the special attack is for zooming in (used for Bows).

`HealthLeechRatio` (float)
* Percent of damage dealt that is restored to player's current health

`HealthBurnLeechRatio` (float)
* Percent of damage dealt that is restored to player's <b>burnt</b> health

### SL_WeaponStats

The <b>Weapon Stats</b> overrides the `StatsHolder` field for a second time (after EquipmentStats). It will look like:
```xml
<StatsHolder xsi:type="SL_WeaponStats">
```

While weapons do have all the same fields as Equipment, they cannot actually use a lot of them. Weapons cannot use any resistance stats, for example.

The extra stats you have on WeaponStats are:

`AttackSpeed` (float)
* The attack speed of the weapon. Usually it's 0.8, 0.9, 1.0, 1.1 or 1.2.

`Impact` (float)
* Base impact damage (displayed value only)

`StamCost` (float)
* Base stamina cost of attacks. For bows, this is consumed when you draw.

`AutoGenerateAttackData` (true/false)
* Only relevant for melee weapons.
* Defaults to `false`, and your `Attacks` will be used to set the Attack Data.
* If `true`, this will ignore your `Attacks`, and instead automatically generate scaled damages from your `BaseDamage`, `Impact` and `StamCost` values. There are standard multipliers used for each weapon class which the SideLoader uses to scale your stats.

#### BaseDamage
The `BaseDamage` is a list of custom SL_Damage objects, and defines the base damage of the weapon. 

A BaseDamage XML should look like this:
```xml
<BaseDamage>
  <SL_Damage>
    <Damage>20</Damage>
    <Type>Physical</Type>
  </SL_Damage>
  <SL_Damage>
    <Damage>15</Damage>
    <Type>Decay</Type>
  </SL_Damage>
</BaseDamage>
```

For the "Type", you must set one of: `Physical`, `Ethereal`, `Decay`, `Electric` (<b>not</b> Lightning), `Frost`, or `Fire`.

You can add as many <SL_Damage> entries as you want.

#### AttackData
The `Attacks` field is a list of `AttackData` objects. This is the data used for the <b>actual attacks</b> for melee weapons, for the damage, impact and stamina cost. Melee weapons have <b>5</b> AttackData entries.

The first two AttackData entries are for the standard attacks, the 3rd is for Special attacks, and the 4th and 5th are for the combo attacks.

If `AutoGenerateAttackData` is `true`, these values are ignored and instead automatically generated.

The Attacks should look like this:

```xml
<Attacks>
  <AttackData>
    <StamCost>0.5</StamCost>
    <Knockback>12</Knockback>
    <AttackSpeed>0</AttackSpeed>
    <Damage>
      <float>20</float>
    </Damage>
  </AttackData>
  <!-- 
  etc... continue adding AttackData here, up to 5 entries.
  ...
  -->
</Attacks>
```

The fields on AttackData are:

`StamCost`
* The stamina cost per swing

`Knockback`
* The impact damage per hit

`AttackSpeed`
* Not actually used, just ignore it.

The `Damage` value is a list of `<float></float>` values.
* This corresponds to the Base Damage you set. Each entry on the list represents a damage type, in the order that you defined the base damage. If you only defined one damage type, you only need one entry, etc.

```xml
    <Damage>
      <float>20</float> <!-- first BaseDamage type you defined -->
      <float>20</float> <!-- second BaseDamage type ... -->
      <!-- 3rd type ..? -->
    </Damage>
```

## SL_Bag
Bags inherit from SL_Equipment, and contain a few extra fields.

`Capacity` (float)
* The capacity of the bag

`Restrict_Dodge` (true/false)
* Does this backpack restrict the character's dodge when equipped?

`InventoryProtection` (float)
* Protection to the durability of items in the backpack when hit from behind

## SL_RecipeItem
<b>SL_RecipeItem</b> is a very simple class, just used to make a Recipe Scroll. It inherits from `SL_Item` and contains only one other value:

`RecipeUID` (string)
* The UID of the recipe you want this scroll to teach.
* This corresponds to the `UID` in a [SL_Recipe](Custom/ItemRecipes) template.

<b>Note:</b> Unlike other items, Recipe Scrolls are not listed on the wiki, so it may be difficult to get an ID to clone from. They are usually in the `5700000` range, you can use `5700022` for example and change the Name, Description and Recipe UID.

## SL_MultiItem

SL_MultiItem can be used from XML to define multiple item edits from one template. This might be useful if you are making bulk edits, or lots of very small edits.

The SL_MultiItem template is very simple, it's basically just a list of SL_Item templates. You only have one field: `Items`, which is a list of SL_Items.

It should look like this in XML:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SL_MultiItem xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<Items>
		<SL_Item>
			<Target_ItemID>2000010</Target_ItemID>
			<New_ItemID>2000010</New_ItemID>
			<Name>Iron Sword 2</Name>
			<Description>Test.</Description>
		</SL_Item>
		<SL_Item>
			<Target_ItemID>2000150</Target_ItemID>
			<New_ItemID>2000150</New_ItemID>
			<Name>Brand 2</Name>
			<Description>Test.</Description>
		</SL_Item>
	</Items>
</SL_MultiItem>
```
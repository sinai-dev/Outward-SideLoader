# Item Extensions

An <b>SL_ItemExtension</b> can be defined in the `ItemExtensions` field on an [SL_Item](Custom/Items.md) template.

## How to define one

When defining an SL_ItemExtension, simply define them like any other Xml object inside the `ItemExtensions` list of the SL_Item.

```xml
<ItemExtensions>
    <!-- replace 'ExtensionType' with the actual extension type -->
    <SL_ItemExtension xsi:type="ExtensionType">
        <!-- put fields and values for this extension here -->
    </SL_ItemExtension>
</ItemExtensions>
```

For example:

```xml
<SL_ItemExtension xsi:type="SL_Ephemeral">
    <Savable>true</Savable>
    <Lifespan>60</Lifespan>
</SL_ItemExtension>
```

## Edit behaviour

The field `SL_Item.ExtensionsEditBehaviour` is an `EditBehaviour` value (enum), it will determine how your Extensions are applied.

* `NONE` or `Override` will not remove any of the existing Extensions, and it will add or modify Extensions based on the ones you have defined.
* `Destroy` will remove all Extensions which are not defined in your ItemExtensions list.

## SL_ItemExtension

All SL_ItemExtension classes have this field.

`Savable` (true/false)
* Does this ItemExtension save its values to the player's save file? This depends on the type of ItemExtension you are using.

# Sub-classes

Below are all the subclasses of SL_ItemExtension. These contain the field above, as well as extra fields.

## SL_Deployable
Used to deploy or pack a deployable item (tents, crafting stations, etc). This behaviour is pretty specific to the existing Deployable items, you will need to use C# to modify the interactions in a significant way.

Deployables all have a "Deployed" state version and a "Packed" state version. The player only ever has the Packed version in their inventory. Generally the Deployed-state Item ID is the Packed-state Item ID + 1.

`State` (enum)
* Must be one of: `Deployed` or `Packed`

`DeployedStateItemID` (int)
* The Item ID of the Deployed-state version of the prefab. This is only set if you are editing a Packed-state item.

`PackedItemPrefabID` (int)
* The Item ID of the Packed-state item prefab. Only valid if you are editing a Deployed-state item. 
* This will allow the Deployed-state item to be disassembled, and the player receives this PackedItemPrefabID item.
* You have to actually set this on the Deployed-state item, setting this to the Packed-state item will do nothing.

`AutoTake` (true/false)
* Whether or not there is an animation when picked up (true = no animation)

`CantDeployInNoBedZones` (true/false)
* Whether or not this item can deploy in "no bed zones"

`CastAnim` (enum)
* The animation when deploying or packing
* Can be any value off [this list](Resources/Types/enums/Character.SpellCastType.txt).

`DeploymentDirection` (Vector3)
* A Vector3 (has `x`, `y` and `z` values) representing the direction that the deployed will be in.

`DeploymentOffset` (Vector3)
* The offset position of the deployed item prefab

`DeploymentSound` (enum)
* The sound played when deployed
* Can be any value off [this list](Resources/Types/enums/GlobalAudioManager.Sounds.txt)

`DisassembleOffset` (Vector3)
* The offset position when disassembling the item

`DisassembleSound` (enum)
* The sound played when disassembled
* Can be any value off [this list](Resources/Types/enums/GlobalAudioManager.Sounds.txt)

## SL_ItemAddOn
SL_ItemAddOn inherits from SL_Deployable, and contains a few extra fields.

This class is for deployable which snap-on to another deployable, such as Alchemy Kit or Cooking Pot.

`AddOnCompatibleItemID` (int)
* The Item ID which this item can be deployed onto.

`AddOnStatePrefabItemID` (int)
* The Item ID of the deployed-state version of this item.

`SnappingRadius` (float)
* The radius which this item will snap-on to the compatible item when being deployed.

## SL_DestroyOnOwnerDeath

This class has no extra fields, it just destroys the Item if the Character dies.

## SL_Ephemeral

Ephemeral components give a lifespan to an Item, after which they are destroyed.

`Lifespan` (float)
* The time, in seconds, after which this item is destroyed.

## SL_MultipleUsage

This is like a more advanced version of the IsUsable and QtyRemovedOnUse fields on SL_Item. It allows a finer degree of control.

`AppliedOnPrice` (true/false)
* Whether each item of the stack applies to the total stack price

`AppliedOnWeight` (true/false)
* Whether each item of the stack applies to the total stack weight

`AutoStack` (true/false)
* Whether multiple items of the same type automatically stack together

`MaxStackAmount` (integer)
* The maximum stack count before making a new stack

## SL_Perishable

Perishable components make the item lose durability over time. Used by Food and Torches/Lanterns.

`BaseDepletionRate` (float)
* The base value used to reduce durability by

`DisableInInventory` (true/false)
* Whether this item perishes while in the inventory or not

`DontPerishInWorld` (true/false)
* Whether this item perishes when placed on the ground in the world

`DontPerishSkipTime` (true/false)
* Whether this item perishes when time is skipped (traveling, sleeping, etc)

`OverrideUpdateRate` (true/false)
* If set to a value above 0, this overrides the rate at which the BaseDepletionRate is applied. Otherwise it's every 5 seconds (multiplied against Environment Time delta).

## SL_Preserver

Preserver is used for Bags, and it slows the rate of Perishable items inside them.

`NullifyPerish` (true/false)
* Whether to completely prevent items perishing inside this bag

`PreservedElements` (list of SL_PreservedElement)
* See below

### SL_PreservedElement
The `PreservedElements` field on SL_Preserver is a list of SL_PreservedElement objects. Each of these sets the preservation amount for a certain Item Tag.

`Preservation` (float)
* The amount of preservation %, from 0 to 100.

`PreservedItemTag` (string)
* The Item Tag for this preservation value. Generally just "Food", but you could also use "Equipment", or something else.

Example:
```xml
<PreservedElements>
    <SL_PreservedElement>
        <Preservation>75</Preservation>
        <PreservedItemTag>Food</PreservedItemTag>
    </SL_PreservedElement>
</PreservedElements>
```

## SL_Sleepable

Used for tents and bedrolls, allows the player to sleep in it. Must be on a Deployed-state SL_Deployable item.

`AffectFoodDrink` (true/false)
* Whether sleeping in this tent affects the players food and drink values

`AmbushReduction` (int)
* The reduction to the ambush chance from sleeping in this item

`Capacity` (int)
* The amount of players that can sleep in this item. Not sure if going above 1 is supported.

`CharAnimOffset` (Vector3)
* Offset position of the player when interacting with the item.

`ColdProtection` (int)
* Protection against Cold weather when sleeping

`CorruptionDamage` (int)
* Corruption received after sleeping (not per hour), from 0 to 1000.

`HealthRecuperationModifier` (int)
* Percent modifier placed on the rate at which the player restores health (not burned health)

`HeatProtection` (int)
* Protection against Hot weather when sleeping

`IsInnsBed` (true/false)
* Is this a bed inside an Inn?

`ManaPreservationModifier` (int)
* Percent modifier placed on the rate at which the player restores mana while sleeping

`Rejuvenate` (true/false)
* Does the player restore all food and drink in this bed?

`StaminaRecuperationModifier` (int)
* Percent modifier placed on the rate at which the player restores stamina while sleeping

## SL_WeaponCharger

Used for Bows to draw-back the weapon.

`ChargingStaminaCost` (float)
* Stamina cost per tick while drawing the bow (tick every 0.5 or 1 seconds?)

## SL_WeaponLoadout

Used by pistols and bows, it handles the ammunition for the weapon.

`AmmunitionType` (enum)
* Must be one of: `Item` or `WeaponType`
* Determines whether the `CompatibleItemID` (Item) or the `CompatibleEquipmentType` (WeaponType) is used to find compatible ammunition.

`CompatibleItemID` (int)
* If AmmunitionType is Item, this is the compatible Item Id.

`CompatibleEquipmentType` (enum)
* If AmmunitionType is WeaponType, this is the compatible weapon type.
* Must be one of [these values](Resources/Types/enums/Weapon.WeaponType.txt)

`MaxProjectileLoaded` (int)
* Maximum projectile shots which can be loaded, before needing to reload.

`SaveRemainingShots` (true/false)
* Whether to save any remaining shots in the weapon when we reload.

`ShowLoadedAmmunition` (true/false)
* Whether to visibly show the loaded ammunition for the weapon

## SL_WeaponLoadoutItem

SL_WeaponLoadoutItem inherits from SL_WeaponLoadout, and contains a few extra fields.

`ReduceAmmunitionOnLoad` (true/false)
* Whether to reduce the ammunition when the weapon is loaded or not.
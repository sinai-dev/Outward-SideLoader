# SL_EffectCondition

An <b>SL_EffectCondition</b> object can be defined on an [Effect Transform](Effect-Transforms). 

## How to define one

When defining an SL_EffectCondition, simply define them like any other Xml object inside the `EffectConditions` list of the EffectTransform.

```xml
<!-- replace 'EffectConditionType' with the actual effect type -->
<SL_EffectCondition xsi:type="EffectConditionType">
    <!-- put fields and values for this effect condition here -->
</SL_EffectCondition>
```

## SL_EffectCondition Base Fields

The only base field which is shared by all EffectConditions is:

`Invert` (true/false)
* If true, it will flip the result of the condition after evaluating.
* Used if your condition produces the opposite behaviour than intended.
* Also used by "XOR" (exclusive-or, or mutually-exclusive) conditions, with one inverted and one not inverted.

# SL_EffectCondition Subclasses

These are all the subclasses of SL_EffectCondition. They all contain the Invert field above.

## SL_AngleFwdToTargetFwdCondition
Used by the Backstab skill. Compares your forward-angle to the target's forward-angle.

`AnglesToCompare` (List of Vector2)
* A list of valid angles.
* For backstab, it uses one Vector2: `<x>100</x> <y>190</y>`.

## SL_AttackTypeCondition
Checks the last Attack ID used by the casting character.

`AffectOnAttackIDs` (list of integer)
* A list of integer (`int`) values, corresponding to accepted Attack IDs.
* Attack IDs are between 0 and 5.

## SL_BooleanCondition
I'll be honest, I have no idea how this one works. If you want to figure it out, let me know if you do.

`Valid` (true/false)
* Is it valid...?

## SL_ContainedWaterCondition
A condition used to check the type of water in a waterskin, or any waterskin maybe?

`ValidWaterType` (enum)
* The water type to check.
* Must be exactly one of: `Clean`, `Fresh`, `Salt`, `Rancid` or `Magic`.

## SL_CorruptionLevelCondition

Requires a certain level of Corruption on the character.

`Value` (float)
* Between 0 and 1000 (1000 = 100% Corruption).

`CompareType` (enum)
* Determines the way the value is checked
* Must be one of: `Equal`, `GEqual`, `Greater`, `LEqual`, `Less`, `NotEqual`

## SL_CounterSuccessCondition
<b>Note: Must be applied to an `SL_CounterSkill`</b>.

Checks if the last counter on this CounterSkill was a success, if so it evaluates to true.

## SL_DealtDamageCondition
Checks if the damage dealt contains a certain type.

`RequiredDamages` (list of SL_Damage)
* The valid damage types to check for
* Determines the damage types and minimum values
* This value is a list of `SL_Damage` objects. Each SL_Damage contains `Type` and a `Damage` value. See documentation on PunctualDamage or Weapons for more details.

## SL_DelayEffectCondition
Not a real condition check, just adds delay to the effects.

`Delay` (float)
* The delay. Value-unit depends on the DelayType you set.

`DelayType` (enum)
* The type of delay.
* Must be exactly one of: `GameTimeHour` or `RealTimeSecond`.

## SL_EquipDurabilityCondition
Checks an equipment slot, and checks if it has enough durability.

`EquipmentSlot` (enum)
* The equipment slot to check.
* Must be exactly one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSlot.EquipmentSlotIDs.txt).

`MinimumDurability` (float)
* The minimum durability value (flat).

## SL_HasQuantityItemsCondition
Checks if the caster owns a total number of items (any item).

`TotalItemsRequired` (integer)
* The total items required.

## SL_HasStatusEffectEffectCondition
Checks if the caster has a Status Effect or not.

`DiseaseAge` (float)
* If checking for a disease, the minimum age of the disease.

`CheckOwner` (true/false)
* Check the owner?

`StatusSelectorType` (enum)
* Determines how your `SelectorValue` is used.
* Must be exactly one of: `StatusSpecific` (Status Identifier), `StatusType` (Status Tag) or `StatusFamily` (Status Family UID).

`SelectorValue` (string)
* Depends on your StatusSelectorType
* If `StatusSpecific`, checks for a Status Identifier of this name
* If `StatusType`, checks for a status with this Tag
* If `StatusFamily`, checks for a status family with this UID

## SL_HasStatusLevelCondition
Similar to HasStatusEffectEffectCondition, but checks for a Level-Status Effect (like Alertness).

`StatusIdentifier` (string)
* The Status Identifier name to check for

`CompareLevel` (integer)
* The level required

`CheckOwner` (true/false)
* Whether to check on the owner or not

`ComparisonType` (enum)
* The behaviour for the comparison evaluation
* Must be exactly one of: `Equal`, `Greater`, `Less`, `NotEqual`, `GEqual` or `LEqual`.

## SL_HeightCondition
Checks the height of the character (relative to nearest ground? or just in general? I don't know).

`AllowEqual` (bool)
* Allow == and not just >= ?

`CompareType` (enum)
* The behaviour of the equality check
* Must be exactly one of: `Higher` or `Lower`

`HeightThreshold` (float)
* The height requirement. Don't know if relative or just based on "sea level".

## SL_ImbueEffectCondition
Checks if the caster has an imbue.

`ImbuePresetID` (integer)
* The Preset ID of the Imbue required.

`AnyImbue` (true/false)
* Will any imbue work, or only the one with the PresetID set?

`WeaponToCheck` (enum)
* Which slot to check?
* Must be exactly one of: `MainHand` or `OffHand`

## SL_ImbueEffectORCondition
Same as ImbueEffectCondition, but allows you to check from a list of valid Imbues, and not just one.

It is exactly the same, but instead of `ImbuePresetID` you have `ImbuePresetIDs` (a list of integer).

## SL_InZoneCondition
I'll be honest, I don't really know how this one works. It might not actually be a real condition that is used.

`Radius` (float)
* Radius of zone to check? Based on what?

## SL_IsEquipSlotFilledCondition
Checks if the given EquipmentSlot has something equipped or not.

`EquipmentSlot` (enum)
* The slot to check.
* Must be one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSlot.EquipmentSlotIDs.txt).

## SL_IsWorldHostCondition
Self-explanatory condition. Is the caster the world host or not? This condition has no extra fields.

## SL_MostRecentCondition
Used by Runic spells to check which of two status effects is most recent.

`StatusIdentifierToCheck` (string)
* The identifier of the status which should be more recent

`StatusIdentifierToCompareTo` (string)
* The identifier of the status you are comparing to (which should be older)

## SL_OwnsItemCondition
Checks if the caster owns an amount of a specific Item ID.

`ReqItemID` (integer)
* The Item ID required

`ReqAmount` (integer)
* Required amount of this item

## SL_PassiveSkillCondition
Checks if the caster has a passive skill of this Item ID.

`ReqSkillID` (integer)
* The required passive skill ID.

## SL_ProbabilityCondition
A random condition. Has a chance to evaluate true or false.

`ChancePercent` (integer)
* Chance from 0 to 100 to evaluate to true.
* Eg, if 100, it will always be true, and if it is 0 and will always be false. 50 would be 50/50 true/false.

## SL_ProximityCondition
Checks for proximity to required Item IDs.

`RequiredItems` (list of SL_Skill.SkillItemReq objects)
* Each `<SkillItemReq>` has: `ItemID` (integer, Item ID), `Consume` (true/false, item is consumed?) and `Quantity` (integer, quantity required).

## SL_ProximitySoulSpotCondition
Inherits from SL_ProximityCondition, and contains one extra field. This is for spells which require a Soul Spot nearby.

`Distance` (float)
* Maximum distance from soulspot.

## SL_QuestEventAreaCondition
Checks the current scene for quest event(s).

`EventUIDs` (list of string)
* A list of `<string>` Event UIDs. These are the UIDs to check for.

## SL_ShooterPosStatusEffect
I'm not really sure. It's used by Rune spells.

`StatusIdentifier` (string)
* The identifier of the status effect to check for.
* If true, overrides the Shooter cast position to the position of the status effect...?

## SL_StatusEffectCondition
Checks if the caster has a given status effect.

`StatusIdentifier` (string)
* Identifier name of the status to check for.

## SL_TimeDelayCondition
Not sure how this is a condition, but it delays the effect like a DelayCondition.

`DelayRange` (Vector2)
* x/y range for the delay.

`TimeFormat` (enum)
* The format of the delay time.
* Must be exactly one of: `GameHours` or `Seconds`

`IgnoreFirstCheck` (true/false)
* Whether to ignore the first check or not

## SL_WeaponIsLoadedCondition
Checks if the weapon in the given slot is loaded or not.

`SlotToCheck` (enum)
* The Weapon Slot to check.
* Must be exactly one of: `MainHand` or `OffHand`.
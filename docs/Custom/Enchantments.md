# Custom Enchantments

<b>Custom Enchantments</b> (`SL_EnchantmentRecipe`s) can be defined from XML or C#. You can edit existing Enchantments by overwriting them with a Custom Enchantment, or you can create a completely new one.

Enchantment XMLs should be placed in the `Enchantments\` sub-folder of your SL Pack.

SideLoader does not currently support EnchantmentRecipeItems (recipe scrolls), but this will be added at some point. In the mean time, making one with C# would be fairly trivial (clone an existing scroll and change the EnchantmentRecipeItem.Recipes field). You would need to use C# anyway if you wanted to add this scroll to merchant stock, loot, etc.

## Important Note

Unlike SL_Item and SL_StatusEffect templates, you cannot leave out fields on a SL_EnchantmentRecipe template, even if you are editing an existing Enchantment.

SideLoader does not clone from an existing Enchantment when it applies your template, so the values you set will define everything about the Enchantment.

## Generating A Template

You can generate a template from an existing Enchantment, either to edit it or to make a new one based off of it.

See [SideLoader Menu](GettingStarted/SLMenu.md) for details on how to dump Enchantments.

## SL_Enchantment Fields

`EnchantmentID` (int)
* The ID of this Enchantment Recipe and result.
* Can be an existing ID, or a new one.

`Name` (string)
* The name of this Enchantment.

`Description` (string)
* Description (displayed when inspecting the <b>applied</b> enchantment). Not all Enchantments use this.

`IncenseItemID` (int)
* The Item ID for the Incense required by this recipe.

The `PillarDatas` field is list of PillarData objects.

Each PillarData has:
* `Direction` (enum): one of `North`, `South`, `East` or `West`
* `IsFar` (true/false): Whether this is a "Far" pillar or not (otherwise "Close").

In Xml, it would look like this:
```xml
<PillarDatas>
  <PillarData>
    <Direction>North</Direction>
    <IsFar>true</IsFar>
  </PillarData>
  <!-- etc, up to 4 -->
</PillarDatas>
```

The `CompatibleEquipment` contains information about the compatible equipment.

* `RequiredTag` (string): The <b>REQUIRED</b> Tag for Equipment with this Enchantment. Eg, "Weapon", "Armor", "Helmet", "Boots". Can pick any Tag off [this list of Tags](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1840819680).
* `Equipments` (list of IngredientData): The optional list of compatible Equipments, either by Tag or Specific Item ID. If not set, any item with RequiredTag will work.

Each IngredientData has:
* `SelectorType` (enum): One of: `SpecificItem` (Item ID) or `Tag` (Item Tag). Determines how the game uses the SelectorValue.
* `SelectorValue` (string): Depends on the SelectorType. If SpecificItem, this should be an <b>Item ID</b>. If Tag, this should be a Tag name.

In Xml, the entire CompatibleEquipment should look something like this:

```xml
<CompatibleEquipment>
  <Equipments>
    <IngredientData>
      <SelectorType>Tag</SelectorType>
      <SelectorValue>Palladium</SelectorValue>
    </IngredientData>
  </Equipments>
  <RequiredTag>Weapon</RequiredTag>
</CompatibleEquipment>
```

`TimeOfDay` (list of Vector2)
* A list of Vector2 (`x` and `y`) which represents valid times during the day for this recipe.
* The values of the Vector2 correspond to the hours of the day (0 to 24).
* For example, a value of  `x: 5, y: 15` would mean that the any time between 5am and 3pm would succeed.

In Xml, the TimeOfDay should look something like this:
```xml
<TimeOfDay>
  <Vector2>
    <x>18</x> <!-- 6 PM -->
    <y>5</y>  <!-- 5 AM -->
  </Vector2>
</TimeOfDay>
```

`Areas` (list of enum)
* A list of AreaEnum objects, corresponding to areas where this recipe is valid.
* You can pick any value off [this list](https://github.com/sinaioutlander/Outward-SideLoader/tree/master/Resources/Types/enums/AreaManager.AreaEnum.txt).

In Xml, this would look like:
```xml
<Areas>
  <AreaEnum>Emercar</AreaEnum>
</Areas>
```

The `WeatherConditions` field is a list of WeatherCondition objects. Each of these has:
* `WeatherType` (enum): One of: `Clear`, `Rain`, `Snow` or `SeasonEffect`.
* `Invert` (true/false): Like the Invert field on SL_EffectCondition, this flips the result of the evaluation.

In Xml, this might look like:
```xml
<WeatherConditions>
  <WeatherCondition>
    <Invert>false</Invert>
    <WeatherType>SeasonEffect</WeatherType>
  </WeatherCondition>
</WeatherConditions>
```

The `Temperature` is a list of TemperatureStep objects, corresponding to valid Environment temperatures for this recipe.
* Each TemperatureStep can be either: `Coldest`, `VeryCold`, `Cold`, `Fresh`, `Neutral`, `Warm`, `Hot`, `VeryHot`, `Hottest`.

For example, in Xml:
```xml
<Temperature>
  <TemperatureSteps>Cold</TemperatureSteps>
  <TemperatureSteps>VeryCold</TemperatureSteps>
  <TemperatureSteps>Coldest</TemperatureSteps>
</Temperature>
```

`WindAltarActivated` (true/false)
* Whether or not the Wind Altar needs to be activated in the current region for this recipe.

`EnchantTime` (float)
* The time (in seconds) that this recipe takes to actually enchant.

`Effects` (list of SL_EffectTransform)
* Like on Items and Status Effects, this is a container for the Effects.
* Enchantments generally only use this for On-Hit effects, but it could probably be used for anything.
* See [the Effects article](Effects/EffectTransforms.md) for more information on how to set this up.

The `AddedDamages` is a list of AdditionalDamage objects. Each AdditionalDamage has:
* `AddedDamageType` (enum): The type of damage that will be added. One of `Physical`, `Ethereal`, `Decay`, `Electric`, `Frost`, `Fire` or `Raw`.
* `SourceDamageType` (enum): The existing type of damage to add from. Same values as AddedDamageType.
* `ConversionRatio` (float): The amount of SourceDamageType which is added as AddedDamageType. For example, 0.25 would add 25% of the Source type as Added type, 2.0 would be 200%, etc.

In Xml, this would look like:
```xml
<AddedDamages>
  <AdditionalDamage>
    <AddedDamageType>Physical</AddedDamageType>
    <SourceDamageType>Physical</SourceDamageType>
    <ConversionRatio>0.8</ConversionRatio>
  </AdditionalDamage>
</AddedDamages>
```

The `StatModifications` is a list of StatModification objects. Each StatModification has:
* `Stat` (enum): Pick any value off [this list](https://github.com/sinaioutlander/Outward-SideLoader/tree/master/Resources/Types/enums/Enchantment.Stat.txt).
* `Type` (enum): How to add this stat. Must be one of `Bonus` or `Modifier`
* `Value` (float): The value applied to this stat.

In Xml, this would look like:
```xml
<StatModifications>
  <StatModification>
    <Stat>MovementSpeed</Stat>
    <Type>Modifier</Type>
    <Value>3</Value>
  </StatModification>
</StatModifications>
```

`FlatDamageAdded` (list of SL_Damage)
* This is flat damage, simply added directly to the weapon's damage.

In xml, the FlatDamageAdded should look something like this:
```xml
<FlatDamageAdded>
  <SL_Damage>
    <Damage>20</Damage>
    <Type>Physical</Type>
  </SL_Damage>
  <!-- can add other types -->
</FlatDamageAdded>
```

`DamageModifierBonus` (list of SL_Damage)
* This is defined the same way as FlatDamageAdded, however this adds Damage Modifier Bonus to the equipment.
* The "Damage" field is actually the value added to the corresponding Damage Bonus stats for that element, it's not literally flat damage.
* Looks the same as FlatDamageAdded, other than the field name.

`DamageResistanceBonus` (list of SL_Damage)
* Again, this is the same as the previous two fields, however this adds Damage Resistance stat.
* The "Damage" field is added to your Damage Resistance for that corresponding damage type.

`HealthAbsorbRatio` (float)
* Used by Vampiric weapons, the amount of Health leeched based on damage dealt.

`ManaAbsorbRatio` (float)
* Like "The Good Moon" enchantment, this will leech Mana based on damage dealt.

`StaminaAbsorbRatio` (float)
* Not actually something that is used by any existing enchantment, but it does work. Leeches Stamina.

`Indestructible` (true/false)
* Makes the equipment indestructible if true.

`TrackDamageRatio` (true/false)
* Used by Vampiric weapons (for the Vampiric Transmutation, the required damage dealt).

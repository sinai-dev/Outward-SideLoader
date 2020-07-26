# SL_EffectTransforms

SideLoader has support for <b>Custom Effects</b> with [Custom Items](Custom/Items), [Custom Skills](Custom/Skills) and [Custom Status Effects](Custom/StatusEffects), etc.

An <b>SL_EffectTransform</b> is a container for these effects, as well as Effect Conditions. Each Transform represents a group of effects that are applied at the same time and with the same conditions.

## Before We Start
Effects in Outward are complex. This is by far the most advanced part of SideLoader and it may take you a little time to get how this works, especially if you are not familiar with Unity.

I recommend using the [SideLoader Menu](GettingStarted/SLMenu) to dump some existing items, skills and statuses to see how they work.

## Edit Behaviour
`EffectBehaviour` is a setting found on various SideLoader templates such as SL_Item and SL_StatusEffect. It determines <b>how your template effects are applied</b>. This is referring to when SideLoader actually applies this template, not when the effects are applied in game.
* `NONE` will <b>leave the existing effects untouched</b>. Use NONE if you don't want to modify the existing effects at all, or you want to add on-top of them.
* `DestroyEffects` will <b>wipe all the existing effects</b> before it applies yours. This is recommended in most cases if you are editing the effects.
* `OverrideEffects` will <b>only destroy transforms if you have defined one with the same TransformName</b>. You can use this to have greater control and only replace specific transforms. <b>NOTE:</b> In some cases, it is possible that there are duplicate-name children on one transform, and SideLoader has no way to solve this. You would need to use DestroyEffects and replace everything in this case.

<b>NOTE:</b> Even if you don't define any SL_EffectTransforms, your EffectBehaviour is <b>still applied</b> to the base prefab.

## SL_EffectTransform
As mentioned above, an SL_EffectTransform is a container for effects and conditions. When defining your Effects on items, skills or status effects, you are defining a <b>list of SL_EffectTransforms</b>.

The fields on an SL_EffectTransform are:

* `TransformName` - Name of this transform, determines the type of effects
* `Effects` - List of `SL_Effect` objects
* `EffectConditions` - Optional list of `SL_EffectCondition` objects
* `ChildEffects` - Optional nested `SL_EffectTransform` objects

### TransformName

The <b>TransformName</b> is very important, the game uses this to know when to apply the Effects on this transform. You can also override the TransformName with the `OverrideCategory` of your [SL_Effect](Effects/SL_Effect).

The game checks if your TransformName <b>contains</b> one of the following <b>keywords</b>, in this order:

`Normal`
* For standard activation effects, ie. using the item or activating the skill
* Unlike `Activation`, these effects happen during the animation at a certain point depending on the animation

`Hit`
* Used for on-hit effects for weapons and skills

`Passive`
* For passive effects. 

`Activation`
* Used inmediately on activation, before the animation begins.

`Block`
* used for skills that have on-block effects

There is one unique exception: if the TransformName is <b>exactly</b> "`Effects`", "`Effect`", "`ExtraEffects`" or "`HiddenEffects`", it works the same as the `Normal` keyword.

This covers everything about the SL_EffectTransform itself, see the linked articles below for details on SL_Effect and SL_EffectCondition classes.

## Effects and Conditions

* [SL_Effect](Effects/SL_Effect), defined in the `Effects` field.
* [SL_EffectCondition](Effects/SL_EffectCondition), defined in the `EffectConditions` field.

## Xml Example
The base XML structure for an SL_EffectTransform should look like this:
```xml
<SL_EffectTransform>
  <!-- The TransformName determines the category of effects. -->
  <!-- HitEffects is a common category, used by weapons, skills and projectiles -->
  <TransformName>HitEffects</TransformName> 
  <Effects> 
    <!-- Any number of SL_Effect objects can be defined here -->
    <SL_Effect xsi:type="Effect Type Here">
      <!-- Fields for this effect go here -->
    </SL_Effect>
  </Effects>
  <EffectConditions>
    <!-- SL_EffectCondition objects can be defined here -->
    <SL_EffectCondition xsi:type="Condition Type Here">
      <!-- ... -->
    </SL_EffectCondition>
  </EffectConditions>
  <ChildEffects>
    <!-- generally you don't need Child Effects for most things, but the game sometimes uses them -->
  </ChildEffects>
</SL_EffectTransform>
```
# SL_Effect

An <b>SL_Effect</b> object can be defined on an [Effect Transform](Effects/EffectTransforms). 

## How to define one

When defining an SL_Effect, simply define them like any other Xml object inside the `Effects` list of the EffectTransform.

```xml
<!-- replace 'EffectType' with the actual effect type -->
<SL_Effect xsi:type="EffectType">
    <!-- put fields and values for this effect here -->
</SL_Effect>
```

For example:

```xml
<SL_Effect xsi:type="SL_AddStatusEffectBuildUp">
    <StatusEffect>Doom</StatusEffect>
    <Buildup>60</Buildup>
</SL_Effect>
```
## SL_Effect Base Fields

<b>All SL_Effect classes have these fields.</b>

`Delay` (float)
* The time, in seconds, after which the effects will be applied. Default is 0.

`SyncType` (enum)
* Must be exactly one of: `OwnerSync`, `MasterSync` or `Everyone`
* <b>OwnerSync</b> means only the owner of the effect (person activating it) will execute the effects
* <b>MasterSync</b> means only the host character will execute it
* <b>Everyone</b> means everyone will execute the effect

`OverrideCategory` (enum)
* Overrides the TransformName to a specific effect category
* Must be exactly one of: `None`, `Normal`, `Activation`, `Restauration`, `Hit`, `Reference` or `Block`
* See "TransformName" on the [Effect Transforms](Effects/EffectTransforms) article for more information.

# SL_Effect Subclasses

Below are all the subclasses of SL_Effect. These contain the 3 fields above, as well as extra fields.

Note: There are a lot of subclasses. Use Ctrl+F to find the effects you are interested in.

## SL_AddStatusEffect
Used for simply adding a status effect on activation.

`StatusEffect` (string)
* Must use a <b>Status Identifier</b>, not the actual name of the status effect.
* For a list of Statuses listed by their Identifier, see [this Google Sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1969601658)
* For example, for Extreme Bleeding, the Identifier is `Bleeding +`.

`ChanceToContract` (float)
* Usually this is `100` for 100%, but it can be between 0 and 100.

## SL_AddStatusEffectRandom
Used by Jinx to apply a random Status Effect from a list of possible values.

`StatusIdentifiers` (list of string)
* The list of Status identifiers to choose from
* Each value is a `<string>IdentifierName</string>`

`ForceID` (int)
* If you want to override the result to a certain value, set this
* This is the index of the StatusIdentifiers list (starting at 0) to choose.

## SL_AddBoonEffect
This is used for Boons, for the Amplified Effect (with Shamanic Resonance).

It's exactly the same as `SL_AddStatusEffect`, except that you have one additional field.

`AmplifiedEffect`
* The Status Identifier for the effect received when you have Shamanic Resonance
* The game just adds ` Amplified` to the identifier for most boons, eg `Bless Amplified`

## SL_AddStatusEffectBuildUp
For adding a build-up effect, usually used by Hit effects.

`StatusEffect` (string)
* Same as StatusEffect on SL_AddStatusEffect

`Buildup` (float)
* The effect build-up value, between 0 and 100.

## SL_AffectBurntHealth
For affecting burnt health.

`AffectQuantity` (float)
* How much burnt health is affected

`IsModifier` (true/false)
* Should this be applied as a modifier (multiplier) value?

## SL_AffectBurntStamina
For affecting burnt stamina.

`AffectQuantity` (float)
* How much burnt stamina is affected

`IsModifier` (true/false)
* Should this be applied as a modifier (multiplier) value?

## SL_AffectCorruption
For affecting the character's Corruption stat (The Soroboreans DLC).

`AffectQuantity` (float)
* The amount corruption is affected by

`IsRaw` (true/false)
* Is this value "Raw"?

## SL_AffectDrink
For affecting the player's Thirst/Drink stat.

`AffectQuantity` (float)
* The amount the player's thirst is affected by

## SL_AffectFatigue
For affecting the player's Fatigue (Rest) stat.

* `AffectQuantity` (float)
* The amount the player's fatigue is affected by.

## SL_AffectFood
For affecting the player's hunger (Food) stat.

`AffectQuantity` (float)
* The amount the player's hunger is affected by.

## SL_AffectHealth
For restoring or affecting current Health.

`AffectQuantity` (float)
* How much current health is affected

`IsModifier` (true/false)
* Should this be applied as a modifier (multiplier) value?

`AffectQuantityOnAI`
* If you want a separate value for AI, set that here. Otherwise set to -1.

## SL_AffectHealthParentOwner
For healing the "owner" of the effect. Used by Blood Bullet, for example.

`AffectQuantity` (float)
* How much current health is affected

`IsModifier` (true/false)
* Should this be applied as a modifier (multiplier) value?

`Requires_AffectedChar` (true/false)
* Does it require a successful affected character (ie. on-hit effect, etc)?

## SL_AffectMana
For affecting current Mana.

`AffectQuantity` (float)
* How much current mana is affected

`IsModifier` (true/false)
* Should this be applied as a modifier (multiplier) value?

`AffectType` (enum)
* Determines whether it restores or uses mana.
* One of: `Restaure` or `Use`

## SL_AffectStability
For affecting the owner character's stability. Used by Juggernaught, for example.

`AffectQuantity` (float)
* How much current stability is affected

`IsModifier` (true/false)
* Should this be applied as a modifier (multiplier) value?

## SL_AffectStamina
For affecting current stamina.

`AffectQuantity` (float)
* How much current stamina is affected

## SL_AffectStat
For affecting a character's <b>Stat</b> attribute for the given `Stat_Tag`.

* See [this google sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1840819680) for a list of tags.
* Stat Tags are between <b>76</b> to <b>125</b>.
* Don't include the number or hyphen, just the tag. Ie, just put "Weapon", not "1-Weapon".

`Stat_Tag` (string)
* The affected stat tag

`AffectQuantity` (float)
* The quantity the stat is affected by

`IsModifier` (true/false)
* Should this be applied as a modifier (multiplier) value?

## SL_AutoKnock
This effect is quite simple, it just guarantees a stagger or knockdown on the affected character. Use HitEffects to apply to enemies.

`KnockDown` (true/false)
* Should the affected character be knocked down, or just staggered?

## SL_CallSquadMembers
This class uses no fields, other than the base SL_Effect fields.

This is used for AI (non-player) enemies to call over their squad members, it has no effect when used by a player character.

## SL_Cough
Used by the "Cold" disease, interrupts the player's actions. 

`ChanceToTrigger` (integer)
* Value from 0 to 100, what is the general chance for the Cough to trigger when this effect is activated?

## SL_CreateItemEffect
Used to add an item to the caster's inventory.

`ItemToCreate` (int)
* The Item ID to create

`Quantity` (int)
* Quantity of the item received

## SL_CureDisease
This effect inherits from `SL_RemoveStatusEffect`. There are no custom fields on this effect other than the ones inherited from SL_RemoveStatusEffect, it's just that the game knows you are removing a Disease instead of a regular Status Effect.

## SL_Death
Another simple effect, the name tells you what happens. Guaranteed death.

## SL_DetachParasite
Used by enemy AIs. Probably related to some DLC content.

## SL_GiveOrder
Used by enemy AIs, related to DLC content.

`OrderID` (integer)
* The "order" given to AI squad members.

## SL_ImbueWeapon
For adding an <b>Imbue Preset</b> to a character.

`Lifespan` (float)
* Lifespan of the imbue from this source

`ImbueEffect_Preset_ID` (integer)
* The ID of the Imbue Preset you want to apply.
* You can find preset IDs with [this google sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1969601658) - use the "ID" of the effect.

`Imbue_Slot` (enum)
* Must be exactly one of: `MainHand` or `OffHand` 
* The slot which this imbue preset will be applied to.

## SL_LightLantern
This effect lights or un-lights your lantern, if equipped.

`Light` (true/false)
* Should this light the lantern? `false` would un-light the lantern.

## SL_LoadWeapon
This effects loads a weapon (ie. Pistol).

`UnloadFirst` (true/false)
* Should it unload the weapon before loading?

`WeaponSlot` (enum)
* Which slot to affect. Must be exactly one of: `MainHand` or `OffHand`

## SL_PlaySoundEffect
This one is obviously for playing a sound effect. You can pick any sound from the global list.

`Sound` (enum)
* Pick any sound of [this list](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/GlobalAudioManager.Sounds.txt).

`Follow` (true/false)
* Should the sound follow the affected character?

`MinPitch` (float)
* Minimum pitch (from distance)

`MaxPitch` (float)
* Maximum pitch (from distance)

## SL_Puke
Similar to SL_Cough, used by the Indigestion disease.

`ChanceToTrigger` (integer)
* The chance for the Puke to trigger when this effect is activated.

## SL_PunctualDamage
PunctualDamage is a type of damage applied by skills or effects. It requires no weapon.

The `Damage` is a list of SL_Damage objects. This is similar to the `BaseDamage` field on a SL_Weapon template.
* Each entry has a `Damage` and `Type` value.
* For the "Type", you must set one of: `Physical`, `Ethereal`, `Decay`, `Electric` (<b>not</b> Lightning), `Frost`, or `Fire`.

The XML should look like:
```xml
<Damage>
  <SL_Damage>
    <Damage>20</Damage>
    <Type>Physical</Type>
  </SL_Damage>
  <!-- etc... -->
</Damage>
```

`Damages_AI` (list of SL_Damage)
* The same as `Damage`, but its separate values applied to AI characters. You don't have to set this.

`Knockback` (float)
* The Impact damage of the effect

`HitInventory` (true/false)
* Does this effect damage the durability of items in the affected character's backpack?

## SL_PunctualDamageAoE
This class inherits from SL_PunctualDamage, and contains a few extra fields.

`Radius` (float)
* Radius of the AoE damage

`TargetType` (enum)
* Must be one of: `Allies` or `Enemies`

`IgnoreShooter` (true/false)
* Should the shooter be ignored?

## SL_ReduceDurability
Simple effect which reduces durability of some equipment.

`Durability` (float)
* How much durability is lost (flat amount)

`EquipmentSlot` (enum)
* Which slot to affect. Must be one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSlot.EquipmentSlotIDs.txt).

## SL_ReduceStatusLevel
For Status Effects which <b>levels</b> (eg the new Alertness status in the DLC).

`StatusIdentifierToReduce` (string)
* The <b>Identifier Name</b> of the Status Effect.
* See [this Google Sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1969601658) for a list of Status Effect Identifiers.

## SL_RemoveImbueEffects
Simply removes the Imbue from a given Equipment Slot.

`AffectSlot` (enum)
* Must be one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSlot.EquipmentSlotIDs.txt).

## SL_RemoveStatusEffect
Used to remove a status effect from the affected character.

You can either use the <b>Identifier Name</b> of the status, or a <b>Tag</b> that the status might have on it.

`Status_Name` (string)
* If you want to remove a specific status by the identifier, set this.
* For a list of Statuses listed by their Identifier, see [this Google Sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1969601658)

`Status_Tag` (string)
* If there's a tag you want to search for on the status and remove all statuses with that tag, set it here.
* See [this google sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1840819680) for a list of tags.

`CleanseType` (enum)
* Exactly one of: `StatusNameContains`, `StatusType`, `StatusFamily` or `StatusSpecific`
* I recommend using <b>StatusNameContains</b> and using the <b>Status_Name</b> field, or <b>StatusType</b> and using the <b>Status_Tag</b> field.

## SL_RunicBlade
This effect is uniquely used by the Runic Blade skill. However, it could be used for other purposes too.

`WeaponID` (integer)
* The Weapon ID which should be summoned for the player.

`GreaterWeaponID` (integer)
* If the player has Arcane Syntax, the weapon from the amplified version of the spell.

`PrefixImbueID` (integer)
* If the player has Runic Prefix, the Preset ID of the Imbue you want to give for the WeaponID weapon.

`PrefixGreaterImbueID` (integer)
* If the player has Runic Prefix, the Preset ID of the Imbue you want to give for the GreaterWeaponID weapon.

## SL_Shooter
The base class used by SL_ShootBlast and SL_ShootProjectile. You cannot use this class directly.

ShootBlast and ShootProjectile are rather complex effects, they are almost as involved as a Skill or Item by itself.

`CastPosition` (enum)
* Determines the cast-position type.
* Must be exactly one of: `Character`, `Given`, `Local`, `ProximitySkillItem`, `Target` or `Transform`.

`LocalPositionAdd` (Vector3)
* Adds position to the start-point of the shooter projectile or blast.
* A Vector3 has `x`, `y` and `z` offsets you can set.

`NoAim` (true/false)
* Force "no-aim" behaviour.

`TargetType` (enum)
* Determiens the target-type behaviour.
* Must be exactly one of: `Allies` or `Enemies`

## SL_ShootBlast
ShootBlast inherits from SL_Shooter, and contains some extra fields.

`BaseBlast` (enum)
* This determines the base visuals for your blast.
* You can pick any value from [this list](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/BlastPrefabs.txt) (there are a lot).

`Radius` (float)
* The radius of the blast. Generally between 1 to 5.

`RefreshTime` (float)
* How frequently the blast refreshes (re-applies damage/effects).

`BlastLifespan` (float)
* Lifespan of the blast, generally 0.1 for one-and-done blasts.

`InstantiatedAmount` (integer)
* Determines the <b>maximum copies of the blast</b> which can be active at once.
* Think about how many you would need (per caster) at any one time, and set to that.

`Interruptible` (true/false)
* Can the blast be interrupted?

`MaxHitTargetCount` (integer)
* Maximum targets that can be hit by the blast, if any.

`AffectHitTargetCenter` (true/false)
* Should the blast affect the target from their center position?

`HitOnShoot` (true/false)
* Should the hit effects be applied automatically on shoot?

`IgnoreShooter` (true/false)
* Should the shooter be ignored?

`IgnoreStop` (true/false)
* Should stopping be ignored?

`NoTargetForwardMultiplier` (float)
* If the shooter has no target, this increases the forward speed.

`ParentToShootTransform` (true/false)
* Determines the shooting behaviour, should the parent transform be the shooter?

`UseTargetCharacterPositionType` (true/false)
* Should the target character's position-type be used for the targeting mode?

`ImpactSoundMaterial` (enum)
* The sound on impact.
* Must be one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSoundMaterials.txt).

`DontPlayHitSound` (true/false)
* Whether to play the hit sound on hit

`FXIsWorld` (true/false)
* Are the effects "world effects"?

`EffectBehaviour` (enum)
* Determines the behaviour for your BlastEffects.
* Must be exactly one of: `OverrideEffects`, `DestroyEffects` or `NONE`.

`BlastEffects` (List<SL_EffectTransform>)
* List of actual effects on the blast.
* See [the Effects article](Effects/EffectTransforms) for more information.

## SL_ShootBlastHornetControl
* Derives from `SL_ShootBlast`.
* Used by Hive AI enemies. Dump one of the Hornet skills to see how this is used.

## SL_ShootBlastProximity
* Derives from `SL_ShootBlast`.
* Contains no extra fields, this works by having a `SL_ProximityCondition` EffectCondition on the same Effect Transform.

## SL_ShootEnchantmentBlast
Inherits from `SL_ShootBlast`.

Used for Enchantments which add an AoE on-hit blast. The Damage is based on the weapon used.

`DamageMultiplier` (float)
* The multiplier of the weapon damage, which determines the damage of the blast
* Generally between 0 and 1.

## SL_ShootProjectile
SL_ShootProjectile inherits from SL_Shooter. It is similar to ShootBlast, this shoots a projectile with a direction and velocity.

`BaseProjectile` (enum)
* This determines the base visuals for your projectile.
* You can pick any value from [this list](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/ProjectilePrefabs.txt) (there are a lot).

`InstantiatedAmount` (int)
* Determines how many projectiles are created on cast
* This limit affects the ProjectileShots.

### SL_ProjectileShot
`ProjectileShots` (list of SL_ProjectileShot)
* This is not always used, but some ShootProjectile effects make use of this.
* Determines unique behaviour for one or more individual projectile, when the effect is activated.

In Xml, it would look like:
```xml
<ProjectileShots>
  <SL_ProjectileShot>
    <!-- SL_ProjectileShot fields/values go here -->
  </SL_ProjectileShot>
</ProjectileShots>
```

The fields on SL_ProjectileShot are:

`LocalDirectionOffset` (Vector3)
* Custom direction offset for this projectile

`LockDirection` (Vector3)
* Custom locked direction for this projectile

`MustShoot` (true/false)
* Should this shot be forced to shoot?

`NoBaseDir` (true/false)
* Whether to override the base direction or not

`RandomLocalDirectionAdd` (Vector3)
* Optional random local direction added

### SL_ShootProjectile fields
Now, back to the fields on SL_ShootProjectile.

`Lifespan` (float)
* Maximum lifespan of the Projectile

`LateShootTime` (float)
* Delay on the projectile shot

`Unblockable` (true/false)
* Is the projectile <b>unblockable</b>?

`EffectsOnlyIfHitCharacter` (true/false)
* Only do effects if the projectile actually hit a targetable character?

`EndMode` (enum)
* Determines the end-mode behaviour
* Must be exactly one of: `LifetimeOnly`, `EnvironmentOnly`, `Normal` or `Custom`

`DisableOnHit` (true/false)
* Should the projectile be disabled once it hits something?

`IgnoreShooterCollision` (true/false)
* Should the shooter collision be ignored?

`TargetingMode` (enum)
* Determines the targeting mode for the projectile
* Must be exactly one of: `FindAllies`, `FindEnemies`, `IgnoreTarget`, `Owner`, `OwnerTarget`, `TargetChar` or `NONE`.

`TargetCountPerProjectile` (integer)
* Per projectile shot, how much targets can each projectile have?

`TargetRange` (float)
* Maximum target-acquisition range.

`AutoTarget` (true/false)
* Should the projectile acquire a target automatically? (No lock-on)

`AutoTargetMaxAngle` (float)
* Maximum angle for auto-targeting, if enabled

`AutoTargetRange` (float)
* Maximum range for auto-targeting, if enabled

`ProjectileForce` (float)
* Force applied to the projectile

`AddDirection` (Vector3)
* Optional extra direction added to projectile

`AddRotationForce` (Vector3)
* Optional extra rotation added to projectile

`YMagnitudeAffect` (float)
* Magnitude that the Y axis is affected by

`YMagnitudeForce` (float)
* Force applied to Y magnitude

`DefenseLength` (float)
* Length of defense (blocking) against this projectile?

`DefenseRange` (float)
* Range of blocking against this projectile?

`ImpactSoundMaterial` (enum)
* The sound on impact.
* Must be one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSoundMaterials.txt).

`LightIntensityFade` (Vector2)
* Fade applied to the Light on the projectile, if any.
* Only has an `x` and `y` value

`PointOffset` (Vector3)
* Offset of the Point-Light, if any.
* Also has a `z` value.

`TrailEnabled` (true/false)
* Is the trail enabled on the projectile (if any)?

`TrailTime` (float)
* If trail enabled and valid, the time of the trail.

`EffectBehaviour` (enum)
* Determines the behaviour of your ProjectileEffects.
* Must be exactly one of: `DestroyEffects`, `OverrideEffects` or `NONE`.

`ProjectileEffects` (list of SL_EffectTransform)
* List of actual effects on the projectile.
* See [the Effects article](Effects/EffectTransforms) for more information.

## SL_ShootProjectilePistol
Inherits from ShootProjectile, and is used for pistol skills.

`UseShot` (true/false)
* Should the loaded shot be used, if any?

## SL_StartDuel
This simple effect will change the affected character's faction to `Bandits`.

This is uniquely used by the Duel Baton item, and contains no fields.

## SL_Stun
Used by the Sweep Kick skill (if target has Confusion).

`Duration` (float)
* Duration of the stun.

## SL_Summon
This effect type is used by skills like Conjure (Summoned Ghost), but also for things like placing a Sigil on the ground, or casting a Runic Trap, etc.

`Prefab` (string)
* The Prefab to load. Depends on the SummonPrefabType that you set.
* If SummonPrefabType set to `Resource`, it will try to load the Prefab as a path in the Resources folder.
* If set to `Item`, it will assume the Prefab is an Item ID.

`SummonPrefabType` (enum)
* Determines whether the summoned object is from the Resources folder, or it's an Item.
* Must be exactly one of `Resource` or `Item`.

`BufferSize` (integer)
* How many can be summoned at once?
* Only valid if SummonMode is `Buffer`.

`LimitOfOne` (true/false)
* Can only one be active at a time?

`SummonMode` (enum)
* Determines some behaviour of the summoning prefab.
* Must be exactly one of: `SameObject`, `CreateNew` or `Buffer`.

`PositionType` (enum)
* Determines the positioning behaviour
* Must be exactly one of: `InFrontOfTarget`, `AroundTarget`, `ProximitySkillItem` or `ProximitySoulSpot`.

`MinDistance` (float)
* Minimum distance of summon from caster (only at time of cast)

`MaxDistance` (float)
* Max distance of summon from caster (only at time of cast)

`SameDirectionAsSummoner` (true/false)
* Summoned prefab has same facing direction as caster?

`SummonLocalForward` (Vector3)
* Optional extra position added to Summoned prefab

`IgnoreOnDestroy` (true/false)
* Does the effect ignore when prefab is destroyed?

## SL_SummonAI
This effect inherits from SL_Summon. It contains no extra fields, it just tells the game you are summoning an AI Character which will follow the caster.

## SL_SummonBloodSpirit
Like SL_SummonAI, but it tells the game you are summoning a "Blood Spirit" (probably DLC-related).

## SL_Teleport
Teleports the caster.

`MaxRange` (float)
* Maximum teleport range.

`MaxTargetRange` (float)
* If using target-teleport, maximum distance of target.

`MaxYDiff` (float)
* If using target-teleport, maximum Y (height) difference.

`OffsetRelativeTarget` (Vector3)
* Offset position to target

`UseTarget` (true/false)
* Do we teleport to the current target?

## SL_ThrowItem
<b>NOTE:</b> Must be on an `SL_ThrowSkill` (eg. cloned from Throw Lantern skill).

`CollisionBehaviour` (enum)
* Determines the collision behaviour
* Must be exactly one of: `Destroyed`, `None` or `Stuck`.

`ProjectileBehaviour` (enum)
* Determines how the projectile itself behaves
* Must be exactly one of: `RayCast` or `Physic`

## SL_UnloadWeapon
Simply unloads the weapon in the given AffectSlot.

`AffectSlot` (enum)
* The equipment slot to unlaod from.
* Must be exactly one of [these values](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/EquipmentSlot.EquipmentSlotIDs.txt).

## SL_UseLoadoutAmmunition
Uses ammunition from a WeaponLoadout.

`MainHand` (true/false)
* Should we affect the main-hand item, or off-hand?

## SL_VitalityDamage
Damage dealt directly to the affected character's health, <b>cannot be resisted</b>.

`PercentOfMax` (float)
* Percent of character's health that is lost.

## SL_WeaponDamage

This is a type of SL_PunctualDamage that <b>requires an equipped weapon</b>, used by Attack Skills. It bases the damage off the required weapon types for the attack skill it is placed on.

This inherits all the fields on SL_PunctualDamage, and also has:

`OverrideType` (enum)
* Used to override the weapon's damage to a certain element
* You can set this to `COUNT` to bypass any override.
* Otherwise, must be one of: `Physical`, `Ethereal`, `Decay`, `Electric` (<b>not</b> Lightning), `Frost`, or `Fire`.

`ForceOnlyLeftHand` (true/false)
* If true, it will ignore the damage on any required main-hand weapons, if there are any.

`Damage_Multiplier` (float)
* Multiplies the weapon damage by this number

`Damage_Multiplier_Kback` (float)
* If you want special multipliers when the target is knocked back, set this.
* Set to -1 to use Damage_Multiplier

`Damage_Multiplier_Kdown` (float)
* Same as Damage_Multiplier_KBack, but for knockdowns instead.

You also have `Impact_Multiplier`, `Impact_Multiplier_Kback` and `Impact_Multiplier_Kdown` which are the same but they're for impact damage.

## SL_WeaponDamageFlurry

Inherits from SL_WeaponDamage, used by Prismatic Flurry.

`HitDelay` (float)
* The time delay between each hit.


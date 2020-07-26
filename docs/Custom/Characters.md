# Custom Characters

<b>Custom Characters</b> can be created and spawned with SideLoader, either through XML or C#.

The SL Pack sub-folder for custom characters is `Characters\`.

## Xml Template

Currently, there is no debug menu generator for characters, so I'll just provide an example template here.

```xml
<?xml version="1.0" encoding="utf-8"?>
<SL_Character xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <!-- The UID is important! Make sure you set a unique identifier. -->
  <!-- This UID is used by OnSpawn callbacks, and can also be used for the actual Character UID if you only spawn 1. -->
  <!-- The following syntax is recommended. It's the reverse domain syntax, but with "_" instead of "." -->
  <UID>myname_myproject_mycharacter</UID>

  <!-- Character's Name -->
  <Name>My Name</Name>
  
  <!-- Spawn information -->
  <!-- If you don't want a static spawn, just delete or comment out this part. -->
  <!-- The SceneToSpawn corresponds to the scene builds names (listed on Wiki) -->
  <SceneToSpawn>ChersoneseNewTerrain</SceneToSpawn>
  <!-- Get SpawnPosition with Explorer or Debug Mode (actual Game's Debug Mode, not SL) -->
  <!-- This is only used if you set a valid SceneToSpawn -->
  <SpawnPosition>
    <x>0.0</x>
    <y>0.0</y>
    <z>0.0</z>
  </SpawnPosition>
  
  <!-- AI settings. -->
  <!-- AddCombatAI will add generic combat AI to the character. -->
  <AddCombatAI>false</AddCombatAI>
  <!-- These two settings only apply if you added the basic combat AI -->
  <CanDodge>true</CanDodge>
  <CanBlock>true</CanBlock>
  
  <!-- Generally use either "NONE", "Players" or "Bandits", there are other options too -->
  <Faction>Bandits</Faction>
  <!-- Human character visual data. Corresponds to Character Creation options. -->
  <!-- You can delete or comment out this part to use the default character visuals. -->
  <CharacterVisualsData>
    <!-- Either "Male" or "Female" -->
    <Gender>Male</Gender>
    <!-- Skin Index is ethnicity (0 aurai / 1 tramon / 2 kazite) -->
    <SkinIndex>0</SkinIndex>
    <!-- These all relate to the character creation options. -->
    <HeadVariationIndex>0</HeadVariationIndex>
    <HairColorIndex>0</HairColorIndex>
    <HairStyleIndex>0</HairStyleIndex>
  </CharacterVisualsData>
  
  <!-- Choose the gear item IDs here -->
  <!-- If you don't use a slot, comment it out or delete it, or set it to -1. -->
  <Weapon_ID>-1</Weapon_ID>
  <Shield_ID>-1</Shield_ID>
  <Helmet_ID>-1</Helmet_ID>
  <Chest_ID>-1</Chest_ID>
  <Boots_ID>-1</Boots_ID>
  <Backpack_ID>-1</Backpack_ID>
  
  <!-- Set Character Stats here -->
  <!-- These are the default stats. -->
  <Health>100</Health>
  <HealthRegen>0</HealthRegen>
  <ImpactResist>0</ImpactResist>
  <Protection>0</Protection>
  <Damage_Resists>
    <float>0</float> <!-- phys -->
    <float>0</float> <!-- ether -->
    <float>0</float> <!-- decay -->
    <float>0</float> <!-- light -->
    <float>0</float> <!-- frost -->
    <float>0</float> <!-- fire -->
  </Damage_Resists>
  <Damage_Bonus>
    <float>0</float> <!-- phys -->
    <float>0</float> <!-- ether -->
    <float>0</float> <!-- decay -->
    <float>0</float> <!-- light -->
    <float>0</float> <!-- frost -->
    <float>0</float> <!-- fire -->
  </Damage_Bonus>
  <!-- This corresponds to Tags on statuses (eg Bleeding, Poison)... -->
  <Status_Immunity>
    <!-- example: <string>Bleeding</string> -->
  </Status_Immunity>
</SL_Character>
```

* <b>Note:</b> If you find examples helpful, there is an example Xml template at the bottom of this page.

## SL_Character Fields

`UID` (string)
* The <b>unique identifier</b> for this character template.
* Can be used for the actual Character UID, or you can override it for dynamic spawns.
* Used by the OnSpawn callback to invoke the corresponding template's method.

`Name` (string)
* The name of the character

`SceneToSpawn` (string)
* Optional, used for an automatic spawn.
* Refers to a <b>Scene build name</b> (these are listed on the wiki) 

`SpawnPosition` (Vector3)
* Optional, only used if SceneToSpawn is set.
* A Vector3 takes an `x`, `y` and `z` value.

`AddCombatAI` (true/false)
* Defaults to false
* If true, will add a basic combat AI to the character.
* This AI is based on the Summoned Ghost. You can modify it if you want.

`CanDodge` (true/false)
* Can the combat AI dodge?

`CanBlock` (true/false)
* Can the combat AI block?

`Faction` (enum)
* The faction ("team") of the character. Not referring to Story factions.
* Common options are: `NONE`, `Players`, `Bandits`
* Other options are: `Mercs`, `Tuanosaurs`, `Deer`, `Hounds`, `Merchants`, `Golden`, `CorruptionSpirit`
* Default is `Players`, if unchanged.

### Visual Data
The `CharacterVisualsData` field contains the visual data. You can set it to null or delete it for default visuals.

`Gender` (enum)
* Must be one of `Male` or `Female`

`SkinIndex` (integer)
* The ethnicity of the character
* `0` is Aurai, `1` is Tramontane, `2` is Kazite

`HeadVariationIndex` (integer)
* Face style. May be clamped depending on your chosen Gender and SkinIndex.

`HairStyleIndex` (integer)
* Chosen hair style, corresponds to the character creation options.

`HairColorIndex` (integer)
* Chosen hair color, corresponds to the character creation options.

### Equipment IDs
You can set the Item ID for each equipment slot on the character. These should be fairly self-explanatory.

`Weapon_ID` (integer)

`Shield_ID` (integer)

`Helmet_ID` (integer)

`Chest_ID` (integer)

`Boots_ID` (integer)

`Backpack_ID` (integer)

### Character Stats
The next few fields relate to the character's stats.

`Health` (float)
* Max health of the character.

`HealthRegen` (float)
* Health regenerated per second, when not in combat.
* Can be negative.

`ImpactResist` (float)
* Impact resist stat, 0-100.

`Protection` (float)
* Physical protection stat, 0-100.

`Damage_Resists` (float array)
* Damage resistance stats for the character.
* Like on SL_Item and SL_Effect, this relates to the damage types.
* Order is physical, ethereal, decay, lightning, frost, fire.
* -100 to 100 values.

`Damage_Bonus` (float array)
* Same as Damage_Resists, but for Damage Bonuses.

`Status_Immunity` (list of string)
* This is like the `Tags` field on SL_Item or SL_StatusEffect. It contains a list of strings.
* Each entry is a Tag.
* Use the Outward Tools google sheet (link in Resources on sidebar) to get Tag names.

## C# Guide
In addition to the example below, there is C# summary documentation that you can see when using SideLoader methods in your IDE.

The C# classes for custom characters are `SL_Character` and `CustomCharacters`.

### OnSpawn Event

`public event Action<Character, string> OnSpawn`

The most important thing to know about your SL_Character template is the OnSpawn event.

This event is invoked <b>locally</b> for <b>all clients</b> via RPC. You do not need to use your own RPC manager for this.

When you call `CreateCharacter` manually, you can include a `string extraRpcData` with any manual network sync that you want to send.

### Example

Here is an example from the Necromancy mod for a unique, static NPC spawn.

```csharp
private static readonly SL_Character trainerTemplate = new SL_Character()
{
    UID = TRAINER_UID,
    Faction = Character.Factions.NONE,
    Name = "Spectral Wanderer",
    SceneToSpawn = "Hallowed_Dungeon4_Interior",
    SpawnPosition = new Vector3(-138.3397f, 58.99699f, -102.4192f),
    Chest_ID = 3200040,
    Helmet_ID = 3200041,
    Boots_ID = 3200042,
    AddCombatAI = false,
};

// use Awake to set up Character templates.
internal void Awake()
{
    Instance = this;

    // Call this to register the internal callbacks and prepare the template.
    // For Xml templates, this is called by SLPack.LoadCharacters().
    trainerTemplate.Prepare();
    // Register our own manual spawn callback.
    trainerTemplate.OnSpawn += LocalTrainerSetup;
}

// In this case, we don't use the extraRpcData, so we can discard it with the "_" name.
public void LocalTrainerSetup(Character trainer, string _)
{
    // The Character has been created and passed to us.
    // Any local setup (like setting up NodeCanvas) can be done here.
}
```

Here is an example of a dynamic spawn from a custom skill `Effect.ActivateLocally()`.

```csharp
// The Ghost and Skeleton are SL_Character templates, didn't feel the need to show them since I showed that above.

// Setup templates in Awake
internal void Awake()
{
    Instance = this;

    Ghost.Prepare();
    Skeleton.Prepare();

    Ghost.OnSpawn += OnSpawn;
    Skeleton.OnSpawn += OnSpawn;
}

// In the custom Effect class for this "Summon" skill, 
// I override the ActivateLocally method.

// This Effect SyncType is set to MasterSync (only host executes)
public override void ActivateLocally(Character _affectedCharacter, object[] _infos)
{
    // Make a unique UID for this character, but the SL_Character template UID is used 
    // for the spawn callback.
    var summonUID = UID.Generate().ToString();

    // Get the appropriate template to use based on a custom condition check
    var template = PlagueAuraProximityCondition.InsidePlagueAura(_affectedCharacter) ? 
        Ghost : 
        Skeleton;

    var spawnPos = _affectedCharacter.transform.position + (Vector3.forward * 0.5f);

    var extraRpcData = caster.UID.ToString();

    // Manually call CustomCharacters.CreateCharacter. This version of the method is 
    // recommended for most cases.
    // We are manually sending a spawn position, character UID, and extraRpcData.
    // In this case, the extraRpcData is the Caster character's UID.
    CustomCharacters.CreateCharacter(template, spawnPos, summonUID, extraRpcData);
}

// Callback from CreateCharacter which executes locally for all clients.
private void OnSpawn(Character character, string rpcData)
{
    var ownerUID = rpcData;
    var summonUID = character.UID;

    // We got the caster's UID from our extraRpcData that we sent.
    // You could use the extraRpcData string for whatever needs you have.

    // In this case, it's used to maintain a dictionary of who spawned which character.
}
```

## Xml Example
This is a simple example template of an NPC. It will spawn outside the Cierzo gate.

```xml
<?xml version="1.0" encoding="utf-8"?>
<SL_Character xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <!-- The UID is important! Make sure you set a unique identifier. -->
  <!-- This UID is used by OnSpawn callbacks, and can also be used for the actual Character UID. -->
  <UID>com.sinai.Fred</UID>

  <!-- Character's Name -->
  <Name>Fred</Name>
  
  <!-- Spawn information -->
  <!-- The SceneToSpawn corresponds to the scene builds names (listed on Wiki) -->
  <SceneToSpawn>ChersoneseNewTerrain</SceneToSpawn>
  <!-- Get SpawnPosition with Explorer or Debug Mode (actual Game's Debug Mode, not SL) -->
  <SpawnPosition>
    <x>133.0</x>
    <y>33.0</y>
    <z>1461.0</z>
  </SpawnPosition>
  
  <!-- AI settings. -->
  <!-- AddCombatAI will add generic combat AI to the character. -->
  <AddCombatAI>false</AddCombatAI>
  <CanDodge>true</CanDodge>
  <CanBlock>true</CanBlock>
  
  <!-- Generally use either "NONE", "Players" or "Bandits", there are other options too (Character.Factions) -->
  <Faction>Bandits</Faction>
  <!-- Human character visual data. Corresponds to Character Creation options. -->
  <CharacterVisualsData>
    <Gender>Female</Gender>
    <!-- Skin Index is ethnicity (0 aurai / 1 tramon / 2 kazite) -->
    <SkinIndex>1</SkinIndex>
    <HeadVariationIndex>3</HeadVariationIndex>
    <HairColorIndex>1</HairColorIndex>
    <HairStyleIndex>2</HairStyleIndex>
  </CharacterVisualsData>
  
  <!-- Choose the gear item IDs here -->
  <Weapon_ID>2000010</Weapon_ID>
  <!-- <Shield_ID></Shield_ID> -->
  <!-- <Helmet_ID></Helmet_ID> -->
  <!-- <Chest_ID></Chest_ID> -->
  <!-- <Boots_ID></Boots_ID> -->
  <!-- <Backpack_ID></Backpack_ID> -->
  
  <!-- Set Character Stats here -->
  <Health>100</Health>
  <HealthRegen>0</HealthRegen>
  <ImpactResist>0</ImpactResist>
  <Protection>0</Protection>
  <Damage_Resists>
    <float>1</float> <!-- phys -->
    <float>2</float> <!-- ether -->
    <float>3</float> <!-- decay -->
    <float>4</float> <!-- light -->
    <float>5</float> <!-- frost -->
    <float>6</float> <!-- fire -->
  </Damage_Resists>
  <Damage_Bonus>
    <float>1</float> <!-- phys -->
    <float>2</float> <!-- ether -->
    <float>3</float> <!-- decay -->
    <float>4</float> <!-- light -->
    <float>5</float> <!-- frost -->
    <float>6</float> <!-- fire -->
  </Damage_Bonus>
  <!-- This corresponds to Tags on statuses (eg Bleeding, Poison)... -->
  <Status_Immunity>
    <string>Bleeding</string>
    <string>Poison</string>
  </Status_Immunity>
</SL_Character>
```
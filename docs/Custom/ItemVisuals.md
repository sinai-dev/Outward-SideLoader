# Custom Item Visuals

<b>Custom Item Visuals</b> (including icons, textures or entire prefabs) can be defined either through your SL Pack, or directly from C#.

# Using PNG Files

First, you <b>need to create a sub-folder for the custom item</b> inside your `Items` folder if you want to use custom png files.
* It should look like `[SLPack]/Items/[MyItem]/`.

You should then create a sub-folder inside that item folder called "<b>Textures</b>". 
* This Textures sub-folder should look like `[SLPack]/Items/[MyItem]/Textures/`.

If you generated a template with the [SL Menu](GettingStarted/SLMenu), it will include the icons and textures in this setup for you.

A sub-folder is also made inside the Textures folder for each <b>material</b> on the item visuals, if any.

<b>The names of the .png files you place in here must use these exact names!</b>

These two files go in the base `Textures\` folder:

`icon.png` (84x128px)
* Replaces the <b>main item icon</b>
* A blank icon template can be found here: [for items](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/ItemIcon.png), or [for skills](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/ActiveSkillIcon.png), or [for passives](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/PassiveSkillIcon.png)

`skillicon.png` (50x50px)
* The small icon used for <b>Skill Tree icons</b>
* Just a small version of the main icon.

You can also replace the <b>Material Textures</b>. A sub-folder is created for each material on the Item Visuals you clone from, and you can edit the layers on these materials as you desire.

Unless you are an advanced user, I would recommend only generating these material textures with Debug Menu, and just changing those files.

`_MainTex.png`
* This is the primary albedo texture.

`_NormTex.png`
* The normal map

`_GenTex.png` 
* Generative texture which uses RGB channels for separate purposes
* Specular (R), Gloss (G), Occlusion (B)

`_SpecColorTex.png`
* Used to add color to the specular map in some cases (RGB)

`_EmissionTex.png`
* Emissive map (glow map)

# Custom Visual Prefabs

There are two parts to setting up your custom Visual Prefab(s).

## In Unity Editor

<b>Important note:</b> 
* You must use the same version of the Unity Editor that Outward uses to build your asset bundles.
* Currently, this is <b>[Unity 2018.4.8](https://download.unity3d.com/download_unity/9bc9d983d803/Windows64EditorInstaller/UnitySetup64-2018.4.8f1.exe)</b>.

First, make sure you are familiar with the [AssetBundle Workflow](https://docs.unity3d.com/Manual/AssetBundles-Workflow.html) in the Unity Editor as well as working with [SL Pack AssetBundles](SL-Packs#assetbundles).

Secondly, note that custom visual prefabs for <b>Armor</b> or <b>Bows</b> is not supported yet. If you figure it out, please let me know! You need to work out how to use the game's SkinnedMeshRenderer and Human_v.fbx rig to set up your own armor .fbx correctly with Blender.

For standard Item Visual prefabs, all you have to do is:
* Create a <b>GameObject</b> out of your model.
* Add a <b>BoxCollider</b> to this gameobject

<B>IMPORTANT!</b>
* Make sure that the <b>scale</b> of your base Transform is <b>(1.0, 1.0, 1.0)</b>.
* Make sure that the <b>rotation</b> of your base Transform is <b>(0.0, 0.0, 0.0)</b>.
* If you need to scale the model, use the "Import Settings" on the .fbx in the Unity Editor.
* Use the "RotationOffset" and "PositionOffset" settings on the SL_Item template to position and rotate the visuals.

Once you set that up, build it to an asset bundle and chuck it in your `[MyPack]/AssetBundles/` folder.

## SL_Item Template

There are three fields in the SL_Item Template which relate to the custom visual prefabs. Each of these fields represent a SL_ItemVisual object, where you can define the data for each type of visuals.

`SL_Item.ItemVisuals`
* The base visuals, also used for the equipped visuals if no SpecialItemVisuals are set.

`SL_Item.SpecialItemVisuals`
* The special (alternate) visuals, commonly used for the <b>equipped</b> visuals.

`SL_Item.SpecialFemaleItemVisuals`
* The special <b>female</b> visuals, when there is a female alternate version.

For each of those three fields, you can define either a SL_ItemVisual or SL_ArmorVisuals object. Generally though you should match the type of SL_ItemVisual you are replacing. You can only replace prefabs if the target item also used the same type of visuals at the moment.

### SL_ItemVisual

These are the fields you can set on a SL_ItemVisual object.

`Prefab_SLPack` (string)
* The name of the SLPack you are using for the visuals.
* This means you can use Asset Bundles found in other SLPacks, if you want to.

`Prefab_AssetBundle` (string)
* The <b>file name</b> of the asset bundle in your SL Pack that contains your item visual prefab
* For example, if the bundle is `MyPack/AssetBundles/mybundle`, then put `mybundle` as your setting.

`Prefab_Name` (string)
* Set this to the name of the <b>GameObject</b> in the AssetBundle for your visual prefab.
* Make sure you actually made a GameObject out of your 3D model and you're not trying to load a Mesh object directly.

#### Vector3 Settings {docsify-ignore}

The next 4 settings are Vector3 settings. In xml, a Vector3 looks like this:
```xml
<!-- Example is Position. 
 Same goes for Rotation, PositionOffset, and RotationOffset -->
<Position> 
  <x>0</x>
  <y>0</y>
  <z>0</z>
</Position>
```

`Position` (Vector3)
* Allows you to directly set the position. This will override any setting from the original item or from Unity Editor.
* If you don't set this (ie. delete or comment it from the template) then SideLoader will copy the values from the existing visuals.

`Rotation` (Vector3)
* Allows you to directly set the rotation. This will override any setting from the original item or from Unity Editor.
* If you don't set this (ie. delete or comment it from the template) then SideLoader will copy the values from the existing visuals.

`PositionOffset` (Vector3)
* An offset applied to the position. You can use this to copy the position of the original visuals, and only apply an offset from that.

`RotationOffset` (Vector3)
* An offset applied to the rotation. You can use this to copy the rotation of the original visuals, and only apply an offset from that.

### SL_ArmorVisuals
SL_ArmorVisuals inherits from SL_ItemVisual, and contains two extra fields.

`HideFace`
* For head or chest armor only. Does equipping these visuals hide the player's Face?

`HideHair`
* For head or chest armor only. Does equipping these visuals hide the player's hair?

### Xml Example
Here is an Xml Example of an SL_ItemVisual object.

```xml
<ItemVisuals>
    <Prefab_SLPack>TsarRevolver</Prefab_SLPack>
    <Prefab_AssetBundle>tsarrevolver_bundle</Prefab_AssetBundle>
    <Prefab_Name>TsarRevolver_prefab</Prefab_Name>
    <!-- 
    I don't define Position or Rotation, so they are copied from the original.
    <Position>
        <x></x>
        <y></y>
        <z></z>
    </Position>
    <Rotation>
        <x></x>
        <y></y>
        <z></z>
    </Rotation>
    -->
    <PositionOffset>
        <x>-0.02</x>
        <y>0.075</y>
        <z>0.08</z>
    </PositionOffset>
    <RotationOffset>
        <x>-18</x>
        <y>90</y>
        <z>6</z>
    </RotationOffset>
</ItemVisuals>

<SpecialItemVisuals>
<!-- etc... -->
```

# AssetBundle Textures
SideLoader supports setting textures from an AssetBundle. This is mainly used for bulk texture changes, since doing so with .png files would take an extremely long time.

These special AssetBundles must be placed in a folder called `TextureBundles` (case sensitive) in the `Items` folder of your SL Pack. For example, `MyPack\Items\TextureBundles\mybundle`.

### Step 1: {docsify-ignore}
Firstly, create a new Unity Project. Use Unity version 2018.4.

In the project, create a folder called "Editor", and create a new C# script inside this folder.

Copy and paste the script provided here (in the "Building the Asset Bundles" section): [Unity: AssetBundle workflow](https://docs.unity3d.com/Manual/AssetBundles-Workflow.html).

You should now see the option to "Build AssetBundles" in the Assets drop-down menu at the top of the Editor.

### Step 2: {docsify-ignore}
Now you need to create a folder corresponding to each Item you want to set. These folders should be placed directly into the root "Assets" folder of your project. Your "Assets" folder can be thought of as the same as the "Items" folder in your SL Pack.

<b>IMPORTANT:</b> These folder names <b>must start with</b> the Item ID of the item the textures correspond to. When generating from the SL Menu, it will be in this structure by default. You can put whatever you want after the ID, but it must start with the ID.

If you generated the items using the SL Menu, you can simply copy the folder (eg. `2000010_IronSword`) and drag it into your Unity Project into the Assets window.

The "Textures" sub-folder that is created by SideLoader is optional, it will work whether or not you use it. So `2000010_IronSword\Textures\...` would work, as would `2000010_IronSword\...`.

You can place the `icon.png` and the `skillicon.png` in the base Item folder (or the Textures sub-folder).

Each material folder should have its own sub-folder, as per the generated format.

It should look something like this:

![The Unity Project structure](https://i.imgur.com/OG93Kfe.png)

### Step 3: {docsify-ignore}
Now, go back to the base Assets folder and select all of the Item folders. 

In the bottom right of the Editor next to AssetBundle, click "None" and then click "New..." and pick any name for the bundle.

![Building the Asset Bundles](https://i.imgur.com/UWjZ06i.png)

Now select from the navigation bar: `Assets > Build Assetbundles`.

Right click your Assets window and select "Show in Explorer" (or navigate to your Unity project Assets folder manually). Open the Assets folder, then open the AssetBundles folder, and select your bundle file.

<b>Note:</b> do not select all the other files (such as the "AssetBundles" file, or the .manifest or .meta files, etc). Only select the AssetBundle file name which you created.

Take this bundle and place it in the `TextureBundles` folder (`MyPack\Items\TextureBundles\`).

![The SideLoader pack structure](https://i.imgur.com/ScW0Pk9.png)

Launch the game, and SideLoader should log that it loaded and applied your textures in the `output_log.txt` file.
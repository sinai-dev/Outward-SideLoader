# C# Guide

This is a brief summary of how to use the SideLoader directly from your own C# mod.

## Important Notes

There are two important things to know about using SideLoder from C#:

1. Everything that applies to the XML templates is exactly the same for C# too. The API Documentation is for the XML / SLPack approach since those users are probably less experienced and might find it confusing to see the C# docs. 
2. I have added documentation to the source code itself with summaries. If you look at the SideLoader methods and classes from your own mod in your IDE, there will be instructions on what the method is for and how to use it.

This page is to help you get started with C# and show you some examples, it is not a complete C# documentation by any means.

## Adding the Reference

1. <b>Install SideLoader</b> to the Outward/BepInEx/plugins folder.
2. In your C# Project, add a reference to <b>SideLoader.dll</b> from this location.
3. Put `using SideLoader;` at the top of classes where you want to use SideLoader.

## Events
The most important thing about using SideLoader from C# are the Events, so I will cover them first.

To subscribe to an event in C#, simply put something like this in your `Awake()` function:
* `SL.OnPacksLoaded += MySetup;` where `MySetup` is a method.

The events you can subscribe to are:
* `SL.BeforePacksLoaded` is called right before SL Packs are loaded and applied. This is <b>after</b> ResourcesPrefabManager has loaded.
* `SL.OnPacksLoaded` is called after all packs have been loaded and applied. This will also be after ResourcesPrefabManager has loaded.
* `SL.OnSceneLoaded` is called when a scene has truly finished loading. This is <b>not</b> called for the Main Menu or for the "Low Memory Transition Scene".

## SLPack Class

The `SLPack` class is the C# wrapper for SL Pack folders.

* Use `SL.Packs["MyPackName"]` to get a handle on your pack.
* You can do `myPack.AssetBundles["MyAssetBundle"]`, or `myPack.Texture2D["MyTexturePNGName"]`, etc.

## Custom Items

The main SideLoader classes for custom items are `CustomItems` and `SL_Item`, and `CustomItemVisuals`.

Defining custom items from C# is the same as the XML method, except you're defining the SL_Item object from code instead of XML. You should subscribe to `SL.BeforePacksLoaded` and set up your custom items at this point. For any changes you want to make which depend on the template being applied, you should make those changes on `SL.OnPacksLoaded`.

```csharp
using SideLoader;

internal void Awake() 
{
    SL.BeforePacksLoaded += BeforePacksLoaded;
    SL.OnPacksLoaded += OnPacksLoaded;
}

private void BeforePacksLoaded()
{
    /* You can define any SL_Item class: SL_Item, SL_Equipment, SL_Skill, SL_Weapon, SL_Bag */
    var template = new SL_Weapon()
    {
        Name = "MyItem",
        StatsHolder = new SL_WeaponStats()
        {
            // ...
        },
        // etc...
    };

    /* If setting any custom item visuals or icons, you need to set these next two values. */

    /* The SLPack folder name in the SideLoader/ folder */
    template.SLPackName = "MyPackName"; 

    /* The subfolder name in the Items/ folder of the SLPack for this custom item.
     * If you're setting an icon or custom textures from .png files, you need to set this.
     * Your .png files need to be in a "Textures/" subfolder inside this subfolder, and use the names as described on the Custom Item Visuals page.
    */
    template.SubfolderName = "myitemsubfoldername"; 

    /* when you're done, call this first to set up the item. This will call ApplyTemplateToItem() too, when SideLoader has finished loading all assets. */
    var myItem = CustomItems.CreateCustomItem(template);
}

private void OnPacksLoaded()
{
    /* any non-template changes should be performed here, after CreateCustomItem() */
    var myItem = ResourcesPrefabManager.Instance.GetItemPrefab(myTemplateNewID);

    var effects = myItem.transform.Find("Effects");
    // etc ...
}
```

There are some helpers in the CustomItems class you can use to make things easier when modifying custom items in general.

`CustomItems.GetOriginalItemPrefab(int ItemID)`
* Use this to get the true original prefab for an Item ID, in case it has been modified.

`CustomItems.CreateCustomItem(int cloneTargetID, int newID, string name, SL_Item template = null)`
* You can use this to clone an item and get the cloned prefab back.
* Helpful if you just want to do your own custom item mods and not use the SideLoader templates at all, while still being compatible with other mods.

You can also look at the source of SideLoader to see how it does things and use the public methods. Most of the code is documented with summaries, so you can see a tip in your IDE when you start trying to write a SideLoader method.

## Custom Textures

Working with your custom textures from C# for replacing Item visuals/icons is exactly the same as when working from XML.

You can also place any custom .png files in the Texture2D folder, if you are using a C# mod which requires custom textures.

For example, you might need a custom texture for a menu or something.

You can do `myPack.Texture2D["MyCustomPngName"]` to get the Texture2D for this .png file at runtime. 

Note: Do <b>not</b> include the ".png" when getting your texture name this way.

## WIP

This page is still a work in progress. If you have suggestions for what else can go here, let me know.


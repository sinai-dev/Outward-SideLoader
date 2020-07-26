# Custom Item Recipes

<b>Custom Recipes</b> can be defined from XML or C#. You can set recipes for any station type.

Recipes XMLs should be placed in the `Recipes\` sub-folder of your SL Pack.

Note that custom recipes are applied after all Custom Items have been set up, so you can safely reference those here.

## Template
Currently there is no template generator for Recipes, so here's how it should look:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SL_Recipe xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <!-- Choose a unique UID for this recipe -->
  <UID>MyUniqueUIDForThisRecipe</UID>
  <!-- One of: "Survival", "Alchemy", "Cooking", "Forge" (unused) -->
  <StationType>Survival</StationType> 
  <Ingredients>
    <Ingredient>
      <!-- Type: "AddSpecificIngredient" for an Item ID, or "AddGenericIngredient" for a Tag Name -->
      <Type>AddSpecificIngredient</Type>
      <!-- Use "Ingredient_ItemID" for AddSpecificIngredient -->
      <Ingredient_ItemID>2010070</Ingredient_ItemID>
    </Ingredient>
    <Ingredient>
      <Type>AddGenericIngredient</Type>
      <!-- Use "Ingredient_Tag" for AddGenericIngredient -->
      <Ingredient_Tag>Water</Ingredient_Tag>
    </Ingredient>
    <!--
    <Ingredient>
      <Type>AddSpecificIngredient</Type>
      <Ingredient_ItemID>2010070</Ingredient_ItemID>
    </Ingredient>
    <Ingredient>
      <Type>AddSpecificIngredient</Type>
      <Ingredient_ItemID>2010070</Ingredient_ItemID>
    </Ingredient>
    -->
  </Ingredients>
  <Results>
    <ItemQty>
      <!-- You can set any ID for the result item ID, including custom items. -->
      <ItemID>2010072</ItemID> 
      <Quantity>1</Quantity>
    </ItemQty>
    <!-- You can have multiple results!
    <ItemQty>		
      <ItemID>2000010</ItemID> 
      <Quantity>1</Quantity>
    </ItemQty>
    -->
  </Results>
</SL_Recipe>
```

## SL_Recipe

`UID` (string)
* Optional. If you don't set one, SideLoader will generate one for you based on the result and ingredients.
* Used by Recipe Scroll items. Those items require a UID to learn the appropriate recipe.

`StationType` (enum)
* Can be exactly one of: `Survival`, `Alchemy`, `Cooking`, or `Forge` (unused)

`Ingredients` (list of Ingredient)
* Add between 1 and 4 ingredients
* Each ingredient can be either a `AddSpecificIngredient` for an Item ID, or `AddGenericIngredient` for a Tag.
* See [this google sheet](https://docs.google.com/spreadsheets/d/1btxPTmgeRqjhqC5dwpPXWd49-_tX_OVLN1Uvwv525K4/edit#gid=1840819680) for a list of tags.

`Results` (list of ItemQty)
* You can add any number of results that you want.
* Each must have an `ItemID`, and `Quantity` for the result.

## Recipe Scrolls

You can make a recipe scroll with a `SL_RecipeItem` template. See the [Custom Items](Custom/Items.md) page for more details.

## From C#

It's pretty much exactly the same as above. It will look like this:

```csharp
var myrecipe = new SL_Recipe() 
{
    // define everything here like its done above
};

myrecipe.ApplyRecipe();
```

That's about all there is to custom recipes. Feel free to ask me for help if you have any questions.
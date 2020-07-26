# Custom Skill Trees

<b>Skill Trees</b> can be defined through the SideLoader with C#. Currently I haven't added XML support for these yet, but if you're setting up an entire skill tree you're probably going to need your own C# anyway.

I suggest setting up the base skills with XML templates, then subscribing to `SL.OnPacksLoaded` and defining the skill tree after this point. The Skill Tree template assumes that the skills IDs are already set up when you apply it.

## SL_SkillTree
The `SL_SkillTree` class is the base class for containing the tree, as well as applying it once it is set up.

`public string Name;`
* The Name of your skill tree. This also sets the SkillSchool.UID and name.

`public List<SL_SkillRow> SkillRows;`
* The rows of your skill tree. `SL_SkillRow` is explained below.

`public SkillSchool CreateBaseSchool()`
* Call this first to create the base `GameObject`. 
* It returns the `SkillSchool` to you (a component on the base `GameObject`)

`public void ApplyRows()`
* Call this after `CreateBaseSchool()` to apply your template rows to the actual component.

### SL_SkillRow
A skill tree can contain up to 5 rows. `SL_SkillRow` is the holder for these rows. Each row can contain 3 `SL_BaseSkillSlot` templates.

`public int RowIndex;`
* The row index. Between `1` and `5`.
* `3` is the breakthrough row.
* `1` is the bottom, and `5` is the top.

`public List<SL_BaseSkillSlot> Slots;`
* The list of slots on this row.
* Each slot should actually be either a `SL_SkillSlot` or a `SL_SkillSlotFork`, don't use the base slot directly.

### SL_BaseSkillSlot
The base class used by both `SL_SkillSlotFork` and `SL_SkillSlot`.

`public int ColumnIndex;`
* The column index of this slot on the row. 
* `1` is left, `2` is middle, `3` is right.

`public Vector2 RequiredSkillSlot;`
* This one may seem a bit confusing at first, but it's pretty simple.
* A Vector2 is just a `x` and `y` value.
* The `x` is the <b>row</b>, and the `y` is the <b>column</b> of your required skill slot.
* You should make sure that `row x column y` exists on your tree.

### SL_SkillSlotFork : SL_BaseSkillSlot
`SL_SkillSlotFork` is used to force a choice between two skills. Normally if you use a fork, it is in <b>Column 2</b> and it is the only slot on that row.

`public SL_SkillSlot Choice1;`
* The first choice.

`public SL_SkillSlot Choice2;`
* The second choice.

### SL_SkillSlot : SL_BaseSkillSlot
`SL_SkillSlot` is the actual class used for defining a skill slot.

`public int SkillID;`
* The reference Skill `ItemID` for this slot. You should make sure it's already set up.

`public int SilverCost;`
* The cost in silver for this slot

`public bool Breakthrough;`
* Is this a breakthrough? Defaults to false

## Example
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using SideLoader;
using SideLoader.CustomSkills;

namespace MyNamespace
{
    [BepInPlugin("com.author.project", "MySkillTree", "1.0.0")]
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class MyMod : BaseUnityPlugin
    {
        public static SkillSchool MyTree;

        internal void Awake()
        {
            SL.OnPacksLoaded += Setup;
        }

        private void Setup()
        {
            // Call this first, to create the base gameobject and register it to the game's SkillTreeHolder
            MyTree = Template.CreateBaseSchool();

            // call this to apply your template to the tree
            Template.ApplyRows();
        }

        private static readonly SL_SkillTree Template = new SL_SkillTree()
        {
            Name = "Necromancy",
            SkillRows = new List<SL_SkillRow>()
            {
                new SL_SkillRow()
                {
                    RowIndex = 1,
                    Slots = new List<SL_BaseSkillSlot>()
                    {
                        new SL_SkillSlot() // Summon
                        {
                            ColumnIndex = 2,
                            SilverCost = 50,
                            SkillID = 8890103,
                        },
                        new SL_SkillSlot() // Vital Attunement
                        {
                            ColumnIndex = 3,
                            SilverCost = 50,
                            SkillID = 8890101,
                        },
                    }
                },
                new SL_SkillRow()
                {
                    RowIndex = 2,
                    Slots = new List<SL_BaseSkillSlot>()
                    {
                        new SL_SkillSlot() // Life Ritual
                        {
                            ColumnIndex = 2,
                            SkillID = 8890105,
                            SilverCost = 100,
                            RequiredSkillSlot = new Vector2(1, 2), // requires Summon (row 1, slot 2)
                        },
                        new SL_SkillSlot() // Tendrils
                        {
                            ColumnIndex = 3,
                            SkillID = 8890100,
                            SilverCost = 100,
                            RequiredSkillSlot = new Vector2(1, 3), // requires Vital Attunement (row 1, slot 3)
                        }
                    }
                },
                new SL_SkillRow()
                {
                    RowIndex = 3,
                    Slots = new List<SL_BaseSkillSlot>() // Transcendence (Breakthrough)
                    {
                        new SL_SkillSlot()
                        {
                            Breakthrough = true,
                            SkillID = 8890104,
                            RequiredSkillSlot = new Vector2(2, 2), // requires Life Ritual (row 2, slot 2),
                            SilverCost = 500,
                            ColumnIndex = 2
                        }
                    }
                },
                new SL_SkillRow()
                {
                    RowIndex = 4,
                    Slots = new List<SL_BaseSkillSlot>() // Death Ritual
                    {
                        new SL_SkillSlot()
                        {
                            ColumnIndex = 2,
                            RequiredSkillSlot = new Vector2(3, 2), // requires breakthrough
                            SkillID = 8890106,
                            SilverCost = 600,
                        }
                    }
                },
                new SL_SkillRow()
                {
                    RowIndex = 5,
                    Slots = new List<SL_BaseSkillSlot>() // fork choice
                    {
                        new SL_SkillSlotFork()
                        {
                            ColumnIndex = 2,
                            RequiredSkillSlot = new Vector2(4, 2), // requires Death Ritual
                            Choice1 = new SL_SkillSlot() // Plague Aura
                            {
                                ColumnIndex = 2,
                                SilverCost = 600,
                                SkillID = 8890107,
                                RequiredSkillSlot = new Vector2(4, 2), // requires Death Ritual
                            },
                            Choice2 = new SL_SkillSlot() // Army of Death
                            {
                                ColumnIndex = 2,
                                SilverCost = 600,
                                SkillID = 8890108,
                                RequiredSkillSlot = new Vector2(4, 2), // requires Death Ritual
                            }
                        }
                    }
                }
            }
        };
    }
}
```
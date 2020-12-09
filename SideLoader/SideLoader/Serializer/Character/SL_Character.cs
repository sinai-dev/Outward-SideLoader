using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SideLoader
{
    /// <summary>SideLoader's wrapper for Custom Characters.</summary>
    [SL_Serialized]
    public class SL_Character
    {
        /// <summary> This event will be executed locally by ALL clients via RPC. Use this for any custom local setup that you need.
        /// <list type="bullet">The character is the Character your template was applied to.</list>
        /// <list type="bullet">The string is the optional extraRpcData provided when you spawned the character.</list>
        /// </summary>
        public event System.Action<Character, string> OnSpawn;

        /// <summary>
        /// This event is invoked locally when save data is loaded and applied to a character using this template. 
        /// Use this to do any custom setup you might need there.
        /// <list type="bullet">The character is the Character your template was applied to.</list>
        /// <list type="bullet">The string is the optional extraRpcData provided when you spawned the character.</list>
        /// </summary>
        public event System.Action<Character, string> OnSaveApplied;

        /// <summary>Determines how this character will be saved.</summary>
        public CharSaveType SaveType;

        /// <summary>The Unique ID for this character template.</summary>
        public string UID;
        /// <summary>The display name for this character.</summary>
        public string Name;

        /// <summary>For Scene-type characters, the Scene Name to spawn in (referring to scene build names).</summary>
        public string SceneToSpawn;
        /// <summary>For Scene-type characters, the Vector3 position to spawn at.</summary>
        public Vector3 SpawnPosition;

        /// <summary>Whether or not to add basic Combat AI to the character.</summary>
        public bool AddCombatAI;
        /// <summary>If combat AI enabled, can the character dodge?</summary>
        public bool? CanDodge;
        /// <summary>If combat AI enabled, can the character block?</summary>
        public bool? CanBlock;

        /// <summary>Faction to set for the Character.</summary>
        public Character.Factions? Faction = Character.Factions.NONE;

        /// <summary>Visual Data to set for the character.</summary>
        public VisualData CharacterVisualsData;

        /// <summary>Item ID for Weapon</summary>
        public int? Weapon_ID;
        /// <summary>Item ID for Shield</summary>
        public int? Shield_ID;
        /// <summary>Item ID for Helmet</summary>
        public int? Helmet_ID;
        /// <summary>Item ID for Chest Armor</summary>
        public int? Chest_ID;
        /// <summary>Item ID for Boots</summary>
        public int? Boots_ID;
        /// <summary>Item ID for Backpack</summary>
        public int? Backpack_ID;

        // TODO pouch items (loot)

        // TODO lootableondeath (and maybe SL_DropTable eventually)

        // stats
        [XmlIgnore] private const string SL_STAT_ID = "SL_Stat";

        /// <summary>Base max health stat, default 100.</summary>
        public float? Health = 100;
        /// <summary>Base health regen stat, default 0.</summary>
        public float? HealthRegen = 0f;
        /// <summary>Base impact resist stat, default 0.</summary>
        public float? ImpactResist = 0;
        /// <summary>Base protection stat, default 0.</summary>
        public float? Protection = 0;
        /// <summary>Base damage resists, default all 0.</summary>
        public float[] Damage_Resists = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
        /// <summary>Base damage bonuses, default all 0.</summary>
        public float[] Damage_Bonus = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };

        /// <summary>
        /// List of Status or Status Family Tags this character is immune to (eg Bleeding, Poison, Burning)
        /// </summary>
        public List<string> Status_Immunity = new List<string>();

        /// <summary>
        /// Prepares callbacks. Only do this after you have set the UID! This is called by SLPack.LoadCharacters().
        /// This adds a callback for the OnSpawn event, and also the Spawner (if you set SpawnLocation and SceneToSpawn).
        /// </summary>
        public void Prepare()
        {
            // add uid to CustomCharacters callback dictionary
            if (!string.IsNullOrEmpty(this.UID))
            {
                if (CustomCharacters.Templates.ContainsKey(this.UID))
                {
                    SL.LogError("Trying to register an SL_Character Template, but one is already registered with this UID: " + UID);
                    return;
                }

                CustomCharacters.Templates.Add(this.UID, this);
            }

            if (!string.IsNullOrEmpty(this.SceneToSpawn))
            {
                CustomCharacters.INTERNAL_SpawnCharacters += SafeSpawn;
            }
        }

        internal void INTERNAL_OnSpawn(Character character, string extraRpcData)
        {
            character.gameObject.SetActive(false);

            ApplyToCharacter(character);

            SL.TryInvoke(OnSpawn, character, extraRpcData);

            character.gameObject.SetActive(true);
        }

        internal void INTERNAL_OnSaveApplied(Character character, string extraRpcData)
        {
            SL.TryInvoke(OnSaveApplied, character, extraRpcData);
        }

        internal void SafeSpawn()
        {
            if (PhotonNetwork.isNonMasterClientInRoom || SceneManagerHelper.ActiveSceneName != this.SceneToSpawn)
                return;

            Spawn(this.SpawnPosition);
        }

        /// <summary>
        /// Calls CustomCharacters.SpawnCharacter with this template.
        /// </summary>
        /// <param name="position">Spawn position for character. eg, template.SpawnPosition.</param>
        /// <param name="characterUID">Optional custom character UID for dynamic spawns</param>
        /// <param name="extraRpcData">Optional extra RPC data to send.</param>
        public Character Spawn(Vector3 position, string characterUID = null, string extraRpcData = null)
        {
            characterUID = characterUID ?? this.UID;

            if (CharacterManager.Instance.GetCharacter(characterUID) is Character existing)
            {
                SL.Log("Trying to spawn a character UID " + characterUID + " but one already exists with that UID!");
                return existing;
            }

            return CustomCharacters.SpawnCharacter(this, position, characterUID, extraRpcData).GetComponent<Character>();
        }

        /// <summary>
        /// Applies this template to a character. Some parts of the template are only applied by the host, while others are applied by any client.
        /// Ideally this method should be called by all clients via RPC (which is the case if you just use CreateCharacter).
        /// </summary>
        public void ApplyToCharacter(Character character)
        {
            // set name
            if (Name != null)
            {
                At.SetValue("", "m_nameLocKey", character);
                At.SetValue(Name, "m_name", character);
            }

            // host stuff
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                // set faction
                if (Faction != null)
                {
                    character.ChangeFaction((Character.Factions)Faction);
                }

                // gear
                if (Weapon_ID != null)
                    TryEquipItem(character, (int)Weapon_ID);

                if (Shield_ID != null)
                    TryEquipItem(character, (int)Shield_ID);

                if (Helmet_ID != null)
                    TryEquipItem(character, (int)Helmet_ID);

                if (Chest_ID != null)
                    TryEquipItem(character, (int)Chest_ID);

                if (Boots_ID != null)
                    TryEquipItem(character, (int)Boots_ID);

                if (Backpack_ID != null)
                    TryEquipItem(character, (int)Backpack_ID);
            }

            // AI
            if (this.AddCombatAI && character.GetComponent<CharacterAI>() is CharacterAI ai)
            {
                foreach (var state in ai.AiStates)
                {
                    if (state is AISCombat aisCombat)
                    {
                        if (CanDodge != null)
                            aisCombat.CanDodge = (bool)CanDodge;
                        if (CanBlock != null)
                            aisCombat.CanBlock = (bool)CanBlock;
                    }
                }
            }

            // stats
            SetStats(character);

            character.gameObject.SetActive(true);
        }

        public void SetStats(Character character)
        {
            if (character.GetComponent<PlayerCharacterStats>())
                CustomCharacters.FixStats(character);

            var stats = character.GetComponent<CharacterStats>();

            if (Health != null)
            {
                var m_maxHealthStat = (Stat)At.GetValue("m_maxHealthStat", stats);
                m_maxHealthStat.AddStack(new StatStack(SL_STAT_ID, (float)Health - 100), false);
            }

            if (HealthRegen != null)
            {
                var m_healthRegenStat = (Stat)At.GetValue("m_healthRegen", stats);
                m_healthRegenStat.AddStack(new StatStack(SL_STAT_ID, (float)HealthRegen), false);
            }

            if (ImpactResist != null)
            {
                var m_impactResistance = (Stat)At.GetValue("m_impactResistance", stats);
                m_impactResistance.AddStack(new StatStack(SL_STAT_ID, (float)ImpactResist), false);
            }

            if (Protection != null)
            {
                var m_damageProtection = (Stat[])At.GetValue("m_damageProtection", stats);
                m_damageProtection[0].AddStack(new StatStack(SL_STAT_ID, (float)Protection), false);
            }

            if (Damage_Resists != null)
            {
                var m_damageResistance = (Stat[])At.GetValue("m_damageResistance", stats);
                for (int i = 0; i < 6; i++)
                {
                    m_damageResistance[i].AddStack(new StatStack(SL_STAT_ID, Damage_Resists[i]), false);
                }
            }

            if (Damage_Bonus != null)
            {
                var m_damageTypesModifier = (Stat[])At.GetValue("m_damageTypesModifier", stats);
                for (int i = 0; i < 6; i++)
                {
                    m_damageTypesModifier[i].AddStack(new StatStack(SL_STAT_ID, Damage_Bonus[i]), false);
                }
            }

            // status immunity
            if (this.Status_Immunity != null)
            {
                var immunities = new List<TagSourceSelector>();
                foreach (var tagName in this.Status_Immunity)
                {
                    if (CustomItems.GetTag(tagName) is Tag tag && tag != Tag.None)
                    {
                        immunities.Add(new TagSourceSelector(tag));
                    }
                }

                At.SetValue(immunities.ToArray(), "m_statusEffectsNaturalImmunity", stats);
            }
        }

        /// <summary>
        /// An EquipInstantiate helper that also works on custom items. It also checks if the character owns the item and in that case tries to equip it.
        /// </summary>
        public static void TryEquipItem(Character character, int id)
        {
            if (id <= 0)
                return;

            if (ResourcesPrefabManager.Instance.GetItemPrefab(id) is Equipment item)
            {
                if (character.Inventory.Equipment.GetEquippedItem(item.EquipSlot) is Equipment existing)
                {
                    if (existing.ItemID == id)
                        return;
                    else
                        existing.ChangeParent(character.Inventory.Pouch.transform);
                }

                Item itemToEquip;

                if (character.Inventory.OwnsItem(id, 1))
                    itemToEquip = character.Inventory.GetOwnedItems(id)[0];
                else
                    itemToEquip = ItemManager.Instance.GenerateItemNetwork(id);

                itemToEquip.ChangeParent(character.Inventory.Equipment.GetMatchingEquipmentSlotTransform(item.EquipSlot));
            }
        }

        public static IEnumerator SetVisuals(Character character, string visualData)
        {
            var visuals = character.GetComponentInChildren<CharacterVisuals>(true);

            var newData = CharacterVisualData.CreateFromNetworkData(visualData);

            float start = Time.time;
            while (!visuals && Time.time - start < 5f)
            {
                yield return new WaitForSeconds(0.5f);
                visuals = character.GetComponentInChildren<CharacterVisuals>(true);
            }

            if (visuals)
            {
                // disable default visuals
                visuals.transform.Find("HeadWhiteMaleA")?.gameObject.SetActive(false);
                visuals.transform.Find("MBody0")?.gameObject.SetActive(false);
                visuals.transform.Find("MFeet0")?.gameObject.SetActive(false);

                // failsafe for head index based on skin and gender
                ClampHeadVariation(ref newData.HeadVariationIndex, (int)newData.Gender, newData.SkinIndex);

                // set visual data
                var data = visuals.VisualData;
                data.Gender = newData.Gender;
                data.SkinIndex = newData.SkinIndex;
                data.HeadVariationIndex = newData.HeadVariationIndex;
                data.HairColorIndex = newData.HairColorIndex;
                data.HairStyleIndex = newData.HairStyleIndex;

                // set to the character too
                character.VisualData = data;

                var presets = CharacterManager.CharacterVisualsPresets;

                // get the skin material
                var mat = (data.Gender == 0) ? presets.MSkins[data.SkinIndex] : presets.FSkins[data.SkinIndex];
                At.SetValue(mat, "m_skinMat", visuals);

                // apply the visuals
                var equipped = (ArmorVisuals[])At.GetValue("m_editorEquippedVisuals", visuals);

                if ((!equipped[0] || !equipped[0].HideFace) && (!equipped[1] || !equipped[1].HideFace))
                    visuals.LoadCharacterCreationHead(data.SkinIndex, (int)data.Gender, data.HeadVariationIndex);

                if ((!equipped[0] || !equipped[0].HideHair) && (!equipped[1] || !equipped[1].HideHair))
                    ApplyHairVisuals(visuals, data.HairStyleIndex, data.HairColorIndex);

                if (!equipped[1])
                    visuals.LoadCharacterCreationBody((int)data.Gender, data.SkinIndex);

                if (!equipped[2])
                    visuals.LoadCharacterCreationBoots((int)data.Gender, data.SkinIndex);
            }
            else
            {
                SL.Log("Couldn't get visuals!");
            }
        }

        public static void ApplyHairVisuals(CharacterVisuals visuals, int _hairStyleIndex, int _hairColorIndex)
        {
            var presets = CharacterManager.CharacterVisualsPresets;
            var key = $"Hair{_hairStyleIndex}";
            var dict = At.GetValue("m_armorVisualPreview", visuals) as Dictionary<string, ArmorVisuals>;

            Material material = presets.HairMaterials[_hairColorIndex];
            ArmorVisuals hairVisuals;

            if (dict.ContainsKey(key))
            {
                if (!dict[key].gameObject.activeSelf)
                {
                    dict[key].gameObject.SetActive(true);
                }

                hairVisuals = dict[key];
            }
            else
            {
                visuals.UseDefaultVisuals = true;

                hairVisuals = visuals.InstantiateVisuals(presets.Hairs[_hairStyleIndex].transform, visuals.transform).GetComponent<ArmorVisuals>();

                At.SetProp(hairVisuals, "DefaultHairVisuals", visuals);

                if (!hairVisuals.gameObject.activeSelf)
                {
                    hairVisuals.gameObject.SetActive(true);
                }

                // Add to dict
                dict.Add(key, hairVisuals);
            }

            if (_hairStyleIndex != 0)
            {
                if (!hairVisuals.Renderer)
                {
                    var renderer = hairVisuals.GetComponent<SkinnedMeshRenderer>();
                    At.SetValue(renderer, "m_skinnedMeshRenderer", hairVisuals);
                }

                hairVisuals.Renderer.material = material;
                At.Call(visuals, "FinalizeSkinnedRenderer", null, hairVisuals.Renderer);
            }

            hairVisuals.ApplyToCharacterVisuals(visuals);
        }

        public enum Ethnicities
        {
            White,
            Black,
            Asian
        }

        // This helper clamps the desired head variation within the available options.
        // Minimum is always 0 obviously, but the max index varies between gender and skin index.
        private static void ClampHeadVariation(ref int index, int gender, int skinindex)
        {
            // get the first letter of the gender name (either M or F)
            // Could cast to Character.Gender and then Substring(0, 1), but that seems unnecessary over a simple if/else.
            string sex = gender == 0 ? "M" : "F";

            // cast skinindex to ethnicity name
            string ethnicity = ((Ethnicities)skinindex).ToString();

            // get the field name (eg. MHeadsWhite, FHeadsBlack, etc)
            string fieldName = sex + "Heads" + ethnicity;

            var array = (GameObject[])At.GetValue(fieldName, CharacterManager.CharacterVisualsPresets);

            int limit = array.Length - 1;

            // clamp desired head inside 0 and limit
            index = Mathf.Clamp(index, 0, limit);
        }

        /// <summary>Wrapper for Visual Data to apply to a Character.</summary>
        [SL_Serialized]
        public class VisualData
        {
            /// <summary>Gender of the character (Male or Female)</summary>
            public Character.Gender Gender = Character.Gender.Male;
            /// <summary>Hair color index (refer to character creation options)</summary>
            public int HairColorIndex = 0;
            /// <summary>Hair style index (refer to character creation options)</summary>
            public int HairStyleIndex = 0;
            /// <summary>Head variation index (refer to character creation options)</summary>
            public int HeadVariationIndex = 0;
            /// <summary>Skin index (refer to character creation options)</summary>
            public int SkinIndex = 0;

            /// <summary>
            /// Generates a string for this VisualData with CharacterVisualData.ToNetworkData()
            /// </summary>
            public override string ToString()
            {
                return new CharacterVisualData()
                {
                    Gender = this.Gender,
                    HairColorIndex = this.HairColorIndex,
                    HeadVariationIndex = this.HeadVariationIndex,
                    HairStyleIndex = this.HairStyleIndex,
                    SkinIndex = this.SkinIndex
                }.ToNetworkData();
            }
        }
    }
}

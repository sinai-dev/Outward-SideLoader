using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;
using SideLoader.Helpers;
using SideLoader.Model;

namespace SideLoader
{
    /// <summary>SideLoader's wrapper for Custom Characters.</summary>
    [SL_Serialized]
    public class SL_Character : IContentTemplate<string>
    {
        #region IContentTemplate
        [XmlIgnore] public string DefaultTemplateName => "Untitled Character";
        [XmlIgnore] public bool IsCreatingNewID => true;
        [XmlIgnore] public bool DoesTargetExist => true;
        [XmlIgnore] public string TargetID => this.UID;
        [XmlIgnore] public string AppliedID => this.UID;
        [XmlIgnore] public SLPack.SubFolders SLPackCategory => SLPack.SubFolders.Characters;
        [XmlIgnore] public bool TemplateAllowedInSubfolder => false;

        [XmlIgnore] public bool CanParseContent => false;
        public IContentTemplate ParseToTemplate(object _) => throw new NotImplementedException();
        public object GetContentFromID(object id) => throw new NotImplementedException();

        [XmlIgnore]
        public string SerializedSLPackName
        {
            get => SLPackName;
            set => SLPackName = value;
        }
        [XmlIgnore]
        public string SerializedSubfolderName
        {
            get => null;
            set { }
        }
        [XmlIgnore]
        public string SerializedFilename
        {
            get => m_serializedFilename;
            set => m_serializedFilename = value;
        }

        public void CreateContent() => this.Prepare();
        #endregion

        /// <summary>[Not Serialized] The name of the SLPack used to load certain assets from. Not required.</summary>
        [XmlIgnore] public string SLPackName { get; set; }

        internal string m_serializedFilename;

        /// <summary> This event will be executed locally by ALL clients via RPC. Use this for any custom local setup that you need.
        /// <list type="bullet">The character is the Character your template was applied to.</list>
        /// <list type="bullet">The string is the optional extraRpcData provided when you spawned the character.</list>
        /// </summary>
        public event Action<Character, string> OnSpawn;

        /// <summary>
        /// This event is invoked locally when save data is loaded and applied to a character using this template. 
        /// Use this to do any custom setup you might need there.
        /// <list type="bullet">The character is the Character your template was applied to.</list>
        /// <list type="bullet">The first string is the optional extraRpcData provided when you spawned the character.</list>
        /// <list type="bullet">The second string is the optional extra save data from your OnCharacterBeingSaved method, if used.</list>
        /// </summary>
        public event Action<Character, string, string> OnSaveApplied;

        /// <summary>
        /// Invoked when the character is being saved.
        /// <list type="bullet">The (in) character is the character being saved</list>
        /// <list type="bullet">The (out) string is your extra save data you want to keep.</list>
        /// </summary>
        public event Func<Character, string> OnCharacterBeingSaved;

        /// <summary>Determines how this character will be saved.</summary>
        public CharSaveType SaveType;

        /// <summary>The Unique ID for this character template.</summary>
        public string UID;
        /// <summary>The display name for this character.</summary>
        public string Name;

        /// <summary>If true, the character will be automatically destroyed when it dies.</summary>
        public bool DestroyOnDeath;

        /// <summary>For Scene-type characters, the Scene Name to spawn in (referring to scene build names).</summary>
        public string SceneToSpawn;
        /// <summary>For Scene-type characters, the Vector3 position to spawn at.</summary>
        public Vector3 SpawnPosition;
        /// <summary>For Scene-type characters, the Vector3 eulerAngles rotation to spawn with.</summary>
        public Vector3 SpawnRotation;

        /// <summary>Faction to set for the Character.</summary>
        public Character.Factions? Faction = Character.Factions.NONE;

        /// <summary>Optional, manually define the factions this character can target (if has Combat AI)</summary>
        public Character.Factions[] TargetableFactions;

        /// <summary>Visual Data to set for the character.</summary>
        public VisualData CharacterVisualsData;

        // ~~~~~~~~~~ equipment ~~~~~~~~~~
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

        // ~~~~~~~~~~ stats ~~~~~~~~~~
        [XmlIgnore] private const string SL_STAT_ID = "SL_Stat";

        /// <summary>Base max Health stat, default 100.</summary>
        public float? Health = 100;
        /// <summary>Base Health regen stat, default 0.</summary>
        public float? HealthRegen = 0f;
        /// <summary>BaseImpact resist stat, default 0.</summary>
        public float? ImpactResist = 0;
        /// <summary>Base Protection stat, default 0.</summary>
        public float? Protection = 0;
        /// <summary>Base Barrier stat, default 0.</summary>
        public float? Barrier = 0;
        /// <summary>Base damage resists, default all 0.</summary>
        public float[] Damage_Resists = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
        /// <summary>Base damage bonuses, default all 0.</summary>
        public float[] Damage_Bonus = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };

        /// <summary>List of Status or Status Family Tags this character is immune to (eg Bleeding, Poison, Burning)</summary>
        public List<string> Status_Immunity = new List<string>();

        // ~~~~~~~~~~ AI States ~~~~~~~~~~
        public SL_CharacterAI AI;
        
        [Obsolete("Use SL_Character.AI instead")] [XmlIgnore] public bool AddCombatAI;
        [Obsolete("Use SL_Character.AI instead")] [XmlIgnore] public bool? CanDodge;
        [Obsolete("Use SL_Character.AI instead")] [XmlIgnore] public bool? CanBlock;

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
                CustomCharacters.INTERNAL_SpawnCharacters += SceneSpawnIfValid;

            OnPrepare();

            SL.Log("Prepared SL_Character '" + Name + "' (" + UID + ")");
        }

        public void Unregister()
        {
            if (CustomCharacters.Templates.ContainsKey(this.UID))
                CustomCharacters.Templates.Remove(this.UID);

            if (!string.IsNullOrEmpty(this.SceneToSpawn))
                CustomCharacters.INTERNAL_SpawnCharacters -= SceneSpawnIfValid;
        }

        /// <summary>
        /// Calls CustomCharacters.SpawnCharacter with this template.
        /// </summary>
        /// <param name="position">Spawn position for character. eg, template.SpawnPosition.</param>
        /// <param name="characterUID">Optional custom character UID for dynamic spawns</param>
        /// <param name="extraRpcData">Optional extra RPC data to send.</param>
        public Character Spawn(Vector3 position, string characterUID = null, string extraRpcData = null)
            => Spawn(position, Vector3.zero, characterUID, extraRpcData);

        /// <summary>
        /// Calls CustomCharacters.SpawnCharacter with this template.
        /// </summary>
        /// <param name="position">Spawn position for character. eg, template.SpawnPosition.</param>
        /// <param name="rotation">Rotation to spawn with, eg template.SpawnRotation</param>
        /// <param name="characterUID">Optional custom character UID for dynamic spawns</param>
        /// <param name="extraRpcData">Optional extra RPC data to send.</param>
        public Character Spawn(Vector3 position, Vector3 rotation, string characterUID = null, string extraRpcData = null)
        {
            characterUID = characterUID ?? this.UID;

            if (CharacterManager.Instance.GetCharacter(characterUID) is Character existing)
            {
                SL.Log("Trying to spawn a character UID " + characterUID + " but one already exists with that UID!");
                return existing;
            }

            return CustomCharacters.SpawnCharacter(this, position, rotation, characterUID, extraRpcData).GetComponent<Character>();
        }

        internal virtual void OnPrepare() { }

        internal void INTERNAL_OnSpawn(Character character, string extraRpcData)
        {
            character.gameObject.SetActive(false);

            ApplyToCharacter(character);

            SL.TryInvoke(OnSpawn, character, extraRpcData);
        }

        internal void INTERNAL_OnSaveApplied(Character character, string extraRpcData, string extraSaveData)
        {
            SL.TryInvoke(OnSaveApplied, character, extraRpcData, extraSaveData);
        }

        internal string INTERNAL_OnPrepareSave(Character character)
        {
            string ret = null;

            try
            {
                ret = OnCharacterBeingSaved?.Invoke(character);
            }
            catch (Exception e)
            {
                SL.LogWarning("Exception invoking OnCharacterBeingSaved for template '" + this.UID + "'");
                SL.LogInnerException(e);
            }

            return ret;
        }

        internal void SceneSpawnIfValid()
        {
            if (PhotonNetwork.isNonMasterClientInRoom || SceneManagerHelper.ActiveSceneName != this.SceneToSpawn)
                return;

            Spawn(this.SpawnPosition, this.SpawnRotation);
        }

        /// <summary>
        /// Applies this template to a character. Some parts of the template are only applied by the host, while others are applied by any client.
        /// Ideally this method should be called by all clients via RPC (which is the case if you just use the Spawn() method).
        /// </summary>
        public virtual void ApplyToCharacter(Character character)
        {
            // set name
            if (Name != null)
            {
                At.SetField(character, "m_nameLocKey", "");
                At.SetField(character, "m_name", Name);
            }

            // if host
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                if (this.DestroyOnDeath)
                    character.OnDeath += () => { SLPlugin.Instance.StartCoroutine(DestroyOnDeathCoroutine(character)); };

                // set faction
                if (Faction != null)
                    character.ChangeFaction((Character.Factions)Faction);

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
            if (this.AI != null && !PhotonNetwork.isNonMasterClientInRoom)
            {
                SL.Log("SL_Character AI is " + this.AI.GetType().FullName + ", applying...");
                this.AI.Apply(character);
            }

            // stats
            SetStats(character);

            if (this.TargetableFactions != null)
            {
                var targeting = character.GetComponent<TargetingSystem>();
                if (targeting)
                    targeting.TargetableFactions = this.TargetableFactions;
                else
                    SL.LogWarning("SL_Character: Could not get TargetingSystem component!");
            }

            character.gameObject.SetActive(true);
        }

        private IEnumerator DestroyOnDeathCoroutine(Character character)
        {
            yield return new WaitForSeconds(2.0f);

            CustomCharacters.DestroyCharacterRPC(character);
        }

        public virtual void SetStats(Character character)
        {
            if (character.GetComponent<PlayerCharacterStats>())
                CustomCharacters.FixStats(character);

            var stats = character.GetComponent<CharacterStats>();

            if (!stats)
                return;

            if (Health != null)
            {
                var m_maxHealthStat = (Stat)At.GetField(stats, "m_maxHealthStat");
                m_maxHealthStat.AddStack(new StatStack(SL_STAT_ID, (float)Health - 100), false);
            }

            if (HealthRegen != null)
            {
                var m_healthRegenStat = (Stat)At.GetField(stats, "m_healthRegen");
                m_healthRegenStat.AddStack(new StatStack(SL_STAT_ID, (float)HealthRegen), false);
            }

            if (ImpactResist != null)
            {
                var m_impactResistance = (Stat)At.GetField(stats, "m_impactResistance");
                m_impactResistance.AddStack(new StatStack(SL_STAT_ID, (float)ImpactResist), false);
            }

            if (Protection != null)
            {
                var m_damageProtection = (Stat[])At.GetField(stats, "m_damageProtection");
                m_damageProtection[0].AddStack(new StatStack(SL_STAT_ID, (float)Protection), false);
            }

            if (this.Barrier != null)
            {
                var m_barrier = (Stat)At.GetField(stats, "m_barrierStat");
                m_barrier.AddStack(new StatStack(SL_STAT_ID, (float)Barrier), false);
            }

            if (Damage_Resists != null)
            {
                var m_damageResistance = (Stat[])At.GetField(stats, "m_damageResistance");
                for (int i = 0; i < 6; i++)
                {
                    m_damageResistance[i].AddStack(new StatStack(SL_STAT_ID, Damage_Resists[i]), false);
                }
            }

            if (Damage_Bonus != null)
            {
                var m_damageTypesModifier = (Stat[])At.GetField(stats, "m_damageTypesModifier");
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

                At.SetField(stats, "m_statusEffectsNaturalImmunity", immunities.ToArray());
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

            yield return new WaitForSeconds(0.5f);

            if (visuals)
            {
                // disable default visuals
                visuals.transform.Find("HeadWhiteMaleA")?.gameObject.SetActive(false);
                ((ArmorVisuals)At.GetField(visuals, "m_defaultHeadVisuals"))?.gameObject.SetActive(false);
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
                At.SetField(visuals, "m_skinMat", mat);

                // apply the visuals
                var hideface = false;
                var hidehair = false;

                if (visuals.ActiveVisualsHead)
                {
                    hideface = visuals.ActiveVisualsHead.HideFace;
                    hidehair = visuals.ActiveVisualsHead.HideHair;
                }

                SL.Log("hideface: " + hideface);

                if (!hideface)
                    visuals.LoadCharacterCreationHead(data.SkinIndex, (int)data.Gender, data.HeadVariationIndex);

                if (!hidehair)
                    ApplyHairVisuals(visuals, data.HairStyleIndex, data.HairColorIndex);

                if (!visuals.ActiveVisualsBody)
                    visuals.LoadCharacterCreationBody((int)data.Gender, data.SkinIndex);

                if (!visuals.ActiveVisualsFoot)
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
            var dict = At.GetField(visuals, "m_armorVisualPreview") as Dictionary<string, ArmorVisuals>;

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

                At.SetProperty(visuals, "DefaultHairVisuals", hairVisuals);

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
                    At.SetField(hairVisuals, "m_skinnedMeshRenderer", renderer);
                }

                hairVisuals.Renderer.material = material;
                At.Invoke(visuals, "FinalizeSkinnedRenderer", hairVisuals.Renderer);
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

            var array = (GameObject[])At.GetField(CharacterManager.CharacterVisualsPresets, fieldName);

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

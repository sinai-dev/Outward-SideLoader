using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SideLoader.SaveData
{
    [SL_Serialized]
    public abstract class PlayerSaveExtension
    {
        // ~~~~~~ Static ~~~~~~

        [XmlIgnore] internal static readonly HashSet<PlayerSaveExtension> s_registeredModels = new HashSet<PlayerSaveExtension>();

        internal static void LoadExtensions(Character character)
        {
            var dir = SLSaveManager.GetSaveFolderForCharacter(character)
                          + "\\" + SLSaveManager.CUSTOM_FOLDER + "\\";

            bool isWorldHost = character.UID == CharacterManager.Instance.GetWorldHostCharacter()?.UID;

            foreach (var file in Directory.GetFiles(dir))
            {
                var typename = Serializer.GetBaseTypeOfXmlDocument(file);

                var model = s_registeredModels.FirstOrDefault(it => it.GetType().Name == typename);
                if (model != null)
                {
                    var serializer = Serializer.GetXmlSerializer(model.GetType());
                    using (var xml = File.OpenRead(file))
                    {
                        try
                        {
                            if (serializer.Deserialize(xml) is PlayerSaveExtension loaded)
                                loaded.LoadSaveData(character, isWorldHost);
                            else
                                throw new Exception("Unknown - extension was null after attempting to load XML!");
                        }
                        catch (Exception ex)
                        {
                            SL.LogWarning("Exception loading Player Save Extension XML!");
                            SL.LogInnerException(ex);
                        }
                    }
                }
                else
                    SL.LogWarning("Loading PlayerSaveExtensions, could not find a matching registered type for " + typename);
            }
        }

        internal static void SaveAllExtensions(Character character)
        {
            var baseDir = SLSaveManager.GetSaveFolderForCharacter(character)
                          + "\\" + SLSaveManager.CUSTOM_FOLDER + "\\";

            bool isWorldHost = character.UID == CharacterManager.Instance.GetWorldHostCharacter()?.UID;

            foreach (var model in s_registeredModels)
            {
                var newModel = (PlayerSaveExtension)Activator.CreateInstance(model.GetType());
                newModel.SaveDataFromCharacter(character, isWorldHost);

                Serializer.SaveToXml(baseDir, newModel.GetType().FullName, newModel);
            }
        }

        // ~~~~~~ Instance ~~~~~~

        public abstract void ApplyLoadedSave(Character character, bool isWorldHost);
        public abstract void Save(Character character, bool isWorldHost);

        public void Prepare()
        {
            // register to data models
            if (!s_registeredModels.Contains(this))
                s_registeredModels.Add(this);
            else
                SL.LogWarning("Trying to register an SL_CustomCharSaveData twice: " + this.GetType().FullName);
        }

        internal void LoadSaveData(Character character, bool isWorldHost)
        {
            try
            {
                ApplyLoadedSave(character, isWorldHost);
            }
            catch (Exception ex)
            {
                SL.LogWarning("Exception loading save data onto character: " + this.ToString());
                SL.LogInnerException(ex);
            }
        }

        internal void SaveDataFromCharacter(Character character, bool isWorldHost)
        {
            try
            {
                Save(character, isWorldHost);
            }
            catch (Exception ex)
            {
                SL.LogWarning("Exception saving data from character: " + this.ToString());
                SL.LogInnerException(ex);
            }
        }
    }
}

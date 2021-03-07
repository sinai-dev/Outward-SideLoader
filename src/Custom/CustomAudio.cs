using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace SideLoader
{
    /// <summary>Helper class used to manage and replace Audio and Music.</summary>
    public class CustomAudio
    {
        /// <summary>The GlobalAudioManager Instance reference (since its not public)</summary>
        public static GlobalAudioManager GAMInstance => References.GLOBALAUDIOMANAGER;

        /// <summary>
        /// List of AudioClips which have been replaced.
        /// </summary>
        public static readonly List<GlobalAudioManager.Sounds> ReplacedClips = new List<GlobalAudioManager.Sounds>();
        
        /// <summary>
        /// Load an Audio Clip from a given file path, and optionally put it in the provided SL Pack.
        /// </summary>
        /// <param name="filePath">The file path (must be a .WAV file) to load from</param>
        /// <param name="pack">Optional SL Pack to put the audio clip inside.</param>
        /// <returns>The loaded audio clip, if successful.</returns>
        public static AudioClip LoadAudioClip(string filePath, SLPack pack = null)
        {
            if (!File.Exists(filePath))
                return null;

            var data = File.ReadAllBytes(filePath);

            return LoadAudioClip(data, Path.GetFileNameWithoutExtension(filePath), pack);
        }

        /// <summary>
        /// Load an Audio Clip from a given byte array, and optionally put it in the provided SL Pack.
        /// </summary>
        /// <param name="data">The byte[] array from <see cref="File.ReadAllBytes(string)"/> on the wav file path.</param>
        /// <param name="name">The name to give to the audio clip.</param>
        /// <param name="pack">Optional SL Pack to put the audio clip inside.</param>
        /// <returns>The loaded audio clip, if successful.</returns>
        public static AudioClip LoadAudioClip(byte[] data, string name, SLPack pack = null)
        {
            try
            {
                var clip = ToAudioClip(data, 0, name);

                return FinalizeAudioClip(clip, name, pack);
            }
            catch (Exception ex)
            {
                SL.LogWarning("Exception loading AudioClip!");
                SL.LogInnerException(ex);
                return null;
            }
        }

        // Finalize the clip (name / SLPack), and try to replace global game audio if one has the same name.
        internal static AudioClip FinalizeAudioClip(AudioClip clip, string name, SLPack pack = null)
        {
            clip.name = name;

            if (pack != null)
            {
                if (pack.AudioClips.ContainsKey(name))
                {
                    SL.LogWarning("Replacing clip '" + name + "' in pack '" + pack.Name + "'");

                    if (pack.AudioClips[name])
                        GameObject.Destroy(pack.AudioClips[name]);

                    pack.AudioClips.Remove(name);
                }

                pack.AudioClips.Add(name, clip);
            }

            if (Enum.TryParse(name, out GlobalAudioManager.Sounds sound))
                ReplaceAudio(sound, clip);

            return clip;
        }

        #region REPLACING GAME AUDIO

        /// <summary>Replace a global sound with the provided AudioClip.</summary>
        public static void ReplaceAudio(GlobalAudioManager.Sounds sound, AudioClip clip)
        {
            if (!GAMInstance)
            {
                SL.LogWarning("Cannot find GlobalAudioManager Instance!");
                return;
            }

            if (ReplacedClips.Contains(sound))
                SL.Log($"The Sound clip '{sound}' has already been replaced, replacing again...");

            try
            {
                DoReplaceClip(sound, clip);
            }
            catch (Exception e)
            {
                SL.LogError($"Exception replacing clip '{sound}'.\r\nMessage: {e.Message}\r\nStack: {e.StackTrace}");
            }
        }

        private static void DoReplaceClip(GlobalAudioManager.Sounds _sound, AudioClip _newClip)
        {
            if (!_newClip)
            {
                SL.LogWarning($"The replacement clip for '{_sound}' is null");
                return;
            }

            //var path = GAMInstance.GetPrefabPath(_sound);
            var path = (string)At.Invoke(GAMInstance, "GetPrefabPath", _sound);
            var resource = Resources.Load<GameObject>("_Sounds/" + path);
            var component = resource.GetComponent<AudioSource>();
            component.clip = _newClip;

            resource.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            if (!ReplacedClips.Contains(_sound))
                ReplacedClips.Add(_sound);

            SL.Log("Replaced " + _sound + " AudioSource with custom clip!");
        }

        #endregion

        #region WAV UTILS

        public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
        {
            //string riff = Encoding.ASCII.GetString (fileBytes, 0, 4);
            //string wave = Encoding.ASCII.GetString (fileBytes, 8, 4);
            int subchunk1 = BitConverter.ToInt32(fileBytes, 16);
            UInt16 audioFormat = BitConverter.ToUInt16(fileBytes, 20);

            // NB: Only uncompressed PCM wav files are supported.
            string formatCode = FormatCode(audioFormat);
            Debug.AssertFormat(audioFormat == 1 || audioFormat == 65534, "Detected format code '{0}' {1}, but only PCM and WaveFormatExtensable uncompressed formats are currently supported.", audioFormat, formatCode);

            UInt16 channels = BitConverter.ToUInt16(fileBytes, 22);
            int sampleRate = BitConverter.ToInt32(fileBytes, 24);
            //int byteRate = BitConverter.ToInt32 (fileBytes, 28);
            //UInt16 blockAlign = BitConverter.ToUInt16 (fileBytes, 32);
            UInt16 bitDepth = BitConverter.ToUInt16(fileBytes, 34);

            int headerOffset = 16 + 4 + subchunk1 + 4;
            int subchunk2 = BitConverter.ToInt32(fileBytes, headerOffset);
            //Debug.LogFormat ("riff={0} wave={1} subchunk1={2} format={3} channels={4} sampleRate={5} byteRate={6} blockAlign={7} bitDepth={8} headerOffset={9} subchunk2={10} filesize={11}", riff, wave, subchunk1, formatCode, channels, sampleRate, byteRate, blockAlign, bitDepth, headerOffset, subchunk2, fileBytes.Length);

            float[] data;
            switch (bitDepth)
            {
                case 8:
                    data = Convert8BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                    break;
                case 16:
                    data = Convert16BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                    break;
                case 24:
                    data = Convert24BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                    break;
                case 32:
                    data = Convert32BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                    break;
                default:
                    throw new Exception(bitDepth + " bit depth is not supported.");
            }

            AudioClip audioClip = AudioClip.Create(name, data.Length, (int)channels, sampleRate, false);
            audioClip.SetData(data, 0);
            return audioClip;
        }

        private static string FormatCode(UInt16 code)
        {
            switch (code)
            {
                case 1:
                    return "PCM";
                case 2:
                    return "ADPCM";
                case 3:
                    return "IEEE";
                case 7:
                    return "μ-law";
                case 65534:
                    return "WaveFormatExtensable";
                default:
                    Debug.LogWarning("Unknown wav code format:" + code);
                    return "";
            }
        }

        private static float[] Convert8BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 8-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

            float[] data = new float[wavSize];

            sbyte maxValue = sbyte.MaxValue;

            int i = 0;
            while (i < wavSize)
            {
                data[i] = (float)source[i] / maxValue;
                ++i;
            }

            return data;
        }

        private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 16-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

            int x = sizeof(Int16); // block size = 2
            int convertedSize = wavSize / x;

            float[] data = new float[convertedSize];

            Int16 maxValue = Int16.MaxValue;

            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
                ++i;
            }

            Debug.AssertFormat(data.Length == convertedSize, "AudioClip .wav data is wrong size: {0} == {1}", data.Length, convertedSize);

            return data;
        }

        private static float[] Convert24BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 24-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

            int x = 3; // block size = 3
            int convertedSize = wavSize / x;

            int maxValue = Int32.MaxValue;

            float[] data = new float[convertedSize];

            byte[] block = new byte[sizeof(int)]; // using a 4 byte block for copying 3 bytes, then copy bytes with 1 offset

            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                Buffer.BlockCopy(source, offset, block, 1, x);
                data[i] = (float)BitConverter.ToInt32(block, 0) / maxValue;
                ++i;
            }

            Debug.AssertFormat(data.Length == convertedSize, "AudioClip .wav data is wrong size: {0} == {1}", data.Length, convertedSize);

            return data;
        }

        private static float[] Convert32BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 32-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

            int x = sizeof(float); //  block size = 4
            int convertedSize = wavSize / x;

            Int32 maxValue = Int32.MaxValue;

            float[] data = new float[convertedSize];

            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt32(source, offset) / maxValue;
                ++i;
            }

            Debug.AssertFormat(data.Length == convertedSize, "AudioClip .wav data is wrong size: {0} == {1}", data.Length, convertedSize);

            return data;
        }

        #endregion
    }
}

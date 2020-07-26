# Replacing Audio

Replacing Audio with SideLoader is fairly simple, the only tedious part is finding the correct name for replacement.

All you have to do is use the same name that the game uses (listed below) and SideLoader will find and replace the original audio with your file.

Your .wav files must be placed in the `AudioClip\` sub-folder of your [SL Pack](SL-Packs).

For example, `Mods\SideLoader\MyPack\AudioClip\BGM_GeneralTitleScreen.wav` would replace the title screen music.

## Audio Clip Names

You can see a full list of audio clip names [here](https://github.com/sinaioutlander/Outward-SideLoader/blob/master/Resources/Types/enums/GlobalAudioManager.Sounds.txt).

The names start with a prefix which is used as a category for the sound, you can use this to find the type of sound you are interested in:
* `BGM_` is background music
* `ENV_` is environment sounds
* `SFX_` is sound effects
* `FT_` is foot (walking) sounds
* `LOC_` is for localization (dialogue / etc)
* `CS_` is combat sounds
* `UI_` is for user interface sounds

## Custom Audio for C#

In C#, you have a bit more control, you can access any .wav file that you have placed in an AudioClip folder like so.

```csharp
var pack = SL.Packs["MyPackName"];

var clips = pack.AudioClips;

var myClip = clips["MyClipName"]; // do not include the ".wav" in your clip name
```
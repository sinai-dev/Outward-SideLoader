# Replacing Textures

Replacing textures is probably the easiest part of using the SideLoader. All you have to do is place the texture .png file in the `Texture2D\` folder of your [SL Pack](SL-Packs).

Importantly, your textures must use <b>the exact same name</b> that the game uses. This is how the SideLoader identifies which texture(s) to replace.

## Items and Status Effects

For Items and Status Effects, you can replace these items and textures using a Custom Item or Custom Status template.

See the related articles for more details.

## Finding Textures

To get textures to replace as well as the names of those textures, use [uTiny Ripper](https://sourceforge.net/projects/utinyripper/files).

1. Download uTiny Ripper and run the "uTinyRipper.exe" file. 
2. Drag <b>the entire "Outward/" folder</b> onto the uTiny Ripper GUI.
3. Wait for uTiny to finish reading the assets.
4. Unpack and Save the Assets to a folder of your choice.

This may take a while. When it's done, open the generated folder and look in the "Texture2D" folder. Most of the game's textures can be found inside here, simply copy+paste them into your Texture2D folder and make changes as desired.

## Shader Layer Names

When working with textures, you will notice some shader layer names used for different .png files.

`_MainTex` (texture<b>_d</b>.png)
* This is the primary albedo texture.

`_NormTex` (texture<b>_n</b>.png)
* The normal map

`_GenTex` (texture<b>_g</b>.png)
* Generative texture which uses RGB channels for separate purposes
* Specular (R), Gloss (G), Occlusion (B)

`_SpecColorTex` (texture<b>_sc</b>.png)
* Used to add color to the specular map in some cases (RGB)

`_EmissionTex` (texture<b>_i</b>.png)
* Emissive map (glow map)
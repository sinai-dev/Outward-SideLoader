//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using HarmonyLib;

///*
// * This class is not actually used yet. Just using for testing / debug purposes at the moment.
// */

//namespace SideLoader
//{
//    public class CustomLighting : MonoBehaviour
//    {
//        public static CustomLighting Instance;

//        private Rect m_guiRect = new Rect(25, 25, 400, 500);

//        private TOD_Sky TODSkyInstance
//        {
//            get
//            {
//                if (m_TODSkyInstance == null)
//                {
//                    m_TODSkyInstance = EnvironmentConditions.Instance?.GetComponentInChildren<TOD_Sky>();
//                }

//                return m_TODSkyInstance;
//            }
//        }

//        private static TOD_Sky m_TODSkyInstance;

//        internal void Awake()
//        {
//            Instance = this;

//            //SL.OnSceneLoaded += SL_OnSceneLoaded;
//        }

//        //private void SL_OnSceneLoaded()
//        //{
//        //    DisableLights();
//        //}

//        internal void Update()
//        {
//            if (Input.GetKeyDown(KeyCode.Home))
//            {
//                DisableLights();
//            }
//        }

//        internal void OnGUI()
//        {
//            if (TODSkyInstance == null)
//            {
//                return;
//            }

//            var orig = GUI.skin;
//            GUI.skin = UI.UIStyles.WindowSkin;

//            m_guiRect = GUI.Window(156, m_guiRect, WindowFunction, "SL Lights Menu");

//            GUI.skin = orig;
//        }

//        private void WindowFunction(int id)
//        {
//            GUI.DragWindow(new Rect(0, 0, m_guiRect.width, 20));

//            GUILayout.BeginArea(new Rect(3, 23, m_guiRect.width - 6, m_guiRect.height - 26));

//            GUILayout.Label("<b>Ambient Light</b>");

//            var intensity = RenderSettings.ambientIntensity;
//            EditFloat("Intensity:", ref intensity, 120);
//            RenderSettings.ambientIntensity = intensity;

//            GUILayout.BeginHorizontal();
//            GUILayout.Label("Ambient Mode:", GUILayout.Width(100));
//            var mode = (object)TODSkyInstance.Ambient.Mode;
//            if (GUILayout.Button("<", GUILayout.Width(25)))
//            {
//                SetEnum(ref mode, -1);
//            }
//            if (GUILayout.Button(">", GUILayout.Width(25)))
//            {
//                SetEnum(ref mode, 1);
//            }
//            TODSkyInstance.Ambient.Mode = (TOD_AmbientType)mode;
//            GUILayout.Label(TODSkyInstance.Ambient.Mode.ToString());
//            GUILayout.EndHorizontal();

//            //if (TODSkyInstance.Ambient.Mode == TOD_AmbientType.Color)
//            //{
//            //    var color = TODSkyInstance.AmbientColor;
//            //    EditColor("Ambient Color:", ref color);
//            //    if (color != TODSkyInstance.AmbientColor)
//            //    {
//            //        typeof(TOD_Sky).GetProperty("AmbientColor").SetValue(TODSkyInstance, color, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null, null);
//            //    }
//            //}
//            //else 
//            if (TODSkyInstance.Ambient.Mode == TOD_AmbientType.Spherical)
//            {
//                var color = RenderSettings.ambientLight;
//                EditColor("Skybox Color:", ref color);
//                RenderSettings.ambientLight = color;
//            }
//            else
//            {
//                var sColor = RenderSettings.ambientSkyColor;
//                EditColor("Sky Color:", ref sColor);
//                RenderSettings.ambientSkyColor = sColor;

//                var eColor = RenderSettings.ambientEquatorColor;
//                EditColor("Equator Color:", ref eColor);
//                RenderSettings.ambientEquatorColor = eColor;

//                var gColor = RenderSettings.ambientGroundColor;
//                EditColor("Ground Color:", ref gColor);
//                RenderSettings.ambientGroundColor = gColor;
//            }

//            GUILayout.Label("<b>Reflection</b>");

//            var bounces = (float)RenderSettings.reflectionBounces;
//            EditFloat("Bounces:", ref bounces, 120);
//            RenderSettings.reflectionBounces = (int)bounces;

//            var rIntensity = RenderSettings.reflectionIntensity;
//            EditFloat("Intensity:", ref rIntensity, 120);
//            RenderSettings.reflectionIntensity = rIntensity;

//            GUILayout.Label("<b>Aniso filtering</b>");

//            var aniso = (float)OptionManager.Instance.CurrentGraphicSettings.AnisotropicQuality;
//            EditFloat("Aniso Quality:", ref aniso, 120);
//            if (OptionManager.Instance.CurrentGraphicSettings.AnisotropicQuality != (int)aniso)
//            {
//                OptionManager.Instance.CurrentGraphicSettings.AnisotropicQuality = (int)aniso;
//                OptionManager.Instance.Apply();
//            }

//            GUILayout.EndArea();
//        }

//        private void DisableLights()
//        {
//            if (TODSkyInstance != null)
//            {
//                var skyTransform = TODSkyInstance.gameObject.transform;

//                if (skyTransform.Find("Sun") is Transform sun)
//                {
//                    sun.gameObject.SetActive(false);
//                }
//                if (skyTransform.Find("Light") is Transform light)
//                {
//                    light.gameObject.SetActive(false);
//                }

//                //RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.3f, 1f);
//                //RenderSettings.ambientIntensity = 0.05f;
//            }
//        }

//        //=========== gui helpers =============

//        public static void SetEnum(ref object value, int change)
//        {
//            var type = value.GetType();
//            var names = Enum.GetNames(type).ToList();

//            int newindex = names.IndexOf(value.ToString()) + change;

//            if ((change < 0 && newindex >= 0) || (change > 0 && newindex < names.Count))
//            {
//                value = Enum.Parse(type, names[newindex]);
//            }
//        }

//        public static void EditColor(string label, ref Color color)
//        {
//            GUILayout.Label(label);

//            GUILayout.BeginHorizontal();
//            EditFloat("R:", ref color.r);
//            EditFloat("G:", ref color.g);
//            EditFloat("B:", ref color.b);
//            EditFloat("A:", ref color.a);
//            GUILayout.EndHorizontal();
//        }

//        public static void EditFloat(string label, ref float f, float labelWidth = 30)
//        {
//            GUILayout.Label(label, GUILayout.Width(labelWidth));

//            var s = f.ToString("0.000");
//            s = GUILayout.TextField(s, GUILayout.Width(40));
//            if (float.TryParse(s, out float f2))
//            {
//                f = f2;
//            }
//        }

//        // ================= harmony patches ====================

//        [HarmonyPatch(typeof(TOD_Sky), "UpdateAmbient")]
//        public class TOD_Sky_UpdateAmbient
//        {
//            [HarmonyPrefix]
//            public static bool Prefix()
//            {
//                return false;
//            }
//        }

//        [HarmonyPatch(typeof(EnvironmentConditions), "UpdateAtmosphere")]
//        public class EnvironmentConditions_UpdateAtmosphere
//        {
//            [HarmonyPrefix]
//            public static void Prefix(ref object[] __state)
//            {
//                __state = new object[]
//                {
//                    new Color(RenderSettings.ambientLight.r, RenderSettings.ambientLight.g, RenderSettings.ambientLight.b, RenderSettings.ambientLight.a),
//                    RenderSettings.ambientIntensity
//                };

//                return;
//            }

//            [HarmonyPostfix]
//            public static void Postfix(object[] __state)
//            {
//                RenderSettings.ambientLight = (Color)__state[0];
//                RenderSettings.ambientIntensity = (float)__state[1];
//            }
//        }

//		//[HarmonyPatch(typeof(TOD_Sky), "UpdateShaderProperties")]
//		//public class TOD_Sky_UpdateShaderProperties
//		//{
//		//	[HarmonyPrefix]
//		//	public static bool Prefix(TOD_Sky __instance)
//		//	{
//		//		var self = __instance;

//		//		if (self.Headless)
//		//		{
//		//			return false;
//		//		}

//		//		Shader.SetGlobalColor(self.Resources.ID_SunLightColor, self.SunLightColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_MoonLightColor, self.MoonLightColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_SunSkyColor, self.SunSkyColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_MoonSkyColor, self.MoonSkyColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_SunMeshColor, self.SunMeshColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_MoonMeshColor, self.MoonMeshColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_SunCloudColor, self.SunCloudColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_MoonCloudColor, self.MoonCloudColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_FogColor, self.FogColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_GroundColor, self.GroundColor);
//		//		Shader.SetGlobalColor(self.Resources.ID_AmbientColor, self.AmbientColor);
//		//		Shader.SetGlobalVector(self.Resources.ID_SunDirection, self.SunDirection);
//		//		Shader.SetGlobalVector(self.Resources.ID_MoonDirection, self.MoonDirection);
//		//		Shader.SetGlobalVector(self.Resources.ID_LightDirection, self.LightDirection);
//		//		Shader.SetGlobalVector(self.Resources.ID_LocalSunDirection, self.LocalSunDirection);
//		//		Shader.SetGlobalVector(self.Resources.ID_LocalMoonDirection, self.LocalMoonDirection);
//		//		Shader.SetGlobalVector(self.Resources.ID_LocalLightDirection, self.LocalLightDirection);
//		//		Shader.SetGlobalFloat(self.Resources.ID_Contrast, self.Atmosphere.Contrast);
//		//		Shader.SetGlobalFloat(self.Resources.ID_Brightness, self.Atmosphere.Brightness);
//		//		Shader.SetGlobalFloat(self.Resources.ID_Fogginess, self.Atmosphere.Fogginess);
//		//		Shader.SetGlobalFloat(self.Resources.ID_Directionality, self.Atmosphere.Directionality);
//		//		Shader.SetGlobalFloat(self.Resources.ID_MoonHaloPower, 1f / self.Moon.HaloSize);
//		//		Shader.SetGlobalColor(self.Resources.ID_MoonHaloColor, self.MoonHaloColor);
//		//		float value = Mathf.Lerp(0.8f, 0f, self.Clouds.Coverage);
//		//		float num = Mathf.Lerp(3f, 9f, self.Clouds.Sharpness);
//		//		float value2 = Mathf.Lerp(0f, 1f, self.Clouds.Attenuation);
//		//		float value3 = Mathf.Lerp(0f, 2f, self.Clouds.Saturation);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudOpacity, self.Clouds.Opacity);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudCoverage, value);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudSharpness, 1f / num);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudDensity, num);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudColoring, self.Clouds.Coloring);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudAttenuation, value2);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudSaturation, value3);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudScattering, self.Clouds.Scattering);
//		//		Shader.SetGlobalFloat(self.Resources.ID_CloudBrightness, self.Clouds.Brightness);
//		//		Shader.SetGlobalVector(self.Resources.ID_CloudOffset, self.Components.Animation.OffsetUV);
//		//		Shader.SetGlobalVector(self.Resources.ID_CloudWind, self.Components.Animation.CloudUV);
//		//		Shader.SetGlobalVector(self.Resources.ID_CloudSize, new Vector3(self.Clouds.Size * 4f, self.Clouds.Size, self.Clouds.Size * 4f));
//		//		Shader.SetGlobalFloat(self.Resources.ID_StarSize, self.Stars.Size);
//		//		Shader.SetGlobalFloat(self.Resources.ID_StarBrightness, self.Stars.Brightness);
//		//		Shader.SetGlobalFloat(self.Resources.ID_StarVisibility, (1f - self.Atmosphere.Fogginess) * (1f - self.LerpValue));
//		//		Shader.SetGlobalFloat(self.Resources.ID_SunMeshContrast, 1f / Mathf.Max(0.001f, self.Sun.MeshContrast));
//		//		Shader.SetGlobalFloat(self.Resources.ID_SunMeshBrightness, self.Sun.MeshBrightness * (1f - self.Atmosphere.Fogginess));
//		//		Shader.SetGlobalFloat(self.Resources.ID_MoonMeshContrast, 1f / Mathf.Max(0.001f, self.Moon.MeshContrast));
//		//		Shader.SetGlobalFloat(self.Resources.ID_MoonMeshBrightness, self.Moon.MeshBrightness * (1f - self.Atmosphere.Fogginess));
//		//		Shader.SetGlobalVector(self.Resources.ID_kBetaMie, (Vector3)At.GetValue(typeof(TOD_Sky), m_TODSkyInstance, "kBetaMie"));
//		//		Shader.SetGlobalVector(self.Resources.ID_kSun, (Vector4)At.GetValue(typeof(TOD_Sky), m_TODSkyInstance, "kSun"));
//		//		Shader.SetGlobalVector(self.Resources.ID_k4PI, (Vector4)At.GetValue(typeof(TOD_Sky), m_TODSkyInstance, "k4PI"));
//		//		Shader.SetGlobalVector(self.Resources.ID_kRadius, (Vector4)At.GetValue(typeof(TOD_Sky), m_TODSkyInstance, "kRadius"));
//		//		Shader.SetGlobalVector(self.Resources.ID_kScale, (Vector4)At.GetValue(typeof(TOD_Sky), m_TODSkyInstance, "kScale"));
//		//		Shader.SetGlobalMatrix(self.Resources.ID_World2Sky, self.Components.DomeTransform.worldToLocalMatrix);
//		//		Shader.SetGlobalMatrix(self.Resources.ID_Sky2World, self.Components.DomeTransform.localToWorldMatrix);

//		//		return false;
//		//	}
//		//}
//    }
//}

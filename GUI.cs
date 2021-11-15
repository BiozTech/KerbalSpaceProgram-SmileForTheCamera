using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;

namespace SmileForTheCamera
{

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class SmileForTheCameraGUI : MonoBehaviour
	{

		static List<AnimatedKerbal> kerbals = Core.AnimatedKerbals;
		static string[] kerbalParts = { " Head", "EyeL", "EyeR" };
		static string[] rotationParams = { "Min", "Left", "Right", "Max" };

		static bool wasGUIVisible = Core.IsGUIVisible;
		static Rect windowPosition = new Rect(UnityEngine.Random.Range(0.23f, 0.27f) * Screen.width, UnityEngine.Random.Range(0.13f, 0.17f) * Screen.height, 0, 0);

		void OnGUI()
		{
			if (Core.IsGUIVisible)
			{
				GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
				windowStyle.padding = new RectOffset(15, 15, 20, 15);
				windowPosition = GUILayout.Window(("SmileForTheCameraGUI").GetHashCode(), windowPosition, OnWindow, "SmileForTheCamera", windowStyle, GUILayout.Width(582f), GUILayout.Height(81f));
			}
		}

		void OnWindow(int windowId)
		{

			GUILayoutOption[] textFieldHeadLayoutOptions = { GUILayout.Width(40f), GUILayout.Height(20f) };
			GUILayoutOption[] textFieldBodyPositionLayoutOptions = { GUILayout.Width(181f), GUILayout.Height(20f) };
			GUILayoutOption[] textFieldBodyRotationLayoutOptions = { GUILayout.Width(63f), GUILayout.Height(20f) };//63
			GUILayoutOption[] sliderLayoutOptions = { GUILayout.Width(114f), GUILayout.Height(20f) };//114
			//GUILayoutOption[] labelParamsLayoutOptions = { GUILayout.Width(40f), GUILayout.Height(20f) };
			GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField) { padding = new RectOffset(4, 4, 3, 3) };
			//GUIStyle textFieldBodyPositionStyle = new GUIStyle(GUI.skin.textField) { padding = new RectOffset(4, 4, 3, 3) };
			GUIStyle boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(4, 4, 3, 3), alignment = TextAnchor.MiddleLeft };
			//GUIStyle labelKerbalPartsStyle = new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 4, 4) };
			//GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle) { margin = new RectOffset(4, 4, 5, 5) };
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { margin = new RectOffset(4, 5, 4, 4) };
			GUIStyle sliderStyle = new GUIStyle(GUI.skin.horizontalSlider) { margin = new RectOffset(4, 4, 8, 8) };
			GUIStyle sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);

			GUILayout.BeginVertical();

			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			GUILayout.Label(" Animating", GUILayout.Width(100f));
			if (Core.IsEnabled != GUILayout.Toggle(Core.IsEnabled, Core.IsEnabled ? " Enabled" : " Disabled"))
			{
				Core.IsEnabled = !Core.IsEnabled;
				Core.ResetAnimatedKerbals();
				Core.WereNoKerbalsFound = Core.AnimatedKerbals.Count == 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(15f);

			if (!Core.IsEnabled) ColorizeFieldIsEnabled(textFieldStyle, false);
			if (kerbals.Count != 0)
			{
				for (int id = kerbals.Count - 1; id >= 0; id--)
				{
					// god, please don't let me die
					if (kerbals[id] == null || kerbals[id].kerbalEVA == null) continue;
					GUILayout.BeginHorizontal();
					//if (kerbals[id].isAnimated != GUILayout.Toggle(kerbals[id].isAnimated, " sdsda", GUILayout.Width(452f)))
					if (kerbals[id].isAnimated != GUILayout.Toggle(kerbals[id].isAnimated, " " + kerbals[id].kerbalEVA.vessel.vesselName, GUILayout.Width(452f)))
					{
						kerbals[id].isAnimated = !kerbals[id].isAnimated;
						kerbals[id].ResetBodyTransform();
					}
					if (GUILayout.Button("Reset angles", buttonStyle, GUILayout.Width(95f)))
					{
						kerbals[id].ResetHeadAngles();
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); // head eyeL eyeR
					for (int i = 0; i < 3; i++)
					{
						GUILayout.BeginVertical(); // min left right max
						if (i == 0)
						{
							if (kerbals[id].isHeadAnimated != GUILayout.Toggle(kerbals[id].isHeadAnimated, kerbalParts[i])) { kerbals[id].isHeadAnimated = !kerbals[id].isHeadAnimated; }
						} else {
							GUILayout.Label(kerbalParts[i], textFieldHeadLayoutOptions);
						}
						for (int j = 0; j < 4; j++)
						{
							GUILayout.BeginHorizontal(); // x y z
							GUILayout.Label(rotationParams[j], textFieldHeadLayoutOptions);
							if (Core.IsEnabled && kerbals[id].isAnimated && kerbals[id].isHeadAnimated)
							{
								for (int k = 0; k < 3; k++)
								{
									FloatToTextField(ref kerbals[id].rotation[i][j][k], ref kerbals[id].strRotation[i][j][k], textFieldStyle, textFieldHeadLayoutOptions);
								}
							} else {
								Vector3 localEulerAngles = (i == 0) ? kerbals[id].neck.localEulerAngles : ( (i == 1) ? kerbals[id].eyeL.localEulerAngles : kerbals[id].eyeR.localEulerAngles );
								for (int k = 0; k < 3; k++)
								{
									GUILayout.Box((j == 0) ? localEulerAngles[k].ToString("F1") : string.Empty, boxStyle, textFieldHeadLayoutOptions);
								}
							}
							GUILayout.EndHorizontal();
						}
						GUILayout.EndVertical();
						GUILayout.Space((i != 2) ? 13f : 1f);
					}
					GUILayout.EndHorizontal();

					if (kerbals[id].isBodyAnimated != GUILayout.Toggle(kerbals[id].isBodyAnimated, " Body"))
					{
						kerbals[id].isBodyAnimated = !kerbals[id].isBodyAnimated;
						kerbals[id].ResetBodyTransform();
					}
					if (Core.IsEnabled && kerbals[id].isAnimated && kerbals[id].isBodyAnimated)
					{
						GUILayout.BeginHorizontal();
						FloatToTextField(ref kerbals[id].bodyPosition.x, ref kerbals[id].strBodyPosition[0], textFieldStyle, textFieldBodyPositionLayoutOptions);
						FloatToTextField(ref kerbals[id].bodyPosition.y, ref kerbals[id].strBodyPosition[1], textFieldStyle, textFieldBodyPositionLayoutOptions);
						FloatToTextField(ref kerbals[id].bodyPosition.z, ref kerbals[id].strBodyPosition[2], textFieldStyle, textFieldBodyPositionLayoutOptions);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						for (int k = 0; k < 3; k++)
						{
							float value = kerbals[id].bodyRotation[k];
							FloatToTextField(ref value, ref kerbals[id].strBodyRotation[k], textFieldStyle, textFieldBodyRotationLayoutOptions);
							kerbals[id].bodyRotation[k] = value;
							kerbals[id].bodyRotation[k] = GUILayout.HorizontalSlider(kerbals[id].bodyRotation[k], 0f, 360f, sliderStyle, sliderThumbStyle, sliderLayoutOptions);
							if (kerbals[id].bodyRotation[k] != value) kerbals[id].strBodyRotation[k] = kerbals[id].bodyRotation[k].ToString(); // the one who's in the way, he will help us
						}
						GUILayout.EndHorizontal();
					} else {
						GUILayout.BeginHorizontal();
						for (int k = 0; k < 3; k++)
							GUILayout.Box(kerbals[id].body.localPosition[k].ToString(), boxStyle, textFieldBodyPositionLayoutOptions);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						for (int k = 0; k < 3; k++)
						{
							GUILayout.Box(kerbals[id].body.localEulerAngles[k].ToString("F1"), boxStyle, textFieldBodyRotationLayoutOptions);
							GUILayout.HorizontalSlider(kerbals[id].body.localEulerAngles[k], 0f, 360f, sliderStyle, sliderThumbStyle, sliderLayoutOptions);
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.Space((id != 0) ? 15f : 4f);
				}
			} else {
				if (Core.WereNoKerbalsFound)
				{
					GUILayout.Label(" No kerbals found to animate (•ิ_•ิ)?  ԅ(≖‿≖ԅ)");
					GUILayout.Space(15f);
				}
			}

			GUILayout.EndVertical();

			GUI.DragWindow();

		}

		static void FloatToTextField(ref float value, ref string strValue, GUIStyle guiStyle, GUILayoutOption[] guiLayoutOption)
		{
			ColorizeFieldIsWrong(guiStyle, float.TryParse(strValue.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out value));
			strValue = GUILayout.TextField(strValue, guiStyle, guiLayoutOption);
		}

		static void ColorizeFieldIsWrong(GUIStyle style, bool isOk)
		{
			style.normal.textColor = style.hover.textColor = style.active.textColor = style.focused.textColor = style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = style.onFocused.textColor = (isOk) ? Color.white : Color.red;
		}
		static void ColorizeFieldIsEnabled(GUIStyle style, bool isOk)
		{
			style.normal.textColor = style.hover.textColor = style.active.textColor = style.focused.textColor = style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = style.onFocused.textColor = (isOk) ? Color.white : Color.grey;
		}

		public static void ToggleByButton()
		{
			Core.IsGUIVisible = !Core.IsGUIVisible;
		}
		void OnShowUI()
		{
			if (wasGUIVisible) ToggleByButton();
			wasGUIVisible = false;
		}
		void OnHideUI()
		{
			wasGUIVisible = Core.IsGUIVisible;
			if (wasGUIVisible) ToggleByButton();
		}

		/*public static void Initialize()
		{
			//strDistanceMax = Settings.DistanceMax.ToString();
			//strDirectory = Settings.ScenesDirectoryToOutput();
			//isStrDistanceMaxOk = isStrDirectoryOk = true;
		}*/

		void Awake()
		{
			DontDestroyOnLoad(gameObject);
			GameEvents.onShowUI.Add(OnShowUI);
			GameEvents.onHideUI.Add(OnHideUI);
			//Initialize();
			//Core.AnimatedKerbals.Add(new AnimatedKerbal());
			//Core.AnimatedKerbals.Add(new AnimatedKerbal());
		}

	}

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class SmileForTheCameraSceneSwitcher : MonoBehaviour
	{
		void Awake()//dsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjk
		{
			Core.IsEnabled = Core.WereNoKerbalsFound = false;
			Core.AnimatedKerbals.Clear();
		}
	}

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class SmileForTheCameraToolbarButton : MonoBehaviour
	{

		public static ApplicationLauncherButton Button;
		public static bool IsButtonAdded = false;

		void Start()
		{

			if (!IsButtonAdded && ApplicationLauncher.Instance != null)
			{
				var resources = new System.Resources.ResourceManager("SmileForTheCamera.Resources", typeof(SmileForTheCameraToolbarButton).Assembly);
				Byte[] bytes = resources.GetObject("ToolbarIcon") as Byte[];
				Texture2D buttonTexture = new Texture2D(38, 38, TextureFormat.ARGB32, false);
				buttonTexture.LoadRawTextureData(bytes);
				buttonTexture.Apply();
				Button = ApplicationLauncher.Instance.AddModApplication(SmileForTheCameraGUI.ToggleByButton, SmileForTheCameraGUI.ToggleByButton, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
				IsButtonAdded = true;
			}
		//	Texture2D pngToTexture = GameDatabase.Instance.GetTexture("CameraTools/Textures/icon", false);
		//	Byte[] textureToBytes = pngToTexture.GetRawTextureData();
		//	File.WriteAllBytes("ToolbarIcon.txt", textureToBytes);

		}

	}

}

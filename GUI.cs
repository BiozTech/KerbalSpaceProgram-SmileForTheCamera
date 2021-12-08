using System;
using UnityEngine;
using KSP.UI.Screens;

namespace SmileForTheCamera
{

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class SmileForTheCameraGUI : MonoBehaviour
	{

		static bool wasGUIVisible = Core.IsGUIVisible;
		static Rect windowPosition = new Rect(UnityEngine.Random.Range(0.23f, 0.27f) * Screen.width, UnityEngine.Random.Range(0.13f, 0.17f) * Screen.height, 0, 0);
		static GUILayoutOption[] textFieldHeadLayoutOptions, textFieldBodyPositionLayoutOptions, textFieldBodyRotationLayoutOptions, sliderLayoutOptions, sliderLayoutOptions1, buttonResetLayoutOptions;
		static GUIStyle textFieldStyle, boxStyle, labelStyle, toggleStyle, buttonResetStyle, sliderStyle, sliderStyle1, sliderThumbStyle;

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

			textFieldHeadLayoutOptions = new[] { GUILayout.Width(40f), GUILayout.Height(20f) };
			textFieldBodyPositionLayoutOptions = new[] { GUILayout.Width(181f), GUILayout.Height(20f) };
			textFieldBodyRotationLayoutOptions = new[] { GUILayout.Width(63f), GUILayout.Height(20f) };
			sliderLayoutOptions = new[] { GUILayout.Width(114f), GUILayout.Height(20f) };
			sliderLayoutOptions1 = new[] { GUILayout.Width(181f), GUILayout.Height(11f) };
			buttonResetLayoutOptions = new[] { GUILayout.Width(95f), GUILayout.Height(20f) };
			textFieldStyle = new GUIStyle(GUI.skin.textField) { padding = new RectOffset(4, 4, 3, 3) };
			boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(4, 4, 3, 3), alignment = TextAnchor.MiddleLeft };
			labelStyle = new GUIStyle(GUI.skin.label) { padding = new RectOffset(4, 4, 3, 0) };
			toggleStyle = new GUIStyle(GUI.skin.toggle) { margin = new RectOffset(4, 4, 4, 6) };
			buttonResetStyle = new GUIStyle(GUI.skin.button) { margin = new RectOffset(37, 4, 4, 4), padding = new RectOffset(4, 4, 0, 0) };
			sliderStyle = new GUIStyle(GUI.skin.horizontalSlider) { margin = new RectOffset(4, 4, 8, 8) };
			sliderStyle1 = new GUIStyle(GUI.skin.horizontalSlider);
			sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);

			GUILayout.BeginVertical();

			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			GUILayout.Label(" Animating", GUILayout.Width(100f));
			if (Core.IsEnabled != GUILayout.Toggle(Core.IsEnabled, Core.IsEnabled ? " Enabled" : " Disabled", GUILayout.Width(340f)))
			{
				Core.IsEnabled = !Core.IsEnabled;
				if (Core.IsEnabled && FlightGlobals.ActiveVessel != null) Core.InitialOrbit = new Orbit(FlightGlobals.ActiveVessel.orbit); // don't care because null ActiveVessel means AnimatedKerbals.Count == 0
				Core.ResetAnimatedKerbals();
				Core.WereNoKerbalsFound = Core.AnimatedKerbals.Count == 0;
			}
			if (Core.IsEditorOpen != GUILayout.Toggle(Core.IsEditorOpen, " Edit alignment"))
			{
				Core.IsEditorOpen = !Core.IsEditorOpen;
			}
			GUILayout.EndHorizontal();
			if (Core.IsEditorOpen)
			{
				GUILayout.Space(11f);
				for (int i = 0; i < 2; i++) // male female
				{
					for (int j = 0; j < 3; j++) // head eyeL eyeR
					{
						GUILayout.Label(String.Format(" {0} ({1})", Settings.configTagsPart[j], Settings.configTagsGender[i]));
						for (int k = 0; k < 3; k++) // x y z
						{
							GUILayout.BeginHorizontal();
							TextFieldFromFloat(ref Settings.OffsetAdjustment[i][j][k], ref Settings.strOffsetAdjustment[i][j][k], textFieldBodyRotationLayoutOptions);
							float value = Settings.OffsetAdjustment[i][j][k];
							Settings.OffsetAdjustment[i][j][k] = GUILayout.HorizontalSlider(Settings.OffsetAdjustment[i][j][k], -180f, 180f, sliderStyle, sliderThumbStyle);
							if (Settings.OffsetAdjustment[i][j][k] != value) Settings.strOffsetAdjustment[i][j][k] = Settings.OffsetAdjustment[i][j][k].ToString();
							GUILayout.EndHorizontal();
						}
					}
					GUILayout.Space((i == 0) ? 10f : 4f);
				}
			}

			if (!Core.IsEnabled) ColorizeFieldIsEnabled(textFieldStyle, false);
			if (Core.AnimatedKerbals.Count != 0)
			{
				for (int id = Core.AnimatedKerbals.Count - 1; id >= 0; id--)
				{
					// god, please don't let me die
					AnimatedKerbal kerbal = Core.AnimatedKerbals[id];
					if (kerbal == null || kerbal.kerbalEVA == null)
					{
						continue;
					}
					GUILayout.Space(15f);
					GUILayout.BeginHorizontal();
					if (kerbal.isAnimated != GUILayout.Toggle(kerbal.isAnimated, " " + kerbal.name, GUILayout.Width(452f)))
					{
						kerbal.isAnimated = !kerbal.isAnimated;
						kerbal.ResetBodyTransform();
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					for (int i = 0; i < 3; i++) // head eyeL eyeR
					{
						GUILayout.BeginVertical();
						GUILayout.BeginHorizontal();
						if (i == 0)
						{
							if (kerbal.isHeadAnimated != GUILayout.Toggle(kerbal.isHeadAnimated, " " + Settings.configTagsPart[i], toggleStyle)) { kerbal.isHeadAnimated = !kerbal.isHeadAnimated; }
						} else {
							GUILayout.Label(Settings.configTagsPart[i], labelStyle, textFieldHeadLayoutOptions);
						}
						if (i == 2)
						{
							if (GUILayout.Button("Reset angles", buttonResetStyle, buttonResetLayoutOptions)) { kerbal.ResetHeadAngles(); }
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						for (int k = 0; k < 4; k++) GUILayout.Label(Settings.configTagsParam[k], labelStyle, textFieldHeadLayoutOptions);
						GUILayout.EndHorizontal();

						for (int j = 0; j < 3; j++) // x y z
						{
							GUILayout.BeginHorizontal();
							if (Core.IsEnabled && kerbal.isAnimated && kerbal.isHeadAnimated)
							{
								for (int k = 0; k < 4; k++) // min left right max
								{
									TextFieldFromFloat(ref kerbal.headRotation[i][j][k], ref kerbal.strHeadRotation[i][j][k], textFieldHeadLayoutOptions);
								}
							} else {
								Vector3 localEulerAngles = (i == 0) ? kerbal.neck.localEulerAngles : ( (i == 1) ? kerbal.eyeL.localEulerAngles : kerbal.eyeR.localEulerAngles );
								for (int k = 0; k < 4; k++)
								{
									GUILayout.Box((k == 0) ? localEulerAngles[j].ToString("F1") : string.Empty, boxStyle, textFieldHeadLayoutOptions);
								}
							}
							GUILayout.EndHorizontal();
						}
						GUILayout.EndVertical();
						GUILayout.Space((i != 2) ? 13f : 1f);
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(4f);

					if (kerbal.isBodyAnimated != GUILayout.Toggle(kerbal.isBodyAnimated, " Body"))
					{
						kerbal.isBodyAnimated = !kerbal.isBodyAnimated;
						kerbal.ResetBodyTransform();
					}
					ThingTransformTextFields(kerbal, kerbal.isAnimated && kerbal.isBodyAnimated);
					GUILayout.Space(4f);
				}
			} else {
				if (Core.WereNoKerbalsFound)
				{
					GUILayout.Space(15f);
					GUILayout.Label(" No kerbals found to animate (•ิ_•ิ)?  ԅ(≖‿≖ԅ)");
					if (Core.AnimatedVessels.Count == 0) GUILayout.Space(15f);
				}
			}

			for (int id = Core.AnimatedVessels.Count - 1; id >= 0; id--)
			{
				AnimatedVessel vessel = Core.AnimatedVessels[id];
				if (vessel == null || vessel.vessel == null)
				{
					continue;
				}
				GUILayout.Space(15f);
				GUILayout.BeginHorizontal();
				if (vessel.isAnimated != GUILayout.Toggle(vessel.isAnimated, " " + vessel.name, GUILayout.Width(452f)))
				{
					vessel.isAnimated = !vessel.isAnimated;
					vessel.ResetTransform();
				}
				GUILayout.EndHorizontal();
				ThingTransformTextFields(vessel, vessel.isAnimated);
				GUILayout.Space(4f);
			}

			GUILayout.EndVertical();

			GUI.DragWindow();

		}

		static void ThingTransformTextFields(AnimatedThing thing, bool doAnimate)
		{
			if (Core.IsEnabled && doAnimate)
			{
				GUILayout.BeginHorizontal();
				TextFieldFromFloat(ref thing.position.x, ref thing.strPosition[0], textFieldBodyPositionLayoutOptions);
				TextFieldFromFloat(ref thing.position.y, ref thing.strPosition[1], textFieldBodyPositionLayoutOptions);
				TextFieldFromFloat(ref thing.position.z, ref thing.strPosition[2], textFieldBodyPositionLayoutOptions);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
				{
					float value = thing.position[j];
					float integer = Mathf.Floor(value);
					float fraction = value - integer;
					fraction = GUILayout.HorizontalSlider(fraction, 0f, 1f, sliderStyle1, sliderThumbStyle, sliderLayoutOptions1);
					if (fraction != 0f && fraction != 1f) thing.position[j] = integer + fraction;
					if (thing.position[j] != value) thing.strPosition[j] = thing.position[j].ToString();
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
				{
					float value = thing.rotation[j];
					TextFieldFromFloat(ref value, ref thing.strRotation[j], textFieldBodyRotationLayoutOptions);
					thing.rotation[j] = value;
					thing.rotation[j] = GUILayout.HorizontalSlider(thing.rotation[j], 0f, 360f, sliderStyle, sliderThumbStyle, sliderLayoutOptions);
					if (thing.rotation[j] != value) thing.strRotation[j] = thing.rotation[j].ToString();
				}
				GUILayout.EndHorizontal();
			} else {
				Vector3 position = thing.isInitiallyLanded ? thing.transform.position : thing.transform.position - (Vector3)Core.InitialOrbit.getPositionAtUT(Planetarium.GetUniversalTime());
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
					GUILayout.Box(position[j].ToString(), boxStyle, textFieldBodyPositionLayoutOptions);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
					GUILayout.HorizontalSlider(position[j] - Mathf.Floor(position[j]), 0f, 1f, sliderStyle1, sliderThumbStyle, sliderLayoutOptions1);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
				{
					GUILayout.Box(thing.transform.localEulerAngles[j].ToString("F1"), boxStyle, textFieldBodyRotationLayoutOptions);
					GUILayout.HorizontalSlider(thing.transform.localEulerAngles[j], 0f, 360f, sliderStyle, sliderThumbStyle, sliderLayoutOptions);
				}
				GUILayout.EndHorizontal();
			}
		}
		static void TextFieldFromFloat(ref float value, ref string strValue, GUILayoutOption[] guiLayoutOption)
		{
			ColorizeFieldIsWrong(textFieldStyle, float.TryParse(strValue.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out value));
			strValue = GUILayout.TextField(strValue, textFieldStyle, guiLayoutOption);
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

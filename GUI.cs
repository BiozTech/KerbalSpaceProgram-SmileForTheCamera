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
		static GUILayoutOption[] layoutTextFieldRotationHead, layoutTextFieldPosition, layoutTextFieldRotation, layoutSliderPosition, layoutSliderRotation, layoutButtonReset;
		static GUIStyle styleTextField, styleBox, styleLabel, styleToggle, styleButtonReset, styleSliderPosition, styleSliderRotation, styleSliderThumb;

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

			layoutTextFieldRotationHead = new[] { GUILayout.Width(40f), GUILayout.Height(20f) };
			layoutTextFieldPosition = new[] { GUILayout.Width(181f), GUILayout.Height(20f) };
			layoutTextFieldRotation = new[] { GUILayout.Width(63f), GUILayout.Height(20f) };
			layoutSliderPosition = new[] { GUILayout.Width(181f), GUILayout.Height(11f) };
			layoutSliderRotation = new[] { GUILayout.Width(114f), GUILayout.Height(20f) };
			layoutButtonReset = new[] { GUILayout.Width(95f), GUILayout.Height(20f) };
			styleTextField = new GUIStyle(GUI.skin.textField) { padding = new RectOffset(4, 4, 3, 3) };
			styleBox = new GUIStyle(GUI.skin.box) { padding = new RectOffset(4, 4, 3, 3), alignment = TextAnchor.MiddleLeft };
			styleLabel = new GUIStyle(GUI.skin.label) { padding = new RectOffset(4, 4, 3, 0) };
			styleToggle = new GUIStyle(GUI.skin.toggle) { margin = new RectOffset(4, 4, 4, 6) };
			styleButtonReset = new GUIStyle(GUI.skin.button) { margin = new RectOffset(37, 4, 4, 4), padding = new RectOffset(4, 4, 0, 0) };
			styleSliderPosition = new GUIStyle(GUI.skin.horizontalSlider);
			styleSliderRotation = new GUIStyle(GUI.skin.horizontalSlider) { margin = new RectOffset(4, 4, 8, 8) };
			styleSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);

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
							TextFieldFromFloat(ref Settings.OffsetAdjustment[i][j][k], ref Settings.strOffsetAdjustment[i][j][k], layoutTextFieldRotation);
							float value = Settings.OffsetAdjustment[i][j][k];
							Settings.OffsetAdjustment[i][j][k] = GUILayout.HorizontalSlider(Settings.OffsetAdjustment[i][j][k], -180f, 180f, styleSliderRotation, styleSliderThumb);
							if (Settings.OffsetAdjustment[i][j][k] != value) Settings.strOffsetAdjustment[i][j][k] = Settings.OffsetAdjustment[i][j][k].ToString();
							GUILayout.EndHorizontal();
						}
					}
					GUILayout.Space((i == 0) ? 10f : 4f);
				}
			}

			if (!Core.IsEnabled) ColorizeFieldIsEnabled(styleTextField, false);
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
							if (kerbal.isHeadAnimated != GUILayout.Toggle(kerbal.isHeadAnimated, " " + Settings.configTagsPart[i], styleToggle)) { kerbal.isHeadAnimated = !kerbal.isHeadAnimated; }
						} else {
							GUILayout.Label(Settings.configTagsPart[i], styleLabel, layoutTextFieldRotationHead);
						}
						if (i == 2)
						{
							if (GUILayout.Button("Reset angles", styleButtonReset, layoutButtonReset)) { kerbal.ResetHeadAngles(); }
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						for (int k = 0; k < 4; k++) GUILayout.Label(Settings.configTagsParam[k], styleLabel, layoutTextFieldRotationHead);
						GUILayout.EndHorizontal();

						for (int j = 0; j < 3; j++) // x y z
						{
							GUILayout.BeginHorizontal();
							if (Core.IsEnabled && kerbal.isAnimated && kerbal.isHeadAnimated)
							{
								for (int k = 0; k < 4; k++) // min left right max
								{
									TextFieldFromFloat(ref kerbal.headRotation[i][j][k], ref kerbal.strHeadRotation[i][j][k], layoutTextFieldRotationHead);
								}
							} else {
								Vector3 localEulerAngles = (i == 0) ? kerbal.neck.localEulerAngles : ( (i == 1) ? kerbal.eyeL.localEulerAngles : kerbal.eyeR.localEulerAngles );
								for (int k = 0; k < 4; k++)
								{
									GUILayout.Box((k == 0) ? localEulerAngles[j].ToString("F1") : string.Empty, styleBox, layoutTextFieldRotationHead);
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

			for (int id = 0; id < Core.AnimatedVessels.Count; id++)
			{
				AnimatedVessel vessel = Core.AnimatedVessels[id];
				if (vessel == null || vessel.vessel == null)
				{
					continue;
				}
				GUILayout.Space((id == 0) ? 15f : 5f);
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
				TextFieldFromFloat(ref thing.position.x, ref thing.strPosition[0], layoutTextFieldPosition);
				TextFieldFromFloat(ref thing.position.y, ref thing.strPosition[1], layoutTextFieldPosition);
				TextFieldFromFloat(ref thing.position.z, ref thing.strPosition[2], layoutTextFieldPosition);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
				{
					float value = thing.position[j];
					float integer = Mathf.Floor(value);
					float fraction = value - integer;
					fraction = GUILayout.HorizontalSlider(fraction, 0f, 1f, styleSliderPosition, styleSliderThumb, layoutSliderPosition);
					if (fraction != 0f && fraction != 1f) thing.position[j] = integer + fraction;
					if (thing.position[j] != value) thing.strPosition[j] = thing.position[j].ToString();
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
				{
					float value = thing.rotation[j];
					TextFieldFromFloat(ref value, ref thing.strRotation[j], layoutTextFieldRotation);
					thing.rotation[j] = value;
					thing.rotation[j] = GUILayout.HorizontalSlider(thing.rotation[j], 0f, 360f, styleSliderRotation, styleSliderThumb, layoutSliderRotation);
					if (thing.rotation[j] != value) thing.strRotation[j] = thing.rotation[j].ToString(); // the one who's in the way will help us
				}
				GUILayout.EndHorizontal();
			} else {
				Vector3 position = thing.isInitiallyLanded ? thing.transform.position : thing.transform.position - (Vector3)Core.InitialOrbit.getPositionAtUT(Planetarium.GetUniversalTime());
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
					GUILayout.Box(position[j].ToString(), styleBox, layoutTextFieldPosition);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
					GUILayout.HorizontalSlider(position[j] - Mathf.Floor(position[j]), 0f, 1f, styleSliderPosition, styleSliderThumb, layoutSliderPosition);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
				{
					GUILayout.Box(thing.transform.localEulerAngles[j].ToString("F1"), styleBox, layoutTextFieldRotation);
					GUILayout.HorizontalSlider(thing.transform.localEulerAngles[j], 0f, 360f, styleSliderRotation, styleSliderThumb, layoutSliderRotation);
				}
				GUILayout.EndHorizontal();
			}
		}
		static void TextFieldFromFloat(ref float value, ref string strValue, GUILayoutOption[] guiLayoutOption)
		{
			ColorizeFieldIsWrong(styleTextField, float.TryParse(strValue.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out value));
			strValue = GUILayout.TextField(strValue, styleTextField, guiLayoutOption);
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

		void Awake()
		{
			DontDestroyOnLoad(gameObject);
			GameEvents.onShowUI.Add(OnShowUI);
			GameEvents.onHideUI.Add(OnHideUI);
		}

	}

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class SmileForTheCameraSceneSwitcher : MonoBehaviour
	{
		void Awake()
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

		}

	}

}

﻿using System;
using UnityEngine;
using KSP.UI.Screens;

namespace SmileForTheCamera
{

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class SmileForTheCameraGUI : MonoBehaviour
	{

		static bool wasGUIVisible = Core.IsGUIVisible;
		static Rect windowPosition = new Rect(UnityEngine.Random.Range(0.23f, 0.27f) * Screen.width, UnityEngine.Random.Range(0.13f, 0.17f) * Screen.height, 0, 0);
		static GUILayoutOption[] layoutTextFieldRotationHead, layoutTextFieldPosition, layoutTextFieldRotation, layoutSliderPosition, layoutSliderRotation, layoutButton;
		static GUIStyle styleTextField, styleBox, styleToggleHeadAnimated, styleToggleHeadPartLimited, styleLabelHeadParts, styleButtonHeadPartsAlignment, styleButtonResetAngles, styleSliderPosition, styleSliderRotation, styleSliderThumb;

		void OnGUI()
		{
			if (Core.IsGUIVisible)
			{
				GUIStyle windowStyle = new GUIStyle(GUI.skin.window) { padding = new RectOffset(15, 15, 20, 15) };
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
			layoutButton = new[] { GUILayout.Width(95f), GUILayout.Height(20f) };
			styleTextField = new GUIStyle(GUI.skin.textField) { padding = new RectOffset(4, 4, 3, 3) };
			styleBox = new GUIStyle(GUI.skin.box) { padding = new RectOffset(4, 4, 3, 3), alignment = TextAnchor.MiddleLeft };
			styleToggleHeadAnimated = new GUIStyle(GUI.skin.toggle) { margin = new RectOffset(4, 4, 6, 4) };
			styleToggleHeadPartLimited = new GUIStyle(GUI.skin.toggle) { margin = new RectOffset(4, 4, 4, 6) };
			styleLabelHeadParts = new GUIStyle(GUI.skin.label) { padding = new RectOffset(4, 4, 3, 0) };
			styleButtonHeadPartsAlignment = new GUIStyle(GUI.skin.button) { margin = new RectOffset(4, 4, 6, 4), padding = new RectOffset(4, 4, 0, 0) };
			styleButtonResetAngles = new GUIStyle(GUI.skin.button) { margin = new RectOffset(22, 4, 4, 4), padding = new RectOffset(4, 4, 0, 0) };
			styleSliderPosition = new GUIStyle(GUI.skin.horizontalSlider);
			styleSliderRotation = new GUIStyle(GUI.skin.horizontalSlider) { margin = new RectOffset(4, 4, 8, 8) };
			styleSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);

			GUILayout.BeginVertical();

			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			GUILayout.Label(" Animating", GUILayout.Width(100f));
			if (Core.IsEnabled != GUILayout.Toggle(Core.IsEnabled, Core.IsEnabled ? " Enabled" : " Disabled", GUILayout.Width(348f)))
			{
				Core.IsEnabled = !Core.IsEnabled;
				if (Core.IsEnabled) CenterOfUniverse.Reset(); // don't care because null ActiveVessel means AnimatedKerbals.Count == 0 && AnimatedVessels.Count == 0
				Core.ResetAnimatedObjects();
			}
			if (GUILayout.Button("Clear all", layoutButton))
			{
				Core.ClearAnimatedObjects();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(15f);

			if (!Core.IsEnabled) ColorizeFieldIsEnabled(styleTextField, false);
			if (Core.AnyObjectFound)
			{

				for (int id = Core.AnimatedKerbals.Count - 1; id >= 0; id--)
				{
					// god, please don't let me die
					AnimatedKerbal kerbal = Core.AnimatedKerbals[id];
					if (kerbal == null || kerbal.kerbalEVA == null)
					{
						continue;
					}
					if (kerbal.isAnimated != GUILayout.Toggle(kerbal.isAnimated, " " + kerbal.name))
					{
						kerbal.isAnimated = !kerbal.isAnimated;
						kerbal.ResetBody();
					}
					GUILayout.BeginHorizontal();
					if (kerbal.isHeadAnimated != GUILayout.Toggle(kerbal.isHeadAnimated, " Head", styleToggleHeadAnimated, GUILayout.Width(55f)))
					{
						kerbal.isHeadAnimated = !kerbal.isHeadAnimated;
					}
					if (kerbal.isFaceFreezed != GUILayout.Toggle(kerbal.isFaceFreezed, " Freeze face", styleToggleHeadAnimated, GUILayout.Width(294f)))
					{
						kerbal.isFaceFreezed = !kerbal.isFaceFreezed;
						kerbal.ResetFaceBones();
					}
					if (GUILayout.Button("Set alignment", styleButtonHeadPartsAlignment, layoutButton))
					{
						kerbal.isEditingOffset = !kerbal.isEditingOffset;
					}
					if (GUILayout.Button("Set limits", styleButtonHeadPartsAlignment, layoutButton))
					{
						kerbal.isEditingLimits = !kerbal.isEditingLimits;
					}
					GUILayout.EndHorizontal();

					if (kerbal.isEditingOffset)
					{
						for (int i = 0; i < 3; i++) // head eyeL eyeR
						{
							GUILayout.Label(" " + Settings.configTagsPart[i]);
							for (int j = 0; j < 3; j++) // x y z
							{
								GUILayout.BeginHorizontal();
								TextFieldFromFloat(ref kerbal.headOffset[i][j], ref kerbal.strHeadOffset[i][j], layoutTextFieldRotation);
								float value = kerbal.headOffset[i][j];
								kerbal.headOffset[i][j] = GUILayout.HorizontalSlider(kerbal.headOffset[i][j], -180f, 180f, styleSliderRotation, styleSliderThumb, GUILayout.Width(484f));
								if (kerbal.headOffset[i][j] != value) kerbal.strHeadOffset[i][j] = kerbal.headOffset[i][j].ToString();
								GUILayout.EndHorizontal();
							}
						}
						GUILayout.Space(6f);
					}

					if (kerbal.isEditingLimits)
					{
						GUILayout.BeginHorizontal();
						for (int i = 0; i < 3; i++) // head eyeL eyeR
						{
							GUILayout.BeginVertical();
							if (i != 2)
							{
								if (kerbal.isHeadPartLimited[i] != GUILayout.Toggle(kerbal.isHeadPartLimited[i], " " + Settings.configTagsPart[i], styleToggleHeadPartLimited)) { kerbal.isHeadPartLimited[i] = !kerbal.isHeadPartLimited[i]; }
							} else {
								GUILayout.BeginHorizontal();
								if (kerbal.isHeadPartLimited[i] != GUILayout.Toggle(kerbal.isHeadPartLimited[i], " " + Settings.configTagsPart[i], styleToggleHeadPartLimited)) { kerbal.isHeadPartLimited[i] = !kerbal.isHeadPartLimited[i]; }
								if (GUILayout.Button("Reset angles", styleButtonResetAngles, layoutButton)) { kerbal.ResetHeadAngles(); }
								GUILayout.EndHorizontal();
							}
							GUILayout.BeginHorizontal();
							for (int k = 0; k < 4; k++) GUILayout.Label(Settings.configTagsParam[k], styleLabelHeadParts, layoutTextFieldRotationHead);
							GUILayout.EndHorizontal();

							for (int j = 0; j < 3; j++) // x y z
							{
								GUILayout.BeginHorizontal();
								if (Core.IsEnabled && kerbal.isAnimated && kerbal.isHeadAnimated)
								{
									for (int k = 0; k < 4; k++) // min left right max
									{
										TextFieldFromFloat(ref kerbal.headLimits[i][j][k], ref kerbal.strHeadLimits[i][j][k], layoutTextFieldRotationHead);
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
					}

					if (kerbal.isBodyAnimated != GUILayout.Toggle(kerbal.isBodyAnimated, " Body"))
					{
						kerbal.isBodyAnimated = !kerbal.isBodyAnimated;
						kerbal.ResetBody();
					}
					ThingTransformTextFields(kerbal, kerbal.isAnimated && kerbal.isBodyAnimated);
					if (id != 0) GUILayout.Space(9f);
				}

				if (Core.AnimatedKerbals.Count != 0) GUILayout.Space((Core.AnimatedVessels.Count != 0) ? 19f : 4f);

				for (int id = 0; id < Core.AnimatedVessels.Count; id++)
				{
					AnimatedVessel vessel = Core.AnimatedVessels[id];
					if (vessel == null || vessel.vessel == null)
					{
						continue;
					}
					GUILayout.BeginHorizontal();
					if (vessel.isAnimated != GUILayout.Toggle(vessel.isAnimated, " " + vessel.name, GUILayout.Width(452f)))
					{
						vessel.isAnimated = !vessel.isAnimated;
						vessel.ResetTransform();
					}
					GUILayout.EndHorizontal();
					ThingTransformTextFields(vessel, vessel.isAnimated);
					GUILayout.Space((id == 0) ? 9f : 4f);
				}

			} else {
				GUILayout.Label(" No objects found to animate (•ิ_•ิ)?  ԅ(≖‿≖ԅ)");
				GUILayout.Space(15f);
			}

			GUILayout.EndVertical();

			GUI.DragWindow();

		}

		static void ThingTransformTextFields(AnimatedObject obj, bool doAnimate)
		{
			if (Core.IsEnabled && doAnimate)
			{
				GUILayout.BeginHorizontal();
				TextFieldFromFloat(ref obj.position.x, ref obj.strPosition[0], layoutTextFieldPosition);
				TextFieldFromFloat(ref obj.position.y, ref obj.strPosition[1], layoutTextFieldPosition);
				TextFieldFromFloat(ref obj.position.z, ref obj.strPosition[2], layoutTextFieldPosition);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
				{
					float value = obj.position[j];
					float integer = Mathf.Floor(value);
					float fraction = value - integer;
					fraction = GUILayout.HorizontalSlider(fraction, 0f, 1f, styleSliderPosition, styleSliderThumb, layoutSliderPosition);
					if (fraction != 0f && fraction != 1f) obj.position[j] = integer + fraction;
					if (obj.position[j] != value) obj.strPosition[j] = obj.position[j].ToString();
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				for (int j = 0; j < 3; j++)
				{
					float value = obj.rotation[j];
					TextFieldFromFloat(ref value, ref obj.strRotation[j], layoutTextFieldRotation);
					obj.rotation[j] = value;
					obj.rotation[j] = GUILayout.HorizontalSlider(obj.rotation[j], 0f, 360f, styleSliderRotation, styleSliderThumb, layoutSliderRotation);
					if (obj.rotation[j] != value) obj.strRotation[j] = obj.rotation[j].ToString(); // the one who's in the way will help us
				}
				GUILayout.EndHorizontal();
			} else {
				Vector3 position = CenterOfUniverse.PositionWorldToCenter(obj);
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
					GUILayout.Box(obj.transform.localEulerAngles[j].ToString("F1"), styleBox, layoutTextFieldRotation);
					GUILayout.HorizontalSlider(obj.transform.localEulerAngles[j], 0f, 360f, styleSliderRotation, styleSliderThumb, layoutSliderRotation);
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
			Core.IsEnabled = false;
			Core.AnyObjectFound = true;
			Core.AnimatedKerbals.Clear();
			Core.AnimatedVessels.Clear();
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
				Byte[] bytes = resources.GetObject("SmileForTheCameraToolbarIcon") as Byte[];
				Texture2D buttonTexture = new Texture2D(38, 38, TextureFormat.ARGB32, false);
				buttonTexture.LoadRawTextureData(bytes);
				buttonTexture.Apply();
				Button = ApplicationLauncher.Instance.AddModApplication(SmileForTheCameraGUI.ToggleByButton, SmileForTheCameraGUI.ToggleByButton, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
				IsButtonAdded = true;
			}

		}

	}

}

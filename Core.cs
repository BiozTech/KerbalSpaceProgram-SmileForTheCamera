using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace SmileForTheCamera
{

	public static class Core
	{

		public static bool IsEnabled = false, IsGUIVisible = false, AnyObjectFound = false;
		public static List<AnimatedKerbal> AnimatedKerbals = new List<AnimatedKerbal>();
		public static List<AnimatedVessel> AnimatedVessels = new List<AnimatedVessel>();

		public static void ResetAnimatedObjects()
		{
			List<AnimatedKerbal> newKerbals = new List<AnimatedKerbal>();
			List<AnimatedVessel> newVessels = new List<AnimatedVessel>();
			foreach (KerbalEVA kerbalEVA in UnityEngine.Object.FindObjectsOfType<KerbalEVA>())
			{
				AnimatedKerbal newKerbal = AnimatedKerbals.Find(o => o.kerbalEVA == kerbalEVA);
				if (newKerbal != null)
				{
					newKerbal.ResetBody();
					newKerbal.ResetHeadAngles();
				} else {
					newKerbal = new AnimatedKerbal(kerbalEVA);
				}
				newKerbals.Add(newKerbal);
			}
			foreach (Vessel vessel in FlightGlobals.VesselsLoaded.Where(v => !v.isEVA))
			{
				AnimatedVessel newVessel = AnimatedVessels.Find(o => o.vessel == vessel);
				if (newVessel != null)
				{
					newVessel.ResetTransform();
				} else {
					newVessel = new AnimatedVessel(vessel);
				}
				newVessels.Add(newVessel);
			}
			AnimatedKerbals = newKerbals;
			AnimatedVessels = newVessels;
			AnyObjectFound = AnimatedKerbals.Count != 0 || AnimatedVessels.Count != 0;
		}

		public static void ClearAnimatedObjects()
		{
			IsEnabled = AnyObjectFound = false;
			AnimatedKerbals.Clear();
			AnimatedVessels.Clear();
		}

		public static void Log(object message)
		{
			Debug.Log("[SmileForTheCamera] " + message);
		}

		public static Quaternion TurnLeft = Quaternion.Euler(0, -90, 0);

	}

	public static class CenterOfUniverse
	{

		static Vector3 Position;
		static Quaternion Rotation;
		static Orbit Orbit;

		public static void Reset()
		{
			if (FlightGlobals.ActiveVessel != null)
			{
				Position = FlightGlobals.ActiveVessel.transform.position + Vector3.zero; // what is more lame - praying to god or being a god?
				Rotation = FlightGlobals.ActiveVessel.transform.rotation * Quaternion.identity;
				Orbit = new Orbit(FlightGlobals.ActiveVessel.orbit);
			}
		}

		public static Vector3 PositionWorldToCenter(AnimatedObject obj)
		{
			Vector3 position = obj.isInitiallyLanded ? obj.transform.position : obj.transform.position - (Vector3)Orbit.getPositionAtUT(Planetarium.GetUniversalTime());
			return Quaternion.Inverse(Rotation) * (position - Position);
		}
		public static Vector3 PositionCenterToWorld(AnimatedObject obj)
		{
			Vector3 position = Rotation * obj.position + Position;
			return obj.isInitiallyLanded ? position : position + (Vector3)Orbit.getPositionAtUT(Planetarium.GetUniversalTime());
		}

	}

	public static class Settings
	{

		public static float[][][] OffsetDefault = new float[][][]
		{
			// Male
			new float[][] {
				new float[] { 0,  -88, -108 },
				new float[] { 0,  -59,    4 },
				new float[] { 0, -152,   -4 } // (0, 118, -4) + (0, 90, 0)
			},
			// Female
			new float[][] {
				new float[] { 0,  -88, -108 },
				new float[] { 0,  -66,    0 },
				new float[] { 0, -156,   -6 } // (0, 114, -6) + (0, 90, 0)
			}
		};

		public static float[][][][] RotationLimits = new float[][][][]
		{
			// Male
			new float[][][] {
				new float[][] { // min left right max
					new float[] { -15, -10,  10,  15 }, // Head
					new float[] { -10,  -5,   5,  10 },
					new float[] { -12, -10,  -4,   0 }
				},
				new float[][] {
					new float[] { -15, -10,  10,  15 }, // EyeL
					new float[] {   0,   5,  10,  15 },
					new float[] { -10,   0,  10,  30 }
				},
				new float[][] {
					new float[] { -80, -10,  20,  50 }, // EyeR
					new float[] { -40, -35, -25, -20 },
					new float[] { -10,  10,  40,  50 }
				}
			},
			// Female
			new float[][][] {
				new float[][] {
					new float[] { -15, -10,  10,  15 },
					new float[] { -10,  -5,   5,  10 },
					new float[] { -12, -10,  -4,   0 }
				},
				new float[][] {
					new float[] { -15,  -5,  10,  20 },
					new float[] {   5,  10,  15,  20 },
					new float[] { -10,   0,  10,  20 }
				},
				new float[][] {
					new float[] { -20, -10,   0,  10 },
					new float[] { -45, -35, -30, -20 },
					new float[] { -10,   0,  20,  30 }
				}
			}
		};

		static string pluginDataDir = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/BiozTech/PluginData"), settingsFileName = Path.Combine(pluginDataDir, "SmileForTheCameraSettings.cfg");
		static string configTagMain = "SmileForTheCameraSettings", configTagOffset = "Offset", configTagLimits = "Limits";
		public static string[] configTagsGender = { "Male", "Female" }, configTagsPart = { "Head", "EyeL", "EyeR" }, configAxesParam = { "X", "Y", "Z" }, configTagsParam = { "Min", "Left", "Right", "Max" };

		public static void Load()
		{
			if (!File.Exists(settingsFileName))
			{
				Save();
				Core.Log("Have no settings file, huh? Here is one for you (づ｡◕‿‿◕｡)づ");
				return;
			}
			ConfigNode configNode = ConfigNode.Load(settingsFileName);
			if (!configNode.HasNode(configTagMain))
			{
				BackupAndSave();
				Core.Log("General Failure reading settings file");
				return;
			}
			configNode = configNode.GetNode(configTagMain);
			try
			{

				bool isOk = true;
				bool isOkCurrent;

				string tagCurrent;
				for (int i = 0; i < configTagsGender.Length; i++) for (int j = 0; j < configTagsPart.Length; j++)
				{
					tagCurrent = configTagOffset + configTagsGender[i] + configTagsPart[j];
					isOkCurrent = configNode.HasValue(tagCurrent);
					if (isOkCurrent)
					{
						float[] float3;
						isOkCurrent = TryParseFloatN(configNode.GetValue(tagCurrent), out float3, 3);
						if (isOkCurrent) OffsetDefault[i][j] = float3;
					}
					isOk &= isOkCurrent;
				}
				for (int i = 0; i < configTagsGender.Length; i++) for (int j = 0; j < configTagsPart.Length; j++) for (int k = 0; k < configAxesParam.Length; k++)
				{
					tagCurrent = configTagLimits + configTagsGender[i] + configTagsPart[j] + configAxesParam[k];
					isOkCurrent = configNode.HasValue(tagCurrent);
					if (isOkCurrent)
					{
						float[] float4;
						isOkCurrent = TryParseFloatN(configNode.GetValue(tagCurrent), out float4, 4);
						if (isOkCurrent) RotationLimits[i][j][k] = float4;
					}
					isOk &= isOkCurrent;
				}

				if (!isOk)
				{
					BackupAndSave();
					Core.Log("General Failure reading settings file");
				}

			}
			catch (Exception stupid)
			{
				BackupAndSave();
				Core.Log("Not to worry, we are still flying half a ship.");
				Core.Log(stupid.Message);
			}
		}

		public static void Save()
		{
			ConfigNode configNode = new ConfigNode(configTagMain);
			for (int i = 0; i < configTagsGender.Length; i++) for (int j = 0; j < configTagsPart.Length; j++)
			{
				configNode.AddValue(configTagOffset + configTagsGender[i] + configTagsPart[j], FloatNToString(OffsetDefault[i][j], 3));
			}
			for (int i = 0; i < configTagsGender.Length; i++) for (int j = 0; j < configTagsPart.Length; j++) for (int k = 0; k < configAxesParam.Length; k++)
			{
				configNode.AddValue(configTagLimits + configTagsGender[i] + configTagsPart[j] + configAxesParam[k], FloatNToString(RotationLimits[i][j][k], 4));
			}
			if (!Directory.Exists(pluginDataDir)) Directory.CreateDirectory(pluginDataDir);
			File.WriteAllText(settingsFileName, configNode.ToString(), System.Text.Encoding.Unicode);
		}
		static void BackupAndSave()
		{
			string settingsFileBackupName = settingsFileName + ".bak";
			if (File.Exists(settingsFileBackupName)) File.Delete(settingsFileBackupName);
			File.Move(settingsFileName, settingsFileBackupName);
			Save();
		}

		static string FloatNToString(float[] f, int length)
		{
			if (f.Length != length)
			{
				throw new Exception("Array length is not " + length);
			}
			return String.Join(", ", f);
		}
		static bool TryParseFloatN(string s, out float[] result, int length)
		{
			try
			{
				result = Array.ConvertAll(s.Split(','), float.Parse);
			}
			catch (Exception stupid)
			{
				result = null;
				Core.Log(String.Format("Could not parse to float[{0}]: {1}", length, s));
				Core.Log(stupid.Message);
				return false;
			}
			if (result.Length != length)
			{
				Core.Log(String.Format("Could not parse to float[{0}]. Number of parsed values is {1}.", length, result.Length));
				return false;
			}
			return true;
		}

	}

}

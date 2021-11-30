using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SmileForTheCamera
{

	public static class Core
	{

		public static bool IsGUIVisible = false;
		public static bool IsEnabled = false;
		public static bool WereNoKerbalsFound = false;
		public static List<AnimatedKerbal> AnimatedKerbals = new List<AnimatedKerbal>();

		public static void ResetAnimatedKerbals()
		{
			List<AnimatedKerbal> oldKerbals = new List<AnimatedKerbal>(AnimatedKerbals);
			AnimatedKerbals.Clear();
			foreach (KerbalEVA kerbalEVA in UnityEngine.Object.FindObjectsOfType<KerbalEVA>())
			{
				AnimatedKerbal newKerbal = oldKerbals.Find(o => o.kerbalEVA == kerbalEVA) ?? new AnimatedKerbal(kerbalEVA);
				AnimatedKerbals.Add(newKerbal);
				newKerbal.ResetBodyTransform();
				newKerbal.ResetHeadAngles();
			}
		}

		public static void Log(object message)
		{
			Debug.Log("[SmileForTheCamera] " + message);
		}

		public static Quaternion TurnLeft = Quaternion.Euler(0, -90, 0);

	}

	public static class Settings
	{

		public static Quaternion MaleHeadOffset = Quaternion.Euler(0,  -88, -108);
		public static Quaternion MaleEyeLOffset = Quaternion.Euler(0,  -59,    4);
		public static Quaternion MaleEyeROffset = Quaternion.Euler(0, -152,   -4); // (0, 118, -4) + (0, 90, 0)
		public static Quaternion FemaleHeadOffset = Quaternion.Euler(0,  -88, -108);
		public static Quaternion FemaleEyeLOffset = Quaternion.Euler(0,  -66,    0);
		public static Quaternion FemaleEyeROffset = Quaternion.Euler(0, -156,   -6); // (0, 114, -6) + (0, 90, 0)

		public static float[][][] DefaultRotationMale = new float[][][]
		{
			// Head
			new float[][] { //min left right max
				new float[] { -15, -10,  10,  15 },
				new float[] { -10,  -5,   5,  10 },
				new float[] { -12, -10,  -4,   0 }
			},
			// EyeL
			new float[][] {
				new float[] { -15, -10,  10,  15 },
				new float[] {   0,   5,  10,  15 },
				new float[] { -10,   0,  10,  30 }
			},
			// EyeR
			new float[][] {
				new float[] { -80, -10,  20,  50 },
				new float[] { -40, -35, -25, -20 },
				new float[] { -10,  10,  40,  50 }
			}
		};
		public static float[][][] DefaultRotationFemale =
		{
			// Head
			new float[][] {
				new float[] { -15, -10,  10,  15 },
				new float[] { -10,  -5,   5,  10 },
				new float[] { -12, -10,  -4,   0 }
			},
			// EyeL
			new float[][] {
				new float[] { -15,  -5,  10,  20 },
				new float[] {   5,  10,  15,  20 },
				new float[] { -10,   0,  10,  20 }
			},
			// EyeR
			new float[][] {
				new float[] { -20, -10,   0,  10 },
				new float[] { -45, -35, -30, -20 },
				new float[] { -10,   0,  20,  30 }
			}
		};

		static string pluginDataDir = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/BiozTech/PluginData");
		static string settingsFileName = Path.Combine(pluginDataDir, "SmileForTheCameraSettings.cfg");
		static string configTagMain = "SmileForTheCameraSettings";
		static string[] configTagsPart = { "Head", "EyeL", "EyeR" };
		static string[] configGenders = { "Male", "Female" };
		static string[] configTagsParam = { "Min", "Left", "Right", "Max" };

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
				for (int i = 0; i < configTagsPart.Length; i++) for (int j = 0; j < configTagsParam.Length; j++)
				{
					tagCurrent = configTagsPart[i] + configGenders[0] + configTagsParam[j];
					isOkCurrent = configNode.HasValue(tagCurrent);
					if (isOkCurrent)
					{
						float[] float3;
						isOkCurrent = TryParseFloat3(configNode.GetValue(tagCurrent), out float3);
						if (isOkCurrent) DefaultRotationMale[i][j] = float3;
					}
					isOk &= isOkCurrent;
				}
				for (int i = 0; i < configTagsPart.Length; i++) for (int j = 0; j < configTagsParam.Length; j++)
				{
					tagCurrent = configTagsPart[i] + configGenders[1] + configTagsParam[j];
					isOkCurrent = configNode.HasValue(tagCurrent);
					if (isOkCurrent)
					{
						float[] float3;
						isOkCurrent = TryParseFloat3(configNode.GetValue(tagCurrent), out float3);
						if (isOkCurrent) DefaultRotationFemale[i][j] = float3;
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
			//SmileForTheCameraGUI.Initialize();
		}

		public static void Save()
		{
			ConfigNode configNode = new ConfigNode(configTagMain);
			for (int i = 0; i < configTagsPart.Length; i++) for (int j = 0; j < configTagsParam.Length; j++)
			{
				configNode.AddValue(configTagsPart[i] + configGenders[0] + configTagsParam[j], Float3ToString(DefaultRotationMale[i][j]));
			}
			for (int i = 0; i < configTagsPart.Length; i++) for (int j = 0; j < configTagsParam.Length; j++)
			{
				configNode.AddValue(configTagsPart[i] + configGenders[1] + configTagsParam[j], Float3ToString(DefaultRotationFemale[i][j]));
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

		static string Float3ToString(float[] f)
		{
			if (f.Length != 3)
			{
				throw new Exception("Array length is not 3. Really? o_O");
			}
			return String.Format("{0},{1},{2}", f[0], f[1], f[2]);
		}
		static bool TryParseFloat3(string s, out float[] result)
		{
			result = null;
			try
			{
				result = Array.ConvertAll(s.Split(','), float.Parse);
			}
			catch (Exception stupid)
			{
				Core.Log("Could not parse to float3: " + s);
				Core.Log(stupid.Message);
				return false;
			}
			if (result.Length != 3)
			{
				Core.Log(String.Format("Could not parse to float3. Number of parsed values is {0}.", result.Length));
				return false;
			}
			return true;
		}

	}

}

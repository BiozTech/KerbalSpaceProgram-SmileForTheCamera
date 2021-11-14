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
			}
		}

		public static void Log(object message)
		{
			Debug.Log("[SmileForTheCamera] " + message);
		}

	}

	public static class Settings
	{

		public static float[][][] DefaultRotationLanded = new float[][][]
		{
			// Head
			new float[][] {
				new float[] { -15, -10, -12 }, // min
				new float[] {   0,   0,  -8 }, // center
				new float[] {  15,  10,   0 }  // max
			},
			// EyeL
			new float[][] {
				new float[] {0,0,0},
				new float[] {0,0,0},
				new float[] {0,0,0}
			},
			// EyeR
			new float[][] {
				new float[] {0,0,0},
				new float[] {0,0,0},
				new float[] {0,0,0}
			}
		};
		public static float[][][] DefaultRotationFlying =
		{
			// Head
			new float[][] {
				new float[] { -15, -10, -30 },
				new float[] {   0,   0, -13 },
				new float[] {  15,  10,   0 }
			},
			// EyeL
			new float[][] {
				new float[] {0,0,0},
				new float[] {0,0,0},
				new float[] {0,0,0}
			},
			// EyeR
			new float[][] {
				new float[] {0,0,0},
				new float[] {0,0,0},
				new float[] {0,0,0}
			}
		};

		static string pluginDataDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "PluginData");
		static string settingsFileName = Path.Combine(pluginDataDir, "SmileForTheCameraSettings.cfg");
		static string configTagMain = "SmileForTheCameraSettings";
		static string[] configTagsPart = { "Head", "EyeL", "EyeR" };
		static string[] configTagsParam = { "Min", "Cen", "Max" };

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
				for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++)
				{
					tagCurrent = configTagsPart[i] + "RotationLanded" + configTagsParam[j];
					isOkCurrent = configNode.HasValue(tagCurrent);
					if (isOkCurrent)
					{
						float[] float3;
						isOkCurrent = TryParseFloat3(configNode.GetValue(tagCurrent), out float3);
						if (isOkCurrent) DefaultRotationLanded[i][j] = float3;
					}
					isOk &= isOkCurrent;
				}
				for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++)
				{
					tagCurrent = configTagsPart[i] + "RotationFlying" + configTagsParam[j];
					isOkCurrent = configNode.HasValue(tagCurrent);
					if (isOkCurrent)
					{
						float[] float3;
						isOkCurrent = TryParseFloat3(configNode.GetValue(tagCurrent), out float3);
						if (isOkCurrent) DefaultRotationFlying[i][j] = float3;
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
			for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++)
			{
				configNode.AddValue(configTagsPart[i] + "RotationLanded" + configTagsParam[j], Float3ToString(DefaultRotationLanded[i][j]));
			}
			for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++)
			{
				configNode.AddValue(configTagsPart[i] + "RotationFlying" + configTagsParam[j], Float3ToString(DefaultRotationFlying[i][j]));
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

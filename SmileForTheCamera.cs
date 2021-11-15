using System;
using System.Linq;
using UnityEngine;

namespace SmileForTheCamera
{

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class SmileForTheCamera : MonoBehaviour
	{

		void LateUpdate()//dsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjkdsjkjksdjk
		{
			if (Core.IsEnabled)
			{
				Transform cam = Camera.main.transform;
				if (cam != null)
				{
					foreach (AnimatedKerbal animatedKerbal in Core.AnimatedKerbals)
					{
						animatedKerbal.ForceAnimate(cam);
					}
				}
			}
		}

		void Awake()
		{
			DontDestroyOnLoad(gameObject);
			//Settings.Load();
		}

	}

	public class AnimatedKerbal
	{

		public KerbalEVA kerbalEVA;
		public Transform body, neck, head, eyeL, eyeR;
		public Vector3 bodyPosition, bodyRotation; // in ResetBodyTransform()
		public string[] strBodyPosition = new string[3], strBodyRotation = new string[3];
		public float[][][] rotation; // in ResetHeadAngles()
		public string[][][] strRotation = new string[3][][].Select(o => new string[4][].Select(oo => new string[3]).ToArray()).ToArray();
		public bool isAnimated = true, isBodyAnimated = true, isHeadAnimated = true;

		public AnimatedKerbal(KerbalEVA kerbalEVA)
		{
			this.kerbalEVA = kerbalEVA;
			this.body = this.kerbalEVA.gameObject.transform;
			this.neck = this.kerbalEVA.transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01");
			this.head = this.kerbalEVA.transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01");
			this.eyeL = this.kerbalEVA.transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01/bn_headPivot_b01/bn_upperJaw01/jntDrv_l_eye01");
			this.eyeR = this.kerbalEVA.transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01/bn_headPivot_b01/bn_upperJaw01/jntDrv_r_eye01");
			ResetBodyTransform();
			ResetHeadAngles();
		}
		public AnimatedKerbal()//ASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJAS
		{
			rotation = Settings.DefaultRotationLanded;
			for (int i = 0; i < 3; i++) for (int j = 0; j < 4; j++) for (int k = 0; k < 3; k++) // bruh
				strRotation[i][j][k] = rotation[i][j][k].ToString();
			bodyPosition = bodyRotation = new Vector3();
			for (int i = 0; i < 3; i++)
			{
				strBodyPosition[i] = bodyPosition[i].ToString();
				strBodyRotation[i] = bodyRotation[i].ToString();
			}
		}

		public void ForceAnimate(Transform target)
		{
			if (isAnimated)
			{
				if (isBodyAnimated)//false deafult?
				{
				//	body.localPosition = bodyPosition;
				//	body.localEulerAngles = bodyRotation;
				}
				if (isHeadAnimated)
				{
					Quaternion direction = Quaternion.LookRotation(head.position - target.position, body.up);
					/*neck.rotation = direction * Quaternion.Euler(rotation[0][0][0], rotation[0][0][1], rotation[0][0][2]);
					eyeL.rotation = direction * Quaternion.Euler(rotation[0][1][0], rotation[0][1][1], rotation[0][1][2]);
					eyeR.rotation = direction * Quaternion.Euler(rotation[0][2][0], rotation[0][2][1], rotation[0][2][2]);*/
					neck.rotation = direction * Settings.HeadOffset;
					neck.localRotation = EstimateRotation(neck.localRotation, rotation[0][0], rotation[0][1], rotation[0][2], rotation[0][3]);
					eyeL.rotation = direction * Settings.EyeLOffset;
					eyeL.localRotation = EstimateRotation(eyeL.localRotation, rotation[1][0], rotation[1][1], rotation[1][2], rotation[1][3]);
					eyeR.rotation = direction * Settings.EyeROffset;
					eyeR.localRotation = EstimateRotation(eyeR.localRotation, rotation[2][0], rotation[2][1], rotation[2][2], rotation[2][3]) * Core.TurnLeft;
					/*Debug.Log("neck "+neck.localRotation.eulerAngles);
					Debug.Log("eyeL "+eyeL.localRotation.eulerAngles);
					Debug.Log("eyeR "+eyeR.localRotation.eulerAngles);*/
				}
			}
		}

		static Quaternion EstimateRotation(Quaternion value, float[] min, float[] left, float[] right, float[] max)
		{
			float x = EstimateNearCenter(value.x/value.w, min[0], left[0], right[0], max[0]);
			float y = EstimateNearCenter(value.y/value.w, min[1], left[1], right[1], max[1]);
			float z = EstimateNearCenter(value.z/value.w, min[2], left[2], right[2], max[2]);
			return new Quaternion(x, y, z, 1.0f);
		}
		static float EstimateNearCenter(float value, float min, float left, float right, float max)
		{
			min = Mathf.Tan(min * Mathf.Deg2Rad);
			left = Mathf.Tan(left * Mathf.Deg2Rad);
			right = Mathf.Tan(right * Mathf.Deg2Rad);
			max = Mathf.Tan(max * Mathf.Deg2Rad);
			if (min > left || left > right || right > max)
			{
				return value;
			}
			if (value < left)
			{
				float widthLeft = left - min;
				if (widthLeft == 0)
				{
					return left;
				}
				float deltaLeft = (value - left) / widthLeft;
				return left - widthLeft * (1.0f - Mathf.Exp(-deltaLeft * deltaLeft));
			}
			if (right < value)
			{
				float widthRight = max - right;
				if (widthRight == 0)
				{
					return right;
				}
				float deltaRight = (value - right) / widthRight;
				return right + widthRight * (1.0f - Mathf.Exp(-deltaRight * deltaRight));
			}
			return value;
		}

		public void ResetBodyTransform()
		{
			bodyPosition = body.localPosition;
			bodyRotation = body.localEulerAngles;
			for (int i = 0; i < 3; i++)
			{
				strBodyPosition[i] = bodyPosition[i].ToString();
				strBodyRotation[i] = bodyRotation[i].ToString();
			}
			foreach (Rigidbody rigidbody in kerbalEVA.part.GetComponents<Rigidbody>())
			{
				rigidbody.velocity = Vector3.zero;
				rigidbody.constraints = (Core.IsEnabled && isAnimated && isBodyAnimated) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
			}
		}
		public void ResetHeadAngles()
		{
			rotation = (kerbalEVA.vessel.situation == Vessel.Situations.LANDED) ? Settings.DefaultRotationLanded : Settings.DefaultRotationFlying;
			for (int i = 0; i < 3; i++) for (int j = 0; j < 4; j++) for (int k = 0; k < 3; k++) // bruh
				strRotation[i][j][k] = rotation[i][j][k].ToString();
		}

	}

}

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
			Settings.Load();
		}

	}

	public class AnimatedKerbal
	{

		public KerbalEVA kerbalEVA;
		public Transform body, neck, head, eyeL, eyeR;
		public Vector3 bodyPosition, bodyRotation; // in ResetBodyTransform()
		public string[] strBodyPosition = new string[3], strBodyRotation = new string[3];
		public float[][][] rotation; // in ResetHeadAngles()
		public string[][][] strRotation = new string[3][][].Select(o => new string[3][].Select(oo => new string[3]).ToArray()).ToArray();
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
			for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) for (int k = 0; k < 3; k++) // bruh
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
				if (isBodyAnimated)
				{
					body.localPosition = bodyPosition;
					body.localEulerAngles = bodyRotation;
				}
				if (isHeadAnimated)
				{
					Quaternion direction = Quaternion.LookRotation(head.position - target.position, body.up);
					neck.rotation = direction * Quaternion.Euler(0, -90, -100);
					neck.localRotation = EstimateRotation(neck.localRotation, rotation[0][0], rotation[0][1], rotation[0][2]);
					eyeL.rotation = direction * Quaternion.Euler(0, -65, 10);
					eyeL.localRotation = EstimateRotation(eyeL.localRotation, rotation[1][0], rotation[1][1], rotation[1][2]);
					eyeR.rotation = direction * Quaternion.Euler(0, 120, -5);
					eyeR.localRotation = EstimateRotation(eyeR.localRotation, rotation[2][0], rotation[2][1], rotation[2][2]);
				}
			}
		}

		static Quaternion EstimateRotation(Quaternion value, float[] min, float[] center, float[] max)
		{
			float x = EstimateNearCenter(value.x/value.w, Mathf.Tan(min[0] * Mathf.Deg2Rad), Mathf.Tan(center[0] * Mathf.Deg2Rad), Mathf.Tan(max[0] * Mathf.Deg2Rad));
			float y = EstimateNearCenter(value.y/value.w, Mathf.Tan(min[1] * Mathf.Deg2Rad), Mathf.Tan(center[1] * Mathf.Deg2Rad), Mathf.Tan(max[1] * Mathf.Deg2Rad));
			float z = EstimateNearCenter(value.z/value.w, Mathf.Tan(min[2] * Mathf.Deg2Rad), Mathf.Tan(center[2] * Mathf.Deg2Rad), Mathf.Tan(max[2] * Mathf.Deg2Rad));
			return new Quaternion(x, y, z, 1.0f);
		}
		static float EstimateNearCenter(float value, float min, float center, float max)
		{
			if (min > center || center > max)
			{
				return value;
			}
			float widthLeft = center - min;
			float widthRight = max - center;
			if (value < center)
			{
				if (widthLeft == 0)
				{
					return center;
				}
				float deltaLeft = (value - center) / widthLeft;
				return center - widthLeft * (1.0f - Mathf.Exp(-deltaLeft * deltaLeft));
			} else {
				if (widthRight == 0)
				{
					return center;
				}
				float deltaRight = (value - center) / widthRight;
				return center + widthRight * (1.0f - Mathf.Exp(-deltaRight * deltaRight));
			}
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
			for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) for (int k = 0; k < 3; k++) // bruh
				strRotation[i][j][k] = rotation[i][j][k].ToString();
		}

	}

}

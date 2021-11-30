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
		public float[][][] headRotation; // in ResetHeadAngles()
		public string[][][] strHeadRotation; // in ResetHeadAngles()
		public bool isAnimated = true, isBodyAnimated = false, isHeadAnimated = true;

		Quaternion headOffset, eyeLOffset, eyeROffset;

		public AnimatedKerbal(KerbalEVA kerbalEVA)
		{
			this.kerbalEVA = kerbalEVA;
			this.body = this.kerbalEVA.gameObject.transform;
			this.neck = this.body.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01");
			this.head = this.body.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01");
			this.eyeL = this.body.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01/bn_headPivot_b01/bn_upperJaw01/jntDrv_l_eye01");
			this.eyeR = this.body.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01/bn_headPivot_b01/bn_upperJaw01/jntDrv_r_eye01");
			if (this.kerbalEVA.part.protoModuleCrew[0].gender == ProtoCrewMember.Gender.Male)
			{
				headOffset = Settings.MaleHeadOffset;
				eyeLOffset = Settings.MaleEyeLOffset;
				eyeROffset = Settings.MaleEyeROffset;
			} else {
				headOffset = Settings.FemaleHeadOffset;
				eyeLOffset = Settings.FemaleEyeLOffset;
				eyeROffset = Settings.FemaleEyeROffset;
			}
			ResetBodyTransform();
			ResetHeadAngles();
		}
		public AnimatedKerbal()//ASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJAS
		{
			headRotation = Settings.DefaultRotationMale;
			strHeadRotation = headRotation.Select(o => o.Select(oo => oo.Select(r => r.ToString()).ToArray()).ToArray()).ToArray();
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
					Quaternion direction = Quaternion.LookRotation(Vector3.Lerp(eyeL.position, eyeR.position, 0.5f) - target.position, body.up);
					neck.rotation = direction * headOffset;
					neck.localRotation = EstimateRotation(neck.localRotation, headRotation[0]);
					eyeL.rotation = direction * eyeLOffset;
					eyeL.localRotation = EstimateRotation(eyeL.localRotation, headRotation[1]);
					eyeR.rotation = direction * eyeROffset;
					eyeR.localRotation = EstimateRotation(eyeR.localRotation, headRotation[2]) * Core.TurnLeft;

					/*neck.rotation = direction * Quaternion.Euler(headRotation[0][0][0], headRotation[0][0][1], headRotation[0][0][2]);
					eyeL.rotation = direction * Quaternion.Euler(headRotation[0][1][0], headRotation[0][1][1], headRotation[0][1][2]);
					eyeR.rotation = direction * Quaternion.Euler(headRotation[0][2][0], headRotation[0][2][1], headRotation[0][2][2]);*/
					/*Debug.Log("neck "+neck.localRotation.eulerAngles);
					Debug.Log("eyeL "+eyeL.localRotation.eulerAngles);
					Debug.Log("eyeR "+eyeR.localRotation.eulerAngles);*/
				}
			}
		}

		static Quaternion EstimateRotation(Quaternion value, float[][] rotation)
		{
			float x = EstimateNearCenter(value.x/value.w, rotation[0]);
			float y = EstimateNearCenter(value.y/value.w, rotation[1]);
			float z = EstimateNearCenter(value.z/value.w, rotation[2]);
			return new Quaternion(x, y, z, 1.0f);
		}
		static float EstimateNearCenter(float value, float[] rotation)
		{
			if (rotation[0] > rotation[1] || rotation[1] > rotation[2] || rotation[2] > rotation[3])
			{
				return value;
			}
			float angle = Mathf.Atan(value) * Mathf.Rad2Deg;
			if (angle > 180f) angle -= 360f;
			if (angle < rotation[1])
			{
				float newAngle = rotation[1];
				float widthLeft = rotation[1] - rotation[0];
				if (widthLeft != 0)
				{
					newAngle = rotation[1] - widthLeft * (1.0f - Mathf.Exp((angle - rotation[1]) / widthLeft));
				}
				return Mathf.Tan(newAngle * Mathf.Deg2Rad);
			}
			if (rotation[2] < angle)
			{
				float newAngle = rotation[2];
				float widthRight = rotation[3] - rotation[2];
				if (widthRight != 0)
				{
					newAngle = rotation[2] + widthRight * (1.0f - Mathf.Exp((rotation[2] - angle) / widthRight));
				}
				return Mathf.Tan(newAngle * Mathf.Deg2Rad);
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
			headRotation = ((kerbalEVA.part.protoModuleCrew[0].gender == ProtoCrewMember.Gender.Male) ? Settings.DefaultRotationMale : Settings.DefaultRotationFemale).Select(o => o.Select(oo => oo.ToArray()).ToArray()).ToArray();
			strHeadRotation = headRotation.Select(o => o.Select(oo => oo.Select(rot => rot.ToString()).ToArray()).ToArray()).ToArray();
		}

	}

}

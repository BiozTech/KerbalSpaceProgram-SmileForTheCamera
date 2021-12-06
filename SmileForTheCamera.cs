using System;
using System.Linq;
using UnityEngine;

namespace SmileForTheCamera
{

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class SmileForTheCamera : MonoBehaviour
	{

		void LateUpdate()
		{
			if (Core.IsEnabled)
			{
				Transform cam = Camera.main.transform;
				foreach (AnimatedKerbal animatedKerbal in Core.AnimatedKerbals)
				{
					animatedKerbal.ForceAnimate(cam);
				}
				foreach (AnimatedVessel animatedVessel in Core.AnimatedVessels)
				{
					animatedVessel.ForceAnimate();
				}
			}
		}

		void Awake()
		{
			DontDestroyOnLoad(gameObject);
			//Settings.Load();
		}

	}

	public class AnimatedKerbal : AnimatedThing
	{

		public KerbalEVA kerbalEVA;
		public Transform neck, head, eyeL, eyeR;
		public float[][][] headRotation;
		public string[][][] strHeadRotation;
		public bool isBodyAnimated = false, isHeadAnimated = true;

		Quaternion headOffset, eyeLOffset, eyeROffset;

		public AnimatedKerbal(KerbalEVA kerbalEVA)
		{
			this.kerbalEVA = kerbalEVA;
			vessel = this.kerbalEVA.vessel;
			transform = this.kerbalEVA.gameObject.transform;
			this.neck = transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01");
			this.head = transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01");
			this.eyeL = transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01/bn_headPivot_b01/bn_upperJaw01/jntDrv_l_eye01");
			this.eyeR = transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01/bn_headPivot_b01/bn_upperJaw01/jntDrv_r_eye01");
			isAnimated = true;
			isInitiallyLanded = IsLanded;
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
			ResetBodyTransform(); // set position, rotation, strPosition, strRotation
			ResetHeadAngles(); // set headRotation, strHeadRotation
		}
		public AnimatedKerbal()//ASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJASASKHJAS
		{
			headRotation = Settings.DefaultRotationMale;
			strHeadRotation = headRotation.Select(o => o.Select(oo => oo.Select(r => r.ToString()).ToArray()).ToArray()).ToArray();
			position = rotation = new Vector3();
			for (int i = 0; i < 3; i++)
			{
				strPosition[i] = position[i].ToString();
				strRotation[i] = rotation[i].ToString();
			}
		}

		public void ForceAnimate(Transform target)
		{
			if (isAnimated)
			{
				if (isBodyAnimated)
				{
					transform.position = isInitiallyLanded ? position : position + (Vector3)Core.InitialOrbit.getPositionAtUT(Planetarium.GetUniversalTime());
					transform.localEulerAngles = rotation;
				}
				if (isHeadAnimated)
				{
					Quaternion direction = Quaternion.LookRotation(Vector3.Lerp(eyeL.position, eyeR.position, 0.5f) - target.position, transform.up);
					neck.rotation = direction * headOffset;
					neck.localRotation = EstimateRotation(neck.localRotation, headRotation[0]);
					eyeL.rotation = direction * eyeLOffset;
					eyeL.localRotation = EstimateRotation(eyeL.localRotation, headRotation[1]);
					eyeR.rotation = direction * eyeROffset;
					eyeR.localRotation = EstimateRotation(eyeR.localRotation, headRotation[2]) * Core.TurnLeft;
					/*neck.rotation = direction * Quaternion.Euler(headRotation[0][0][0], headRotation[0][0][1], headRotation[0][0][2]);
					eyeL.rotation = direction * Quaternion.Euler(headRotation[0][1][0], headRotation[0][1][1], headRotation[0][1][2]);
					eyeR.rotation = direction * Quaternion.Euler(headRotation[0][2][0], headRotation[0][2][1], headRotation[0][2][2]);*/
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
				float widthLeft = rotation[1] - rotation[0];
				return Mathf.Tan(((widthLeft != 0) ? rotation[1] - widthLeft * (1.0f - Mathf.Exp((angle - rotation[1]) / widthLeft)) : rotation[1]) * Mathf.Deg2Rad);
			}
			if (rotation[2] < angle)
			{
				float widthRight = rotation[3] - rotation[2];
				return Mathf.Tan(((widthRight != 0) ? rotation[2] + widthRight * (1.0f - Mathf.Exp((rotation[2] - angle) / widthRight)) : rotation[2]) * Mathf.Deg2Rad);
			}
			return value;
		}

		public void ResetBodyTransform()
		{
			if (Core.IsEnabled && isAnimated && isBodyAnimated)
			{
				isInitiallyLanded = IsLanded;
				position = isInitiallyLanded ? transform.position : transform.position - (Vector3)Core.InitialOrbit.getPositionAtUT(Planetarium.GetUniversalTime());
				rotation = transform.localEulerAngles;
				for (int i = 0; i < 3; i++)
				{
					strPosition[i] = position[i].ToString();
					strRotation[i] = rotation[i].ToString();
				}
			}
			bool doConstrain = Core.IsEnabled && isAnimated && isBodyAnimated && IsLanded;
			foreach (Rigidbody rigidbody in kerbalEVA.part.GetComponents<Rigidbody>())
			{
				rigidbody.velocity = rigidbody.angularVelocity = Vector3.zero;
				rigidbody.constraints = doConstrain ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
			}
		}
		public void ResetHeadAngles()
		{
			headRotation = ((kerbalEVA.part.protoModuleCrew[0].gender == ProtoCrewMember.Gender.Male) ? Settings.DefaultRotationMale : Settings.DefaultRotationFemale).Select(o => o.Select(oo => oo.ToArray()).ToArray()).ToArray();
			strHeadRotation = headRotation.Select(o => o.Select(oo => oo.Select(rot => rot.ToString()).ToArray()).ToArray()).ToArray();
		}

	}

	public class AnimatedVessel : AnimatedThing
	{

		public AnimatedVessel(Vessel vessel)
		{
			this.vessel = vessel;
			transform = this.vessel.vesselTransform;
			isAnimated = true;
			isInitiallyLanded = IsLanded;
			ResetTransform();
		}

		public void ForceAnimate()
		{
			if (isAnimated)
			{
				vessel.SetPosition(isInitiallyLanded ? position : position + (Vector3)Core.InitialOrbit.getPositionAtUT(Planetarium.GetUniversalTime()));
				vessel.SetRotation(Quaternion.Euler(rotation));
			}
		}

		public void ResetTransform()
		{
			if (Core.IsEnabled && isAnimated)
			{
				isInitiallyLanded = IsLanded;
				position = isInitiallyLanded ? transform.position : transform.position - (Vector3)Core.InitialOrbit.getPositionAtUT(Planetarium.GetUniversalTime());
				rotation = transform.localEulerAngles;
				for (int i = 0; i < 3; i++)
				{
					strPosition[i] = position[i].ToString();
					strRotation[i] = rotation[i].ToString();
				}
			}
			bool doConstrain = Core.IsEnabled && isAnimated && IsLanded;
			foreach (Part part in vessel.parts)
			{
				foreach (Rigidbody rigidbody in part.GetComponents<Rigidbody>())
				{
					rigidbody.velocity = rigidbody.angularVelocity = Vector3.zero;
					rigidbody.constraints = doConstrain ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
				}
			}
			vessel.SetWorldVelocity(Vector3.zero);
			vessel.angularVelocity = vessel.angularMomentum = Vector3.zero;
		}

	}

	public class AnimatedThing
	{
		public Vessel vessel;
		public Transform transform;
		public Vector3 position, rotation;
		public string[] strPosition = new string[3], strRotation = new string[3];
		public bool isAnimated, isInitiallyLanded;

		public bool IsLanded
		{
			get
			{
				if (vessel != null)
				{
					return vessel.LandedOrSplashed || vessel.situation == Vessel.Situations.PRELAUNCH;
				}
				return false;
			}
		}
	}

}

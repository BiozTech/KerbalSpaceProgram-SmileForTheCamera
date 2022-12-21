using System;
using System.Collections.Generic;
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
			Settings.Load();
		}

	}

	public class AnimatedKerbal : AnimatedObject
	{

		public KerbalEVA kerbalEVA;
		public Transform neck, eyeL, eyeR;
		public float[][] headOffset = Settings.OffsetDefault[0].Select(o => o.Select(oo => 0f).ToArray()).ToArray();
		public string[][] strHeadOffset = Settings.OffsetDefault[0].Select(o => o.Select(oo => "0").ToArray()).ToArray();
		public float[][][] headLimits;
		public string[][][] strHeadLimits;
		public bool isBodyAnimated = false, isHeadAnimated = true, isFaceFreezed = false, isEditingOffset = false, isEditingLimits = false, isMale = true;
		public bool[] isHeadPartLimited = { true, true, true };
		float jetpackThrustPercentage;
		List<Bone> faceBones = new List<Bone>();
		Quaternion headOffsetQuaternion, eyeLOffsetQuaternion, eyeROffsetQuaternion;

		const string pathLowerJaw = "globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01/bn_headPivot_b01/bn_lowerJaw01/";
		const string pathUpperJaw = "globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01/be_neck01/bn_headPivot_a01/bn_headPivot_b01/bn_upperJaw01/";

		public AnimatedKerbal(KerbalEVA kerbalEVA)
		{
			this.kerbalEVA = kerbalEVA;
			vessel = this.kerbalEVA.vessel;
			transform = this.kerbalEVA.gameObject.transform;
			this.neck = transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_neck01");
			this.eyeL = transform.Find(pathUpperJaw + "jntDrv_l_eye01");
			this.eyeR = transform.Find(pathUpperJaw + "jntDrv_r_eye01");
			AddFaceBone(pathLowerJaw);
			AddFaceBone(pathLowerJaw + "bn_lowerTeeth01");
			AddFaceBone(pathLowerJaw + "bn_l_mouthCorner01");
			AddFaceBone(pathLowerJaw + "bn_l_mouthLow_b01");
			AddFaceBone(pathLowerJaw + "bn_l_mouthLow_c01");
			AddFaceBone(pathLowerJaw + "bn_l_mouthLow_d01");
			AddFaceBone(pathLowerJaw + "bn_l_mouthLowMid_a01");
			AddFaceBone(pathLowerJaw + "bn_r_mouthCorner01");
			AddFaceBone(pathLowerJaw + "bn_r_mouthLow_b01");
			AddFaceBone(pathLowerJaw + "bn_r_mouthLow_c01");
			AddFaceBone(pathLowerJaw + "bn_r_mouthLow_d01");
			AddFaceBone(pathUpperJaw);
			AddFaceBone(pathUpperJaw + "bn_upperTeet01");
			AddFaceBone(pathUpperJaw + "bn_l_mouthUp_b01");
			AddFaceBone(pathUpperJaw + "bn_l_mouthUp_c01");
			AddFaceBone(pathUpperJaw + "bn_l_mouthUp_d01");
			AddFaceBone(pathUpperJaw + "bn_l_mouthUpMid_a01");
			AddFaceBone(pathUpperJaw + "bn_r_mouthUp_b01");
			AddFaceBone(pathUpperJaw + "bn_r_mouthUp_c01");
			AddFaceBone(pathUpperJaw + "bn_r_mouthUp_d01");
			this.jetpackThrustPercentage = this.kerbalEVA.thrustPercentage;
			this.isMale = this.kerbalEVA.part.protoModuleCrew[0].gender == ProtoCrewMember.Gender.Male;
			name = this.kerbalEVA.part.protoModuleCrew[0].name;
			isAnimated = true;
			isInitiallyLanded = IsLanded;
			ResetOffset(); // set headOffsetQuaternion, eyeLOffsetQuaternion, eyeROffsetQuaternion
			ResetBody(); // set position, rotation, strPosition, strRotation
			ResetHeadAngles(); // set headLimits, strHeadLimits
		}

		public void ForceAnimate(Transform target)
		{
			if (isAnimated)
			{
				if (isBodyAnimated)
				{
					vessel.SetPosition(CenterOfUniverse.PositionCenterToWorld(this));
					vessel.SetRotation(Quaternion.Euler(rotation));
					kerbalEVA.JetpackIsThrusting = false;
				}
				if (isHeadAnimated)
				{
					if (isEditingOffset) ResetOffset();
					Quaternion direction = Quaternion.LookRotation(Vector3.Lerp(eyeL.position, eyeR.position, 0.5f) - target.position, transform.up);
					neck.rotation = direction * headOffsetQuaternion;
					if (isHeadPartLimited[0]) neck.localRotation = EstimateRotation(neck.localRotation, headLimits[0]);
					eyeL.rotation = direction * eyeLOffsetQuaternion;
					if (isHeadPartLimited[1]) eyeL.localRotation = EstimateRotation(eyeL.localRotation, headLimits[1]);
					eyeR.rotation = direction * eyeROffsetQuaternion;
					if (isHeadPartLimited[2]) eyeR.localRotation = EstimateRotation(eyeR.localRotation, headLimits[2]);
					eyeR.localRotation *= Core.TurnLeft;
					if (isFaceFreezed)
					{
						foreach (Bone bone in faceBones) bone.ForceAnimate();
					}
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

		public void ResetBody()
		{
			if (Core.IsEnabled && isAnimated && isBodyAnimated)
			{
				isInitiallyLanded = IsLanded;
				position = CenterOfUniverse.PositionWorldToCenter(this);
				rotation = transform.localEulerAngles;
				for (int i = 0; i < 3; i++)
				{
					strPosition[i] = position[i].ToString();
					strRotation[i] = rotation[i].ToString();
				}
				kerbalEVA.thrustPercentage = 0;
			} else {
				kerbalEVA.thrustPercentage = jetpackThrustPercentage;
			}
			foreach (Rigidbody rigidbody in kerbalEVA.part.GetComponents<Rigidbody>())
			{
				rigidbody.velocity = rigidbody.angularVelocity = Vector3.zero;
				rigidbody.constraints = (Core.IsEnabled && isAnimated && isBodyAnimated && IsLanded) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
			}
			vessel.SetWorldVelocity(Vector3.zero);
			vessel.angularVelocity = vessel.angularMomentum = Vector3.zero;
		}
		public void ResetFaceBones()
		{
			foreach (Bone bone in faceBones) bone.Reset();
		}
		public void ResetHeadAngles()
		{
			headLimits = Settings.RotationLimits[isMale ? 0 : 1].Select(o => o.Select(oo => oo.ToArray()).ToArray()).ToArray();
			strHeadLimits = headLimits.Select(o => o.Select(oo => oo.Select(rot => rot.ToString()).ToArray()).ToArray()).ToArray();
		}
		void ResetOffset()
		{
			if (isMale)
			{
				headOffsetQuaternion = Quaternion.Euler(Settings.OffsetDefault[0][0][0] + headOffset[0][0], Settings.OffsetDefault[0][0][1] + headOffset[0][1], Settings.OffsetDefault[0][0][2] + headOffset[0][2]);
				eyeLOffsetQuaternion = Quaternion.Euler(Settings.OffsetDefault[0][1][0] + headOffset[1][0], Settings.OffsetDefault[0][1][1] + headOffset[1][1], Settings.OffsetDefault[0][1][2] + headOffset[1][2]);
				eyeROffsetQuaternion = Quaternion.Euler(Settings.OffsetDefault[0][2][0] + headOffset[2][0], Settings.OffsetDefault[0][2][1] + headOffset[2][1], Settings.OffsetDefault[0][2][2] + headOffset[2][2]);
			} else {
				headOffsetQuaternion = Quaternion.Euler(Settings.OffsetDefault[1][0][0] + headOffset[0][0], Settings.OffsetDefault[1][0][1] + headOffset[0][1], Settings.OffsetDefault[1][0][2] + headOffset[0][2]);
				eyeLOffsetQuaternion = Quaternion.Euler(Settings.OffsetDefault[1][1][0] + headOffset[1][0], Settings.OffsetDefault[1][1][1] + headOffset[1][1], Settings.OffsetDefault[1][1][2] + headOffset[1][2]);
				eyeROffsetQuaternion = Quaternion.Euler(Settings.OffsetDefault[1][2][0] + headOffset[2][0], Settings.OffsetDefault[1][2][1] + headOffset[2][1], Settings.OffsetDefault[1][2][2] + headOffset[2][2]);
			}
		}

		void AddFaceBone(string boneName)
		{
			if (transform == null)
			{
				throw new Exception("Somehow, gameObject.transform not returned.");
			}
			Transform bone = transform.Find(boneName);
			if (bone == null)
			{
				throw new Exception("Bone not found: " + boneName);
			}
			faceBones.Add(new Bone(bone));
		}

		class Bone
		{

			Transform transform;
			public Vector3 position;
			public Quaternion rotation;

			public Bone(Transform transform)
			{
				this.transform = transform;
				Reset();
			}

			public void Reset()
			{
				position = transform.localPosition;
				rotation = transform.localRotation;
			}
			public void ForceAnimate()
			{
				transform.localPosition = position;
				transform.localRotation = rotation;
			}

		}

	}

	public class AnimatedVessel : AnimatedObject
	{

		public AnimatedVessel(Vessel vessel)
		{
			this.vessel = vessel;
			transform = this.vessel.vesselTransform;
			name = this.vessel.vesselName;
			isAnimated = false;
			isInitiallyLanded = IsLanded;
			ResetTransform();
		}

		public void ForceAnimate()
		{
			if (isAnimated)
			{
				vessel.SetPosition(CenterOfUniverse.PositionCenterToWorld(this));
				vessel.SetRotation(Quaternion.Euler(rotation));
			}
		}

		public void ResetTransform()
		{
			if (Core.IsEnabled && isAnimated)
			{
				isInitiallyLanded = IsLanded;
				position = CenterOfUniverse.PositionWorldToCenter(this);
				rotation = transform.localEulerAngles;
				for (int i = 0; i < 3; i++)
				{
					strPosition[i] = position[i].ToString();
					strRotation[i] = rotation[i].ToString();
				}
			}
			foreach (Part part in vessel.parts)
			{
				foreach (Rigidbody rigidbody in part.GetComponents<Rigidbody>())
				{
					rigidbody.velocity = rigidbody.angularVelocity = Vector3.zero;
					rigidbody.constraints = (Core.IsEnabled && isAnimated && IsLanded) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
				}
			}
			vessel.SetWorldVelocity(Vector3.zero);
			vessel.angularVelocity = vessel.angularMomentum = Vector3.zero;
		}

	}

	public class AnimatedObject
	{
		public Vessel vessel;
		public Transform transform;
		public Vector3 position, rotation;
		public string[] strPosition = new string[3], strRotation = new string[3];
		public string name;
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

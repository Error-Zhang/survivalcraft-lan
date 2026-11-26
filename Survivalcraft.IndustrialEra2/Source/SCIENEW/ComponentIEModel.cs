using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentIEModel : ComponentModel
{
	public string ModelName { get; set; } = string.Empty;

	public Dictionary<string, int> BonesIndexMap { get; private set; } = new Dictionary<string, int>();

	public string MainBoneName { get; set; } = string.Empty;

	public virtual Matrix? GetBoneTransform(string boneName)
	{
		return m_boneTransforms[BonesIndexMap[boneName]];
	}

	public virtual void SetBoneTransform(string boneName, Matrix? transformation)
	{
		m_boneTransforms[BonesIndexMap[boneName]] = transformation;
	}

	public override void CalculateIsVisible(Camera camera)
	{
		if (!m_boneTransforms[GetBoneIndexByName(MainBoneName)].HasValue)
		{
			IsVisibleForCamera = false;
			return;
		}
		if (camera.GameWidget.IsEntityFirstPersonTarget(Entity))
		{
			IsVisibleForCamera = false;
			return;
		}
		float num = MathUtils.Sqr(m_subsystemSky.VisibilityRange);
		Vector3 vector = m_boneTransforms[GetBoneIndexByName(MainBoneName)].Value.Translation - camera.ViewPosition;
		vector.Y *= m_subsystemSky.VisibilityRangeYMultiplier;
		if (vector.LengthSquared() < num)
		{
			BoundingSphere sphere = new BoundingSphere(m_boneTransforms[GetBoneIndexByName(MainBoneName)].Value.Translation, m_boundingSphereRadius);
			IsVisibleForCamera = camera.ViewFrustum.Intersection(sphere);
		}
		else
		{
			IsVisibleForCamera = false;
		}
	}

	public override void SetModel(Model model)
	{
		base.SetModel(model);
		BonesIndexMap.Clear();
		foreach (ModelBone bone in model.Bones)
		{
			BonesIndexMap[bone.Name] = bone.Index;
		}
	}

	public int GetBoneIndexByName(string name)
	{
		if (!BonesIndexMap.ContainsKey(name))
		{
			return 0;
		}
		return BonesIndexMap[name];
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		m_subsystemSky = Project.FindSubsystem<SubsystemSky>(throwOnError: true);
		CastsShadow = valuesDictionary.GetValue<bool>("CastsShadow");
		PrepareOrder = valuesDictionary.GetValue<int>("PrepareOrder");
		m_boundingSphereRadius = valuesDictionary.GetValue<float>("BoundingSphereRadius");
		ModelName = valuesDictionary.GetValue("ModelName", string.Empty);
		ModelScale = 1f;
	}

	public virtual void SetTexture(Texture2D texture2D)
	{
		TextureOverride = texture2D;
	}

	public override void ProcessBoneHierarchy(ModelBone modelBone, Matrix currentTransform, Matrix[] transforms)
	{
		Matrix transform = modelBone.Transform;
		if (m_boneTransforms[modelBone.Index].HasValue)
		{
			Vector3 translation = transform.Translation;
			transform.Translation = Vector3.Zero;
			Matrix m = transform * m_boneTransforms[modelBone.Index].Value;
			m.Translation += translation;
			Matrix.MultiplyRestricted(ref m, ref currentTransform, out transforms[modelBone.Index]);
		}
		else
		{
			Matrix m2 = transform * Matrix.Zero;
			Matrix.MultiplyRestricted(ref m2, ref currentTransform, out transforms[modelBone.Index]);
		}
		foreach (ModelBone childBone in modelBone.ChildBones)
		{
			ProcessBoneHierarchy(childBone, transforms[modelBone.Index], transforms);
		}
	}
}

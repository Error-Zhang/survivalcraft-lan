using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentPropellerModel : ComponentModel
{
	public float m_headAngleY;

	public string m_baseMeshName;

	public string m_rotorMeshesName;

	public ModelBone m_bodyBone;

	public ModelBone[] m_rotorBones;

	public SubsystemGameInfo m_subsystemGameInfo;

	public ComponentSpawn m_componentSpawn;

	public Vector3 m_rotateAxis;

	public override void Animate()
	{
		if (m_componentSpawn != null)
		{
			Opacity = ((m_componentSpawn.SpawnDuration > 0f) ? ((float)MathUtils.Saturate((m_subsystemGameInfo.TotalElapsedGameTime - m_componentSpawn.SpawnTime) / (double)m_componentSpawn.SpawnDuration)) : 1f);
			if (m_componentSpawn.DespawnTime.HasValue)
			{
				Opacity = MathUtils.Min(Opacity.Value, (float)MathUtils.Saturate(1.0 - (m_subsystemGameInfo.TotalElapsedGameTime - m_componentSpawn.DespawnTime.Value) / (double)m_componentSpawn.DespawnDuration));
			}
		}
		SetBoneTransform(m_bodyBone.Model.RootBone.Index, m_componentFrame.Matrix);
		ModelBone[] rotorBones = m_rotorBones;
		foreach (ModelBone modelBone in rotorBones)
		{
			SetBoneTransform(modelBone.Index, Matrix.CreateFromAxisAngle(m_rotateAxis, m_headAngleY));
		}
	}

	public void Rotate(float dt, float speed)
	{
		m_headAngleY += dt * speed;
		m_headAngleY %= (float)Math.PI * 2f;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
		m_componentSpawn = Entity.FindComponent<ComponentSpawn>();
		m_baseMeshName = valuesDictionary.GetValue<string>("BaseMeshName");
		m_rotorMeshesName = valuesDictionary.GetValue<string>("RotorMeshesName");
		m_rotateAxis = valuesDictionary.GetValue("RotateAxis", Vector3.UnitZ);
		base.Load(valuesDictionary, idToEntityMap);
	}

	public override void SetModel(Model model)
	{
		base.SetModel(model);
		if (Model != null)
		{
			m_bodyBone = Model.FindBone(m_baseMeshName);
			string[] array = m_rotorMeshesName.Split(';');
			List<ModelBone> list = new List<ModelBone>();
			string[] array2 = array;
			foreach (string name in array2)
			{
				list.Add(Model.FindBone(name));
			}
			m_rotorBones = list.ToArray();
		}
		else
		{
			m_bodyBone = null;
			m_rotorBones = Array.Empty<ModelBone>();
		}
	}
}

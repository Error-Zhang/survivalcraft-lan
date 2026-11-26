using System;
using System.Collections;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentSpringPlate : Component, IUpdateable
{
	public ComponentIEModel m_componentIEModel;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public CoroutineScheduler m_coroutineScheduler = new CoroutineScheduler();

	public bool m_isMoving;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		SpringPlateDevice device = DevicesBlockManager.GetDevice<SpringPlateDevice>();
		if (device != null)
		{
			m_componentIEModel.SetModel(device.m_model);
			m_componentIEModel.SetTexture(device.m_texture);
			m_componentIEModel.MainBoneName = m_componentIEModel.Model.RootBone.Name;
		}
	}

	public void Update(float dt)
	{
		if (!m_isMoving)
		{
			m_componentIEModel.SetBoneTransform(m_componentIEModel.MainBoneName, null);
		}
		m_coroutineScheduler.UpdateCoroutines();
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public void OnJump()
	{
		m_coroutineScheduler.AddCoroutine(ScaleObject());
	}

	private void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<SpringPlateDevice>(m_isMoving ? 1 : 0);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		m_coroutineScheduler.RemoveAllCoroutines();
	}

	public IEnumerator ScaleObject()
	{
		m_isMoving = true;
		Vector3 originalScale = Vector3.One;
		Vector3 targetScale = new Vector3(1f, 0.75f, 1f);
		float duration = 0.2f;
		float elapsedTime = 0f;
		Matrix translationMat = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 0f, 0.5f));
		while (elapsedTime < duration / 2f)
		{
			elapsedTime += m_subsystemTime.GameTimeDelta;
			float t = Math.Clamp(elapsedTime / (duration / 2f), 0f, 1f);
			Vector3 scale = originalScale.Lerp(targetScale, t);
			m_componentIEModel.SetBoneTransform(m_componentIEModel.MainBoneName, Matrix.CreateScale(scale) * translationMat);
			yield return default(WaitForFrameCondition);
		}
		elapsedTime = 0f;
		while (elapsedTime < duration / 2f)
		{
			elapsedTime += m_subsystemTime.GameTimeDelta;
			float t2 = Math.Clamp(elapsedTime / (duration / 2f), 0f, 1f);
			Vector3 scale2 = targetScale.Lerp(originalScale, t2);
			m_componentIEModel.SetBoneTransform(m_componentIEModel.MainBoneName, Matrix.CreateScale(scale2) * translationMat);
			yield return default(WaitForFrameCondition);
		}
		m_componentIEModel.SetBoneTransform(m_componentIEModel.MainBoneName, Matrix.CreateScale(originalScale) * translationMat);
		m_isMoving = false;
	}
}

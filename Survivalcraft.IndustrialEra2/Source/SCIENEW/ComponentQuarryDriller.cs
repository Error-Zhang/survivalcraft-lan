using System;
using Engine;
using Engine.Graphics;
using Game;
using Game.Network;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentQuarryDriller : ComponentIEModel, IUpdateable
{
	public ComponentBody m_componentBody;

	public float m_drillRotateSpeed;

	public float m_wheelRotateSpeed;

	public bool m_displayDrillerHead;

	public bool m_isLowQuality;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float Length { get; set; } = 10f;

	public float Depth { get; set; } = 4.5f;

	public float Phase { get; set; } = 0.3f;

	public float DrillRotate { get; set; }

	public float WheelRotate { get; set; }

	public Quaternion InitialRotate { get; set; }

	public int ID { get; set; }

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentFrame = Entity.FindComponent<ComponentFrame>(throwOnError: true);
		m_componentBody = Entity.FindComponent<ComponentBody>(throwOnError: true);
		m_componentBody.IsGravityEnabled = false;
		ID = valuesDictionary.GetValue("ID", -1);
		Length = valuesDictionary.GetValue("Length", 0f);
		Depth = valuesDictionary.GetValue("Depth", 0f);
		Phase = valuesDictionary.GetValue("Phase", 0f);
		DrillRotate = valuesDictionary.GetValue("DrillRotate", 0f);
		InitialRotate = valuesDictionary.GetValue("InitialRotate", m_componentFrame.Rotation);
		WheelRotate = valuesDictionary.GetValue("WheelRotate", 0f);
		m_componentBody.BoxSize = valuesDictionary.GetValue("BoxSize", Vector3.One);
		m_drillRotateSpeed = valuesDictionary.GetValue("DrillRotateSpeed", 0f);
		m_wheelRotateSpeed = valuesDictionary.GetValue("WheelRotateSpeed", 0f);
		m_isLowQuality = IESettingsManager.Get<bool>("LowQuality");
		SetModel(m_isLowQuality ? ContentManager.Get<Model>("Models/QuarryDriller_Low") : ContentManager.Get<Model>("Models/QuarryDriller"));
		SetTexture(ContentManager.Get<Texture2D>("Textures/QuarryDriller"));
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("ID", ID);
		valuesDictionary.SetValue("Length", Length);
		valuesDictionary.SetValue("Depth", Depth);
		valuesDictionary.SetValue("Phase", Phase);
		valuesDictionary.SetValue("DrillRotate", DrillRotate);
		valuesDictionary.SetValue("InitialRotate", InitialRotate);
		valuesDictionary.SetValue("WheelRotate", WheelRotate);
		valuesDictionary.SetValue("BoxSize", m_componentBody.BoxSize);
		valuesDictionary.SetValue("DrillRotateSpeed", m_drillRotateSpeed);
		valuesDictionary.SetValue("WheelRotateSpeed", m_wheelRotateSpeed);
	}

	internal void SerializeLanPose(PacketSerializer archive)
	{
		Vector3 position = m_componentFrame.Position;
		Quaternion rotation = InitialRotate;
		float length = Length;
		float depth = Depth;
		float phase = Phase;
		float drill = DrillRotate;
		float wheels = WheelRotate;
		archive.Value(ref position);
		archive.Value(ref rotation);
		archive.Value(ref length);
		archive.Value(ref depth);
		archive.Value(ref phase);
		archive.Value(ref drill);
		archive.Value(ref wheels);
		archive.Value(ref m_displayDrillerHead);
		if (archive.IsReading)
		{
			m_componentFrame.Position = position;
			InitialRotate = rotation;
			Length = length;
			Depth = depth;
			Phase = phase;
			DrillRotate = drill;
			WheelRotate = wheels;
		}
	}

	public override void Animate()
	{
		base.Animate();
		m_componentFrame.Rotation = InitialRotate;
		SetBoneTransform(Model.RootBone.Index, m_componentFrame.Matrix);
		SetBoneTransform("CenterRod", Matrix.CreateScale(1f, (Length + 1f) * 2f + 1f, 1f));
		SetBoneTransform("DrillerRoot", Matrix.CreateTranslation(0f, (Phase - 0.5f) * Length * 2f, 0f));
		SetBoneTransform("WheelGroup12", Matrix.CreateTranslation(0f, 0f - (Length + 1f), 0f));
		SetBoneTransform("WheelGroup34", Matrix.CreateTranslation(0f, Length + 1f, 0f));
		if (!m_isLowQuality)
		{
			SetBoneTransform("Side1", Matrix.Identity);
			SetBoneTransform("Side2", Matrix.Identity);
			SetBoneTransform("Wheel1", Matrix.CreateRotationY(WheelRotate));
			SetBoneTransform("Wheel2", Matrix.CreateRotationY(WheelRotate));
			SetBoneTransform("Wheel3", Matrix.CreateRotationY(WheelRotate));
			SetBoneTransform("Wheel4", Matrix.CreateRotationY(WheelRotate));
			float num = ((MathF.Ceiling(Depth) == Depth) ? (Depth + 1f) : MathF.Ceiling(Depth));
			for (int i = 0; i < 40; i++)
			{
				string boneName = $"DrillerRod.{i + 1:000}";
				if ((float)i < num)
				{
					Matrix identity = Matrix.Identity;
					if (i == 0)
					{
						float num2 = Depth % 1f;
						Vector3 position = -Vector3.UnitZ * (num2 / 2f);
						Vector3 scale = Vector3.One - Vector3.UnitZ * (1f - num2);
						identity = Matrix.CreateRotationZ(DrillRotate) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
					}
					else
					{
						Vector3 vector = -Vector3.UnitZ * (Depth % 1f + (float)i);
						identity = Matrix.CreateRotationZ(DrillRotate) * Matrix.CreateTranslation(vector + Vector3.UnitZ * 0.5f);
					}
					SetBoneTransform(boneName, identity);
				}
				else
				{
					SetBoneTransform(boneName, Matrix.Zero);
				}
			}
		}
		else
		{
			Matrix value = Matrix.CreateRotationZ(DrillRotate) * Matrix.CreateScale(1f, 1f, Depth) * Matrix.CreateTranslation(0f, 0f, (0f - Depth) / 2f);
			SetBoneTransform("DrillerRod", value);
		}
		Vector3 position2 = -Vector3.UnitZ * (Depth + 0.5f);
		SetBoneTransform("DrillerHead", Matrix.CreateScale(m_displayDrillerHead ? 1 : 0) * Matrix.CreateRotationZ(DrillRotate) * Matrix.CreateTranslation(position2));
	}

	public override void CalculateIsVisible(Camera camera)
	{
		if (camera.GameWidget.IsEntityFirstPersonTarget(Entity))
		{
			IsVisibleForCamera = false;
			return;
		}
		float num = MathUtils.Sqr(m_subsystemSky.VisibilityRange);
		Vector3 vector = m_componentFrame.Position - camera.ViewPosition;
		vector.Y *= m_subsystemSky.VisibilityRangeYMultiplier;
		if (vector.LengthSquared() < num)
		{
			BoundingSphere sphere = new BoundingSphere(m_componentFrame.Position, MathF.Max(Length, Depth));
			IsVisibleForCamera = camera.ViewFrustum.Intersection(sphere);
		}
		else
		{
			IsVisibleForCamera = false;
		}
	}

	public void Update(float dt)
	{
		if (ID < 0)
		{
			Project.RemoveEntity(Entity, disposeEntity: true);
		}
		m_componentBody.IsGravityEnabled = false;
		DrillRotate += dt * 2f * m_drillRotateSpeed;
		DrillRotate %= (float)Math.PI * 2f;
		WheelRotate += dt * 2f * m_wheelRotateSpeed;
		WheelRotate %= (float)Math.PI * 2f;
		m_componentBody.Velocity = Vector3.Zero;
	}
}

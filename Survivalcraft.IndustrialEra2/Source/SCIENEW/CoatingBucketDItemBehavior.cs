using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CoatingBucketDItemBehavior : DamageableItemBehavior
{
	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemAudio m_subsystemAudio;

	public SubsystemParticles m_subsystemParticles;

	public Game.Random m_random = new Game.Random();

	public CoatingBucketDItemBehavior(Project project)
		: base(project)
	{
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemAudio = project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemParticles = project.FindSubsystem<SubsystemParticles>(throwOnError: true);
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		DamageableItemBlock item = DamageableItemManager.GetItem(componentMiner.ActiveBlockValue);
		TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
		if (terrainRaycastResult.HasValue)
		{
			CellFace cellFace = terrainRaycastResult.Value.CellFace;
			int cellValue = m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			IPaintableBlock paintableBlock = GetPaintableBlock(cellValue);
			if (paintableBlock != null)
			{
				Vector3 normal = CellFace.FaceToVector3(terrainRaycastResult.Value.CellFace.Face);
				Vector3 position = terrainRaycastResult.Value.HitPoint();
				int? num = ((item is CoatingStripperBucketDItem) ? ((int?)null) : ((CoatingBucketDItem)item).GetColorIndex());
				Color color = (num.HasValue ? SubsystemPalette.GetColor(m_subsystemTerrain, num) : new Color(128, 128, 128, 128));
				int value = paintableBlock.Paint(m_subsystemTerrain, cellValue, num);
				if (!IndustrialEraLanAdapter.IsRemote(Project))
				{
					m_subsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, value);
					DamageableItemManager.DamageActiveTool(componentMiner, 1);
				}
				m_subsystemAudio.PlayRandomSound("Audio/Paint", 0.4f, m_random.Float(-0.1f, 0.1f), componentMiner.ComponentCreature.ComponentBody.Position, 2f, autoDelay: true);
				m_subsystemParticles.AddParticleSystem(new PaintParticleSystem(m_subsystemTerrain, position, normal, color));
				return true;
			}
			return false;
		}
		return false;
	}

	public IPaintableBlock? GetPaintableBlock(int cellValue)
	{
		if (Terrain.ExtractContents(cellValue) == BaseNormalBlock.Index && NormalBlockManager.GetNormalBlock(cellValue) is IPaintableBlock result)
		{
			return result;
		}
		if (Terrain.ExtractContents(cellValue) == BaseDeviceBlock.Index && DevicesBlockManager.GetDevice(cellValue) is IPaintableBlock result2)
		{
			return result2;
		}
		return null;
	}
}

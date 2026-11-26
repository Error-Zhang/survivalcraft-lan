using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class DrillDItemBehavior : DamageableItemBehavior
{
	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemAudio m_subsystemAudio;

	public DrillDItemBehavior(Project project)
		: base(project)
	{
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemAudio = project.FindSubsystem<SubsystemAudio>(throwOnError: true);
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Interaction);
		if (!terrainRaycastResult.HasValue)
		{
			return false;
		}
		CellFace cellFace = terrainRaycastResult.Value.CellFace;
		int cellValue = m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
		int activeBlockValue = componentMiner.ActiveBlockValue;
		if (Terrain.ExtractContents(activeBlockValue) != BaseDamageableItemBlock.Index)
		{
			return false;
		}
		if (DamageableItemManager.GetItemCraftingID(activeBlockValue) != "Drill")
		{
			return false;
		}
		if (ElectricToolDItem.IsOutOfPower(activeBlockValue))
		{
			return false;
		}
		if (BlockInterfaceResolver.ResolveFromValue<IElectricDrillRemovable>(cellValue) == null)
		{
			return false;
		}
		if (IndustrialEraLanAdapter.IsRemote(Project)) return true;
		m_subsystemTerrain.DestroyCell(0, cellFace.X, cellFace.Y, cellFace.Z, 0, noDrop: false, noParticleSystem: false);
		m_subsystemAudio.PlaySound("Audio/Drill", 0.6f, 0f, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 3f, autoDelay: true);
		DamageableItemManager.DamageActiveTool(componentMiner, 1);
		return true;
	}
}

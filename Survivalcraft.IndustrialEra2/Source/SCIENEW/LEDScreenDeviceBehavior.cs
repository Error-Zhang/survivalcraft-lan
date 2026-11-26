using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class LEDScreenDeviceBehavior : DeviceBehavior
{
	private SubsystemLEDScreenControllers m_subsystemControllers;

	private SubsystemTerrain m_subsystemTerrain;

	public LEDScreenDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemControllers = project.FindSubsystem<SubsystemLEDScreenControllers>(throwOnError: true);
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	private static bool TryGetMountFace(int value, out int face)
	{
		face = 0;
		if (!(DevicesBlockManager.GetDevice(value) is LEDScreenDevice))
		{
			return false;
		}
		face = LEDScreenDevice.GetDirection_Static(value);
		return true;
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		if (TryGetMountFace(value, out var face))
		{
			m_subsystemControllers.UpdateController(x, y, z, face);
		}
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		if (TryGetMountFace(value, out var face))
		{
			m_subsystemControllers.UpdateController(x, y, z, face, !isLoaded);
			if (isLoaded)
			{
				m_subsystemControllers.SchedulePostLoadSync();
			}
		}
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		bool flag = TryGetMountFace(oldValue, out var face);
		bool flag2 = TryGetMountFace(value, out var face2);
		if (flag && (!flag2 || face != face2))
		{
			DetachFromGroup(x, y, z, face);
		}
		if (flag2)
		{
			m_subsystemControllers.UpdateController(x, y, z, face2);
		}
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		if (TryGetMountFace(value, out var face))
		{
			if (!TryGetMountFace(newValue, out _))
			{
				m_subsystemControllers.RemoveLedGroupEntity(new Point3(x, y, z));
			}
			DetachFromGroup(x, y, z, face);
		}
	}

	private void DetachFromGroup(int x, int y, int z, int face)
	{
		CellFace cellFace = new CellFace(x, y, z, face);
		if (m_subsystemControllers.IsThereAController(cellFace))
		{
			m_subsystemControllers.RemoveController(m_subsystemControllers.GetController(cellFace));
		}
		DevicesUtils.FaceToAxesAndConner(face, out Point3[] axes, out Point3[] _);
		Point3 point = new Point3(x, y, z);
		for (int i = 0; i < 4; i++)
		{
			Point3 point2 = point + axes[i];
			if (TryGetMountFace(m_subsystemTerrain.Terrain.GetCellValueFast(point2.X, point2.Y, point2.Z), out var face2) && face2 == face)
			{
				m_subsystemControllers.UpdateController(point2.X, point2.Y, point2.Z, face);
			}
		}
	}
}

using System;
using System.Text;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class MultimeterItemBehavior : ItemBehavior
{
	public SubsystemVoltNet m_subsystemVoltNet;

	public MultimeterItemBehavior(Project project)
		: base(project)
	{
		m_subsystemVoltNet = project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
		if (terrainRaycastResult.HasValue)
		{
			CellFace cellFace = terrainRaycastResult.Value.CellFace;
			VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(cellFace.Point);
			if (voltElement != null)
			{
				IVoltDevice voltDevice = BlockInterfaceResolver.ResolveFromValue<IVoltDevice>(voltElement.BlockValue);
				if (voltDevice == null)
				{
					return false;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(LanguageControl.Get("Behaviors", "MultimeterItemBehavior", "WorkState") + ": " + voltElement.CurrentWorkState);
				stringBuilder.AppendLine(LanguageControl.Get("Behaviors", "MultimeterItemBehavior", "StandardUL") + ": " + MathF.Abs(voltDevice.GetStandardUL(voltElement.BlockValue)));
				stringBuilder.AppendLine(LanguageControl.Get("Behaviors", "MultimeterItemBehavior", "StandardPL") + ": " + MathF.Abs(voltDevice.GetStandardPL(voltElement.BlockValue)));
				stringBuilder.AppendLine(LanguageControl.Get("Behaviors", "MultimeterItemBehavior", "ActualULThisFace") + ": " + MathF.Abs(voltElement.GetUL(voltDevice.GetStandardUL(voltElement.BlockValue), cellFace)));
				stringBuilder.AppendLine(LanguageControl.Get("Behaviors", "MultimeterItemBehavior", "ActualPLThisFace") + ": " + MathF.Abs(voltElement.GetPL(voltDevice.GetStandardPL(voltElement.BlockValue), cellFace)));
				stringBuilder.AppendLine(LanguageControl.Get("Behaviors", "MultimeterItemBehavior", "CanIOPowerThisFace") + ": " + (voltDevice.GetFaceMask(voltElement.BlockValue).Contains(cellFace.Face) ? LanguageControl.Get("Behaviors", "MultimeterItemBehavior", "Yes") : LanguageControl.Get("Behaviors", "MultimeterItemBehavior", "No")));
				componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(stringBuilder.ToString(), Color.White, blinking: false, playNotificationSound: true);
				return true;
			}
		}
		m_subsystemVoltNet.SwitchDraw(Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true).WorldSettings.GameMode == GameMode.Creative);
		return false;
	}
}

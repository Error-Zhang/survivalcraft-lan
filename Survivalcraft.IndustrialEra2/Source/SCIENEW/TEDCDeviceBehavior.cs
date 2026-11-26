using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using NCalc;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class TEDCDeviceBehavior : EntityDeviceBehavior<ComponentAdvancedCrusher>
{
	public SubsystemVoltNet m_subsystemVoltNet;

	public Dictionary<Point3, string> m_expressions = new Dictionary<Point3, string>();

	public TEDCDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemVoltNet = project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		base.OnBlockRemoved(value, newValue, x, y, z);
		m_expressions.Remove(new Point3(x, y, z));
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		Point3 coordinate = raycastResult.CellFace.Point;
		if (!m_expressions.ContainsKey(coordinate))
		{
			m_expressions[coordinate] = string.Empty;
		}
		VoltElement? voltElement = m_subsystemVoltNet.GetVoltElement(coordinate);
		if (voltElement == null || voltElement.CurrentWorkState != WorkState.Active)
		{
			componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get("Behaviors", "TEDCDeviceBehavior", "InActive"), Color.White, blinking: true, playNotificationSound: true);
			return true;
		}
		DialogsManager.ShowDialog(componentMiner.ComponentPlayer.GuiWidget, new TextBoxDialog(LanguageControl.Get("Behaviors", "TEDCDeviceBehavior", "EnterExpression"), m_expressions[coordinate], int.MaxValue, (string s) =>
		{
			if (!string.IsNullOrEmpty(s))
			{
				m_expressions[coordinate] = s;
				Execute(s, componentMiner.ComponentPlayer);
			}
		}));
		return true;
	}

	public void Execute(string expressionStr, ComponentPlayer componentPlayer)
	{
		try
		{
			float num = new Expression(expressionStr).ToLambda<float>()();
			componentPlayer.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get("Behaviors", "TEDCDeviceBehavior", "CalcSucceed"), num), Color.White, blinking: true, playNotificationSound: true);
		}
		catch (Exception ex)
		{
			componentPlayer.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get("Behaviors", "TEDCDeviceBehavior", "CalcFailed"), ex.Message), Color.White, blinking: true, playNotificationSound: true);
		}
	}
}

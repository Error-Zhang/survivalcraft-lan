using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using GameEntitySystem;
using SCIENEW;
using SCIENEW.Ponder;

namespace Game;

public class BlockPonderDialog : Dialog
{
	public ButtonWidget m_leftButtonWidget;

	public ButtonWidget m_rightButtonWidget;

	public ButtonWidget m_restartButtonWidget;

	public PonderWidget m_ponderWidget;

	public BlockPonderDialog(Project project)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/BlockPonderWidget");
		LoadContents(this, node);
		m_leftButtonWidget = Children.Find<ButtonWidget>("Left");
		m_rightButtonWidget = Children.Find<ButtonWidget>("Right");
		m_restartButtonWidget = Children.Find<ButtonWidget>("Restart");
		m_ponderWidget = Children.Find<PonderWidget>("Ponder");
		m_ponderWidget.Setup(project, Func(m_ponderWidget));
	}

	public IEnumerator Func(PonderWidget ponderWidget)
	{
		Dictionary<Point3, int> dictionary = new Dictionary<Point3, int>();
		for (int i = 6; i < 11; i++)
		{
			for (int j = 6; j < 11; j++)
			{
				dictionary.Add(new Point3(i, 1, j), ((i + j) % 2 == 1) ? BlocksManager.GetBlockIndex<IronBlock>() : NormalBlockManager.GetNormalBlockID("BasicMechanicalShell"));
			}
		}
		yield return ponderWidget.AddStaticBlockRangeEaseIn(dictionary);
		yield return ponderWidget.ViewportSizeTo(24f, 1f);
		yield return ponderWidget.AddStaticBlockEaseIn(new Point3(8, 2, 8), DevicesBlockManager.GetDeviceValue("GReactorCoreDevice", 0));
		yield return ponderWidget.AddStaticBlockEaseIn(new Point3(8, 3, 8), DevicesBlockManager.GetDeviceValue("GReactorTopDevice", 0));
		yield return ponderWidget.AddStaticBlockEaseIn(new Point3(7, 2, 8), DevicesBlockManager.GetDeviceValue("HeatExchangerDevice", CellFace.Point3ToFace(new Point3(7, 2, 8) - new Point3(8, 2, 8))));
		yield return ponderWidget.AddStaticBlockEaseIn(new Point3(9, 2, 8), DevicesBlockManager.GetDeviceValue("HeatExchangerDevice", CellFace.Point3ToFace(new Point3(9, 2, 8) - new Point3(8, 2, 8))));
		yield return ponderWidget.AddStaticBlockEaseIn(new Point3(8, 2, 7), DevicesBlockManager.GetDeviceValue("GReactorControllerDevice", CellFace.Point3ToFace(new Point3(8, 2, 7) - new Point3(8, 2, 8))));
		yield return ponderWidget.AddStaticBlockEaseIn(new Point3(6, 2, 8), DevicesBlockManager.GetDeviceValue("ReactorTurbineDevice", CellFace.Point3ToFace(new Point3(7, 2, 8) - new Point3(8, 2, 8))));
		yield return ponderWidget.AddStaticBlockEaseIn(new Point3(10, 2, 8), DevicesBlockManager.GetDeviceValue("ReactorTurbineDevice", CellFace.Point3ToFace(new Point3(9, 2, 8) - new Point3(8, 2, 8))));
		PickablePonderBlockHandle pickablePonderBlock = new PickablePonderBlockHandle();
		yield return ponderWidget.AddPickableBlock(BlocksManager.GetBlockIndex<TallGrassBlock>(), new Vector3(6.5f, 3.5f, 8.5f), Vector3.Zero, (PickablePonderBlock block) =>
		{
			pickablePonderBlock.Instance = block;
		});
		yield return ponderWidget.Idle(2.5f);
		yield return ponderWidget.RemovePickableBlock(pickablePonderBlock);
		yield return ponderWidget.Idle(0.5f);
		yield return ponderWidget.AddProjectileBlock(BlocksManager.GetBlockIndex<DiamondChunkBlock>(), new Vector3(6.5f, 3.5f, 8.5f), Vector3.UnitX + Vector3.UnitY * 6f, Vector3.One * 2f, (ProjectilePonderBlock block) =>
		{
		});
		yield return ponderWidget.Idle(2.5f);
		yield return ponderWidget.RotateYLerp(360f, 4f);
		yield return ponderWidget.Idle(1.5f);
		yield return ponderWidget.RemoveStaticBlock(new Point3(7, 2, 8));
		yield return ponderWidget.Idle(1.5f);
		yield return ponderWidget.AddStaticBlockEaseIn(new Point3(7, 2, 8), DevicesBlockManager.GetDeviceValue("HeatExchangerDevice", CellFace.Point3ToFace(new Point3(7, 2, 8) - new Point3(8, 2, 8))));
	}

	public override void Update()
	{
		if (m_leftButtonWidget.IsClicked)
		{
			m_ponderWidget.Pause();
		}
		if (m_rightButtonWidget.IsClicked)
		{
			m_ponderWidget.Resume();
		}
		if (m_restartButtonWidget.IsClicked)
		{
			m_ponderWidget.Setup(m_ponderWidget.Project, Func(m_ponderWidget));
		}
	}
}

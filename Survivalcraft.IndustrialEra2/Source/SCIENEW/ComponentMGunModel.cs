using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class ComponentMGunModel : ComponentModel
{
	public ModelBone m_tripodeBone;

	public ModelBone m_turretBone;

	public override void Animate()
	{
		SetBoneTransform(m_tripodeBone.Index, m_componentFrame.Matrix);
		SetBoneTransform(m_turretBone.Index, Matrix.Identity);
	}

	public override void SetModel(Model model)
	{
		base.SetModel(model);
		if (Model != null)
		{
			m_tripodeBone = Model.FindBone("Tripode");
			m_turretBone = Model.FindBone("Turret");
		}
		else
		{
			m_tripodeBone = null;
			m_tripodeBone = null;
		}
	}
}

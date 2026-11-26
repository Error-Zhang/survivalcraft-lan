using System;
using Engine;

namespace SCIENEW;

public class ModuleItem : MeshItem
{
	protected Type ResultType;

	public ModuleItem(string id, string meshName, Type resultType)
		: base(id, "Models/Modules", meshName, IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.5f, 0f), Matrix.Identity, 1.2f)
	{
		ResultType = resultType;
	}

	public Type GetResultType()
	{
		return ResultType;
	}
}

using Game;

namespace SCIENEW;

public class ComponentPerspectiveCameraMaskModel : ComponentIEModel
{
	public PerspectiveCamera MaskCamera { get; set; }

	public override void CalculateIsVisible(Camera camera)
	{
		base.CalculateIsVisible(camera);
		IsVisibleForCamera &= camera != MaskCamera;
	}
}

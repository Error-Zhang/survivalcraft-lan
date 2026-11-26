using System;
using Engine;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW;

public static class DevicesUtils
{
	public static readonly Vector3[] m_faceToVector3 = new Vector3[6]
	{
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 1f, 0f)
	};

	public static int GetInnerData(int value)
	{
		return Terrain.ExtractData(value) & 7;
	}

	public static int GetSixFaceDirection(int value)
	{
		int face = GetInnerData(value);
		return face < 6 ? face : 0;
	}

	public static int GetDeviceIndex(int value)
	{
		return Terrain.ExtractData(value) >> 3;
	}

	public static void FaceToAxesAndConner(int face, out Point3[] axes, out Point3[] conners)
	{
		switch (face)
		{
		case 0:
			axes = new Point3[4]
			{
				Point3.UnitY,
				Point3.UnitX,
				-Point3.UnitY,
				-Point3.UnitX
			};
			conners = new Point3[4]
			{
				new Point3(-1, 1, 0),
				new Point3(1, 1, 0),
				new Point3(1, -1, 0),
				new Point3(-1, -1, 0)
			};
			break;
		case 1:
			axes = new Point3[4]
			{
				Point3.UnitY,
				-Point3.UnitZ,
				-Point3.UnitY,
				Point3.UnitZ
			};
			conners = new Point3[4]
			{
				new Point3(0, 1, 1),
				new Point3(0, 1, -1),
				new Point3(0, -1, -1),
				new Point3(0, -1, 1)
			};
			break;
		case 2:
			axes = new Point3[4]
			{
				Point3.UnitY,
				-Point3.UnitX,
				-Point3.UnitY,
				Point3.UnitX
			};
			conners = new Point3[4]
			{
				new Point3(1, 1, 0),
				new Point3(-1, 1, 0),
				new Point3(-1, -1, 0),
				new Point3(1, -1, 0)
			};
			break;
		case 3:
			axes = new Point3[4]
			{
				Point3.UnitY,
				Point3.UnitZ,
				-Point3.UnitY,
				-Point3.UnitZ
			};
			conners = new Point3[4]
			{
				new Point3(0, 1, -1),
				new Point3(0, 1, 1),
				new Point3(0, -1, 1),
				new Point3(0, -1, -1)
			};
			break;
		case 4:
			axes = new Point3[4]
			{
				-Point3.UnitZ,
				Point3.UnitX,
				Point3.UnitZ,
				-Point3.UnitX
			};
			conners = new Point3[4]
			{
				new Point3(-1, 0, -1),
				new Point3(1, 0, -1),
				new Point3(1, 0, 1),
				new Point3(-1, 0, 1)
			};
			break;
		case 5:
			axes = new Point3[4]
			{
				Point3.UnitZ,
				Point3.UnitX,
				-Point3.UnitZ,
				-Point3.UnitX
			};
			conners = new Point3[4]
			{
				new Point3(-1, 0, 1),
				new Point3(1, 0, 1),
				new Point3(1, 0, -1),
				new Point3(-1, 0, -1)
			};
			break;
		default:
			axes = new Point3[4]
			{
				-Point3.UnitZ,
				Point3.UnitX,
				Point3.UnitZ,
				-Point3.UnitX
			};
			conners = new Point3[4]
			{
				new Point3(-1, 0, -1),
				new Point3(1, 0, -1),
				new Point3(1, 0, 1),
				new Point3(-1, 0, 1)
			};
			break;
		}
	}

	public static DeviceBlock? FindDevice(SubsystemTerrain subsystemTerrain, Point3 position, out int blockValue)
	{
		blockValue = subsystemTerrain.Terrain.GetCellValueFast(position.X, position.Y, position.Z);
		if (BlocksManager.Blocks[Terrain.ExtractContents(blockValue)] is BaseDeviceBlock)
		{
			return DevicesBlockManager.GetDevice(blockValue);
		}
		return null;
	}

	public static int SetInnerData(int value, int innerData)
	{
		if (innerData < 0 || innerData > 7)
		{
			throw new OverflowException($"{innerData} is out of range.");
		}
		int num = Terrain.ExtractData(value);
		num &= -8;
		num |= innerData;
		return Terrain.ReplaceData(value, num);
	}

	public static int GetDirectionXZ(ComponentMiner componentMiner, bool isOpposite = false)
	{
		Vector3 forwardVector = componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation.GetForwardVector();
		float num = forwardVector.Dot(Vector3.UnitZ);
		float num2 = forwardVector.Dot(Vector3.UnitX);
		float num3 = forwardVector.Dot(-Vector3.UnitZ);
		float num4 = forwardVector.Dot(-Vector3.UnitX);
		float num5 = MathUtils.Max(num, num2, num3, num4);
		int num6 = 0;
		if (num == num5)
		{
			num6 = 2;
		}
		if (num2 == num5)
		{
			num6 = 3;
		}
		if (num3 == num5)
		{
			num6 = 0;
		}
		if (num4 == num5)
		{
			num6 = 1;
		}
		if (!isOpposite)
		{
			return num6;
		}
		return CellFace.OppositeFace(num6);
	}

	public static int GetDirectionXYZ(ComponentMiner componentMiner, bool isOpposite = false)
	{
		Vector3 forwardVector = componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation.GetForwardVector();
		float num = forwardVector.Dot(Vector3.UnitZ);
		float num2 = forwardVector.Dot(Vector3.UnitX);
		float num3 = forwardVector.Dot(-Vector3.UnitZ);
		float num4 = forwardVector.Dot(-Vector3.UnitX);
		float num5 = forwardVector.Dot(Vector3.UnitY);
		float num6 = forwardVector.Dot(-Vector3.UnitY);
		float num7 = MathUtils.Min(MathUtils.Min(num, num2, num3), MathUtils.Min(num4, num5, num6));
		int num8 = 0;
		if (num == num7)
		{
			num8 = 0;
		}
		if (num2 == num7)
		{
			num8 = 1;
		}
		if (num3 == num7)
		{
			num8 = 2;
		}
		if (num4 == num7)
		{
			num8 = 3;
		}
		if (num5 == num7)
		{
			num8 = 4;
		}
		if (num6 == num7)
		{
			num8 = 5;
		}
		if (!isOpposite)
		{
			return num8;
		}
		return CellFace.OppositeFace(num8);
	}

	public static void GetSideFaceOfXZ(int face, out int face1, out int face2)
	{
		face1 = -1;
		face2 = -1;
		switch (face)
		{
		case 0:
			face1 = 1;
			face2 = 3;
			break;
		case 1:
			face1 = 0;
			face2 = 2;
			break;
		case 2:
			face1 = 3;
			face2 = 1;
			break;
		case 3:
			face1 = 2;
			face2 = 0;
			break;
		}
	}

	public static Vector3 NormalToVector3(int faceNormal)
	{
		switch (faceNormal)
		{
		case 0:
		case 2:
			return new Vector3(1f, 1f, 0f);
		case 1:
		case 3:
			return new Vector3(0f, 1f, 1f);
		case 4:
		case 5:
			return new Vector3(1f, 0f, 1f);
		default:
			return Vector3.Zero;
		}
	}

	public static Vector3 FaceToVector3Abs(int face)
	{
		return m_faceToVector3[face];
	}

	public static int GetLeftFace(int face)
	{
		return CellFace.Vector3ToFace(CellFace.FaceToVector3(face).Transform(Quaternion.CreateFromAxisAngle(Vector3.UnitY, -(float)Math.PI / 2f)));
	}

	public static int GetRightFace(int face)
	{
		return CellFace.Vector3ToFace(CellFace.FaceToVector3(face).Transform(Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2f)));
	}
}

using System;
using Engine.Media;
using SixLabors.ImageSharp.PixelFormats;

namespace SCIENEW;

public class BitmapRenderer
{
	private readonly Image _bitmap;

	private readonly Rgba32[] _colorPalette;

	private readonly Random _random = new Random(DateTime.Now.GetHashCode());

	public BitmapRenderer()
	{
		_colorPalette = new Rgba32[64];
		_colorPalette[0] = new Rgba32(84, 84, 84);
		_colorPalette[1] = new Rgba32(0, 30, 116);
		_colorPalette[2] = new Rgba32(8, 16, 144);
		_colorPalette[3] = new Rgba32(48, 0, 136);
		_colorPalette[4] = new Rgba32(68, 0, 100);
		_colorPalette[5] = new Rgba32(92, 0, 48);
		_colorPalette[6] = new Rgba32(84, 4, 0);
		_colorPalette[7] = new Rgba32(60, 24, 0);
		_colorPalette[8] = new Rgba32(32, 42, 0);
		_colorPalette[9] = new Rgba32(8, 58, 0);
		_colorPalette[10] = new Rgba32(0, 64, 0);
		_colorPalette[11] = new Rgba32(0, 60, 0);
		_colorPalette[12] = new Rgba32(0, 50, 60);
		_colorPalette[13] = new Rgba32(0, 0, 0);
		_colorPalette[14] = new Rgba32(0, 0, 0);
		_colorPalette[15] = new Rgba32(0, 0, 0);
		_colorPalette[16] = new Rgba32(152, 150, 152);
		_colorPalette[17] = new Rgba32(8, 76, 196);
		_colorPalette[18] = new Rgba32(48, 50, 236);
		_colorPalette[19] = new Rgba32(92, 30, 228);
		_colorPalette[20] = new Rgba32(136, 20, 176);
		_colorPalette[21] = new Rgba32(160, 20, 100);
		_colorPalette[22] = new Rgba32(152, 34, 32);
		_colorPalette[23] = new Rgba32(120, 60, 0);
		_colorPalette[24] = new Rgba32(84, 90, 0);
		_colorPalette[25] = new Rgba32(40, 114, 0);
		_colorPalette[26] = new Rgba32(8, 124, 0);
		_colorPalette[27] = new Rgba32(0, 118, 40);
		_colorPalette[28] = new Rgba32(0, 102, 120);
		_colorPalette[29] = new Rgba32(0, 0, 0);
		_colorPalette[30] = new Rgba32(0, 0, 0);
		_colorPalette[31] = new Rgba32(0, 0, 0);
		_colorPalette[32] = new Rgba32(236, 238, 236);
		_colorPalette[33] = new Rgba32(76, 154, 236);
		_colorPalette[34] = new Rgba32(120, 124, 236);
		_colorPalette[35] = new Rgba32(176, 98, 236);
		_colorPalette[36] = new Rgba32(228, 84, 236);
		_colorPalette[37] = new Rgba32(236, 88, 180);
		_colorPalette[38] = new Rgba32(236, 106, 100);
		_colorPalette[39] = new Rgba32(212, 136, 32);
		_colorPalette[40] = new Rgba32(160, 170, 0);
		_colorPalette[41] = new Rgba32(116, 196, 0);
		_colorPalette[42] = new Rgba32(76, 208, 32);
		_colorPalette[43] = new Rgba32(56, 204, 108);
		_colorPalette[44] = new Rgba32(56, 180, 204);
		_colorPalette[45] = new Rgba32(60, 60, 60);
		_colorPalette[46] = new Rgba32(0, 0, 0);
		_colorPalette[47] = new Rgba32(0, 0, 0);
		_colorPalette[48] = new Rgba32(236, 238, 236);
		_colorPalette[49] = new Rgba32(168, 204, 236);
		_colorPalette[50] = new Rgba32(188, 188, 236);
		_colorPalette[51] = new Rgba32(212, 178, 236);
		_colorPalette[52] = new Rgba32(236, 174, 236);
		_colorPalette[53] = new Rgba32(236, 174, 212);
		_colorPalette[54] = new Rgba32(236, 180, 176);
		_colorPalette[55] = new Rgba32(228, 196, 144);
		_colorPalette[56] = new Rgba32(204, 210, 120);
		_colorPalette[57] = new Rgba32(180, 222, 120);
		_colorPalette[58] = new Rgba32(168, 226, 144);
		_colorPalette[59] = new Rgba32(152, 226, 180);
		_colorPalette[60] = new Rgba32(160, 214, 228);
		_colorPalette[61] = new Rgba32(160, 162, 160);
		_colorPalette[62] = new Rgba32(0, 0, 0);
		_colorPalette[63] = new Rgba32(0, 0, 0);
		_bitmap = new Image(256, 240);
	}

	public Image Render(byte[] bitmap)
	{
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				_bitmap.SetPixelFast(j, i, _colorPalette[bitmap[i * 256 + j]]);
			}
		}
		return _bitmap;
	}

	public byte[] GenerateNoise()
	{
		byte[] array = new byte[61440];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (byte)((_random.Next(0, 10) <= 5) ? 13 : 48);
		}
		return array;
	}
}

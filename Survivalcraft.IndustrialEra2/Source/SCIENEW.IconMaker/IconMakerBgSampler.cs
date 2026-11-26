using System;
using Engine.Graphics;
using Game;

namespace SCIENEW.IconMaker;

public static class IconMakerBgSampler
{
	public readonly record struct Entry(IconMakerBgSamplerMode Mode, SamplerState Sampler, string LangKey);

	public static readonly Entry[] Options = new Entry[6]
	{
		new Entry(IconMakerBgSamplerMode.PointClamp, SamplerState.PointClamp, "FilterPointClamp"),
		new Entry(IconMakerBgSamplerMode.PointWrap, SamplerState.PointWrap, "FilterPointWrap"),
		new Entry(IconMakerBgSamplerMode.LinearClamp, SamplerState.LinearClamp, "FilterLinearClamp"),
		new Entry(IconMakerBgSamplerMode.LinearWrap, SamplerState.LinearWrap, "FilterLinearWrap"),
		new Entry(IconMakerBgSamplerMode.AnisotropicClamp, SamplerState.AnisotropicClamp, "FilterAnisotropicClamp"),
		new Entry(IconMakerBgSamplerMode.AnisotropicWrap, SamplerState.AnisotropicWrap, "FilterAnisotropicWrap")
	};

	public static SamplerState ToSampler(IconMakerBgSamplerMode mode)
	{
		Entry[] options = Options;
		for (int i = 0; i < options.Length; i++)
		{
			Entry entry = options[i];
			if (entry.Mode == mode)
			{
				return entry.Sampler;
			}
		}
		return SamplerState.PointClamp;
	}

	public static string GetLabel(IconMakerBgSamplerMode mode)
	{
		Entry[] options = Options;
		for (int i = 0; i < options.Length; i++)
		{
			Entry entry = options[i];
			if (entry.Mode == mode)
			{
				return LanguageControl.Get("ContentWidgets", "IconMakerScreen", entry.LangKey);
			}
		}
		return string.Empty;
	}

	public static IconMakerBgSamplerMode Next(IconMakerBgSamplerMode mode)
	{
		int num = Array.FindIndex(Options, (Entry e) => e.Mode == mode);
		if (num < 0)
		{
			num = 0;
		}
		return Options[(num + 1) % Options.Length].Mode;
	}
}

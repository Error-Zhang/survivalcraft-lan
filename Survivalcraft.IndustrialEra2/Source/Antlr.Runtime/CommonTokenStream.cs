using System;

namespace Antlr.Runtime;

[Serializable]
public class CommonTokenStream : BufferedTokenStream
{
	public int Channel { get; private set; }

	public override ITokenSource TokenSource
	{
		get
		{
			return base.TokenSource;
		}
		set
		{
			base.TokenSource = value;
			Channel = 0;
		}
	}

	public CommonTokenStream()
	{
	}

	public CommonTokenStream(ITokenSource tokenSource)
		: this(tokenSource, 0)
	{
	}

	public CommonTokenStream(ITokenSource tokenSource, int channel)
		: base(tokenSource)
	{
		Channel = channel;
	}

	public override void Consume()
	{
		if (_p == -1)
		{
			Setup();
		}
		_p++;
		_p = SkipOffTokenChannels(_p);
	}

	protected override IToken LB(int k)
	{
		if (k == 0 || _p - k < 0)
		{
			return null;
		}
		int num = _p;
		for (int i = 1; i <= k; i++)
		{
			num = SkipOffTokenChannelsReverse(num - 1);
		}
		if (num < 0)
		{
			return null;
		}
		return _tokens[num];
	}

	public override IToken LT(int k)
	{
		if (_p == -1)
		{
			Setup();
		}
		if (k == 0)
		{
			return null;
		}
		if (k < 0)
		{
			return LB(-k);
		}
		int num = _p;
		for (int i = 1; i < k; i++)
		{
			num = SkipOffTokenChannels(num + 1);
		}
		if (num > Range)
		{
			Range = num;
		}
		return _tokens[num];
	}

	protected virtual int SkipOffTokenChannels(int i)
	{
		Sync(i);
		while (_tokens[i].Channel != Channel)
		{
			i++;
			Sync(i);
		}
		return i;
	}

	protected virtual int SkipOffTokenChannelsReverse(int i)
	{
		while (i >= 0 && _tokens[i].Channel != Channel)
		{
			i--;
		}
		return i;
	}

	protected override void Setup()
	{
		_p = 0;
		_p = SkipOffTokenChannels(_p);
	}
}

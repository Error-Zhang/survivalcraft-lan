using System.Text;

namespace Antlr.Runtime.Tree;

public class TreePatternLexer
{
	public const int Begin = 1;

	public const int End = 2;

	public const int Id = 3;

	public const int Arg = 4;

	public const int Percent = 5;

	public const int Colon = 6;

	public const int Dot = 7;

	protected string pattern;

	protected int p = -1;

	protected int c;

	protected int n;

	public StringBuilder sval = new StringBuilder();

	public bool error;

	public TreePatternLexer(string pattern)
	{
		this.pattern = pattern;
		n = pattern.Length;
		Consume();
	}

	public virtual int NextToken()
	{
		sval.Length = 0;
		while (c != -1)
		{
			if (c == 32 || c == 10 || c == 13 || c == 9)
			{
				Consume();
				continue;
			}
			int num = c;
			if (num < 97 || num > 122)
			{
				num = c;
				if ((num < 65 || num > 90) && c != 95)
				{
					if (c == 40)
					{
						Consume();
						return 1;
					}
					if (c == 41)
					{
						Consume();
						return 2;
					}
					if (c == 37)
					{
						Consume();
						return 5;
					}
					if (c == 58)
					{
						Consume();
						return 6;
					}
					if (c == 46)
					{
						Consume();
						return 7;
					}
					if (c == 91)
					{
						Consume();
						while (c != 93)
						{
							if (c == 92)
							{
								Consume();
								if (c != 93)
								{
									sval.Append('\\');
								}
								sval.Append((char)c);
							}
							else
							{
								sval.Append((char)c);
							}
							Consume();
						}
						Consume();
						return 4;
					}
					Consume();
					error = true;
					return -1;
				}
			}
			sval.Append((char)c);
			Consume();
			while (true)
			{
				num = c;
				if (num < 97 || num > 122)
				{
					num = c;
					if (num < 65 || num > 90)
					{
						num = c;
						if ((num < 48 || num > 57) && c != 95)
						{
							break;
						}
					}
				}
				sval.Append((char)c);
				Consume();
			}
			return 3;
		}
		return -1;
	}

	protected virtual void Consume()
	{
		p++;
		if (p >= n)
		{
			c = -1;
		}
		else
		{
			c = pattern[p];
		}
	}
}

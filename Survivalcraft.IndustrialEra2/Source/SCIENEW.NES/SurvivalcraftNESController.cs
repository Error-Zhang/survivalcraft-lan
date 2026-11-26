using XamariNES.Common.Extensions;
using XamariNES.Controller;
using XamariNES.Controller.Enums;

namespace SCIENEW.NES;

public class SurvivalcraftNESController : IController
{
	private byte _buttonStatusShift;

	private bool _isPolling;

	public BiDictionary<SurvivalcraftNESButtons, enumButtons> _buttonMap = new BiDictionary<SurvivalcraftNESButtons, enumButtons>
	{
		{
			SurvivalcraftNESButtons.A,
			enumButtons.A
		},
		{
			SurvivalcraftNESButtons.B,
			enumButtons.B
		},
		{
			SurvivalcraftNESButtons.Start,
			enumButtons.Start
		},
		{
			SurvivalcraftNESButtons.Select,
			enumButtons.Select
		},
		{
			SurvivalcraftNESButtons.Up,
			enumButtons.Up
		},
		{
			SurvivalcraftNESButtons.Down,
			enumButtons.Down
		},
		{
			SurvivalcraftNESButtons.Left,
			enumButtons.Left
		},
		{
			SurvivalcraftNESButtons.Right,
			enumButtons.Right
		}
	};

	public byte ButtonStates { get; set; }

	public void ButtonPress(SurvivalcraftNESButtons button)
	{
		ButtonPress(_buttonMap[button]);
	}

	public void ButtonRelease(SurvivalcraftNESButtons button)
	{
		ButtonRelease(_buttonMap[button]);
	}

	public void Input(byte input)
	{
		for (int i = 0; i < 8; i++)
		{
			if (input.IsBitSet(i))
			{
				ButtonPress((SurvivalcraftNESButtons)(1 << i));
			}
			else
			{
				ButtonRelease((SurvivalcraftNESButtons)(1 << i));
			}
		}
	}

	public void ButtonPress(enumButtons button)
	{
		ButtonStates |= (byte)button;
	}

	public void ButtonRelease(enumButtons button)
	{
		ButtonStates &= (byte)(~(int)button);
	}

	public void SignalController(byte input)
	{
		if (input.IsBitSet(0))
		{
			_isPolling = false;
			_buttonStatusShift = 0;
		}
		else
		{
			_isPolling = true;
		}
	}

	public byte ReadController()
	{
		if (_buttonStatusShift > 7)
		{
			return 1;
		}
		bool result = ButtonStates.IsBitSet(_buttonStatusShift);
		if (_isPolling)
		{
			_buttonStatusShift++;
		}
		return result ? ((byte)1) : ((byte)0);
	}
}

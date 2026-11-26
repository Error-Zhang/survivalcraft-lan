using System;
using System.IO;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Game;
using GameEntitySystem;
using SCIENEW.NES;
using SCIENEW.Screens;
using SCIENEW.VoltNet;
using TemplatesDatabase;
using XamariNES.Emulator;

namespace SCIENEW;

public class ComponentFamicon : Component, IScreenProviderComponent, IUpdateable
{
	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemVoltNet m_subsystemVoltNet;

	protected NESEmulator m_emulator;

	protected byte[] m_frame = new byte[61440];

	protected BitmapRenderer m_renderer = new BitmapRenderer();

	public readonly TexturedBatch3D cachedBatch = new TexturedBatch3D
	{
		BlendState = BlendState.AlphaBlend,
		DepthStencilState = DepthStencilState.DepthRead,
		Layer = 0,
		RasterizerState = SCIENEW.Screens.Screen.QuadRasterizerState,
		SamplerState = SamplerState.PointClamp
	};

	public readonly FontBatch3D m_fontBatch3D = new FontBatch3D
	{
		BlendState = BlendState.AlphaBlend,
		DepthStencilState = DepthStencilState.DepthRead,
		Layer = 0,
		RasterizerState = SCIENEW.Screens.Screen.FontRasterizerState,
		SamplerState = SamplerState.PointClamp
	};

	protected DateTime m_lastUpdatedTime = DateTime.MinValue;

	public readonly bool RomValid = true;

	protected Exception? m_exception;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public bool PowerOn { get; set; } = true;

	public bool Reset { get; set; }

	public bool EmuStarted { get; protected set; }

	public NESFile? CurrentGame { get; protected set; }

	public SurvivalcraftNESController InputController { get; protected set; } = new SurvivalcraftNESController();

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		string value = valuesDictionary.GetValue("CurrentGame", string.Empty);
		if (NESManager.TryGet((value == string.Empty) ? "snailmaze" : value, out NESFile nesFile))
		{
			CurrentGame = nesFile;
			m_emulator = new NESEmulator(nesFile.Data, InputController, GetFrameFromEmulator);
		}
		else
		{
			nesFile = (CurrentGame = NESManager.NesFiles.First().Value);
			m_emulator = new NESEmulator(nesFile.Data, InputController, GetFrameFromEmulator);
		}
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		if (CurrentGame != null)
		{
			valuesDictionary.SetValue("CurrentGame", CurrentGame.FileName);
		}
	}

	public void ReloadGame(NESFile nes)
	{
		CurrentGame = nes;
		m_emulator.Reset();
		m_emulator = new NESEmulator(nes.Data, InputController, GetFrameFromEmulator);
	}

	public void Draw(SCIENEW.Screens.Screen screen, Project project, Camera camera, int drawOrder)
	{
		if (PowerOn)
		{
			if (m_exception == null)
			{
				ScreensManager.QueueQuad(cachedBatch, screen.Corner, screen.Right, screen.Up, Color.White);
				cachedBatch.Flush(camera.ViewProjectionMatrix);
				return;
			}
			screen.GetTextAxes(0.003f, out var right, out var down);
			m_fontBatch3D.QueueText(LanguageControl.Get("Components", "ComponentFamicon", "ErrorText") + "\n" + m_exception.Message, screen.CenterPos, right, down, Color.Red, TextAnchor.Center);
			m_fontBatch3D.Flush(camera.ViewProjectionMatrix);
		}
	}

	public byte GetInput()
	{
		byte b = 0;
		if (Keyboard.IsKeyDown(Key.RightArrow))
		{
			b |= 0x80;
		}
		if (Keyboard.IsKeyDown(Key.LeftArrow))
		{
			b |= 0x40;
		}
		if (Keyboard.IsKeyDown(Key.DownArrow))
		{
			b |= 0x20;
		}
		if (Keyboard.IsKeyDown(Key.UpArrow))
		{
			b |= 0x10;
		}
		if (Keyboard.IsKeyDown(Key.Q))
		{
			b |= 8;
		}
		if (Keyboard.IsKeyDown(Key.W))
		{
			b |= 4;
		}
		if (Keyboard.IsKeyDown(Key.B))
		{
			b |= 2;
		}
		if (Keyboard.IsKeyDown(Key.A))
		{
			b |= 1;
		}
		return b;
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		VoltElement? voltElement = m_subsystemVoltNet.GetVoltElement(coordinates);
		PowerOn = voltElement != null && voltElement.CurrentWorkState == WorkState.Active;
		try
		{
			m_exception = null;
			if (Reset)
			{
				m_emulator.Reset();
				EmuStarted = false;
			}
			else if (PowerOn)
			{
				if (EmuStarted)
				{
					m_emulator.Continue();
				}
				else
				{
					m_emulator.Start();
					EmuStarted = true;
				}
			}
			else if (EmuStarted)
			{
				m_emulator.Stop();
				EmuStarted = false;
			}
		}
		catch (Exception exception)
		{
			m_exception = exception;
		}
		if (Reset || !PowerOn)
		{
			return;
		}
		DateTime now = DateTime.Now;
		if ((now - m_lastUpdatedTime).TotalMilliseconds > 33.0)
		{
			m_lastUpdatedTime = now;
			if (!RomValid)
			{
				m_frame = m_renderer.GenerateNoise();
			}
			cachedBatch.Texture?.Dispose();
			cachedBatch.Texture = Texture2D.Load(m_renderer.Render(m_frame));
		}
	}

	public void LoadRomFromPath(string path)
	{
		byte[] array = null;
		try
		{
			if (path == "nestest")
			{
				array = ContentManager.Get<NESFile>("Nestest").Data;
			}
			else
			{
				using Stream stream = Storage.OpenFile(path, OpenFileMode.Read);
				array = GetByteFromStream(stream);
			}
		}
		catch (Exception e)
		{
			Log.Error(e);
		}
		if (array != null)
		{
			m_emulator._cartridge.LoadROM(array);
			m_emulator.LoadRom(array);
		}
	}

	public Entity GetEntity()
	{
		return Entity;
	}

	public Vector3 GetPosition()
	{
		return new Vector3(m_componentBlockEntity.Coordinates) + Vector3.One * 0.5f;
	}

	public BoundingBox GetBoundingBox()
	{
		return new BoundingBox(new Vector3(m_componentBlockEntity.Coordinates), new Vector3(m_componentBlockEntity.Coordinates) + Vector3.One);
	}

	public string GetDescription()
	{
		return LanguageControl.Get("Components", "ComponentFamicon", "ProviderDescription");
	}

	private void GetFrameFromEmulator(byte[] frame)
	{
		m_frame = frame;
	}

	public static byte[] GetByteFromStream(Stream stream)
	{
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, (int)stream.Length);
		return array;
	}
}

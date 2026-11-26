using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.IconMaker;

namespace SCIENEW;

public class IconMakerScreen : Screen
{
	private IndustryBackgroundWidget m_background;

	private CanvasWidget m_previewHost;

	private StackPanelWidget m_inspector;

	private LabelWidget m_statusLabel;

	private IconMakerPreviewWidget m_preview;

	private BevelledButtonWidget m_btnExport;

	private BevelledButtonWidget m_btnPickSubjectBlock;

	private BevelledButtonWidget m_btnPickSubjectTexture;

	private BevelledButtonWidget m_btnPickBackground;

	private BevelledButtonWidget m_glowShadowToggle;

	private BevelledButtonWidget m_outlineColorBtn;

	private BevelledButtonWidget m_glowColorBtn;

	private BevelledButtonWidget m_bgFilterBtn;

	private readonly IconMakerProject m_project = new IconMakerProject();

	private readonly List<IconMakerSliderRow> m_sliders = new List<IconMakerSliderRow>();

	private Screen? m_previousScreen;

	public IconMakerScreen()
	{
		XElement node = ContentManager.Get<XElement>("Screens/IconMakerScreen");
		LoadContents(this, node);
		m_background = Children.Find<IndustryBackgroundWidget>("BG");
		m_previewHost = Children.Find<CanvasWidget>("PreviewHost");
		m_inspector = Children.Find<StackPanelWidget>("InspectorPanel");
		m_statusLabel = Children.Find<LabelWidget>("StatusLabel");
		m_btnExport = Children.Find<BevelledButtonWidget>("BtnExport");
		m_btnPickSubjectBlock = Children.Find<BevelledButtonWidget>("BtnPickSubjectBlock");
		m_btnPickSubjectTexture = Children.Find<BevelledButtonWidget>("BtnPickSubjectTexture");
		m_btnPickBackground = Children.Find<BevelledButtonWidget>("BtnPickBackground");
		m_preview = new IconMakerPreviewWidget
		{
			Project = m_project,
			HorizontalAlignment = WidgetAlignment.Stretch,
			VerticalAlignment = WidgetAlignment.Stretch,
			Margin = new Vector2(8f)
		};
		m_previewHost.Children.Add(m_preview);
		BuildInspector();
	}

	private void BuildInspector()
	{
		AddSectionTitle("30");
		AddSlider("31", 128f, 2048f, 64f, m_project.OutputSize, (float v) =>
		{
			m_project.OutputSize = (int)v;
			MarkDirty();
		});
		AddSectionTitle("40");
		AddSlider("41", 0.2f, 2.5f, 0.05f, m_project.SubjectScale, (float v) =>
		{
			m_project.SubjectScale = v;
			MarkDirty();
		});
		AddSlider("42", -1f, 1f, 0.02f, m_project.SubjectOffset.X, (float v) =>
		{
			m_project.SubjectOffset = new Vector2(v, m_project.SubjectOffset.Y);
			MarkDirty();
		});
		AddSlider("43", -1f, 1f, 0.02f, m_project.SubjectOffset.Y, (float v) =>
		{
			m_project.SubjectOffset = new Vector2(m_project.SubjectOffset.X, v);
			MarkDirty();
		});
		AddSectionTitle("50");
		AddSlider("51", 0f, 24f, 0.5f, m_project.OutlineWidth, (float v) =>
		{
			m_project.OutlineWidth = v;
			m_project.OutlineEnabled = v > 0.25f;
			MarkDirty();
		});
		m_outlineColorBtn = AddColorButton("52", m_project.OutlineColor);
		AddSectionTitle("60");
		AddSlider("61", 0f, 48f, 1f, m_project.GlowRadius, (float v) =>
		{
			m_project.GlowRadius = v;
			m_project.GlowEnabled = v > 0.25f;
			MarkDirty();
		});
		AddSlider("62", 0f, 1.5f, 0.02f, m_project.GlowIntensity, (float v) =>
		{
			m_project.GlowIntensity = v;
			MarkDirty();
		});
		AddSlider("63", -32f, 32f, 1f, m_project.GlowOffset.X, (float v) =>
		{
			m_project.GlowOffset = new Vector2(v, m_project.GlowOffset.Y);
			MarkDirty();
		});
		AddSlider("64", -32f, 32f, 1f, m_project.GlowOffset.Y, (float v) =>
		{
			m_project.GlowOffset = new Vector2(m_project.GlowOffset.X, v);
			MarkDirty();
		});
		m_glowColorBtn = AddColorButton("65", m_project.GlowColor);
		AddSectionTitle("70");
		m_bgFilterBtn = AddCycleButton("82", GetBgFilterButtonText());
		AddSlider("71", 0.1f, 8f, 0.1f, m_project.BgTileScale, (float v) =>
		{
			m_project.BgTileScale = v;
			MarkDirty();
		});
		AddSlider("72", 0f, 0.49f, 0.01f, m_project.BgCropMin.X, (float v) =>
		{
			m_project.BgCropMin = new Vector2(v, m_project.BgCropMin.Y);
			MarkDirty();
		});
		AddSlider("73", 0f, 0.49f, 0.01f, m_project.BgCropMin.Y, (float v) =>
		{
			m_project.BgCropMin = new Vector2(m_project.BgCropMin.X, v);
			MarkDirty();
		});
		AddSlider("74", 0.51f, 1f, 0.01f, m_project.BgCropMax.X, (float v) =>
		{
			m_project.BgCropMax = new Vector2(v, m_project.BgCropMax.Y);
			MarkDirty();
		});
		AddSlider("75", 0.51f, 1f, 0.01f, m_project.BgCropMax.Y, (float v) =>
		{
			m_project.BgCropMax = new Vector2(m_project.BgCropMax.X, v);
			MarkDirty();
		});
		AddSlider("76", 0f, 24f, 0.5f, m_project.BgBlur, (float v) =>
		{
			m_project.BgBlur = v;
			MarkDirty();
		});
		AddSlider("77", -1f, 1f, 0.02f, m_project.BgBrightness, (float v) =>
		{
			m_project.BgBrightness = v;
			MarkDirty();
		});
		AddSlider("78", -1f, 1f, 0.02f, m_project.BgContrast, (float v) =>
		{
			m_project.BgContrast = v;
			MarkDirty();
		});
		AddSlider("79", -1f, 1f, 0.02f, m_project.BgSaturation, (float v) =>
		{
			m_project.BgSaturation = v;
			MarkDirty();
		});
		AddSlider("80", -1f, 1f, 0.02f, m_project.BgVibrance, (float v) =>
		{
			m_project.BgVibrance = v;
			MarkDirty();
		});
		m_inspector.Children.Add(new BevelledButtonWidget
		{
			Text = LanguageControl.Get("ContentWidgets", "IconMakerScreen", "81"),
			Size = new Vector2(388f, 52f),
			HorizontalAlignment = WidgetAlignment.Stretch,
			MarginTop = 12f,
			IsAutoCheckingEnabled = true,
			IsChecked = (m_project.GlowMode == IconMakerGlowMode.Shadow)
		});
		WidgetsList children = m_inspector.Children;
		m_glowShadowToggle = (BevelledButtonWidget)children[children.Count - 1];
	}

	private string GetBgFilterButtonText()
	{
		return LanguageControl.Get("ContentWidgets", "IconMakerScreen", "82") + ": " + IconMakerBgSampler.GetLabel(m_project.BgSamplerMode);
	}

	private BevelledButtonWidget AddCycleButton(string langKey, string initialText)
	{
		BevelledButtonWidget bevelledButtonWidget = new BevelledButtonWidget
		{
			Text = initialText,
			Size = new Vector2(388f, 52f),
			HorizontalAlignment = WidgetAlignment.Stretch,
			MarginTop = 4f,
			FontScale = 0.78f
		};
		m_inspector.Children.Add(bevelledButtonWidget);
		return bevelledButtonWidget;
	}

	private void AddSectionTitle(string langKey)
	{
		m_inspector.Children.Add(new LabelWidget
		{
			Text = LanguageControl.Get("ContentWidgets", "IconMakerScreen", langKey),
			Color = new Color(220, 220, 220),
			FontScale = 0.85f,
			Size = new Vector2(388f, 28f),
			HorizontalAlignment = WidgetAlignment.Stretch,
			MarginTop = ((m_inspector.Children.Count > 0) ? 12f : 0f),
			MarginBottom = 4f
		});
	}

	private void AddSlider(string langKey, float min, float max, float granularity, float initial, Action<float> onChanged)
	{
		IconMakerSliderRow iconMakerSliderRow = new IconMakerSliderRow(LanguageControl.Get("ContentWidgets", "IconMakerScreen", langKey), min, max, granularity, initial, onChanged);
		m_sliders.Add(iconMakerSliderRow);
		m_inspector.Children.Add(iconMakerSliderRow.Container);
	}

	private BevelledButtonWidget AddColorButton(string langKey, Color initial)
	{
		CanvasWidget canvasWidget = new CanvasWidget
		{
			Size = new Vector2(388f, 52f),
			HorizontalAlignment = WidgetAlignment.Stretch,
			MarginTop = 4f
		};
		canvasWidget.Children.Add(new LabelWidget
		{
			Text = LanguageControl.Get("ContentWidgets", "IconMakerScreen", langKey),
			Color = new Color(200, 200, 200),
			FontScale = 0.75f,
			Size = new Vector2(132f, 52f),
			TextAnchor = TextAnchor.VerticalCenter
		});
		BevelledButtonWidget bevelledButtonWidget = new BevelledButtonWidget
		{
			Size = new Vector2(192f, 44f),
			BevelSize = 1f,
			AmbientLight = 1f,
			CenterColor = initial
		};
		canvasWidget.Children.Add(bevelledButtonWidget);
		canvasWidget.SetWidgetPosition(bevelledButtonWidget, new Vector2(132f, 4f));
		m_inspector.Children.Add(canvasWidget);
		return bevelledButtonWidget;
	}

	private void SyncColorButtons()
	{
		m_outlineColorBtn.CenterColor = m_project.OutlineColor;
		m_glowColorBtn.CenterColor = m_project.GlowColor;
	}

	public override void Enter(object[] parameters)
	{
		m_previousScreen = ScreensManager.PreviousScreen;
		bool isStatic = IESettingsManager.Get<bool>("IndustrialHubStaticBackground");
		m_background.IsStatic = isStatic;
		IconMakerAssetCatalog.Refresh();
		SyncColorButtons();
		m_bgFilterBtn.Text = GetBgFilterButtonText();
		RefreshStatus();
		MarkDirty();
	}

	private void MarkDirty()
	{
		m_preview.IsDirty = true;
		RefreshStatus();
	}

	private void RefreshStatus()
	{
		string arg = ((m_project.SubjectKind == IconMakerSubjectKind.Block) ? string.Format(CultureInfo.CurrentCulture, LanguageControl.Get("ContentWidgets", "IconMakerScreen", "90"), m_project.BlockValue) : m_project.TexturePath);
		m_statusLabel.Text = string.Format(CultureInfo.CurrentCulture, LanguageControl.Get("ContentWidgets", "IconMakerScreen", "91"), m_project.OutputSize, arg, m_project.BackgroundTexturePath);
	}

	public override void Update()
	{
		foreach (IconMakerSliderRow slider in m_sliders)
		{
			slider.Poll();
		}
		if (m_glowShadowToggle.IsClicked)
		{
			m_project.GlowMode = (m_glowShadowToggle.IsChecked ? IconMakerGlowMode.Shadow : IconMakerGlowMode.Glow);
			MarkDirty();
		}
		if (m_outlineColorBtn.IsClicked)
		{
			DialogsManager.ShowDialog(this, new EditColorDialog(m_project.OutlineColor, (Color? color) =>
			{
				if (color.HasValue)
				{
					m_project.OutlineColor = color.Value;
					m_outlineColorBtn.CenterColor = color.Value;
					MarkDirty();
				}
			}));
		}
		if (m_glowColorBtn.IsClicked)
		{
			DialogsManager.ShowDialog(this, new EditColorDialog(m_project.GlowColor, (Color? color) =>
			{
				if (color.HasValue)
				{
					m_project.GlowColor = color.Value;
					m_glowColorBtn.CenterColor = color.Value;
					MarkDirty();
				}
			}));
		}
		if (m_bgFilterBtn.IsClicked)
		{
			m_project.BgSamplerMode = IconMakerBgSampler.Next(m_project.BgSamplerMode);
			m_bgFilterBtn.Text = GetBgFilterButtonText();
			MarkDirty();
		}
		if (m_btnExport.IsClicked)
		{
			ExportIcon();
		}
		if (m_btnPickSubjectBlock.IsClicked)
		{
			DialogsManager.ShowDialog(this, new SelectBlockDialog(LanguageControl.Get("ContentWidgets", "IconMakerScreen", "11"), (int value) =>
			{
				m_project.SubjectKind = IconMakerSubjectKind.Block;
				m_project.BlockValue = value;
				MarkDirty();
			}));
		}
		if (m_btnPickSubjectTexture.IsClicked)
		{
			DialogsManager.ShowDialog(this, new SelectTextureDialog(LanguageControl.Get("ContentWidgets", "IconMakerScreen", "12"), (string path) =>
			{
				m_project.SubjectKind = IconMakerSubjectKind.Texture;
				m_project.TexturePath = path;
				MarkDirty();
			}));
		}
		if (m_btnPickBackground.IsClicked)
		{
			DialogsManager.ShowDialog(this, new SelectTextureDialog(LanguageControl.Get("ContentWidgets", "IconMakerScreen", "13"), (string path) =>
			{
				m_project.BackgroundTexturePath = path;
				MarkDirty();
			}));
		}
		if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
		{
			if (ScreensManager.TopOfHistoryScreen != null)
			{
				ScreensManager.GoBack();
			}
			else if (m_previousScreen != null)
			{
				ScreensManager.SwitchScreen(m_previousScreen);
			}
		}
	}

	private void ExportIcon()
	{
		string text = Storage.CombinePaths(ModsManager.ExternalPath, "Icons");
		if (!Storage.DirectoryExists(text))
		{
			Storage.CreateDirectory(text);
		}
		string text2 = $"icon_{DateTime.Now:yyyyMMdd_HHmmss}.png";
		string text3 = Storage.CombinePaths(text, text2);
		try
		{
			IconMakerRenderer.ExportPng(m_project, text3);
			DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "IconMakerScreen", "100"), string.Format(CultureInfo.CurrentCulture, LanguageControl.Get("ContentWidgets", "IconMakerScreen", "101"), text3), LanguageControl.Ok, null, (MessageDialogButton _) =>
			{
			}));
			try
			{
				Storage.OpenFileWithExternalApplication(text3);
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "IconMakerScreen", "102"), ex.Message, LanguageControl.Ok, null, (MessageDialogButton _) =>
			{
			}));
		}
	}
}

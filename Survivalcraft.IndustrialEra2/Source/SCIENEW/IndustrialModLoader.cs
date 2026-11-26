using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Media;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using RecipaediaEX.Overlay;
using RecipaediaEX.UI;
using SCIENEW.AssetProtection;
using SCIENEW.Benchmark;
using SCIENEW.Events;
using SCIENEW.NES;
using SCIENEW.RecipaediaEX.Overlay;
using SCIENEW.Tech;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class IndustrialModLoader : ModLoader
{
	private readonly IndustrialEraLanAdapter m_lanAdapter = new();
	private static Texture2D m_ieTexture;

	private static BitmapFont m_pixelFont;

	private static Texture2D m_pureWhiteTexture;

	private static Project? m_project;

	private static AmpkContentStore? m_ampkStore;

	public static ModEntity ModEntity { get; set; }

	public static Texture2D BaseTexture => BlocksTexturesManager.DefaultBlocksTexture ?? ContentManager.Get<Texture2D>("Textures/Blocks");

	public static Texture2D IETexture
	{
		get
		{
			if (m_ieTexture == null)
			{
				m_ieTexture = ContentManager.Get<Texture2D>("IndustrialMod");
			}
			return m_ieTexture;
		}
	}

	public static Texture2D PureWhiteTexture
	{
		get
		{
			if (m_pureWhiteTexture == null)
			{
				Image image = new Image(1, 1);
				image.SetPixel(0, 0, Color.White);
				m_pureWhiteTexture = Texture2D.Load(image);
			}
			return m_pureWhiteTexture;
		}
	}

	public static BitmapFont PixelFont => m_pixelFont;

	public static string ModPackageName => "com.IndustrialEra2";

	public static bool IsAprilFools
	{
		get
		{
			if (DateTime.Now.Month == 4)
			{
				return DateTime.Now.Day == 1;
			}
			return false;
		}
	}

	public override void __ModInitialize()
	{
		ModEntity = Entity;
		CompatNetAdapterRegistry.Register(m_lanAdapter);
		ModsManager.RegisterHook("OnPlayerControlBoat", this);
		ModsManager.RegisterHook("OnIUpdateableUpdate", this);
		ModsManager.RegisterHook("OnProjectileAdded", this);
		ModsManager.RegisterHook("OnNetworkProjectileFired", this);
		if (AmpkRuntimeConfig.IsEnabled)
		{
			if (AmpkContentStore.TryLoadFromMod(Entity, AmpkRuntimeConfig.MasterKey, out AmpkContentStore store))
			{
				m_ampkStore = store;
				AmpkContentRegistrar.RegisterAll(m_ampkStore.Index, m_ampkStore, Entity);
			}
			else
			{
				Log.Warning("[IE2] AMPK enabled but assets.index missing; using plaintext Assets/ if present.");
			}
		}
		ModsManager.RegisterHook("OnLoadingStart", this);
		ModsManager.RegisterHook("OnLoadingFinished", this);
		ModsManager.RegisterHook("SubsystemUpdate", this);
		ModsManager.RegisterHook("OnMinerDig", this);
		ModsManager.RegisterHook("OnModalPanelWidgetSet", this);
		ModsManager.RegisterHook("GetMaxInstancesCount", this);
		ModsManager.RegisterHook("ToFreeChunks", this);
		ModsManager.RegisterHook("ToAllocateChunks", this);
		ModsManager.RegisterHook("GenerateChunkVertices", this);
		ModsManager.RegisterHook("OnProjectLoaded", this);
		ModsManager.RegisterHook("OnProjectDisposed", this);
		ModsManager.RegisterHook("OnTerrainContentsGenerated", this);
		ModsManager.RegisterHook("GenerateChunkLightSources", this);
		ModsManager.RegisterHook("GuiUpdate", this);
		ModsManager.RegisterHook("OnPlayerInputHit", this);
		ModsManager.RegisterHook("CraftingRecipesManagerInitialized", this);
		ModsManager.RegisterHook("GetKeyboardMappings", this);
		ModsManager.RegisterHook("TerrainContentsGenerator24Initialize", this);
		ModsManager.RegisterHook("EditBlockDescriptionScreen", this);
		ModsManager.RegisterHook("OnMainMenuScreenCreated", this);
		ModsManager.RegisterHook("AfterWidgetUpdate", this);
		ModsManager.RegisterHook("OnSwitchScreen", this);
		IndustrialPlacableRecipeAdapters.Register();
		IndustrialRecipePlacement.InstallFeedbackHook();
		CraftingOverlayIngredientBridge.BlockValueToCraftingId = RecipesUtils.BlockToCratingID;
		CraftingOverlayIngredientBridge.ExpandIngredientBlockValues = RecipesUtils.ExpandIngredientToBlockValues;
		CraftingOverlayIngredientBridge.DecodeIngredientResult = SafeDecodeIEResult;
		GetFile("SCIENEW.dll", IEAssemblyManager.Init);
	}

	public override void GetModFile(string filename, Action<Stream> stream, out bool skip, out bool fileFound)
	{
		skip = false;
		fileFound = false;
		if (m_ampkStore != null && filename.StartsWith("Assets/", StringComparison.Ordinal))
		{
			string virtualPath = filename.Substring(7);
			if (m_ampkStore.TryOpenPlaintext(virtualPath, out MemoryStream stream2))
			{
				stream(stream2);
				skip = true;
				fileFound = true;
				return;
			}
		}
		base.GetModFile(filename, stream, out skip, out fileFound);
	}

	public override void GetModFiles(string extension, Action<string, Stream> action, out bool skip)
	{
		skip = false;
		if (m_ampkStore != null)
		{
			bool flag = false;
			foreach (AmpkAssetEntry entry in m_ampkStore.Index.Entries)
			{
				string path = "Assets/" + entry.VirtualPath;
				if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
					GetModFile(path, (Stream s) =>
					{
						action(path, s);
					}, out var _, out var _);
				}
			}
			if (flag)
			{
				skip = true;
				return;
			}
		}
		base.GetModFiles(extension, action, out skip);
	}

	public override void OnLoadingStart(List<Action> actions)
	{
		SolarPanelTextureManager.Init();
		LEDScreenTextureManager.Init();
		WireDrawingManager.Init();
		ItemsBlockManager.Init();
		NormalBlockManager.Init();
		DevicesBlockManager.Init();
		DamageableItemManager.Init();
		FluidsManager.Init();
		IESettingsManager.Init();
		MaterialBlocksController.Init();
		RecipaediaEventBus.GetPublisher<IndustrialModInitializedEvent>().Publish(new IndustrialModInitializedEvent(DevicesBlockManager.Devices.Length));
	}

	public override void OnLoadingFinished(List<Action> actions)
	{
		base.OnLoadingFinished(actions);
		m_ieTexture = ContentManager.Get<Texture2D>("IndustrialMod");
		m_pixelFont = ContentManager.Get<BitmapFont>("Fonts/Pixel");
		if (BlocksManager.m_categories.Contains("Devices"))
		{
			BlocksManager.m_categories.Remove("Devices");
		}
		BlocksManager.m_categories.Insert(5, "Devices");
		HelpsManager.Init();
		NESManager.Init();
		IESettingsManager.LateInit();
		CrafterHints.IsEnabled = () => IESettingsManager.Get<bool>("CraftingMessage");
		RecipaediaEXManager.EnableLevelRestrictions = false;
		ScreensManager.m_screens["FluidRecipaediaDescription"] = new FluidRecipaediaDescriptionScreen();
		RecipaediaEXScreen.RegisterChildScreen("FluidRecipaediaDescription");
	}

	public void InitCraftingModule()
	{
		TechTreeManager.Init();
	}

	public override void OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float DigProgress, out bool Digged)
	{
		base.OnMinerDig(miner, raycastResult, ref DigProgress, out Digged);
		if (Terrain.ExtractContents(miner.ActiveBlockValue) == BaseDamageableItemBlock.Index)
		{
			miner.Project.FindSubsystem<SubsystemDamageableItemBlockBehavior>(throwOnError: true).OnMinerDig(miner, raycastResult, ref DigProgress, out Digged);
		}
	}

	public override void OnModalPanelWidgetSet(ComponentGui gui, Widget Old, Widget New)
	{
		base.OnModalPanelWidgetSet(gui, Old, New);
		if (Old != New)
		{
			RecipaediaCraftingOverlayController.DismissForModalWidget(Old);
		}
		if (Old is IRemovedActionWidget removedActionWidget && (New == null || New != Old))
		{
			removedActionWidget.OnRemoved();
		}
	}

	public override void ToFreeChunks(TerrainUpdater terrainUpdater, TerrainChunk chunk, out bool KeepWorking)
	{
		KeepWorking = terrainUpdater.m_subsystemTerrain.Project.FindSubsystem<Subsystem996Chunks>().m_terrainChunks996.ContainsKey(chunk.Coords);
	}

	public override bool ToAllocateChunks(TerrainUpdater terrainUpdater, TerrainUpdater.UpdateLocation[] locations)
	{
		bool result = false;
		foreach (KeyValuePair<Point2, bool> item in terrainUpdater.m_subsystemTerrain.Project.FindSubsystem<Subsystem996Chunks>().m_terrainChunks996)
		{
			Point2 key = item.Key;
			TerrainChunk terrainChunk = terrainUpdater.m_terrain.GetChunkAtCoords(key.X, key.Y);
			if (terrainChunk == null)
			{
				result = true;
				terrainChunk = terrainUpdater.m_terrain.AllocateChunk(key.X, key.Y);
			}
			do
			{
				terrainUpdater.UpdateChunkSingleStep(terrainChunk, terrainUpdater.m_subsystemSky.SkyLightValue);
			}
			while (terrainChunk.ThreadState < TerrainChunkState.InvalidVertices1);
			if (!terrainChunk.AreBehaviorsNotified)
			{
				terrainChunk.AreBehaviorsNotified = true;
				terrainUpdater.NotifyBlockBehaviors(terrainChunk);
			}
		}
		return result;
	}

	public override void GuiUpdate(ComponentGui componentGui)
	{
		base.GuiUpdate(componentGui);
		ComponentRider componentRider = componentGui.m_componentPlayer.ComponentRider;
		if ((componentGui.m_componentPlayer.ComponentInput.PlayerInput.ToggleInventory || componentGui.m_inventoryButtonWidget.IsClicked) && componentRider.Mount != null)
		{
			Widget widget = componentGui.Entity.FindComponent<ComponentEntityOpener>().OpenEntity(componentGui.m_componentPlayer.ComponentMiner.Inventory, componentGui.m_componentPlayer.ComponentRider.Mount.Entity);
			if (widget != null)
			{
				componentGui.ModalPanelWidget = widget;
				AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
			}
		}
	}

	public override void AfterWidgetUpdate(Widget widget)
	{
		if (widget is Screen screen && screen == ScreensManager.CurrentScreen)
		{
			UiPhysicsEasterEgg.TickAfterScreenUpdate(screen);
		}
	}

	public override void OnSwitchScreen(ref Screen screen, object[] parameters)
	{
		UiPhysicsEasterEgg.OnSwitchScreen();
	}

	public override int GetMaxInstancesCount()
	{
		if (!IESettingsManager.Get<bool>("LowQuality"))
		{
			return 64;
		}
		return base.GetMaxInstancesCount();
	}

	public override void GenerateChunkVertices(TerrainChunk chunk, bool even)
	{
		if (m_project != null)
		{
			m_project.FindSubsystem<SubsystemSolarPanelGroups>(throwOnError: true).UpdateLightInChunk(chunk);
		}
	}

	public override void OnTerrainContentsGenerated(TerrainChunk chunk)
	{
	}

	public override void GenerateChunkLightSources(DynamicArray<TerrainUpdater.LightSource> lightSources, TerrainChunk chunk)
	{
	}

	public override void OnPlayerControlBoat(ComponentPlayer player, bool skippedByOtherMods, out bool skipVanilla)
	{
		skipVanilla = false;
		if (!skippedByOtherMods)
		{
			m_lanAdapter.QueueVehicleInput(player);
		}
	}

	public override void OnIUpdateableUpdate(SubsystemUpdate subsystem, IUpdateable updateable, float dt, out bool skip)
	{
		skip = false;
		if (updateable is ComponentBoat boat)
		{
			m_lanAdapter.ApplyVehicleInput(boat);
		}
	}

	public override void OnProjectileAdded(SubsystemProjectiles subsystem, ref Projectile projectile, ValuesDictionary saved)
	{
		if (ReferenceEquals(subsystem.Project, GameManager.Project))
		{
			m_lanAdapter.Projectiles.Track(projectile, fired: false);
		}
	}

	public override void OnNetworkProjectileFired(Projectile projectile, bool isFireProjectile)
	{
		if (projectile.UseModNetworkMotion)
		{
			m_lanAdapter.PublishProjectile(projectile, isFireProjectile);
		}
	}

	public override void OnProjectLoaded(Project project)
	{
		m_project = project;
		IEBenchmarkCollector.Subscribe();
	}

	public override void OnProjectDisposed()
	{
		IEBenchmarkCollector.Unsubscribe();
		m_project = null;
		base.OnProjectDisposed();
	}

	public override void TerrainContentsGenerator24Initialize(ITerrainContentsGenerator terrainContentsGenerator, SubsystemTerrain subsystemTerrain)
	{
		subsystemTerrain.Project.FindSubsystem<SubsystemTerrainElementsIE>(throwOnError: true).TerrainContentsGenerator24Initialize(terrainContentsGenerator, subsystemTerrain);
	}

	public override void OnPlayerInputHit(ComponentPlayer componentPlayer, ref bool playerOperated, ref double timeIntervalHit, ref float meleeAttackRange, bool skippedByOtherMods, out bool skipVanilla)
	{
		skipVanilla = false;
		PlayerInput playerInput = componentPlayer.ComponentInput.PlayerInput;
		BodyRaycastResult? bodyRaycastResult = componentPlayer.ComponentMiner.Raycast<BodyRaycastResult>(playerInput.Hit.Value, RaycastMode.Interaction);
		if (bodyRaycastResult.HasValue && componentPlayer.ComponentMiner.ActiveBlockValue == 0)
		{
			Widget widget = componentPlayer.Entity.FindComponent<ComponentEntityOpener>().OpenEntity(componentPlayer.ComponentMiner.Inventory, bodyRaycastResult.Value.ComponentBody.Entity);
			if (widget != null)
			{
				componentPlayer.ComponentGui.ModalPanelWidget = widget;
				AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
				skipVanilla = true;
			}
		}
	}

	public override void CraftingRecipesManagerInitialized()
	{
		base.CraftingRecipesManagerInitialized();
		InitCraftingModule();
	}

	public override IEnumerable<KeyValuePair<string, object>> GetKeyboardMappings()
	{
		return base.GetKeyboardMappings().Concat(TechTreeManager.InitKeySettings());
	}

	public override void EditBlockDescriptionScreen(Dictionary<string, string> properties, int blockValue)
	{
		try
		{
			if (properties != null && !properties.ContainsKey("UL") && !properties.ContainsKey("PL") && DevicesBlockManager.GetDevice(blockValue) is IVoltDevice voltDevice)
			{
				float num = MathF.Abs(voltDevice.GetStandardUL(blockValue));
				float num2 = MathF.Abs(voltDevice.GetStandardPL(blockValue));
				properties["UL"] = num.ToString("0.###", CultureInfo.InvariantCulture);
				properties["PL"] = num2.ToString("0.###", CultureInfo.InvariantCulture);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[IE2] EditBlockDescriptionScreen failed: " + ex);
		}
	}

	public static void GetFile(string filename, Action<Stream> streamAction)
	{
		ModsManager.ModList.First((ModEntity entity) => entity.modInfo.PackageName == ModPackageName).GetFile(filename, streamAction);
	}

	public static ModInfo GetCurrentModInfo()
	{
		return ModsManager.ModList.First((ModEntity entity) => entity.modInfo.PackageName == ModPackageName).modInfo;
	}

	public override void OnMainMenuScreenCreated(MainMenuScreen mainMenuScreen, StackPanelWidget leftBottomBar, StackPanelWidget rightBottomBar)
	{
		if (rightBottomBar != null && rightBottomBar.Children.Find<Widget>("IEModSettingsButton", throwIfNotFound: false) == null)
		{
			IESettingsButtonWidget iESettingsButtonWidget = new IESettingsButtonWidget
			{
				Name = "IEModSettingsButton",
				Size = new Vector2(60f, 60f)
			};
			RectangleWidget widget = new RectangleWidget
			{
				Size = new Vector2(30f, 30f),
				TextureLinearFilter = false,
				HorizontalAlignment = WidgetAlignment.Center,
				VerticalAlignment = WidgetAlignment.Center,
				Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/IEIcon"),
				OutlineColor = new Color(0, 0, 0, 0),
				FillColor = new Color(255, 255, 255),
				IsVisible = true,
				TextureAnisotropicFilter = false,
				BlendState = BlendState.NonPremultiplied
			};
			iESettingsButtonWidget.AddChildren(widget);
			rightBottomBar.Children.Add(iESettingsButtonWidget);
		}
	}

	private static int SafeDecodeIEResult(string ingredient)
	{
		return RecipesUtils.DecodeIEResult(ingredient);
	}
}

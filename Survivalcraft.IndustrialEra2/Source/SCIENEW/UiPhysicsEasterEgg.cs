using System;
using System.Collections.Generic;
using Box2D.NET;
using Engine;
using Engine.Input;
using Game;
using RecipaediaEX.UI;
using SCIENEW.Widgets.Recipes.Slots;

namespace SCIENEW;

public static class UiPhysicsEasterEgg
{
	private struct Binding
	{
		public Widget Widget;

		public B2BodyId BodyId;

		public Vector2 LayoutCenterInParent;

		public Vector2 LocalCenter;

		public Widget Parent;
	}

	private const float PixelsPerMeter = 48f;

	private const float FixedDt = 1f / 60f;

	private const float SizeMaxShortFrac = 0.35f;

	private const float SizeMaxLongFrac = 0.55f;

	private const float MinEdgePx = 8f;

	private const int MaxBodies = 200;

	private const float WallHalf = 1.25f;

	private static readonly List<Binding> s_bindings = new List<Binding>();

	private static readonly List<(Widget Widget, bool Clamp)> s_clampRestore = new List<(Widget, bool)>();

	private static B2WorldId s_worldId = B2Ids.b2_nullWorldId;

	private static Screen s_activeScreen;

	private static float s_accumulator;

	private static bool s_chordHeld;

	private static Vector2 s_lastScreenSize;

	public static bool IsActive
	{
		get
		{
			if (B2Ids.B2_IS_NON_NULL(s_worldId))
			{
				return s_activeScreen != null;
			}
			return false;
		}
	}

	public static void TickAfterScreenUpdate(Screen screen)
	{
		if (screen == null)
		{
			return;
		}
		UpdateChord(screen);
		if (s_activeScreen != null && s_activeScreen != screen)
		{
			Disable();
		}
		if (!IsActive || s_activeScreen != screen)
		{
			return;
		}
		Vector2 actualSize = screen.ActualSize;
		if (actualSize.X < 32f || actualSize.Y < 32f)
		{
			return;
		}
		if (s_lastScreenSize != actualSize)
		{
			Enable(screen);
			return;
		}
		float num = Math.Clamp(Time.FrameDuration, 0f, 0.05f);
		s_accumulator += num;
		while (s_accumulator >= 1f / 60f)
		{
			B2Worlds.b2World_Step(s_worldId, 1f / 60f, 8);
			s_accumulator -= 1f / 60f;
		}
		ApplyTransforms();
	}

	public static void OnSwitchScreen()
	{
		Disable();
	}

	private static void UpdateChord(Screen screen)
	{
		int num;
		if (Keyboard.IsKeyDown(Key.I))
		{
			num = (Keyboard.IsKeyDown(Key.E) ? 1 : 0);
			if (num != 0 && !s_chordHeld)
			{
				if (IsActive && s_activeScreen == screen)
				{
					Disable();
				}
				else
				{
					Disable();
					Enable(screen);
				}
			}
		}
		else
		{
			num = 0;
		}
		s_chordHeld = (byte)num != 0;
	}

	private static void Enable(Screen screen)
	{
		Disable();
		s_activeScreen = screen;
		s_lastScreenSize = screen.ActualSize;
		s_accumulator = 0f;
		B2WorldDef def = B2Types.b2DefaultWorldDef();
		def.gravity = new B2Vec2(0f, -12f);
		s_worldId = B2Worlds.b2CreateWorld(in def);
		float num = screen.ActualSize.X / 48f;
		float num2 = screen.ActualSize.Y / 48f;
		float num3 = 1.25f;
		CreateStaticBox(num * 0.5f, 0f - num3, num * 0.5f + num3, num3);
		CreateStaticBox(num * 0.5f, num2 + num3, num * 0.5f + num3, num3);
		CreateStaticBox(0f - num3, num2 * 0.5f, num3, num2 * 0.5f + num3);
		CreateStaticBox(num + num3, num2 * 0.5f, num3, num2 * 0.5f + num3);
		List<Widget> list = new List<Widget>();
		CollectCandidates(screen, screen.ActualSize, list);
		int num4 = Math.Min(list.Count, 200);
		for (int i = 0; i < num4; i++)
		{
			TryBind(list[i]);
		}
		Log.Information($"[IE UiPhysics] On ({num4} bodies) {screen.GetType().Name}");
	}

	public static void Disable()
	{
		foreach (Binding s_binding in s_bindings)
		{
			if (s_binding.Widget != null)
			{
				s_binding.Widget.RenderTransform = Matrix.Identity;
			}
		}
		foreach (var (widget, clampToBounds) in s_clampRestore)
		{
			if (widget != null)
			{
				widget.ClampToBounds = clampToBounds;
			}
		}
		s_bindings.Clear();
		s_clampRestore.Clear();
		if (B2Ids.B2_IS_NON_NULL(s_worldId))
		{
			B2Worlds.b2DestroyWorld(s_worldId);
			s_worldId = B2Ids.b2_nullWorldId;
		}
		s_activeScreen = null;
		s_accumulator = 0f;
	}

	private static void CollectCandidates(Widget widget, Vector2 screenSize, List<Widget> dst)
	{
		if (widget == null || !widget.IsVisible)
		{
			return;
		}
		if (!IsLayoutContainerExcluded(widget) && PassesWhitelist(widget) && (IsLabelLike(widget) || PassesSize(widget, screenSize)) && !HasCompositeAncestor(widget))
		{
			dst.Add(widget);
		}
		if (!(widget is ContainerWidget containerWidget))
		{
			return;
		}
		foreach (Widget child in containerWidget.Children)
		{
			CollectCandidates(child, screenSize, dst);
		}
	}

	private static bool IsLayoutContainerExcluded(Widget w)
	{
		if (!(w is Screen) && !(w is ScrollPanelWidget) && !(w is StackPanelWidget) && !(w is GridPanelWidget) && !(w is ListPanelWidget) && !(w is UniformSpacingPanelWidget) && !(w is IndustryBackgroundWidget) && !(w is PanoramaWidget) && !(w is CreditsSandboxWidget))
		{
			return w.GetType() == typeof(CanvasWidget);
		}
		return true;
	}

	private static bool PassesWhitelist(Widget w)
	{
		if (w is CreditsSandboxWidget || w is IndustryBackgroundWidget || w is PanoramaWidget)
		{
			return false;
		}
		if (w.GetType() == typeof(CanvasWidget))
		{
			return false;
		}
		if (w is ButtonWidget)
		{
			return true;
		}
		if (w is CheckboxWidget || w is SliderWidget || w is LinkWidget || w is TextBoxWidget)
		{
			return true;
		}
		if (w is ClickableWidget && !(w is ButtonWidget))
		{
			return true;
		}
		if (w is LabelWidget)
		{
			return true;
		}
		if (w is InventorySlotWidget || w is FluidRecipeSlotWidget || w is IEIconSlotWidget || w is RecipeSlotWidget)
		{
			return true;
		}
		if (w is ValueBarWidget || w is ArrowLineWidget || w is FireWidget || w is ChargeWidget || w is FractionalWidget || w is FluidWidget)
		{
			return true;
		}
		if (w is TechTreeNodeInfoWidget || w is TechTreeBlockIconWidget)
		{
			return true;
		}
		if (w is VoltmeterWidget || w is AmmeterWidget)
		{
			return true;
		}
		if (w is BetterRectangleWidget || w is RoundRectangleWidget)
		{
			return true;
		}
		if (w is RectangleWidget || w is BevelledRectangleWidget || w is LegacyBevelledRectangleWidget)
		{
			return true;
		}
		switch (w.GetType().Name)
		{
		case "NuclearFireWidget":
		case "CustomFontLabelWidget":
		case "LegacyBevelledButtonWidget":
			return true;
		default:
			return false;
		}
	}

	private static bool IsLabelLike(Widget w)
	{
		if (!(w is LabelWidget))
		{
			return w is FontTextWidget;
		}
		return true;
	}

	private static bool PassesSize(Widget w, Vector2 screenSize)
	{
		if (IsLabelLike(w))
		{
			return true;
		}
		Vector2 actualSize = w.ActualSize;
		if (actualSize.X < 8f || actualSize.Y < 8f)
		{
			return false;
		}
		float num = Math.Min(actualSize.X, actualSize.Y);
		float num2 = Math.Max(actualSize.X, actualSize.Y);
		float num3 = 0.35f * Math.Min(screenSize.X, screenSize.Y);
		float num4 = 0.55f * Math.Max(screenSize.X, screenSize.Y);
		if (num > num3 || num2 > num4)
		{
			return false;
		}
		return actualSize.X * actualSize.Y <= 0.18f * screenSize.X * screenSize.Y;
	}

	private static bool HasCompositeAncestor(Widget w)
	{
		for (Widget parentWidget = w.ParentWidget; parentWidget != null; parentWidget = parentWidget.ParentWidget)
		{
			if (parentWidget is ButtonWidget || parentWidget is CheckboxWidget || parentWidget is SliderWidget || parentWidget is InventorySlotWidget || parentWidget is RecipeSlotWidget || parentWidget is FluidRecipeSlotWidget || parentWidget is IEIconSlotWidget || parentWidget is LinkWidget || parentWidget is TextBoxWidget)
			{
				return true;
			}
		}
		return false;
	}

	private static void TryBind(Widget widget)
	{
		Vector2 actualSize = widget.ActualSize;
		if (IsLabelLike(widget) || (!(actualSize.X < 8f) && !(actualSize.Y < 8f)))
		{
			if (IsLabelLike(widget))
			{
				actualSize.X = Math.Max(actualSize.X, 8f);
				actualSize.Y = Math.Max(actualSize.Y, 8f);
			}
			Widget parentWidget = widget.ParentWidget;
			if (parentWidget != null)
			{
				StashClampPath(widget);
				Vector2 vector = widget.ActualSize * 0.5f;
				Vector2 localInScreen = s_activeScreen.ScreenToWidget(widget.WidgetToScreen(vector));
				Vector2 layoutCenterInParent = parentWidget.ScreenToWidget(widget.WidgetToScreen(vector));
				float halfWidth = Math.Max(actualSize.X * 0.5f / 48f, 0.05f);
				float halfHeight = Math.Max(actualSize.Y * 0.5f / 48f, 0.05f);
				B2Vec2 position = LocalToPhysics(localInScreen, s_activeScreen.ActualSize);
				B2BodyDef def = B2Types.b2DefaultBodyDef();
				def.type = B2BodyType.b2_dynamicBody;
				def.position = position;
				def.rotation = B2MathFunction.b2MakeRot(0f);
				B2BodyId bodyId = B2Bodies.b2CreateBody(s_worldId, in def);
				B2Polygon polygon = B2Geometries.b2MakeBox(halfWidth, halfHeight);
				B2ShapeDef def2 = B2Types.b2DefaultShapeDef();
				def2.density = 1f;
				def2.material.friction = 0.4f;
				def2.material.restitution = 0.12f;
				B2Shapes.b2CreatePolygonShape(bodyId, in def2, in polygon);
				s_bindings.Add(new Binding
				{
					Widget = widget,
					BodyId = bodyId,
					LayoutCenterInParent = layoutCenterInParent,
					LocalCenter = vector,
					Parent = parentWidget
				});
			}
		}
	}

	private static void StashClampPath(Widget widget)
	{
		Widget widget2 = widget;
		while (widget2 != null)
		{
			bool flag = false;
			for (int i = 0; i < s_clampRestore.Count; i++)
			{
				if (s_clampRestore[i].Widget == widget2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				s_clampRestore.Add((widget2, widget2.ClampToBounds));
				widget2.ClampToBounds = false;
			}
			if (!(widget2 is Screen))
			{
				widget2 = widget2.ParentWidget;
				continue;
			}
			break;
		}
	}

	private static void ApplyTransforms()
	{
		for (int num = s_bindings.Count - 1; num >= 0; num--)
		{
			Binding binding = s_bindings[num];
			if (binding.Widget == null || binding.Parent == null || !B2Worlds.b2Body_IsValid(binding.BodyId))
			{
				s_bindings.RemoveAt(num);
			}
			else
			{
				B2Transform b2Transform = B2Bodies.b2Body_GetTransform(binding.BodyId);
				Vector2 p = PhysicsToLocal(b2Transform.p, s_activeScreen.ActualSize);
				Vector2 p2 = s_activeScreen.WidgetToScreen(p);
				Vector2 vector = binding.Parent.ScreenToWidget(p2) - binding.LayoutCenterInParent;
				float radians = 0f - B2MathFunction.b2Rot_GetAngle(in b2Transform.q);
				Vector2 localCenter = binding.LocalCenter;
				binding.Widget.RenderTransform = Matrix.CreateTranslation(0f - localCenter.X, 0f - localCenter.Y, 0f) * Matrix.CreateRotationZ(radians) * Matrix.CreateTranslation(localCenter.X + vector.X, localCenter.Y + vector.Y, 0f);
			}
		}
	}

	private static void CreateStaticBox(float cx, float cy, float hx, float hy)
	{
		B2BodyDef def = B2Types.b2DefaultBodyDef();
		def.type = B2BodyType.b2_staticBody;
		def.position = new B2Vec2(cx, cy);
		B2BodyId bodyId = B2Bodies.b2CreateBody(s_worldId, in def);
		B2Polygon polygon = B2Geometries.b2MakeBox(hx, hy);
		B2Shapes.b2CreatePolygonShape(bodyId, B2Types.b2DefaultShapeDef(), in polygon);
	}

	private static B2Vec2 LocalToPhysics(Vector2 localInScreen, Vector2 screenSize)
	{
		return new B2Vec2(localInScreen.X / 48f, (screenSize.Y - localInScreen.Y) / 48f);
	}

	private static Vector2 PhysicsToLocal(B2Vec2 physics, Vector2 screenSize)
	{
		return new Vector2(physics.X * 48f, screenSize.Y - physics.Y * 48f);
	}
}

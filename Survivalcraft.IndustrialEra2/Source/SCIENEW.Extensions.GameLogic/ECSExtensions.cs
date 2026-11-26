using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW.Extensions.GameLogic;

public static class ECSExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? GetComponentAtPoint<T>(this SubsystemTerrain terrain, Point3 position, bool throwOnEntityNotFound = false, bool throwOnError = true) where T : class
	{
		if (BlockEntityUtils.GetBlockEntity(terrain, position, out ComponentBlockEntity blockEntity))
		{
			return blockEntity.Entity.FindComponent<T>(throwOnError);
		}
		if (!throwOnEntityNotFound)
		{
			return null;
		}
		throw new KeyNotFoundException($"Block entity with position {position} does not exist");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Lazy<T?> GetComponentAtPointLazy<T>(this SubsystemTerrain terrain, Point3 position, bool throwOnEntityNotFound = false, bool throwOnError = false) where T : class
	{
		return new Lazy<T>(() => terrain.GetComponentAtPoint<T>(position, throwOnEntityNotFound, throwOnError));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetComponent<T>(this Component component, bool throwOnError = true) where T : class
	{
		return component.Entity.FindComponent<T>(throwOnError);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetComponent<T>(this Component component, out T target) where T : class
	{
		target = component.Entity.FindComponent<T>(throwOnError: false);
		return target != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetSubsystem<T>(this Component component, bool throwOnError = true) where T : class
	{
		return component.Project.FindSubsystem<T>(throwOnError);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSubsystem<T>(this Component component, out T target) where T : class
	{
		target = component.Project.FindSubsystem<T>(throwOnError: false);
		return target != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetSubsystem<T>(this Subsystem subsystem, bool throwOnError = true) where T : class
	{
		return subsystem.Project.FindSubsystem<T>(throwOnError);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSubsystem<T>(this Subsystem subsystem, out T target) where T : class
	{
		target = subsystem.Project.FindSubsystem<T>(throwOnError: false);
		return target != null;
	}
}

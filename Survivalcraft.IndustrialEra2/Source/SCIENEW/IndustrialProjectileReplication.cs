using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Game;
using Game.Network;
using GameEntitySystem;

namespace SCIENEW;

internal sealed class IndustrialProjectileReplication {
    private const int BatchSize = 96;
    private readonly Dictionary<Projectile, (int Id, bool Fired)> m_host = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<int, Projectile> m_guest = new();
    private readonly List<(Projectile Projectile, int Id, bool Fired, bool Alive)> m_updates = new();
    private Project m_hostProject;
    private Project m_guestProject;
    private double m_nextPublish;
    private int m_nextId;

    public void Track(Projectile projectile, bool fired) {
        if (!NetworkManager.IsServerRunning || GameManager.Project is not Project project) {
            return;
        }
        ResetHost(project);
        projectile.UseModNetworkMotion = true;
        if (m_host.TryGetValue(projectile, out (int Id, bool Fired) previous)) {
            m_host[projectile] = (previous.Id, previous.Fired || fired);
        }
        else {
            m_host.Add(projectile, (++m_nextId, fired));
        }
    }

    public void PublishSpawn(Projectile projectile, bool fired, Action<byte[]> send) {
        Track(projectile, fired);
        if (m_host.TryGetValue(projectile, out var entry) && !projectile.ToRemove) {
            m_updates.Add((projectile, entry.Id, entry.Fired, true));
            Flush(send);
        }
    }

    public void Publish(Project project, Action<byte[]> send) {
        ResetHost(project);
        if (Time.RealTime < m_nextPublish || project.FindSubsystem<SubsystemProjectiles>() is not SubsystemProjectiles subsystem) {
            return;
        }
        m_nextPublish = Time.RealTime + 0.08;

        HashSet<Projectile> live = new(ReferenceEqualityComparer.Instance);
        foreach (Projectile projectile in subsystem.Projectiles) {
            if (!projectile.ToRemove) {
                live.Add(projectile);
                if (!m_host.ContainsKey(projectile)) {
                    Track(projectile, fired: false);
                }
            }
        }
        foreach ((Projectile projectile, (int id, bool fired)) in m_host) {
            m_updates.Add((projectile, id, fired, live.Contains(projectile)));
            if (m_updates.Count == BatchSize) {
                Flush(send);
            }
        }
        Flush(send);
        foreach (Projectile projectile in m_host.Keys) {
            if (!live.Contains(projectile)) {
                m_host.Remove(projectile);
            }
        }
    }

    private void ResetHost(Project project) {
        if (!ReferenceEquals(project, m_hostProject)) {
            m_host.Clear();
            m_updates.Clear();
            m_hostProject = project;
            m_nextId = 0;
            m_nextPublish = 0;
        }
    }

    private void Flush(Action<byte[]> send) {
        if (m_updates.Count == 0) {
            return;
        }
        byte[] packet = CompatNetPayload.Write(archive => {
            byte count = (byte)m_updates.Count;
            archive.Value(ref count);
            foreach ((Projectile projectile, int id, bool fired, bool alive) in m_updates) {
                int key = id;
                byte flags = (byte)((alive ? 1 : 0) | (fired ? 2 : 0) | (projectile.IsInFluid ? 4 : 0));
                archive.Value(ref key);
                archive.Value(ref flags);
                if (alive) {
                    int owner = projectile.OwnerEntity?.Id ?? 0;
                    archive.Value(ref owner);
                    archive.Value(ref projectile.Value);
                    archive.Value(ref projectile.Position);
                    archive.Value(ref projectile.Velocity);
                    archive.Value(ref projectile.AngularVelocity);
                    archive.Value(ref projectile.Rotation);
                    archive.Value(ref projectile.TrailOffset);
                }
            }
        });
        send(packet);
        m_updates.Clear();
    }

    public void Receive(Project project, byte[] packet) {
        if (!ReferenceEquals(project, m_guestProject)) {
            m_guest.Clear();
            m_guestProject = project;
        }
        if (project.FindSubsystem<SubsystemProjectiles>() is not SubsystemProjectiles subsystem) {
            return;
        }
        CompatNetPayload.Read(packet, archive => {
            byte count = 0;
            archive.Value(ref count);
            if (count > BatchSize) {
                throw new InvalidDataException("Invalid industrial projectile batch size.");
            }
            for (int i = 0; i < count; i++) {
                ReadProjectile(archive, subsystem);
            }
        });
    }

    private void ReadProjectile(PacketSerializer archive, SubsystemProjectiles subsystem) {
        int id = 0;
        byte flags = 0;
        archive.Value(ref id);
        archive.Value(ref flags);
        if ((flags & 1) == 0) {
            if (m_guest.Remove(id, out Projectile existing)) {
                existing.ToRemove = true;
            }
            return;
        }

        int value = 0;
        int owner = 0;
        Vector3 position = default;
        Vector3 velocity = default;
        Vector3 angular = default;
        Vector3 rotation = default;
        Vector3 trail = default;
        archive.Value(ref owner);
        archive.Value(ref value);
        archive.Value(ref position);
        archive.Value(ref velocity);
        archive.Value(ref angular);
        archive.Value(ref rotation);
        archive.Value(ref trail);
        if (!m_guest.TryGetValue(id, out Projectile projectile)) {
            ComponentCreature creature = owner == 0 ? null : subsystem.Project.FindEntity(owner)?.FindComponent<ComponentCreature>();
            projectile = subsystem.CreateProjectile(value, position, velocity, angular, creature);
            if (projectile == null) {
                return;
            }
            projectile.IsNetworkVisualReplica = true;
            if ((flags & 2) != 0) {
                subsystem.FireProjectileFast(projectile);
            }
            else {
                subsystem.AddProjectile(projectile);
            }
            m_guest.Add(id, projectile);
        }
        projectile.Position = position;
        projectile.Velocity = velocity;
        projectile.AngularVelocity = angular;
        projectile.Rotation = rotation;
        projectile.TrailOffset = trail;
        projectile.IsInFluid = (flags & 4) != 0;
    }
}

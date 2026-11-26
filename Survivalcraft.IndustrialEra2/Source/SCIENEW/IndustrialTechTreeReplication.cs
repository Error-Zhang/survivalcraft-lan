using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Game;
using Game.Network;
using SCIENEW.Tech;

namespace SCIENEW;

internal sealed class IndustrialTechTreeReplication {
    private const int MaximumTreeBytes = 4 * 1024 * 1024;
    private const int FragmentBytes = 48 * 1024;
    private TechTree m_encodedTree;
    private byte[] m_definition = [];
    private string m_lastUnlocks;
    private int m_sequence;
    private int m_receivedSequence = -1;
    private int m_receivedBytes;
    private byte[] m_incoming;
    private byte[] m_appliedDefinition = [];

    public void Publish(SubsystemTechTree subsystem, bool refresh, Action<byte[]> send) {
        TechTree tree = subsystem?.TechTree;
        if (tree == null) {
            return;
        }
        bool changedTree = !ReferenceEquals(tree, m_encodedTree);
        if (changedTree) {
            m_definition = Encoding.UTF8.GetBytes(TechTreeManager.EncodeTechTree(tree).ToString(SaveOptions.DisableFormatting));
            m_encodedTree = tree;
        }
        string unlocked = string.Join(";", tree.GetUnlockedNodes().Select(node => node.Name));
        if (!refresh && !changedTree && unlocked == m_lastUnlocks) {
            return;
        }
        m_lastUnlocks = unlocked;
        byte[] snapshot = CompatNetPayload.Write(archive => {
            byte[] definition = m_definition;
            archive.Buffer(ref definition, MaximumTreeBytes);
            archive.Value(ref unlocked);
        });
        if (snapshot.Length > MaximumTreeBytes) {
            Engine.Log.Warning("[IndustrialEra2] The technology tree exceeds the network snapshot limit.");
            return;
        }

        int sequence = ++m_sequence;
        int total = snapshot.Length;
        for (int position = 0; position < total; position += FragmentBytes) {
            int offset = position;
            byte[] fragment = snapshot.AsSpan(offset, Math.Min(FragmentBytes, total - offset)).ToArray();
            send(CompatNetPayload.Write(archive => {
                archive.Value(ref sequence);
                archive.Value(ref total);
                archive.Value(ref offset);
                archive.Buffer(ref fragment, FragmentBytes);
            }));
        }
    }

    public void Receive(SubsystemTechTree subsystem, byte[] payload) {
        CompatNetPayload.Read(payload, archive => {
            int sequence = 0;
            int total = 0;
            int offset = 0;
            byte[] fragment = null;
            archive.Value(ref sequence);
            archive.Value(ref total);
            archive.Value(ref offset);
            archive.Buffer(ref fragment, FragmentBytes);
            if (subsystem == null || sequence < m_receivedSequence || total is <= 0 or > MaximumTreeBytes
                || offset < 0 || fragment.Length == 0 || offset > total - fragment.Length) {
                return;
            }
            if (sequence != m_receivedSequence || m_incoming?.Length != total) {
                if (offset != 0) {
                    return;
                }
                m_receivedSequence = sequence;
                m_incoming = new byte[total];
                m_receivedBytes = 0;
            }
            if (offset != m_receivedBytes) {
                return;
            }
            fragment.CopyTo(m_incoming, offset);
            m_receivedBytes += fragment.Length;
            if (m_receivedBytes == total) {
                Apply(subsystem, m_incoming);
                m_incoming = null;
            }
        });
    }

    private void Apply(SubsystemTechTree subsystem, byte[] payload) {
        CompatNetPayload.Read(payload, archive => {
            byte[] definition = null;
            string unlocked = string.Empty;
            archive.Buffer(ref definition, MaximumTreeBytes);
            archive.Value(ref unlocked);
            TechTree tree = null;
            if (!definition.AsSpan().SequenceEqual(m_appliedDefinition)) {
                using MemoryStream stream = new(definition, writable: false);
                using XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                    MaxCharactersInDocument = MaximumTreeBytes
                });
                tree = TechTreeManager.DecodeTechTree(XElement.Load(reader));
                m_appliedDefinition = definition;
            }
            subsystem.ApplyLanProgress(tree, unlocked);
        });
    }
}

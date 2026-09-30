using System.Diagnostics;
using Engine;

namespace Game {
    public static partial class Program {
#if WINDOWS
        static Process m_debugMultiplayerHost;
        static EventWaitHandle m_debugMultiplayerHostReady;
        static bool m_debugMultiplayerHostReadySignaled;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

        public static bool IsDebugMultiplayerHost => StartupParameters.ContainsKey("debug-host");
        public static bool IsDebugMultiplayerClient => StartupParameters.ContainsKey("debug-multiplayer");
        public static bool DebugMultiplayerPlayerCreationStarted { get; private set; }

        public static bool BeginDebugMultiplayerPlayerCreation() {
            if (!IsDebugMultiplayerClient || DebugMultiplayerPlayerCreationStarted) {
                return false;
            }

            DebugMultiplayerPlayerCreationStarted = true;
            return true;
        }

        /// <summary>
        /// Starts the hidden host for the local multiplayer debugging mode.
        /// The current process remains the visible, debugger-attached client.
        /// </summary>
        public static void StartDebugMultiplayerHost(string worldDirectory, Action hostReady) {
#if WINDOWS
            if (!IsDebugMultiplayerClient || m_debugMultiplayerHost != null) {
                return;
            }

            try {
                string readyEventName = $"Survivalcraft.DebugMultiplayer.{Environment.ProcessId}.{Guid.NewGuid():N}";
                m_debugMultiplayerHostReady = new EventWaitHandle(false, EventResetMode.ManualReset, readyEventName);
                ProcessStartInfo startInfo = new() {
                    FileName = Process.GetCurrentProcess().MainModule!.FileName!,
                    UseShellExecute = false,
                    WorkingDirectory = Environment.CurrentDirectory
                };
                startInfo.ArgumentList.Add("--debug-host");
                startInfo.ArgumentList.Add("--debug-world");
                startInfo.ArgumentList.Add(worldDirectory);
                startInfo.ArgumentList.Add("--debug-ready");
                startInfo.ArgumentList.Add(readyEventName);
                m_debugMultiplayerHost = Process.Start(startInfo);
                if (m_debugMultiplayerHost == null) {
                    throw new InvalidOperationException("Failed to start the debug multiplayer host process.");
                }
                m_debugMultiplayerHost.EnableRaisingEvents = true;
                m_debugMultiplayerHost.Exited += (_, _) => {
                    Dispatcher.Dispatch(() => {
                        if (Window.IsCreated) {
                            Window.Close();
                        }
                    });
                };
                Log.Information($"Debug multiplayer host started, process id={m_debugMultiplayerHost.Id}.");

                EventWaitHandle readyEvent = m_debugMultiplayerHostReady;
                _ = Task.Run(() => {
                    bool isReady = false;
                    try {
                        isReady = readyEvent.WaitOne(TimeSpan.FromMinutes(2));
                    }
                    catch (Exception e) {
                        Log.Error($"Failed waiting for debug multiplayer host: {e}");
                    }
                    finally {
                        readyEvent.Dispose();
                        if (ReferenceEquals(m_debugMultiplayerHostReady, readyEvent)) {
                            m_debugMultiplayerHostReady = null;
                        }
                    }
                    Dispatcher.Dispatch(() => {
                        if (isReady && Window.IsCreated) {
                            hostReady?.Invoke();
                        }
                        else if (Window.IsCreated) {
                            Log.Error("Debug multiplayer host did not become ready.");
                            Window.Close();
                        }
                    });
                });
            }
            catch (Exception e) {
                Log.Error($"Failed to start debug multiplayer host: {e}");
                m_debugMultiplayerHost?.Dispose();
                m_debugMultiplayerHost = null;
                m_debugMultiplayerHostReady?.Dispose();
                m_debugMultiplayerHostReady = null;
                throw;
            }
#else
            throw new PlatformNotSupportedException("Debug multiplayer startup mode is only supported on Windows.");
#endif
        }

        public static void SignalDebugMultiplayerHostReady() {
#if WINDOWS
            if (!IsDebugMultiplayerHost
                || m_debugMultiplayerHostReadySignaled
                || !StartupParameters.TryGetValue("debug-ready", out string eventName)
                || string.IsNullOrWhiteSpace(eventName)) {
                return;
            }

            if (!OperatingSystem.IsWindows()) {
                return;
            }

            try {
                using EventWaitHandle readyEvent = EventWaitHandle.OpenExisting(eventName);
                readyEvent.Set();
                m_debugMultiplayerHostReadySignaled = true;
                Log.Information("Debug multiplayer host is ready.");
            }
            catch (Exception e) {
                Log.Error($"Failed to signal debug multiplayer host readiness: {e}");
            }
#endif
        }

        static void StopDebugMultiplayerHost() {
#if WINDOWS
            try {
                if (m_debugMultiplayerHost != null && !m_debugMultiplayerHost.HasExited) {
                    m_debugMultiplayerHost.Kill(entireProcessTree: true);
                }
            }
            catch (Exception e) {
                Log.Warning($"Failed to stop debug multiplayer host: {e.Message}");
            }
            finally {
                m_debugMultiplayerHost?.Dispose();
                m_debugMultiplayerHost = null;
                m_debugMultiplayerHostReady?.Dispose();
                m_debugMultiplayerHostReady = null;
            }
#endif
        }
    }
}

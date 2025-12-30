using System;

namespace BlackBoxControl.Services
{
    /// <summary>
    /// Manages a single persistent ESP32 simulator instance
    /// </summary>
    public static class ESP32SimulatorManager
    {
        private static ESP32Simulator _instance;
        private static readonly object _lock = new object();

        public static ESP32Simulator Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = ESP32Simulator.CreateVirtualPort("COM_SIMULATOR");
                        System.Diagnostics.Debug.WriteLine("========== SIMULATOR MANAGER CREATED ==========");
                        System.Diagnostics.Debug.WriteLine("[SimulatorManager] Created new ESP32 Simulator instance");
                    }
                    return _instance;
                }
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    _instance.Dispose();
                    _instance = null;
                    System.Diagnostics.Debug.WriteLine("[SimulatorManager] Reset ESP32 Simulator");
                }
            }
        }

        public static bool HasStoredData()
        {
            lock (_lock)
            {
                return _instance != null && _instance.HasData();
            }
        }
    }
}

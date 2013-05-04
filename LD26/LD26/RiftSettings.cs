using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RiftDotNet;

namespace LD26
{
    public static class RiftSettings
    {
        private static ISensorDevice sensor;
        public static ISensorFusion SensorFusion { get; set; }
        public static int HResolution { get; set; }

        public static int VResolution { get; set; }

        static RiftSettings()
        {
            HResolution = 1280;
            VResolution = 800;
            HScreenSize = 0.14976f;
            VScreenSize = 0.09356f;
            VScreenCenter = 0.0468f;
            EyeToScreenDistance = 0.041f; //0.05f; //5cm?
            LensSeparationDistance = 0.00635f;//0.00544f; // 5.44mm???
            InterpupillaryDistance = 0.064f; // 64mm?
            DistortionK = new float[] {1f, 0.22f, 0.24f, 0f}; // ??????????????/

            deviceManager = Factory.CreateDeviceManager();
            device = deviceManager.HMDDevices.FirstOrDefault();

            if (device != null)
            {
                sensor = device.CreateDevice().Sensor;
            }

            SensorFusion = Factory.CreateSensorFusion(sensor);

            ReloadRiftSettings();
        }

        private static void ReloadRiftSettings()
        {
            if (device == null)
            {
                device = deviceManager.HMDDevices.FirstOrDefault();
                if (device != null)
                {
                    sensor = device.CreateDevice().Sensor;
                    SensorFusion = Factory.CreateSensorFusion(sensor);
                }
            }

            if (device != null)
            {
                HResolution = Convert.ToInt32(device.DeviceInfo.HResolution);
                VResolution = Convert.ToInt32(device.DeviceInfo.VResolution);
                HScreenSize = device.DeviceInfo.HScreenSize;
                VScreenSize = device.DeviceInfo.VScreenSize;
                VScreenCenter = device.DeviceInfo.VScreenCenter;
                EyeToScreenDistance = device.DeviceInfo.EyeToScreenDistance;
                LensSeparationDistance = device.DeviceInfo.LensSeparationDistance;
                InterpupillaryDistance = device.DeviceInfo.InterpupillaryDistance;
                DistortionK = device.DeviceInfo.DistortionK;
            }
        }

        public static float VScreenSize { get; set; }
        public static float HScreenSize { get; set; }
        public static float VScreenCenter { get; set; }

        // TODO: Find representational default value, in meters
        public static float EyeToScreenDistance { get; set; }

        // TODO: Find representational default value
        public static float LensSeparationDistance { get; set; }

        public static float InterpupillaryDistance { get; set; }

        // TODO: the fuck is this shit
        public static float[] DistortionK { get; set; }

        internal static void Dispose()
        {
            try
            {
                if (sensor != null)
                {
                    sensor.Dispose();
                }
                if (device != null)
                {
                    device.Dispose();
                }
                if (deviceManager != null)
                {
                    deviceManager.Dispose();
                }
                if (SensorFusion != null)
                {
                    SensorFusion.Dispose();
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public static IDeviceManager deviceManager { get; set; }

        public static IDeviceHandle<IHMDDevice, IHMDInfo> device { get; set; }
    }
}

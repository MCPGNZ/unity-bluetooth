namespace pk.Bluetooth
{
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using InTheHand.Devices.Bluetooth;
    using InTheHand.Devices.Bluetooth.Rfcomm;
    using InTheHand.Devices.Enumeration;

    /// <summary>
    /// sample script for communicating with serial-port devices
    /// - opens device picker
    /// - checks if picked device supports serial-port communication
    /// - writes random value byte to stream
    /// - reads byte from stream
    /// </summary>
    public class BluetoothController : MonoBehaviour
    {
        #region Unity Methods
        /// <summary>
        /// Establish communication
        /// </summary>
        private void Start()
        {
            /* pick desired device */
            var deviceInformation = PickDevice();
            if(deviceInformation == null){ throw new InvalidDataException("Fail to retrieve device information - is the device turned on? (if so, try pairing it in Windows and try again)");}

            /* open serial-port stream (i.e in HC-05 bluetooth module) */
            _Stream = OpenBluetoothStream(deviceInformation, RfcommServiceId.SerialPort);
            if(_Stream == null) {throw new InvalidDataException("Failed to open stream - required service does not exist"); }
        }

        /// <summary>
        /// Test read/write to device
        /// </summary>
        private void Update()
        {
            /* check if the stream is opened */
            if (_Stream != null)
            {
                TestReadWrite(_Stream);
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        private void OnDisable()
        {
            _Stream?.Close();
        }
        #endregion Unity Methods

        #region Private Methods
        /// <summary>
        /// opens Bluetooth device picker and tries to open stream on it's SerialPort service,
        /// </summary>
        /// <returns>SerialPort stream, or null if it cannot connect</returns>
        private static Stream OpenBluetoothStream(DeviceInformation deviceInformation, RfcommServiceId serviceId)
        {
            /* get services from selected device */
            var device = BluetoothDevice.FromDeviceInformation(deviceInformation);
            var result = device.GetRfcommServices(BluetoothCacheMode.Cached);
            var services = result.Services;

            /* find requested service and open connection*/
            for (int i = 0; i < services.Count; ++i)
            {
                var current = services[i];
                if (current.ServiceId == serviceId)
                {
                    return current.OpenStream();
                }
            }

            /* if the reqired service does not exist */
            return null;
        }

        /// <summary>
        /// opens device picker and allows user to pick a single device from the list
        ///
        /// [troubleshooting]:
        /// - if no devices show up on the device list, try to pair the PC with desired device inside Windows and try again
        /// </summary>
        private static DeviceInformation PickDevice()
        {
            /* open device picker */
            var picker = new DevicePicker();
            var deviceInfo = picker.PickSingleDevice();
            return deviceInfo;
        }

        /// <summary>
        /// finds all devices that support specified service id
        /// </summary>
        private DeviceInformation[] FindAll(RfcommServiceId serviceId)
        {
            return DeviceInformation.FindAll(RfcommDeviceService.GetDeviceSelector(serviceId)).ToArray();
        }

        /// <summary>
        /// writes random value byte to stream 
        /// reads byte from stream
        /// </summary>
        private void TestReadWrite(Stream stream)
        {
            if (_Stream == null) { throw new InvalidDataException("Cannot read/write to null stream"); }

            /* send it to device via bluetooth*/
            {
                /* pick a random byte value */
                byte value = (byte) Random.Range(0, 255);

                /* send and log */
                _Stream.WriteByte(value);
                Debug.Log("Sent: " + value);
            }

            /* read the value passed back from device */
            {
                var buffer = new byte[1];
                int read = _Stream.Read(buffer, 0, 1);
                if (read != 0)
                {
                    Debug.Log("Received: " + buffer[0]);
                }
            }
        }
        #endregion Private Methods

        #region Private Variables
        private Stream _Stream;
        #endregion Private Variables
    }
}
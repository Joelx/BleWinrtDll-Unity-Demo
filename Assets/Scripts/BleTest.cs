using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BleTest : MonoBehaviour
{
    // Change this to match your device.
    string targetDeviceName = "EnterDeviceNameHere";
    string serviceUuid = "{2a2b1072-5199-11eb-ae93-0242ac130002}";
    string[] characteristicUuids = {
         "{59c2f246-5199-11eb-ae93-0242ac130002}",      // CUUID 1
         "{617c753e-5199-11eb-ae93-0242ac130002}"       // CUUID 2
    };

    BLE ble;
    BLE.BLEScan scan;
    bool isScanning = false;
    string deviceId = null;  
    IDictionary<string, string> discoveredDevices = new Dictionary<string, string>();

    // BLE Threads 
    Thread scanningThread, connectionThread;

    // GUI elements
    public Text TextDiscoveredDevices, TextIsScanning, TextTargetDeviceConnection, TextTargetDeviceData;
    public Button ButtonEstablishConnection;

    // Start is called before the first frame update
    void Start()
    {
        ble = new BLE();
    }

    // Update is called once per frame
    void Update()
    {
  
        if (isScanning)
        {
            TextIsScanning.color = new Color(244, 180, 26);
            TextIsScanning.text = "Scanning...";
            TextDiscoveredDevices.text = "";
            foreach (KeyValuePair<string, string> entry in discoveredDevices)
            {
                TextDiscoveredDevices.text += "DeviceID: " + entry.Key + "\nDeviceName: " + entry.Value + "\n\n";
                Debug.Log("Added device: " + entry.Key);
            }
        } else
        {
            TextIsScanning.color = Color.white;
            TextIsScanning.text = "Not scanning.";
        }

        // The target device was found.
        if (deviceId != null && deviceId != "-1")
        {
            if (ble.isConnected)
            {
                ButtonEstablishConnection.enabled = false;
                TextTargetDeviceConnection.text = "Connected to target device:\n" + targetDeviceName;

                TextTargetDeviceData.text = "";
                byte[] receivedPackage = BLE.ReadBytes();                
                Debug.Log("Raw package: " + Encoding.ASCII.GetString(receivedPackage));                

                // Example: Receiving int values from ESP32.
                ulong endianConvertedData = ConvertLittleEndian(receivedPackage);
                Debug.Log("Endian converted package: " + endianConvertedData);
                TextTargetDeviceData.text += "Endian conv. data: \n" + endianConvertedData;
                TextTargetDeviceData.text += "\n\nRaw data: \n" + Encoding.ASCII.GetString(receivedPackage);
            } else
            {
                ButtonEstablishConnection.enabled = true;
                TextTargetDeviceConnection.text = "Found target device:\n" + targetDeviceName;
            }
            
        } else
        {
            ButtonEstablishConnection.enabled = false;
            TextTargetDeviceConnection.text = targetDeviceName + " not found.";
        }

    }

    private void OnDestroy()
    {
        CleanUp();
    }

    private void OnApplicationQuit()
    {
        CleanUp();
    }

    // Prevent threading issues and free BLE stack.
    // Can cause Unity to freeze and lead
    // to errors when omitted.
    private void CleanUp()
    {
        try
        {
            scan.Cancel();
            ble.Close();
            scanningThread.Abort();
            connectionThread.Abort();
        } catch(NullReferenceException e)
        {
            Debug.Log("Thread or object never initialized.\n" + e);
        }        
    }

    public void StartScanHandler()
    {
        isScanning = true;
        scanningThread = new Thread(ScanBleDevices);
        scanningThread.Start();
    }

    public void ResetHandler()
    {
        TextTargetDeviceData.text = "";
        TextTargetDeviceConnection.text = targetDeviceName + " not found.";
        // Reset previous discovered devices
        discoveredDevices.Clear();
        TextDiscoveredDevices.text = "No devices.";
        deviceId = null;
        CleanUp();
    }

    void ScanBleDevices()
    {
        scan = BLE.ScanDevices();
        Debug.Log("BLE.ScanDevices() started.");
        scan.Found = (_deviceId, deviceName) =>
        {
            Debug.Log("found device with name: " + deviceName);
            discoveredDevices.Add(_deviceId, deviceName);

            if (deviceId == null && deviceName == targetDeviceName)
                deviceId = _deviceId;
        };

        scan.Finished = () =>
        {
            isScanning = false;
            Debug.Log("scan finished");
            if (deviceId == null)
                deviceId = "-1";
        };
        while (deviceId == null)
            Thread.Sleep(500);
        scan.Cancel();
        scanningThread.Abort();
        isScanning = false;

        if (deviceId == "-1")
        {
            Debug.Log("no device found!");
            return;
        }
    }

    // Start establish BLE connection with
    // target device in dedicated thread.
    public void StartConHandler()
    {
        connectionThread = new Thread(ConnectBleDevice);
        connectionThread.Start();
    }

    void ConnectBleDevice()
    {
        if (deviceId != null)
        {
            try
            {
                ble.Connect(deviceId,
                serviceUuid,
                characteristicUuids);
            } catch(Exception e)
            {
                Debug.Log("Could not establish connection to device with ID " + deviceId + "\n" + e);
            }
        }
        if (ble.isConnected)
            Debug.Log("Connected to: " + targetDeviceName);
    }

    ulong ConvertLittleEndian(byte[] array)
    {
        int pos = 0;
        ulong result = 0;
        foreach (byte by in array)
        {
            result |= ((ulong)by) << pos;
            pos += 8;
        }
        return result;
    }
}

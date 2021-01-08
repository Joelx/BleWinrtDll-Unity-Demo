# BleWinrtDll-Unity-Demo


This is a small Unity demo project showing an exemplary implementation of the <a href="https://github.com/adabru/BleWinrtDll">BleWinrtDLL repository</a> by <a href="https://github.com/adabru">adabru</a>.
<br>
Included is a simple Unity scene containing a GUI listing scanned BLE devices and making the connection to a BLE device accessible via button click. 
<br>
## Instructions
1. Download the project folder and import it as a new project in Unity Hub.
2. In Unity, open the `BleTest` scene.
3. Hit "play" to start the GUI.
4. If you want to connect a device, you first need to fill in the corresponding fields `targetDeviceName`, `serviceUuid` and `characteristicUuids` in the `BleTest.cs` file to match your BLE device.
5. (optional) A `BleWinrtDll.dll` is included, but you may also want to compile your own from the <a href="https://github.com/adabru/BleWinrtDll">BleWinrtDLL repository</a>)

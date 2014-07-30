RTSCam
======

Simple Real Time Strategy Camera Script for Unity 2D in C# 

Requirements
------
1. [Unity 3D] (http://unity3d.com/unity/download)
2. [Android SDK] (http://developer.android.com/sdk/index.html) for building to Android 
3. Mac with XCode for building to Mac and iOS
4. [Unity Web Player] (http://unity3d.com/webplayer) for running on browser

Instructions
------
1. Create an empty project in Unity.
2. Import the camera script (Assets/scripts/RTSCameraControl.cs) and an image file to serve as your world map. *Menu -> Assets -> Import New Asset*
3. Click on your world map image, set the 'Texture Type' property to Sprite(2d\ uGUI) and set the 'Max Size' property to 4096 through the Inspector.
4. Drag the world map image from your Project window to your Scene, set the Position to (0, 0, 0).
5. Click on the Scene's Main Camera, then apply the RTSCameraControl.cs by dragging it to Main Camera's Inspector
6. Click on the Scene's Main Camera again. On the Inspector, click on the arrow beside RTSCamera Control component to show the script's properties.
7. There should be a Map Sprite property. Assign your world map by dragging the world map image from the Heirarchy window to the script's Map Sprite field
8. Press Play

Notes
------
- You can download all the files and builds by the 'Download ZIP' at the right side
- There is a working Unity scene file named RTSCam.unity at Assets/scenes

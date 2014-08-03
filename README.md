RTSCam
======

Simple Real Time Strategy Camera Script for Unity 2D in C# 

Requirements
------
1. [Unity 3D] (http://unity3d.com/unity/download)
2. [Android SDK] (http://developer.android.com/sdk/index.html) for building the Android app
3. Mac with XCode for building the iOS app
4. [Unity Web Player] (http://unity3d.com/webplayer) for running on browser

Instructions
------
1. Create an empty project in Unity.
2. Import the camera script (Assets/scripts/RTSCameraControl.cs) and an image file to serve as your world map. *Menu -> Assets -> Import New Asset*
3. Click on your world map image, set the 'Texture Type' property to Sprite(2d\ uGUI) and set the 'Max Size' property to one size bigger the image. If the image is 2400x1600, use 4096.
    * Some devices do not support very large images, slicing them to smaller images may be necessary. Use a photo editor then import and set 'Texture Type' to Sprite(2d\ uGui) for each.
    * Drag the image slices from your Project window to your Scene then **reassemble** them. 
    * For example, a 2400x1600 image is sliced to two 1200x1600 images. 'Max Size' property should be 2048 for each slice.
4. Click on the Scene's Main Camera, then apply the RTSCameraControl.cs by dragging it to Main Camera's Inspector
5. Click on the Scene's Main Camera again. On the Inspector, click on the arrow beside RTSCamera Control component to show the script's properties.
6. There should be a Map Sprite Width and Map Sprite Height properties, input the world map width and height. If using image slices, use the total width and height of the assembled world map.
7. Press Play

Notes
------
- You can download all the files and builds by the *Download Zip* button at the right side of this page
- There is a working Unity scene file named RTSCam.unity at Assets/scenes
- There are working builds for different platforms at *build* folder
- You can preview the web browser build using this [link] (http://htmlpreview.github.io/?https://github.com/gaddamit/RTSCam/blob/master/build/unitywebplayer/unitywebplayer.html)

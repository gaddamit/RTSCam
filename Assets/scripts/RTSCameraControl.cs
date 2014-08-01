//
//Filename: RTSCameraControl.cs
//

using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/RTS")]
public class RTSCameraControl : MonoBehaviour
{ 
	public float mapSpriteWidth;
	public float mapSpriteHeight;

	public float orthoZoomSensitivity = 5.0f;
	public float orthoZoomSpeed = 3.0f;
	public float orthoZoomMin = 2.5f;
	public float orthoZoomMax= 8.0f;
	
	private float perspectiveZoomSensitivity= 30.0f;
	private float perspectiveZoomSpeed= 5.0f;
	private float perspectiveZoomMin= 15.0f;
	private float perspectiveZoomMax= 80.0f;

	public float touchZoomSpeed = 0.2f;
	public float touchDragSpeed = 15.0f;

	private float startTouchMagnitude;
	private float startTouchZoom;
	private float targetZoom;
	private bool isPinching = false;
	private Vector2 previousTouch;

	private Vector3 origPosition;
	private float origZoom;
	private float zoom;
	private float zoomSensitivity;
	private float zoomSpeed;
	private float zoomMin;
	private float zoomMax;

	private Rect bounds;
	
	private float vertExtent;
	private float horzExtent;

	private float lastScreenHeight;

	[System.Serializable]
	// Handles left modifiers keys (Alt, Ctrl, Shift)
	public class Modifiers
	{
		public bool leftAlt;
		public bool leftControl;
		public bool leftShift;
		
		public bool checkModifiers()
		{
			return (!leftAlt ^ Input.GetKey(KeyCode.LeftAlt)) &&
				(!leftControl ^ Input.GetKey(KeyCode.LeftControl)) &&
					(!leftShift ^ Input.GetKey(KeyCode.LeftShift));
		}
	}

	// MOUSE CONFIGURATION
	// Mouse buttons in the same order as Unity
	public enum MouseButton { Left = 0, Right = 1, Middle = 2, None = 3 }
	
	[System.Serializable]
	// Handles common parameters for translations and rotations
	public class MouseControlConfiguration
	{
		public bool activate;
		public MouseButton mouseButton;
		public Modifiers modifiers;
		public float sensitivity;
		
		public bool isActivated()
		{
			return activate && Input.GetMouseButton((int)mouseButton) && modifiers.checkModifiers();
		}
	}
	
	[System.Serializable]
	// Handles scroll parameters
	public class MouseScrollConfiguration
	{
		public bool activate;
		public Modifiers modifiers;
		public float sensitivity;
		
		public bool isActivated()
		{
			return activate && modifiers.checkModifiers();
		}
	}

	// Vertical translation default configuration
	public MouseControlConfiguration mouseVerticalTranslation = new MouseControlConfiguration { activate = true, mouseButton = MouseButton.Left, sensitivity = 0.25F };
	
	// Horizontal translation default configuration
	public MouseControlConfiguration mouseHorizontalTranslation = new MouseControlConfiguration { activate = true, mouseButton = MouseButton.Left, sensitivity = 0.25F };

	// Scroll default configuration
	public MouseScrollConfiguration mouseScroll = new MouseScrollConfiguration { activate = true, sensitivity = 2F };
	
	// Default unity names for mouse axes
	public string mouseHorizontalAxisName = "Mouse X";
	public string mouseVerticalAxisName = "Mouse Y";
	public string scrollAxisName = "Mouse ScrollWheel";

	// END OF MOUSE CONFIGURATION

	// KEYBOARD CONFIGURATION
	// Keyboard axes buttons in the same order as Unity
	public enum KeyboardAxis { Horizontal = 0, Vertical = 1, None = 3 }

	[System.Serializable]
	public class KeyboardControlConfiguration
	{
		public bool activate;
		public KeyboardAxis keyboardAxis;
		public Modifiers modifiers;
		public float sensitivity;
		
		public bool isActivated()
		{
			return activate && keyboardAxis != KeyboardAxis.None && modifiers.checkModifiers();
		}
	}

	// Vertical translation default configuration
	public KeyboardControlConfiguration keyboardVerticalTranslation = new KeyboardControlConfiguration { activate = true, keyboardAxis = KeyboardAxis.Vertical, sensitivity = 0.5F };
	
	// Horizontal translation default configuration
	public KeyboardControlConfiguration keyboardHorizontalTranslation = new KeyboardControlConfiguration { activate = true, keyboardAxis = KeyboardAxis.Horizontal, sensitivity = 0.5F };

	// Default unity names for keyboard axes
	public string keyboardHorizontalAxisName = "Horizontal";
	public string keyboardVerticalAxisName = "Vertical";
	public string[] keyboardAxesNames;
	// END KEYBOARD CONFIGURATION

	void Start() 
	{
		keyboardAxesNames = new string[] { keyboardHorizontalAxisName, keyboardVerticalAxisName};

		origPosition = camera.transform.localPosition;

#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY)
		lastScreenHeight = Screen.height;
#endif

		InitZoom ();
		AdjustBounds ();
	}

	void InitZoom()
	{
		if (camera.isOrthoGraphic)
		{
			origZoom = zoom = camera.orthographicSize;
			zoomSensitivity = orthoZoomSensitivity;
			zoomSpeed = orthoZoomSpeed;
			zoomMin = orthoZoomMin;
			//Ortographic Size: Camera's half-size when in orthographic mode.
			//Need to fit map's height
			// 1 Unity World Unity = 100 pixels
			zoomMax = mapSpriteHeight / 100.0f / 2;

			if (camera.aspect < mapSpriteWidth / (float)mapSpriteHeight)
			{
				float adjust = (mapSpriteWidth * Screen.height) / (mapSpriteHeight * Screen.width);
				zoomMax *= adjust;
			}
		} else {
			origZoom = zoom = camera.fieldOfView;
			zoomSensitivity = perspectiveZoomSensitivity;
			zoomSpeed = perspectiveZoomSpeed;
			zoomMin = perspectiveZoomMin;
			zoomMax = perspectiveZoomMax;
		}
	}

	void Update()
	{

#if !(UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY)
		zoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
		zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
#else
		if (lastScreenHeight != Screen.height)
		{
			zoomMax = mapSpriteHeight / 100.0f / 2;
			if (camera.aspect < (mapSpriteWidth / (float)mapSpriteHeight))
			{
				float adjust = (mapSpriteWidth * Screen.height) / (mapSpriteHeight * Screen.width);
				zoomMax *= adjust;
			}

			camera.orthographicSize = zoomMax;
			lastScreenHeight = Screen.height;
			AdjustBounds();
			Move ();
		}
#endif
	}
	
	void LateUpdate ()
	{
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
		if (Input.touchCount == 1 && !isPinching) 
		{
			Touch touch = Input.touches[0];
			if (Input.touches[0].phase == TouchPhase.Began)
			{
				previousTouch = Input.touches[0].position;
			}
			if (Input.touches[0].phase == TouchPhase.Moved) 
			{
				Vector3 previous = camera.ScreenToViewportPoint(new Vector3(previousTouch.x, previousTouch.y, camera.nearClipPlane));
				Vector3 current = camera.ScreenToViewportPoint(new Vector3(touch.position.x, touch.position.y, camera.nearClipPlane));
				
				Vector3 touchPosition = previous - current;
				camera.transform.position += (touchPosition * touchDragSpeed);

				previousTouch = touch.position;
			} 
		}
		
		//Pinch: Check for 2 fingers touch
		else if(Input.touchCount == 2) 
		{
			isPinching = true;
			if(Input.touches[1].phase == TouchPhase.Began)
			{ 
				startTouchMagnitude = (Input.touches[0].position - Input.touches[1].position).magnitude;
				startTouchZoom = Camera.main.orthographicSize;
			}
			
			float relativeMagnitudeChange = startTouchMagnitude / (Input.touches[0].position-Input.touches[1].position).magnitude;
			targetZoom = startTouchZoom * relativeMagnitudeChange;
			targetZoom = Mathf.Clamp(targetZoom, zoomMin, zoomMax);
			camera.orthographicSize = Mathf.Lerp (camera.orthographicSize, targetZoom, touchZoomSpeed);
		} else 
		{
			isPinching = false;
		}
#else
		if (mouseVerticalTranslation.isActivated())
		{
			float translateY = Input.GetAxis(mouseVerticalAxisName) * mouseVerticalTranslation.sensitivity;
			transform.Translate(0, -translateY, 0);
		}
		
		if (mouseHorizontalTranslation.isActivated())
		{
			float translateX = Input.GetAxis(mouseHorizontalAxisName) * mouseHorizontalTranslation.sensitivity;
			transform.Translate(-translateX, 0, 0);
		}
		
		if (mouseScroll.isActivated ()) 
		{
			if (camera.isOrthoGraphic)
				camera.orthographicSize = Mathf.Lerp (camera.orthographicSize, zoom, Time.deltaTime * zoomSpeed);
			else
				camera.fieldOfView = Mathf.Lerp (camera.fieldOfView, zoom, Time.deltaTime * zoomSpeed);
		}

		if (keyboardVerticalTranslation.isActivated())
		{
			float translateY = Input.GetAxis(keyboardAxesNames[(int)keyboardVerticalTranslation.keyboardAxis]) * keyboardVerticalTranslation.sensitivity;
			transform.Translate(0, translateY, 0);
		}

		if (keyboardHorizontalTranslation.isActivated())
		{
			float translateX = Input.GetAxis(keyboardAxesNames[(int)keyboardHorizontalTranslation.keyboardAxis]) * keyboardHorizontalTranslation.sensitivity;
			transform.Translate(translateX, 0, 0);
		}
#endif

		AdjustBounds ();
		Move ();
	}

	void AdjustBounds()
	{
		float mapWorldSizeWidth = mapSpriteWidth / 100 / 2.0f;
		float mapWorldSizeHeight = mapSpriteHeight / 100 / 2.0f;
		vertExtent = camera.orthographicSize;
		horzExtent = vertExtent * camera.aspect;
		
		bounds.yMin = vertExtent - mapWorldSizeHeight;
		bounds.yMin = Mathf.Clamp (bounds.yMin, -100, 0);
		bounds.yMax = mapWorldSizeHeight - vertExtent;
		bounds.yMax = Mathf.Clamp (bounds.yMax, 0, 100);

		bounds.xMin = horzExtent - mapWorldSizeWidth;
		bounds.xMin = Mathf.Clamp (bounds.xMin, -100, 0);
		bounds.xMax = mapWorldSizeWidth - horzExtent;
		bounds.xMax = Mathf.Clamp (bounds.xMax, 0, 100);
	}

	void Move()
	{
		Vector3 v3 = transform.localPosition;
		v3.x = Mathf.Clamp(v3.x, bounds.xMin, bounds.xMax);
		v3.y = Mathf.Clamp(v3.y, bounds.yMin, bounds.yMax);
		transform.localPosition = v3;
	}
}
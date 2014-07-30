//
//Filename: RTSCameraControl.cs
//

using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/RTS")]
public class RTSCameraControl : MonoBehaviour
{ 
	public SpriteRenderer mapSprite;

	public float orthoZoomSensitivity = 5.0f;
	public float orthoZoomSpeed = 3.0f;
	public float orthoZoomMin = 2.5f;
	public float orthoZoomMax= 8.0f;
	
	public float perspectiveZoomSensitivity= 30.0f;
	public float perspectiveZoomSpeed= 5.0f;
	public float perspectiveZoomMin= 15.0f;
	public float perspectiveZoomMax= 80.0f;

	public float touchZoomSpeed = 0.2f;
	public float touchDragSpeed = 15.0f;

	private float startTouchMagnitude;
	private float startTouchZoom;
	private float targetZoom;
	private bool isPinching = false;
	private bool isResettingCamera = false;
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
		InitBounds ();
		InitZoom ();
	}

	void InitBounds()
	{
		float mapSpriteWidth = mapSprite.sprite.rect.width;
		float mapSpriteHeight = mapSprite.sprite.rect.height;
		
		Vector3 mapVector = new Vector3 (mapSpriteWidth, mapSpriteHeight, camera.nearClipPlane);

		vertExtent = camera.orthographicSize;
		horzExtent = vertExtent * camera.aspect;
		
		// Calculations assume map is at origin
		bounds.xMin = (horzExtent - camera.ScreenToWorldPoint(mapVector).x) / 2.0f;
		bounds.xMax = (camera.ScreenToWorldPoint(mapVector).x  - horzExtent) / 2.0f;

		bounds.yMin = (vertExtent - camera.ScreenToWorldPoint(mapVector).y) / 2.0f;
		bounds.yMax = (camera.ScreenToWorldPoint(mapVector).y - vertExtent) / 2.0f;
	}

	void InitZoom()
	{
		if (camera.isOrthoGraphic)
		{
			origZoom = zoom = camera.orthographicSize;
			zoomSensitivity = orthoZoomSensitivity;
			zoomSpeed = orthoZoomSpeed;
			zoomMin = orthoZoomMin;
			zoomMax = orthoZoomMax;
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
#endif
	}
	
	void LateUpdate ()
	{
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
		if (isResettingCamera)
		{
			StartCoroutine("ResetCamera");
		}

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
			} else
			{
				if (touch.tapCount == 2)
				{
					isResettingCamera = true;
				}
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
		AdjustBounds();
		Move();
	}

	IEnumerator ResetCamera()
	{
		float i = 0;
		isResettingCamera = true;
		while(i < 1) {
			if (camera.isOrthoGraphic)
				camera.orthographicSize = Mathf.Lerp (camera.orthographicSize, zoom, i * zoomSpeed);
			else
				camera.fieldOfView = Mathf.Lerp (camera.fieldOfView, origZoom, i * zoomSpeed);
			
			camera.transform.position = Vector3.Lerp(camera.transform.position, origPosition, i * zoomSpeed);
			i += Time.deltaTime * zoomSpeed;
			yield return null;
		}
		isResettingCamera = false;
	}
	
	void AdjustBounds()
	{
		vertExtent = camera.orthographicSize;
		horzExtent = vertExtent * camera.aspect;
		
		bounds.yMin = vertExtent - zoomMax;
		bounds.yMin = Mathf.Clamp (bounds.yMin, -100, 0);
		bounds.yMax = zoomMax - vertExtent;
		bounds.yMax = Mathf.Clamp (bounds.yMax, 0, 100);

		bounds.xMin = bounds.yMin * camera.aspect;
		bounds.xMin = Mathf.Clamp (bounds.xMin, -100, 0);
		bounds.xMax = bounds.yMax * camera.aspect;
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
//
//Filename: MouseCameraControl.cs
//

using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse")]
public class MouseCameraControl : MonoBehaviour
{ 
	public SpriteRenderer mapSprite;
	private float mapSpriteWidth;
	private float mapSpriteHeight;
	
	public float cameraPositionX;
	public float cameraPositionY;
	public float orthoZoomSensitivity = 5.0f;
	public float orthoZoomSpeed = 3.0f;
	public float orthoZoomMin = 1.5f;
	public float orthoZoomMax= 8.0f;

	public float perspectiveZoomSensitivity= 30.0f;
	public float perspectiveZoomSpeed= 5.0f;
	public float perspectiveZoomMin= 15.0f;
	public float perspectiveZoomMax= 80.0f;

	private float zoom;
	private float zoomSensitivity;
	private float zoomSpeed;
	private float zoomMin;
	private float zoomMax;

	
	public float scrWidth;
	public float scrHeight;
	public float minX;
	public float maxX;
	public float minY;
	public float maxY;
	
	public float vertExtent;
	public float horzExtent;

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

	// Mouse Configuration
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
	
	// Yaw default configuration
	public MouseControlConfiguration yaw = new MouseControlConfiguration { mouseButton = MouseButton.Right, sensitivity = 10F };
	
	// Pitch default configuration
	public MouseControlConfiguration pitch = new MouseControlConfiguration { mouseButton = MouseButton.Right, modifiers = new Modifiers{ leftControl = true }, sensitivity = 10F };
	
	// Roll default configuration
	public MouseControlConfiguration roll = new MouseControlConfiguration();
	
	// Vertical translation default configuration
	public MouseControlConfiguration verticalTranslation = new MouseControlConfiguration { mouseButton = MouseButton.Middle, sensitivity = 2F };
	
	// Horizontal translation default configuration
	public MouseControlConfiguration horizontalTranslation = new MouseControlConfiguration { mouseButton = MouseButton.Middle, sensitivity = 2F };
	
	// Depth (forward/backward) translation default configuration
	public MouseControlConfiguration depthTranslation = new MouseControlConfiguration { mouseButton = MouseButton.Left, sensitivity = 2F };
	
	// Scroll default configuration
	public MouseScrollConfiguration scroll = new MouseScrollConfiguration { sensitivity = 2F };
	
	// Default unity names for mouse axes
	public string mouseHorizontalAxisName = "Mouse X";
	public string mouseVerticalAxisName = "Mouse Y";
	public string scrollAxisName = "Mouse ScrollWheel";

	// End of Mouse Configuration

	// Keyboard Configuration

	// End of Keyboard Configuration

	void Start() 
	{
		mapSpriteWidth = mapSprite.sprite.rect.width;
		mapSpriteHeight = mapSprite.sprite.rect.height;

		Vector3 mapVector = new Vector3 (mapSpriteWidth, mapSpriteHeight, camera.nearClipPlane);

		scrWidth = Screen.width;
		scrHeight = Screen.height;
		camera.orthographicSize = Screen.height / 2.0f / 100;
		
		vertExtent = camera.orthographicSize;
		horzExtent = vertExtent * camera.aspect;
		
		// Calculations assume map is at origin
		minX = (horzExtent - camera.ScreenToWorldPoint(mapVector).x) / 2.0f;
		maxX = (camera.ScreenToWorldPoint(mapVector).x  - horzExtent) / 2.0f;
		minY = (vertExtent - camera.ScreenToWorldPoint(mapVector).y) / 2.0f;
		maxY = (camera.ScreenToWorldPoint(mapVector).y - vertExtent) / 2.0f;

		if (camera.isOrthoGraphic)
		{
			zoom = camera.orthographicSize;
			zoomSensitivity = orthoZoomSensitivity;
			zoomSpeed = orthoZoomSpeed;
			zoomMin = orthoZoomMin;
			zoomMax = orthoZoomMax;
		} else {
			zoom = camera.fieldOfView;
			zoomSensitivity = perspectiveZoomSensitivity;
			zoomSpeed = perspectiveZoomSpeed;
			zoomMin = perspectiveZoomMin;
			zoomMax = perspectiveZoomMax;
		}
	}
	
	void Update()
	{
		zoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
		zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
	}

	void LateUpdate ()
	{
		if (yaw.isActivated())
		{
			float rotationX = Input.GetAxis(mouseHorizontalAxisName) * yaw.sensitivity;
			transform.Rotate(0, rotationX, 0);
		}
		if (pitch.isActivated())
		{
			float rotationY = Input.GetAxis(mouseVerticalAxisName) * pitch.sensitivity;
			transform.Rotate(-rotationY, 0, 0);
		}
		if (roll.isActivated())
		{
			float rotationZ = Input.GetAxis(mouseHorizontalAxisName) * roll.sensitivity;
			transform.Rotate(0, 0, rotationZ);
		}
		
		if (verticalTranslation.isActivated())
		{
			float translateY = Input.GetAxis(mouseVerticalAxisName) * verticalTranslation.sensitivity;
			transform.Translate(0, -translateY, 0);
		}
		
		if (horizontalTranslation.isActivated())
		{
			float translateX = Input.GetAxis(mouseHorizontalAxisName) * horizontalTranslation.sensitivity;
			transform.Translate(-translateX, 0, 0);
		}
		
		if (depthTranslation.isActivated())
		{
			float translateZ = Input.GetAxis(mouseVerticalAxisName) * depthTranslation.sensitivity;
			transform.Translate(0, 0, translateZ);
		}
		
		if (scroll.isActivated())
		{
			if (camera.isOrthoGraphic)
				camera.orthographicSize = Mathf.Lerp (camera.orthographicSize, zoom, Time.deltaTime * zoomSpeed);
			else
				camera.fieldOfView = Mathf.Lerp (camera.fieldOfView, zoom, Time.deltaTime * zoomSpeed);

			vertExtent = camera.orthographicSize;
			horzExtent = vertExtent * camera.aspect;

			minY = vertExtent - zoomMax;
			minY = Mathf.Clamp(minY, -100, 0);
			maxY = zoomMax - vertExtent;
			maxY = Mathf.Clamp(maxY, 0, 100);

			minX = minY * camera.aspect;
			minX = Mathf.Clamp(minX, -100, 0);
			maxX = maxY * camera.aspect;
			maxX = Mathf.Clamp(maxX, 0, 100);
		}

		Vector3 v3 = transform.localPosition;
		v3.x = Mathf.Clamp(v3.x, minX, maxX);
		v3.y = Mathf.Clamp(v3.y, minY, maxY);
		transform.localPosition = v3;
	}
}
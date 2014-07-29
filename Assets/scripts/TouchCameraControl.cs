//
//Filename: TouchCameraControl.cs
//

using UnityEngine;

[AddComponentMenu("Camera-Control/Touch")]
public class TouchCameraControl : MonoBehaviour
{
	public SpriteRenderer mapSprite;

	public float zoomSpeed = 0.2f;

	private float startTouchMagnitude;
	private float startTouchZoom;
	private float targetZoom;

	public float orthoZoomSensitivity = 5.0f;
	public float orthoZoomSpeed = 3.0f;
	public float orthoZoomMin = 2.5f;
	public float orthoZoomMax= 8.0f;
	
	public float perspectiveZoomSensitivity= 30.0f;
	public float perspectiveZoomSpeed= 5.0f;
	public float perspectiveZoomMin= 15.0f;
	public float perspectiveZoomMax= 80.0f;
	
	private float zoom;
	private float zoomSensitivity;

	private float zoomMin;
	private float zoomMax;
	
	private Rect bounds;
	
	private float vertExtent;
	private float horzExtent;

	private bool isPinching = false;
	private Vector2 previousTouch;
	void Start()
	{
		InitBounds ();
		InitZoom ();
	}

	void InitZoom()
	{
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

	void InitBounds()
	{
		float mapSpriteWidth = mapSprite.sprite.rect.width;
		float mapSpriteHeight = mapSprite.sprite.rect.height;
		
		Vector3 mapVector = new Vector3 (mapSpriteWidth, mapSpriteHeight, camera.nearClipPlane);
		
		//camera.orthographicSize = Screen.height / 2.0f / 100;
		
		vertExtent = camera.orthographicSize;
		horzExtent = vertExtent * camera.aspect;
		
		// Calculations assume map is at origin
		bounds.xMin = (horzExtent - camera.ScreenToWorldPoint(mapVector).x) / 2.0f;
		bounds.xMax = (camera.ScreenToWorldPoint(mapVector).x  - horzExtent) / 2.0f;
		
		bounds.yMin = (vertExtent - camera.ScreenToWorldPoint(mapVector).y) / 2.0f;
		bounds.yMax = (camera.ScreenToWorldPoint(mapVector).y - vertExtent) / 2.0f;
	}

	void Update()
	{

	}

	void LateUpdate()
	{
		//Drag: Check for single touch
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
				camera.transform.position += (touchPosition * 15);

				previousTouch = touch.position;
				//Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, camera.nearClipPlane));
				//touchPosition.x = -touchPosition.x;
				//touchPosition.y = -touchPosition.y;
				//transform.position = Vector3.Lerp(transform.position, touchPosition, Time.deltaTime * 5);
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
			camera.orthographicSize = Mathf.Lerp (camera.orthographicSize, targetZoom, zoomSpeed);
		} else 
		{
			isPinching = false;
		}

		
		AdjustBounds();
		Move();
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

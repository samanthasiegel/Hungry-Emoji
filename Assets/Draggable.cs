using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Draggable. Base class for Draggable behavior on objects in the scene. It uses the messages from the Unity UI system. To work
/// you need a EventSystem in the scene and a GraphicsRaycaster on the camera. The object with the Draggable component on it has
/// to have a collider on it. Depending on the Physics you need a 2D collidr and 2D raycaster or 3D respectively.
/// Extend this class and override the functions HandleBeginDrag, HandleDrag and HandleEndDrag, OnStartHovering, OnHovering, OnStopHovering and
/// OnDrop. They get called when the respective events happen.
/// </summary>
public abstract class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private const float PATH_NODE_DISTANCE = 10;
	private const float DRAG_FORCE_FACTOR = 200;

	#region Draggable abstract functions
	/// <summary>
	/// Gets called when the user starts to drag the object.
	/// </summary>
	/// <param name="dragData">Contains information about the ongoing drag.</param>
	protected abstract void HandleBeginDrag(DragInfo dragData);
	/// <summary>
	/// Gets called continously when the user drags the object.
	/// </summary>
	/// <param name="dragData">Contains information about the ongoing drag.</param>
	protected abstract void HandleDrag(DragInfo dragData);
	/// <summary>
	/// Gets called when the user stops to drag the object
	/// </summary>
	/// <param name="dragData">Contains information about the ongoing drag.</param>
	protected abstract void HandleEndDrag(DragInfo dragData);
	#endregion

	#region Droppable abstract functions
	/// <summary>
	/// Gets called when the user stops to drag the object and drops it into a dropcontainer
	/// </summary>
	/// <param name="dragData">Contains information about the ongoing drag.</param>
	/// <param name="dropData">Contains information about the drop event </param>
	protected abstract void OnDrop(DragInfo dragData, DropInfo dropData);
	/// <summary>
	/// Gets called when the user starts to hover (drag) the Draggable over a drop container
	/// </summary>
	/// <param name="dragData">Contains information about the ongoing drag.</param>
	/// <param name="dropData">Contains information about the hovered container</param>
	protected abstract void OnStartHovering(DragInfo dragData, DropInfo dropData);
	/// <summary>
	/// Gets called continuesly while the user is hovering (dragging) the Draggable over a drop container
	/// </summary>
	/// <param name="dragData">Contains information about the ongoing drag.</param>
	/// <param name="dropData">Contains information about the hovered container</param>
	protected abstract void OnHovering(DragInfo dragData, DropInfo dropData);
	/// <summary>
	/// Gets called when the user stops hovering (dragging) the Draggable over a drop container. 
	/// </summary>
	/// <param name="dragData">Contains information about the ongoing drag.</param>
	/// <param name="dropData">Contains information about the hovered container</param>
	protected abstract void OnStopHovering(DragInfo dragData, DropInfo dropData);
	#endregion

	#region Draggable fields
	public bool IsDragged { get; private set; }
	private float _lastOnDragCallTime;
	private Vector3 _lastFramePos;
	private float _dragDistanceToScreen;
	#endregion

	#region Droppable fields
	[SerializeField]
	private DroppableConfig _droppableConfig = new DroppableConfig();
	public bool IsDroppable
	{
		get
		{
			return _droppableConfig.isDroppable;
		}
	}

	private DropContainer _currentHoveredContainer = null;
	#endregion

	private DragInfo _dragInfo;
	private DropInfo _dropInfo;
	private delegate DropContainer GetHoveredContainer(PointerEventData data, Draggable draggable);

	private void InitializeDrag(PointerEventData eventData)
	{
		_dragInfo = new DragInfo();
		_dropInfo = new DropInfo();
		_dropInfo.Draggable = this;
		_dragDistanceToScreen = eventData.pressEventCamera.WorldToScreenPoint(gameObject.transform.position).z;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		InitializeDrag(eventData);
		//Debug.Log("Begin Drag");
		IsDragged = true;
		_dragInfo.DragStartPos = transform.position;
		_dragInfo.DragEndPos = transform.position;
		_dragInfo.DragStartTime = Time.time;

		Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);

		_dragInfo.DragScreenPath.AddLast(screenPos);
		_dragInfo.DragScreenPath.AddLast(screenPos);

		_dragInfo.DragPath.AddLast(transform.position);
		_dragInfo.DragPath.AddLast(transform.position);
		HandleBeginDrag(_dragInfo);
	}

	public void OnDrag(PointerEventData eventData)
	{
		//Debug.Log("On Drag");
		IsDragged = true;
		SetDraggedPosition(eventData);

		_dragInfo.DragEndPos = transform.position;
		Vector3 dragMoveDelta = _lastFramePos - transform.position;
		float dragDeltaTime = (Time.time - _lastOnDragCallTime);
		_dragInfo.DragDistance += dragMoveDelta.magnitude;
		_dragInfo.DragVelocity = Vector3.Lerp(dragMoveDelta * (1 / dragDeltaTime), _dragInfo.DragVelocity, 0.5f);
		_lastFramePos = transform.position;
		_lastOnDragCallTime = Time.time;

		Vector2 lastScreenPathPos = _dragInfo.DragScreenPath.Last.Value;
		Vector2 screenPos = eventData.pressEventCamera.WorldToScreenPoint(transform.position);
		_dragInfo.DragScreenPath.Last.Value = screenPos;
		_dragInfo.DragPath.Last.Value = transform.position;
		if (Vector2.Distance(lastScreenPathPos, screenPos) > PATH_NODE_DISTANCE)
		{
			_dragInfo.DragScreenPath.AddLast(screenPos);
			_dragInfo.DragPath.AddLast(transform.position);
		}
		HandleDrag(_dragInfo);
		if (_droppableConfig.isDroppable)
		{
			UpdateDroppableHover(eventData);
		}

		#if UNITY_EDITOR
		IEnumerator<Vector3> it = _dragInfo.DragPath.GetEnumerator();
		it.MoveNext();
		Vector3 first = it.Current;
		Vector3 last;
		while (it.MoveNext())
		{
			last = it.Current;
			Debug.DrawLine(first, last, Color.blue);
			first = last;
		}
		#endif

	}

	public void OnEndDrag(PointerEventData eventData)
	{
		//Debug.Log("End Drag");
		IsDragged = false;
		_dragInfo.DragEndPos = transform.position;
		_dragInfo.DragEndTime = Time.time;
		HandleEndDrag(_dragInfo);
		if (_droppableConfig.isDroppable && _currentHoveredContainer != null)
		{
			DropIntoContainer(_currentHoveredContainer);
			_currentHoveredContainer = null;
		}
	}

	private void SetDraggedPosition(PointerEventData eventData)
	{
		Vector3 newPos = new Vector3(eventData.position.x, eventData.position.y, _dragDistanceToScreen);
		transform.position = eventData.pressEventCamera.ScreenToWorldPoint(newPos);
	}

	#region Droppable functions
	private void UpdateDroppableHover(PointerEventData eventData)
	{
		DropContainer oldHover = _currentHoveredContainer;
		_currentHoveredContainer = PhysicsModule.GetHoveredContainer(_droppableConfig.overlapPhysics, eventData, this);
		if (_currentHoveredContainer != null)
		{
			_dropInfo.Container = _currentHoveredContainer;
		}
		SendHoverMessages(oldHover);
	}

	private void DropIntoContainer(DropContainer container)
	{
		OnStopHovering(_dragInfo, _dropInfo);
		container.OnStopHover(_dragInfo, _dropInfo);
		OnDrop(_dragInfo, _dropInfo);
		container.OnDrop(_dragInfo, _dropInfo);
	}

	private void SendHoverMessages(DropContainer oldHover)
	{
		// if both are null nothing happes, if one if not null, we ether enter a new one, leave the current one or both
		if (_currentHoveredContainer != oldHover)
		{
			if (_currentHoveredContainer != null) // we enter a new hover
			{
				OnStartHovering(_dragInfo, _dropInfo);
				_currentHoveredContainer.OnStartHover(_dragInfo, _dropInfo);
			}
			else if (oldHover != null) // we leave the current hover
			{
				OnStopHovering(_dragInfo, _dropInfo);
				oldHover.OnStopHover(_dragInfo, _dropInfo);
			}
		}

		if (_currentHoveredContainer != null)
		{
			OnHovering(_dragInfo, _dropInfo);
			_currentHoveredContainer.OnHover(_dragInfo, _dropInfo);
		}
	}
	#endregion

	#region SubClasses
	internal enum DroppablePhysics
	{
		Overlap2D, Overlap3D, Point2D, ScreenRay3D
	}

	public class DragInfo
	{
		internal DragInfo()
		{
			DragPath = new LinkedList<Vector3>();
			DragScreenPath = new LinkedList<Vector2>();
		}

		public Vector3 DragStartPos { get; internal set; }
		public Vector3 DragEndPos { get; internal set; }
		public Vector3 DragPotentialForce { get { return (DragStartPos - DragEndPos) * DRAG_FORCE_FACTOR; } }
		public Vector3 DragVelocity { get; internal set; }

		public float DragTime { get { return DragEndTime - DragStartTime; } }
		public float DragStartTime { get; internal set; }
		public float DragEndTime { get; internal set; }
		public LinkedList<Vector3> DragPath { get; internal set; }
		public float DragDistance { get; internal set; }

		public LinkedList<Vector2> DragScreenPath { get; internal set; }
		public Vector2 DragStartScreenPos { get; internal set; }
		public Vector2 DragEndScreenPos { get; internal set; }
		public Vector2 DragTotalScreenDistance { get; internal set; }
		public Vector2 DragFrameScreenDistance { get; internal set; }

		internal void Reset()
		{
		}
	}

	public class DropInfo
	{

		internal DropInfo()
		{
		}

		public DropContainer Container { get; internal set; }
		public Draggable Draggable { get; internal set; }
	}
	#endregion

	[Serializable]
	public class DroppableConfig
	{
		internal DroppableConfig() { }

		[SerializeField]
		internal bool isDroppable = true;

		[SerializeField]
		internal float collisionDistance = 5;

		[SerializeField]
		internal LayerMask dropContainerLayers = 1; // default layer

		[SerializeField]
		internal DroppablePhysics overlapPhysics = DroppablePhysics.ScreenRay3D;
	}

	#region PhysicChecks
	private static class PhysicsModule
	{
		private const int PHYSICS_CHECK_MEMORY_ALLOC = 10;
		private static readonly Dictionary<DroppablePhysics, GetHoveredContainer> GET_HOVERED_CONTAINER_FUNCTIONS = null;

		private static Collider2D[] hits2D = new Collider2D[PHYSICS_CHECK_MEMORY_ALLOC];
		private static Collider[] hits3D = new Collider[PHYSICS_CHECK_MEMORY_ALLOC];
		private static RaycastHit[] RaycastHits = new RaycastHit[PHYSICS_CHECK_MEMORY_ALLOC];

		static PhysicsModule()
		{
			GET_HOVERED_CONTAINER_FUNCTIONS = new Dictionary<DroppablePhysics, GetHoveredContainer>();
			GET_HOVERED_CONTAINER_FUNCTIONS.Add(DroppablePhysics.Overlap2D, GetHoveredContainerOverlap2D);
			GET_HOVERED_CONTAINER_FUNCTIONS.Add(DroppablePhysics.Overlap3D, GetHoveredContainerOverlap3D);
			GET_HOVERED_CONTAINER_FUNCTIONS.Add(DroppablePhysics.Point2D, GetHoveredContainerPosition2D);
			GET_HOVERED_CONTAINER_FUNCTIONS.Add(DroppablePhysics.ScreenRay3D, GetHoveredContainerScreenRay3D);
		}

		internal static DropContainer GetHoveredContainer(DroppablePhysics physics, PointerEventData eventData, Draggable draggable)
		{
			return GET_HOVERED_CONTAINER_FUNCTIONS[physics](eventData, draggable);
		}

		private static DropContainer GetHoveredContainerPosition2D(PointerEventData data, Draggable draggable)
		{
			int numHits = Physics2D.OverlapPointNonAlloc(draggable.transform.position, hits2D, draggable._droppableConfig.dropContainerLayers);
			#if UNITY_EDITOR
			Debug.DrawLine(draggable.transform.position + new Vector3(draggable._droppableConfig.collisionDistance, 0, 0), draggable.transform.position - new Vector3(draggable._droppableConfig.collisionDistance, 0, 0), Color.blue);
			Debug.DrawLine(draggable.transform.position + new Vector3(0, draggable._droppableConfig.collisionDistance, 0), draggable.transform.position - new Vector3(0, draggable._droppableConfig.collisionDistance, 0), Color.blue);
			#endif
			DropContainer nowHoveredContainer = null;
			float closestDist;
			DropContainer container;
			for (int i = 0; i < numHits; i++)
			{
				closestDist = float.PositiveInfinity;
				if ((container = hits2D[i].gameObject.GetComponent<DropContainer>()) != null)
				{
					float containerDist = Vector2.Distance(container.transform.position, draggable.transform.position);
					if (containerDist < closestDist)
					{
						nowHoveredContainer = container;
					}
				}
			}
			return nowHoveredContainer;
		}

		private static DropContainer GetHoveredContainerScreenRay3D(PointerEventData data, Draggable draggable)
		{
			Ray ray = data.pressEventCamera.ScreenPointToRay(data.pressEventCamera.WorldToScreenPoint(draggable.transform.position));
			int numHits = Physics.RaycastNonAlloc(ray, RaycastHits, draggable._droppableConfig.collisionDistance, draggable._droppableConfig.dropContainerLayers);
			#if UNITY_EDITOR
			Debug.DrawLine(ray.GetPoint(0), ray.GetPoint(draggable._droppableConfig.collisionDistance), Color.blue);
			#endif
			DropContainer nowHoveredContainer = null;
			float closestDist;
			DropContainer container;
			for (int i = 0; i < numHits; i++)
			{
				closestDist = float.PositiveInfinity;
				if ((container = RaycastHits[i].collider.gameObject.GetComponent<DropContainer>()) != null)
				{
					float containerDist = Vector3.Distance(container.transform.position, draggable.transform.position);
					if (containerDist < closestDist)
					{
						nowHoveredContainer = container;
					}
				}
			}
			return nowHoveredContainer;
		}

		private static DropContainer GetHoveredContainerOverlap2D(PointerEventData data, Draggable draggable)
		{
			int numHits = Physics2D.OverlapCircleNonAlloc(draggable.transform.position, draggable._droppableConfig.collisionDistance, hits2D, draggable._droppableConfig.dropContainerLayers);
			#if UNITY_EDITOR
			DebugGizmos.DrawWireSphere(draggable.transform.position, draggable._droppableConfig.collisionDistance, Color.blue);
			#endif
			DropContainer nowHoveredContainer = null;
			float closestDist;
			DropContainer container;
			for (int i = 0; i < numHits; i++)
			{
				closestDist = float.PositiveInfinity;
				if ((container = hits2D[i].gameObject.GetComponent<DropContainer>()) != null)
				{
					float containerDist = Vector2.Distance(container.transform.position, draggable.transform.position);
					if (containerDist < closestDist)
					{
						nowHoveredContainer = container;
					}
				}
			}
			return nowHoveredContainer;
		}

		private static DropContainer GetHoveredContainerOverlap3D(PointerEventData data, Draggable draggable)
		{
			int numHits = Physics.OverlapSphereNonAlloc(draggable.transform.position, draggable._droppableConfig.collisionDistance, hits3D, draggable._droppableConfig.dropContainerLayers);
			#if UNITY_EDITOR
			DebugGizmos.DrawWireSphere(draggable.transform.position, draggable._droppableConfig.collisionDistance, Color.blue);
			#endif
			DropContainer nowHoveredContainer = null;
			float closestDist;
			DropContainer container;
			for (int i = 0; i < numHits; i++)
			{
				closestDist = float.PositiveInfinity;
				if ((container = hits3D[i].gameObject.GetComponent<DropContainer>()) != null)
				{
					float containerDist = Vector3.Distance(container.transform.position, draggable.transform.position);
					if (containerDist < closestDist)
					{
						nowHoveredContainer = container;
					}
				}
			}
			return nowHoveredContainer;
		}
	}
	#endregion
}
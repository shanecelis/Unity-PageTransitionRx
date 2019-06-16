using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ObservableEventTrigger))]
public class SwipeGesture : MonoBehaviour {
  // public float TotalSeconds = 1.0f;
  public float ThresholdSeconds = 1.0f;
  public float ThresholdDistance = 100.0f;

  public IObservable<PointerEventData> OnSwipeLeft;
  public IObservable<PointerEventData> OnSwipeRight;
  public IObservable<PointerEventData> OnSwipeDown;
  public IObservable<PointerEventData> OnSwipeUp;
  public IObservable<Vector2> OnDragRelative;

  private Vector2 beginPosition;
  private DateTime beginTime;
  private ObservableEventTrigger eventTrigger;

  private IObservable<PointerEventData> onEndDragObservable;

  public IObservable<MoveDirection> OnSwipe() {
    return onEndDragObservable
      .Select(eventData => SwipeDirection(eventData));
  }

  public IObservable<PointerEventData> OnSwipeWhere(Func<MoveDirection, bool> predicate) {
    return onEndDragObservable
      .Where(eventData => predicate(SwipeDirection(eventData)));
  }

  public IObservable<PointerEventData> OnSwipe(MoveDirection direction) {
    return OnSwipeWhere(moveDirection => moveDirection == direction);
  }

  void Awake() {
    eventTrigger = this.gameObject.GetComponent<ObservableEventTrigger>();
  }

  void OnEnable() {
    // XXX: Consider switching to Observable{Begin,,End}DragTrigger.
    eventTrigger
      .OnBeginDragAsObservable()
      .TakeUntilDisable(this)
      .Where(eventData => eventData.pointerDrag.gameObject == this.gameObject)
      .Select(eventData => eventData.position)
      .Subscribe(position => {
          this.beginPosition = position;
          this.beginTime = DateTime.Now;
        });

    OnDragRelative = eventTrigger
      .OnDragAsObservable()
      .TakeUntilDisable(this)
      .Where(eventData => eventData.pointerDrag.gameObject == this.gameObject )
      .Select(eventData => eventData.delta);

    onEndDragObservable = eventTrigger
      .OnEndDragAsObservable()
      .TakeUntilDisable(this);

    OnSwipeLeft = OnSwipe(MoveDirection.Left);

    OnSwipeRight = OnSwipe(MoveDirection.Right);

    OnSwipeUp = OnSwipe(MoveDirection.Up);

    OnSwipeDown = OnSwipe(MoveDirection.Down);

  }

  protected MoveDirection SwipeDirection(PointerEventData eventData) {
    // Timeout.
    if ((DateTime.Now - this.beginTime).TotalSeconds > this.ThresholdSeconds)
      return MoveDirection.None;

    var delta_x = eventData.position.x - beginPosition.x;
    var delta_y = eventData.position.y - beginPosition.y;
    if (Mathf.Max(Mathf.Abs(delta_x), Mathf.Abs(delta_y)) >= this.ThresholdDistance) {
      // We have a swipe.
      if (Mathf.Abs(delta_x) > Mathf.Abs(delta_y)) {
        return delta_x > 0 ? MoveDirection.Right : MoveDirection.Left;
      } else {
        return delta_y > 0 ? MoveDirection.Up : MoveDirection.Down;
      }
    } else {
      return MoveDirection.None;
    }
  }
}

using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeGesture : MonoBehaviour
{
  public float TotalSeconds = 1.0f;
  public float ThresholdSenconds = 1.0f;
  public float ThresholdDistance = 100.0f;

  private Subject<Unit> onSwipeLeft = new Subject<Unit>();
  public IObservable<PointerEventData> OnSwipeLeft;
  // {
  //   get { return onSwipeLeft; }
  // }

  private Subject<Unit> onSwipeRight = new Subject<Unit>();
  public IObservable<PointerEventData> OnSwipeRight;
  // {
  //   get { return onSwipeRight; }
  // }

  // private Subject<PointerEventData> onSwipeRight2 = new Subject<PointerEventData>();
  // public IObservable<PointerEventData> OnSwipeRight2
  // {
  //   get { return onSwipeRight2; }
  // }

  private Subject<Unit> onSwipeDown = new Subject<Unit>();
  public IObservable<PointerEventData> OnSwipeDown;
  // {
  //   get { return onSwipeDown; }
  // }

  private Subject<Unit> onSwipeUp = new Subject<Unit>();
  public IObservable<PointerEventData> OnSwipeUp;
  // {
  //   get { return onSwipeUp; }
  // }

  private Subject<Vector2> onDragRelative = new Subject<Vector2>();
  public IObservable<Vector2> OnDragRelative
  {
    get { return onDragRelative; }
  }

  private Vector2 beginPosition;
  private DateTime beginTime;

  public IObservable<PointerEventData> onEndDragObservable;

  public IObservable<PointerEventData> OnSwipeWhere(Func<MoveDirection, bool> predicate) {
    return onEndDragObservable
      .Where(eventData => predicate(SwipeDirection(eventData)));
  }

  void OnEnable()
  {
    Debug.Log("Enabled");
    var eventTrigger = this.gameObject.AddComponent < ObservableEventTrigger > ();

    // eventTrigger
    //   .OnBeginDragAsObservable ()
    //   .Subscribe(_ => Debug.Log("Begin Drag."));

    eventTrigger
     .OnBeginDragAsObservable ()
     .TakeUntilDisable(this)
     .Where(eventData => eventData.pointerDrag.gameObject == this.gameObject)
     .Select(eventData => eventData.position)
     .Subscribe(position =>
          {
            this.beginPosition =  position ;
            this.beginTime  =  DateTime.Now ;
          });

    eventTrigger
      .OnDragAsObservable ()
      .TakeUntilDisable(this )
      .Where(eventData => eventData.pointerDrag.gameObject == this.gameObject )
      // .Select(eventData => eventData.position - eventData.pressPosition)
      .Select(eventData => eventData.delta)
      .Subscribe(delta => onDragRelative.OnNext(delta));


    eventTrigger
      .OnEndDragAsObservable ()
      .TakeUntilDisable(this )
      .Where(eventData => eventData.pointerDrag.gameObject == this.gameObject )
      // .Select(eventData => eventData.position - eventData.pressPosition)
      .Select(eventData => eventData.delta)
      .Subscribe(delta => onDragRelative.OnNext(delta));

    onEndDragObservable = eventTrigger
      .OnEndDragAsObservable()
      .TakeUntilDisable(this)
      .Where(eventData => (DateTime.Now - this.beginTime).TotalSeconds < this.ThresholdSenconds);
      // .Select(eventData => eventData.position )
      // .Share();

    // var  onEndDragObservable2  =  eventTrigger
    //   .OnEndDragAsObservable ()
    //   .TakeUntilDisable(this )
    //   .Where(eventData => (DateTime.Now - this.beginTime).TotalSeconds < this.ThresholdSenconds )
    //   // .Select(eventData  => eventData.position )
    //   .Share ();

    OnSwipeLeft = onEndDragObservable
      .Where(eventData => SwipeDirection(eventData) == MoveDirection.Left);

    OnSwipeRight = onEndDragObservable
      .Where(eventData => SwipeDirection(eventData) == MoveDirection.Right);

    OnSwipeUp = onEndDragObservable
      .Where(eventData => SwipeDirection(eventData) == MoveDirection.Up);

    OnSwipeDown = onEndDragObservable
      .Where(eventData => SwipeDirection(eventData) == MoveDirection.Down);

    // // left
    // onEndDragObservable
    //  .Where(position  =>  beginPosition.x  >  position.x )
    //  .Where(position  =>  Mathf.Abs(beginPosition.x  -  position.x ) >= this.ThresholdDistance )
    //  .Subscribe(_  =>  onSwipeLeft.OnNext(Unit.Default )) ;

    // // right
    // // onEndDragObservable
    // //  .Where(position => position.x  >  beginPosition.x )
    // //  .Where(position => Mathf.Abs(position.x  -  beginPosition.x ) >= this.ThresholdDistance )
    // //  .Subscribe(_  => onSwipeRight.OnNext(Unit.Default ));
    // // right
    // onEndDragObservable2
    //  .Where(eventData => eventData.position.x  >  beginPosition.x )
    //  .Where(eventData => Mathf.Abs(eventData.position.x  -  beginPosition.x ) >= this.ThresholdDistance )
    //  .Subscribe(eventData => onSwipeRight2.OnNext(eventData));

    // // down
    // onEndDragObservable
    //  .Where(position => beginPosition.y > position.y )
    //  .Where(position => Mathf.Abs (beginPosition.y  -  position.y ) >= this.ThresholdDistance )
    //  .Subscribe(_ => onSwipeDown.OnNext (Unit.Default));

    // // up
    // onEndDragObservable
    //  .Where(position => position.y > beginPosition.y )
    //  .Where(position => Mathf.Abs(position.y - beginPosition.y ) >=  this.ThresholdDistance )
    //  .Subscribe(_ => onSwipeUp.OnNext (Unit.Default));
  }

  public MoveDirection SwipeDirection(PointerEventData eventData) {
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

// public static class SwipeGestureExtensions {

//   public static IObservable<PointerEventData> WhereSwipe(IObservable<PointerEventData
// }

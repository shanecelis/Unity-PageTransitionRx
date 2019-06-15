using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UniRx.Triggers;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SwipeGesture), typeof(HorizontalLayoutGroup))]
public class TurnPage : MonoBehaviour
{
  public float PageWidth = 1080;

  private RectTransform rectTransform;
  private SwipeGesture swipeGesture;
  private Tween moveAnimation;
  private int pageCount;
  private int currentPage = 0;
  private Vector2 anchorBeginPosition;
  private Vector2 pageWidthVector;

  void Awake()
  {
    DOTween.Init();
    DOTween.defaultAutoPlay = AutoPlay.None;  // Tween 생성시 자동 실행시키지

    pageWidthVector = new Vector2(-PageWidth, 0f);
  }

  void OnEnable()
  {
    // Debug.Log("Enabled");

    // eventTrigger
    //   .OnBeginDragAsObservable ()
    //   .TakeUntilDisable(this)
    //   .Where(eventData => eventData.pointerDrag.gameObject == this.gameObject)
    //   .Subscribe(_ => {
    //       anchorBeginPosition = rectTransform.anchoredPosition;
    //     });

    var eventTrigger = GetComponent<ObservableEventTrigger>();
    this.rectTransform  =  this.GetComponent<RectTransform>();
    anchorBeginPosition = rectTransform.anchoredPosition;
    this.swipeGesture  =  this.GetComponent<SwipeGesture>();
    this.pageCount  =  this.transform.childCount ;
    this.swipeGesture.OnDragRelative
      .Where(_  =>  this.moveAnimation  ==  null  ||  ! this.moveAnimation.IsPlaying ())  // 애니메이션 실행 중이 아니
      .Subscribe(delta =>
          {
           // Debug.Log("delta " + delta);
            rectTransform.anchoredPosition += delta;
          });

    // // next
    // this.swipeGesture
    //  .OnSwipeLeft
    //  .Where(_  =>  currentPage  <  pageCount - 1)  // 최대 페이지 이전 인 경우에만 진행
    //  .Where(_  =>  this.moveAnimation  ==  null  ||  ! this.moveAnimation.IsPlaying ())  // 애니메이션 실행 중이 아니
    //  .Subscribe(_ =>
    //       {
    //         this.currentPage++;
    //         this.moveAnimation  =  this.rectTransform
    //         .DOAnchorPos (anchorBeginPosition +  currentPage * this.pageWidthVector, 1.0f)
    //         .SetEase(Ease.OutBounce)
    //         .Play ();
    //       });

    // // back
    // this.swipeGesture
    //  .OnSwipeRight
    //  .Where(_  =>  currentPage  >  0)  // 1 페이지 이상인 경우에만 돌아갈
    //  .Where(_  =>  this.moveAnimation == null || !this.moveAnimation.IsPlaying())  // 애니메이션 실행 중이 없다
    //  .Subscribe(_  =>
    //       {
    //         this.currentPage--;
    //         this.moveAnimation = this.rectTransform
    //         .DOAnchorPos(anchorBeginPosition + currentPage * this.pageWidthVector, 1.0f)
    //         .SetEase(Ease.OutBounce).Play();
    //       });

    swipeGesture
      .OnSwipeWhere(moveDirection =>
          {
            if (this.moveAnimation != null && this.moveAnimation.IsPlaying())
              return false;

            switch (moveDirection) {
              case MoveDirection.Right:
              if (currentPage > 0)
                this.currentPage--;
              break;
              case MoveDirection.Left:
              if (currentPage < pageCount - 1)
                this.currentPage++;
              break;
            }
            return true;
          })
      .Subscribe(_ =>
          {
            this.moveAnimation = this.rectTransform
            .DOAnchorPos(anchorBeginPosition + currentPage * this.pageWidthVector, 1.0f)
            .SetEase(Ease.OutBounce).Play();
          });

    // back
    // this.swipeGesture
    //  .OnSwipeRight2
    //  .Where(_  =>  currentPage  >  1)  // 1 페이지 이상인 경우에만 돌아갈
    //  .Where(_  =>  this.moveAnimation == null || !this.moveAnimation.IsPlaying())  // 애니메이션 실행 중이 없다
    //  .Subscribe(eventData  =>
    //       {
    //         this.currentPage--;
    //         this.moveAnimation = this.rectTransform
    //         .DOAnchorPos(rectTransform.anchoredPosition
    //                       - (eventData.position - eventData.pressPosition)
    //                       + new Vector2(this.PageWidth, 0f),
    //                       1.0f)
    //         .SetEase(Ease.OutBounce).Play();
    //       });
  }
}

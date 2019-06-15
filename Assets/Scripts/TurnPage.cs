using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UniRx.Triggers;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SwipeGesture), typeof(HorizontalLayoutGroup))]
public class TurnPage : MonoBehaviour {
  public float PageWidth = 1080;

  private RectTransform rectTransform;
  private SwipeGesture swipeGesture;
  private Tween moveAnimation;
  private int pageCount;
  private int currentPage = 0;
  private Vector2 anchorBeginPosition;
  private Vector2 pageWidthVector;

  void Awake() {
    DOTween.Init();
    DOTween.defaultAutoPlay = AutoPlay.None; // Tween 생성시 자동 실행시키지
    pageWidthVector = new Vector2(-PageWidth, 0f);
    this.rectTransform = this.GetComponent<RectTransform>();
    this.swipeGesture = this.GetComponent<SwipeGesture>();
    anchorBeginPosition = rectTransform.anchoredPosition;
  }

  void OnEnable() {
    pageCount = transform.childCount;
    swipeGesture
      .OnDragRelative
      .Where(_ => this.moveAnimation == null || ! this.moveAnimation.IsPlaying ()) // 애니메이션 실행 중이 아니
      .Subscribe(delta =>
          {
            rectTransform.anchoredPosition += delta;
          });

    swipeGesture
      .OnSwipe()
      .Where(_ => this.moveAnimation == null || !this.moveAnimation.IsPlaying()) // 애니메이션 실행 중이 없다
      .Do(moveDirection =>
          {
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
          })
      .Subscribe(_ =>
          {
            this.moveAnimation
              = this.rectTransform
              .DOAnchorPos(anchorBeginPosition + currentPage * this.pageWidthVector, 1.0f)
              .SetEase(Ease.OutBounce)
              .Play();
          });

  }
}

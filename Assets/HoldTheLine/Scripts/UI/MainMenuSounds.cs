using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HoldTheLine.Scripts.UI
{
    public class TestPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public MMF_Player MyPlayer;

        public void OnPointerEnter(PointerEventData eventData)
        {
            MyPlayer.Direction = MMFeedbacks.Directions.TopToBottom;
            MyPlayer.PlayFeedbacks();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MyPlayer.Direction = MMFeedbacks.Directions.BottomToTop;
            MyPlayer.PlayFeedbacks();
        }
    }
}
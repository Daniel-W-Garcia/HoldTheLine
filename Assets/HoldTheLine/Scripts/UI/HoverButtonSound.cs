using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HoldTheLine.Scripts.UI
{
    public class HoverButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Sound Feedback")]
        public MMF_Player MyPlayer;

        [Header("Visual Feedback")]
        public Graphic TargetGraphic; 
        public Color HoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        private Color _originalColor;

        void Start()
        {
            if (TargetGraphic == null)
            {
                TargetGraphic = GetComponent<Graphic>();
            }

            if (TargetGraphic != null)
            {
                _originalColor = TargetGraphic.color;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (MyPlayer != null)
            {
                MyPlayer.Direction = MMFeedbacks.Directions.TopToBottom;
                MyPlayer.PlayFeedbacks();
            }

            if (TargetGraphic != null)
            {
                TargetGraphic.color = HoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TargetGraphic != null)
            {
                TargetGraphic.color = _originalColor;
            }

        
            if (MyPlayer != null)
            {
                MyPlayer.Direction = MMFeedbacks.Directions.BottomToTop;
                MyPlayer.PlayFeedbacks();
            }
        }
    }
}
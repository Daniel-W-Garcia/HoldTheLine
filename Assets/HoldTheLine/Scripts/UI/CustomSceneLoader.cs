using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace HoldTheLine.Scripts.UI
{
    public class CustomSceneLoader : MMSceneLoadingManager
    {
        [Header("Custom Loading Elements")]
        [Tooltip("The opaque image that will be revealed from left to right")]
        public Image revealImage; // Assign your "RevealImage" object here

        protected override void Update()
        {
            base.Update();

            if (revealImage != null && _progressBarImage != null)
            {
                revealImage.fillAmount = _progressBarImage.fillAmount;
            }
        }
    }
}
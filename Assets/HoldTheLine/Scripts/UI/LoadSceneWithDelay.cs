using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace HoldTheLine.Scripts.UI
{
    public class LoadSceneWithDelay : MMSceneLoadingManager
    {
        [Header("Delay Settings")]
        [Tooltip("Delay in seconds before loading the scene")]
        public int delay = 5;
    
        [Header("Scene Settings")]
        [Tooltip("Name of the scene to load")]
        public string sceneToLoad = "";
    
        [Tooltip("Optional: Name of the loading screen scene (leave empty for default)")]
        public string loadingScreenScene = "";
    

        public void LoadSceneOnWin()
        {
            MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Stop);
            StartCoroutine(LoadSceneWithDelayCoroutine());
        }

        private IEnumerator LoadSceneWithDelayCoroutine()
        {
            yield return new WaitForSeconds(delay);
        
            // Call the static LoadScene method with the scene name
            if (string.IsNullOrEmpty(loadingScreenScene))
            {
                LoadScene(sceneToLoad);
            }
            else
            {
                LoadScene(sceneToLoad, loadingScreenScene);
            }
        }
    }
}
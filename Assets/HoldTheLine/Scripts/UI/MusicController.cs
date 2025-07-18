using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace HoldTheLine.Scripts.UI
{
    [RequireComponent(typeof(MMF_Player))]
    public class MusicController : MonoBehaviour
    {
        public static MusicController Instance { get; private set; }

        [SerializeField]
        public float delay = 0.5f;

        private MMF_Player _musicPlaylistPlayer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _musicPlaylistPlayer = GetComponent<MMF_Player>();
        }

        void Start()
        {
            StartCoroutine(DelayMusicStart(delay));
        }
        private IEnumerator DelayMusicStart(float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayMusic();
        }

       
        public void PlayMusic()
        {
            if (_musicPlaylistPlayer != null)
            {
                _musicPlaylistPlayer.PlayFeedbacks();
            }
        }
        
        public void StopMusic()
        {
            if (_musicPlaylistPlayer != null)
            {
                _musicPlaylistPlayer.StopFeedbacks(true);
            }
        }

        public void SetMusicState(bool isEnabled)
        {
            if (isEnabled)
            {
                PlayMusic();
            }
            else
            {
                StopMusic();
            }
        }
    }
}
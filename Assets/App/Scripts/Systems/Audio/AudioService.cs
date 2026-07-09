using UnityEngine;
using MagicTile.ServiceLocator;

namespace MagicTile.Audio
{
    public class AudioService : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource _audioSource;

        public float Time
        {
            get => _audioSource.time;
            set => _audioSource.time = value;
        }

        public float Pitch
        {
            get => _audioSource.pitch;
            set => _audioSource.pitch = value;
        }

        public float Volume
        {
            get => _audioSource.volume;
            set => _audioSource.volume = value;
        }

        public bool IsPlaying => _audioSource.isPlaying;

        public AudioClip Clip
        {
            get => _audioSource.clip;
            set => _audioSource.clip = value;
        }

        private void Awake()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            ServiceLocator.ServiceLocator.Register(this);
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            ServiceLocator.ServiceLocator.Unregister<AudioService>();
        }

        public void Play() => _audioSource.Play();
        public void Pause() => _audioSource.Pause();
        public void Resume() => _audioSource.UnPause();
        public void Stop() => _audioSource.Stop();
    }
}

using UnityEngine;

public class SimpleSoundPlayer : MonoBehaviour
{
    private AudioSource _audioSource;

    [SerializeField] private bool _playOnAwake = false;
    [SerializeField] private float _startTime = -1.0f;
    [SerializeField] private AudioClip[] _clips;
    [SerializeField] private float _randomPitchMin = 1.0f;
    [SerializeField] private float _randomPitchMax = 1.0f;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _clips[Random.Range(0, _clips.Length)];
        
        if (_audioSource.clip == null)
        {
            Debug.LogWarning("No audio clip found.");
            return;
        }

        if (!_playOnAwake)
        {
            return;
        }

        PlayAudio();
    }

    public void PlayAudio()
    {
        _audioSource.pitch = Random.Range(_randomPitchMin, _randomPitchMax);
        if (_startTime > 0.0f)
        {
            _audioSource.time = _startTime;
        }
        _audioSource.Play();
    }

}

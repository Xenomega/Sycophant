using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
sealed internal class AudioEffect : MonoBehaviour
{
    private AudioSource _audioSource;

    [SerializeField] private Vector2 _audioPitchRange;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
        _audioSource.pitch = UnityEngine.Random.Range(_audioPitchRange.x, _audioPitchRange.y);
        _audioSource.Play();
    }
}

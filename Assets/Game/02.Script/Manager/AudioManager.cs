using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ThreeMatch.Manager
{
    public class AudioManager : MonoBehaviour
    {

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>();
                }

                return _instance;
            }
        }

        private static AudioManager _instance;

        [Serializable]
        public class AudioSourceData
        {
            public string key;
            public AudioClip audioClip;
            public AudioSource audioSource;
        }

        [SerializeField] private List<AudioSourceData> _sfxDataList;
        [SerializeField] private List<AudioSourceData> _bgmDataList;

        private List<AudioSource> _audioSourceList;

        [Button]
        private void AddKey()
        {
            #if UNITY_EDITOR
            foreach (AudioSourceData audioSourceData in _sfxDataList)
            {
                string key = audioSourceData.audioClip.name;
                audioSourceData.key = key;
            }
            
            foreach (AudioSourceData audioSourceData in _bgmDataList)
            {
                audioSourceData.key = "BGM";
            }
            
            EditorUtility.SetDirty(gameObject);
            #endif
        }
        
        private void Start()
        {
            AddAudioSource();
        }

        private void AddAudioSource()
        {
            int count = 5;
            _audioSourceList ??= new List<AudioSource>(5);
            for (int i = 0; i < count; i++)
            {
                var component = gameObject.AddComponent<AudioSource>();
                _audioSourceList.Add(component);
            }
        }

        public void PlaySFX(string key, float volume = 1)
        {
            var audioSourceData = TryGetAudioSourceData(_sfxDataList, key);
            if (audioSourceData == null)
            {
                Debug.LogWarning($"failed play sfx audio {key}");
                return;
            }

            audioSourceData.audioSource.PlayOneShot(audioSourceData.audioClip, volume);
        }

        public void StopSfx(string key)
        {
            bool isSuccess = TryStopAudio(_sfxDataList, key);
            if (!isSuccess)
            {
                Debug.LogWarning($"failed stop sfx audio {key}");
            }
        }

        public void StopBGM(string key)
        {
            bool isSuccess = TryStopAudio(_bgmDataList, key);
            if (!isSuccess)
            {
                Debug.LogWarning($"failed stop bgm audio {key}");
            }
        }

        public void PlayBGM(string key, float volume = 1, bool isLoop = true)
        {
            var audioSource = TryGetAudioSourceData(_bgmDataList, key);
            if (audioSource == null)
            {
                Debug.LogWarning($"failed play bgm audio {key}");
                return;
            }

            audioSource.audioSource.clip = audioSource.audioClip;
            audioSource.audioSource.volume = volume;
            audioSource.audioSource.loop = isLoop;
            audioSource.audioSource.Play();
        }
        
        private AudioSourceData TryGetAudioSourceData(List<AudioSourceData> audioSourceDataList, string key)
        {
            foreach (AudioSourceData data in audioSourceDataList)
            {
                if (data.key == key)
                {
                    var audioSource = _audioSourceList.Find(v => !v.isPlaying);
                    if (audioSource == null)
                    {
                        var component = gameObject.AddComponent<AudioSource>();
                        _audioSourceList.Add(component);
                        audioSource = component;
                    }

                    data.audioSource = audioSource;
                    return data;
                }
            }

            return null;
        }

        private IEnumerator DOFadeStopAudioCor(AudioSource audioSource, bool isFadeOut, float fadeDuration)
        {
            yield return audioSource.DOFade(isFadeOut ? 0 : 1, fadeDuration).WaitForCompletion();
            
            audioSource.Stop();
        }

        private bool TryStopAudio(List<AudioSourceData> audioSourceDataList, string key, bool fade = false)
        {
            foreach (AudioSourceData data in audioSourceDataList)
            {
                if (data.key == key && data.audioSource != null)
                {
                    if (fade)
                    {
                        StartCoroutine(DOFadeStopAudioCor(data.audioSource, true, 0.5f));
                    }
                    else
                    {
                        data.audioSource.Stop();
                    }
                    data.audioSource = null;
                    return true;
                }
            }

            return false;
        }
    }
}
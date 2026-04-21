using ArquipelagoPerdidoRPG.Core;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.Systems
{
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null)
            {
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }
    }
}

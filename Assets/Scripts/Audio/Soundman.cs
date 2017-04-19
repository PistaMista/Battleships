using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soundman : MonoBehaviour
{
    //Values accessed by the editor.
    //The music used in the game.
    public AudioClip[] defaultMusic;
    //The in-world sounds used.
    public GameObject[] defaultSoundFX;
    //The UI sounds used.
    public AudioClip[] defaultUISFX;
    //The object used to play music.
    public GameObject defaultMusicPlayer;
    //The object used to play UI sounds.
    public GameObject defaultUISFXPlayer;

    //Values accessed by code.
    /// <summary>
    /// All the music usable.
    /// </summary>
    public static AudioClip[] music;
    /// <summary>
    /// All the in-world sounds usable.
    /// </summary>
    public static GameObject[] soundFX;
    /// <summary>
    /// All the UI sounds usable.
    /// </summary>
    public static AudioClip[] UISFX;
    /// <summary>
    /// The object used to play music.
    /// </summary>
    public static GameObject musicPlayer;
    /// <summary>
    /// /// The object used to play UI sounds.
    /// </summary>
    public static GameObject UISFXPlayer;

    /// <summary>
    /// The awake function.
    /// </summary>
    public void Awake()
    {
        music = defaultMusic;
        soundFX = defaultSoundFX;
        UISFX = defaultUISFX;
        musicPlayer = defaultMusicPlayer;
        UISFXPlayer = defaultUISFXPlayer;
    }
    /// <summary>
    /// The rate of change in volume.
    /// </summary>
    float uphillVolumeChange;
    /// <summary>
    /// The rate of change in volume.
    /// </summary>
    float downhillVolumeChange;
    /// <summary>
    /// The update function.
    /// </summary>
    public void Update()
    {
        if (primaryMusicPlayer != null)
        {
            primaryMusicPlayer.volume = Mathf.SmoothDamp(primaryMusicPlayer.volume, 1f, ref uphillVolumeChange, 1f);
        }

        if (secondaryMusicPlayer != null)
        {
            secondaryMusicPlayer.volume = Mathf.SmoothDamp(secondaryMusicPlayer.volume, 0f, ref downhillVolumeChange, 1f);
        }

        //Debug.Log(musicState);
    }
    //Music management
    enum MusicState
    {
        PLAYING,
        SWITCHING
    }
    /// <summary>
    /// The primary music player.
    /// </summary>
    static AudioSource primaryMusicPlayer;
    /// <summary>
    /// The secondary music player.
    /// </summary>
    static AudioSource secondaryMusicPlayer;
    /// <summary>
    /// The state of the music.
    /// </summary>
    static MusicState musicState;
    /// <summary>
    /// Changes the currently played track.
    /// </summary>
    public static void ChangeTrack(int id, bool smoothSwitch, bool repeat)
    {
        bool primaryPlaying = false;
        if (primaryMusicPlayer != null)
        {
            primaryPlaying = primaryMusicPlayer.isPlaying;
        }

        if (secondaryMusicPlayer != null)
        {
            Destroy(secondaryMusicPlayer.gameObject);
        }

        if (primaryPlaying && smoothSwitch)
        {
            secondaryMusicPlayer = primaryMusicPlayer;
            primaryMusicPlayer = null;

        }

        if (primaryMusicPlayer != null)
        {
            Destroy(primaryMusicPlayer.gameObject);
        }

        if (!(id < 0 || id >= music.Length))
        {
            primaryMusicPlayer = Instantiate(musicPlayer).GetComponent<AudioSource>();
            if (smoothSwitch)
            {
                primaryMusicPlayer.volume = 0;
                musicState = MusicState.SWITCHING;
            }
            primaryMusicPlayer.clip = music[id];
            primaryMusicPlayer.Play();
            primaryMusicPlayer.loop = repeat;
        }
    }

}

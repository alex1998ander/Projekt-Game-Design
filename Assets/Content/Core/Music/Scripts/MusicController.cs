using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Audio sources for the intro and  the main loop
    [SerializeField] private AudioSource intro, mainLoop;

    // Audio sources for the variants of the upgrade selection loop
    [SerializeField] private AudioSource[] upgradeSelectionLoops;

    // Time in seconds it takes to fully fade from the main loop to the upgrade selection loop and vice versa
    [SerializeField] private float loopCrossFadeTimeInSeconds = 1f;

    // Is the main loop currently playing?
    private static bool _mainLoopPlaying = true;

    // Index of the upgrade selection loop currently being used
    private static int _currentUpgradeSelectionLoopIdx = 0;

    // Fade Coroutines to stop
    private static readonly Coroutine[] _fadeCoroutines = { };

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        EventManager.OnLevelEnter.Subscribe(InitializeMusicFadeOnSceneChange);
        EventManager.OnLevelExit.Subscribe(InitializeMusicFadeOnSceneChange);

        // Play the intro, schedule the main und upgrade loops to start playing at the point in time where
        // the intro stops playing to make sure they are synchronized
        // This assumes that the player will never finish a level before the intro has finished playing
        intro.PlayScheduled(AudioSettings.dspTime);
        double mainAndUpgradeLoopStartTime = AudioSettings.dspTime + intro.clip.length;
        mainLoop.PlayScheduled(mainAndUpgradeLoopStartTime);
        foreach (AudioSource upgradeSelectionLoop in upgradeSelectionLoops)
        {
            upgradeSelectionLoop.volume = 0f;
            upgradeSelectionLoop.PlayScheduled(mainAndUpgradeLoopStartTime);
        }
    }

    /// <summary>
    /// Starts a volume fade on the given audio source after a certain delay.
    /// </summary>
    /// <param name="audioSource">The audio to fade</param>
    /// <param name="duration">The duration of the fade</param>
    /// <param name="targetVolume">The target volume the audio volume fades to</param>
    public static IEnumerator StartVolumeFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0f;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
    }

    /// <summary>
    /// Initializes the fade of the music when the scene is changed.
    /// Happens when level was exited and upgrade selection screen is entered or
    /// upgrade selection screen was exited and new level was entered
    /// </summary>
    private void InitializeMusicFadeOnSceneChange()
    {
        foreach (Coroutine coroutine in _fadeCoroutines)
        {
            StopCoroutine(coroutine);
        }

        Array.Clear(_fadeCoroutines, 0, _fadeCoroutines.Length);

        // TODO: When player chooses upgrade too fast, the different coroutines clash and destroy the fading effect
        if (_mainLoopPlaying)
        {
            _fadeCoroutines.Append(StartCoroutine(StartVolumeFade(mainLoop, loopCrossFadeTimeInSeconds, 0f)));
            _fadeCoroutines.Append(
                StartCoroutine(StartVolumeFade(upgradeSelectionLoops[_currentUpgradeSelectionLoopIdx],
                    loopCrossFadeTimeInSeconds, 1f)));
        }
        else
        {
            _fadeCoroutines.Append(
                StartCoroutine(StartVolumeFade(upgradeSelectionLoops[_currentUpgradeSelectionLoopIdx],
                    loopCrossFadeTimeInSeconds, 0f)));
            _fadeCoroutines.Append(StartCoroutine(StartVolumeFade(mainLoop, loopCrossFadeTimeInSeconds, 1f)));

            _currentUpgradeSelectionLoopIdx = (_currentUpgradeSelectionLoopIdx + 1) % upgradeSelectionLoops.Length;
        }

        _mainLoopPlaying = !_mainLoopPlaying;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioScript : MonoBehaviour
{
    public GameObject ambientButton;
    public AudioSource spinSound;
    public AudioSource[] stopSound = new AudioSource[5],
        winSound = new AudioSource[5], coinSound = new AudioSource[10],
        errorSound = new AudioSource[3], confirmSound = new AudioSource[5];

    public int soundActive = 1;

    private int internalCoinCount = 0, internalErrorCount = 0, internalConfirmCount = 0;

    AudioSource ambient;

    // Start is called before the first frame update
    void Start()
    {
        ambient = GetComponent<AudioSource>();

        soundActive = PlayerPrefs.GetInt("SoundEnabled", 1);
    }

    public void PlayPauseAmbient()
    {

        Color c = ambientButton.transform.GetChild(0).GetComponent<Image>().color;

        if (ambient.isPlaying)
        {
            soundActive = 0;

            c.a = 0f;
            ambient.Pause();
            spinSound.Stop();
            StopCoinSound();
        } else
        {
            soundActive = 1;

            c.a = 1f;
            ambient.Play();
        }

        ambientButton.transform.GetChild(0).GetComponent<Image>().color = c;
        PlayerPrefs.SetInt("SoundEnabled", soundActive);
    }

    public void BackStopAmbient()
    {
        if(soundActive == 1)
        {
            ambient.Pause();
        }
    }

    public void BeginPlayAmbient()
    {
        if (soundActive == 1)
        {
            Color c = ambientButton.transform.GetChild(0).GetComponent<Image>().color;
            c.a = 1f;

            ambientButton.transform.GetChild(0).GetComponent<Image>().color = c;
            ambient.Play();
        }
    }

    public void PlayStopSpin()
    {
        if(soundActive == 0)
        {
            return;
        }

        if(spinSound.isPlaying)
        {
            spinSound.Stop();
        } else
        {
            spinSound.Play();
        }
    }

    public void PlayStopSound(int pos)
    {
        if (soundActive == 0)
        {
            return;
        }

        stopSound[pos].Play();
    }

    public void PlayWinSound(int pos)
    {
        if (soundActive == 0)
        {
            return;
        }

        winSound[pos].Play();
    }

    public void PlayCoinSound()
    {
        if (soundActive == 0)
        {
            return;
        }

        coinSound[internalCoinCount].Play();

        internalCoinCount++;
        if(internalCoinCount > 9)
        {
            internalCoinCount = 0;
        }
    }

    public void StopCoinSound()
    {
        for(int i = 0; i < 10; i++)
        {
            coinSound[i].Stop();
        }
    }

    public void PlayErrorSound()
    {
        if (soundActive == 0)
        {
            return;
        }

        errorSound[internalErrorCount].Play();

        internalErrorCount++;
        if (internalErrorCount > 2)
        {
            internalErrorCount = 0;
        }
    }

    public void PlayConfirmSound()
    {
        if (soundActive == 0)
        {
            return;
        }

        confirmSound[internalConfirmCount].Play();

        if(internalConfirmCount > 4)
        {
            internalConfirmCount = 0;
        }
    }
}

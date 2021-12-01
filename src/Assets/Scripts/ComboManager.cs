using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DigitalRuby.Tween;

public class ComboManager : MonoBehaviour
{
    public TextMeshPro comboText;
    public SpriteRenderer comboBar;
    public SpriteMask spriteMask;
    public AudioSource comboSound;

    public Tween<float> comboAnimation;

    public int combo = 0;
    public float comboTime = 0;
    public float displayComboTime = 0;
    public float scale = 0;
    public float scaleMult = 1;
    public Color comboColor = Manager.COMBO_COLORS[0];

    // Update is called once per frame
    void Update()
    {
        if (combo > 0)
        {
            comboTime -= Time.deltaTime;

            if (comboTime <= 0)
            {
                comboTime = 0;
                combo = 0;
            }
            else
            {
                float comboColorLerp = 1.0f - Mathf.Pow(1.0f - 0.95f, Time.deltaTime);
                comboColor = comboColor * (1.0f - comboColorLerp) + Manager.instance.getComboColor(combo) * comboColorLerp;
                comboText.text = "Combo x" + string.Format("{0:#,##0.##}", combo);
                comboText.color = comboColor;
                comboBar.color = comboColor;
            }
        }

        float displayComboTimeLerp = 1.0f - Mathf.Pow(1.0f - 0.996f, Time.deltaTime);
        displayComboTime = displayComboTime * (1.0f - displayComboTimeLerp) + comboTime * displayComboTimeLerp;

        float scaleLerp = 1.0f - Mathf.Pow(1.0f - 0.998f, Time.deltaTime);
        scale *= 1.0f - scaleLerp;
        if (combo > 0)
        {
            scale += scaleLerp;
        }

        spriteMask.alphaCutoff = 1f - displayComboTime / getComboTime(combo);
        transform.localScale = new Vector3(scale * scaleMult, scale * scaleMult, scale * scaleMult);
    }

    public void addCombo(int comboAdd)
    {
        if (combo == 0)
        {
            comboColor = Manager.COMBO_COLORS[0];
        }
        else
        {
            float scl = 1.05f;
            if (combo + 1 >= 20)
            {
                if ((combo + 1) % 10 == 0)
                {
                    scl = 1.06f;
                }

                if ((combo + 1) % 25 == 0)
                {
                    scl = 1.07f;
                }

                if ((combo + 1) % 50 == 0)
                {
                    scl = 1.08f;
                }

                if ((combo + 1) % 100 == 0)
                {
                    scl = 1.09f;
                }

                if ((combo + 1) % 250 == 0)
                {
                    scl = 1.1f;
                }
            }

            comboAnimation = TweenFactory.Tween(null, scl, 1.0f, 0.2f, TweenScaleFunctions.QuadraticEaseIn, t =>
            {
                scaleMult = t.CurrentValue;
            });
        }

        combo += comboAdd;
        comboTime = getComboTime(combo);
        playComboSound(combo);
    }

    public float getComboTime(int combo)
    {
        return Manager.instance.gameMode.getComboTime(combo);
    }

    public float getComboMultiplier()
    {
        return Manager.instance.gameMode.getComboMultiplier(combo);
    }

    public void playComboSound(int combo)
    {
        if (combo < 5)
        {
            return;
        }


        float pitch = (combo - 5) * 0.1f + 0.5f;
        if (pitch > 2f)
        {
            pitch = 2f;
        }

        float volume = (combo - 5) * 0.03f + 0.03f;
        if (volume > 0.15f)
        {
            volume = 0.15f;
        }

        if (combo >= 20)
        {
            if (combo % 10 == 0)
            {
                pitch = 2.5f;
                volume = 0.2f;
            }

            if (combo % 25 == 0)
            {
                pitch = 3f;
                volume = 0.25f;
            }

            if (combo % 50 == 0)
            {
                pitch = 3.5f;
                volume = 0.3f;
            }

            if (combo % 100 == 0)
            {
                pitch = 4f;
                volume = 0.35f;
            }

            if (combo % 250 == 0)
            {
                pitch = 4.5f;
                volume = 0.4f;
            }
        }

        comboSound.volume = volume;
        comboSound.pitch = pitch;
        comboSound.time = 0.1f / pitch;
        comboSound.Play();
    }
}
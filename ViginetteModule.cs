using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using UnityEngine.Rendering;

[Serializable]
public class ViginetteModule : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] private bool enabled = false;
    [SerializeField] private float lowHPLevel = 50;
    [SerializeField] private float speed = 1f;
    [SerializeField] private AnimationCurve beatAnimation;
    [SerializeField] Color color;
    #endregion

    #region Internal Variables
    private Vignette vignette;
    private float initialIntensity;
    private Volume volume;
    private bool beating;
    private float beatTimer;
    ColorParameter originalColor;
    #endregion

    #region Methods
    public void Start()
    {
        if (!volume) volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
        initialIntensity = vignette.intensity.value;
        GameManager.Instance.OnPlayerChanged += ActorChanged;
        //beating = true; // for testing the curve feeling.
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;      
    }
    private void ActorChanged(Actor newActor)
    {
        if(newActor == null) SetBeating(false);
        else newActor.Player.LifeModule.OnLifeChanged += LifeModule_OnLifeChanged;
    }

    private void LifeModule_OnLifeChanged(float arg1, float arg2)
    {
        SetBeating(arg1 < lowHPLevel);
    }
    private void SetBeating(bool b)
    {
        beating = b;
        vignette.intensity.value = initialIntensity;
        vignette.color.value = b ? color:Color.black;
        beatTimer = 0;
    }
    private void Update()
    {

        if (!beating || !enabled) return;
        beatTimer += Time.deltaTime * speed;
        if (beatTimer > 1) beatTimer = 0;
        vignette.intensity.value = initialIntensity + (beatAnimation.Evaluate(beatTimer));
        print(initialIntensity + (beatAnimation.Evaluate(beatTimer)));

    }
    #endregion

}

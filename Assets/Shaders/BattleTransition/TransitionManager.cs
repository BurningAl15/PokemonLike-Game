using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{

    public static TransitionManager _instance;
    
    [SerializeField] Material material;
    [SerializeField] private AnimationCurve _animationCurve;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public IEnumerator TransitionEffect_FadeOut (float _delay=.5f) {
        material.SetFloat ("_Cutoff", _animationCurve.Evaluate(1));

        for (float i = _delay; i > 0; i -= Time.fixedDeltaTime)
        {
            float animValue = _animationCurve.Evaluate(i / _delay);
            material.SetFloat ("_Cutoff", animValue);
            yield return null;
        }
        material.SetFloat ("_Cutoff", _animationCurve.Evaluate(0));
        yield return null;

    }

    public IEnumerator TransitionEffect_FadeIn (float _delay=.5f) {
        material.SetFloat ("_Cutoff", _animationCurve.Evaluate(0));

        for (float i = 0; i < _delay; i += Time.fixedDeltaTime) {
            float animValue = _animationCurve.Evaluate(i / _delay);
            material.SetFloat ("_Cutoff", animValue);
            yield return null;
        }
        material.SetFloat ("_Cutoff", _animationCurve.Evaluate(1));
        yield return null;
    }
}
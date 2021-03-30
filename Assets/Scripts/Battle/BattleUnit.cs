using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private PokemonBase _base;
    [SerializeField] private int level;
    [SerializeField] private bool isPlayerUnit;
    [SerializeField] private Image pokemonImage;

    public bool endAnimation = false;
    
    public  Pokemon Pokemon { get; set; }

    private Vector3 originalPos;

    private Color originalColor;
    
    private void Awake()
    {
        originalPos = pokemonImage.transform.localPosition;
        originalColor = pokemonImage.color;
    }

    public void Setup()
    {
        print("Calling");
        Pokemon = new Pokemon(_base, level);
        pokemonImage.sprite = isPlayerUnit ? _base.BackSprite : _base.FrontSprite;

        PlayerEnterAnimation();
    }

    public void PlayerEnterAnimation()
    {
        if (isPlayerUnit)
            pokemonImage.transform.localPosition = new Vector3(-500f, originalPos.y);
        else
            pokemonImage.transform.localPosition = new Vector3(500f, originalPos.y);

        pokemonImage.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public IEnumerator PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(pokemonImage.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequence.Append(pokemonImage.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));

        sequence.Append(pokemonImage.transform.DOLocalMoveX(originalPos.x, .2f));
        yield return null;
    }

    public IEnumerator PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(pokemonImage.DOColor(Color.gray, .1f));
        sequence.Append(pokemonImage.DOColor(originalColor, .1f));
        yield return null;
    }
    
    public IEnumerator PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(pokemonImage.transform.DOLocalMoveY(originalPos.y - 150f, .5f));
        sequence.Join(pokemonImage.DOFade(0f, .5f));
        print("Fainting");
        yield return new WaitForSeconds(1.25f);
        endAnimation = true;
    }
}

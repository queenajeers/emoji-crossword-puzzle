using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public DOTweenAnimation levelHeaderAnimation;
    public DOTweenAnimation draggableLettersBG;

    public GameObject coinsBg;
    Animator coinsBGAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        coinsBGAnimator = coinsBg.GetComponent<Animator>();
    }

    // Update is called once per frame
    public void OutLevelHeaderInCoins()
    {
        levelHeaderAnimation.DOPlay();
        draggableLettersBG.DOPlay();
        coinsBg.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            levelHeaderAnimation.DORewind();
        }
    }

    public void OnCoinReached()
    {
        coinsBGAnimator.Play("CoinCollect", 0, 0);
    }

    public void OnAllCoinsFinished()
    {
        coinsBGAnimator.Play("Reverse", 0, 0);
    }

}

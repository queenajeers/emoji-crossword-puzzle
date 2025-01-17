using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public DOTweenAnimation levelHeaderAnimation;
    public DOTweenAnimation draggableLettersBG;

    public GameObject coinsBg;
    [SerializeField] GameObject confetti;
    Animator coinsBGAnimator;

    public GameObject LevelFinishedPage;

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

    public void ActivateConfetti()
    {
        confetti.SetActive(true);
    }

    public void OnCoinReached()
    {
        coinsBGAnimator.Play("CoinCollect", 0, 0);
    }

    public void OnAllCoinsFinished()
    {
        coinsBGAnimator.Play("Reverse", 0, 0);
        LevelFinishedPage.SetActive(true);
    }

    public void RestartScene()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadSceneAsync(0);

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [Header("----------[ Object Pooling ]")]
    public GameObject donglePrefeb;
    public Transform dongleGroup;

    public GameObject effectPrefeb;
    public Transform effectGroup;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;

    public List<Dongle> donglePool;
    public List<ParticleSystem> effectPool;
    public Dongle lastDongle;

    [Header("----------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over};
    int sfxCursor;


    [Header("----------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text MaxScoreText;
    public Text subScoreText;


    [Header("----------[ core ]")]
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("----------[ ETC ]")]
    public GameObject Line;
    public GameObject Bottom;

    public static GameManager instanse;

    private void Awake()
    {

        instanse = this;
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }

        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore",0);
        }

        MaxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart()
    {
        Line.SetActive(true);
        Bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        MaxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);



        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        Invoke("NextDongle", 1f);
        
    }

    Dongle MakeDongle()
    {
        GameObject instantEffectObj = Instantiate(effectPrefeb, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantDongleObj = Instantiate(donglePrefeb, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);
        
        return instantDongle;
    }

    Dongle GetDongle()
    {
        for(int i = 0; i< donglePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf)
                return donglePool[poolCursor];
            
        }

        return MakeDongle();
    }

    void NextDongle()
    {
        if (isOver)
            return;
        
        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext()
    {
        while (lastDongle != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        NextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    {
        if (isOver)
            return;
        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {

        Dongle[] dongles = FindObjectsOfType<Dongle>();

        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].setRigidSimul();
        }

        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        //점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        subScoreText.text = "점수 : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);

        StartCoroutine("ResetCoroutine");

    }
    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("Main");
    }

    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
         
            case Sfx.Next:
                {
                    sfxPlayer[sfxCursor].clip = sfxClip[3];
                    break;
                }
            case Sfx.Attach:
                {
                    sfxPlayer[sfxCursor].clip = sfxClip[4];
                    break;
                }

            case Sfx.LevelUp:
                {
                    sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                    break;
                }

            case Sfx.Button:
                {
                    sfxPlayer[sfxCursor].clip = sfxClip[5];
                    break;
                }
            case Sfx.Over:
                {
                    sfxPlayer[sfxCursor].clip = sfxClip[6];
                    break;
                }
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void asdf() 
    {

    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}

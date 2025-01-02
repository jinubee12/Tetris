using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Title : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _likeALionText;
    [SerializeField] private TextMeshProUGUI _unityText;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _pressEnterText;

    [SerializeField] private AudioClip _beepSound;
    private AudioSource _audioSource;
    
    public GameObject GameScene;
    public GameObject TetrisManager;
    
    

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        StartCoroutine(TitleSequence());
    }
    IEnumerator TitleSequence()
    {
        yield return new WaitForSeconds(1f);
        _likeALionText.gameObject.SetActive(true);
        SoundManager.instance.PlayClip(_beepSound,_audioSource);
        yield return new WaitForSeconds(3f);
        _likeALionText.gameObject.SetActive(false);
        SoundManager.instance.PlayClip(_beepSound,_audioSource);
        
        yield return new WaitForSeconds(2f);
        
        _unityText.gameObject.SetActive(true);
        SoundManager.instance.PlayClip(_beepSound,_audioSource);
        yield return new WaitForSeconds(3f);
        _unityText.gameObject.SetActive(false);
        SoundManager.instance.PlayClip(_beepSound,_audioSource);
        
        yield return new WaitForSeconds(2f);
        
        _titleText.gameObject.SetActive(true);
        _pressEnterText.gameObject.SetActive(true);
        SoundManager.instance.PlayClip(_beepSound,_audioSource);
        yield return StartCoroutine(BlinkPressEnter(_pressEnterText, 0.5f));
        
        TetrisManager.gameObject.SetActive(true);
        GameScene.SetActive(true);
        SoundManager.instance.PlayClip(_beepSound,_audioSource);
        
    }

    IEnumerator BlinkPressEnter(TextMeshProUGUI text, float interval)
    {
        bool isVisible = true;

        while (!Input.GetKeyDown(KeyCode.Return))
        {
            text.gameObject.SetActive(isVisible);
            isVisible = !isVisible;
            
            yield return new WaitForSeconds(interval);
            
        }
        SoundManager.instance.PlayClip(_beepSound,_audioSource);
        _pressEnterText.gameObject.SetActive(false);
        _titleText.gameObject.SetActive(false);
    }
}

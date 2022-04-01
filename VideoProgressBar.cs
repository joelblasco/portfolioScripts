using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    private VideoPlayer video;

    private Image progress;
    public Animator anim;

    void Start()
    {
        progress = GetComponent<Image>();
        video.url = PlayerPrefs.GetString("videourl");
        video.Prepare();
        video.Play();
        Invoke("DisableAnim", 3);
    }

    // Update is called once per frame
    void Update()
    {
        if (video.frameCount > 0)
        {
            progress.fillAmount = (float)video.frame / (float)video.frameCount;
        }
        if (Input.touchCount > 0 || Input.anyKeyDown)
        {
            anim.SetBool("disabled", false);
            CancelInvoke("DisaleAnim");
            Invoke("DisableAnim", 3);
        }
        
    }
    void DisableAnim()
    {
        anim.SetBool("disabled", true);
    }
    public void OnDrag(PointerEventData eventData)
    {
        TrySkip(eventData);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        TrySkip(eventData);
    }
    private void TrySkip(PointerEventData eventData)
    {
        Vector2 localPoint;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(progress.rectTransform,eventData.position,null,out localPoint))
        {
            float percent = Mathf.InverseLerp(progress.rectTransform.rect.xMin, progress.rectTransform.rect.xMax, localPoint.x);
            SkipToPercent(percent);

        }
    }
    private void SkipToPercent(float percent)
    {
        var frame = video.frameCount * percent;
        video.frame = (long)frame;
    }
    public void goMainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Principal");
    }
}

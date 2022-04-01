using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    public int game, diffIndex;
    public bool playing;
    public Transform insImage;
    Vector2 size;
    public Transform partsContainer;
    public GridLayoutGroup destinations, origins;
    public Image[] gParts;
    public Transform[] gOrigins, gDestinations;


    //difficulty
    public Image[] buttonsBg;
    public Sprite[] backgrounds;
    public Image background;
    private Arrastrar3 util;
    public int[] difficulty;

    IEnumerator mixing;
    IEnumerator old_mixing;

    int old_r1 = 10;
    int r1 = 20;

    public void setDifficulty(int d)
    {
        playing = false;
        initGame(d);
        if (mixing != null)
        { 
            old_mixing = mixing;
            StopCoroutine(old_mixing);
        }
        mixing = mixParts();
       
        StartCoroutine(mixing);
    }
    private void Start()
    {
        gParts = partsContainer.GetComponentsInChildren<Image>();
        gOrigins = origins.transform.GetComponentsInChildren<Transform>();
        gDestinations = destinations.transform.GetComponentsInChildren<Transform>();
        util = GetComponent<Arrastrar3>();
        myAudio = GetComponent<AudioSource>();

        //eliminar el primer transform de cada array ya que coge el objeto padre.
        List<Transform> g = gOrigins.ToList();
        g.RemoveAt(0);
        gOrigins = g.ToArray();
        g = gDestinations.ToList();
        g.RemoveAt(0);
        gDestinations = g.ToArray();

        initGame(0);

         insImage.gameObject.SetActive(true);

        if (PlayerPrefs.GetInt("mute") == 1) toggleMute();
    }
    public void initGame(int dif)
    {
        countdown = false;
        diffIndex = dif;

        timeLeft = difficulty[diffIndex] < 8 ? 60 : 120;
        TimeSpan t = TimeSpan.FromSeconds(timeLeft);
        timerInfo.text = t.ToString(@"mm\:ss");

        util.placed = new bool[difficulty[diffIndex]];
        //desactivar y activar los elementos (imagenes,origenes y destinaciones) necesarios
        for (int i = 0; i < 16; i++)
        {
            gParts[i].gameObject.SetActive(false);
            gOrigins[i].gameObject.SetActive(false);
            gDestinations[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < difficulty[diffIndex]; i++)
        {
            gParts[i].gameObject.SetActive(true);
            gOrigins[i].gameObject.SetActive(true);
            gDestinations[i].gameObject.SetActive(true);
            gParts[i].GetComponent<DragDropEasy>().puzzle = true;
            gParts[i].GetComponent<DragDropEasy>().locked = false;
            gParts[i].GetComponent<DragDropEasy>().moving = true;

        }
       
        //establecer columnas y filas
        //print("ImagenesPuzzle/cuento " + (PlayerPrefs.GetInt("book") + 1) + " Puzzle " + (PlayerPrefs.GetInt("game")+1));
        Texture2D tex = Resources.Load<Sprite>("ImagenesPuzzle/cuento " + (PlayerPrefs.GetInt("book")+1) + " Puzzle " + (PlayerPrefs.GetInt("game")+1)).texture;        
        zoomedImage.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f, 0.5f));
        zoomedImage2.sprite = zoomedImage.sprite;
        //zoomedImage.gameObject.SetActive(false);
        int rows = 0;
        int columns = 0;

        foreach(Image i in buttonsBg)
        {
            i.color = new Color(0, .56f, .8f);
        }
        Color selectedColor = new Color(0, .25f, .35f);
        switch (diffIndex)
        {
            default: //necesitaremos dos filas y dos columnas
                rows = 2;
                columns = 2;
                buttonsBg[0].color = selectedColor;
                background.sprite = backgrounds[0];
                destinations.startAxis = GridLayoutGroup.Axis.Vertical;
                origins.startAxis = GridLayoutGroup.Axis.Vertical;
                break;               
            case 1:                 
                rows = 2;           
                columns = 3;        
                buttonsBg[1].color = selectedColor;
                background.sprite = backgrounds[1];
                break;           
            case 2:                  
                rows = 3;           
                columns = 3;        
                buttonsBg[2].color = selectedColor;
                background.sprite = backgrounds[2];
                break;           
            case 3:               
                rows = 4;            
                columns = 3;         
                buttonsBg[3].color = selectedColor;
                background.sprite = backgrounds[3];
                break;               
            case 4:                 
                rows = 4;            
                columns = 4;         
                buttonsBg[4].color = selectedColor;
                background.sprite = backgrounds[4];
                break;
        }
        //el tamaño de cada parte será el total de la texura entre las columnas/filas
        size.x = tex.width / columns; 
        size.y = tex.height / rows;
        float x = (partsContainer.GetComponent<RectTransform>().sizeDelta.x-(5*columns)) / columns;
        float y = (partsContainer.GetComponent<RectTransform>().sizeDelta.y-(5*rows)) / rows;
        Vector2 partsSize = new Vector2(x, y);
        //asignar a cada parte un trozo
        int partCounter = 0;
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Sprite newSprite = Sprite.Create(tex, new Rect(i * size.x, j * size.y, size.x, size.y), new Vector2(0.5f, 0.5f)); //creacion de la imagen
                newSprite.name = "part_"+partCounter;
                gParts[partCounter].sprite = newSprite; //sprite de la imagen
                gParts[partCounter].rectTransform.sizeDelta = partsSize; //tamaño de la imagen
                partCounter++;
            }
        }
        //establecer cellSize del layout grup
        destinations.cellSize = partsSize;
        origins.cellSize = partsSize;
        util.initGame(gDestinations, difficulty[diffIndex]);

    }

   IEnumerator mixParts()
    {
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < difficulty[diffIndex] * 0.5; i++)
        {
            //En cada recorrido siempre habra un cambio de piezas
            do
            {
                r1 = UnityEngine.Random.Range(0, difficulty[diffIndex]);
            } while (old_r1 == r1);
            old_r1 = r1;

            gOrigins[r1].SetAsFirstSibling();
            gParts[r1].transform.SetAsFirstSibling();

            yield return new WaitForSeconds(1.0f);
        }
        startTimer();
    }

    public Text timerInfo;
    public float timeLeft;
    private void startTimer()
    {
        int counter = 0;
        foreach(Image part in gParts) //establecer las partes del puzzle inicialmente
        {
            part.GetComponent<DragDropEasy>().manager1 = util;
            part.GetComponent<DragDropEasy>().moving = true;
            part.GetComponent<DragDropEasy>().index = counter;
            part.GetComponent<DragDropEasy>().locked = false;
            //part.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            //part.GetComponent<SpringJoint2D>().enabled = true;
            counter++;
        }
        util.unlockImages(); //desbloquear las partes
        playing = true;
    }
    private AudioSource myAudio;
    public AudioClip ticSound, gongSound;
    private bool countdown;
    void Update()
    {
        if (playing)
        {
            timeLeft -= Time.deltaTime;
            TimeSpan t = TimeSpan.FromSeconds(timeLeft);
            timerInfo.text = t.ToString(@"mm\:ss");
            if (timeLeft < 11 && !countdown)
            {
                countdown = true;
                StartCoroutine(playTic());
            }
            if (timeLeft <= 0)
            {
                myAudio.PlayOneShot(gongSound);
                setDifficulty(diffIndex);
            }
        }
        else
        {
            foreach (Image part in gParts) //las partes estan bloqueadas inicialmente
            {
                part.GetComponent<DragDropEasy>().locked = true;
            }
            for (int i = 0; i < difficulty[diffIndex]; i++)
            {
                float dist = Vector2.Distance(gParts[i].GetComponent<RectTransform>().position, gOrigins[i].transform.position);
                gParts[i].GetComponent<RectTransform>().transform.position = Vector2.MoveTowards(gParts[i].GetComponent<RectTransform>().position,
                                   gOrigins[i].transform.position,
                                   (200+(dist*5)) * Time.deltaTime);
            }

            if ((Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0) && insImage.gameObject.activeSelf && Time.timeSinceLevelLoad > 1f)
            {
                insImage.gameObject.SetActive(false);
                setDifficulty(0);
            }

        }
       
    }

    IEnumerator playTic()
    {
        if (!countdown) yield break;
            myAudio.PlayOneShot(ticSound);
            yield return new WaitForSeconds(1);
            StartCoroutine(playTic());
    }

    public Image zoomedImage, zoomedImage2;
    public GameObject panelZoom;
    public void showImage()
    {
        panelZoom.SetActive(true);
        zoomedImage.GetComponent<Animator>().Play("zoomImagePuzzle");
        StartCoroutine(hideImage());
    }
    IEnumerator hideImage()
    {
        yield return new WaitForSeconds(3);
        panelZoom.SetActive(false);
    }

    public void returnMain()
    {
        //volver a escena principal
        PlayerPrefs.SetString("prevScene", "Principal");
        SceneManager.LoadScene("Principal");
    }
    public void returnGames()
    {
        SceneManager.LoadScene("Principal");
        //volver a actividades
    }
    public void resetGame()
    {
        //recargar escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("prevScene", "Principal");
    }
    public AudioSource music;
    public Image buttonMute;
    public void toggleMute()
    {
        //cambiar sonido on/off
        if (music.mute)
        {
            music.mute = false;
            buttonMute.color = Color.white;
        }
        else
        {
            music.mute = true;
            buttonMute.color = Color.gray;
        }
    }
}

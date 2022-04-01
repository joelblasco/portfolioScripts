using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SopaLetrasManager : MonoBehaviour
{
    public int difficulty;
    public gamesClass[] games;
    [System.Serializable]
    public class gamesClass
    {
        public string[] wordsEasy;
        public string[] wordsMedium;
        public string[] wordsHard;
        public Sprite[] difficultyWords;
    }
    public Text[] wordsToFind;
    public letterUtil[] letters;
    public GridLayoutGroup lettersGroup;
    public string[,] finalBoard;
    private int[] direction;
    private int gridSize, totalWordsLength, timesCreated, wins, totalWords;
    public string abc;
    public bool[] wordsFound;
    private bool playing = false, sentInput = false;

    public GameObject difPanel, gamePanel, insPanel;
    public Image difficultyWordsDisplay;

    private void Start()
    {
        insPanel.SetActive(false);
        difPanel.SetActive(true);
        gamePanel.SetActive(false);
        winImage.SetActive(false);
        //resetButton.SetActive(false);
        //resetButtonLine.SetActive(false);


        if (PlayerPrefs.GetInt("mute") == 1) toggleMute();
    }
    IEnumerator createAgain()
    {
        playing = false;
        yield return new WaitForSeconds(0.1f);
        setDifficulty(difficulty);
    }
    public void setDifficulty(int dif)
    {
        //cerrar panel eleccion dificultad
        difPanel.SetActive(false);
        gamePanel.SetActive(false);
        insPanel.SetActive(true);
        //resetButton.SetActive(true);
        //resetButtonLine.SetActive(true);
        //homeButton.SetActive(false);
        //homeButtonLine.SetActive(false);

        Debug.ClearDeveloperConsole();
        timesCreated++;
        print(timesCreated);

        difficulty = dif;
        int book = PlayerPrefs.GetInt("book");
        int game = PlayerPrefs.GetInt("game");
        game = Random.Range(0, 2);
        //game = 0;

        string[] words = { "", "" };

        difficultyWordsDisplay.sprite = games[game].difficultyWords[dif];

        
        switch (difficulty) // ajustar el layout group
        {
            default:
                words = games[game].wordsEasy;
                lettersGroup.cellSize = new Vector2(55, 55);
                break;
            case 1:
                words = games[game].wordsMedium;
                lettersGroup.cellSize = new Vector2(40, 40);
                break;
            case 2:
                words = games[game].wordsHard;
                lettersGroup.cellSize = new Vector2(34, 34);
                break;
        }
        print("Game: "+game);
        totalWordsLength = 0;

        //rellenar las palabras a encontrar
        for (int i = 0; i < words.Length; i++)
        {
            print(i);
            wordsToFind[i].text = words[i];
            wordsToFind[i].color = Color.white;
            //StartCoroutine(animateWord(i));
            totalWordsLength += words[i].Length;
        }

        //crear direcciones para las 5 palabras
        direction = new int[words.Length];
        wordsFound = new bool[words.Length];
        totalWords = words.Length;


        gridSize = 6 + (2 * difficulty); //cuantas filas y columnas tenemos

        //crear array bidimensional que es el tablero de palabras
        string[,] board = new string[gridSize, gridSize];
        for (int i = 0; i < letters.Length; i++)
        {
            bool active = i < (gridSize * gridSize) ? true : false;
                letters[i].gameObject.SetActive(active);
            //letters[i].fontSize = 
        }
        //limpiar las palabras subrayadas
        foreach (letterUtil t in letters)
        {
            t.used = false;
            t.highlight(false);
        }

        for (int i = 0; i < words.Length; i++) //coloca cada palabra
        {
            if ((words[i]).Length <= gridSize)
            {
                Debug.Log("Colocando: " + (words[i]));
                Vector2 placePosition = Vector2.zero;
                placePosition = wordFits((words[i]), board, gridSize, i); // busca la posicion para colocar la palabra
                if (placePosition != new Vector2(gridSize, gridSize))
                {
                    for (int j = 0; j < (words[i]).Length; j++) //coloca cada letra
                    {
                        int x = (int)placePosition.x;
                        int y = (int)placePosition.y;
                        if (direction[i]==0) //horizontal
                        {
                            x += j; // le sumamos las anteriores letras
                        }
                        else if(direction[i] == 1) //vertical
                        {
                            y += j; // le sumamos las anteriores letras
                        }
                        else //diagonal
                        {
                            x += j;
                            y += j;
                        }

                        board[x, y] = (words[i])[j].ToString();
                    }
                }
            }
        }

        // rellenamos los huecos libres con letras        
        for (int i = 0; i < gridSize; i++) //recorremos las filas
        {
            for (int j = 0; j < gridSize; j++) // recorremos las columnas
            {
                if(board[j,i] == "" || board[j,i] == null)
                {
                    board[j, i] = abc[Random.Range(0, abc.Length)].ToString(); //rellenalo con una letra aleatoria

                }
                letters[j + (i * gridSize)].txt.text = board[j, i];
            }
        }

        finalBoard = board;
        StartCoroutine(setPlay(true));
    }
    IEnumerator animateWord(int i)
    {
        yield return new WaitForSeconds(Random.Range(0f, 1.5f));
        sounds.PlayOneShot(popSound);
        wordsToFind[i].GetComponent<Animator>().SetTrigger("in");
    }
    IEnumerator setPlay(bool b)
    {
        yield return new WaitForSeconds(.15f);
        playing = b;
    }

    Vector2 wordFits(string word, string[,] board, int gridSize, int wI)
    {
        int x = 0;
        int y = 0;

        //int maxRandom = word.Length == gridSize? 2: 3; //posibilidad de ir en diagonal
        int maxRandom = 2;
        if(difficulty == 0 && totalWordsLength > 25)
        {
            maxRandom = 2;
        }
        int random = Random.Range(0, maxRandom);
        direction[wI] = random;
        for (int i = 0; i < 100; i++) // comprueba 50 veces donde poder colocar la palabra
        {
            if (i == 30 || i==60 || i==90) //si llega a 20 repeticiones sin encontrar sitio, cambia dirección
            {
                if(direction[wI] == 0)//si es horizontal
                {
                    int r = Random.Range(0, 2);
                    if (r == 0) direction[wI] = 1; //vertical 
                    if (r == 1) direction[wI] = 2; //diagonal
                }
                else if(direction[wI] == 1) //vertical
                {
                    int r = Random.Range(0, 2);
                    if (r == 0) direction[wI] = 0; //horizontal
                    if (r == 1) direction[wI] = 2; //diagonal
                }
                else //diagonal
                {
                    int r = Random.Range(0, 2);
                    if(r == 0) direction[wI] = 1; //vertical
                    if (r == 1) direction[wI] = 0; //horizontal
                }
            }
            if (direction[wI] == 0) // horizontal
            {
                x = Random.Range(0, gridSize - word.Length);
                y = Random.Range(0, gridSize);
            }
            else if(direction[wI] == 1) // vertical
            {
                x = Random.Range(0, gridSize);
                y = Random.Range(0, gridSize - word.Length);
            }
            else //diagonal
            {
                x = Random.Range(0, gridSize - word.Length);
                y = Random.Range(0, gridSize - word.Length);
            }
             // coge una coordenada al azar en la que cabria la palabra
            for (int j = 0; j < word.Length; j++) // comprueba que quepa toda la palabra letra a letra 
            {
                if (direction[wI] == 0)
                {
                    if(board[x + j, y] == null || board[x + j, y] == "" || board[x + j, y] == word[j].ToString()) // esta libre la casilla en direccion horizontal?
                    {
                        if (j == word.Length-1) // ha llegado al final de la palabra, puede devolver el valor encontrado
                        {
                            print("Palabra colocada: "+word);
                            return new Vector2(x, y);
                        }
                    }
                    else // habria fallado la comprobacion, por lo que la casilla esta ocupada 
                    {
                        break;
                    }
                }
                else if (direction[wI] == 1)// vertical
                {
                    if (board[x, y + j] == null || board[x, y + j] == "" || board[x, y + j] == word[j].ToString()) // esta libre la casilla en direccion vertical?
                    {
                        if (j == word.Length-1) // ha llegado al final de la palabra, puede devolver el valor encontrado
                        {
                            print("Palabra colocada: " + word);
                            return new Vector2(x, y);
                        }
                    }
                    else // habria fallado la comprobacion, por lo que la casilla esta ocupada
                    {
                        break;
                    }
                }/*
                else //diagonal 
                {
                    if (board[x+j, y + j] == null || board[x+j, y + j] == "" || board[x+j, y + j] == word[j].ToString()) // esta libre la casilla en direccion vertical?
                    {
                        if (j == word.Length - 1) // ha llegado al final de la palabra, puede devolver el valor encontrado
                        {
                            print("Palabra colocada: " + word);
                            return new Vector2(x, y);
                        }
                    }
                    else // habria fallado la comprobacion, por lo que la casilla esta ocupada
                    {
                        break;
                    }
                }*/
            }

        }

        print("No hay sitio para " + word + " - direccion: "+direction[wI]);
        StartCoroutine(createAgain());
        return new Vector2(gridSize,gridSize);
    }

    Vector2 initialPos, finalPos;
    public Transform highlighterRoot;
    public Image highlighter;
    public GameObject touchTrail;
    private void Update()
    {
        if (insPanel.activeSelf && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            insPanel.SetActive(false);
            difPanel.SetActive(false);
            winImage.SetActive(false);
            gamePanel.SetActive(true);

            //setDifficulty(2);
        }
        //#if UNITY_STANDALONE || UNITY_EDITOR
        
        //#endif

    }
    private void LateUpdate()
    {
        if (playing)
        {
            if (Input.GetMouseButtonUp(0)) //soltarlo
            {
                checkFormingWord();
            }
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 touchPos = Input.mousePosition;
                switch (touch.phase)
                {
                    default:
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        checkFormingWord();
                        break;
                }
            }
        }
    }
    string formingWord;

    public void addLetter(string l)
    {
        formingWord += l;
    }
    public void enlargeNeighbors(letterUtil letter)
    {
        for (int f = 0; f < gridSize; f++) //recorremos las filas
        {
            for (int c = 0; c < gridSize; c++) // recorremos las columnas
            {
                if(letters[(c + (f * gridSize))] == letter)
                {
                    letters[(c + (f * gridSize)) + gridSize].enlargeSet(new Vector3(1.3f, 1.3f, 1)); //vecino de abajo
                    letters[(c + (f * gridSize)) - gridSize].enlargeSet(new Vector3(1.3f, 1.3f, 1)); //vecino de arriba
                    letters[(c + (f * gridSize)) - 1].enlargeSet(new Vector3(1.3f, 1.3f, 1)); //vecino de izquierda
                    letters[(c + (f * gridSize)) + 1].enlargeSet(new Vector3(1.3f, 1.3f, 1)); //vecino de derecha
                }
            }
        }
    }
    void highlightClosestLetter(Vector2 pos)
    {
        float radius = 25f;
        for (int i = 0; i < finalBoard.Length; i++)
        {
            if (Vector2.Distance(pos, letters[i].transform.position) < radius)
            {
                if (!letters[i].wasAdded)
                {
                    print("added " + letters[i].txt.text);
                    //letters[i].wasAdded = true;
                    formingWord += letters[i].txt.text;
                    letters[i].highlight(true);
                    //Debug.Break();
                }
            }
        }
        sentInput = false;
    }
    void checkFormingWord()
    {
        if (formingWord == "") return;
        bool allFound = true;
        bool theWordWasFound = false;
        for (int i = 0; i < totalWords; i++)
        {
            print("comparando: " + formingWord + " - " + wordsToFind[i].text);
            if (formingWord == wordsToFind[i].text)
            {
                wordsFound[i] = true; // ha encontrado la palabra
                wordsToFind[i].color = Color.white * Color.gray; //destaca la palabra en la lista


                print("se ha encontrado: " + formingWord);
                theWordWasFound = true;


            }
            if (wordsFound[i] == false)
            {
                allFound = false; // comprueba si ha encontrado ya esta palabra
            }
        }
        if (theWordWasFound)
        {
            //sounds.PlayOneShot(goalSound);
        }
        else
        {
            //if(playing)
            //sounds.PlayOneShot(failSound);
        }
        print("limpiando trails y "+allFound.ToString());
        formingWord = "";
        foreach(letterUtil l in letters)
        {
            if (l.wasAdded && theWordWasFound) l.used = true;
            if (l.gameObject.activeSelf)
            {
                l.highlight(false);
            }
        }
        if (allFound)
        {
            // GANAS
            print("__ GANAS __");
                StartCoroutine(winEvents());
                wins++;
        }
    }
    void getHighlightedLetters()
    {
        int closerInitialIndex = 0;
        int closerFinalIndex = 0;
        float closerInitialDist = 500;
        float closerFinalDist = 500;

        int initialX, initialY;
        initialX = 0;
        initialY = 0;
        int finalX, finalY;
        finalX = finalY = 0;

        for (int i = 0; i < gridSize; i++) //recorremos las filas
        {
            for (int j = 0; j < gridSize; j++) // recorremos las columnas
            {
                //encontrar la letra mas cercana a la posicion inicial
                float dist = Vector2.Distance(initialPos, letters[j + (i * gridSize)].transform.position);
                if (dist < closerInitialDist)
                {
                    closerInitialDist = dist; // esta es la distancia mas cercana
                    closerInitialIndex = j + (i * gridSize); // este es el indice mas cercano
                    initialX = j; // la fila donde se encuentra la letra
                    initialY = i; // la columna donde se encuentra la letra
                }

                //encontrar la letra mas cercana  a la posicion final
                dist = Vector2.Distance(finalPos, letters[j + (i * gridSize)].transform.position);
                if (dist < closerFinalDist)
                {
                    closerFinalDist = dist; // esta es la distancia mas cercana
                    closerFinalIndex = j + (i * gridSize); // este es el indice mas cercano
                    finalX = j; // la filas donde se encuentra la letra
                    finalY = i; // la columna donde se encuentra la letra
                }
            }
        }
        print(closerInitialIndex + " | " + closerFinalIndex);
        print(initialX + " | X | " + finalX);
        print(initialY + " | Y | " + finalY);

        List<Vector2> gridPositions = new List<Vector2>();
        string wordFound = "";
        bool found = false;
        // intentar encontrar la letra final mediante horizontal
        for (int i = initialX; i < gridSize; i++)
        {
            gridPositions.Add(new Vector2(i, initialY));
            print("añadido: " + finalBoard[i, initialY]);
            wordFound+= finalBoard[i, initialY];
            if ((i+(initialY*gridSize)) == closerFinalIndex)
            {
                print("encontrado horizontal "+gridPositions.Count);

                //encontrado
                found = true;
                break;
            }
        }

        //intentar encontrar la letra mediante vertical
        if (!found)
        {
            gridPositions.Clear();
            wordFound = "";
            print("buscando vertical");
            for (int i = initialY; i < gridSize; i++)
            {
                gridPositions.Add(new Vector2(initialX, i));
                print("añadido: " + finalBoard[initialX, i]);
                wordFound += finalBoard[initialX, i];
                if ((initialX + (i * gridSize)) == closerFinalIndex)
                {
                    print("encontrado vertical");
                    //encontrado
                    found = true;
                    break;
                }
            }
        }

        //intentar encontrar mediante horizontal
        if (!found)
        {
            gridPositions.Clear();
            wordFound = "";
            print("buscando diagonal");
            for (int i = initialX, j = initialY; i < gridSize; i++, j++)
            {
                gridPositions.Add(new Vector2(i, j));
                print("añadido: " + finalBoard[i, j]);
                wordFound += finalBoard[i, j];
                if (i == finalX || j == finalY)
                {
                    print("encontrado diagonal");
                    //encontrado
                    found = true;
                    break;
                }
            }
        }
        if (found)
        {
            bool allFound = true;
            bool theWordWasFound = false;
            for (int i = 0; i < 5; i++)
            {
                print("comparando: " + wordFound + " - "+wordsToFind[i].text);
                if (wordFound == wordsToFind[i].text)
                {
                    wordsFound[i] = true; // ha encontrado la palabra
                    wordsToFind[i].color = Color.white*Color.gray; //destaca la palabra en la lista
                    //copiar el highlighter
                    Transform copy = highlighterRoot;
                    copy = Instantiate(copy, highlighterRoot.parent);
                    copy.SetAsFirstSibling();

                    print("se ha encontrado: " + wordFound);
                    theWordWasFound = true;
                }
                if (wordsFound[i] == false)
                {
                    allFound = false; // comprueba si ha encontrado ya esta palabra
                }
            }
            if (theWordWasFound)
            {
                sounds.PlayOneShot(goalSound);
            }
            else
            {
                sounds.PlayOneShot(failSound);
            }
            print("limpiando trails");
            Transform[] trails = highlighterRoot.GetComponentsInChildren<Transform>();
            if (trails.Length == 0) return;
            trails[0] = trails[1];
            foreach (Transform t in trails)
            {
                Destroy(t.gameObject);
            }
            if (allFound)
            {
                // GANAS
                print("__ GANAS __");
                //winFx.startAnimation();
            }
        }
        else
        {
            
        }
       
    }

    public GameObject winImage;
    public GameObject[] stars;
    IEnumerator winEvents()
    {
        yield return new WaitForSeconds(1);
        gamePanel.SetActive(false);
        winImage.SetActive(true);
        //winImage.GetComponent<Animator>().SetBool("show", true);
        yield return new WaitForSeconds(1);
        //stars[wins-1].GetComponent<Image>().color = Color.yellow;
        //sounds.PlayOneShot(starSound);
        yield return new WaitForSeconds(4);
        
        if (wins == 4)
        {
            //winFx.startAnimation();
            cleanVariables();
        }
        else
        {
            //winImage.GetComponent<Animator>().SetBool("show", false);
            yield return new WaitForSeconds(0.75f);
            winImage.SetActive(false);
            setDifficulty(difficulty);
        }
    }
    void cleanVariables()
    {
        /*foreach( GameObject s in stars)
        {
            s.GetComponent<Image>().color = Color.white;
        }*/
        wins = 0;
    }
    public void returnMain()
    {
        //volver a escena principal
        SceneManager.LoadScene("mainMenu" );
        PlayerPrefs.SetString("prevScene", "sopaLetras");
    }
    public void returnGames()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("prevScene"));
    }
    public GameObject resetButton, resetButtonLine, homeButton, homeButtonLine;
    public void resetGame()
    {
        //recargar escena
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        setDifficulty(difficulty);
    }
    [Space(30)]
    public Image buttonMute;
    public AudioSource music, sounds;
    public AudioClip failSound, goalSound, starSound, popSound;
    //public balloonFx winFx;
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
    private void OnApplicationQuit()
    {

    }
}

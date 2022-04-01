using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // game
    public int questionsLeft = 10;
    private int selectedCharacter;
    private static int charactersLeft;
    public Characters CharacterUtil;
    public UIHelper UIHelper;

    // misc
    public AudioClip yesAudio, noAudio;
    private AudioSource myAudio;

    void Start()
    {
        selectedCharacter = Random.Range(0, CharacterUtil.CharactersList.Count); //selecciona un personaje aleatorio

        charactersLeft = CharacterUtil.CharactersList.Count; // nos quedan X personajes

        UIHelper.fillCharacters(CharacterUtil.CharactersList); //envia los personajes al canvas

        UIHelper.setRemainingQuestions(questionsLeft); // muestra cuantas preguntas podemos hacer restantes

        myAudio = GetComponent<AudioSource>();

        Debug.Log(CharacterUtil.CharactersList[selectedCharacter].Name);

        UIHelper.lastQuestionInfo.text = "Haz preguntas!";
        UIHelper.selectedCharacterReveal.sprite = CharacterUtil.CharactersList[selectedCharacter].image; // establece el sprite del pj seleccionado
        
        getTags(); //muestra los posibles atributos disponibles.
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0); //recarga la escena
        }
    }

    public void receiveQuestion(string question, string[]possiblePrefixes)
    {
        if (questionsLeft <= 0 || charactersLeft<=1) return;

        //quitamos de la pregunta recibida la informacion innecesaria para compararla con los personajes
        string attribute = getAttributeFromQuestion(question, possiblePrefixes);

        //tachamos a los personajes que no tengan este atributo
        if ((CharacterUtil.CharactersList[selectedCharacter].attributes.Contains(attribute) // si nuestro personaje tiene este atributo
            || CharacterUtil.CharactersList[selectedCharacter].Name.Equals(attribute))) // o acierta el nombre
        {
            foreach (CharacterInfo ch in CharacterUtil.CharactersList) //tacha a los personajes que no lo tengan
            {
                if (!(ch.attributes.Contains(attribute) || ch.Name.Equals(attribute)) && !ch.hidden)
                {
                    UIHelper.hideCharacter(ch.Name); //esconde este personaje
                    ch.hidden = true;
                    charactersLeft--;
                }
            }
            myAudio.PlayOneShot(yesAudio);
            UIHelper.lastQuestionInfo.text += " Si!";
        } 
        else //si no tiene el atributo preguntado
        {
            foreach (CharacterInfo ch in CharacterUtil.CharactersList) //tacha a los personajes que lo tengan
            {
                if ((ch.attributes.Contains(attribute) || ch.Name.Equals(attribute)) &&!ch.hidden)
                {
                    UIHelper.hideCharacter(ch.Name);
                    ch.hidden = true;
                    charactersLeft--;
                }
            }
            myAudio.PlayOneShot(noAudio);
            UIHelper.lastQuestionInfo.text += " No!";
        }

        //restamos una remaining question
        questionsLeft--;
        UIHelper.setRemainingQuestions(questionsLeft);

        if(questionsLeft == 0) //si no le quedan intentos
        {
            StartCoroutine(revealWinner(true));
        }
        if (charactersLeft <= 1) //gana
        {
            StartCoroutine(revealWinner(false));
        }

        getTags(); //muestra los posibles atributos disponibles.
    }
    string getAttributeFromQuestion(string question, string[] possiblePrefixes)
    {
        foreach (string prefix in possiblePrefixes)
        {
            if (question.Contains(prefix)) question = question.Replace(prefix+" ", ""); //elimina cualquier prefijo
        }
        //elimina los ¿?
        question = question.Replace("¿", ""); 
        question = question.Replace("?", "");

        UIHelper.lastQuestionInfo.text = question+"..."; //muestra la pregunta que se ha hecho
        Debug.Log(question);
        return question;
    }
    IEnumerator revealWinner(bool lost)
    {
        UIHelper.selectedCharacterReveal.color = Color.black;
        yield return new WaitForSeconds(1);
        UIHelper.selectedCharacterReveal.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        UIHelper.selectedCharacterReveal.color = Color.white;
        if (lost)
        {
            myAudio.PlayOneShot(noAudio);
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(0);
        }
        else
        {
            myAudio.PlayOneShot(yesAudio);
        }
    }
    public static void characterLess()
    {
        charactersLeft--;
    }
    void getTags()
    {
        string allAttributes = "";
        foreach (CharacterInfo ch in CharacterUtil.CharactersList) //itera por cada personaje
        {
            if (ch.Name.Equals("")) continue; //si el personaje tiene info
            if (ch.hidden) continue;
            allAttributes+=(ch.Name)+","; //añade el nombre como atributo del personaje
            string[] attributes = ch.attributes.Split(','); //separa los atributos para añadirlos a la lista de atributos en comun
            foreach (string s in attributes) //itera por cada atributo del personaje
            {
                if (!allAttributes.Contains(s)) //comprueba que aun no tenemos este atributo
                {
                    allAttributes += (s) + ","; //se añade el atributo
                }
            }
        }
        UIHelper.tagsInfo.text = allAttributes; //muestra los atributos en pantalla
    }
}

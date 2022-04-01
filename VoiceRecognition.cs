using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceRecognition : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    public List<string> phrases = new List<string>();
    public string[] possiblePrefixes;
    public Characters CharactersUtil;
    public GameManager gm;

    private void Start()
    {
        //obtiene todos los atributos posibles de los personajes
        List<string> allAttributes = new List<string>();
        foreach(CharacterInfo ch in CharactersUtil.CharactersList) //itera por cada personaje
        {
            if (ch.Name.Equals("")) continue; //si el personaje tiene info
            allAttributes.Add(ch.Name); //añade el nombre como atributo del personaje
            string[] attributes = ch.attributes.Split(','); //separa los atributos para añadirlos a la lista de atributos en comun
            foreach(string s in attributes) //itera por cada atributo del personaje
            {
                if (!allAttributes.Contains(s)) //comprueba que aun no tenemos este atributo
                {
                    allAttributes.Add(s); //se añade el atributo
                }
            }
        }

        //crea las posibles frases con los atributos y los prefijos combinados
        foreach(string prefix in possiblePrefixes)
        {
            foreach(string attribute in allAttributes)
            {
                //añade las posibles frases al diccionario de frases que reconocemos
                phrases.Add("¿"+prefix + " " + attribute+"?");
            }
        }

        //inicia el sistema de reconocimiento de las frases
        keywordRecognizer = new KeywordRecognizer(phrases.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognisedPhrase;
        keywordRecognizer.Start();
    }

    private void RecognisedPhrase(PhraseRecognizedEventArgs phrase)
    {
        Debug.Log(phrase.text);
        gm.receiveQuestion(phrase.text,possiblePrefixes);
    }
}

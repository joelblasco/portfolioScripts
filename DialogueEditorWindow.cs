#if UNITY_EDITOR
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class DialogueEditorWindow : EditorWindow
{
    private DialogueContainerSO currentDialogueContainer;
    private DialogueGraphView graphView;
    private DialogueSaveAndLoad saveAndLoad;

    private LanguageType languageType = LanguageType.Spanish;
    private ToolbarMenu toolbarMenu; //para añadir los elementos
    private Label nameOfDialogueContainer; //nombre del grafo

    public LanguageType LanguageType { get => languageType; set => languageType = value; } //para cambiar el tipo de idioma

    [OnOpenAsset(1)]
    public static bool ShowWindow(int _instanceId, int line)
    {
        UnityEngine.Object item = EditorUtility.InstanceIDToObject(_instanceId);
        if(item is DialogueContainerSO)
        {
            DialogueEditorWindow window = (DialogueEditorWindow)GetWindow(typeof(DialogueEditorWindow));
            window.titleContent = new GUIContent("Custom Dialogue Editor");
            window.currentDialogueContainer = item as DialogueContainerSO;
            window.minSize = new Vector2(500, 250);
            window.Load();
        }
        return false;
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        Load();
    }
    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView(this);
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);

        saveAndLoad = new DialogueSaveAndLoad(graphView);
    }
    private void GenerateToolbar()
    {
        StyleSheet styleSheet = Resources.Load<StyleSheet>("GraphCustomStyle");
        rootVisualElement.styleSheets.Add(styleSheet);

        Toolbar toolbar = new Toolbar();

        //Save Buttons
        Button saveBtn = new Button()
        {
            text = "Save"
        };
        saveBtn.clicked += () => //add the f() Save to the button
        {
            Save();
        };
        toolbar.Add(saveBtn);

        //Load Button
        Button loadBtn = new Button()
        {
            text = "Load"
        };
        loadBtn.clicked += () => //add the f() Load to the button
        {
            Load();
        };
        toolbar.Add(loadBtn);

        //Dropdpwm fpr languages
        toolbarMenu = new ToolbarMenu();
        foreach(LanguageType lang in (LanguageType[])Enum.GetValues(typeof(LanguageType))) //iterate all languages and add the option
        {
            toolbarMenu.menu.AppendAction(lang.ToString(),new Action<DropdownMenuAction>(x => Language(lang, toolbarMenu)));
        }
        toolbar.Add(toolbarMenu);

        // Name of current DialogueContainer you have open
        nameOfDialogueContainer = new Label("");
        toolbar.Add(nameOfDialogueContainer);
        nameOfDialogueContainer.AddToClassList("nameOfDialogueContainer");


        //añade la toolbar
        rootVisualElement.Add(toolbar);
    }

    private void Load()
    {
        Debug.Log("Load");
        if(currentDialogueContainer != null)
        {
            Language(LanguageType.Spanish, toolbarMenu);  //set idioma por defecto
            nameOfDialogueContainer.text = "Name: " + currentDialogueContainer.name;
            saveAndLoad.Load(currentDialogueContainer);
        }
    }
    private void Save()
    {
        if (currentDialogueContainer != null)
        {
            saveAndLoad.Save(currentDialogueContainer);
        }
    }

    private void Language(LanguageType lang, ToolbarMenu toolbarMenu)
    {
        toolbarMenu.text = "Language: " + lang.ToString();
        languageType = lang;
        graphView.LanguageReload();
    }
}
#endif
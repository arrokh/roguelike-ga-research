using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;

public class Danesh : EditorWindow{

    //This adds us to the window menu so we can be opened
    [MenuItem ("Window/Danesh")]
    public static void  ShowWindow () {
        EditorWindow.GetWindow(typeof(Danesh));
    }

    //Panel styles
    GUIStyle main_panel_style;
    GUIStyle tab_style;
    GUIStyle selected_tab_style;
    //Text styles
    GUIStyle parameter_text_label;
    GUIStyle body_text_style_left;
    GUIStyle body_text_style_center;
    GUIStyle body_text_style_right;
    GUIStyle header_text_style;

    /*
        Colors
    */
    Color color_background = new Color(0.45f, 0.45f, 0.45f, 1f);
    Color color_selected_tab = new Color(0.6f, 0.6f, 0.6f, 1f);
    Color color_selected_text = new Color(0.2f, 0.2f, 0.2f, 1f);
    Color color_black = new Color(0f, 0f, 0f, 1f);

    public Color32[] standardColors = new Color32[]{
        new Color32(220,20,60,255),
        new Color32(255,131,250,255),
        new Color32(72,72,255,255),
        new Color32(0,229,238,255),
        new Color32(0,205,102,255),
        new Color32(255,215,0,255),
        new Color32(255,127,0,255),
    };

    /*
        Textures
    */
    Texture2D pixel_background;

    /*
        Settings
    */
    float window_width = 1280;
    float window_height = 720;

    float tab_width = 0.5f;

    /*
        Flags/Variables
    */
    //use the 3x3 output style?
    bool showSingleOutput = false;

    public static int TAB_PARAM = 0;
    public static int TAB_ERA = 1;
    public static int TAB_GENERATOR = 2;
    public static int TAB_ABOUT = -1;

    int activeTabLeft = TAB_PARAM;
    int activeTabRight = TAB_GENERATOR;

    bool regenerateOnParameterChange = false;
    int sizeOfERAHistory = 4;
    int sizeOfHistoryTexture = 96;

    //Loaded Generators and Visualisers
    int selected_generator = 0;
    public string[] listOfGeneratorNames;
    int selected_visualiser = 0;
    public string[] listOfVisualiserNames;

    public List<object> generatorObjects;
    public List<MethodInfo> generatorMethods;
    public List<GeneratorVisualiser> visualiserList;

    public List<string[]> visualiserNamesPerGenerator;
    public List<List<GeneratorVisualiser>> visualisersPerGenerator;
    public List<List<GeneratorParameter>> generatorParameters;

    //Metrics
    public List<List<GeneratorMetric>> generatorMetricsPerGenerator;
    public List<GeneratorMetric> metricList;
    public string[] axis_options;

    public List<float> metricTargets;
    public List<string> metricInputs;

    //Expressive Range
    public int numberOfERARuns = 200;
    public int numberOfRERARuns = 1500;

    public ExpressiveRangeAnalysis eranalyser;
    public Texture2D[] eraTextures;
    public Texture2D[] eraTexturesAnnotated;
    public List<float[]> eraParams;
    public List<Texture2D[]> pastERATextures;
    public List<Texture2D[]> pastRERATextures;
    public Texture2D[] reraTextures;
    public bool showingERA = true;

    //in pixels

    public int sparklineMaxLength = 40;
    public int miniSparklineMaxLength = 15;

    public int miniSparklineWidth = 1;
    public int sparklineWidth = 4;

    public int miniSparklineSpacing = 1;
    public int sparklineSpacing = 4;

    //Previews
    public List<List<GeneratorSample[,]>>[] eraSamples;
    public List<List<GeneratorSample[,]>>[] reraSamples;

    int[] xAxesERA;
    int[] yAxesERA;
    int[] xAxesRERA;
    int[] yAxesRERA;

    //Generated Content
    public Texture2D[] visualisedContent;
    public Texture2D[,] visualisedContentGrid;

    //Hover Info
    Texture2D HoverTexture;
    bool HoveringOverHistory = false;
    int HoveringOverIndex = -1;
    Rect[] pastContentRect;

    //Saving and Loading
    bool reallyReset;
    bool beginSave;
    bool beginLoad;
    bool saving;
    bool loading;
    string saveName;
    List<string> possibleLoadFiles;
    int selectedFile;

    public void SetupGUIStyles(){
        pixel_background = MakeSinglePixelTexture(color_background);

        main_panel_style = new GUIStyle();
        main_panel_style.normal.background = pixel_background;

        tab_style = new GUIStyle("button");
        selected_tab_style = new GUIStyle("button");

        parameter_text_label = new GUIStyle();
        parameter_text_label.alignment = TextAnchor.MiddleLeft;
        parameter_text_label.fontSize = 14;

        body_text_style_left = new GUIStyle();
        body_text_style_left.alignment = TextAnchor.MiddleLeft;

        body_text_style_center = new GUIStyle();
        body_text_style_center.alignment = TextAnchor.MiddleCenter;

        body_text_style_right = new GUIStyle();
        body_text_style_right.alignment = TextAnchor.MiddleRight;

        header_text_style = new GUIStyle();
        header_text_style.alignment = TextAnchor.MiddleCenter;
        header_text_style.fontSize = 16;
    }

    public void Init(){
        generatorObjects = new List<object>();
        generatorMethods = new List<MethodInfo>();
        visualiserList = new List<GeneratorVisualiser>();
        generatorParameters = new List<List<GeneratorParameter>>();

        LoadAllGenerators();
        LoadAllMetrics();

    }

    public void Update(){

    }

    /*
        OnGUI()

        Called whenever Unity wants to redraw the GUI. Abandon hope all ye who enter here.
    */

    void OnGUI() {
        //If we haven't set up our styles, do so
        if(main_panel_style == null){
            SetupGUIStyles();
            Init();
        }
        //This is so we can obtain the mouse position later on
        if(!wantsMouseMove){
            wantsMouseMove = true;
        }

        window_width = position.width;

        //Paint the background
        GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), pixel_background, ScaleMode.StretchToFill);

        //Header
        GUILayout.BeginVertical(main_panel_style);
        GUILayout.Space(15);
        GUILayout.EndVertical();

        //Paint stuff
        GUILayout.BeginHorizontal(main_panel_style);


        GUILayout.BeginVertical(main_panel_style, GUILayout.Width(window_width * tab_width));

        GUILayout.BeginHorizontal();

        if(GUILayout.Toggle(activeTabLeft == TAB_PARAM, "Edit", "Button")){
            activeTabLeft = TAB_PARAM;
        }
        if(GUILayout.Toggle(activeTabLeft == TAB_GENERATOR, "View", "Button")){
            activeTabLeft = TAB_GENERATOR;
        }
        if(GUILayout.Toggle(activeTabLeft == TAB_ERA, "Analyse", "Button")){
            activeTabLeft = TAB_ERA;
        }
        if(GUILayout.Toggle(activeTabLeft == TAB_ABOUT, "About", "Button", GUILayout.Width(70))){
            activeTabLeft = TAB_ABOUT;
        }

        GUILayout.EndHorizontal();

        if(activeTabLeft == TAB_PARAM){
            DrawParameterTab();
        }
        else if(activeTabLeft == TAB_GENERATOR){
            DrawGeneratorArea();
        }
        else if(activeTabLeft == TAB_ERA){
            DrawERATab();
        }
        else if(activeTabLeft == TAB_ABOUT){
            DrawAboutTab();
        }

        GUILayout.EndVertical();

        //Right-hand panel - shows the output from the generator or other processes
        GUILayout.BeginVertical(main_panel_style, GUILayout.Width(window_width * tab_width));

        GUILayout.BeginHorizontal();

        if(GUILayout.Toggle(activeTabRight == TAB_PARAM, "Edit", "Button")){
            activeTabRight = TAB_PARAM;
        }
        if(GUILayout.Toggle(activeTabRight == TAB_GENERATOR, "View", "Button")){
            activeTabRight = TAB_GENERATOR;
        }
        if(GUILayout.Toggle(activeTabRight == TAB_ERA, "Analyse", "Button")){
            activeTabRight = TAB_ERA;
        }
        if(GUILayout.Toggle(activeTabRight == TAB_ABOUT, "About", "Button", GUILayout.Width(70))){
            activeTabLeft = TAB_ABOUT;
        }

        GUILayout.EndHorizontal();

        if(activeTabRight == TAB_PARAM){
            DrawParameterTab();
        }
        else if(activeTabRight == TAB_GENERATOR){
            DrawGeneratorArea();
        }
        else if(activeTabRight == TAB_ERA){
            DrawERATab();
        }
        else if(activeTabRight == TAB_ABOUT){
            DrawAboutTab();
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        int old_selected_generator = selected_generator;
        int old_selected_visualiser = selected_visualiser;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Active Generator:", parameter_text_label, GUILayout.Width(150));
        selected_generator = EditorGUILayout.Popup(selected_generator, listOfGeneratorNames, GUILayout.Width(300));
        GUILayout.Space(100);
        GUILayout.Label("Active Visualiser:", parameter_text_label, GUILayout.Width(150));
        selected_visualiser = EditorGUILayout.Popup(selected_visualiser, visualiserNamesPerGenerator[selected_generator], GUILayout.Width(300));
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        if(old_selected_generator != selected_generator){
            selected_visualiser = 0;
        }
        if(old_selected_visualiser != selected_visualiser || old_selected_generator != selected_generator){
            GenerateAndShowContent();
        }

        GUILayout.Space(15);
    }

    void DrawAboutTab(){
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Danesh v1.0 [June 2017]", header_text_style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();GUILayout.Space(50);GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));GUILayout.Space(50);GUILayout.EndHorizontal();

        //Credits:
        // Microscope by IYIKON from the Noun Project
        // by Nat
        // Dice by Eliricon from the Noun Project

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(Resources.Load<Texture2D>("Danesh/metamakers-logo"), header_text_style, GUILayout.Width(225));
        GUILayout.Space(20);
        GUILayout.Label(Resources.Load<Texture2D>("Danesh/falmouthuniversity-logo"), header_text_style, GUILayout.Width(225));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Label("Danesh was developed by The Metamakers Institute, with support from Falmouth University", body_text_style_center);

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Credits & Thanks", header_text_style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();GUILayout.Space(50);GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));GUILayout.Space(50);GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.Label("Primary development by Michael Cook and Rob Saunders.\nAdditional development by Jeremy Gow [Goldsmiths University].",body_text_style_center);
        GUILayout.Label("");
        GUILayout.Label("The Metamakers are: Simon Colton, Rob Saunders, Michael Cook, \nSwen Gaudl, Peter Ivey, Mark Nelson, Blanca Perez-Ferrer, Ed Powley.",body_text_style_center);
        GUILayout.Label("");
        GUILayout.Label("Thanks also to Nat Ireton for contributing sprites to the Chunky Level Generator, \nRune Skovbo Johansen, and about a billion people on Twitter.",body_text_style_center);
        GUILayout.Label("");
        GUILayout.Label("For updates and news, you can follow @mtrc or @thosemetmakers on Twitter, \nor visit www.metamakersinstitute.com",body_text_style_center);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(30);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Citations & Further Reading", header_text_style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();GUILayout.Space(50);GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));GUILayout.Space(50);GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.Label("While not an exhaustive list, here are some of the most important influences on Danesh. \nYou can find more information on our website.",body_text_style_center);
        GUILayout.Label("");
        GUILayout.Label("Expressive Range Analysis was first proposed in the paper \n'Analyzing The Expressive Range Of A Generator'\n by Gillian Smith [Worcester Polytechnic Institute] and Jim Whitehead [UC Santa Cruz].",body_text_style_center);
        GUILayout.Label("");
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.EndVertical();

        //Close the padding
        GUILayout.Space(window_width * tab_width * 0.05f);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
    }

    public void DrawParameterTab(){

        if(generatorObjects == null || generatorObjects.Count == 0){
            GUILayout.FlexibleSpace();
            GUILayout.Label("No generators found!");
            GUILayout.FlexibleSpace();
            return;
        }

        GUILayout.Space(20);

        GUIStyle style = GUI.skin.horizontalSliderThumb;
        style.fixedHeight = 15;
        style.fixedWidth = 15;
        GUI.skin.horizontalSliderThumb = style;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Parameters for "+generatorObjects[selected_generator].GetType()+"."+generatorMethods[selected_generator].Name, header_text_style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();
        if(generatorObjects != null && generatorObjects.Count > 0 && generatorParameters.Count > 0){
            GUILayout.Space(30);
            for(int i=0; i<generatorParameters[selected_generator].Count; i++){
                GeneratorParameter p = generatorParameters[selected_generator][i];

                GUILayout.BeginHorizontal();
                GUILayout.Label(p.name, parameter_text_label);
                GUILayout.Label("(Variable name: "+p.field.Name+")", body_text_style_right);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if(p.type == "float"){
                    object newVal = (object) GUILayout.HorizontalSlider((float) p.currentValue, (float)p.minValue, (float)p.maxValue);
                    if((float) newVal != (float) p.currentValue){
                        //Change in the generator
                        p.field.SetValue(generatorObjects[selected_generator], newVal);
                        if(regenerateOnParameterChange){
                            GenerateAndShowContent();
                        }
                    }
                    p.currentValue = newVal;
                    GUILayout.TextField(Math.Round((Decimal)(float)p.currentValue, 3, MidpointRounding.AwayFromZero)+"", 20, GUILayout.Width(50));
                }
                if(p.type == "int"){
                    object newVal = (object)(int) GUILayout.HorizontalSlider(Convert.ToInt32(p.currentValue), Convert.ToInt32(p.minValue), Convert.ToInt32(p.maxValue));
                    if(Convert.ToInt32(newVal) != Convert.ToInt32(p.currentValue)){
                        //Change in the generator
                        p.field.SetValue(generatorObjects[selected_generator], newVal);
                        if(regenerateOnParameterChange){
                            GenerateAndShowContent();
                        }
                    }
                    p.currentValue = newVal;
                    GUILayout.TextField(Convert.ToInt32(p.currentValue)+"", 20, GUILayout.Width(50));
                }
                if(p.type == "bool"){
                    GUILayout.FlexibleSpace();
                    bool cval = (bool) p.currentValue;
                    cval = (bool) GUILayout.Toggle(cval, "", GUILayout.Width(20));
                    p.field.SetValue(generatorObjects[selected_generator], (object) cval);
                    p.currentValue = (object) cval;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(15);
            }
            GUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            //Experimental: this needs more info for the user, but it seems like a useful feature, as long as they don't click and drag the sliders >_>
            //TODO: Find a way to only proc this when the user stops dragging?
            // regenerateOnParameterChange = (bool) GUILayout.Toggle(regenerateOnParameterChange, "  Generate content on parameter change", GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else{
            GUILayout.Label("ERROR: No parameters found, or no generator loaded.", header_text_style);
        }

        GUILayout.EndVertical();
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        /*
            Alright bear with me on this.
            We want to display two columns of metrics. A metric is displayed if:
             + It does not have a specified base type and it throws no exceptions on a test piece of content
             + Its specified base type matches the currently selected generator's base type
            We iterate through the list looking for valid generators, and slap them into columns of width two.
        */

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Loaded Metrics", header_text_style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

        //I'm doing this so I know how much space I have for metrics, since the box autofills.
        //I'm not _sure_ this is the best way to do this but it seems like it won't explode anything at least.
        float columnWidth = GUILayoutUtility.GetLastRect().width;

        GUILayout.Space(5);

        int loadedMetrics = 0;

        GUIStyle s = new GUIStyle(GUI.skin.box);
        s.alignment = TextAnchor.MiddleCenter;

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);

        GUILayout.BeginVertical();
        if(generatorMetricsPerGenerator == null && generatorMethods != null){
            SetupGeneratorMetrics();
        }
        if(generatorMetricsPerGenerator != null && generatorMetricsPerGenerator[selected_generator] != null){
            for(int i=0; i<generatorMetricsPerGenerator[selected_generator].Count; i+=2){
                GUILayout.BeginHorizontal();
                for(int j=0; j<2; j++){
                    if(i+j >= generatorMetricsPerGenerator[selected_generator].Count)
                        continue;
                    if(metricList[i+j].base_type == null || generatorMetricsPerGenerator[selected_generator][i+j].base_type == generatorObjects[selected_generator].GetType()){
                        GUILayout.BeginHorizontal();

                        GUILayout.Box(MakeTex( 15, 15, generatorMetricsPerGenerator[selected_generator][i+j].color), s);
                        //Note that we clamp the length of the label to columnwidth/2 - 20 (to compensate for the box width) so that the flexible spacing is even regardless of the label's text content
                        GUILayout.Label(generatorMetricsPerGenerator[selected_generator][i+j].name+": "+generatorMetricsPerGenerator[selected_generator][i+j].currentValue, body_text_style_left, GUILayout.Height(28), GUILayout.Width(columnWidth/2f - 20));

                        loadedMetrics++;

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();
        GUILayout.Space(20);
        GUILayout.EndHorizontal();


        if(loadedMetrics == 0){
            GUILayout.Label("No metrics found for this generator!", body_text_style_center);
        }

        GUILayout.Space(10);

        GUILayout.EndVertical();

        //Close the padding
        GUILayout.Space(window_width * tab_width * 0.05f);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
    }

    public void DrawERATab(){
        if(metricList == null){
            GUILayout.FlexibleSpace();
            GUILayout.Label("No Metrics Loaded!", header_text_style);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Scan For Metrics", "Button", GUILayout.Width(150))){
                LoadAllMetrics();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else{
            DrawERATabWithMetrics();
        }
        GUILayout.FlexibleSpace();
    }

    public void DrawERATabWithMetrics(){
        if((showingERA && (eraTextures == null || eraTextures[selected_generator] == null)) || (!showingERA && (reraTextures == null || reraTextures[selected_generator] == null))){
            GUILayout.Space(20);
        }
        else{
            GUILayout.Space(20);
        }

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Expressive Range Analysis", header_text_style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        if((showingERA && (eraTextures == null || eraTextures[selected_generator] == null)) || (!showingERA && (reraTextures == null || reraTextures[selected_generator] == null))){
            GUILayout.Label("No current Expressive Range Analysis available.", body_text_style_center);
        }
        else{
            //Display the current ERA
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();


            if(showingERA){
                GUILayout.BeginVertical(GUILayout.Height(eraTexturesAnnotated[selected_generator].height));
                GUILayout.Box(eraTexturesAnnotated[selected_generator]);
            }
            else{
                GUILayout.BeginVertical(GUILayout.Height(reraTextures[selected_generator].height));
                GUILayout.Box(reraTextures[selected_generator]);
            }

            if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)){
                float mx = Event.current.mousePosition.x;
                float my = Event.current.mousePosition.y;
                if(showingERA)
                    DisplayERASample(mx, my, GUILayoutUtility.GetLastRect(), eraSamples, xAxesERA[selected_generator], yAxesERA[selected_generator]);
                else
                    DisplayERASample(mx, my, GUILayoutUtility.GetLastRect(), reraSamples, xAxesRERA[selected_generator], yAxesRERA[selected_generator]);
            }
            if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)){
                if(!showingERA){
                    float mx = Event.current.mousePosition.x;
                    float my = Event.current.mousePosition.y;
                    //The user has clicked on a square in the RERA. Find the sample and reconfigure the generator to this type
                    ConfigureGenerator(mx, my, GUILayoutUtility.GetLastRect(), reraSamples, xAxesRERA[selected_generator], yAxesRERA[selected_generator]);
                }
            }
            GUILayout.EndVertical();

            if(showingERA){
                GUILayout.BeginVertical(GUILayout.Height(eraTexturesAnnotated[selected_generator].height));
            }
            else{
                GUILayout.BeginVertical(GUILayout.Height(reraTextures[selected_generator].height));
            }

            if(showingERA){
                GUILayout.Label("History", body_text_style_center);
                for(int i=0; i<pastERATextures[selected_generator].Length; i++){
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal(GUILayout.Width(sizeOfHistoryTexture+2));
                    GUILayout.FlexibleSpace();
                    if(pastERATextures[selected_generator][i] != null){
                      GUILayout.Box(pastERATextures[selected_generator][i]);
                    }
                    else if(pastERATextures[selected_generator][i] == null){
                      GUILayout.Box("", GUILayout.Width(sizeOfHistoryTexture), GUILayout.Height(sizeOfHistoryTexture));
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            else{
                //Removed in 1.0: We no longer show a history for RERAs (since they should be mostly similar)
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            int old = 0;
            if(showingERA){
                old = xAxesERA[selected_generator];
                GUILayout.Label("X-Axis: ");
                xAxesERA[selected_generator] = EditorGUILayout.Popup(xAxesERA[selected_generator], GetMetricNamesForActiveGenerator(), GUILayout.Width(200));
                if(old != xAxesERA[selected_generator]){
                    ShowERAorRERA();
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label("Y-Axis: ");
                old = yAxesERA[selected_generator];
                yAxesERA[selected_generator] = EditorGUILayout.Popup(yAxesERA[selected_generator], GetMetricNamesForActiveGenerator(), GUILayout.Width(200));
                if(old != yAxesERA[selected_generator]){
                    ShowERAorRERA();
                }
            }
            else{
                old = xAxesRERA[selected_generator];
                GUILayout.Label("X-Axis: ");
                xAxesRERA[selected_generator] = EditorGUILayout.Popup(xAxesRERA[selected_generator], GetMetricNamesForActiveGenerator(), GUILayout.Width(200));
                if(old != xAxesRERA[selected_generator]){
                    ShowERAorRERA();
                }
                GUILayout.Label("Y-Axis: ");
                old = yAxesRERA[selected_generator];
                yAxesRERA[selected_generator] = EditorGUILayout.Popup(yAxesRERA[selected_generator], GetMetricNamesForActiveGenerator(), GUILayout.Width(200));
                if(old != yAxesRERA[selected_generator]){
                    ShowERAorRERA();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Perform New ERA", "Button")){
            PerformERAOnActiveGenerator();
            showingERA = true;
        }
        if(showingERA && reraTextures[selected_generator] != null){
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Switch To RERA", "Button")){
                showingERA = false;
            }
        }
        else if(!showingERA && eraTextures[selected_generator] != null){
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Switch To ERA", "Button")){
                showingERA = true;
            }
        }
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Perform New RERA", "Button")){
            PerformRERAOnActiveGenerator();
            showingERA = false;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.EndVertical();
        GUILayout.Space(window_width * tab_width * 0.05f);
        GUILayout.EndHorizontal();

        if((showingERA && (eraTextures == null || eraTextures[selected_generator] == null)) || (!showingERA && (reraTextures == null || reraTextures[selected_generator] == null))){
            GUILayout.Space(50);
        }
        else{
            GUILayout.FlexibleSpace();
        }
    }

    public void DrawTuningTab(){
        GUILayout.FlexibleSpace();
        GUILayout.Label("Automatic Tuning", header_text_style);
        GUILayout.FlexibleSpace();
    }

    public void DrawGeneratorArea(){
        if(generatorObjects != null && generatorObjects.Count > 0){
            DrawGeneratorTab();
        }
        else{
            GUILayout.FlexibleSpace();
            GUILayout.Label("No Generator Loaded!", header_text_style);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Scan For Generators", "Button", GUILayout.Width(150))){
                LoadAllGenerators();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        GUILayout.FlexibleSpace();
    }

    public void DrawGeneratorTab(){
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Generator Output", header_text_style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();

        //Larger column with generator output in it
        GUILayout.BeginVertical();
        //If there's a texture generated for this generator, pop it in a box and show it
        if(HoveringOverHistory){
            GUILayout.FlexibleSpace();
            GUILayout.Box(HoverTexture);
            GUILayout.FlexibleSpace();
        }
        else if(showSingleOutput && visualisedContent[selected_generator] != null){
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(visualisedContent[selected_generator]);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else if(!showSingleOutput && visualisedContentGrid[selected_generator, 0] != null){
            for(int i=0; i<9; i+=3){
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(visualisedContentGrid[selected_generator, i]);
                GUILayout.Box(visualisedContentGrid[selected_generator, i+1]);
                GUILayout.Box(visualisedContentGrid[selected_generator, i+2]);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        else{
            GUILayout.FlexibleSpace();
            GUILayout.Label("Danesh couldn't visualise this content. Check your generator and visualiser and try again?", body_text_style_center, GUILayout.Width(300));
            GUILayout.FlexibleSpace();
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Generate Content", "Button", GUILayout.Width(150))){
            GenerateAndShowContent();
        }
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Toggle Single/Grid View", "Button", GUILayout.Width(150))){
            showSingleOutput = !showSingleOutput;
            GenerateAndShowContent();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        RenderSaveLoadReset();

        GUILayout.FlexibleSpace();
    }

    public void RenderSaveLoadReset(){
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(generatorParameters[selected_generator] != null && GUILayout.Button("Save State", "button", GUILayout.Width(150))){
            beginSave = true;
            reallyReset = false;
            beginLoad = false;
            saveName = GetRandomConfigName();
        }
        GUILayout.FlexibleSpace();
        if(generatorParameters[selected_generator] != null && GUILayout.Button("Reset Generator", "button", GUILayout.Width(150))){
            reallyReset = true;
            beginSave = false;
            beginLoad = false;
        }
        GUILayout.FlexibleSpace();
        if(generatorParameters[selected_generator] != null && GUILayout.Button("Load State", "button", GUILayout.Width(150))){
            beginSave = false;
            reallyReset = false;
            beginLoad = true;
            possibleLoadFiles = new List<string>();

            DirectoryInfo root = new DirectoryInfo(".");
            FileInfo[] infos = root.GetFiles();
            // Debug.Log(infos.Length+" files found");
            foreach(FileInfo f in infos){
                // Debug.Log(f.Name);
                if(f.Name.EndsWith(".param")){
                    possibleLoadFiles.Add(f.Name);
                }
            }

            selectedFile = 0;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(reallyReset){
            GUILayout.Label("Reset all parameters to their originally loaded values?");
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Yes", "button", GUILayout.Width(50))){
                foreach(GeneratorParameter p in generatorParameters[selected_generator]){
                    p.field.SetValue(generatorObjects[selected_generator], p.originalValue);
                    p.currentValue = p.originalValue;
                }
                reallyReset = false;
            }
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("No", "button", GUILayout.Width(50))){
                reallyReset = false;
            }
        }
        else if(generatorParameters[selected_generator] != null && beginSave){
            GUILayout.Label("Config. Name:");
            saveName = GUILayout.TextField(saveName, 20, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Save", "button", GUILayout.Width(100))){
                //Save the configuration
                if (File.Exists(saveName+".param"))
                {
                    Debug.Log("File '"+saveName+"'' already exists.");
                    return;
                }
                var sr = File.CreateText(saveName+".param");
                foreach(GeneratorParameter p in generatorParameters[selected_generator]){
                    sr.WriteLine(p.field.Name+":"+p.type+":"+p.currentValue);
                }
                sr.Close();

                beginSave = false;
            }
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Cancel", "button", GUILayout.Width(100))){
                beginSave = false;
            }
        }
        else if(generatorParameters[selected_generator] != null && beginLoad){
            if(possibleLoadFiles.Count == 0){
                GUILayout.Label("Found no files to load!");
                if(GUILayout.Button("Cancel", "button", GUILayout.Width(100))){
                    beginLoad = false;
                }
            }
            else{
                GUILayout.Label("File:", body_text_style_center);
                selectedFile = EditorGUILayout.Popup("", selectedFile, possibleLoadFiles.ToArray(), GUILayout.Width(250));

                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Load", "button", GUILayout.Width(100))){
                    //Load the configuration
                    string line;
                    StreamReader theReader = new StreamReader(possibleLoadFiles[selectedFile]);
                    using (theReader){
                        do{
                            line = theReader.ReadLine();
                            if (line != null){
                                //Parse the line
                                string[] parts = line.Split(':');
                                foreach(GeneratorParameter p in generatorParameters[selected_generator]){
                                    if(p.field.Name == parts[0]){
                                        p.ParseAndSetValue(parts[2]);
                                    }
                                }
                            }
                        }
                        while (line != null);
                    }
                    theReader.Close();

                    beginLoad = false;
                }
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Cancel", "button", GUILayout.Width(100))){
                    beginLoad = false;
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
    }

    /*
        Functionality Stuff
        This should probably be moved out in the future, but I'm keeping it local until we port it.
    */

    public void LoadAllMetrics(){
        metricList = new List<GeneratorMetric>();
        List<string> list_axis_options = new List<string>();

        metricTargets = new List<float>();
        metricInputs = new List<string>();

        UnityEngine.Object[] obs = FindObjectsOfType(typeof(UnityEngine.Object));
        foreach(UnityEngine.Object o in obs){
            foreach(MethodInfo method in o.GetType().GetMethods()){
                foreach(Attribute attr in method.GetCustomAttributes(false)){
                    if(attr is Metric){
                        //This check is a hangover from the old Danesh... I don't think we need this requirement actually. Keeping it for now.
                        if(method.IsStatic){
                            GeneratorMetric gm = new GeneratorMetric(method, ((Metric)attr).Name , ((Metric)attr).Target);
                            if(metricList.Count < standardColors.Length){
                                gm.color = standardColors[metricList.Count];
                            }
                            metricList.Add(gm);
                            list_axis_options.Add(((Metric)attr).Name);
                            metricTargets.Add(0f);
                            metricInputs.Add("0");
                        }
                    }
                }
            }
        }

        axis_options = list_axis_options.ToArray();

        //refresh metric list
        SetupGeneratorMetrics();
    }

    public void LoadAllGenerators(){
        generatorObjects = new List<object>();
        generatorMethods = new List<MethodInfo>();
        visualiserList = new List<GeneratorVisualiser>();
        // visualiserObjects = new List<object>();
        // visualiserMethods = new List<MethodInfo>();
        List<string> names = new List<string>();
        List<string> vnames = new List<string>();
        generatorParameters = new List<List<GeneratorParameter>>();
        List<GeneratorParameter> ps;

        UnityEngine.Object[] obs = FindObjectsOfType(typeof(UnityEngine.Object));
        foreach(UnityEngine.Object o in obs){
            bool generatorFoundHere = false;
            foreach(MethodInfo method in o.GetType().GetMethods()){
                foreach(Attribute attr in method.GetCustomAttributes(false)){
                    if(attr is Generator){
                        generatorObjects.Add((object)o);
                        generatorMethods.Add(method);
                        names.Add(o.GetType()+"."+method.Name);
                        generatorFoundHere = true;
                    }
                    if(attr is Visualiser){
                        visualiserList.Add(new GeneratorVisualiser(o.GetType()+"."+method.Name, (object)o, method));
                        vnames.Add(o.GetType()+"."+method.Name);
                    }
                }
            }
            /*
                Before, we only searched for a single generator. Now we support multiples, we assume
                that fields are defined within the same file as the generator. We can broaden this
                later but it saves us a lot of time now.
            */
            if(generatorFoundHere){
                ps = new List<GeneratorParameter>();
                foreach(FieldInfo field in o.GetType().GetFields()){
                    foreach(Attribute _attr in field.GetCustomAttributes(false)){
                        if(_attr is TunableAttribute){
                            TunableAttribute attr = (TunableAttribute) _attr;
                            ps.Add(new GeneratorParameter(attr.Name, field.GetValue(o), attr.MinValue, attr.MaxValue, field, o));
                        }
                    }
                }
                generatorParameters.Add(ps);
            }
        }

        //Setup visualisers, since we added new ones and are about to generate filler content
        SetupGeneratorVisualisers();

        if(generatorObjects.Count > 0){
            reraTextures = new Texture2D[generatorObjects.Count];
            eraTextures = new Texture2D[generatorObjects.Count];
            eraTexturesAnnotated = new Texture2D[generatorObjects.Count];
            eraParams = new List<float[]>();
            eraSamples = new List<List<GeneratorSample[,]>>[generatorObjects.Count];
            reraSamples = new List<List<GeneratorSample[,]>>[generatorObjects.Count];
            visualisedContent = new Texture2D[generatorObjects.Count];
            visualisedContentGrid = new Texture2D[generatorObjects.Count, 9];

            pastERATextures = new List<Texture2D[]>();
            pastRERATextures = new List<Texture2D[]>();
            pastContentRect = new Rect[sizeOfERAHistory];

            eranalyser = new ExpressiveRangeAnalysis(generatorObjects.Count);
            xAxesRERA = new int[generatorObjects.Count];
            xAxesERA = new int[generatorObjects.Count];
            yAxesRERA = new int[generatorObjects.Count];
            yAxesERA = new int[generatorObjects.Count];

            for(int i=0; i<generatorObjects.Count; i++){
                pastERATextures.Add(new Texture2D[sizeOfERAHistory]);
                pastRERATextures.Add(new Texture2D[sizeOfERAHistory]);
                eraParams.Add(new float[1]);
                //This technically causes a crash if you have one metric, but I think we're good?
                //It's a lot nicer, is the thing, because it doesn't default to showing you the same metric for X and Y.
                yAxesERA[i] = 1;
                yAxesRERA[i] = 1;
            }
            GenerateAndShowContent();
        }

        listOfVisualiserNames = vnames.ToArray();
        listOfGeneratorNames = names.ToArray();

        //refresh metric list
        SetupGeneratorMetrics();
    }

    public void GenerateAndShowContent(){
        //Hacky code to find the nearest power of two that fits into the right-hand tab... this code is definitely up for chopping
        float width = (position.width * (1 - tab_width - 0.1f));
        if(!showSingleOutput)
            width /= 3;
        float p = 2;
        double diff = Math.Pow(p, 2) - width;
        while(diff < 0){
            p++;
            diff = Math.Pow(p, 2) - width;
        }
        p--;

        if(showSingleOutput){
            object content = GenerateContent();

            foreach(GeneratorMetric g in GetMetricsForActiveGenerator()){
                g.currentValue = (float) g.method.Invoke(null, new object[]{content});
            }

            visualisedContent[selected_generator] = (Texture2D) visualisersPerGenerator[selected_generator][selected_visualiser].method.Invoke(visualisersPerGenerator[selected_generator][selected_visualiser].targetObject, new object[]{content});

            //Then scale the texture up (even though we pass a fixed-size texture to the visualiser we can't guarantee it comes back like that, so)
            TextureScale.Point(visualisedContent[selected_generator], (int)Mathf.Pow(p, 2), (int)Mathf.Pow(p, 2));
        }
        else{
            List<GeneratorMetric> ms = GetMetricsForActiveGenerator();
            float[] m_avgs = new float[ms.Count];

            //generate nine pieces of content, evaluate their average metric values, visualise them
            for(int i=0; i<9; i++){
                object content = GenerateContent();

                visualisedContentGrid[selected_generator, i] = (Texture2D) visualisersPerGenerator[selected_generator][selected_visualiser].method.Invoke(visualisersPerGenerator[selected_generator][selected_visualiser].targetObject, new object[]{content});
                TextureScale.Point(visualisedContentGrid[selected_generator, i], (int)Mathf.Pow(p, 2), (int)Mathf.Pow(p, 2));

                for(int m=0; m<ms.Count; m++){
                    m_avgs[m] += (float) ms[m].method.Invoke(null, new object[]{content});
                }
            }

            for(int m=0; m<ms.Count; m++){
                ms[m].currentValue = m_avgs[m]/9f;
            }
        }
    }

    public object GenerateContent(){
        try{
            return generatorMethods[selected_generator].Invoke(generatorObjects[selected_generator], new object[]{});
        }
        catch{
            return null;
        }
    }

    //Again, idk, maybe we just use a list of list of strings and statically calculate this? I think that's sensible but
    //it felt like overkill implementationally for just prototyping and testing
    public string[] GetMetricNamesForActiveGenerator(){
        List<GeneratorMetric> ms = GetMetricsForActiveGenerator();
        string[] res = new string[ms.Count];
        for(int i=0; i<ms.Count; i++){
            res[i] = ms[i].name;
        }
        return res;
    }

    /*
        For each generator, take all visualisers and test to see if they can process a piece of content
        without exceptions. Note this doesn't assess whether the output is meaningful, obviously.
    */

    public void SetupGeneratorVisualisers(){

        visualisersPerGenerator = new List<List<GeneratorVisualiser>>();
        visualiserNamesPerGenerator = new List<string[]>();

        for(int i=0; i<generatorMethods.Count; i++){
            List<GeneratorVisualiser> visualisers = new List<GeneratorVisualiser>();
            List<string> names = new List<string>();
            MethodInfo m = generatorMethods[i];
            if(visualiserList != null){
                foreach(GeneratorVisualiser vis in visualiserList){
                    bool visualiserValid = true;
                    try{
                        object o = m.Invoke(generatorObjects[i], new object[]{});
                        vis.method.Invoke(vis.targetObject, new object[]{o});
                    }
                    catch{
                        visualiserValid = false;
                    }
                    if(visualiserValid){
                        visualisers.Add(vis);
                        names.Add(vis.name);
                    }
                }
            }

            visualisersPerGenerator.Add(visualisers);
            visualiserNamesPerGenerator.Add(names.ToArray());
        }
    }

    /*
        Called whenever new generators or metrics are loaded.
        For each generator, take all the metrics and test to see if they can process a piece of content
        without error. If so, add it to a list so they can be referred to later.
    */
    public void SetupGeneratorMetrics(){

        if(generatorMethods == null){
            return;
        }

        generatorMetricsPerGenerator = new List<List<GeneratorMetric>>();

        for(int i=0; i<generatorMethods.Count; i++){
            List<GeneratorMetric> metrics = new List<GeneratorMetric>();
            MethodInfo m = generatorMethods[i];
            if(metricList != null){
                foreach(GeneratorMetric metric in metricList){
                    bool metricValid = true;
                    try{
                        object o = m.Invoke(generatorObjects[i], new object[]{});
                        metric.method.Invoke(null, new object[]{o});
                    }
                    catch{
                        metricValid = false;
                    }
                    if(metricValid){
                        metrics.Add(metric);
                    }
                }
            }
            generatorMetricsPerGenerator.Add(metrics);
        }

    }

    public List<GeneratorMetric> GetMetricsForActiveGenerator(){

        if(generatorMetricsPerGenerator == null){
            SetupGeneratorMetrics();
        }

        return generatorMetricsPerGenerator[selected_generator];
    }

    public void PerformERAOnActiveGenerator(){
        if(eranalyser == null){
            eranalyser = new ExpressiveRangeAnalysis(generatorMethods.Count);
        }
        eranalyser.StartERA(this, numberOfERARuns, selected_generator);

        if(eraTextures[selected_generator] != null){
            int i=0;
            for(i=0; i<pastERATextures[selected_generator].Length; i++){
                if(pastERATextures[selected_generator][i] == null){
                    TextureScale.Point(eraTextures[selected_generator], sizeOfHistoryTexture, sizeOfHistoryTexture);
                    AddMiniSparklines(eraTextures[selected_generator], eraParams[selected_generator]);
                    AddMiniAxisSparklines(eraTextures[selected_generator], metricList[xAxesERA[selected_generator]].color, metricList[yAxesERA[selected_generator]].color);
                    pastERATextures[selected_generator][i] = eraTextures[selected_generator];
                    break;
                }
            }
            if(i == pastERATextures[selected_generator].Length){
                for(i=0; i<pastERATextures[selected_generator].Length-1; i++){
                    pastERATextures[selected_generator][i] = pastERATextures[selected_generator][i+1];
                }
                TextureScale.Point(eraTextures[selected_generator], sizeOfHistoryTexture, sizeOfHistoryTexture);
                AddMiniSparklines(eraTextures[selected_generator], eraParams[selected_generator]);
                AddMiniAxisSparklines(eraTextures[selected_generator], metricList[xAxesERA[selected_generator]].color, metricList[yAxesERA[selected_generator]].color);
                pastERATextures[selected_generator][pastERATextures[selected_generator].Length-1] = eraTextures[selected_generator];
            }
        }

        //Get the current params and record them for sparklines
        List<GeneratorParameter> ps = generatorParameters[selected_generator];
        eraParams[selected_generator] = new float[ps.Count];
        for(int i=0; i<ps.Count; i++){
            eraParams[selected_generator][i] = ps[i].GetSparklineValue();
        }

        eraTextures[selected_generator] = eranalyser.GenerateERAGraphForAxes(selected_generator,xAxesERA[selected_generator], yAxesERA[selected_generator]);
        eraTexturesAnnotated[selected_generator] = eranalyser.GenerateERAGraphForAxes(selected_generator,xAxesERA[selected_generator], yAxesERA[selected_generator]);
        AddAxisSparklines(eraTexturesAnnotated[selected_generator], metricList[xAxesERA[selected_generator]].color, metricList[yAxesERA[selected_generator]].color);
        AddSparklines(eraTexturesAnnotated[selected_generator], eraParams[selected_generator]);

        eraSamples[selected_generator] = eranalyser.eraSamples;
        showingERA = true;
    }

    public void ShowERAorRERA(){
        if(showingERA){
            eraTextures[selected_generator] = eranalyser.GenerateERAGraphForAxes(selected_generator,xAxesERA[selected_generator], yAxesERA[selected_generator]);
            eraTexturesAnnotated[selected_generator] = eranalyser.GenerateERAGraphForAxes(selected_generator,xAxesERA[selected_generator], yAxesERA[selected_generator]);
            AddAxisSparklines(eraTexturesAnnotated[selected_generator], metricList[xAxesERA[selected_generator]].color, metricList[yAxesERA[selected_generator]].color);
            AddSparklines(eraTexturesAnnotated[selected_generator], eraParams[selected_generator]);
        }
        else{
            reraTextures[selected_generator] = eranalyser.GenerateRERAGraphForAxes(selected_generator,xAxesRERA[selected_generator], yAxesRERA[selected_generator]);
        }
    }

    public void PerformRERAOnActiveGenerator(){
        if(eranalyser == null){
            eranalyser = new ExpressiveRangeAnalysis(generatorMethods.Count);
        }

        QuickSave();

        eranalyser.StartRERA(this, numberOfRERARuns, selected_generator);

        if(reraTextures[selected_generator] != null){
            int i=0;
            for(i=0; i<pastRERATextures[selected_generator].Length; i++){
                if(pastRERATextures[selected_generator][i] == null){
                    TextureScale.Point(reraTextures[selected_generator], sizeOfHistoryTexture, sizeOfHistoryTexture);
                    pastRERATextures[selected_generator][i] = reraTextures[selected_generator];
                    break;
                }
            }
            if(i == pastRERATextures[selected_generator].Length){
                for(i=0; i<pastRERATextures[selected_generator].Length-1; i++){
                    pastRERATextures[selected_generator][i] = pastRERATextures[selected_generator][i+1];
                }
                TextureScale.Point(reraTextures[selected_generator], sizeOfHistoryTexture, sizeOfHistoryTexture);
                pastRERATextures[selected_generator][pastRERATextures[selected_generator].Length-1] = reraTextures[selected_generator];
            }
        }

        reraTextures[selected_generator] = eranalyser.GenerateRERAGraphForAxes(selected_generator,xAxesRERA[selected_generator], yAxesRERA[selected_generator]);
        reraSamples[selected_generator] = eranalyser.eraSamples;

        QuickLoad();

        showingERA = false;
    }

    //This is so that we can ask for metric calls elsewhere, e.g. from ERA, however this currently assumes the output is
    //always a float, which we might not want to be the case. Okay for now, perhaps?
    public float GetMetric(int index, object[] content){
        GeneratorMetric m = metricList[index];
        try{
            return (float) m.method.Invoke(null, content);
        }
        catch{
            return 0f;
        }
        return 0f;
    }

    public List<GeneratorParameter> GetParametersForActiveGenerator(){
        return generatorParameters[selected_generator];
    }

    /*
        organised programmers don't read this






        ...deadline programmers, hello
    */
    public void ConfigureGenerator(float mx, float my, Rect eraRect, List<List<GeneratorSample[,]>>[] samples, int x_axis_era, int y_axis_era){

        float tex_mx = mx - eraRect.x;
        float tex_my = my - eraRect.y;

        float width = 500;
        float height = 500;

        int x = (int)Mathf.Floor(100*((tex_mx/width)))-1;
        if(x < 0) x = 0;
        if(x > 99) x = 99;
        int y = 100-(int)Mathf.Floor(100*((tex_my/height)));
        if(y < 0) y = 0;
        if(y > 99) y = 99;

        Texture2D last_era_preview = null;
        //If the cursor target is null, be generous and find an adjacent one, because it's really hard to find the exact spot right now
        if(samples[selected_generator][x_axis_era][y_axis_era][x,y] != null){
            for(int i=-1; i<2; i++){
                for(int j=-1; j<2; j++){
                    if((i != 0 || j != 0) && x+i >= 0 && x+i < 100 && y+j >=0 && y+j < 100 && samples[selected_generator][x_axis_era][y_axis_era][x+i,y+j] != null){
                        x = x+i;
                        y = y+j;
                        break;
                    }
                }
            }
        }

        object[] vs = samples[selected_generator][x_axis_era][y_axis_era][x,y].parameterValues;
        List<GeneratorParameter> ps = GetParametersForActiveGenerator();
        for(int i=0; i< vs.Length; i++){
            ps[i].SetValue(vs[i]);
        }

        GenerateAndShowContent();
    }

    public void AddSparklines(Texture2D tex, float[] data){
        Color c = new Color(1f, 1f, 1f, 1f);
        for(int i=0; i<data.Length; i++){
            int len = (int)Mathf.Round(data[i] * (float)sparklineMaxLength);
            for(int x=sparklineSpacing; x<sparklineSpacing+len; x++){
                for(int y=0; y<sparklineWidth; y++){
                    tex.SetPixel(x, tex.height - (sparklineSpacing*(i+2)) - (sparklineWidth*i) - y, new Color(1f, 1f, 1f, 1f));
                }
            }
        }
        tex.Apply();
    }

    public void AddMiniSparklines(Texture2D tex, float[] data){
        Color c = new Color(1f, 1f, 1f, 1f);
        for(int i=0; i<data.Length; i++){
            int len = (int)Mathf.Round(data[i] * (float)miniSparklineMaxLength);
            for(int x=2; x<2+len; x++){
                for(int y=0; y<miniSparklineWidth; y++){
                    tex.SetPixel(x, tex.height - (miniSparklineSpacing*(i+2)) - (miniSparklineWidth*i) - y, new Color(1f, 1f, 1f, 1f));
                }
            }
        }
        tex.Apply();
    }

    public void AddAxisSparklines(Texture2D tex, Color x, Color y){
        for(int i=0; i<sparklineMaxLength; i++){
            for(int j=0; j<sparklineWidth; j++){
                tex.SetPixel(sparklineSpacing+j, sparklineSpacing+i, y);
                tex.SetPixel(sparklineSpacing+i, sparklineSpacing+j, x);
            }
        }
        tex.Apply();
    }

    public void AddMiniAxisSparklines(Texture2D tex, Color x, Color y){
        for(int i=0; i<miniSparklineMaxLength; i++){
            for(int j=0; j<miniSparklineWidth; j++){
                tex.SetPixel(miniSparklineSpacing+j, miniSparklineSpacing+i, y);
                tex.SetPixel(miniSparklineSpacing+i, miniSparklineSpacing+j, x);
            }
        }
        tex.Apply();
    }

    public void DisplayERASample(float mx, float my, Rect eraRect, List<List<GeneratorSample[,]>>[] samples, int x_axis_era, int y_axis_era){

        float tex_mx = mx - eraRect.x;
        float tex_my = my - eraRect.y;

        float width = 500;
        float height = 500;

        int x = (int)Mathf.Floor(100*((tex_mx/width)))-1;
        if(x < 0) x = 0;
        if(x > 99) x = 99;
        int y = 100-(int)Mathf.Floor(100*((tex_my/height)));
        if(y < 0) y = 0;
        if(y > 99) y = 99;

        Texture2D last_era_preview = null;

        if(samples[selected_generator][x_axis_era][y_axis_era][x,y] != null){// && (x != last_era_x || y != last_era_y)){
            last_era_preview = (Texture2D) visualisersPerGenerator[selected_generator][selected_visualiser].method.Invoke(visualisersPerGenerator[selected_generator][selected_visualiser].targetObject, new object[]{samples[selected_generator][x_axis_era][y_axis_era][x,y].content});
            TextureScale.Point(last_era_preview, 250, 250);
            last_era_preview.Apply();
            Repaint ();
        }
        if(samples[selected_generator][x_axis_era][y_axis_era][x,y] == null){// && (x != last_era_x || y != last_era_y)){
            last_era_preview = null;
        }
        if(last_era_preview != null){
            if(height - tex_my < 250){
                GUI.DrawTexture(new Rect(eraRect.x + (width/2f - 125), eraRect.y, 250, 250), last_era_preview);
            }
            else{
                GUI.DrawTexture(new Rect(eraRect.x + (width/2f - 125), eraRect.y+height-250, 250, 250), last_era_preview);
            }
        }

    }

    /*
        Utility
    */
    public Texture2D MakeSinglePixelTexture(Color c){
        Color[] pix = new Color[1];
        for(int i = 0; i < pix.Length; i++) {
            pix[i] = c;
        }
        Texture2D result = new Texture2D(1, 1);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    //Thanks to https://forum.unity3d.com/threads/change-gui-box-color.174609/
    private Texture2D MakeTex( int width, int height, Color col )
    {
        Color[] pix = new Color[width * height];
        for( int i = 0; i < pix.Length; ++i )
        {
            pix[ i ] = col;
        }
        Texture2D result = new Texture2D( width, height );
        result.SetPixels( pix );
        result.Apply();
        return result;
    }

    public bool WithinRect(float x, float y, Rect r){
        if(x > r.x && y > r.y && x < r.x+r.width && y < r.y+r.height){
            return true;
        }
        return false;
    }

    //THE FUN ZONE
    public string GetRandomConfigName(){
        string[] occupations = new string[]{"accountant", "actor", "actuary", "adjudicator", "agent", "anesthesiologist", "animator", "anthropologist", "arbitrator", "archeologist", "architect", "archivist", "artist", "assembler", "astronomer", "athlete", "attendant", "audiologist", "auditor", "author", "bailiff", "baker", "barback", "barber", "bartender", "bellhop", "biochemist", "biophysicist", "blaster", "blockmason", "boilermaker", "bookkeeper", "brazer", "brickmason", "butcher", "buyer", "cabinetmaker", "carpenter", "cartographer", "cashier", "caster", "chauffeur", "checker", "chef", "chemist", "chiropractor", "choreographer", "cleaner", "composer", "concierge", "conciliator", "conservator", "cook", "correspondent", "cosmetologist", "counselor", "courier", "curator", "cutter", "dancer", "demonstrator", "dentist", "designer", "detective", "dietitian", "director", "dishwasher", "dispatcher", "drafter", "dressmaker", "economist", "editor", "electrician", "embalmer", "engineer", "engraver", "epidemiologist", "escort", "etcher", "fabricator", "faller", "farmer", "farmworker", "firefighter", "fisher", "forester", "geographer", "geoscientist", "glazier", "groundskeeper", "gynecologist", "hairdresser", "hairstylist", "historian", "host", "hostess", "hostler", "hunter", "hydrologist", "illustrator", "inspector", "instructor", "internist", "interpreter", "interviewer", "investigator", "jailer", "janitor", "jeweler", "judge", "lawyer", "legislator", "librarian", "lifeguard", "locksmith", "logistician", "machinist", "magistrate", "maid", "manicurist", "mathematician", "measurer", "mediator", "messenger", "microbiologist", "millwright", "model", "molder", "mortician", "musician", "nutritionist", "obstetrician", "offbearer", "optician", "optometrist", "orderly", "orthodontist", "orthotist", "paperhanger", "paralegal", "paramedic", "pediatrician", "pedicurist", "pharmacist", "photogrammetrist", "photographer", "physician", "pipefitter", "pipelayer", "plasterer", "plumber", "podiatrist", "postmaster", "priest", "producer", "proofreader", "prosthetist", "prosthodontist", "psychiatrist", "psychologist", "rancher", "receptionist", "referee", "reporter", "rigger", "roofer", "roustabout", "sailor", "sampler", "scaler", "sculptor", "secretary", "shampooer", "shaper", "shipmate", "singer", "slaughterer", "sociologist", "solderer", "sorter", "statistician", "steamfitter", "stonemason", "surgeon", "surveyor", "tailor", "taper", "teacher", "telemarketer", "teller", "tester", "therapist", "translator", "trapper", "trimmer", "tuner", "typist", "umpire", "undertaker", "upholsterer", "usher", "veterinarian", "waiter", "waitress", "weigher", "welder", "woodworker", "writer", "yardmaster", "zoologist"};
        string[] animals = new string[]{"aardvark", "alligator", "alpaca", "antelope", "armadillo", "baboon", "badger", "bat", "bear", "beaver", "bison", "boar", "buffalo", "bull", "camel", "canary", "capybara", "cat", "chameleon", "cheetah", "chimpanzee", "chinchilla", "chipmunk", "cougar", "cow", "coyote", "crocodile", "crow", "deer", "dingo", "dog", "donkey", "dromedary", "elephant", "elk", "ewe", "ferret", "finch", "fish", "fox", "frog", "gazelle", "giraffe", "gnu", "goat", "gopher", "gorilla", "hamster", "hedgehog", "hippopotamus", "hog", "horse", "hyena", "ibex", "iguana", "impala", "jackal", "jaguar", "kangaroo", "koala", "lamb", "lemur", "leopard", "lion", "lizard", "llama", "lynx", "mandrill", "marmoset", "mink", "mole", "mongoose", "monkey", "moose", "mouse", "mule", "muskrat", "mustang", "newt", "ocelot", "opossum", "orangutan", "oryx", "otter", "ox", "panda", "panther", "parakeet", "parrot", "pig", "platypus", "porcupine", "porpoise", "puma", "rabbit", "raccoon", "ram", "rat", "reindeer", "reptile", "rhinoceros", "salamander", "seal", "sheep", "shrew", "skunk", "sloth", "snake", "squirrel", "tapir", "tiger", "toad", "turtle", "walrus", "warthog", "weasel", "whale", "wildcat", "wolf", "wolverine", "wombat", "woodchuck", "yak", "zebra"};
        string job = occupations[UnityEngine.Random.Range(0, occupations.Length)];
        return animals[UnityEngine.Random.Range(0, animals.Length)]+job.Substring(0,1).ToUpper() + job.Substring(1);
    }
    //THE FUN ZONE ENDS

    List<string> quickSaveNames = new List<string>();
    List<string> quickSaveValues = new List<string>();

    public void QuickSave(){
        //Preserve the current state of the generator, so we can restore afterwards
        quickSaveNames = new List<string>();
        quickSaveValues = new List<string>();
        foreach(GeneratorParameter p in generatorParameters[selected_generator]){
            quickSaveNames.Add(p.field.Name);
            quickSaveValues.Add(p.currentValue+"");
        }
    }

    public void QuickLoad(){
        for(int i=0; i<quickSaveNames.Count; i++){
            foreach(GeneratorParameter p in generatorParameters[selected_generator]){
                if(p.field.Name == quickSaveNames[i]){
                    p.ParseAndSetValue(quickSaveValues[i]);
                }
            }
        }
    }

    /*
        Experimental - not in Danesh currently but feel free to play around
    */

    //This snaps a screenshot of an ERA at regular intervals through a parameter change, allowing you to animate
    //the effect of changing a parameter on the expressive range. It's pretty cool! We might add it in future.
    public void GifIt(GeneratorParameter p){
        if(eranalyser == null){
        eranalyser = new ExpressiveRangeAnalysis(generatorMethods.Count);
        }
        int framenum = 0;
        for(float f=0; f<1f; f+=0.05f){
            p.SetValueFractional(f);
            eranalyser.StartERA(this, numberOfERARuns, selected_generator);
            Texture2D frame = eranalyser.GenerateERAGraphForAxes(selected_generator,xAxesERA[selected_generator], yAxesERA[selected_generator]);
            byte[] bytes = frame.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/frame-" + framenum + ".png", bytes);
            framenum++;
        }
    }
}

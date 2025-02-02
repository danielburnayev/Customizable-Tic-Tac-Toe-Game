using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public Camera menuCamera;
    
    int buttonSelected;
    int deadSpacesPlaced = 0;
    int maxNumOfDeadSpaces;
    int displayBoardNum = 0;
    int deadCounter;
    float boxWidthDecider = 3;
    float previousWidthDecider = 3;
    float boxHeightDecider = 3;
    float previousHeightDecider = 3;
    float inARowToWinDecider = 3;
    float previousInARowToWinDecider = 3;
    float maxDimensionDecider = 10;
    float previousMaxDimensionDecider = 10;
    float buttonTimePassed;
    float navigatingMenuCounter;
    bool placeDeadSpacesRandomly;
    bool deadSpacesAreCalculated = true;
    bool widthSliderTouched;
    bool heightSliderTouched;
    bool inARowSliderTouched;
    bool maxDimensionTouched;
    string menuLocation = "main menu";
    string boxWidthString;
    string boxHeightString;
    string inARowToWinString;
    string maxDimensionString;
    string widthDescriptor;
    string heightDescriptor;
    string inARowDescriptor;
    string deadSpacesDisplayer;
    string deadSpacesDescriptor;
    string maxDimensionDescriptor;
    string calculatedDeadSpaceDescriptor;
    GameObject marks;

    string[] buttonNames = new string[] {"X and O", "Green and Red"};
    List<GameObject> deadSpacesObjects = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        if (menuLocation != "in pvp arena") {
            GameObject[] markChangingObjects = GameObject.FindGameObjectsWithTag("MarkChangingGameObject");
            if (markChangingObjects.Length > 1) {
                Destroy(markChangingObjects[1]);
            }
            marks = markChangingObjects[0];
            if (marks == null) {
                marks = GameObject.Find("Xs and Os selected");
            }

            GameObject[] dimensionChangingObjects  = GameObject.FindGameObjectsWithTag("DimensionChangingGameObject");
            if (dimensionChangingObjects.Length > 1) {
                Destroy(dimensionChangingObjects[1]);
            }
            maxDimensionDecider = GameObject.Find("Dimension Changing GameObject").transform.position.x;

            GameObject[] deadSpaceChangingObjects  = GameObject.FindGameObjectsWithTag("DeadSpaceChangingGameObject");
            if (deadSpaceChangingObjects.Length > 1) {
                Destroy(deadSpaceChangingObjects[1]);
            }
            if (deadSpaceChangingObjects[0].layer == 0) {
                deadSpacesAreCalculated = false;
            }
            else if (deadSpaceChangingObjects[0].layer == 1) {
                deadSpacesAreCalculated = true;
            }

            GameObject[] navigatingMenuObjects  = GameObject.FindGameObjectsWithTag("NavigatingMenuObjects");
            if (navigatingMenuObjects.Length > 1) {
                Destroy(navigatingMenuObjects[1]);
            }

            if (menuLocation == "pvp menu") {
                createSampleTicTacToeBoard();
            }
            else if (menuLocation == "settings menu") {
                previousMaxDimensionDecider = GameObject.Find("Dimension Changing GameObject").transform.position.x;
            }

            if (navigatingMenuCounter == 0f) {
                LoadMenuData();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (menuLocation != "in pvp arena") {
            if (menuLocation == "pvp menu") {
                updateSampleBoardDimensions();
                updateInARowToWin();
                showSpecificBlockCursorIsOver();

                if (widthSliderTouched || heightSliderTouched || inARowSliderTouched) {
                    if (buttonTimePassed > 0f) {
                        buttonTimePassed += Time.deltaTime;
                        if (buttonTimePassed >= 0.15f) {
                            buttonTimePassed = 0f;
                        }
                    }
                }
                
                if (Input.GetMouseButtonUp(0)) {
                    placingDeadSpacesOnSampleBoard(Input.mousePosition.x, Input.mousePosition.y);
                }
                if (placeDeadSpacesRandomly) {
                    cyclingToPlaceDeadSpacesRandomly();
                    placeDeadSpacesRandomly = false;
                }
            }
            else if (menuLocation == "settings menu") {
                updateMaxDimensions();
                if (maxDimensionTouched) {
                    if (buttonTimePassed > 0f) {
                        buttonTimePassed += Time.deltaTime;
                        if (buttonTimePassed >= 0.15f) {
                            buttonTimePassed = 0f;
                        }
                    }
                }
            }
        }
    }
    
    void OnGUI() {
        if (menuLocation == "main menu") {
            if (GUI.Button(new Rect((float)(Screen.width*0.667), (float)(Screen.height/2)-50f, 200f, 100f), "Player vs. Player")) {
                GameObject newMenuObject = new GameObject("pvp menu", typeof(MainMenuScript));
                MainMenuScript playerVsPlayerMenu = newMenuObject.GetComponent<MainMenuScript>();
                playerVsPlayerMenu.addAndStoreNavigatingMenuCounter();
                playerVsPlayerMenu.setMenuLocation("pvp menu");
                playerVsPlayerMenu.setTypeOfMarkButtonNumAndGameObject(marks);
                playerVsPlayerMenu.setCamera(menuCamera);
                Destroy(this.gameObject);
            }
            if (GUI.Button(new Rect((float)(Screen.width*0.334)-200f, (float)(Screen.height/2)-50f, 200f, 100f), "Settings")) {
                GameObject newMenuObject = new GameObject("settings menu", typeof(MainMenuScript));
                MainMenuScript settingsMenu = newMenuObject.GetComponent<MainMenuScript>();
                settingsMenu.addAndStoreNavigatingMenuCounter();
                settingsMenu.setMenuLocation("settings menu");
                settingsMenu.setTypeOfMarkButtonNumAndGameObject(marks);
                settingsMenu.setCamera(menuCamera);
                Destroy(this.gameObject);
            }
            if (GUI.Button(new Rect((float)(Screen.width/2)-100f, (float)(Screen.height)-150f, 200f, 100f), "Exit")) {
                Debug.Log("Game exited");
                Application.Quit();
            }
        }
        else if (menuLocation == "settings menu") {
            settingUpMarkChangingGUIForSettings();
            settingUpMaxDimensionGUIForSettings();
            settingUpCaculatingDeadSpacesGUIForSettings();

            if (GUI.Button(new Rect((float)(Screen.width/2)-100f, (float)(Screen.height)-150f, 200f, 100f), "Main Menu")) {
                GameObject newMenuObject = new GameObject("main menu", typeof(MainMenuScript));
                MainMenuScript mainMenu = newMenuObject.GetComponent<MainMenuScript>();
                mainMenu.addAndStoreNavigatingMenuCounter();
                mainMenu.setMenuLocation("main menu");
                mainMenu.setTypeOfMarkButtonNumAndGameObject(marks);
                mainMenu.setCamera(menuCamera);
                SaveMenuData();
                Destroy(this.gameObject); 
            }
        }
        else if (menuLocation == "pvp menu") {
            settingUpWidthGUIForPVP();
            settingUpHeightGUIForPVP();
            settingUpInARowToWinGUIForPVP();
            settingUpRandomizeAllButtonForPVP();
            settingUpDeadSpaceGUIForPVP();
            
            if (GUI.Button(new Rect((float)Screen.width - 225f, (float)(Screen.height/2f) - 50f, 200f, 100f), "Play")) {
                GameObject boxHeightObject = new GameObject("boxHeightDecider");
                GameObject boxWidthObject = new GameObject("boxWidthDecider");
                GameObject inARowToWinObject = new GameObject("inARowToWinDecider");
                GameObject deadSpacesObject = new GameObject("deadSpacesDisplayer");
                boxHeightObject.transform.position = new Vector3((float)((int)boxHeightDecider), 0f, 0f);
                boxHeightObject.tag = "Board";
                boxWidthObject.transform.position = new Vector3((float)((int)boxWidthDecider), 0f, 0f);
                boxWidthObject.tag = "Board";
                inARowToWinObject.transform.position = new Vector3((float)((int)inARowToWinDecider), 0f, 0f);
                inARowToWinObject.tag = "Board";
                deadSpacesObject.transform.position = new Vector3((float)(deadSpacesPlaced), 0f, 0f);
                deadSpacesObject.tag = "Board";
                SceneManager.LoadScene("PlayerVsPlayerBoard", LoadSceneMode.Single);
                DontDestroyOnLoad(marks);
                DontDestroyOnLoad(boxHeightObject);
                DontDestroyOnLoad(boxWidthObject);
                DontDestroyOnLoad(inARowToWinObject);
                DontDestroyOnLoad(deadSpacesObject);
                for (int i = 0; i < deadSpacesObjects.Count; i++) {
                    DontDestroyOnLoad(deadSpacesObjects[i]);
                }
                DontDestroyOnLoad(GameObject.Find("Dimension Changing GameObject"));
                DontDestroyOnLoad(GameObject.Find("Dead Space Changing GameObject"));
                DontDestroyOnLoad(GameObject.Find("Navigating Menu Counter"));
                menuLocation = "in pvp arena";
                this.gameObject.name = "in pvp arena";
                DontDestroyOnLoad(this.gameObject);
            }

            if (GUI.Button(new Rect(25f, (float)(Screen.height)-125f, 200f, 100f), "Main Menu")) {
                GameObject newMenuObject = new GameObject("main menu", typeof(MainMenuScript));
                MainMenuScript mainMenu = newMenuObject.GetComponent<MainMenuScript>();
                mainMenu.addAndStoreNavigatingMenuCounter();
                mainMenu.setMenuLocation("main menu");
                mainMenu.setTypeOfMarkButtonNumAndGameObject(marks);
                mainMenu.setCamera(menuCamera);
                GameObject[] boardTagObjects = GameObject.FindGameObjectsWithTag("Display Board");
                for (int i = 0; i < boardTagObjects.Length; i++) {
                    Destroy(boardTagObjects[i]);
                }
                Destroy(this.gameObject); 
            }
        }
    }

    void OnApplicationQuit()
    {
        SaveMenuData();
    }

    private void createSampleTicTacToeBoard() {
        GameObject displayBoard = new GameObject("Board", typeof(SpriteRenderer));
        SpriteRenderer boardSpriteRend = displayBoard.GetComponent<SpriteRenderer>();
        boardSpriteRend.sprite = Resources.Load<Sprite>("Square");
        displayBoard.tag = "Display Board";
        boardSpriteRend.color = Color.white;
        boardSpriteRend.transform.localScale = new Vector3(2.5f * (int)boxWidthDecider, 2.5f * (int)boxHeightDecider, 1.0f);
            
        for (int rows = 0; rows < (int)boxHeightDecider; rows++) {
            for (int columns = 0; columns < (int)boxWidthDecider; columns++) {
                GameObject block = new GameObject("Display Block (" + rows + ", " + columns + ") " + displayBoardNum, typeof(SpriteRenderer));
                block.transform.position = new Vector3((float)((-1*((int)boxWidthDecider*2.5)/2) + (2.5/2) + 2.5*(columns)), (float)((((int)boxHeightDecider*2.5)/2) - (2.5/2) - 2.5*(rows)), 0f);
                block.transform.localScale = new Vector3(2.4f, 2.4f, 1.0f);
                block.tag = "Display Board";
                SpriteRenderer blockSp = block.GetComponent<SpriteRenderer>();
                Sprite blockSprite = Resources.Load<Sprite>("Square");
                blockSp.sprite = blockSprite;
                blockSp.sortingOrder = 1;
            }
        }
        
        for (int verticalLineNumber = 1; verticalLineNumber < (int)boxWidthDecider; verticalLineNumber++) {
            GameObject verticalLine = new GameObject("Vertical Line " + verticalLineNumber, typeof(SpriteRenderer));
            SpriteRenderer gridLineSp = verticalLine.GetComponent<SpriteRenderer>();
            Sprite theSprite = Resources.Load<Sprite>("Square");
            verticalLine.transform.position = new Vector3((float)((-1*((int)boxWidthDecider*2.5)/2) + 2.5*(verticalLineNumber)), 0f, 0f);
            verticalLine.transform.localScale = new Vector3(0.1875f, (float)(2.5*(int)boxHeightDecider), 1.0f);
            verticalLine.tag = "Display Board";
            gridLineSp.color = Color.black;
            gridLineSp.sprite = theSprite;
            gridLineSp.sortingOrder = 2;
        }
        for (int horizontalLineNumber = 1; horizontalLineNumber < (int)boxHeightDecider; horizontalLineNumber++) {
            GameObject horizontalLine = new GameObject("Horizontal Line " + horizontalLineNumber, typeof(SpriteRenderer));
            SpriteRenderer gridLineSp = horizontalLine.GetComponent<SpriteRenderer>();
            Sprite theSprite = Resources.Load<Sprite>("Square");
            horizontalLine.transform.Rotate(0f, 0f, 90f);
            horizontalLine.transform.position = new Vector3(0f, (float)((-1*((int)boxHeightDecider*2.5)/2) + 2.5*(horizontalLineNumber)), 0f);
            horizontalLine.transform.localScale = new Vector3(0.1875f, (float)(2.5*(int)boxWidthDecider), 1.0f);
            horizontalLine.tag = "Display Board";
            gridLineSp.color = Color.black;
            gridLineSp.sprite = theSprite;
            gridLineSp.sortingOrder = 2;
        }

        if (boxHeightDecider >= boxWidthDecider) {
            menuCamera.orthographicSize = (float)((((int)boxHeightDecider*2.5)/2) + 3.25);
        }
        else {
            menuCamera.orthographicSize = (float)(((((int)boxWidthDecider)*2.5)/2) + 1.625);
        }

        deadSpacesPlaced = 0;
        displayBoardNum++;
    }

    private void placingDeadSpacesOnSampleBoard(float xPosition, float yPosition) {
        GameObject clickLocater = new GameObject("Locater");
        clickLocater.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        clickLocater.transform.position = menuCamera.ScreenToWorldPoint(new Vector3(xPosition, yPosition, 0f));
        if ((clickLocater.transform.position.x >= -1*((int)boxWidthDecider*2.5)/2 && clickLocater.transform.position.x <= ((int)boxWidthDecider*2.5)/2) && (clickLocater.transform.position.y >= -1*((int)boxHeightDecider*2.5)/2 && clickLocater.transform.position.y <= ((int)boxHeightDecider*2.5)/2)) {
            int xIndex = (int)((((-2.5*boxWidthDecider)/2)-(clickLocater.transform.position.x))/-2.5);
            int yIndex = (int)((((2.5*boxHeightDecider)/2)-clickLocater.transform.position.y)/2.5);
            if (xIndex >= (int)boxWidthDecider) {
                xIndex--;
            }
            if (yIndex >= (int)boxHeightDecider) {
                yIndex--;
            }
            GameObject selectedBlock = GameObject.Find("Display Block (" + yIndex + ", " + xIndex + ") " + (displayBoardNum - 1));
            SpriteRenderer selectedBlockSprite = selectedBlock.GetComponent<SpriteRenderer>();
            if (selectedBlockSprite.color != Color.black) {
                if (placeDeadSpacesRandomly) {
                    if (deadSpacesPlaced < maxNumOfDeadSpaces) {
                        selectedBlockSprite.color = Color.black;
                        selectedBlock.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
                        deadSpacesObjects.Add(selectedBlock);
                        deadSpacesPlaced++;
                    }
                }
                else {
                    if (deadSpacesAreCalculated) {
                        if (deadSpacesPlaced < ((int)(Mathf.Sqrt((float)((int)boxWidthDecider*(int)boxHeightDecider))) - 3)) {
                            selectedBlockSprite.color = Color.black;
                            selectedBlock.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
                            deadSpacesObjects.Add(selectedBlock);
                            deadSpacesPlaced++;
                        }
                    }
                    else {
                        if (deadSpacesPlaced < ((int)boxWidthDecider*(int)boxHeightDecider)) {
                            selectedBlockSprite.color = Color.black;
                            selectedBlock.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
                            deadSpacesObjects.Add(selectedBlock);
                            deadSpacesPlaced++;
                        }
                    }
                }
            }
            else {
                if (placeDeadSpacesRandomly) {
                    deadCounter--;
                }
                else {
                    selectedBlockSprite.color = Color.white;
                    deadSpacesObjects.Remove(selectedBlock);
                    deadSpacesPlaced--;
                }
            }
        }
        Destroy(clickLocater);
    }

    private void cyclingToPlaceDeadSpacesRandomly() {
        for (deadCounter = 0; deadCounter < maxNumOfDeadSpaces; deadCounter++) {
            float chosenXPosition = Random.Range((float)(((Screen.width/2) * ((menuCamera.orthographicSize*2) - ((2.5*boxWidthDecider)/2)))/(menuCamera.orthographicSize*2)), (float)(((Screen.width/2) * ((menuCamera.orthographicSize*2) + ((2.5*boxWidthDecider)/2)))/(menuCamera.orthographicSize*2)));
            float chosenYPosition = Random.Range((float)(((Screen.height/2) * ((menuCamera.orthographicSize) - ((2.5*boxHeightDecider)/2)))/(menuCamera.orthographicSize)), (float)(((Screen.height/2) * ((menuCamera.orthographicSize) + ((2.5*boxHeightDecider)/2)))/(menuCamera.orthographicSize)));
            placingDeadSpacesOnSampleBoard(chosenXPosition, chosenYPosition);
        }
    }

    private void updateSampleBoardDimensions() {
        if (boxWidthDecider != previousWidthDecider || boxHeightDecider != previousHeightDecider) {
            deadSpacesObjects.Clear();
            GameObject[] boardTagObjects = GameObject.FindGameObjectsWithTag("Display Board");
            for (int i = 0; i < boardTagObjects.Length; i++) {
                Destroy(boardTagObjects[i]);
            }
            createSampleTicTacToeBoard();

            if (boxWidthDecider != previousWidthDecider) {
                widthSliderTouched = true;
                heightSliderTouched = false;
                inARowSliderTouched = false;
                previousWidthDecider = boxWidthDecider;
            }
            if (boxHeightDecider != previousHeightDecider) {
                widthSliderTouched = false;
                heightSliderTouched = true;
                inARowSliderTouched = false;
                previousHeightDecider = boxHeightDecider;
            }
        }
    }

    private void updateInARowToWin() {
        if (inARowToWinDecider != previousInARowToWinDecider) {
            widthSliderTouched = false;
            heightSliderTouched = false;
            inARowSliderTouched = true;
            previousInARowToWinDecider = inARowToWinDecider;
        }
    }

    private void updateMaxDimensions() {
        if (maxDimensionDecider != previousMaxDimensionDecider) {
            maxDimensionTouched = true;
            previousMaxDimensionDecider = maxDimensionDecider;
        }
    }

    private void settingUpWidthGUIForPVP() {
        GUI.SetNextControlName("widthSlider");
        boxWidthDecider = GUI.HorizontalSlider(new Rect(25f, 50f, 100f, 20f), boxWidthDecider, 3.0f, (float)((int)maxDimensionDecider));
        if (widthSliderTouched) {
            GUI.FocusControl("widthSlider");
            if ((int)boxWidthDecider >= 3 && (int)boxWidthDecider <= (int)maxDimensionDecider) {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                    if (buttonTimePassed == 0f) {
                        if ((int)boxWidthDecider > 3) {
                            boxWidthDecider--;
                        }
                        buttonTimePassed += 0.02f;
                    }
                }
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                    if (buttonTimePassed == 0f) {
                        if ((int)boxWidthDecider < (int)maxDimensionDecider) {
                            boxWidthDecider++;
                        }
                        buttonTimePassed += 0.02f;
                    }
                }
            }
        }
        if (GUI.Button(new Rect(25f, 70f, 100f, 50f), "Randomize")) {
            boxWidthDecider = (float)((int)Random.Range(3.0f, (float)((int)maxDimensionDecider) + 1f));
            deadSpacesObjects.Clear();
        }
        boxWidthString = GUI.TextField(new Rect(145f, 20f, 100f, 50f), (int)boxWidthDecider + "");
        widthDescriptor = GUI.TextField(new Rect(25f, 20f, 100f, 25f), "Width");
    }

    private void settingUpHeightGUIForPVP() {
        GUI.SetNextControlName("heightSlider");
        boxHeightDecider = GUI.HorizontalSlider(new Rect(25f, 200f, 100f, 20f), boxHeightDecider, 3.0f, (float)((int)maxDimensionDecider));
        if (heightSliderTouched) {
            GUI.FocusControl("heightSlider");
            if ((int)boxHeightDecider >= 3 && (int)boxHeightDecider <= (int)maxDimensionDecider) {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                    if (buttonTimePassed == 0f) {
                        if ((int)boxHeightDecider > 3) {
                            boxHeightDecider--;
                        }
                        buttonTimePassed += 0.02f;
                    }
                }
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                    if (buttonTimePassed == 0f) {
                        if ((int)boxHeightDecider < (int)maxDimensionDecider) {
                            boxHeightDecider++;
                        }
                        buttonTimePassed += 0.02f;
                    }
                }
            }
        }
        if (GUI.Button(new Rect(25f, 220f, 100f, 50f), "Randomize")) {
            boxHeightDecider = (float)((int)Random.Range(3.0f, (float)((int)maxDimensionDecider) + 1f));
            deadSpacesObjects.Clear();
        }
        boxHeightString = GUI.TextField(new Rect(145f, 170f, 100f, 50f), (int)boxHeightDecider + "");
        heightDescriptor = GUI.TextField(new Rect(25f, 170f, 100f, 25f), "Height");
    }

    private void settingUpInARowToWinGUIForPVP() {
        GUI.SetNextControlName("inARowSlider");
        if (boxWidthDecider >= boxHeightDecider) {
            inARowToWinDecider = GUI.HorizontalSlider(new Rect(25f, 350f, 100f, 20f), inARowToWinDecider, 3.0f, boxWidthDecider);
            if (inARowSliderTouched) {
                GUI.FocusControl("inARowSlider");
                if ((int)inARowToWinDecider >= 3 && (int)inARowToWinDecider <= (int)boxWidthDecider) {
                    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                        if (buttonTimePassed == 0f) {
                            if ((int)inARowToWinDecider > 3) {
                                inARowToWinDecider--;
                            }
                            buttonTimePassed += 0.02f;
                        }
                    }
                    if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                        if (buttonTimePassed == 0f) {
                            if ((int)inARowToWinDecider < (int)boxWidthDecider) {
                                inARowToWinDecider++;
                            }
                            buttonTimePassed += 0.02f;
                        }
                    }
                }
            }
            if (GUI.Button(new Rect(25f, 370f, 100f, 50f), "Randomize")) {
                inARowToWinDecider = (float)((int)Random.Range(3.0f, boxWidthDecider + 1f));
            }

            if (inARowToWinDecider > boxWidthDecider) {
                inARowToWinDecider = boxWidthDecider;
            }
        }
        else {
            inARowToWinDecider = GUI.HorizontalSlider(new Rect(25f, 350f, 100f, 20f), inARowToWinDecider, 3.0f, boxHeightDecider);
            if (inARowSliderTouched) {
                GUI.FocusControl("inARowSlider");
                if ((int)inARowToWinDecider >= 3 && (int)inARowToWinDecider <= (int)boxHeightDecider) {
                    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                        if (buttonTimePassed == 0f) {
                            if ((int)inARowToWinDecider > 3) {
                                inARowToWinDecider--;
                            }
                            buttonTimePassed += 0.02f;
                        }
                    }
                    if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                        if (buttonTimePassed == 0f) {
                            if ((int)inARowToWinDecider < (int)boxHeightDecider) {
                                inARowToWinDecider++;
                            }
                            buttonTimePassed += 0.02f;
                        }
                    }
                }
            }
            if (GUI.Button(new Rect(25f, 370f, 100f, 50f), "Randomize")) {
                inARowToWinDecider = (float)((int)Random.Range(3.0f, boxHeightDecider + 1f));
            }

            if (inARowToWinDecider > boxHeightDecider) {
                inARowToWinDecider = boxHeightDecider;
            }
        }
        inARowToWinString = GUI.TextField(new Rect(145f, 320f, 100f, 50f), (int)inARowToWinDecider + "");
        inARowDescriptor = GUI.TextField(new Rect(25f, 320f, 100f, 25f), "In a row to win");
    }

    private void settingUpRandomizeAllButtonForPVP() {
        if (GUI.Button(new Rect(25f, 440f, 220f, 100f), "Randomize All")) {
            boxWidthDecider = (float)((int)Random.Range(3.0f, (float)((int)maxDimensionDecider) + 1f));
            boxHeightDecider = (float)((int)Random.Range(3.0f, (float)((int)maxDimensionDecider) + 1f));
            if (boxWidthDecider >= boxHeightDecider) {
                inARowToWinDecider = (float)((int)Random.Range(3.0f, boxWidthDecider + 1f));
            }
            else {
                inARowToWinDecider = (float)((int)Random.Range(3.0f, boxHeightDecider + 1f));
            }

            if (deadSpacesAreCalculated) {
                maxNumOfDeadSpaces = (int)Random.Range(0f, (float)((int)(Mathf.Sqrt((float)((int)boxWidthDecider*(int)boxHeightDecider))) - 2));
            }
            else {
                maxNumOfDeadSpaces = (int)Random.Range(0f, (float)((int)boxWidthDecider*(int)boxHeightDecider) + 1f);
            }
            placeDeadSpacesRandomly = true;
        }
    }

    private void settingUpDeadSpaceGUIForPVP() {
        if (deadSpacesAreCalculated) {
            deadSpacesDisplayer = GUI.TextField(new Rect((float)Screen.width - 175f, 50f, 150f, 50f), (((int)(Mathf.Sqrt((float)((int)boxWidthDecider*(int)boxHeightDecider))) - 3) - deadSpacesPlaced) + "");
        }
        else {
            deadSpacesDisplayer = GUI.TextField(new Rect((float)Screen.width - 175f, 50f, 150f, 50f), (((int)boxWidthDecider*(int)boxHeightDecider) - deadSpacesPlaced) + "");
        }
        deadSpacesDescriptor = GUI.TextField(new Rect((float)Screen.width - 175f, 20f, 150f, 25f), "Dead spaces left");
        if (GUI.Button(new Rect((float)Screen.width - 175f, 175f, 150f, 50f), "Clear All")) {
            GameObject[] boardTagObjects = GameObject.FindGameObjectsWithTag("Display Board");
            for (int i = 0; i < boardTagObjects.Length; i++) {
                Destroy(boardTagObjects[i]);
            }
            deadSpacesObjects.Clear();
            createSampleTicTacToeBoard();
        }
        if (GUI.Button(new Rect((float)Screen.width - 175f, 115f, 150f, 50f), "Randomize")) {
            GameObject[] boardTagObjects = GameObject.FindGameObjectsWithTag("Display Board");
            for (int i = 0; i < boardTagObjects.Length; i++) {
                Destroy(boardTagObjects[i]);
            }
            deadSpacesObjects.Clear();
            createSampleTicTacToeBoard();

            if (deadSpacesAreCalculated) {
                maxNumOfDeadSpaces = (int)Random.Range(0f, (float)((int)(Mathf.Sqrt((float)((int)boxWidthDecider*(int)boxHeightDecider))) - 2));
            }
            else {
                maxNumOfDeadSpaces = (int)Random.Range(0f, (float)((int)boxWidthDecider*(int)boxHeightDecider) + 1f);
            }
            placeDeadSpacesRandomly = true;
        }
    }

    private void settingUpCaculatingDeadSpacesGUIForSettings() {
        deadSpacesAreCalculated = GUI.Toggle(new Rect(Screen.width - 287.5f, 25f, 20f, 20f), deadSpacesAreCalculated, "");
        calculatedDeadSpaceDescriptor = GUI.TextArea(new Rect(Screen.width - 317.5f, 50f, 80f, 50f), "Calculated Dead Spaces");
        if (deadSpacesAreCalculated) {
            GameObject.Find("Dead Space Changing GameObject").layer = 1;
        }
        else {
            GameObject.Find("Dead Space Changing GameObject").layer = 0;
        }
    }

    private void settingUpMaxDimensionGUIForSettings() {
        GUI.SetNextControlName("maxDimensionSlider");
        maxDimensionDecider = GUI.HorizontalSlider(new Rect(Screen.width - 125f, 80f, 100f, 20f), maxDimensionDecider, 3.0f, 100.0f);
        if (maxDimensionTouched) {
            GUI.FocusControl("maxDimensionSlider");
            if ((int)maxDimensionDecider >= 3 && (int)maxDimensionDecider <= 100) {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                    if (buttonTimePassed == 0f) {
                        if ((int)maxDimensionDecider > 3) {
                            maxDimensionDecider--;
                        }
                        buttonTimePassed += 0.02f;
                    }
                }
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                    if (buttonTimePassed == 0f) {
                        if ((int)maxDimensionDecider < 100) {
                            maxDimensionDecider++;
                        }
                        buttonTimePassed += 0.02f;
                    }
                }
            }
        }
        maxDimensionString = GUI.TextField(new Rect(Screen.width - 125f, 25f, 100f, 50f), (int)maxDimensionDecider + "");
        maxDimensionDescriptor = GUI.TextArea(new Rect(Screen.width - 230f, 25f, 100f, 75f), "The highest you want the dimensions of the grid to be");
        GameObject maxDimensionSizeGameObject = GameObject.Find("Dimension Changing GameObject");
        maxDimensionSizeGameObject.transform.position = new Vector3((float)((int)maxDimensionDecider), 0f, 0f);
    }

    private void settingUpMarkChangingGUIForSettings() {
        buttonSelected = GUI.Toolbar(new Rect(25, 25, 250, 50), buttonSelected, buttonNames);
        if (buttonSelected == 0) {
            if (!GameObject.Find("Xs and Os selected")) {
                marks = new GameObject("Xs and Os selected");
                marks.tag = "MarkChangingGameObject";
                Destroy(GameObject.Find("Greens and Reds selected"));
            }
            Color marksColor = new Color(0.2688679f, 0.4377447f, 1f, 1f);
            GUI.DrawTexture(new Rect(30, 85, 115, 115), Resources.Load<Texture>("xmark"), ScaleMode.ScaleToFit, true, 0f, marksColor, 0f, 0f);
            GUI.DrawTexture(new Rect(155, 85, 115, 115), Resources.Load<Texture>("omark"), ScaleMode.ScaleToFit, true, 0f, marksColor, 0f, 0f);
        }
        else if (buttonSelected == 1) {
            if (!GameObject.Find("Greens and Reds selected")) {
                marks = new GameObject("Greens and Reds selected");
                marks.tag = "MarkChangingGameObject";
                Destroy(GameObject.Find("Xs and Os selected"));
            }
            GUI.DrawTexture(new Rect(30, 85, 115, 115), Resources.Load<Texture>("greenmark"));
            GUI.DrawTexture(new Rect(155, 85, 115, 115), Resources.Load<Texture>("redmark"));
        }
    }

    private void showSpecificBlockCursorIsOver() {
        Vector3 location = menuCamera.ScreenToWorldPoint(Input.mousePosition);
        GameObject cover = GameObject.Find("Cover");
        if ((location.x >= -1*(boxWidthDecider*2.5)/2 && location.x <= (boxWidthDecider*2.5)/2) && (location.y >= -1*(boxHeightDecider*2.5)/2 && location.y <= (boxHeightDecider*2.5)/2)) {
            int squaresDown = (int)((location.y - ((boxHeightDecider*2.5)/2))/-2.5);
            int squaresRight = (int)((location.x + ((boxWidthDecider*2.5)/2))/2.5);
            if (squaresRight >= (int)boxWidthDecider) {
                squaresRight--;
            }
            if (squaresDown >= (int)boxHeightDecider) {
                squaresDown--;
            }
            GameObject selectedBlock = GameObject.Find("Display Block (" + squaresDown + ", " + squaresRight + ") " + (displayBoardNum - 1));
            SpriteRenderer blockSprite = selectedBlock.GetComponent<SpriteRenderer>();
            if (blockSprite.color != Color.black) {
                cover.transform.position = selectedBlock.transform.position;
            }
            else {
                cover.transform.position = new Vector3(1000f, 1000f, 0f);
            }
        }
        else {
            cover.transform.position = new Vector3(1000f, 1000f, 0f);
        }
    }

    public void addAndStoreNavigatingMenuCounter() {
        navigatingMenuCounter = navigatingMenuCounter + 1;
        GameObject.Find("Navigating Menu Counter").transform.position = new Vector3(navigatingMenuCounter, 0f, 0f);
    }

    public void setMenuLocation(string location) {
        menuLocation = location;
    }

    public void setTypeOfMarkButtonNumAndGameObject(GameObject obj) {
        marks = obj;
        if (obj.name == "Xs and Os selected") {
            buttonSelected = 0;
        }
        else if (obj.name == "Greens and Reds selected") {
            buttonSelected = 1;
        } 
    }

    public void setCamera(Camera camera) {
        menuCamera = camera;
    }

    public GameObject getMarks() {
        return marks;
    }

    public GameObject getMaxDimensions() {
        return GameObject.Find("Dimension Changing GameObject");
    }

    public GameObject getCalculatedDeadSpaces() {
        return GameObject.Find("Dead Space Changing GameObject"); 
    }

    public void SaveMenuData() {
        SaveSystemForMenu.SaveDataFromMainMenu(this);
    }

    public void LoadMenuData() {
        RetrievedMenuData data = SaveSystemForMenu.LoadDataFromMainMenu();

        marks.name = data.markName;
        GameObject.Find("Dimension Changing GameObject").transform.position = new Vector3(data.maxDimesnsions, 0f, 0f);
        GameObject.Find("Dead Space Changing GameObject").layer = data.areDeadSpacesCalculated;
    }

}

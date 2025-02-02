using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerVSPlayerGameScript : MonoBehaviour
{
    public int inARowToWin;
    public Camera mainCamera;
    public CinemachineVirtualCamera otherCamera;
    public GUIStyle styleForTurnText;
    public GUIStyle styleForWinText;
    public GUIStyle styleForTieText;
    public GUIStyle styleForNewBoardButton;
    public GUIStyle styleForRematchButton;
    public GUIStyle styleForMainMenuButton;
    public GUIStyle styleForConnectingText;
    
    int boxWidth;
    int boxHeight;
    int numOfDeadSpaces;
    int squaresDown;
    int squaresRight;
    int turnDecider;
    int connectors = 0;
    int boardNum = 0;
    int winningOrientation;
    int xWinningConnections;
    int oWinningConnections;
    float mainMenuButtonHeight;
    float sideButtonsWidth;
    float timePassedSinceFirstUpdate;
    bool winChecker;
    bool tieChecker;
    bool isRematching;
    bool firstTurn = true;
    bool markPlaced;
    bool zoomedIn;
    string turnWinAndTieString;
    string connectString;
    string turnString1;
    string turnString2;
    Sprite mark1;
    Sprite mark2;
    GameObject mainMenuObject;

    List<string> deadSpacesCoords = new List<string>();
    List<GameObject> xPlacements = new List<GameObject>();
    List<GameObject> oPlacements = new List<GameObject>();
    List<GameObject> winningConnectors = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        checkForMarkChangingGameObjects();
        createTicTacToeBoard();
        mainMenuObject = GameObject.Find("in pvp arena");
        if (!isRematching) {
            turnDecider = (int)Random.Range(0f, 2f);
        }
        tieChecker = checkForTie();
    }

    // Update is called once per frame 
    void Update()
    {
        if (timePassedSinceFirstUpdate > 0.125f) {
            showSpecificBlockCursorIsOver();

            if (!tieChecker) {
                if (!winChecker) {
                    if (Input.GetMouseButtonUp(0)) {
                        placingMarksAndUpdatingBoard(Input.mousePosition.x, Input.mousePosition.y);
                    }
                    else if (Input.GetMouseButtonUp(1)) {
                        zoomedIn = zoomInOrOut(zoomedIn);
                    }
                }
                else {
                    if (zoomedIn) {
                        zoomInOrOut(true);
                    }
                }
            }
            else {
                if (zoomedIn) {
                    zoomInOrOut(true);
                }
            }
        }
        timePassedSinceFirstUpdate += Time.deltaTime;
    }

    void OnGUI() {
        if (!tieChecker) {
            if (winChecker) {
                if ((turnDecider + 1) % 2 == 0) {
                    turnWinAndTieString = GUI.TextField(new Rect((float)((Screen.width/2)-(((Screen.width/2)*(2.5*boxWidth)/2)/(otherCamera.m_Lens.OrthographicSize*2))), 0f, (float)((Screen.width/2)*2.5*boxWidth)/(otherCamera.m_Lens.OrthographicSize*2), (float)((Screen.height/2)-(((Screen.height/2)*2.5*boxHeight)/2)/otherCamera.m_Lens.OrthographicSize)), turnString1 + " wins!", styleForWinText);
                }
                else {
                    turnWinAndTieString = GUI.TextField(new Rect((float)((Screen.width/2)-(((Screen.width/2)*(2.5*boxWidth)/2)/(otherCamera.m_Lens.OrthographicSize*2))), 0f, (float)((Screen.width/2)*2.5*boxWidth)/(otherCamera.m_Lens.OrthographicSize*2), (float)((Screen.height/2)-(((Screen.height/2)*2.5*boxHeight)/2)/otherCamera.m_Lens.OrthographicSize)), turnString2 + " wins!", styleForWinText);
                }

                if (GUI.Button(new Rect(((float)(Screen.width/2) - (float)(((Screen.width/2)*((2.5*boxWidth)/2))/(otherCamera.m_Lens.OrthographicSize*2)) - sideButtonsWidth)/2f, (float)((Screen.height/2)-(sideButtonsWidth/4f)), sideButtonsWidth, sideButtonsWidth/2f), "New Board", styleForNewBoardButton)) {
                    GameObject[] boardComponents = GameObject.FindGameObjectsWithTag("Board");
                    for (int boardItem = 0; boardItem < boardComponents.Length; boardItem++) {
                        Destroy(boardComponents[boardItem]);
                    }
                    GameObject newBoard = new GameObject("Player VS Player Game script " + (boardNum + 1), typeof(PlayerVSPlayerGameScript));
                    PlayerVSPlayerGameScript newBoardScript = newBoard.GetComponent<PlayerVSPlayerGameScript>();
                    newBoardScript.setBoardNum(boardNum + 1);
                    newBoardScript.setCameras(mainCamera, otherCamera);
                    newBoardScript.setStyles(styleForTurnText, styleForWinText, styleForTieText, styleForNewBoardButton, styleForRematchButton, styleForMainMenuButton, styleForConnectingText);
                    newBoardScript.setIsRematch(false);
                    Destroy(GameObject.Find("Player VS Player Game script " + boardNum));
                }
                else if (GUI.Button(new Rect((((float)(Screen.width/2) - (float)(((Screen.width/2)*((2.5*boxWidth)/2))/(otherCamera.m_Lens.OrthographicSize*2)) - sideButtonsWidth)/2f) + ((float)(Screen.width/2) + (float)(((Screen.width/2)*((2.5*boxWidth)/2))/(otherCamera.m_Lens.OrthographicSize*2))), (float)((Screen.height/2) - (sideButtonsWidth/4f)), sideButtonsWidth, sideButtonsWidth/2f), "Rematch", styleForRematchButton)) {
                    GameObject[] boardComponents = GameObject.FindGameObjectsWithTag("Board");
                    for (int boardItem = 0; boardItem < boardComponents.Length; boardItem++) {
                        Destroy(boardComponents[boardItem]);
                    }
                    GameObject newBoard = new GameObject("Player VS Player Game script " + (boardNum + 1), typeof(PlayerVSPlayerGameScript));
                    PlayerVSPlayerGameScript newBoardScript = newBoard.GetComponent<PlayerVSPlayerGameScript>();
                    newBoardScript.setBoardNum(boardNum + 1);
                    newBoardScript.setCameras(mainCamera, otherCamera);
                    newBoardScript.setStyles(styleForTurnText, styleForWinText, styleForTieText, styleForNewBoardButton, styleForRematchButton, styleForMainMenuButton, styleForConnectingText);
                    newBoardScript.setIsRematch(true);
                    newBoardScript.carryPreviousInfoForRematch(boxWidth, boxHeight, numOfDeadSpaces, deadSpacesCoords, turnDecider, inARowToWin);
                    Destroy(GameObject.Find("Player VS Player Game script " + boardNum));
                }
                else if (GUI.Button(new Rect((float)(Screen.width/2) - mainMenuButtonHeight, (float)(((Screen.height/2) + ((((Screen.height/2)*(2.5*boxHeight))/2)/(otherCamera.m_Lens.OrthographicSize))) + ((Screen.height/2) - ((((Screen.height/2)*(2.5*boxHeight))/2)/(otherCamera.m_Lens.OrthographicSize)))/2f) - (mainMenuButtonHeight/2f), mainMenuButtonHeight * 2f, mainMenuButtonHeight), "Main Menu", styleForMainMenuButton)) {
                    GameObject[] boardComponents = GameObject.FindGameObjectsWithTag("Board");
                    for (int boardItem = 0; boardItem < boardComponents.Length; boardItem++) {
                        Destroy(boardComponents[boardItem]);
                    }
                    Destroy(mainMenuObject);
                    SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
                }
            }
            else {
                if (turnDecider % 2 == 0) {
                    if (firstTurn) {
                        turnWinAndTieString = GUI.TextField(new Rect((float)((Screen.width/2)-(((Screen.width/2)*(2.5*boxWidth)/2)/(otherCamera.m_Lens.OrthographicSize*2))), 0f, (float)((Screen.width/2)*2.5*boxWidth)/(otherCamera.m_Lens.OrthographicSize*2), (float)((Screen.height/2)-(((Screen.height/2)*2.5*boxHeight)/2)/otherCamera.m_Lens.OrthographicSize)), turnString1 + " goes first!", styleForTurnText);
                        if (markPlaced) {
                            firstTurn = false;
                        }
                    }
                    else {
                       turnWinAndTieString = GUI.TextField(new Rect((float)((Screen.width/2)-(((Screen.width/2)*(2.5*boxWidth)/2)/(otherCamera.m_Lens.OrthographicSize*2))), 0f, (float)((Screen.width/2)*2.5*boxWidth)/(otherCamera.m_Lens.OrthographicSize*2), (float)((Screen.height/2)-(((Screen.height/2)*2.5*boxHeight)/2)/otherCamera.m_Lens.OrthographicSize)), turnString1 + "'s turn", styleForTurnText); 
                    }
                    
                }
                else {
                    if (firstTurn) {
                        turnWinAndTieString = GUI.TextField(new Rect((float)((Screen.width/2)-(((Screen.width/2)*(2.5*boxWidth)/2)/(otherCamera.m_Lens.OrthographicSize*2))), 0f, (float)((Screen.width/2)*2.5*boxWidth)/(otherCamera.m_Lens.OrthographicSize*2), (float)((Screen.height/2)-(((Screen.height/2)*2.5*boxHeight)/2)/otherCamera.m_Lens.OrthographicSize)), turnString2 + " goes first!", styleForTurnText);
                        if (markPlaced) {
                            firstTurn = false;
                        }
                    }
                    else {
                        turnWinAndTieString = GUI.TextField(new Rect((float)((Screen.width/2)-(((Screen.width/2)*(2.5*boxWidth)/2)/(otherCamera.m_Lens.OrthographicSize*2))), 0f, (float)((Screen.width/2)*2.5*boxWidth)/(otherCamera.m_Lens.OrthographicSize*2), (float)((Screen.height/2)-(((Screen.height/2)*2.5*boxHeight)/2)/otherCamera.m_Lens.OrthographicSize)), turnString2 + "'s turn", styleForTurnText);
                    }
                    
                }

                connectString = GUI.TextField(new Rect(150f, 0f, 200f, 50f), "Get " + inARowToWin + " or more in a row to win!", styleForConnectingText);

                if (GUI.Button(new Rect(10f, 10f, 100f, 50f), "Main Menu")) {
                    GameObject[] boardComponents = GameObject.FindGameObjectsWithTag("Board");
                    for (int boardItem = 0; boardItem < boardComponents.Length; boardItem++) {
                        Destroy(boardComponents[boardItem]);
                    }
                    Destroy(mainMenuObject);
                    SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
                }
            }
        }
        else {
            turnWinAndTieString = GUI.TextField(new Rect((float)((Screen.width/2)-(((Screen.width/2)*(2.5*boxWidth)/2)/(otherCamera.m_Lens.OrthographicSize*2))), 0f, (float)((Screen.width/2)*2.5*boxWidth)/(otherCamera.m_Lens.OrthographicSize*2), (float)((Screen.height/2)-(((Screen.height/2)*2.5*boxHeight)/2)/otherCamera.m_Lens.OrthographicSize)), "Tie Game!", styleForTieText);

            if (GUI.Button(new Rect(((float)(Screen.width/2) - (float)(((Screen.width/2)*((2.5*boxWidth)/2))/(otherCamera.m_Lens.OrthographicSize*2)) - sideButtonsWidth)/2f, (float)((Screen.height/2)-(sideButtonsWidth/4f)), sideButtonsWidth, sideButtonsWidth/2f), "New Board", styleForNewBoardButton)) {
                GameObject[] boardComponents = GameObject.FindGameObjectsWithTag("Board");
                for (int boardItem = 0; boardItem < boardComponents.Length; boardItem++) {
                    Destroy(boardComponents[boardItem]);
                }
                GameObject newBoard = new GameObject("Player VS Player Game script " + (boardNum + 1), typeof(PlayerVSPlayerGameScript));
                PlayerVSPlayerGameScript newBoardScript = newBoard.GetComponent<PlayerVSPlayerGameScript>();
                newBoardScript.setBoardNum(boardNum + 1);
                newBoardScript.setCameras(mainCamera, otherCamera);
                newBoardScript.setStyles(styleForTurnText, styleForWinText, styleForTieText, styleForNewBoardButton, styleForRematchButton, styleForMainMenuButton, styleForConnectingText);
                newBoardScript.setIsRematch(false);
                Destroy(GameObject.Find("Player VS Player Game script " + boardNum));
            }
            else if (GUI.Button(new Rect((((float)(Screen.width/2) - (float)(((Screen.width/2)*((2.5*boxWidth)/2))/(otherCamera.m_Lens.OrthographicSize*2)) - sideButtonsWidth)/2f) + ((float)(Screen.width/2) + (float)(((Screen.width/2)*((2.5*boxWidth)/2))/(otherCamera.m_Lens.OrthographicSize*2))), (float)((Screen.height/2) - (sideButtonsWidth/4f)), sideButtonsWidth, sideButtonsWidth/2f), "Rematch", styleForRematchButton)) {
                GameObject[] boardComponents = GameObject.FindGameObjectsWithTag("Board");
                for (int boardItem = 0; boardItem < boardComponents.Length; boardItem++) {
                    Destroy(boardComponents[boardItem]);
                }
                GameObject newBoard = new GameObject("Player VS Player Game script " + (boardNum + 1), typeof(PlayerVSPlayerGameScript));
                PlayerVSPlayerGameScript newBoardScript = newBoard.GetComponent<PlayerVSPlayerGameScript>();
                newBoardScript.setBoardNum(boardNum + 1);
                newBoardScript.setCameras(mainCamera, otherCamera);
                newBoardScript.setStyles(styleForTurnText, styleForWinText, styleForTieText, styleForNewBoardButton, styleForRematchButton, styleForMainMenuButton, styleForConnectingText);
                newBoardScript.setIsRematch(true);
                newBoardScript.carryPreviousInfoForRematch(boxWidth, boxHeight, numOfDeadSpaces, deadSpacesCoords, turnDecider, inARowToWin);
                Destroy(GameObject.Find("Player VS Player Game script " + boardNum));
            }
            else if (GUI.Button(new Rect((float)(Screen.width/2) - mainMenuButtonHeight, (float)(((Screen.height/2) + ((((Screen.height/2)*(2.5*boxHeight))/2)/(otherCamera.m_Lens.OrthographicSize))) + ((Screen.height/2) - ((((Screen.height/2)*(2.5*boxHeight))/2)/(otherCamera.m_Lens.OrthographicSize)))/2f) - (mainMenuButtonHeight/2f), mainMenuButtonHeight * 2f, mainMenuButtonHeight), "Main Menu", styleForMainMenuButton)) {
                GameObject[] boardComponents = GameObject.FindGameObjectsWithTag("Board");
                for (int boardItem = 0; boardItem < boardComponents.Length; boardItem++) {
                    Destroy(boardComponents[boardItem]);
                }
                Destroy(mainMenuObject);
                SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
            }
        }
    }

    private void createTicTacToeBoard() {
        if (!isRematching) { 
            setWidthAndHeightOfNewBoard();
            setMarksInARowToWinNewBoard();
            setNumOfDeadSpaces();
        }

        int deadSpacesTracker = 0;
        for (int rows = 0; rows < boxHeight; rows++) {
            for (int columns = 0; columns < boxWidth; columns++) {
                GameObject block = new GameObject("Block (" + rows + ", " + columns + ")", typeof(SpriteRenderer));
                block.transform.position = new Vector3((float)((-1*(boxWidth*2.5)/2) + (2.5/2) + 2.5*(columns)), (float)(((boxHeight*2.5)/2) - (2.5/2) - 2.5*(rows)), 0f);
                block.transform.localScale = new Vector3(2.4f, 2.4f, 1.0f);
                block.tag = "Board";
                if (deadSpacesCoords.Count > 0) {
                    if (deadSpacesTracker < numOfDeadSpaces) {
                        if (deadSpacesCoords.Contains(block.name)) {
                            SpriteRenderer blockSp = block.GetComponent<SpriteRenderer>();
                            Sprite blockSprite = Resources.Load<Sprite>("Square");
                            blockSp.color = Color.black;
                            blockSp.sprite = blockSprite;
                            blockSp.sortingOrder = 1;
                            block.transform.localScale = new Vector3(2.5f, 2.5f, 1.0f);
                            deadSpacesTracker++; 
                        }
                    }
                }
            }
        }

        GameObject box = new GameObject("Game Box", typeof(SpriteRenderer));
        SpriteRenderer boxSp = box.GetComponent<SpriteRenderer>();
        Sprite boxSprite = Resources.Load<Sprite>("Square");
        box.transform.localScale = new Vector3((float)(2.5*boxWidth), (float)(2.5*boxHeight), 1.0f);
        box.tag = "Board";
        boxSp.color = Color.white;
        boxSp.sprite = boxSprite;

        for (int verticalLineNumber = 1; verticalLineNumber < boxWidth; verticalLineNumber++) {
            GameObject verticalLine = new GameObject("Vertical Line " + verticalLineNumber, typeof(SpriteRenderer));
            SpriteRenderer gridLineSp = verticalLine.GetComponent<SpriteRenderer>();
            Sprite theSprite = Resources.Load<Sprite>("Square");
            verticalLine.transform.position = new Vector3((float)((-1*(boxWidth*2.5)/2) + 2.5*(verticalLineNumber)), 0f, 0f);
            verticalLine.transform.localScale = new Vector3(0.1875f, (float)(2.5*boxHeight), 1.0f);
            verticalLine.tag = "Board";
            gridLineSp.color = Color.black;
            gridLineSp.sprite = theSprite;
            gridLineSp.sortingOrder = 2;
        }
        for (int horizontalLineNumber = 1; horizontalLineNumber < boxHeight; horizontalLineNumber++) {
            GameObject horizontalLine = new GameObject("Horizontal Line " + horizontalLineNumber, typeof(SpriteRenderer));
            SpriteRenderer gridLineSp = horizontalLine.GetComponent<SpriteRenderer>();
            Sprite theSprite = Resources.Load<Sprite>("Square");
            horizontalLine.transform.Rotate(0f, 0f, 90f);
            horizontalLine.transform.position = new Vector3(0f, (float)((-1*(boxHeight*2.5)/2) + 2.5*(horizontalLineNumber)), 0f);
            horizontalLine.transform.localScale = new Vector3(0.1875f, (float)(2.5*boxWidth), 1.0f);
            horizontalLine.tag = "Board";
            gridLineSp.color = Color.black;
            gridLineSp.sprite = theSprite;
            gridLineSp.sortingOrder = 2;
        }

        if (boxHeight >= boxWidth) {
            otherCamera.m_Lens.OrthographicSize = (float)(((boxHeight*2.5)/2) + 3.25);
        }
        else {
            otherCamera.m_Lens.OrthographicSize = (float)(((boxWidth*2.5)/2) + 1.625);
        }

        setMainMenuButtonHeight();
        setSideButtonsWidth();

    }

    public void placingMarksAndUpdatingBoard(float xPos, float yPos) {
        GameObject clickLocater = new GameObject("Locater");
        clickLocater.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        clickLocater.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(xPos, yPos, 0f));
        if ((clickLocater.transform.position.x >= -1*(boxWidth*2.5)/2 && clickLocater.transform.position.x <= (boxWidth*2.5)/2) && (clickLocater.transform.position.y >= -1*(boxHeight*2.5)/2 && clickLocater.transform.position.y <= (boxHeight*2.5)/2)) {
            squaresDown = (int)((clickLocater.transform.position.y - ((boxHeight*2.5)/2))/-2.5);
            squaresRight = (int)((clickLocater.transform.position.x + ((boxWidth*2.5)/2))/2.5);
            GameObject selectedBlock = GameObject.Find("Block (" + squaresDown + ", " + squaresRight + ")");
            SpriteRenderer blockSprite = selectedBlock.GetComponent<SpriteRenderer>();
            Sprite markSprite;
            if (blockSprite.color != Color.black && (blockSprite.sprite != mark1 && blockSprite.sprite != mark2)) {
                markPlaced = true;
                if (turnDecider % 2 == 0) {
                    markSprite = mark1;
                    xPlacements.Add(selectedBlock);
                }
                else {
                    markSprite = mark2;
                    oPlacements.Add(selectedBlock);
                }
                blockSprite.sprite = markSprite;
                selectedBlock.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
                blockSprite.sortingOrder = 1;
                cycleOptionsAroundMark(selectedBlock);
                tieChecker = checkForTie();
                turnDecider++;
            }
        }
        Destroy(clickLocater);
    }

    private void cycleOptionsAroundMark(GameObject theBlock) {
        for (int counter = 0; counter < 4; counter++) {
            int heightDifference = -1;
            int widthDifferece = -1 + counter;
            if (counter == 3) {
                heightDifference = 0;
                widthDifferece = -1;
            }
            winChecker = checkForWin(theBlock, heightDifference, widthDifferece);
            connectors = 0;
            if (winChecker) {
                if (turnDecider % 2 == 0) {
                    xWinningConnections++;
                }
                else {
                    oWinningConnections++;
                }
                winningOrientation = counter;
                placeWinningLine();
            }
            else {
                if (xWinningConnections >= 1 || oWinningConnections >= 1) {
                   winChecker = true; 
                }
            }
            winningConnectors.Clear();
        }
    }

    private bool checkForWin(GameObject coords, int heightDiff, int widthDiff) {
        int multiplier = 1;
        bool keepGoing1 = true;
        bool keepGoing2 = true;
        winningConnectors.Add(coords);
        if (turnDecider % 2 == 0) {
            while (keepGoing1) {
                if (xPlacements.Contains(GameObject.Find("Block (" + (squaresDown + (heightDiff * multiplier)) + ", " + (squaresRight + (widthDiff * multiplier)) + ")"))) {
                    winningConnectors.Add(GameObject.Find("Block (" + (squaresDown + (heightDiff * multiplier)) + ", " + (squaresRight + (widthDiff * multiplier)) + ")"));
                    connectors++;
                    multiplier++;
                }
                else {
                    keepGoing1 = false;
                    multiplier = -1;
                }
            }

            while (keepGoing2) {
                if (xPlacements.Contains(GameObject.Find("Block (" + (squaresDown + (heightDiff * multiplier)) + ", " + (squaresRight + (widthDiff * multiplier)) + ")"))) {
                    winningConnectors.Add(GameObject.Find("Block (" + (squaresDown + (heightDiff * multiplier)) + ", " + (squaresRight + (widthDiff * multiplier)) + ")"));
                    connectors++;
                    multiplier--;
                }
                else {
                    keepGoing2 = false;
                    multiplier = 1;
                }
            }
        }
        else {
            while (keepGoing1) {
                if (oPlacements.Contains(GameObject.Find("Block (" + (squaresDown + (heightDiff * multiplier)) + ", " + (squaresRight + (widthDiff * multiplier)) + ")"))) {
                    winningConnectors.Add(GameObject.Find("Block (" + (squaresDown + (heightDiff * multiplier)) + ", " + (squaresRight + (widthDiff * multiplier)) + ")"));
                    connectors++;
                    multiplier++;
                }
                else {
                    keepGoing1 = false;
                    multiplier = -1;
                }
            }

            while (keepGoing2) {
                if (oPlacements.Contains(GameObject.Find("Block (" + (squaresDown + (heightDiff * multiplier)) + ", " + (squaresRight + (widthDiff * multiplier)) + ")"))) {
                    winningConnectors.Add(GameObject.Find("Block (" + (squaresDown + (heightDiff * multiplier)) + ", " + (squaresRight + (widthDiff * multiplier)) + ")"));
                    connectors++;
                    multiplier--;
                }
                else {
                    keepGoing2 = false;
                    multiplier = 1;
                }
            }
        }
        return (connectors + 1) >= inARowToWin; 
    }

    private bool checkForTie() {
        if (!winChecker) { 
            return (xPlacements.Count + oPlacements.Count == (boxWidth * boxHeight) - numOfDeadSpaces);
        }
        else {
            return false;
        }
    }

    private void checkForMarkChangingGameObjects() {
        if (GameObject.Find("Xs and Os selected")) {
            mark1 = Resources.Load<Sprite>("xmark");
            mark2 = Resources.Load<Sprite>("omark");
            turnString1 = "X";
            turnString2 = "O";
        }
        else if (GameObject.Find("Greens and Reds selected")) {
            mark1 = Resources.Load<Sprite>("greenmark");
            mark2 = Resources.Load<Sprite>("redmark");
            turnString1 = "Green";
            turnString2 = "Red";
        }
    }

    private void placeWinningLine() {
        GameObject markWinningLine = new GameObject("Winning Line", typeof(SpriteRenderer));
        markWinningLine.transform.position = new Vector3(calculateWinningXAverage(), calculateWinningYAverage(), 0f);
        if (Mathf.Abs(winningOrientation) == 1 || Mathf.Abs(winningOrientation) == 3) {
            markWinningLine.transform.localScale = new Vector3(0.1875f, (float)(2.5*winningConnectors.Count), 1.0f);
            if (Mathf.Abs(winningOrientation) == 3) {
                markWinningLine.transform.Rotate(0f, 0f, 90f);
            }
        }
        else if (Mathf.Abs(winningOrientation) == 0 || Mathf.Abs(winningOrientation) == 2) {
            markWinningLine.transform.localScale = new Vector3(0.1875f, Mathf.Sqrt(Mathf.Pow((float)(2.5*winningConnectors.Count), 2f) * 2), 1.0f);
            if (Mathf.Abs(winningOrientation) == 0) {
                markWinningLine.transform.Rotate(0f, 0f, 45f);
            }
            else if (Mathf.Abs(winningOrientation) == 2) {
                markWinningLine.transform.Rotate(0f, 0f, -45f);
            }
        }
        SpriteRenderer sprRendOfLine = markWinningLine.GetComponent<SpriteRenderer>();
        sprRendOfLine.sprite = Resources.Load<Sprite>("Square");
        sprRendOfLine.color = new Color(0.0f, 0.5f, 1.0f);
        sprRendOfLine.sortingOrder = 10;
        markWinningLine.tag = "Board";
    }
    
    private float calculateWinningXAverage() {
        float xVariable = 0;
        for (int winningCoord = 0; winningCoord < winningConnectors.Count; winningCoord++) {
            xVariable += winningConnectors[winningCoord].transform.position.x;
        }
        return (xVariable/(float)(winningConnectors.Count));
    }

    private float calculateWinningYAverage() {
        float yVariable = 0;
        for (int winningCoord = 0; winningCoord < winningConnectors.Count; winningCoord++) {
            yVariable += winningConnectors[winningCoord].transform.position.y;
        }
        return (yVariable/(float)(winningConnectors.Count));
    }

    private void setMarksInARowToWinNewBoard() {
        if (boardNum != 0) {
            if (boxWidth >= boxHeight) {
                inARowToWin = (int)Random.Range(3.0f, (float)(boxWidth + 1));
            }
            else {
                inARowToWin = (int)Random.Range(3.0f, (float)(boxHeight + 1));
            }
        }
        else {
            inARowToWin = (int)GameObject.Find("inARowToWinDecider").transform.position.x;
        }
    }

    private void setWidthAndHeightOfNewBoard() {
        if (boardNum == 0) {
            boxWidth = (int)GameObject.Find("boxWidthDecider").transform.position.x;
            boxHeight = (int)GameObject.Find("boxHeightDecider").transform.position.x;
        }
        else {
            GameObject maxDimensionSizeGameObject = GameObject.Find("Dimension Changing GameObject");
            boxWidth = (int)Random.Range(3.0f, maxDimensionSizeGameObject.transform.position.x);
            boxHeight = (int)Random.Range(3.0f, maxDimensionSizeGameObject.transform.position.x);
        }
    }

    private void setMainMenuButtonHeight() {
        if ((float)((Screen.height - ((Screen.height*(2.5*boxHeight))/2)/otherCamera.m_Lens.OrthographicSize) / 2) - 20f < 150f) {
            mainMenuButtonHeight = (float)((Screen.height - ((Screen.height*(2.5*boxHeight))/2)/otherCamera.m_Lens.OrthographicSize)/2) - 20f;
        }
        else {
            mainMenuButtonHeight = 150f;
        }
        styleForMainMenuButton.fontSize = (int)(mainMenuButtonHeight/3);
    }

    private void setSideButtonsWidth() {
        if ((float)((((otherCamera.m_Lens.OrthographicSize*2) - ((2.5*boxWidth)/2))*(Screen.width/2))/(otherCamera.m_Lens.OrthographicSize*2)) < 300f) {
            sideButtonsWidth = (float)((((otherCamera.m_Lens.OrthographicSize*2) - ((2.5*boxWidth)/2))*(Screen.width/2))/(otherCamera.m_Lens.OrthographicSize*2));
        }
        else {
            sideButtonsWidth = 300f;
        }
        styleForNewBoardButton.fontSize = (int)(sideButtonsWidth/6);
        styleForRematchButton.fontSize = (int)(sideButtonsWidth/6);

    }

    private void setNumOfDeadSpaces() {
        if (boardNum == 0) {
            numOfDeadSpaces = (int)GameObject.Find("deadSpacesDisplayer").transform.position.x;
            GameObject[] displayDeadSpaces = GameObject.FindGameObjectsWithTag("Display Board");
            for (int i = 0; i < numOfDeadSpaces; i++) {
                int xIndex = (int)(((((-2.5*boxWidth)+2.5)/2)-(displayDeadSpaces[i].transform.position.x))/-2.5);
                int yIndex = (int)(((((2.5*boxHeight)-2.5)/2)-(displayDeadSpaces[i].transform.position.y))/2.5);
                deadSpacesCoords.Add("Block (" + yIndex + ", " + xIndex + ")");
                Destroy(displayDeadSpaces[i]);
            }
        }
        else {
            GameObject deadSpaceChangingObject = GameObject.Find("Dead Space Changing GameObject");
            if (deadSpaceChangingObject.layer == 1) {
                numOfDeadSpaces = (int)Random.Range(0f, (float)((int)(Mathf.Sqrt((float)(boxWidth*boxHeight))) - 2));
            }
            else {
                numOfDeadSpaces = (int)Random.Range(0f, (float)(boxWidth*boxHeight) + 1f);
            }
            int deadCounter = 0;
            while (deadCounter < numOfDeadSpaces) {
                int randomXCoord = (int)(Random.Range(0f, (float)boxWidth));
                int randomYCoord = (int)(Random.Range(0f, (float)boxHeight));
                string sampleText = "Block (" + randomYCoord + ", " + randomXCoord + ")";
                if (!(deadSpacesCoords.Contains(sampleText))) {
                    deadSpacesCoords.Add(sampleText);
                    deadCounter++;
                }
            }
        }
    }

    private void showSpecificBlockCursorIsOver() {
        Vector3 location = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        GameObject cover = GameObject.Find("Cover");
        if (!winChecker && !tieChecker) {
            if ((location.x >= -1*(boxWidth*2.5)/2 && location.x <= (boxWidth*2.5)/2) && (location.y >= -1*(boxHeight*2.5)/2 && location.y <= (boxHeight*2.5)/2)) {
                squaresDown = (int)((location.y - ((boxHeight*2.5)/2))/-2.5);
                squaresRight = (int)((location.x + ((boxWidth*2.5)/2))/2.5);
                GameObject selectedBlock = GameObject.Find("Block (" + squaresDown + ", " + squaresRight + ")");
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
        else {
            cover.transform.position = new Vector3(1000f, 1000f, 0f);
        }
    }

    private bool zoomInOrOut(bool zoomOption) {
        Vector3 location = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (zoomOption) {
            GameObject center = GameObject.Find("center of the board");
            otherCamera.m_Follow = center.transform;
            if (boxHeight >= boxWidth) {
                otherCamera.m_Lens.OrthographicSize = (float)(((boxHeight*2.5)/2) + 3.25);
            }
            else {
                otherCamera.m_Lens.OrthographicSize = (float)(((boxWidth*2.5)/2) + 1.625);
            }
            return false;
        }
        else {
            if ((location.x >= -1*(boxWidth*2.5)/2 && location.x <= (boxWidth*2.5)/2) && (location.y >= -1*(boxHeight*2.5)/2 && location.y <= (boxHeight*2.5)/2)) {
                squaresDown = (int)((location.y - ((boxHeight*2.5)/2))/-2.5);
                squaresRight = (int)((location.x + ((boxWidth*2.5)/2))/2.5);
                GameObject selectedBlock = GameObject.Find("Block (" + squaresDown + ", " + squaresRight + ")");
                otherCamera.m_Follow = selectedBlock.transform;
                otherCamera.m_Lens.OrthographicSize /= 4;
                return true;
            }
        }
        return zoomOption;    
    }

    public void setBoardNum(int newBoardNum) {
        boardNum = newBoardNum;
    }

    public void setCameras(Camera newCamera1, CinemachineVirtualCamera newCamera2) {
        mainCamera = newCamera1;
        otherCamera = newCamera2;
    }

    public void setStyles(GUIStyle style1, GUIStyle style2, GUIStyle style3, GUIStyle style4, GUIStyle style5, GUIStyle style6, GUIStyle style7) {
        styleForTurnText = style1;
        styleForWinText = style2;
        styleForTieText = style3;
        styleForNewBoardButton = style4;
        styleForRematchButton = style5;
        styleForMainMenuButton = style6;
        styleForConnectingText = style7;
    }

    public void setIsRematch(bool rematchChecker) {
        isRematching = rematchChecker;
    }

    public void carryPreviousInfoForRematch(int width, int height, int numOfDead, List<string> deadCoords, int turn, int inARow) {
        boxWidth = width;
        boxHeight = height;
        numOfDeadSpaces = numOfDead;
        deadSpacesCoords = deadCoords;
        turnDecider = turn + 1;
        inARowToWin = inARow;
    }

}
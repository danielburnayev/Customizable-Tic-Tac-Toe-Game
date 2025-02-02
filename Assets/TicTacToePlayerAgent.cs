using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Cinemachine;

public class TicTacToePlayerAgent : Agent
{

    public Camera mainCamera;
    public CinemachineVirtualCamera otherCamera;
    public TicTacToePlayerAgent otherPlayer;

    BehaviorParameters controller;
    BrainParameters brain;
    int inARowToWin;
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
    int nextRewardAtTurn;
    float timePassedSinceFirstUpdate;
    bool winChecker;
    bool tieChecker;
    bool placedMarker;
    Sprite mark1;
    Sprite mark2;

    List<string> deadSpacesCoords = new List<string>();
    List<GameObject> xPlacements = new List<GameObject>();
    List<GameObject> oPlacements = new List<GameObject>();
    List<GameObject> winningConnectors = new List<GameObject>();

    public override void Initialize() {
        controller = this.gameObject.GetComponent<BehaviorParameters>();
        brain = controller.BrainParameters;
    }

    public override void OnEpisodeBegin() { 
        checkForMarkChangingGameObjects();
        createTicTacToeBoard();
        if (this.gameObject.layer == 2) {
            turnDecider = (int)Random.Range(0f, 2f);
        }
        else {
            turnDecider = otherPlayer.getTurnNumber();
        }
        brain.VectorObservationSize = boxHeight * boxWidth;
    }
    
    public override void CollectObservations(VectorSensor sensor) {
        for (int height = 0; height < boxHeight; height++) {
            for (int width = 0; width < boxWidth; width++) {
                GameObject selectedBlock = GameObject.Find("Block (" + height + ", " + width + ")");
                if (selectedBlock != null) {
                    SpriteRenderer blockSprite = selectedBlock.GetComponent<SpriteRenderer>();
                    if (blockSprite.color != Color.black) {
                        if (blockSprite.sprite != mark1 && blockSprite.sprite != mark2) {
                            sensor.AddObservation(1);
                        }
                        else if (blockSprite.sprite == mark1) {
                            sensor.AddObservation(2);
                        }
                        else if (blockSprite.sprite == mark2) {
                            sensor.AddObservation(3);
                        }
                    }
                    else {
                        sensor.AddObservation(0);
                    }
                }
            }
        }
    }

    public override void OnActionReceived(float[] vectorAction) {
        if (timePassedSinceFirstUpdate > 0.125f) {
            if (!tieChecker) {
                if (!winChecker) {
                    if (turnDecider % 2 == controller.TeamId) {
                        float xPixelPosition = ((vectorAction[0] + 1f)/2f) * Screen.width;
                        float yPixelPosition = ((vectorAction[1] + 1f)/2f) * Screen.height;  
                        Debug.Log(this.gameObject.name + "'s position: " + xPixelPosition + ", " + yPixelPosition); 
                        placingMarksAndUpdatingBoard(xPixelPosition, yPixelPosition);
                    }
                }
                else {
                    if (turnDecider % 2 == controller.TeamId) {
                       AddReward(10f);
                    }
                    else {
                        AddReward(-10f);
                    }
                    resetBoard();
                    otherPlayer.resetBoard();
                    EndEpisode();
                    otherPlayer.EndEpisode();
                }
            }
            else {
                AddReward(10f);
                resetBoard();
                otherPlayer.resetBoard();
                EndEpisode();
                otherPlayer.EndEpisode();
            }
        }
        timePassedSinceFirstUpdate += Time.deltaTime;
    }

    public override void Heuristic(float[] actionsOut) { //player input from human

    }

    void Update() {
        if (!tieChecker) {
            if (!winChecker) {
                if (Input.GetMouseButtonUp(0)) {
                    placingMarksAndUpdatingBoard(Input.mousePosition.x, Input.mousePosition.y);
                } 
            }
        }

        Debug.Log(this.gameObject.name + ": " + GetCumulativeReward());
    }

    private void checkForMarkChangingGameObjects() {
        if (GameObject.Find("Xs and Os selected")) {
            mark1 = Resources.Load<Sprite>("xmark");
            mark2 = Resources.Load<Sprite>("omark");
        }
        else if (GameObject.Find("Greens and Reds selected")) {
            mark1 = Resources.Load<Sprite>("greenmark");
            mark2 = Resources.Load<Sprite>("redmark");
        }
    }

    private void createTicTacToeBoard() {
        GameObject[] boardComponents = GameObject.FindGameObjectsWithTag("Board");
        for (int boardItem = 0; boardItem < boardComponents.Length; boardItem++) {
            Destroy(boardComponents[boardItem]);
        }
        
        setWidthAndHeightOfNewBoard();
        setMarksInARowToWinNewBoard();
        setNumOfDeadSpaces();

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

    }

    private void setWidthAndHeightOfNewBoard() {
        if (boardNum == 0) {
            boxWidth = (int)GameObject.Find("boxWidthDecider").transform.position.x;
            boxHeight = (int)GameObject.Find("boxHeightDecider").transform.position.x;
        }
        else {
            if (this.gameObject.layer == 2) {
                GameObject maxDimensionSizeGameObject = GameObject.Find("Dimension Changing GameObject");
                boxWidth = (int)Random.Range(3.0f, maxDimensionSizeGameObject.transform.position.x);
                boxHeight = (int)Random.Range(3.0f, maxDimensionSizeGameObject.transform.position.x);
                otherPlayer.setWidth(boxWidth);
                otherPlayer.setHeight(boxHeight);
            }
        }
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
            if (this.gameObject.layer == 2) {
                GameObject deadSpaceChangingObject = GameObject.Find("Dead Space Changing GameObject");
                if (deadSpaceChangingObject.layer == 1) {
                    numOfDeadSpaces = (int)Random.Range(0f, (float)((int)(Mathf.Sqrt((float)(boxWidth*boxHeight))) - 2));
                }
                else {
                    numOfDeadSpaces = (int)Random.Range(0f, (float)(boxWidth*boxHeight) + 1f);
                }
                otherPlayer.setDeadSpaceAmount(numOfDeadSpaces);
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
                otherPlayer.setDeadSpacePlacement(deadSpacesCoords);
            }
        }
    }

    public void placingMarksAndUpdatingBoard(float xPos, float yPos) {
        GameObject clickLocater = new GameObject("Locater");
        clickLocater.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        clickLocater.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(xPos, yPos, 0f));
        if ((clickLocater.transform.position.x >= -1*(boxWidth*2.5)/2 && clickLocater.transform.position.x <= (boxWidth*2.5)/2) && (clickLocater.transform.position.y >= -1*(boxHeight*2.5)/2 && clickLocater.transform.position.y <= (boxHeight*2.5)/2)) {
            squaresDown = (int)((clickLocater.transform.position.y - ((boxHeight*2.5)/2))/-2.5);
            squaresRight = (int)((clickLocater.transform.position.x + ((boxWidth*2.5)/2))/2.5);
            if (squaresRight >= boxWidth) {
                squaresRight--;
            }
            if (squaresDown >= boxHeight) {
                squaresDown--;
            }
            GameObject selectedBlock = GameObject.Find("Block (" + squaresDown + ", " + squaresRight + ")");
            SpriteRenderer blockSprite = selectedBlock.GetComponent<SpriteRenderer>();
            Sprite markSprite;
            if (blockSprite.color != Color.black && (blockSprite.sprite != mark1 && blockSprite.sprite != mark2)) {
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
                otherPlayer.setTurnNumber(turnDecider);
                otherPlayer.setPlacements(xPlacements, oPlacements);

                if (!placedMarker) {
                    AddReward(0.1f);
                    placedMarker = true;
                    nextRewardAtTurn = turnDecider + 1;
                }
                else {
                    if (nextRewardAtTurn == turnDecider) {
                        placedMarker = false;
                    }
                }
            }
            else {
                AddReward(-0.1f);
            }
        }
        else {
            AddReward(-0.1f);
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

    private void resetBoard() {
        inARowToWin = 0;
        boxWidth = 0;
        boxHeight = 0;
        numOfDeadSpaces = 0;
        squaresDown = 0;
        squaresRight = 0;
        turnDecider = (int)Random.Range(0f, 2f);
        connectors = 0;
        winningOrientation = 0;
        xWinningConnections = 0;
        oWinningConnections = 0;
        boardNum++;
        nextRewardAtTurn = 0;
        timePassedSinceFirstUpdate = 0f;
        winChecker = false;
        tieChecker = false;
        placedMarker = false;

        deadSpacesCoords.Clear();
        xPlacements.Clear();
        oPlacements.Clear();
        winningConnectors.Clear();
    }

    public int getTurnNumber() {
        return turnDecider;
    }

    private void setTurnNumber(int turn) {
        turnDecider = turn;
    }

    private void setHeight(int height) {
        boxHeight = height;
    }
    
    private void setWidth(int width) {
        boxWidth = width;
    }

    private void setDeadSpaceAmount(int amount) {
        numOfDeadSpaces = amount;
    }

    private void setDeadSpacePlacement(List<string> placements) {
        deadSpacesCoords = placements;
    }

    private void setPlacements(List<GameObject> xStuff, List<GameObject> oStuff) {
        xPlacements = xStuff;
        oPlacements = oStuff;
    }

    public bool getPlacedMarkStatus() {
        return placedMarker;
    }

    private void setMarkStatus(bool status) {
        placedMarker = status;
    }
}
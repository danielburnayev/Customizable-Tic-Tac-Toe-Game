using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RetrievedMenuData {
    
    public string markName;
    public float maxDimesnsions;
    public int areDeadSpacesCalculated;

    public RetrievedMenuData(MainMenuScript menu) {
        markName = menu.getMarks().name;
        
        maxDimesnsions = menu.getMaxDimensions().transform.position.x;
        
        areDeadSpacesCalculated = menu.getCalculatedDeadSpaces().layer;
    }

}

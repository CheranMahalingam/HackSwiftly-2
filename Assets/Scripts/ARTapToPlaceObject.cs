using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ARTapToPlaceObject : MonoBehaviour
{
    private ARRaycastManager rayManager; // controller
    private Pose placementPose; //coordinate
    private bool placementPoseIsValid = false;//place holder that change when falt surface detexted
    private bool checkPreview;
    public GameObject placementIndicator;//indicator of flat floor (a picture)
/////////list of item that you can select////////
    public GameObject microwave;
    public GameObject tree;
    public GameObject grass;
    public GameObject indoorPlant;
    private GameObject objectToPlace;// selected item from list of item above
    private GameObject objectPreview;
    private GameObject previewing = null;
    private GameObject newObject;
    private GameObject objectSelected;
    private Vector2 fingerLeft;//swipe detection
    private Vector2 fingerRight;//swipe detection
    public Canvas canvas;
    private bool detectSwipe = false;
    private float SWIPE_THRESHOLD = 100f;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Stack<GameObject> objectsChanged = new Stack<GameObject>();
    private Stack<int> changes = new Stack<int>();
    private Stack<double> footprintChanges = new Stack<double>();
    private Stack<GameObject> undoneObjects = new Stack<GameObject>();
    private Stack<int> undoneChanges = new Stack<int>();
    private Stack<double> undoneFootprint = new Stack<double>();
    public Animator animator1;
    public Animator animator2;
    public Animator animator3;
    public Animator animator4;
    public Animator animator5;
    public Animator animator6;
    public Animator trashAnimator;
    private int cnt = 0;
    private Text text;
    private string userCountry;
    private double userPerCapita, userEmissionPerEnergy;
    private double footprintValue = 0;
    private double objectFootprint = 0;

    // Temporary values (NOT ACCURATE)
    private double microwaveFootprint = 1.2 * 365 / 48000;
    private double treeFootprint = -3.1;
    private double grassFootprint = -1.2;
    private double indoorPlantFootprint = -1.8;

    // Hardcoding researched data
    private string[] countries = { "Africa", "Algeria", "Argentina", "Asia", "Asia (excl. China & India)", "Australia", "Austria", "Azerbaijan", "Bangladesh", "Belarus", "Belgium", "Brazil", "Bulgaria", "Canada", "Chile", "China", "Colombia", "Croatia", "Cyprus", "Czech Republic", "Denmark", "EU-27", "EU-28", "Ecuador", "Egypt", "Estonia", "Europe", "Europe (excl. EU-27)", "Europe (excl. EU-28)", "Finland", "France", "Germany", "Greece", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Israel", "Italy", "Japan", "Kazakhstan", "Kuwait", "Latvia", "Lithuania", "Luxembourg", "Macedonia", "Malaysia", "Mexico", "Morocco", "Netherlands", "New Zealand", "North America", "North America (excl. USA)", "Norway", "Oceania", "Oman", "Pakistan", "Peru", "Philippines", "Poland", "Portugal", "Qatar", "Romania", "Russia", "Saudi Arabia", "Singapore", "Slovakia", "Slovenia", "South Africa", "South America", "South Korea", "Spain", "Sri Lanka", "Sweden", "Switzerland", "Taiwan", "Thailand", "Trinidad and Tobago", "Turkey", "Turkmenistan", "Ukraine", "United Arab Emirates", "United Kingdom", "United States", "Uzbekistan", "Venezuela", "Vietnam", "World" };
    private Dictionary<string, int> countryIndex = new Dictionary<string, int>();
    private double[] perCapita = { 1.0993818917121063, 3.687725231194427, 4.4077322843963005, 4.40271809538225, 4.134082534486007, 16.877695238710345, 7.746022465906321, 3.697285836630497, 0.5310083565340248, 6.924370593319992, 8.680126474865046, 2.182616134412372, 6.311851950770211, 15.33141517858052, 4.585221051439735, 7.049837003334932, 1.9586683858107736, 4.475474885113382, 6.2999334289560975, 9.932975212688957, 6.051757903357753, 6.896694283797919, 6.7324937680480765, 2.453487110215181, 2.4263971772503874, 14.783869868717536, 7.530424281024628, 8.431121554888305, 9.218675435484837, 8.507890447308046, 5.199387784323418, 9.130960322189397, 7.022126794425618, 5.852471830829396, 5.135645359775103, 10.773324882441118, 1.9621606806349396, 2.2972843121351096, 8.80702257424004, 5.312351621454673, 8.078671169673767, 7.666879555614273, 5.575514858356949, 9.134928103311148, 17.56478182059321, 23.70361783534803, 3.7295870054339315, 4.839151253791632, 15.863719913098796, 3.4894041903445108, 8.073031541488215, 3.7825377308942647, 1.8404553761559288, 9.473854653791745, 7.329914479039798, 11.478636342461545, 4.879712359077876, 8.303918336644474, 11.485521382152582, 13.93478565775504, 1.0533030474723015, 1.7359537758135262, 1.2664584198293032, 9.059067734881962, 4.965424330899978, 37.96653291372538, 3.796817166833256, 11.738429162114768, 18.434622245072717, 7.096495091076375, 6.607058716689194, 6.94503274492473, 8.090290042510969, 2.6292285882169684, 12.873988602584767, 5.744625772654941, 1.100780203838032, 4.1142048974108265, 4.324823107506513, 11.575612888086313, 4.151482978206693, 31.27851855955225, 5.200140710555262, 13.654702157750151, 5.085616219962497, 21.34797284494581, 5.6453470540704265, 16.558679664323854, 2.8102354847859115, 4.803704171799324, 2.162946588789357, 4.792598235492193 };
    private double[] emissionPerEnergy = { 0.4074039326966968, 0.23924256624025564, 0.20019536334085736, 0.2535234307895213, 0.2226883453142788, 0.2499421058852147, 0.16746289098466866, 0.21913915276532828, 0.2057532868753944, 0.227457886857395, 0.13250575907733958, 0.13492885479403255, 0.21236966784825945, 0.14339972206534932, 0.1920762820242496, 0.2738450905834177, 0.18818986523297812, 0.19210163969407834, 0.23198497778685995, 0.2297502083084381, 0.18629396098018464, 0.18080189967760768, 0.1804401611365617, 0.21748853867613174, 0.2229189212275067, 0.2473658877217804, 0.1884422837429299, 0.19876530918823546, 0.2033533997703566, 0.14127832812024488, 0.12369536684297125, 0.210116276868668, 0.2305978411791196, 0.12945478019900056, 0.1818394597515844, 0.05704494093102451, 0.2835360881598765, 0.2833141285652604, 0.21833277720881653, 0.35029846479101473, 0.220236713295543, 0.2212384096802575, 0.19652664791076804, 0.2300643900826287, 0.35575744596217274, 0.2165627156186795, 0.16471110353897905, 0.20273300084593776, 0.21407509798319813, 0.23118217741041294, 0.21967954123893976, 0.22397147057175634, 0.27457971080795923, 0.16699922061044187, 0.13662395526186846, 0.204142290473374, 0.195636890756588, 0.08095450813875146, 0.24383014414783805, 0.1859924259906288, 0.2206311861822439, 0.1898568901452664, 0.2452145176156733, 0.2794069009676688, 0.16538679500931516, 0.18473559868086054, 0.1985107130628271, 0.2014276845641016, 0.2071304822888824, 0.03846734178986535, 0.19009086273495315, 0.18025377222691025, 0.3296831158627669, 0.17080367452963735, 0.18181766925570936, 0.16343004372912834, 0.2657754668272164, 0.06966057720797529, 0.12584972065536407, 0.19836873343261632, 0.18932619846146373, 0.24397663045321005, 0.2386193862342348, 0.2196430340656992, 0.22419026522216784, 0.1572873026975691, 0.17768375957698182, 0.20621803868697186, 0.1794614851278662, 0.19419933581099436, 0.2134094252158901, 0.2318847106282661 };

    void Awake()
    {
        // Load the Arial font from the Unity Resources folder.
        Font arial;
        arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        // Create Canvas GameObject.
        GameObject canvasGO = new GameObject();
        canvasGO.name = "Canvas";
        canvasGO.AddComponent<Canvas>();
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Get canvas from the GameObject.
        Canvas canvas;
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Create the Text GameObject.
        GameObject textGO = new GameObject();
        textGO.transform.parent = canvasGO.transform;
        textGO.AddComponent<Text>();

        // Set Text component properties.
        text = textGO.GetComponent<Text>();
        text.font = arial;
        text.fontSize = 78;
        text.alignment = TextAnchor.MiddleCenter;

        // Provide Text position and size using RectTransform.
        RectTransform rectTransform;
        rectTransform = text.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, 0, 0);
        rectTransform.sizeDelta = new Vector2(600, 200);
    }


    // Start is called before the first frame update
    void Start()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        canvas.enabled = false;

        // Preprocessing for generating faster mapping
        for (int i = 0; i < countries.Length; i++)
        {
            countryIndex[countries[i]] = i;
        }

        // Get user information
        userCountry = System.Globalization.RegionInfo.CurrentRegion.EnglishName;
        int userIndex;
        if (countryIndex.ContainsKey(userCountry))
        {
            userIndex = countryIndex[userCountry];
        }
        else
        {
            userIndex = countries.Length - 1;
        }
        userPerCapita = perCapita[userIndex];
        userEmissionPerEnergy = emissionPerEnergy[userIndex];
    }

    // Update is called once per frame
    void Update()
    {
        if (previewing)
        {
            Destroy(previewing);
        }
        UpdatePlacementPose();
        UpdatePlacementIndicator();

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                detectSwipe = false;
                fingerLeft = touch.position;
                fingerRight = touch.position;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                fingerRight = touch.position;
                checkSwipe();
            }

            if (touch.phase == TouchPhase.Ended)
            {
                fingerRight = touch.position;

                if (!checkPreview && !IsPointerAboveUIObject())
                {
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.name == "NaturePack_Grass1" || hit.transform.name == "default" || hit.transform.name == "Plane.001" || hit.transform.name == "01Alocasia_fbx")
                        {
                            objectSelected = hit.transform.gameObject;
                            objectSelected.SetActive(false);
                            animator1.SetTrigger("LeftButton");
                            animator2.SetTrigger("LeftButton");
                            animator3.SetTrigger("LeftButton");
                            animator4.SetTrigger("RightButton");
                            animator5.SetTrigger("RightButton");
                            animator6.SetTrigger("RightButton");
                            trashAnimator.SetTrigger("trashFadeIn");

                            checkPreview = true;
                            if (hit.transform.name == "NaturePack_Grass1")
                                selectGrass();
                            else if (hit.transform.name == "default")
                                selectTree();
                            else if (hit.transform.name == "Plane.001")
                                selectMicrowave();
                            else if (hit.transform.name == "01Alocasia_fbx")
                                selectIndoorPlant();
                            footprintValue -= objectFootprint;
                            text.text = Math.Round(footprintValue, 3).ToString();
                        }
                    }
                }
            }
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        rayManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;

        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;
            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            if (objectPreview && checkPreview)
            {
                //objectPreview.SetActive(true);
                //objectPreview.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
                previewing = Instantiate(objectPreview, placementPose.position, placementPose.rotation) as GameObject;
            }
        } 
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void PlaceObject()
    {
        if (objectToPlace)
        {
            if (objectSelected)
            {
                changes.Push(2);
                footprintChanges.Push(objectFootprint);
                objectsChanged.Push(objectSelected);
                changes.Push(3);
            }
            else
                changes.Push(1);

            GameObject newObject = Instantiate(objectToPlace, placementPose.position, placementPose.rotation) as GameObject;
            footprintValue += objectFootprint;
            text.text = Math.Round(footprintValue, 3).ToString() + " kg of CO\x2082";
            objectsChanged.Push(newObject);
            footprintChanges.Push(objectFootprint);
            clearRedoStack();
        }
    }

    public void DeleteObject()
    {
        if (objectSelected) {
            GameObject currentObject = objectSelected;
            currentObject.SetActive(false);
            objectsChanged.Push(currentObject);
            changes.Push(0);
            footprintChanges.Push(objectFootprint);
            clearRedoStack();
            checkPreview = false;   // special! Do not call previewingMode because it will bring back original position of selected object
            objectSelected = null;
            animator1.SetTrigger("LeftButton");
            animator2.SetTrigger("LeftButton");
            animator3.SetTrigger("LeftButton");
            animator4.SetTrigger("RightButton");
            animator5.SetTrigger("RightButton");
            animator6.SetTrigger("RightButton");
            trashAnimator.SetTrigger("trashFadeOut");
        }
    }

    public void undo()
    {
        if (objectSelected)     // should not undo when object is selected
            return;
        if (changes.Count == 0) // empty stack
            return;

        int itemsToPop;
        if (changes.Peek() >= 2)
            itemsToPop = 2;
        else
            itemsToPop = 1;

        for (int i = 0;i < itemsToPop;i++)
        {
            int lastChange = changes.Pop();
            double tempFootprint = footprintChanges.Pop();
            if (lastChange % 2 == 1)
            {
                GameObject currentObject = objectsChanged.Pop();
                currentObject.SetActive(false);
                Debug.LogWarning("tempfootprint: " + tempFootprint);
                footprintValue -= tempFootprint;
                text.text = Math.Round(footprintValue, 3).ToString() + " kg of CO\x2082";

                undoneObjects.Push(currentObject);
                undoneChanges.Push(lastChange ^ 1);
                undoneFootprint.Push(tempFootprint);
            }
            else if (lastChange % 2 == 0)
            {
                GameObject currentObject = objectsChanged.Pop();
                currentObject.SetActive(true);
                footprintValue += tempFootprint;
                text.text = Math.Round(footprintValue, 3).ToString() + " kg of CO\x2082";

                undoneObjects.Push(currentObject);
                undoneChanges.Push(lastChange ^ 1);
                undoneFootprint.Push(tempFootprint);
            }
        }
    }

    public void redo()
    {
        if (objectSelected)     // should not redo when object is selected
            return;
        if (undoneChanges.Count == 0) // empty stack
            return;

        int itemsToPop;
        if (undoneChanges.Peek() >= 2)
            itemsToPop = 2;
        else
            itemsToPop = 1;

        for (int i = 0;i < itemsToPop;i++)
        {
            int lastChange = undoneChanges.Pop();
            double tempFootprint = undoneFootprint.Pop();
            if (lastChange % 2 == 0)
            {
                GameObject currentObject = undoneObjects.Pop();
                currentObject.SetActive(true);
                footprintValue += tempFootprint;
                text.text = Math.Round(footprintValue, 3).ToString() + " kg of CO\x2082";

                objectsChanged.Push(currentObject);
                changes.Push(lastChange ^ 1);
                footprintChanges.Push(tempFootprint);
            }
            else if (lastChange % 2 == 1)
            {
                GameObject currentObject = undoneObjects.Pop();
                currentObject.SetActive(false);
                footprintValue -= tempFootprint;
                text.text = Math.Round(footprintValue, 3).ToString() + " kg of CO\x2082";

                objectsChanged.Push(currentObject);
                changes.Push(lastChange ^ 1);
                footprintChanges.Push(tempFootprint);
            }
        }
    }

    private void clearRedoStack()
    {
        while (undoneChanges.Count > 0)
        {
            if (undoneChanges.Pop() % 2 == 0)
                Destroy(undoneObjects.Peek());
            undoneObjects.Pop();
            undoneFootprint.Pop();
        }
    }

    public void previewMode()   // called when cancelled but not checkmarked
    {
        checkPreview = !checkPreview;
        if (objectSelected && !checkPreview)
        {
            objectSelected.SetActive(true);
            objectSelected = null;
            footprintValue += objectFootprint;
            text.text = Math.Round(footprintValue, 3).ToString() + " kg of CO\x2082";
        }
    }

    public void previewModePlacement()  // called when checkmarked
    {
        if (placementPoseIsValid)
        {
            PlaceObject();
            if (objectSelected)
                objectSelected = null;
            checkPreview = false;
        }
    }

    private bool IsPointerAboveUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void checkSwipe()
    {
        if (horizontalMove() > SWIPE_THRESHOLD)
        {
            showUI();
            detectSwipe = true;
        }
        else if (horizontalMove() < -SWIPE_THRESHOLD)
        {
            hideUI();
            detectSwipe = true;
        }
    }

    private float horizontalMove()
    {
        return fingerRight.x - fingerLeft.x;
    }

    private void showUI()
    {
        canvas.enabled = true;
    }

    private void hideUI()
    {
        canvas.enabled = false;
    }

    public void selectMicrowave()
    {
        objectToPlace = microwave;
        objectPreview = microwave;
        objectFootprint = microwaveFootprint * userEmissionPerEnergy;
    }

    public void selectTree()
    {
        objectToPlace = tree;
        objectPreview = tree;
        objectFootprint = -42;
    }

    public void selectGrass()
    {
        objectToPlace = grass;
        objectPreview = grass;
        objectFootprint = 0.099;
    }

    public void selectIndoorPlant()
    {
        objectToPlace = indoorPlant;
        objectPreview = indoorPlant;
        objectFootprint = -5;
    }

    public void HapticFeedBack()
    {
        Vibration.VibrateMs(200);
    }
}

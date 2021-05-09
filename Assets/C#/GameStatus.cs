using UnityEngine;

public class GameStatus : MonoBehaviour
{
    public GameObject oPlayer;
    private Vector3 vOffset = new Vector3(-8.0f / 10.0f, -17.0f / 10.0f, -5.8f / 10.0f); //from camera (x,y)
    private Vector3 vOffsetNoVR = new Vector3(-6.0f / 10.0f, -6.0f / 10.0f, -5.8f / 10.0f); //from camera (x,y)
    //private Vector3 vOffsetNoVR = new Vector3(-9.15f / 10.0f, -6.3f / 10.0f, -5.8f / 10.0f); //from camera (x,y)

    public GameLevel oMap;
    private Vector3 vMapSize;

    public GameObject oTextTime, oTextLapProgress, oTextScore, oTextLives;
    public GameObject oHealthBar, oFuelBar, oCargoBar;

    public GameObject oLeft, oRight, oBottom;
    public GameObject oBack;

    float BAR_LENGTH = 5.0f;

    Material oMatRed;
    Material oMatOriginal;

    public void Init(bool i_bIsRace)
    {
        oTextTime.SetActive(i_bIsRace);
        oTextLapProgress.SetActive(i_bIsRace);
        oTextScore.SetActive(!i_bIsRace);
        oTextLives.SetActive(!i_bIsRace);
        oHealthBar.SetActive(true);
        oFuelBar.SetActive(!i_bIsRace);
        oCargoBar.SetActive(!i_bIsRace);

        gameObject.SetActive(true);

        if (!GameManager.bNoVR) transform.eulerAngles = new Vector3(45, 0, 0);
        transform.localScale = new Vector3(0.68f, 0.68f, 0.68f);

        string szMaterial = i_bIsRace ? "Status2" : "Status";
        Material oMaterial = Resources.Load(szMaterial, typeof(Material)) as Material;
        oBack.GetComponent<Renderer>().material = oMaterial;

        oMatRed = Resources.Load("Ship_Body", typeof(Material)) as Material;
        oMatOriginal = oCargoBar.GetComponent<Renderer>().material;

        vMapSize = oMap.GetMapSize();
        vMapSize.x /= 10.0f;
        vMapSize.y /= 10.0f;

        if (i_bIsRace)
        {
            oLeft.transform.localPosition = new Vector3(-3.625f, -1.8f + 1.35f, 0.4f) / 10.0f;
            oLeft.transform.localScale = new Vector3(0.25f, 3.80f, 0.25f) / 10.0f;
            oRight.transform.localPosition = new Vector3(2.625f, -1.8f + 1.35f, 0.4f) / 10.0f;
            oRight.transform.localScale = new Vector3(0.25f, 3.80f, 0.25f) / 10.0f;
            oBottom.transform.localPosition = new Vector3(-0.5f, -4.925f + 2.70f, 0.4f) / 10.0f;
        }
    }

    public void SetForRace(float i_fHealth, float i_fTime, string i_szLapProgress)
    {
        if (i_fHealth < 0.01f) i_fHealth = 0.01f; //done because 0 makes a black quad
        oHealthBar.transform.localPosition = new Vector3(-3 + ((i_fHealth * BAR_LENGTH) / 2), -1.5f, 0) / 10.0f;
        oHealthBar.transform.localScale = new Vector3((i_fHealth * BAR_LENGTH), 1, 1) / 10.0f;

        oTextTime.GetComponent<TextMesh>().text = i_fTime.ToString("N2");
        oTextLapProgress.GetComponent<TextMesh>().text = i_szLapProgress;
    }

    public void SetForMission(float i_fHealth, int i_iNumLives, float i_fCargo, bool i_bCargoFull, float i_fFuel, float i_fScore)
    {
        if (i_fHealth < 0.01f) i_fHealth = 0.01f; //done because 0 makes a black quad
        oHealthBar.transform.localPosition = new Vector3(-3 + ((i_fHealth * BAR_LENGTH) / 2), -1.5f, 0) / 10.0f;
        oHealthBar.transform.localScale = new Vector3((i_fHealth * BAR_LENGTH), 1, 1) / 10.0f;

        oTextScore.GetComponent<TextMesh>().text = i_fScore.ToString();
        oTextLives.GetComponent<TextMesh>().text = "x "+i_iNumLives.ToString();

        if (i_fFuel < 0.01f) i_fFuel = 0.01f; //done because 0 makes a black quad
        oFuelBar.transform.localPosition = new Vector3(-3 + ((i_fFuel * BAR_LENGTH) / 2), -2.75f, 0) / 10.0f;
        oFuelBar.transform.localScale = new Vector3((i_fFuel * BAR_LENGTH), 1, 1) / 10.0f;
        if (i_fCargo < 0.01f) i_fCargo = 0.01f; //done because 0 makes a black quad
        oCargoBar.transform.localPosition = new Vector3(-3 + ((i_fCargo * BAR_LENGTH) / 2), -4.0f, 0) / 10.0f;
        oCargoBar.transform.localScale = new Vector3((i_fCargo * BAR_LENGTH), 1, 1) / 10.0f;

        if (!i_bCargoFull) oCargoBar.GetComponent<Renderer>().material = oMatOriginal;
        else oCargoBar.GetComponent<Renderer>().material = oMatRed;
    }

    void LateUpdate()
    {
        Vector3 v = CameraController.vCamPos;
        v.z = 0;
        if (GameManager.bNoVR)
        {
            transform.position = v + vOffsetNoVR;
        }
        else
        {
            //limit left/bottom movement:
            v += vOffset;
            float fLeftLimit = -(vMapSize.x / 2.0f) - .40f;
            if (v.x < fLeftLimit) v.x = fLeftLimit;
            float fBottomLimit = -(vMapSize.y / 2.0f) + .30f;
            if (v.y < fBottomLimit) v.y = fBottomLimit;
            transform.position = v;
        }
    }
}

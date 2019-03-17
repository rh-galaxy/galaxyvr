using UnityEngine;

public class GameStatus : MonoBehaviour
{
    public GameObject oPlayer;
    private Vector3 vOffset = new Vector3(-8, -17, -8);
    private Vector3 vOffsetNoVR = new Vector3(-7.5f, -5.5f, -8);

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

        if (!GameManager.bNoVR) transform.Rotate(45, 0, 0);
        else
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            if (!i_bIsRace) vOffsetNoVR.y = -4.0f;
        }

        string szMaterial = i_bIsRace ? "Status2" : "Status";
        Material oMaterial = Resources.Load(szMaterial, typeof(Material)) as Material;
        oBack.GetComponent<Renderer>().material = oMaterial;

        oMatRed = Resources.Load("Ship_Body", typeof(Material)) as Material;
        oMatOriginal = oCargoBar.GetComponent<Renderer>().material;

        vMapSize = oMap.GetMapSize();

        if(i_bIsRace)
        {
            oLeft.transform.localPosition = new Vector3(-3.625f, -1.8f + 1.35f, 0.4f);
            oLeft.transform.localScale = new Vector3(0.25f, 3.80f, 0.25f);
            oRight.transform.localPosition = new Vector3(2.625f, -1.8f + 1.35f, 0.4f);
            oRight.transform.localScale = new Vector3(0.25f, 3.80f, 0.25f);
            oBottom.transform.localPosition = new Vector3(-0.5f, -4.925f + 2.70f, 0.4f);
        }
    }

    public void SetForRace(float i_fHealth, float i_fTime, string i_szLapProgress)
    {
        if (i_fHealth < 0) i_fHealth = 0;
        oHealthBar.transform.localPosition = new Vector3(-3 + ((i_fHealth * BAR_LENGTH) / 2), -1.5f, 0);
        oHealthBar.transform.localScale = new Vector3((i_fHealth * BAR_LENGTH), 1, 1);

        oTextTime.GetComponent<TextMesh>().text = i_fTime.ToString("N2");
        oTextLapProgress.GetComponent<TextMesh>().text = i_szLapProgress;
    }

    public void SetForMission(float i_fHealth, int i_iNumLives, float i_fCargo, bool i_bCargoFull, float i_fFuel, float i_fScore)
    {
        if (i_fHealth < 0) i_fHealth = 0;
        oHealthBar.transform.localPosition = new Vector3(-3 + ((i_fHealth * BAR_LENGTH) / 2), -1.5f, 0);
        oHealthBar.transform.localScale = new Vector3((i_fHealth * BAR_LENGTH), 1, 1);

        oTextScore.GetComponent<TextMesh>().text = i_fScore.ToString();
        oTextLives.GetComponent<TextMesh>().text = "x "+i_iNumLives.ToString();

        oFuelBar.transform.localPosition = new Vector3(-3 + ((i_fFuel * BAR_LENGTH) / 2), -2.75f, 0);
        oFuelBar.transform.localScale = new Vector3((i_fFuel * BAR_LENGTH), 1, 1);
        oCargoBar.transform.localPosition = new Vector3(-3 + ((i_fCargo * BAR_LENGTH) / 2), -4.0f, 0);
        oCargoBar.transform.localScale = new Vector3((i_fCargo * BAR_LENGTH), 1, 1);

        if (!i_bCargoFull) oCargoBar.GetComponent<Renderer>().material = oMatOriginal;
        else oCargoBar.GetComponent<Renderer>().material = oMatRed;
    }

    void LateUpdate()
    {
        if (GameManager.bNoVR)
        {
            Vector3 v = CameraController.vCamPos;
            v.z = 0;
            transform.position = v + vOffsetNoVR;
        }
        else
        {
            //transform.position = oPlayer.transform.position + vOffset;

            //limit left/bottom movement instead:
            Vector3 v = oPlayer.transform.position + vOffset;
            float fLeftLimit = -(vMapSize.x / 2.0f) - 3.0f;
            if (v.x < fLeftLimit) v.x = fLeftLimit;
            float fBottomLimit = -(vMapSize.y / 2.0f) + 3.0f;
            if (v.y < fBottomLimit) v.y = fBottomLimit;
            transform.position = v;
        }
    }
}

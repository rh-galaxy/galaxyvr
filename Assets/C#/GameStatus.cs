using UnityEngine;

public class GameStatus : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset = new Vector3(-8, -17, -8);

    public GameObject oTextTime, oTextLapProgress, oTextScore, oTextLives;
    public GameObject oHealthBar, oFuelBar, oCargoBar;

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

        transform.Rotate(45, 0, 0);

        string szMaterial = i_bIsRace ? "Status2" : "Status";
        Material oMaterial = Resources.Load(szMaterial, typeof(Material)) as Material;
        oBack.GetComponent<Renderer>().material = oMaterial;

        oMatRed = Resources.Load("Ship_Body", typeof(Material)) as Material;
        oMatOriginal = oCargoBar.GetComponent<Renderer>().material;
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
        transform.position = player.transform.position + offset;
    }
}

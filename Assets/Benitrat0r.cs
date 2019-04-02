using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColumnData
{
    public float speed;
    public float originalSpeed;
    public int slotsPassed;
    public int slotBrake = 10;
    public bool final = false, end = false;
    public GameObject finalSlot = null;
}

public class Benitrat0r : MonoBehaviour
{
    public ColumnData[] columnData = new ColumnData[5];
    public GameObject slotPrefab, slotParent, winSymbols, benisImagePrefab, benisParent, sound,
        digitParent, menu;
    public Sprite[] slotSprites = new Sprite[10], digits = new Sprite[10];
    public Vector3[] rowOffsets = new Vector3[5];
    private List<GameObject> slotObjects = new List<GameObject>();
    public float[] setColSpeeds = new float[5];
    private int[] winRow = new int[5];
    private int winStart = -1, tempWinCount = 0, bet = 16, winBenis = 0;
    private int betBackup = 19, winBackup = 3;
    private bool spinning = false, spinSoundPlayed = false, backPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 5; i++)
        {
            ColumnData newColumn = new ColumnData();
            columnData[i] = newColumn;
        }

        Fill();
    }

    private int GenNewType()
    {
        int type = (int)SlotPrefab.SlotTypes.reich;

        if(Random.Range(0, 512) != 0)
        { //kein reichstangle :(
            bool notFound = true;
            while(notFound)
            {
                notFound = false;

                type = Random.Range(0, 10);
                if(type == (int)SlotPrefab.SlotTypes.reich)
                {
                    notFound = true;
                }
            }
        }

        return type;
    }

    private void Fill()
    {
        for(int i = 0; i < 5; i++)
        { //x
            for(int a = 0; a < 6; a++)
            { //y
                GameObject newSlot = Instantiate(slotPrefab, slotParent.transform);
                newSlot.transform.position = new Vector3(rowOffsets[i].x, rowOffsets[i].y - (76 * a));

                int type = GenNewType();

                newSlot.GetComponent<Image>().sprite = slotSprites[type];
                newSlot.GetComponent<SlotPrefab>().type = type;

                slotObjects.Add(newSlot);
            }
        }
    }

    private void ResetSpin()
    {
        CancelInvoke("FlashWin");
        CancelInvoke("SpawnBlus");

        spinSoundPlayed = false;

        SetWinBenis(0);

        tempWinCount = 0;
        winStart = -1;

        foreach(Transform child in benisParent.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 5; i++)
        {
            columnData[i].slotsPassed = 0;
            winRow[i] = -1;
            winSymbols.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private int GetSlotType()
    {
        int ran = Random.Range(100, 1050);

        int type = (int)SlotPrefab.SlotTypes.fliese;

        if(ran >= 300 && ran < 400)
        {
            type = (int)SlotPrefab.SlotTypes.plug;
        } else if(ran >= 400 && ran < 500)
        {
            type = (int)SlotPrefab.SlotTypes.pepe;
        } else if(ran >= 500 && ran < 600)
        {
            type = (int)SlotPrefab.SlotTypes.ofen;
        } else if(ran >= 600 && ran < 700)
        {
            //type = (int)SlotPrefab.SlotTypes.ban;
        } else if(ran >= 700 && ran < 800)
        {
            type = (int)SlotPrefab.SlotTypes.nyan;
        } else if(ran >= 800 && ran < 850)
        {
            type = (int)SlotPrefab.SlotTypes.ball;
        } else if(ran >= 950 && ran < 1000)
        {
            //type = (int)SlotPrefab.SlotTypes.pr0mium;
        } else if(ran >= 1000 && ran < 1040)
        {
            type = (int)SlotPrefab.SlotTypes.pr0gramm;
        } else if(ran >= 1040 && ran < 1050)
        {
            if (Random.Range(0, 25) == 0)
            {
                type = (int)SlotPrefab.SlotTypes.reich;
            }
        }

        return type;
    }

    private int GetWinBenis(int slotType, int combo)
    {
        int wB = 0;

        switch(combo)
        {
            case 3: //3er-kombo
                switch(slotType)
                {
                    case (int)SlotPrefab.SlotTypes.fliese:
                        wB = bet / 4;
                        break;
                    case (int)SlotPrefab.SlotTypes.ofen:
                    case (int)SlotPrefab.SlotTypes.plug:
                        wB = bet / 2;
                        break;
                    case (int)SlotPrefab.SlotTypes.nyan:
                    case (int)SlotPrefab.SlotTypes.pepe:
                    case (int)SlotPrefab.SlotTypes.ball:
                        wB = bet;
                        break;
                    case (int)SlotPrefab.SlotTypes.pr0gramm:
                        wB = bet * 2;
                        break;
                    case (int)SlotPrefab.SlotTypes.reich:
                        wB = bet * 8;
                        break;
                }

                break;
            case 4: //4er-kombo
                switch (slotType)
                {
                    case (int)SlotPrefab.SlotTypes.fliese:
                        wB = bet * 2;
                        break;
                    case (int)SlotPrefab.SlotTypes.ofen:
                    case (int)SlotPrefab.SlotTypes.plug:
                        wB = bet * 8;
                        break;
                    case (int)SlotPrefab.SlotTypes.nyan:
                    case (int)SlotPrefab.SlotTypes.pepe:
                    case (int)SlotPrefab.SlotTypes.ball:
                        wB = bet * 16;
                        break;
                    case (int)SlotPrefab.SlotTypes.pr0gramm:
                        wB = bet * 32;
                        break;
                    case (int)SlotPrefab.SlotTypes.reich:
                        wB = bet * 128;
                        break;
                }
                break;
            case 5: //5er-kombo
                switch (slotType)
                {
                    case (int)SlotPrefab.SlotTypes.fliese:
                    case (int)SlotPrefab.SlotTypes.plug:
                        wB = bet * 64;
                        break;
                    case (int)SlotPrefab.SlotTypes.ofen:
                    case (int)SlotPrefab.SlotTypes.nyan:
                        wB = bet * 256;
                        break;
                    case (int)SlotPrefab.SlotTypes.pepe:
                        wB = bet * 128;
                        break;
                    case (int)SlotPrefab.SlotTypes.ball:
                        wB = bet * 512;
                        break;
                    case (int)SlotPrefab.SlotTypes.pr0gramm:
                        wB = bet * 1024;
                        break;
                    case (int)SlotPrefab.SlotTypes.reich:
                        wB = bet * 4096;
                        break;
                }
                break;
        }

        if(wB <= 0)
        {
            wB = 1;
        }

        return wB;
    }

    private void SetBetValue(int bet)
    {
        this.bet = bet;
        betBackup = bet + 3;
    }

    private void SetWinBenis(int wB)
    {
        winBenis = wB;
        winBackup = wB + 3;
    }

    public void Spin()
    {
        if(spinning || (UserHandler.GetPlayerUserBenis() < (ulong)bet))
        {
            sound.GetComponent<AudioScript>().PlayErrorSound();
            return;
        }

        UserHandler.SetGlobalUserBenis(UserHandler.GetPlayerUserBenis() - (ulong)bet);

        SetBenis(UserHandler.GetPlayerUserBenis());

        ResetSpin();

        int type = GetSlotType();

        int winType = Random.Range(0, 100);

        int win = 0;

        if(winType >= 32 && winType < 66)
        { //3er kombo
            win = 1;

            int startPos = Random.Range(0, 3);
            winStart = startPos;

            for(int i = startPos; i < 3 + startPos; i++)
            {
                winRow[i] = type;
            }
        } else if(winType >= 66 && winType < 70)
        { //4er kombo
            win = 2;

            int startPos = Random.Range(0, 2);
            winStart = startPos;

            for (int i = startPos; i < 4 + startPos; i++)
            {
                winRow[i] = type;
            }
        } else if(winType >= 70 && winType < 71)
        { //5er kombo
            if (Random.Range(0, 2) == 0)
            {
                win = 3;

                winStart = 0;

                for (int i = 0; i < 5; i++)
                {
                    winRow[i] = type;
                }
            }
        }

        if(win == 0)
        {
            winStart = -1;

            for(int i = 0; i < 5; i++)
            {
                int loseType = Random.Range(0, 9);

                winRow[i] = loseType;

                if(i >= 2)
                {
                    bool notFound = true;

                    while(notFound)
                    {
                        notFound = false;

                        loseType = GenNewType();

                        if (winRow[i-1] == loseType)
                        { //damit keine kombo wenn verloren
                            notFound = true;
                        }
                    }

                    winRow[i] = loseType;
                }
            }
        } else
        {
            SetWinBenis(GetWinBenis(type, win + 2));

            for (int i = 0; i < 5; i++)
            {
                if(winRow[i] == -1)
                {
                    int fillType = Random.Range(0, 9);
                    bool notFound = true;

                    while(notFound)
                    { //damit nicht mehr kombo als gewonnen
                        notFound = false;

                        fillType = GenNewType();

                        if (type == fillType)
                        {
                            notFound = true;
                        }
                    }

                    winRow[i] = fillType;
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            columnData[i].speed = Random.Range(600f, 1500f);
            columnData[i].originalSpeed = columnData[i].speed;
            columnData[i].slotBrake = Random.Range(5, 25);
        }

        spinning = true;

        sound.GetComponent<AudioScript>().PlayStopSpin();
        sound.GetComponent<AudioScript>().StopCoinSound();
    }

    private void ResetSlotObj(GameObject slot, int xrow, float diff)
    {
        slot.transform.position = new Vector3(rowOffsets[xrow].x,
            rowOffsets[xrow].y - diff,
            rowOffsets[xrow].z);

        int newType = GenNewType();

        slot.GetComponent<Image>().sprite = slotSprites[newType];
        slot.GetComponent<SlotPrefab>().type = newType;
    }

    private void FlashWin()
    {
        int pos = winStart + tempWinCount;

        winSymbols.transform.GetChild(pos).gameObject.SetActive(true);

        sound.GetComponent<AudioScript>().PlayWinSound(tempWinCount);
        tempWinCount++;

        if(winStart + tempWinCount > 4 ||
            winRow[winStart + tempWinCount] != winRow[winStart + tempWinCount - 1])
        {
            CancelInvoke("FlashWin");
            InvokeRepeating("SpawnBlus", 0f, 0.15f);
        }
    }

    public void Back()
    {
        if(!spinning)
        {
            ResetSpin();
            sound.GetComponent<AudioScript>().BackStopAmbient();
            menu.GetComponent<MenuData>().Benitrat0rBack();
            return;
        }
    }

    private void SpawnBlus()
    {
        float startX = -593.4f + (winStart * 76);

        int winEnd = 0;

        for(int i = winStart; i < 5; i++)
        {
            if(winSymbols.transform.GetChild(i).gameObject.activeSelf)
            {
                winEnd++;
            }
        }

        float endX = startX + (winEnd * 76);

        Vector3 pos = new Vector3(Random.Range(startX, endX), 1970, 0);

        GameObject newBlus = Instantiate(benisImagePrefab, benisParent.transform);
        newBlus.transform.position = pos;

        Vector3 force = new Vector3(Random.Range(-50f, 50f), 200f);

        newBlus.GetComponent<Rigidbody2D>().AddForce(force * 100);

        sound.GetComponent<AudioScript>().PlayCoinSound();
    }

    public void IncreaseBet()
    {

        if(bet * 2 > 512 || spinning)
        {
            sound.GetComponent<AudioScript>().PlayErrorSound();
            return;
        } else
        {
            SetBetValue(bet * 2);
            sound.GetComponent<AudioScript>().PlayConfirmSound();
        }

        SetBet(bet);
    }

    public void DecreaseBet()
    {
        if(bet / 2 < 4 || spinning)
        {
            sound.GetComponent<AudioScript>().PlayErrorSound();
            return;
        } else
        {
            SetBetValue(bet / 2);
            sound.GetComponent<AudioScript>().PlayConfirmSound();
        }

        SetBet(bet);
    }

    private void SetBet(int bet)
    {
        string betString = bet.ToString("000");

        for(int i = 0; i < betString.Length; i++)
        {
            int num = System.Int32.Parse(betString[i].ToString());

            digitParent.transform.GetChild(0).GetChild(i).GetComponent<Image>().sprite =
                digits[num];
        }
    }

    private void SetWin(int win)
    {
        string winString = win.ToString("0000");

        if(win > 9999)
        {
            digitParent.transform.GetChild(1).GetChild(4).gameObject.SetActive(true);
        } else
        {
            digitParent.transform.GetChild(1).GetChild(4).gameObject.SetActive(false);
        }

        if (win > 99999)
        {
            digitParent.transform.GetChild(1).GetChild(5).gameObject.SetActive(true);
        }
        else
        {
            digitParent.transform.GetChild(1).GetChild(5).gameObject.SetActive(false);
        }

        if(win > 999999)
        {
            digitParent.transform.GetChild(1).GetChild(6).gameObject.SetActive(true);
        } else
        {
            digitParent.transform.GetChild(1).GetChild(6).gameObject.SetActive(false);
        }

        for(int i = 0; i < winString.Length; i++)
        {
            int num = System.Int32.Parse(winString[i].ToString());

            digitParent.transform.GetChild(1).GetChild(i).GetComponent<Image>().sprite =
                digits[num];
        }
    }

    public void SetBenis(ulong benis)
    {
        string benisString = benis.ToString("0000000");

        if(benis > 9999999)
        {
            digitParent.transform.GetChild(2).GetChild(7).gameObject.SetActive(true);
        } else
        {
            digitParent.transform.GetChild(2).GetChild(7).gameObject.SetActive(false);
        }

        if(benis > 99999999)
        {
            digitParent.transform.GetChild(2).GetChild(8).gameObject.SetActive(true);
        } else
        {
            digitParent.transform.GetChild(2).GetChild(8).gameObject.SetActive(false);
        }

        for (int i = 0; i < benisString.Length; i++)
        {
            int num = System.Int32.Parse(benisString[i].ToString());

            digitParent.transform.GetChild(2).GetChild(i).GetComponent<Image>().sprite =
                digits[num];
        }
    }

    private void CancelBackPressed()
    {
        backPressed = false;
    }

    private void FixedUpdate()
    {
       // Debug.Log(columnData[0].slotsPassed + " " + columnData[1].slotsPassed + " "
            //+ columnData[2].slotsPassed + " " + columnData[3].slotsPassed + " " + columnData[4].slotsPassed);
        
        if(MenuData.state == 4 && !spinning)
        { //benitrat0r aktiv

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if (!backPressed)
                {
                    backPressed = true;
                    Invoke("CancelBackPressed", 2f);
                }
                else
                {
                    CancelInvoke("CancelBackPressed");
                    backPressed = false;
                    Back();
                }
            }
        }

        if (spinning)
        {

            bool[] topClean = new bool[5];

            for (int i = 0; i < 5; i++)
            {
                topClean[i] = true;
            }

            foreach (GameObject slot in slotObjects)
            {
                int xrow = 0;

                for (int i = 0; i < 5; i++)
                {
                    if (slot.transform.position.x.Equals(rowOffsets[i].x))
                    {
                        xrow = i;
                        break;
                    }
                }

                if (columnData[xrow].speed.Equals(0f))
                { //skip da col angehalten
                    continue;
                }

                float cleanSpeed = columnData[xrow].speed * Time.fixedDeltaTime;

                slot.transform.position = new Vector3(slot.transform.position.x,
                    slot.transform.position.y - cleanSpeed,
                    slot.transform.position.z);

                if (slot.transform.position.y < (rowOffsets[0].y - 456))
                {
                    columnData[xrow].slotsPassed++;

                    float diff = slot.transform.position.y - (rowOffsets[0].y - 456);
                    diff = Mathf.Abs(diff);

                    ResetSlotObj(slot, xrow, diff);

                    if(columnData[xrow].slotsPassed == columnData[xrow].slotBrake)
                    {
                        columnData[xrow].finalSlot = slot;

                        int newType = winRow[xrow];

                        slot.GetComponent<Image>().sprite = slotSprites[newType];
                        slot.GetComponent<SlotPrefab>().type = newType;
                    }
                }
            }

            bool allDoneSpinning = true;

            for (int i = 0; i < 5; i++)
            {

                if (columnData[i].speed.Equals(0))
                {
                    continue;
                } else
                {
                    allDoneSpinning = false;
                }

                if (columnData[i].slotsPassed >= columnData[i].slotBrake)
                {

                    Vector3 finalSlotPos = columnData[i].finalSlot.transform.position;

                    float distanceToEnd = Vector3.Distance(finalSlotPos,
                        new Vector3(finalSlotPos.x, 1917.4f, finalSlotPos.z));

                    columnData[i].speed = (columnData[i].originalSpeed * (distanceToEnd / 228f));

                    if(columnData[i].speed < 5f)
                    {
                        columnData[i].speed = 0f;

                        if (!spinSoundPlayed)
                        { //sobald 1. stop -> stop spin sound + set win data (falls vorhanden)
                            sound.GetComponent<AudioScript>().PlayStopSpin();
                        }

                        spinSoundPlayed = true;

                        sound.GetComponent<AudioScript>().PlayStopSound(i);
                    }
                }
            }

            if(allDoneSpinning)
            {
                if(winStart > -1)
                {
                    InvokeRepeating("FlashWin", .5f, 0.3f);
                }

                spinning = false;

                if (winBenis > 0)
                { //gewonnen
                    SetWin(winBenis);

                    UserHandler.SetGlobalUserBenis(UserHandler.GetPlayerUserBenis() + (ulong)winBenis);
                    SetBenis(UserHandler.GetPlayerUserBenis());
                }
                else
                {
                    SetWin(0);
                }
            }
        }

        if(winBenis != winBackup - 3)
        {
            winBenis = winBackup - 3;

            Application.Quit();
        }

        if(bet != betBackup - 3)
        {
            bet = betBackup - 3;

            Application.Quit();
        }
    }
}

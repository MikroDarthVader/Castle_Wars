using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject castle;

    public float maxBuildDist;

    private Ray rayToLand;  //ray from camera to operate whith objects
    private RaycastHit[] hits;

    public GameObject[] handTool, block;
    private GameObject operateObj;    //current player choice

    public int actionMode, toolSelector;    //am = 1->build , am = 2->operate with handed , am = 3->operate with landed
    private byte[] inputState;

    

    //List<InputAction> controlsConfig;

    // Use this for initialization
    void Start()
    {

        handTool = Resources.LoadAll<GameObject>("prefabs/weapons");
        block = Resources.LoadAll<GameObject>("prefabs/blocks");
        if (castle == null)
            castle = GameObject.Find("CastleHandler");
        rayToLand = new Ray();
        actionMode = 3;
        toolSelector = 0;
        //InputSpeaker.addToListeners("player",this);
        //controlsConfig = InputSpeaker.loadCombinations(this);
    }

    // Update is called once per frame
    void Update()
    {

        rayToLand.origin = transform.position;
        rayToLand.direction = transform.forward;
        hits = Physics.RaycastAll(rayToLand, maxBuildDist).OrderBy(h => h.distance).ToArray();
        if (Input.anyKey || Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            inputState = keyListener();
            if (inputState[1] == 1) //state change object = true
                changeObject(actionMode, toolSelector);

            switch (inputState[0])
            {
                case 1:
                    operateWithObj(0); // pressed LKM
                    break;
                case 2:
                    operateWithObj(1); // pressed RKM
                    break;
                case 3:
                    operateWithObj(2); // released LKM
                    break;
                case 4:
                    operateWithObj(3); // released RKM
                    break;
            }
        }

        /* String objects = "";
         foreach (RaycastHit hit in hits)
             objects += hit.transform.name + "  ";
         Debug.Log(objects + "  " + hits.Length); //printing all objects on the Ray and length of array of them*/

        switch (actionMode)
        {
            case 1:
                if (hits.Length > 0 && (Math.Abs(hits[0].normal.x) == 1 || Math.Abs(hits[0].normal.y) == 1 || Math.Abs(hits[0].normal.z) == 1) && /*not very nice*/(hits[0].transform.tag == "Ground" || hits[0].transform.tag == "Buildable"))
                {
                    if (operateObj == null)
                        changeObject(actionMode, toolSelector);
                    Debug.DrawLine(transform.position, hits[0].point, Color.blue);
                    operateObj.SendMessage("prepareToOperate", hits[0]);
                }
                else
                    Destroy(operateObj);

                break;

                /*case 2:
                    operateObj.SendMessage("prepareToOperate", hits[0]);
                    break;                 // maybe will use...*/

                /*case 3:
                    hits[0].transform.SendMessage("prepareToOperate", hits[0]);
                    break;                 //will used when we create builded operateble objects */
        }
    }

    byte[] keyListener()  //return byte array with states of input sensors
    {
        byte[] state = new byte[3];

        for (int i = 0; i < state.Length; i++)
            state[i] = 0;
        if (!Input.GetKey(KeyCode.KeypadEnter))
        {
            if (operateObj != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    state[0] = 1;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    state[0] = 2;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    state[0] = 3;
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    state[0] = 4;
                }
            }

            int tmpMode = actionMode, tmpSelect = toolSelector;
            if (Input.GetKeyDown(KeyCode.Alpha1))
                actionMode = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                actionMode = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                actionMode = 3;

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
                toolSelector++;
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
                toolSelector--;
            state[1] = (byte)((tmpMode != actionMode || tmpSelect != toolSelector) ? 1 : 0);

        }
        else if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            castle.SendMessage("saveToFile");
            state[2] = 1;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            castle.SendMessage("loadFromFile");
            state[2] = 2;
        }

        return state;
    }

    private void changeObject(int mode, int select)
    {
        if (operateObj != null)
            Destroy(operateObj);
        switch (mode)
        {
            case 1:
                if (hits.Length > 0 && /*not very nice*/(hits[0].transform.tag == "Ground" || hits[0].transform.tag == "Buildable"))
                {
                    select = Math.Abs(select % block.Length);
                    operateObj = Instantiate(block[select], hits[0].point, Quaternion.identity);
                    operateObj.name = block[select].name;
                    operateObj.SendMessage("setCastle", castle.GetComponent<Castle>());
                }
                break;

            case 2:
                select = Math.Abs(select % handTool.Length);
                operateObj = Instantiate(handTool[select], transform);
                operateObj.name = handTool[select].name;
                operateObj.transform.localPosition = handTool[select].transform.position;
                break;

            case 3:
                break;
        }
    }


    private void operateWithObj(int button)
    {
        if (actionMode < 3)
        {
            operateObj.SendMessage("operate", button);
        }
        /*else
        hits[0].transform.SendMessage("operate"); //will used when we create builded operateble objects */
    }

    /*void InputListener.onKeyDown(KeyCode key)
    {

        string name = null;

        if (controlsConfig != null)
        {
            foreach (InputAction i in controlsConfig)
                /*foreach*/
                /*if (Helper.ListEquals<KeyCode>(i.combination, InputSpeaker.keyboard))
                    name = i.name;
        }
        else return;

        if (name == null)
            return;

        //Debug.Log(name);

        switch (name)
        {
            case "castle_save":
                break;
            case "castle_load":
                break;
            case "operate":
                break;
            case "aiming":
                break;
            case "actionMode1":
                break;
            case "actionMode2":
                break;
            case "actionMode3":
                break;
        }
    }

    void InputListener.onKeyUp(KeyCode key)
    {
    }

    void InputListener.onMouseScroll(float delta)
    {
    }

    string InputListener.getName()
    {
        return "player";
    }*/
}

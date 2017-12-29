using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Castle : MonoBehaviour
{
    public List<Builded> castleBlocks;
    public float cellSize;
    private string filePath;

    void Awake()
    {
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
            createChildrenHandler(obj);
    }

    public void Start()
    {
        filePath = Application.streamingAssetsPath + "/castle.json";
        castleBlocks = new List<Builded>();
    }

    public void createChildrenHandler(GameObject obj)
    {
        //Debug.Log(obj.transform.localScale.ToString() + "   " + obj.name + "   " + obj.transform.tag);
        if ((obj.transform.tag == "Ground" || obj.transform.tag == "Buildable") && obj.transform.localScale != new Vector3(1, 1, 1))
        {
            //Debug.Log(obj.name);
            GameObject childrenHandler = new GameObject();
            childrenHandler.name = "ChildrenHandler";
            childrenHandler.transform.position = obj.transform.position;
            childrenHandler.transform.SetParent(obj.transform);
        }
    }

    public Vector3 buildOnTheGrowndCoord(Vector3 pos)
    {
        return new Vector3(Mathf.RoundToInt((pos.x - transform.position.x) / cellSize), 0, Mathf.RoundToInt((pos.z - transform.position.z) / cellSize));
    }

    public Vector3 getPosByElement(Vector3 element)
    {
        return transform.position + element * cellSize + new Vector3(0, cellSize / 2, 0);
    }

    private void saveToFile()
    {
        List<WritebleView> writebleCastle = new List<WritebleView>();
        foreach (Builded block in castleBlocks)
        {
            writebleCastle.Add(block.getWritebleView());
        }

        WritebleContainer container = new WritebleContainer(writebleCastle);    

        string toJson = JsonUtility.ToJson(container, true);

        //Debug.Log(toJson);

        File.WriteAllText(filePath, toJson);

        //Debug.Log("Castle saved!");
    }

    private void loadFromFile()
    {
        if (File.Exists(filePath))
        {
            foreach (Builded block in castleBlocks)
            {
                block.SendMessage("notAPIOnDestroy", false);
                Destroy(block.gameObject);
            }
            castleBlocks = new List<Builded>();

            WritebleContainer container = JsonUtility.FromJson<WritebleContainer>(File.ReadAllText(filePath));
            GameObject tmp;

            foreach (WritebleView view in container.writebleCastle)
            {
                tmp = Instantiate(findBlockByName(view.name), getPosByElement(view.inCastlePos), Quaternion.identity);
                tmp.name = view.name;
                tmp.GetComponent<Cursor>().enabled = false;
                tmp.GetComponent<Builded>().enabled = true;
                tmp.GetComponent<Builded>().setInCastlePos(view.inCastlePos);
                tmp.transform.tag = "Buildable";
                createChildrenHandler(tmp);
                castleBlocks.Add(tmp.GetComponent<Builded>());
                tmp.transform.SetParent(transform);
            }

            //Debug.Log("Castle loaded!");
        }
        else
            Debug.LogAssertion("File does not exist!");
    }

    private GameObject findBlockByName(string name)
    {
        return (GameObject)Resources.Load("prefabs/blocks/" + name, typeof(GameObject));
    }

    private void Update()
    {
    }

    /// <summary>
    /// recursion for find block that need to fall
    /// </summary>
    /// <param name="block"></param>
    void checkForBuildClouds(Builded block)
    {
        Vector3[] directions = new Vector3[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
        List<Builded> cloud; //saving 6 clouds around destroyed cube to this iterated
        Builded tmp;

        castleBlocks.Remove(block);  //removing destroed block

        for (int i = 0; i < 6; i++)
        {
            tmp = findByCastlePos(block.getInCastlePos() + directions[i]);
            if (block.canBuildOn[i] && tmp != null && tmp.canBuildOn[block.getOppositeSide(i)])  //check tmp is exist and this block and tmp are coupling
            {
                cloud = new List<Builded>();
                if (!isBranchOnTheGrownd(findByCastlePos(block.getInCastlePos() + directions[i]), cloud, directions))
                    letThisCloudDown(cloud);
            }
        }
    }

    /// <summary>
    /// while check all block in cloud or find block on the ground
    /// if block on the ground return true for all previos function
    /// else go to next block
    /// </summary>
    /// <param name="block"></param>
    /// <param name="branch"></param>
    /// <param name="directions"></param>
    /// <returns></returns>
    bool isBranchOnTheGrownd(Builded block, List<Builded> branch, Vector3[] directions)  // recoursive blocks-clouds detecting
    {
        bool isOnTheGrownd = false;
        Builded tmp;

        //block.GetComponent<Renderer>().material.color = Color.black; //function was-here visualization
        branch.Add(block);

        if (block.getInCastlePos().y == 0)
            return true;  //this block is on the ground 
        else
            for (int i = 0; i < 6; i++)
            {
                tmp = findByCastlePos(block.getInCastlePos() + directions[i]);
                if (block.canBuildOn[i] && tmp != null && tmp.canBuildOn[block.getOppositeSide(i)] && findByCastlePos(tmp.getInCastlePos(), branch) == null)  //check tmp is exist, this block and tmp are coupling, and tmp was not checked yet
                {
                    isOnTheGrownd = (isBranchOnTheGrownd(tmp, branch, directions)); //block is on the ground if one of his branches toching the ground
                    tmp = null;
                }
            }
        return isOnTheGrownd;
    }

    /// <summary>
    /// drop cloud
    /// </summary>
    /// <param name="cloud"></param>
    private void letThisCloudDown(List<Builded> cloud)
    {
        foreach (Builded block in cloud)
        {
            block.SendMessage("setFallen");
            castleBlocks.Remove(block);
        }
    }

    /// <summary>
    /// find block in castle by position 
    /// </summary>
    /// <param name="inCastlePos"></param>
    /// <returns></returns>
    Builded findByCastlePos(Vector3 inCastlePos)
    {
        foreach (Builded block in castleBlocks)
        {
            if ((block.getInCastlePos() - inCastlePos).magnitude < 0.01)
            {
                return block;
            }
        }
        return null;
    }

    /// <summary>
    /// find block in list of block(used for branch) by position
    /// </summary>
    /// <param name="inCastlePos"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    Builded findByCastlePos(Vector3 inCastlePos, List<Builded> list)
    {
        foreach (Builded block in list)
        {
            if ((block.getInCastlePos() - inCastlePos).magnitude < 0.01)
            {
                return block;
            }
        }
        return null;
    }

}

public struct WritebleContainer
{
    public List<WritebleView> writebleCastle;

    public WritebleContainer(List<WritebleView> list)
    {
        writebleCastle = list;
    }
}
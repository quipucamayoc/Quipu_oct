using UnityEngine;
using UnityEngine.Networking;
//using Windows.Kinect;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

public class CharacterManager : MonoBehaviour
{
    
    //created player prefab
    public GameObject playerPrefab;
    public int numCharacters = 0;
    
    private List<GameObject> characters;
    
    private static CharacterManager instance = null;
    public static CharacterManager Instance
    {
        get
        {
            return instance;
        }
    }

    public List<Int64> userIDs;
    
    // Use this for initialization
    void Start()
    {
        transform.position = new Vector3(-1.86f, 2.5f, 3.3f);
        numCharacters = 0;
        characters = new List<GameObject>();
        instance = this;
    }
    
    // Update is called once per frame
    void Update()
    {
        KinectManager manager = KinectManager.Instance;
        //check is new users have entered scene
        for (int i = 0; i < manager.GetUsersCount(); i++)
        {
            Int64 id = manager.GetUserIdByIndex(i);
            if ( !userIDs.Contains(id) )
            {
                SpawnAvatar(id);
                userIDs.Add(id);
                numCharacters++;
            }
        }
        //check if users have left scene
        if (numCharacters > manager.GetUsersCount())
        {
            foreach (Int64 id in userIDs)
            {
                if (manager.GetUserIndexById(id) == -1)
                {
                    DestoryAvatar(FindAvatarByID(id));
                    numCharacters--;
                    return;
                }
            }
        }

       
        
    }
    GameObject FindAvatarByID(Int64 id)
    {
        foreach(Transform child in gameObject.transform)
        {
            if( id == child.gameObject.GetComponent<LightAvatarController>().UserID)
            {
                return child.gameObject;
            }
        }
        return characters[0];
    }

    void SpawnAvatar(Int64 UserID)
    {
        GameObject character = Instantiate<GameObject>(playerPrefab);
        character.transform.position = KinectManager.Instance.GetUserPosition(UserID);
        character.GetComponent<LightAvatarController>().UserID = UserID;
        character.transform.parent = gameObject.transform;
        NetworkServer.Spawn(character);
        //foreach (Transform child in character.transform)
        //{
        //    NetworkServer.Spawn(child.gameObject);
        //}
        characters.Add(character);
    }

    void DestoryAvatar(GameObject character)
    {
        userIDs.Remove(character.GetComponent<LightAvatarController>().UserID);
        Destroy(character);
    }
}


﻿using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;
using UnityEngine.Networking;

public class LightAvatarController : MonoBehaviour {
    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("Whether the cubeman is allowed to move vertically or not.")]
    public bool verticalMovement = true;

    [Tooltip("Whether the cubeman is facing the player or not.")]
    public bool mirroredMovement = false;

    [Tooltip("Rate at which the cubeman will move through the scene.")]
    public float moveRate = 1f;

    //public GameObject debugText;

    public GameObject Hip_Center;
    public GameObject Spine;
    public GameObject Neck;
    public GameObject Head;
    public GameObject Shoulder_Left;
    public GameObject Elbow_Left;
    public GameObject Wrist_Left;
    public GameObject Hand_Left;
    public GameObject Shoulder_Right;
    public GameObject Elbow_Right;
    public GameObject Wrist_Right;
    public GameObject Hand_Right;
    public GameObject Hip_Left;
    public GameObject Knee_Left;
    public GameObject Ankle_Left;
    public GameObject Foot_Left;
    public GameObject Hip_Right;
    public GameObject Knee_Right;
    public GameObject Ankle_Right;
    public GameObject Foot_Right;

    public LineRenderer skeletonLine;
    public LineRenderer debugLine;

    private GameObject[] bones;
    private LineRenderer[] lines;

    private LineRenderer lineTLeft;
    private LineRenderer lineTRight;
    private LineRenderer lineFLeft;
    private LineRenderer lineFRight;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialPosOffset = Vector3.zero;
    private Int64 initialPosUserID = 0;

    public Int64 UserID;


    void Start()
    {
        
        //store bones in a list for easier access
        bones = new GameObject[] {
            Hip_Center,
            Spine,
            Neck,
            Head,
            Shoulder_Left,
            Elbow_Left,
            Wrist_Left,
            Hand_Left,
            Shoulder_Right,
            Elbow_Right,
            Wrist_Right,
            Hand_Right,
            Hip_Left,
            Knee_Left,
            Ankle_Left,
            Foot_Left,
            Hip_Right,
            Knee_Right,
            Ankle_Right,
            Foot_Right,
            
        };

        foreach (GameObject bone in bones)
        {
            //bone.AddComponent<NetworkIdentity>();
            //bone.GetComponent<NetworkIdentity>().isLocalPlayer = true;
            //bone.AddComponent<NetworkTransform>();
            NetworkServer.Spawn(bone);
        }

        // array holding the skeleton lines
        lines = new LineRenderer[bones.Length];

        if (skeletonLine)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if ((i == 22 || i == 24) && debugLine)
                    lines[i] = Instantiate(debugLine) as LineRenderer;
                else
                    lines[i] = Instantiate(skeletonLine) as LineRenderer;

                lines[i].transform.parent = transform;
            }
        }

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        //transform.rotation = Quaternion.identity;
    }


    void Update()
    {
        KinectManager manager = KinectManager.Instance;
        

        if (UserID <= 0)
        {
            transform.position = manager.GetUserPosition(UserID);
            // reset the pointman position and rotation
            if (transform.position != initialPosition)
            {
                transform.position = initialPosition;
            }

            if (transform.rotation != initialRotation)
            {
                transform.rotation = initialRotation;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].gameObject.SetActive(true);

                //bones[i].transform.localPosition = Vector3.zero;
                //bones[i].transform.localRotation = Quaternion.identity;

                if (skeletonLine)
                {
                    lines[i].gameObject.SetActive(false);
                }
            }

            return;
        }

        // set the position in space
        Vector3 posPointMan = manager.GetUserPosition(UserID);
        posPointMan.z = !mirroredMovement ? -posPointMan.z : posPointMan.z;

        // store the initial position
        if (initialPosUserID != UserID)
        {
            initialPosUserID = UserID;
            initialPosOffset = transform.position - (verticalMovement ? posPointMan * moveRate : new Vector3(posPointMan.x, 0, posPointMan.z) * moveRate);
        }

        transform.position = initialPosOffset +
            (verticalMovement ? posPointMan * moveRate : new Vector3(posPointMan.x, 0, posPointMan.z) * moveRate);

        // update the local positions of the bones
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                int joint = !mirroredMovement ? i : (int)KinectInterop.GetMirrorJoint((KinectInterop.JointType)i);
                if (joint < 0)
                    continue;

                if (manager.IsJointTracked(UserID, joint))
                {
                    bones[i].gameObject.SetActive(true);

                    Vector3 posJoint = manager.GetJointPosition(UserID, joint);
                    posJoint.z = !mirroredMovement ? -posJoint.z : posJoint.z;

                    Quaternion rotJoint = manager.GetJointOrientation(UserID, joint, !mirroredMovement);
                    rotJoint = initialRotation * rotJoint;

                    posJoint -= posPointMan;

                    if (mirroredMovement)
                    {
                        posJoint.x = -posJoint.x;
                        posJoint.z = -posJoint.z;
                    }

                    bones[i].transform.localPosition = posJoint;
                    bones[i].transform.rotation = rotJoint;

                    if (skeletonLine)
                    {
                        lines[i].gameObject.SetActive(true);
                        Vector3 posJoint2 = bones[i].transform.position;

                        Vector3 dirFromParent = manager.GetJointDirection(UserID, joint, false, false);
                        dirFromParent.z = !mirroredMovement ? -dirFromParent.z : dirFromParent.z;
                        Vector3 posParent = posJoint2 - dirFromParent;

                        //lines[i].SetVertexCount(2);
                        lines[i].SetPosition(0, posParent);
                        lines[i].SetPosition(1, posJoint2);
                    }

                }
                else
                {
                    bones[i].gameObject.SetActive(false);

                    if (skeletonLine)
                    {
                        lines[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }

}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;
using UnityEngine.UI;
using System.Linq;

[Serializable]
public class Patient : NetworkBehaviour
{
    public GameObject PlayerCamera;
    public int roomID; //Door ID is used to determine what room patient is in
                       //If roomID = 0, else they are in office
    public enum Cure { None, Bandaid, Stitches };
    public Cure cure;
    public int money;
    public int patientID;
    public string patientName;

    private Door door; //door that the player is in queue, null if not

    public Text health;
    public Text cured;
    public Text moneyText;

    //public localPlayer;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = "Local";
        if (isLocalPlayer)
        {
            PlayerCamera.SetActive(true);
        }
        else
        {
            PlayerCamera.SetActive(false);
        }
        door = null;
        cure = Cure.None;
        GlobalVariables.patientList.Add(this);
        patientID = GlobalVariables.patientList.Count;
        health.text = String.Concat("Health: :",GetRandomHealth().ToString());
        EnableDisableButtons(false);
        //money = GlobalVariables.patientMoney;
        //this.gameObject.GetComponent<Camera>().
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        moneyText.text = String.Concat("Money: ", money.ToString());
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        //gameObject.name = "Local";
    }

    public void NewDoor(Door newDoor)
    {
        if (newDoor == null)
        {
            //Edge case that should not occur
            CmdRemovePatientinQueue(this.GetInstanceID(), door.doorID);
        }
        if (!newDoor.active)
        {
            return;
        }
        if (door != null)
        {
            CmdRemovePatientinQueue(this.GetInstanceID(), door.doorID);
        }
        door = newDoor;
    }

    public Door GetDoor()
    {
        return door;
    }

    public bool IsInline()
    {
        return door != null;
    }

    private int GetRandomHealth()
    {
        System.Random random = new System.Random();
        int num = random.Next(0, 2);
        for (int i = 1; i < 10; i++)
        {
            int nextNumber = (int)random.Next(0, 10);
            num = (nextNumber > GlobalVariables.chanceOfOne) ? (int)(1 * Math.Pow(10, i)) + num : (int)(0 * Math.Pow(10, i) + num);
        }
        return num;
    }




    /// <summary>
    /// If patient is null or is cured, nothing happens
    /// </summary>
    /// <param name="instanceID"></param>
    [Command]
    public void CmdAddToQueue(int instanceID, int doorID)
    //public void AddToQueue(Patient patient)
    {
        Patient[] patients = FindObjectsOfType<Patient>();
        Patient patient = patients.AsEnumerable().First(x => x.GetInstanceID() == instanceID);
        Door localDoor = FindObjectsOfType<Door>().First(x => x.doorID == doorID);
        if (patient == null || patient.cure != Patient.Cure.None)
        {
            return;
        }
        if (patient.GetDoor() != null)
        {
            CmdRemovePatientinQueue(patient.GetInstanceID(), doorID);
        }
        localDoor.playerQueue.Add(patient);
        //this.playerQueue.Add(new Player(patient));
        patient.NewDoor(localDoor);
        patient.transform.position = new Vector3(
            localDoor.coordsToPlace.x,
            localDoor.coordsToPlace.y - (float)((localDoor.playerQueue.Count() - 1) * 100),
            patient.transform.position.z);
    }

    /// <summary>
    /// Pops first patient from queue
    /// </summary>
    /// <returns>patient in front of queue. If queue is empty, returns null</returns>
    [Command]
    public void CmdPopQueue(int doorID)
    //public Patient PopQueue()
    {
        Door localDoor = FindObjectsOfType<Door>().First(x => x.doorID == doorID);
        //if (this.playerQueue.First<Patient>() == null)
        //{
        //    return null;
        //}
        if (!localDoor.playerQueue.Any())
        {
            return;// null;
        }
        Patient popped = localDoor.playerQueue.First();
        //Patient popped = this.playerQueue.First<Player>().patient;
        localDoor.playerQueue.RemoveAt(0);
        if (localDoor.playerQueue.Any())
        {
            foreach (Patient patient in localDoor.playerQueue)
            {
                patient.transform.position = new Vector3(localDoor.coordsToPlace.x,
                    localDoor.coordsToPlace.y + 100,
                    patient.transform.position.z);
            }
            //foreach (Player patient in playerQueue)
            //{
            //    patient.patient.transform.position = new Vector3(coordsToPlace.x,
            //        coordsToPlace.y + 100,
            //        patient.patient.transform.position.z);
            //}
        }
        popped.NewDoor(null);
        popped.roomID = doorID;
        popped.transform.position = localDoor.officeCoords;
        EnableDisableButtons(true);
        //return popped;
    }

    /// <summary>
    /// Removes patient in the queue, irregardless of position
    /// </summary>
    [Command]
    public void CmdRemovePatientinQueue(int instanceID, int doorID)
    //public void RemovePatientinQueue(Patient patient)
    {
        Patient[] patients = FindObjectsOfType<Patient>();
        Patient patient = patients.AsEnumerable().First(x => x.GetInstanceID() == instanceID);
        Door localDoor = FindObjectsOfType<Door>().First(x => x.doorID == doorID);
        if (localDoor.playerQueue.Contains(patient))
        {
            localDoor.playerQueue.Remove(patient);
        }
        //if (this.playerQueue.Contains(playerQueue.Where(x => x.patient == patient)))
        //{
        //    this.playerQueue.Remove(playerQueue.First(x => x.patient == patient));
        //}
    }

    /// <summary>
    /// Makes patient buttons active or inactive
    /// </summary>
    /// <param name="b">true: active, false: inactive</param>
    public static void EnableDisableButtons(bool b)
    {
        if (GlobalVariables.buttons.Count == 0)
        {
            GlobalVariables.buttons.AddRange(GameObject.FindGameObjectsWithTag("PatientButton"));
        }
        foreach (GameObject button in GlobalVariables.buttons)
        {
            button.SetActive(b);
        }
    }
}

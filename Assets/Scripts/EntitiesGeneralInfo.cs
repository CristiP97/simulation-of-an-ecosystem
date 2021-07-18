using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitiesGeneralInfo : MonoBehaviour
{

    public float refreshTime;
    public GameObject bunnyHolder;
    public GameObject wolfHolder;

    [SerializeField]
    public static Vector2Int averageFemaleBunnyPosition;
    public static Vector2Int averageFemaleWolfPosition;
    
    private float remainingTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        } else
        {
            remainingTime = refreshTime;
            UpdateInformation();
        }
    }

    private void UpdateInformation()
    {
        Vector2Int femaleBunnyPosition = new Vector2Int();
        Vector2Int femaleWolfPosition = new Vector2Int();
        int femaleBunnies = 0;
        int femaleWolves = 0;


        foreach (Transform child in bunnyHolder.GetComponentInChildren<Transform>())
        {
            if (child != bunnyHolder.transform)
            {
                if (child.GetComponent<Animals>().GetGender() == 0)
                {
                    femaleBunnyPosition += child.GetComponent<Animals>().GetMapPosition();
                    femaleBunnies++;
                }
            }
        }

        if (femaleBunnies != 0)
        {
            femaleBunnyPosition /= femaleBunnies;
            averageFemaleBunnyPosition = femaleBunnyPosition;
        }
        else
        {
            averageFemaleBunnyPosition = new Vector2Int();
        }

        foreach (Transform child in wolfHolder.GetComponentInChildren<Transform>())
        {
            if (child != wolfHolder.transform)
            {
                if (child.GetComponent<Animals>().GetGender() == 0)
                { 
                    femaleWolfPosition += child.GetComponent<Animals>().GetMapPosition();
                    femaleWolves++;
                }
            }
        }

        if (femaleWolves != 0)
        {
            femaleWolfPosition /= femaleWolves;
            averageFemaleWolfPosition = femaleWolfPosition;
        } else
        {
            averageFemaleWolfPosition = new Vector2Int();
        }

        Debug.Log(averageFemaleBunnyPosition);
        Debug.Log(averageFemaleWolfPosition);
    }
}

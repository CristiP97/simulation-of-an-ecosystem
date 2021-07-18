using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public float nutrition;
    public float nutritionRegen;
    bool eaten;

    private Vector2Int mapPosition;
    private float size;
    private float remainingNutrition;
    private bool setMapPosition;
    private float localScale;
    private bool depleted;
    private Test mapScript;


    public void Start()
    {
        eaten = false;
        remainingNutrition = nutrition;
        setMapPosition = false;
        mapScript = Test.instance;

        // It should be the same on all axis
        localScale = gameObject.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (depleted)
        {
            mapScript.SpawnNewFood(mapPosition);
            Destroy(gameObject);
            return;
        }


        if (!eaten)
        {
            if (remainingNutrition < nutrition)
                Regenerate();
        }

        eaten = false;
    }

    public float Consume(float rate)
    {
        eaten = true;
        remainingNutrition -= rate * Time.deltaTime;

        // Rescale
        size = remainingNutrition / nutrition;
        size *= localScale;

        if (size < 0)
        {
            depleted = true;
            size = 0;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            return 0;
        }

        transform.localScale = new Vector3(size, size, size);

        return rate * Time.deltaTime;
    }

    private void Regenerate ()
    {
        remainingNutrition += nutritionRegen * Time.deltaTime;
        
        if (remainingNutrition > nutrition)
        {
            remainingNutrition = nutrition;
        }

        size = remainingNutrition / nutrition;
        size *= localScale;

        transform.localScale = new Vector3(size, size, size);
    }

    public void SetMapPosition(Vector2Int coordinates)
    {
        if (!setMapPosition)
        {
            setMapPosition = true;
            mapPosition.x = coordinates.x;
            mapPosition.y = coordinates.y;

            Debug.Log("My position on the map is: " + mapPosition);
        }
    }

    public void Detected()
    {
        Debug.Log("A bunny has detected me at position: " + transform.position);
    }

    public Vector2Int GetMapPosition()
    {
        return mapPosition;
    }

    public bool IsDepleted()
    {
        return depleted;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnySearchFood : MonoBehaviour
{
    private List<Vector2Int> memory;
    private Vector2Int targetMapCoord;
    private Food foodScript;

    private Bunny bunnyScript;
    private Test mapScript;

    private Vector2Int result;

    // Start is called before the first frame update
    void Start()
    {
        bunnyScript = gameObject.GetComponent<Bunny>();
        mapScript = Test.instance;

        bunnyScript.SetSearchFoodStatus(false);
        memory = new List<Vector2Int>();
    }

    private void Update()
    {
        if (bunnyScript.GetPriority() == 2 && !bunnyScript.IsMoving())
        {
            SearchForFood();
        }
    }

    void SearchForFood()
    {
        if (!bunnyScript.IsSearchingFood())
        {
            // Clear data if we were doing anything else before
            if (bunnyScript.IsSearchingForWater())
            {
                bunnyScript.SetSearchingWaterStatus(false);
                memory.Clear();
            }

            if (bunnyScript.IsSearchingForPartner())
            {
                bunnyScript.SetSearchingPartnerStatus(false);
                memory.Clear();
            }

            // Check for food sources in our vision radius
            List<Vector2Int> foodLocations = new List<Vector2Int>();
            List<Food> foodScripts = new List<Food>();

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, bunnyScript.GetSightRadius());
            foreach (var hitCollider in hitColliders)
            {
                // Food layer is 8
                if (hitCollider.gameObject.layer == 8)
                {
                    foodScripts.Add(hitCollider.gameObject.GetComponent<Food>());
                    foodLocations.Add(hitCollider.gameObject.GetComponent<Food>().GetMapPosition());
                }
            }

            // If there are multiple options, choose the closest one
            if (foodLocations.Count > 1)
            {
                int index;
                targetMapCoord = Utils.GetClosestInterestItem(foodLocations, out index, transform.position);
                foodScript = foodScripts[index];
                bunnyScript.SetSearchFoodStatus(true);
            }
            else if (foodLocations.Count == 1)
            {
                targetMapCoord = foodLocations[0];
                foodScript = foodScripts[0];
                bunnyScript.SetSearchFoodStatus(true);

            }
            else
            {
                result = Utils.SearchTiles(memory,
                                           bunnyScript.GetMapPosition(), 
                                           transform, 
                                           bunnyScript.GetMemorySize(),
                                           bunnyScript.GetSearchAngle()
                                          );
                StartCoroutine(bunnyScript.Hop(result));
                bunnyScript.SetMapPosition(result);
            }
        }
        else
        {
            if (!bunnyScript.IsMoving())
            {
                if (foodScript != null && !foodScript.IsDepleted())
                {
                    List<int> nextDest = Utils.GoToInterestItem(targetMapCoord, bunnyScript.GetMapPosition());
                    if (nextDest.Count == 1)
                    {
                        StartCoroutine(Eat());
                        memory.Clear();
                    } else
                    {
                        StartCoroutine(bunnyScript.Hop(new Vector2Int(nextDest[0], nextDest[1])));
                        bunnyScript.SetMapPosition(new Vector2Int(nextDest[0], nextDest[1]));
                    }
                }
            }
        }
    }

    IEnumerator Eat()
    {
        bunnyScript.SetEatingStatus(true);

        // Don't forget to reset their focus so they can change the task
        bunnyScript.ResetFocus();

        if (foodScript == null)
        {
            bunnyScript.SetEatingStatus(false);
            bunnyScript.SetSearchFoodStatus(false);

            yield break;
        }


        // Rotate the target first
        Vector3 foodPosition = foodScript.gameObject.transform.position;
        Vector3 dir = foodPosition - transform.position;
        dir = dir.normalized;

        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float angle;

        while (true)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05)
            {
                angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, bunnyScript.GetTurnSpeed() * Time.deltaTime);
                transform.eulerAngles = Vector3.up * angle;
                yield return null;
            }
            else
            {
                break;
            }
        }

        // Eat until you are full
        while (bunnyScript.GetCurrentHunger() < bunnyScript.GetMaxHunger())
        {
            if (foodScript == null || foodScript.IsDepleted() || bunnyScript.IsGivingBirth() || bunnyScript.IsHunted())
            {
                break;
            }

            bunnyScript.AddCurrentHunger(foodScript.Consume(bunnyScript.GetConsumptionRate()));

            if (bunnyScript.GetCriticalThirstPercent() < bunnyScript.GetCriticalThirstPercent())
            {
                if (bunnyScript.GetHungerPercent() >= bunnyScript.GetCriticalHungerPercent() * 2)
                {
                    break;
                }
            }

            yield return null;
        }

        // Set hunger in case it overflows
        if ((bunnyScript.GetCurrentHunger() > bunnyScript.GetMaxHunger()))
        {
            bunnyScript.SetCurrentHunger(bunnyScript.GetMaxHunger());
        }

        foodScript = null;
        bunnyScript.SetEatingStatus(false);
        bunnyScript.SetSearchFoodStatus(false);
    }
}

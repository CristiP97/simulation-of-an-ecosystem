using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Wolf))]
public class WolfSearchFood : MonoBehaviour
{
    private List<Vector2Int> memory;
    private Vector2Int targetMapCoord;
    //private Food foodScript;

    private Wolf wolfScript;
    private Bunny currentTarget;
    private Test mapScript;
    private float timeToEat = 10;
    private int persistance = 25; // how many tiles is the wolf willing to follow a target
    private float breakTime = 3;
    private int currentPersistance;
    private float remainingBreak;


    private Vector2Int result;

    // Start is called before the first frame update
    void Start()
    {
        wolfScript = gameObject.GetComponent<Wolf>();
        mapScript = Test.instance;

        wolfScript.SetSearchFoodStatus(false);
        memory = new List<Vector2Int>();
        remainingBreak = 0;
    }

    private void Update()
    {
        if (wolfScript.GetPriority() == 2 && !wolfScript.IsMoving() && remainingBreak <= 0)
        {
           SearchForFood();
        } else if (wolfScript.GetPriority() != 2 && !wolfScript.IsEating())
        {
            if (currentTarget != null)
            {
                wolfScript.SetSearchFoodStatus(false);
                currentTarget.SetHuntedStatus(false, null);
                currentTarget = null;
                currentPersistance = persistance;
            }
        }

        if (remainingBreak > 0)
        {
            remainingBreak -= Time.deltaTime;
        }
    }

    void SearchForFood()
    {
        if (!wolfScript.IsSearchingFood())
        {
            // Clear data if we were doing anything else before
            if (wolfScript.IsSearchingForWater())
            {
                wolfScript.SetSearchingWaterStatus(false);
                memory.Clear();
            }

            if (wolfScript.IsSearchingForPartner())
            {
                wolfScript.SetSearchingPartnerStatus(false);
                memory.Clear();
            }

            // Check for food sources in our vision radius
            List<Bunny> foodLocations = new List<Bunny>();

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, wolfScript.GetSightRadius());
            foreach (var hitCollider in hitColliders)
            {
                // Food layer is 8
                if (hitCollider.gameObject.layer == 10 && hitCollider.gameObject.CompareTag("Bunny"))
                {
                    if (IsValidHuntingTarget(hitCollider.gameObject.GetComponent<Bunny>()))
                    {
                        foodLocations.Add(hitCollider.gameObject.GetComponent<Bunny>());
                    }
                }
            }

            // If there are multiple options, choose at random
            if (foodLocations.Count > 1)
            {
                currentTarget = foodLocations[Random.Range(0, foodLocations.Count)];
                wolfScript.SetSearchFoodStatus(true);
                currentTarget.SetHuntedStatus(true, wolfScript);
                currentPersistance = persistance;
            }
            else if (foodLocations.Count == 1)
            {
                currentTarget = foodLocations[0];
                wolfScript.SetSearchFoodStatus(true);
                currentTarget.SetHuntedStatus(true, wolfScript);
                currentPersistance = persistance;

            }
            else
            {
                result = Utils.SearchTiles(memory,
                                           wolfScript.GetMapPosition(),
                                           transform,
                                           wolfScript.GetMemorySize(),
                                           wolfScript.GetSearchAngle()
                                          );
                StartCoroutine(wolfScript.Run(result));
                wolfScript.SetMapPosition(result);
            }
        }
        else
        {
            if (!wolfScript.IsMoving())
            {
                if (currentTarget != null)
                {
                    // Hunt him down!
                    if (persistance > 0)
                    {
                        List<int> nextDest = Utils.GoToInterestItem(currentTarget.GetMapPosition(), wolfScript.GetMapPosition());
                        if (nextDest.Count == 1)
                        {
                            currentTarget.Die(timeToEat);
                            StartCoroutine(Eat(currentTarget.GetHungerRefill()));
                            Debug.Log("Ate the bunny! YUM");
                            memory.Clear();
                        }
                        else
                        {
                            StartCoroutine(wolfScript.Run(new Vector2Int(nextDest[0], nextDest[1])));
                            wolfScript.SetMapPosition(new Vector2Int(nextDest[0], nextDest[1]));
                            currentPersistance--;
                        }
                    } else
                    {
                        // Give up and take a breather
                        currentTarget.SetHuntedStatus(false, null);
                        currentTarget = null;
                        wolfScript.SetSearchFoodStatus(false);
                        remainingBreak = breakTime;
                    }
                } else
                {
                    wolfScript.SetSearchFoodStatus(false);
                }
            }
        }
    }

    // Make sure that 2 wolves don't hunt the same bunny
    private bool IsValidHuntingTarget(Bunny bunny)
    {
        if (bunny.IsHunted())
        {
            return false;
        }

        return true;
    }

    IEnumerator Eat(float amount)
    {
        wolfScript.SetEatingStatus(true);
        wolfScript.SetSearchFoodStatus(false);
        wolfScript.ResetFocus();

        float eatingTime = timeToEat * currentTarget.GetDevelopmentPercent();
        float timePassed = 0;

        while (currentTarget.IsMoving())
        {
            yield return StartCoroutine(Rotate(currentTarget.transform.position - transform.position));
        }

        yield return StartCoroutine(Rotate(currentTarget.transform.position - transform.position));

        while (!currentTarget.HasFallenOver())
        {
            yield return null;
        }


        // TODO: Maybe make the eating more realistic
        // TODO: Do similar to the bunny eating plants, that they substract the food from the plant
        while (timePassed < eatingTime)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }

        wolfScript.AddCurrentHunger(amount);
        wolfScript.CheckFoodOverflow();


        currentTarget = null;
        wolfScript.SetEatingStatus(false);
    }

    IEnumerator Rotate(Vector3 dir)
    {
        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float angle;

        while (true)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05)
            {
                angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, wolfScript.GetTurnSpeed() * Time.deltaTime);
                transform.eulerAngles = Vector3.up * angle;
                yield return null;
            }
            else
            {
                break;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSearchWater : MonoBehaviour
{
    private Animals animalScript;
    private Vector2Int targetMapCoord;
    private List<Vector2Int> memory;
    private Vector2Int result;
    private Test mapScript;


    // Start is called before the first frame update
    void Start()
    {
        mapScript = Test.instance;

        animalScript = gameObject.GetComponent<Bunny>();
        if (animalScript == null)
        {
            animalScript = gameObject.GetComponent<Wolf>();
        }

        memory = new List<Vector2Int>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animalScript.GetPriority() == 1 && !animalScript.IsMoving() && !animalScript.IsDrinking())
        {
            SearchForWater();
        }
    }

    // Looks around for a water source
    private void SearchForWater()
    {
        if (!animalScript.IsSearchingForWater())
        {
            // Clear data if we were doing anything else before
            if (animalScript.IsSearchingFood())
            {
                animalScript.SetSearchFoodStatus(false);
                memory.Clear();
            }

            if (animalScript.IsSearchingForPartner())
            {
                animalScript.SetSearchingPartnerStatus(false);
                memory.Clear();
            }

            // Check for water sources in our vision radius
            List<Vector2Int> waterLocations = new List<Vector2Int>();
            List<Vector3> realWaterLocations = new List<Vector3>();

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, animalScript.GetSightRadius());
            foreach (var hitCollider in hitColliders)
            {
                // Food layer is for terrain
                if (hitCollider.gameObject.layer == 9 && hitCollider.gameObject.tag == "Water")
                {
                    waterLocations.Add(hitCollider.gameObject.GetComponent<WaterTile>().GetMapPosition());
                    realWaterLocations.Add(hitCollider.gameObject.transform.position);
                }
            }

            if (waterLocations.Count > 1)
            {
                int index;
                targetMapCoord = Utils.GetClosestInterestItem(waterLocations, out index, transform.position);
                animalScript.SetSearchingWaterStatus(true);
            }
            else if (waterLocations.Count == 1)
            {
                targetMapCoord = waterLocations[0];
                animalScript.SetSearchingWaterStatus(true);
            }
            else
            {
                result = Utils.SearchTiles(memory,
                                          animalScript.GetMapPosition(),
                                          transform,
                                          animalScript.GetMemorySize(),
                                          animalScript.GetSearchAngle()
                                         );
                if (animalScript is Bunny)
                {
                    StartCoroutine(((Bunny)animalScript).Hop(result));
                } else
                {
                    StartCoroutine(((Wolf)animalScript).Run(result));
                }
                animalScript.SetMapPosition(result);
            }
        }
        else
        {
            if (!animalScript.IsMoving())
            {
                List<int> nextDest = Utils.GoToInterestItem(targetMapCoord, animalScript.GetMapPosition());
                if (nextDest.Count == 1)
                {
                    StartCoroutine(Drink());
                    memory.Clear();
                }
                else
                {
                    Vector2Int newLocation = new Vector2Int(nextDest[0], nextDest[1]);
                    if (animalScript is Bunny)
                    {
                        StartCoroutine(((Bunny)animalScript).Hop(newLocation));
                    }
                    else
                    {
                        StartCoroutine(((Wolf)animalScript).Run(newLocation));
                    }
                    animalScript.SetMapPosition(newLocation);
                }
            }
        }
    }

    IEnumerator Drink()
    {
        animalScript.SetDrinkingStatus(true);

        // The bunny can look for new tasks after this, so we can reset his focus
        animalScript.ResetFocus();

        // Rotate the target first
        Vector3 waterTarget = mapScript.ConvertMapPositionToWorldPosition(targetMapCoord);
        Vector3 dir = waterTarget - transform.position;
        dir = dir.normalized;

        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float angle;

        while (true)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05)
            {
                angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, animalScript.GetTurnSpeed() * Time.deltaTime);
                transform.eulerAngles = Vector3.up * angle;
                yield return null;
            }
            else
            {
                break;
            }
        }

        // Drink until you are full
        while (animalScript.GetCurrentThirst() < animalScript.GetMaxThirst())
        {
            if (animalScript is Bunny)
            {
                if (((Bunny)animalScript).IsHunted())
                {
                    break;
                }
            }

            if (animalScript.IsGivingBirth())
            {
                break;
            }

            animalScript.AddThirstAmount(animalScript.GetDrinkRate() * Time.deltaTime);

            if (animalScript.GetHungerPercent() <= animalScript.GetCriticalHungerPercent())
            {
                if (animalScript.GetThirstPercent() >= animalScript.GetCriticalThirstPercent() * 2)
                {
                    break;
                }
            }
            yield return null;
        }

        // Set thirst in case it overflows
        if (animalScript.GetCurrentThirst() > animalScript.GetMaxThirst())
        {
            animalScript.SetThirstAmount(animalScript.GetMaxThirst());
        }

        animalScript.SetSearchingWaterStatus(false);
        animalScript.SetDrinkingStatus(false);
    }
}

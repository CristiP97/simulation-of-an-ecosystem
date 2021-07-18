using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WolfSearchFood))]
[RequireComponent(typeof(AnimalSearchWater))]
[RequireComponent(typeof(AnimalsSearchPartner))]
public class Wolf : Animals
{
    public float advancedSearchAngle;

    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private GeneticsLab.GeneticInfo wolfInfo;
    private GeneticsLab.GeneticInfo partnerInfo;


    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        currentFocus = 0;

        moving = false;
        if (!setDevelopment)
        {
            developmentPercent = 1;
            currentSightRadius = initialSightRadius;
            currentMoveSpeed = initialMoveSpeed;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        hungerPercent = currentHunger / hunger;
        thirstPercent = currentThirst / thirst;
        reproductionPercent = currentUrge / reproductionUrge;
        currentFocus -= Time.deltaTime;

        if (currentFocus > 0)
        {
            currentFocus -= Time.deltaTime;
        }

        if (developmentPercent < 1)
        {
            Grow();
        }
        else if (child)
        {
            child = false;
        }

        if (givingBirth)
        {
            priority = -1;
            return;
        }

        if (eating)
        {
            priority = -1;
            return;
        }

        if (drinking)
        {
            priority = -1;
            return;
        }

        if (reproducing)
        {
            priority = -1;
            return;
        }

        if (!moving && currentFocus <= 0)
        {
            if (thirstPercent < thirstCriticalPercent)
            {
                priority = 1;
                currentFocus = startFocus;
            }
            else if (hungerPercent < foodCriticalPercent)
            {
                priority = 2;
                currentFocus = startFocus;
            }
            else if (reproductionPercent < reproductionPercentSearch)
            {
                priority = 0;
                currentFocus = startFocus;
            }
            else if (thirstPercent < thirstPercentSearch)
            {
                priority = 1;
                currentFocus = startFocus;
            }
            else if (hungerPercent < foodPercentSearch)
            {
                priority = 2;
                currentFocus = startFocus;
            }
            else
            {
                priority = -1;
                Wander();
            }
        }
    }

    public void Wander()
    {
        availableMoves.Clear();
        Utils.GetValidMoves(availableMoves, position);

        if (availableMoves.Count > 0)
        {
            int index = Random.Range(0, availableMoves.Count);

            StartCoroutine(Run(availableMoves[index]));
            position = availableMoves[index];
        }
    }

    public IEnumerator Run(Vector2Int target)
    {
        moving = true;

        Vector3 updatedTarget = mapScript.ConvertMapPositionToWorldPosition(target);
        Vector3 distance = updatedTarget - transform.position;
        Vector3 direction = distance.normalized;

        yield return StartCoroutine(Rotate(direction));

        float runDuration = 1.0f / currentMoveSpeed;

        float time = 0;
        float percent;

        while (time < runDuration)
        {
            percent = (float)time / runDuration;
            transform.position += distance / runDuration * Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }

        yield return null;

        moving = false;
    }

    IEnumerator Rotate(Vector3 dir)
    {
        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float angle;

        while (true)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05)
            {
                angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
                transform.eulerAngles = Vector3.up * angle;
                yield return null;
            }
            else
            {
                break;
            }
        }
    }

    private void Grow()
    {
        developmentPercent += growRate * Time.deltaTime;

        if (developmentPercent > 1)
        {
            developmentPercent = 1;
        }

        float scale = Mathf.Lerp(0, maxScale, developmentPercent);

        currentSightRadius = initialSightRadius * developmentPercent;
        currentMoveSpeed = initialMoveSpeed * developmentPercent;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public override void GenerateGeneticsInfo(Animals partner)
    {
        Wolf wolfPartner = (Wolf)partner;

        wolfInfo = new GeneticsLab.GeneticInfo(
                                                initialMoveSpeed,
                                                initialSightRadius,
                                                hungerDecay,
                                                thirstDecay,
                                                gestationTime,
                                                attractiveness
                                                );

        partnerInfo = new GeneticsLab.GeneticInfo(
                                                    wolfPartner.initialMoveSpeed,
                                                    wolfPartner.initialSightRadius,
                                                    wolfPartner.hungerDecay,
                                                    wolfPartner.thirstDecay,
                                                    wolfPartner.gestationTime,
                                                    wolfPartner.attractiveness
                                                  );
    }

    public override float GetSearchAngle()
    {
        return advancedSearchAngle;
    }

    public void CheckFoodOverflow()
    {
        if (currentHunger > hunger)
        {
            currentHunger = hunger;
        }
    }

    public GeneticsLab.GeneticInfo GetPartnerGenetics()
    {
        return partnerInfo;
    }

    public GeneticsLab.GeneticInfo GetBunnyGenetics()
    {
        return wolfInfo;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentSightRadius);
    }


}

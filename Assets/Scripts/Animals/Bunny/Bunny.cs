using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BunnySearchFood))]
[RequireComponent(typeof(AnimalSearchWater))]
[RequireComponent(typeof(AnimalsSearchPartner))]
public class Bunny : Animals
{
    public float hungerProvidedWhenEaten;
    [Range(0, 180.0f)]
    public float advancedSearchAngle;
    
    private List<Vector2Int> availableMoves;
    
    private Vector2Int targetMapCoord;
    private bool hunted;
    private bool dying;
    private bool fellOver;
    private Wolf hunter;

    private GeneticsLab.GeneticInfo bunnyInfo;
    private GeneticsLab.GeneticInfo partnerInfo;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        availableMoves = new List<Vector2Int>();
        moving = false;

        if (!setDevelopment)
        {
            developmentPercent = 1;
            currentSightRadius = initialSightRadius;
            currentMoveSpeed = initialMoveSpeed;
        }

        priority = -1;
    }

    // Update is called once per frame
    public override void Update()
    {
        // If the bunny is dying don't don't update any of it's characteristics
        if (dying)
        {
            priority = -1;
            return;
        }

        base.Update();
        hungerPercent = currentHunger / hunger;
        thirstPercent = currentThirst / thirst;
        reproductionPercent = currentUrge / reproductionUrge;

        if (currentFocus > 0)
        {
            currentFocus -= Time.deltaTime;
        }

        if (developmentPercent < 1)
        {
            Grow();
        } else if (child)
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
            // Maximum priority, need to get away ASAP
            if (hunted) {

                // The wolf died or gave up
                if (hunter == null)
                {
                    hunted = false;
                } else
                {
                    priority = 3;
                    Run();
                }
            }
            else if (thirstPercent < thirstCriticalPercent)
            {
                priority = 1;
                currentFocus = startFocus;
            }
            else if (hungerPercent < foodCriticalPercent) {
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
            else if (hungerPercent < foodPercentSearch) {
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

    public void Run()
    {
        Debug.Log("Bunny number: " + animalId + " is running for its life!");
        availableMoves.Clear();
        Utils.GetValidMoves(availableMoves, position);

        Vector2Int farthestPosition = Utils.GetFarthestPositionFromTarget(availableMoves, GetMapPosition(), hunter.GetMapPosition());
        StartCoroutine(Hop(farthestPosition));
        position = farthestPosition;
    }

    public void Wander()
    {
        availableMoves.Clear();
        Utils.GetValidMoves(availableMoves, position);

        if (availableMoves.Count > 0)
        {
            int index = Random.Range(0, availableMoves.Count);

            StartCoroutine(Hop(availableMoves[index]));
            position = availableMoves[index];
        }
    }

    public IEnumerator Hop(Vector2Int target)
    {
        moving = true;

        Vector3 updatedTarget = mapScript.ConvertMapPositionToWorldPosition(target);
        Vector3 distance = updatedTarget - transform.position;
        Vector3 direction = distance.normalized;

        yield return StartCoroutine(Rotate(direction));
        
        float initialY = transform.position.y;
        float jumpDuration = 1.0f / currentMoveSpeed;

        float time = 0;
        float percent;
        float yAmount;

        while (time < jumpDuration)
        {
            percent = (float)time / jumpDuration;
            yAmount = (-Mathf.Pow(percent, 2) + percent) + initialY;
            transform.position += distance / jumpDuration * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, yAmount, transform.position.z);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, initialY, transform.position.z);

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
    

    // Retrieves the partner
    // Only used for females that are giving birth
    public GeneticsLab.GeneticInfo GetPartnerGenetics()
    {
        return partnerInfo;
    }

    public GeneticsLab.GeneticInfo GetBunnyGenetics()
    {
        return bunnyInfo;
    }

    public void SetHuntedStatus(bool newStatus, Wolf _hunter)
    {
        hunted = newStatus;
        hunter = _hunter;
        ResetFocus();
        priority = 3;
    }

    public float GetHungerRefill()
    {
        return hungerProvidedWhenEaten * developmentPercent;
    }

    public void Die(float timeToBeEaten)
    {
        StartCoroutine(Dying(timeToBeEaten));
    }

    IEnumerator Dying(float timeToBeEaten)
    {
        dying = true;

        while (moving)
        {
            yield return null;
        }

        timeToBeEaten *= developmentPercent;
        yield return StartCoroutine(FallOver());


        float timePassed = 0;

        while (timePassed < timeToBeEaten)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator FallOver()
    {
        Vector3 direction;
        float fallTime = 1;
        float percent = 0;
        float timePassed = 0;
        float initialX = transform.rotation.eulerAngles.z;

        // Choose the direction in which the bunny will fall
        float chance = Random.Range(0, 2);
        if (chance == 0)
        {
            direction = Vector3.forward;
        } else
        {
            direction = Vector3.back;
        }

        float lastValue = 0;
        float amount;

        while (percent < 1)
        {
            percent = timePassed / fallTime;
            amount = percent * 90 * direction.z;
            Quaternion localRotation = Quaternion.Euler(0f, 0f, amount - lastValue);
            transform.rotation = transform.rotation * localRotation;
            lastValue = amount;
            timePassed += Time.deltaTime;

            yield return null;
        }

        fellOver = true;

    }

    public bool HasFallenOver()
    {
        return fellOver;
    }

    public override float GetSearchAngle()
    {
        return advancedSearchAngle;
    }
   

    public float GetConsumptionRate()
    {
        return consumptionRate;
    }

    public override void GenerateGeneticsInfo(Animals partner)
    {
        Bunny bunnyPartner = (Bunny)partner;

        bunnyInfo = new GeneticsLab.GeneticInfo(
                                                initialMoveSpeed,
                                                initialSightRadius,
                                                hungerDecay,
                                                thirstDecay,
                                                gestationTime,
                                                attractiveness
                                                );

        partnerInfo = new GeneticsLab.GeneticInfo(
                                                    bunnyPartner.initialMoveSpeed,
                                                    bunnyPartner.initialSightRadius,
                                                    bunnyPartner.hungerDecay,
                                                    bunnyPartner.thirstDecay,
                                                    bunnyPartner.gestationTime,
                                                    bunnyPartner.attractiveness
                                                  );
    }

    public void SetMovingStatus(bool newStatus)
    {
        moving = newStatus;
    }

    public float GetCurrentMovementSpeed()
    {
        return currentMoveSpeed;
    }

    public bool IsHunted()
    {
        return hunted;
    }

    public bool IsDying()
    {
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, currentSightRadius);
    }
}

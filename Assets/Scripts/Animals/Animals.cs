using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animals : MonoBehaviour
{
    [Header("Characteristics range")]
    public Vector3 speedRange;
    public Vector3 sightRange;
    public Vector3 hungerDecayRange;
    public Vector3 thirstDecayRange;
    public Vector3 gestationTimeRange; // Female specific
    public Vector3 attractivenessRange; // Male Specific - 
    [Range(0, 1.0f)]
    public float foodPercentSearch;
    [Range(0, 1.0f)]
    public float thirstPercentSearch;
    [Range(0, 1.0f)]
    public float foodCriticalPercent;
    [Range(0, 0.5f)]
    public float thirstCriticalPercent;
    [Range(0, 0.5f)]
    public float reproductionPercentSearch;


    [Header("Characteristics range, but they need to be between 0 and 1")]
    public Vector3 developmentRange;

    [Header("Characteristics of individual")]
    public float hunger;
    public float thirst;
    public float reproductionUrge;
    public float consumptionRate;
    public float drinkRate;
    public float urgeDecay;
    public float timeToReproduce;
    public float startFocus; // used to keep the bunny from changing tasks instantly
    public float turnSpeed;
    public int memorySize; // used by the bunny to search more efficiently
    [Range(0, 0.1f)]
    public float growRate;


    protected Test mapScript;
    protected List<Vector2Int> directions;
    protected Vector2Int position;
    protected int gender; // 0 - female, 1 - male

    protected int priority;
    protected float gestationTime;
    protected float hungerDecay;
    protected float thirstDecay;
    protected float hungerPercent;
    protected float thirstPercent;
    protected float reproductionPercent;
    protected float maxScale;
    protected float developmentPercent;



    [Header("Initial value of stats")]
    protected float initialMoveSpeed;
    protected float initialSightRadius;


    [Header("Current values of stats")]
    protected float currentHunger;
    protected float currentThirst;
    protected float currentUrge;
    protected float currentSightRadius;
    protected float currentMoveSpeed;
    protected float currentFocus;
    protected float attractiveness;

    [Header("Current status")]
    protected bool eating;
    protected bool drinking;
    protected bool reproducing;
    protected bool pregnant;
    protected bool givingBirth;
    protected bool child;
    protected bool moving;
    protected bool searchingFood;
    protected bool searchingWater;
    protected bool searchingPartner;
    protected bool setPosition;
    protected bool setDevelopment;


    protected int animalId;

    private static int id = 0;
    private bool setGender;

    // Start is called before the first frame update
    public virtual void Start()
    {
        animalId = id;
        id += 1;

        mapScript = Test.getInstance();

        // All possible directions the bunny can move
        directions = new List<Vector2Int>();
        directions.Add(new Vector2Int(-1, -1));
        directions.Add(new Vector2Int(0, -1));
        directions.Add(new Vector2Int(1, -1));
        directions.Add(new Vector2Int(-1, 0));
        directions.Add(new Vector2Int(1, 0));
        directions.Add(new Vector2Int(-1, 1));
        directions.Add(new Vector2Int(0, 1));
        directions.Add(new Vector2Int(1, 1));

        currentHunger = hunger;
        currentThirst = thirst;
        currentUrge = reproductionUrge;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        StatsDecay();
    }

    // Constantly decay the stats of the bunny
    void StatsDecay()
    {
        if (!eating)
        {
            currentHunger -= hungerDecay * Time.deltaTime;
        }

        if (!drinking)
        {
            currentThirst -= thirstDecay * Time.deltaTime;
        }

        // The urge starts only when the bunny is grown
        // And only after the female has given birth
        if (currentUrge > 0 && !pregnant && !child)
        {
            currentUrge -= urgeDecay * Time.deltaTime;
            if (currentUrge < 0)
            {
                currentUrge = 0;
            }
        }

        if (currentHunger < 0 || currentThirst < 0)
        {
            Destroy(gameObject);
        }
    }


    public virtual void GenerateGeneticsInfo (Animals animal)
    {

    }

    public void SetDevelopmentPercent(float percent)
    {
        setDevelopment = true;

        if (percent < 1)
        {
            child = true;
            developmentPercent = percent;

            float scale = Mathf.Lerp(0, maxScale, developmentPercent);
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            child = false;
        }
    }

    public void SetInitialCharacteristics(
                                            float _speed,
                                            float _sightRadius,
                                            float _hungerDecay,
                                            float _thirstDecay,
                                            float _gestationTime,
                                            float _attractiveness
                                            )
    {
        // TODO: this should be able to be set only once
        initialMoveSpeed = _speed;
        initialSightRadius = _sightRadius;
        hungerDecay = _hungerDecay;
        thirstDecay = _thirstDecay;
        gestationTime = _gestationTime;
        attractiveness = _attractiveness;

    }

    public void ResetFocus()
    {
        currentFocus = 0;
    }

    public void SetInitialPosition(Vector2Int _position)
    {
        position = _position;
    }

    public void SetGender(int _gender)
    {
        if (!setGender)
        {
            gender = _gender;
            setGender = true;
        }
    }

    public int GetPriority()
    {
        return priority;
    }

    public bool IsDrinking()
    {
        return drinking;
    }

    public bool IsEating()
    {
        return eating;
    }

    public bool IsReproducing()
    {
        return reproducing;
    }

    public bool IsPregnant()
    {
        return pregnant;
    }

    public bool IsMoving()
    {
        return moving;
    }

    public bool IsSearchingFood()
    {
        return searchingFood;
    }

    public bool IsSearchingForWater()
    {
        return searchingWater;
    }

    public bool IsSearchingForPartner()
    {
        return searchingPartner;
    }

    public void SetSearchFoodStatus(bool newStatus)
    {
        searchingFood = newStatus;
    }

    public void SetEatingStatus(bool newStatus)
    {
        eating = newStatus;
    }

    public void SetSearchingWaterStatus(bool newStatus)
    {
        searchingWater = newStatus;
    }

    public void SetSearchingPartnerStatus(bool newStatus)
    {
        searchingPartner = newStatus;
    }

    public int GetGender()
    {
        return gender;
    }

    public float GetSightRadius()
    {
        return currentSightRadius;
    }

    public int GetMemorySize()
    {
        return memorySize;
    }

    public Vector2Int GetMapPosition()
    {
        return position;
    }

    public virtual float GetSearchAngle()
    {
        return 0;
    }
    
    public void SetMapPosition(Vector2Int mapPos)
    {
        position = mapPos;
    }
    public bool IsGivingBirth()
    {
        return givingBirth;
    }

    public void SetGivingBirthStatus(bool newStatus)
    {
        givingBirth = newStatus;
    }

    public void SetDrinkingStatus(bool newStatus)
    {
        drinking = newStatus;
    }


    public float GetInitialMovementSpeed()
    {
        return initialMoveSpeed;
    }

    public float GetInitialSightRadius()
    {
        return initialSightRadius;
    }

    public float GetTurnSpeed()
    {
        return turnSpeed;
    }

    public float GetCurrentHunger()
    {
        return currentHunger;
    }

    public float GetHungerPercent()
    {
        return hungerPercent;
    }

    public float GetCriticalHungerPercent()
    {
        return foodCriticalPercent;
    }

    public float GetHungerDecay()
    {
        return hungerDecay;
    }

    public void AddCurrentHunger(float amount)
    {
        currentHunger += amount;
    }

    public void SetCurrentHunger(float value)
    {
        currentHunger = value;
    }

    public float GetMaxHunger()
    {
        return hunger;
    }

    public float GetCurrentThirst()
    {
        return currentThirst;
    }

    public float GetThirstPercent()
    {
        return thirstPercent;
    }

    public float GetCriticalThirstPercent()
    {
        return thirstCriticalPercent;
    }

    public float GetThirstDecay()
    {
        return thirstDecay;
    }

    public float GetMaxThirst()
    {
        return thirst;
    }

    public void AddThirstAmount(float amount)
    {
        currentThirst += amount;
    }

    public void SetThirstAmount(float value)
    {
        currentThirst = value;
    }

    public float GetDrinkRate()
    {
        return drinkRate;
    }

    public void SetReproducingStatus(bool newStatus)
    {
        reproducing = newStatus;
    }

    public void SetPregnantStatus(bool newStatus)
    {
        pregnant = newStatus;
    }

    public float GetCurrentReproduction()
    {
        return reproductionPercent;
    }

    public float GetReproductionSearchPercent()
    {
        return reproductionPercentSearch;
    }

    public int GetAnimalID()
    {
        return animalId;
    }

    public float GetAttractiveness()
    {
        return attractiveness;
    }

    public void SetPositionStatus(bool newStatus)
    {
        setPosition = newStatus;
    }
    public bool IsPositionSet()
    {
        return setPosition;
    }

    public float GetTimeToReproduce()
    {
        return timeToReproduce;
    }

    public float GetGestationTime()
    {
        return gestationTime;
    }

    public void ResetUrge()
    {
        currentUrge = reproductionUrge;
    }

    public void SetMaxScale(float scale)
    {
        maxScale = scale;
    }

    public float GetDevelopmentPercent()
    {
        return developmentPercent;
    }
}

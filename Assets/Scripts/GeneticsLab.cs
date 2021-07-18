using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticsLab
{
    public struct GeneticInfo
    {
        private float movementSpeed;
        private float sightRadius;
        private float hungerDecay;
        private float thirstDecay;
        private float gestationTime;
        private float attractiveness;

        public GeneticInfo(
                           float _movementSpeed,
                           float _sightRadius,
                           float _hungerDecay,
                           float _thirstDecay,
                           float _gestationTime,
                           float _attractiveness
                           )
        {
            movementSpeed = _movementSpeed;
            sightRadius = _sightRadius;
            hungerDecay = _hungerDecay;
            thirstDecay = _thirstDecay;
            gestationTime = _gestationTime;
            attractiveness = _attractiveness;
        }

        public float GetSpeed()
        {
            return movementSpeed;
        }

        public float GetSight()
        {
            return sightRadius;
        }

        public float GetHungerDecay()
        {
            return hungerDecay;
        }

        public float GetThirstDecay()
        {
            return thirstDecay;
        }

        public float GetGestationTime()
        {
            return gestationTime;
        }

        public float GetAttractiveness()
        {
            return attractiveness;
        }
    };

    public static int crossoverIndex = 4;
    public static List<int> frequencyMutationSelectionBunny;
    public static List<int> frequencyMutationSelectionWolf;


    private static float mutationChance = 0.15f; // Base is 0.15f; modify if different
    private static Vector2 minMaxMutationPercent = new Vector2(0.1f, 0.2f);
    private static List<List<float>> mutationSchemes;
    private static List<float> mutationSchemeChance;
    private static List<List<float>> mutationIncreaseChance;
    private static List<List<int>> crossOverSchemes;
    private static float none = 0;
    private static float smallUp = 0.1f;
    private static float bigUp = 0.25f;
    private static float smallDown = -0.1f;
    private static float bigDown = -0.25f;

    static GeneticsLab()
    {
        mutationSchemes = new List<List<float>>()
        {
            //                Speed,  Sight, Hunger, Thirst, Gestation, Attractiveness
            new List<float>() {bigUp, bigUp, bigUp, bigUp, bigUp, bigUp}, // 1.Buff up scheme
            new List<float>() {bigDown, bigDown, smallDown, smallDown, bigDown, smallDown}, // 2.Buff down scheme
            new List<float>() {bigUp, none, none, bigUp, none, none}, // 3.Speed up
            new List<float>() {none, bigUp, smallDown, smallUp, none, none}, // 4.Sight up
            new List<float>() {none, none, none, none, none, none}, // 5.Independent
            new List<float>() {none, none, smallUp, none, bigUp, smallUp}, // 6.Gestation up
            new List<float>() {none, smallDown, none, none, none, bigUp}, // 7.Attractiveness increase
        };

        mutationSchemeChance = new List<float>()
        {
            0.05f, 0.1f, 0.25f, 0.4f, 0.7f, 0.85f, 1.0f
        };

        mutationIncreaseChance = new List<List<float>>()
        {
            new List<float>() {0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f},
            new List<float>() {0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f},
            new List<float>() {0.2f, 0.0f, 0.0f, 0.2f, 0.0f, 0.0f},
            new List<float>() {0.0f, 0.2f, 0.2f, 0.2f, 0.0f, 0.0f},
            new List<float>() {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            new List<float>() {0.0f, 0.0f, 0.2f, 0.0f, 0.2f, 0.2f},
            new List<float>() {0.0f, 0.2f, 0.0f, 0.0f, 0.0f, 0.2f}
        };

        crossOverSchemes = new List<List<int>>()
        {
            new List<int>() {-1, -1, -1, -1, -1, -1}, // Random
            new List<int>() {0, 0, 1, 1, -1, -1}, // First half mother, second half father
            new List<int>() {1, 1, 0, 0, -1, -1}, // First half father, second half mother
            new List<int>() {1, 0, 1, 0, -1, -1}, // Even father, odd mother
            new List<int>() {0, 1, 0, 1, -1, -1}, // Even mother, odd father
        };

        frequencyMutationSelectionBunny = new List<int>()
        {
            0, 0, 0, 0, 0, 0, 0
        };

        frequencyMutationSelectionWolf = new List<int>()
        {
            0, 0, 0, 0, 0, 0, 0
        };

        // TODO: maybe add a list with increased mutation chances
        // TODO: give a bonus chance for a mutation to happen if in the scheme they have a different value than none

    }

    public static void CreateNewAnimal(GeneticInfo female, GeneticInfo male, Animals offspring)
    {
        int index = 0;
        float chance = Random.Range(0.0f, 1.0f);

        // Select a mutation scheme
        index = SelectMutationScheme(index, chance);
        //index = 4;

        if (offspring is Bunny)
        {
            frequencyMutationSelectionBunny[index]++;
        } else
        {
            frequencyMutationSelectionWolf[index]++;
        }
        //Debug.Log("Selected mutation scheme: " + index);


        // TODO: Decide wether the mutation scheme will be applied whole or not
        // TODO: Do i want to let them happend individually by chance or do i want to mutate all the genes when i apply a mutation scheme?

        float movementSpeed = VaryStat(female.GetSpeed(), male.GetSpeed(), offspring.speedRange, offspring.GetGender(), index, 0);
        float sightRadius = VaryStat(female.GetSight(), male.GetSight(), offspring.sightRange, offspring.GetGender(), index, 1);
        float hungerDecay = VaryStat(female.GetHungerDecay(), male.GetHungerDecay(), offspring.hungerDecayRange, offspring.GetGender(), index, 2);
        float thirstDecay = VaryStat(female.GetThirstDecay(), male.GetThirstDecay(), offspring.thirstDecayRange, offspring.GetGender(), index, 3);
        float gestationTime = VaryStat(female.GetGestationTime(), male.GetGestationTime(), offspring.gestationTimeRange, offspring.GetGender(), index, 4);
        float attractiveness = VaryStat(female.GetAttractiveness(), male.GetAttractiveness(), offspring.attractivenessRange, offspring.GetGender(), index, 5);

        offspring.SetInitialCharacteristics(
                                            movementSpeed,
                                            sightRadius,
                                            hungerDecay,
                                            thirstDecay,
                                            gestationTime,
                                            attractiveness
                                            );
    }

    private static int SelectMutationScheme(int index, float chance)
    {
        if (chance > mutationSchemeChance[0])
        {
            for (int i = 1; i < mutationSchemeChance.Count; ++i)
            {
                if (chance <= mutationSchemeChance[i])
                {
                    break;
                }
                else
                {
                    index = i;
                }
            }
        }

        return index;
    }

    //public static void CreateNewBunny(GeneticInfo female, GeneticInfo male, Bunny offspring)
    //{
    //    int index = 0;
    //    float chance = Random.Range(0.0f, 1.0f);

    //    // Select a mutation scheme
    //    index = SelectMutationScheme(index, chance);
    //    //Debug.Log("Selected mutation scheme: " + index);


    //    // TODO: Decide wether the mutation scheme will be applied whole or not
    //    // TODO: Do i want to let them happend individually by chance or do i want to mutate all the genes when i apply a mutation scheme?

    //    float movementSpeed = VaryStat(female.GetSpeed(), male.GetSpeed(), offspring.speedRange, offspring.GetGender(), index, 0);
    //    float sightRadius = VaryStat(female.GetSight(), male.GetSight(), offspring.sightRange, offspring.GetGender(), index, 1);
    //    float hungerDecay = VaryStat(female.GetHungerDecay(), male.GetHungerDecay(), offspring.hungerDecayRange, offspring.GetGender(), index, 2);
    //    float thirstDecay = VaryStat(female.GetThirstDecay(), male.GetThirstDecay(), offspring.thirstDecayRange, offspring.GetGender(), index, 3);
    //    float gestationTime = VaryStat(female.GetGestationTime(), male.GetGestationTime(), offspring.gestationTimeRange, offspring.GetGender(), index, 4);
    //    float attractiveness = VaryStat(female.GetAttractiveness(), male.GetAttractiveness(), offspring.attractivenessRange, offspring.GetGender(), index, 5);

    //    offspring.SetInitialCharacteristics(
    //                                        movementSpeed,
    //                                        sightRadius,
    //                                        hungerDecay,
    //                                        thirstDecay,
    //                                        gestationTime,
    //                                        attractiveness
    //                                        );
    //}

    private static float VaryStat(float femaleValue, float maleValue, Vector3 range, int gender, int index, int traitCode)
    {
        float chance;
        float stat;

        // Negative stats are only if the male/female does not have that characteristic
        if (maleValue < 1 || femaleValue < 1)
        {
            if (gender == 0)
            {
                stat = femaleValue;
            }
            else
            {
                stat = maleValue;
            }

            // Vary the specific stat
            chance = Random.Range(0.0f, 1.0f);
            if (chance < mutationChance + mutationIncreaseChance[index][traitCode])
            {
                stat = Mutate(stat, index, traitCode);
                stat = Mathf.Clamp(stat, range.x, range.y);
            }

        } else
        {
            stat = SelectCharacteristicFromParent(femaleValue, maleValue);
            chance = Random.Range(0.0f, 1.0f);
            if (chance < mutationChance + mutationIncreaseChance[index][traitCode])
            {
                stat = Mutate(stat, index, traitCode);
                stat = Mathf.Clamp(stat, range.x, range.y);
            }
        }

        return stat;
    }

    // Random crossover
    private static float SelectCharacteristicFromParent(float femaleValue, float maleValue)
    {
        int index = Random.Range(0, 2);
        if (index == 0)
        {
            return femaleValue;
        }

        return maleValue;
    }

    private static float SelectCharacteristicFromParent(float femaleValue, float maleValue, int traitCode)
    {
        if (crossOverSchemes[crossoverIndex][traitCode] == -1)
        {
            int index = Random.Range(0, 2);
            if (index == 0)
            {
                return femaleValue;
            }

            return maleValue;
        } else if (crossOverSchemes[crossoverIndex][traitCode] == 0)
        {
            return femaleValue;
        }

        return maleValue;

    }

    public static void GenerateRandomBunnyCharacteristics(Bunny bunny)
    {
        float speed = Random.Range(bunny.speedRange.x, bunny.speedRange.z);
        float sightRange = Random.Range(bunny.sightRange.x, bunny.sightRange.z);
        float hungerDecay = Random.Range(bunny.hungerDecayRange.x, bunny.hungerDecayRange.z);
        float thirstDecay = Random.Range(bunny.thirstDecayRange.x, bunny.thirstDecayRange.z);
        float gestationTime = Random.Range(bunny.gestationTimeRange.x, bunny.gestationTimeRange.z);
        float attractiveness = Random.Range(bunny.attractivenessRange.x, bunny.attractivenessRange.z);

        // If it's a male set all the useless characteristics
        if (bunny.GetGender() == 1)
        {
            gestationTime = -1;
        }

        // If it's a female set all the male characteristics to 0
        if (bunny.GetGender() == 0)
        {
            attractiveness = -1;
        }

        bunny.SetInitialCharacteristics(
                                        speed,
                                        sightRange,
                                        hungerDecay,
                                        thirstDecay,
                                        gestationTime,
                                        attractiveness
                                        );
    }

    // TODO: Use polymorphism to combine the above function with this one
    public static void GenerateRandomWolfCharacteristics(Wolf wolf)
    {
        float speed = Random.Range(wolf.speedRange.x, wolf.speedRange.z);
        float sightRange = Random.Range(wolf.sightRange.x, wolf.sightRange.z);
        float hungerDecay = Random.Range(wolf.hungerDecayRange.x, wolf.hungerDecayRange.z);
        float thirstDecay = Random.Range(wolf.thirstDecayRange.x, wolf.thirstDecayRange.z);
        float gestationTime = Random.Range(wolf.gestationTimeRange.x, wolf.gestationTimeRange.z);
        float attractiveness = Random.Range(wolf.attractivenessRange.x, wolf.attractivenessRange.z);

        // If it's a male set all the useless characteristics
        if (wolf.GetGender() == 1)
        {
            gestationTime = -1;
        }

        // If it's a female set all the male characteristics to 0
        if (wolf.GetGender() == 0)
        {
            attractiveness = -1;
        }

        wolf.SetInitialCharacteristics(
                                        speed,
                                        sightRange,
                                        hungerDecay,
                                        thirstDecay,
                                        gestationTime,
                                        attractiveness
                                        );
    }

    private static float Mutate(float trait, int index, int traitCode)
    {
        //Debug.Log("Mutation happened!");
        float mutationPercent;
        float border = 0.5f;
        float chance;

        mutationPercent = Random.Range(minMaxMutationPercent.x, minMaxMutationPercent.y);
        chance = Random.Range(0.0f, 1.0f);

        //Debug.Log("Modifier: " + mutationSchemes[index][traitCode] + " index: " + index + "   trait code: " + traitCode);

        if (chance + mutationSchemes[index][traitCode] <= border)
        {
            trait -= trait * mutationPercent;
        } else
        {
            trait += trait * mutationPercent;
        }

        return trait;
    }
}

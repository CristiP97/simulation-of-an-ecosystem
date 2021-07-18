using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class InformationLogger : MonoBehaviour
{
    public float logTime;
    public string runFolderName;
    public GameObject preyHolder;
    public GameObject hunterHolder;

    private string runsFolder = "Assets/Resources/LoggedRuns/";
    private float remainingTime;
    private float timePassed;

    [Header("Bunny info")]
    private float avgBunnySpeed;
    private float avgBunnySightRadius;
    private float avgBunnyHungerDecay;
    private float avgBunnyThirstDecay;
    private float avgBunnyGestationTime;
    private float avgBunnyAttractiveness;

    [Header("Wolves info")]
    private float avgWolfSpeed;
    private float avgWolfSightRadius;
    private float avgWolfHungerDecay;
    private float avgWolfThirstDecay;
    private float avgWolfGestationTime;
    private float avgWolfAttractiveness;

    private Animals currentAnimal;

    private List<string> fileNames;

    private int counter;

    // Start is called before the first frame update
    private void Start()
    {
        FileStream aux;
        fileNames = new List<string>();
        
        if (!Directory.Exists("Assets/Resources/Bunnies"))
        {
            Directory.CreateDirectory("Assets/Resources/Bunnies");
        }

        if (!Directory.Exists("Assets/Resources/Wolves"))
        {
            Directory.CreateDirectory("Assets/Resources/Wolves");
        }

        // For the bunnies!
        fileNames.Add("Assets/Resources/Bunnies/BunnySpeed.csv");
        fileNames.Add("Assets/Resources/Bunnies/BunnySight.csv");
        fileNames.Add("Assets/Resources/Bunnies/BunnyHunger_Decay.csv");
        fileNames.Add("Assets/Resources/Bunnies/BunnyThirst_Decay.csv");
        fileNames.Add("Assets/Resources/Bunnies/BunnyGestation.csv");
        fileNames.Add("Assets/Resources/Bunnies/BunnyAttractiveness.csv");

        // For the wolfs!
        fileNames.Add("Assets/Resources/Wolves/WolfSpeed.csv");
        fileNames.Add("Assets/Resources/Wolves/WolfSight.csv");
        fileNames.Add("Assets/Resources/Wolves/WolfHunger_Decay.csv");
        fileNames.Add("Assets/Resources/Wolves/WolfThirst_Decay.csv");
        fileNames.Add("Assets/Resources/Wolves/WolfGestation.csv");
        fileNames.Add("Assets/Resources/Wolves/WolfAttractiveness.csv");

        fileNames.Add("Assets/Resources/Wolves/MutationFrequencyBunny.csv");
        fileNames.Add("Assets/Resources/Wolves/MutationFrequencyWolf.csv");


        // Open/Create and empty the files
        for (int i = 0; i < fileNames.Count; ++i)
        {
            if (!File.Exists(fileNames[i]))
            {
                File.Create(fileNames[i]);
            } else
            {
                aux = new FileStream(fileNames[i], FileMode.Truncate);
                aux.Close();
            }
        }

        remainingTime = logTime;
        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        int bunnyFemales = 0;
        int bunnyMales = 0;
        int wolfFemales = 0;
        int wolfMales = 0;

        remainingTime -= Time.deltaTime;
        timePassed += Time.deltaTime;

        if (remainingTime < 0)
        {
            remainingTime = logTime;
            avgBunnySpeed = 0;
            avgBunnySightRadius = 0;
            avgBunnyHungerDecay = 0;
            avgBunnyThirstDecay = 0;
            avgBunnyGestationTime = 0;
            avgBunnyAttractiveness = 0;
            avgWolfSpeed = 0;
            avgWolfSightRadius = 0;
            avgWolfHungerDecay = 0;
            avgWolfThirstDecay = 0;
            avgWolfGestationTime = 0;
            avgWolfAttractiveness = 0;


            LogInformationAboutEntity(ref avgBunnySpeed,
                                      ref avgBunnySightRadius, 
                                      ref avgBunnyHungerDecay, 
                                      ref avgBunnyThirstDecay,
                                      ref avgBunnyGestationTime,
                                      ref avgBunnyAttractiveness,
                                      ref bunnyFemales,
                                      ref bunnyMales,
                                      preyHolder);

            LogInformationAboutEntity(ref avgWolfSpeed,
                                      ref avgWolfSightRadius,
                                      ref avgWolfHungerDecay,
                                      ref avgWolfThirstDecay,
                                      ref avgWolfGestationTime,
                                      ref avgWolfAttractiveness,
                                      ref wolfFemales,
                                      ref wolfMales,
                                      hunterHolder);


            for (int i = 0; i < fileNames.Count; ++i)
            {
                StreamWriter writer = new StreamWriter(fileNames[i], true);
                //writer.Write("Time elapsed: " + timePassed + " | ");

                switch (i) {
                    case 0:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|AverageSpeed|Individuals|Females|Males\n");
                        }

                        writer.Write(timePassed + "|" + avgBunnySpeed + "|" + (bunnyMales + bunnyFemales) + "|" + bunnyFemales + "|" + bunnyMales);
                        break;
                    case 1:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|AverageSight|Individuals|Females|Males\n");
                        }

                        writer.Write(timePassed + "|" + avgBunnySightRadius + "|" + (bunnyMales + bunnyFemales) + "|" + bunnyFemales + "|" + bunnyMales);
                        break;
                    case 2:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|AverageHunger|Individuals|Females|Males\n");
                        }

                        writer.Write(timePassed + "|" + avgBunnyHungerDecay + "|" + (bunnyMales + bunnyFemales) + "|" + bunnyFemales + "|" + bunnyMales);
                        break;
                    case 3:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|AverageThirst|Individuals|Females|Males\n");
                        }

                        writer.Write(timePassed + "|" + avgBunnyThirstDecay + "|" + (bunnyMales + bunnyFemales) + "|" + bunnyFemales + "|" + bunnyMales);
                        break;
                    case 4:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|GestationTime|Individuals|Females|\n");
                        }

                        writer.Write(timePassed + "|" + avgBunnyGestationTime + "|" + (bunnyMales + bunnyFemales) + "|" + bunnyFemales );
                        break;
                    case 5:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|Attractiveness|Individuals|Males|\n");
                        }

                        writer.Write(timePassed + "|" + avgBunnyAttractiveness + "|" + (bunnyMales + bunnyFemales) + "|" + bunnyMales);
                        break;
                    case 6:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|Speed|Individuals|Females|Males\n");
                        }
                        writer.Write(timePassed + "|" + avgWolfSpeed + "|" + (wolfMales + wolfFemales) + "|" + wolfFemales + "|" + wolfMales);
                        break;
                    case 7:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|Sight|Individuals|Females|Males\n");
                        }
                        writer.Write(timePassed + "|" + avgWolfSightRadius + "|" + (wolfMales + wolfFemales) + "|" + wolfFemales + "|" + wolfMales);
                        break;
                    case 8:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|Hunger|Individuals|Females|Males\n");
                        }
                        writer.Write(timePassed + "|" + avgWolfHungerDecay + "|" + (wolfMales + wolfFemales) + "|" + wolfFemales + "|" + wolfMales);
                        break;
                    case 9:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|Thirst|Individuals|Females|Males\n");
                        }
                        writer.Write(timePassed + "|" + avgWolfThirstDecay + "|" + (wolfMales + wolfFemales) + "|" + wolfFemales + "|" + wolfMales);
                        break;
                    case 10:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|GestationTime|Individuals|Females\n");
                        }
                        writer.Write(timePassed + "|" + avgWolfThirstDecay + "|" + (wolfMales + wolfFemales) + "|" + wolfFemales);
                        break;
                    case 11:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("Time|Attractiveness|Individuals|Males\n");
                        }
                        writer.Write(timePassed + "|" + avgWolfAttractiveness + "|" + (wolfMales + wolfFemales) + "|" + wolfMales);
                        break;
                    case 12:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("TIME|BUFF_UP|BUFF_DOWN|SPEED_UP|SIGHt_UP|INDEPENDENT|GESTATION_UP|ATTRACTIVENESS_UP\n");
                        }
                        writer.Write(timePassed + "|");

                        for (int j = 0; j < GeneticsLab.frequencyMutationSelectionBunny.Count; ++j)
                        {
                            if (j < GeneticsLab.frequencyMutationSelectionBunny.Count - 1)
                            {
                                writer.Write(GeneticsLab.frequencyMutationSelectionBunny[j] + "|");
                            } else
                            {
                                writer.Write(GeneticsLab.frequencyMutationSelectionBunny[j]);
                            }
                        }

                        break;

                    case 13:
                        if (counter == 0)
                        {
                            writer.Write("sep=|\n");
                            writer.Write("TIME|BUFF_UP|BUFF_DOWN|SPEED_UP|SIGHt_UP|INDEPENDENT|GESTATION_UP|ATTRACTIVENESS_UP\n");
                        }
                        writer.Write(timePassed + "|");

                        for (int j = 0; j < GeneticsLab.frequencyMutationSelectionWolf.Count; ++j)
                        {
                            if (j < GeneticsLab.frequencyMutationSelectionWolf.Count - 1)
                            {
                                writer.Write(GeneticsLab.frequencyMutationSelectionWolf[j] + "|");
                            }
                            else
                            {
                                writer.Write(GeneticsLab.frequencyMutationSelectionWolf[j]);
                            }
                        }

                        break;
                    default:
                        break;
                }
                writer.Write("\n");
                writer.Close();
            }
            counter++;
        }
    }

    private void LogInformationAboutEntity(ref float speed,
                                           ref float sight, 
                                           ref float hungerDecay,
                                           ref float thirstDecay,
                                           ref float gestation,
                                           ref float attractiveness,
                                           ref int females,
                                           ref int males,
                                           GameObject holder)
    {
        foreach (Transform child in holder.GetComponentsInChildren<Transform>())
        {
            if (child != holder.transform)
            {
                currentAnimal = child.GetComponent<Animals>();
                speed += currentAnimal.GetInitialMovementSpeed();
                sight += currentAnimal.GetInitialSightRadius();
                hungerDecay += currentAnimal.GetHungerDecay();
                thirstDecay += currentAnimal.GetThirstDecay();

                if (currentAnimal.GetGender() == 0)
                {
                    gestation += currentAnimal.GetGestationTime();
                    females++;
                }
                else
                {
                    attractiveness += currentAnimal.GetAttractiveness();
                    males++;
                }
            }
        }

        if (holder.transform.childCount != 0)
        {
            speed /= holder.transform.childCount * 1.0f;
            sight /= holder.transform.childCount * 1.0f;
            hungerDecay /= holder.transform.childCount * 1.0f;
            thirstDecay /= holder.transform.childCount * 1.0f;
            gestation /= females * 1.0f;
            attractiveness /= males * 1.0f;
        }
    }

    private void OnDestroy()
    {
        if (!Directory.Exists(runsFolder))
        {
            Directory.CreateDirectory(runsFolder);
        }

        int index = 1;
        string candidateFolder;
        
        if (runFolderName != null && runFolderName != "")
        {
            candidateFolder = runsFolder + runFolderName + index;
        } else
        {
            candidateFolder = runsFolder + "Run" + index;
        }


        while (Directory.Exists(candidateFolder))
        {
            index++;
            if (runFolderName != null && runFolderName != "")
            {
                candidateFolder = runsFolder + "/" + runFolderName + index;
            }
            else
            {
                candidateFolder = runsFolder + "/Run" + index;
            }
        }

        Directory.CreateDirectory(candidateFolder);

        for (int i = 0; i < fileNames.Count; ++i)
        {
            string[] words = fileNames[i].Split('/');
            FileUtil.CopyFileOrDirectory(fileNames[i], candidateFolder + "/" + words[words.Length - 1]);
        }
    }
}

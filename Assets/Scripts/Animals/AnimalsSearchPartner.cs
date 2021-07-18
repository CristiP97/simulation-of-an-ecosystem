using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalsSearchPartner : MonoBehaviour
{
    public LayerMask myMask;

    private Animals animalScript;
    private AnimalsSearchPartner partner;
    private List<AnimalsSearchPartner> rejectedPartners;
    private List<float> rejectedPartnersCd;
    private List<int> result;
    private Test mapScript;
    private float remainingGestationTime;
    private float forgetTime = 10;
    private bool setAverageFemalePosition;
    private bool reachedAverageFemalePosition;
    private Vector2Int averageFemalePosition;

    // Start is called before the first frame update
    void Start()
    {
        animalScript = gameObject.GetComponent<Bunny>();
        if (animalScript == null)
        {
            animalScript = gameObject.GetComponent<Wolf>();
        }
        
        mapScript = Test.instance;
        rejectedPartners = new List<AnimalsSearchPartner>();
        rejectedPartnersCd = new List<float>();
        remainingGestationTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (animalScript.GetPriority() == 0 && !animalScript.IsMoving() && !animalScript.IsReproducing())
        {
            SearchForPartner();
        } 
        // We want to reset bunny partner IF it decides that it reached critical water percent or critical food percent
        else if ((animalScript.GetPriority() == 1 || animalScript.GetPriority() == 2) && partner != null)
        {
            //Debug.Log(bunnyScript.GetPriority());
            //Debug.Log(bunnyScript.IsReproducing());
            //Debug.Log(partner);
            //Debug.Log("Reseted partner cause i changed my priority! This is bunny number: " + animalScript.GetAnimalID());
            animalScript.SetSearchingPartnerStatus(false);
            partner = null;

            setAverageFemalePosition = false;
            reachedAverageFemalePosition = false;
        }

        // Decrease the gestation time
        if (animalScript.IsPregnant())
        {
            if (animalScript is Bunny && ((Bunny)animalScript).IsDying()) 
            {
                animalScript.SetPregnantStatus(false);
            }
            else
            {
                remainingGestationTime -= Time.deltaTime;

                if (remainingGestationTime < 0 && !animalScript.IsMoving())
                {
                    StartCoroutine(Birth());
                }
            }
        }

        bool reconstruct = false;

        // Decrease the time until you forget
        for (int i = 0; i < rejectedPartners.Count; ++i)
        {
            rejectedPartnersCd[i] -= Time.deltaTime;
            if (rejectedPartnersCd[i] <= 0)
            {
                reconstruct = true;
            }
        }

        // Reconstruct new list
        if (reconstruct)
        {
            List<float> cds = new List<float>();
            List<AnimalsSearchPartner> partners = new List<AnimalsSearchPartner>();

            for (int i = 0; i < rejectedPartners.Count; ++i)
            {
                if (rejectedPartnersCd[i] > 0)
                {
                    cds.Add(rejectedPartnersCd[i]);
                    partners.Add(rejectedPartners[i]);
                }
            }

            rejectedPartners = partners;
            rejectedPartnersCd = cds;
        }
    }

    private void SearchForPartner()
    {
        if (!animalScript.IsSearchingForPartner())
        {
            // Clear what the bunny was doing before
            if (animalScript.IsSearchingFood())
            {
                animalScript.SetSearchFoodStatus(false);
            }

            if (animalScript.IsSearchingForWater())
            {
                animalScript.SetSearchingWaterStatus(false);
            }

            List<Vector2Int> partnerLocations = new List<Vector2Int>();
            List<Bunny> partnerScripts = new List<Bunny>();

            // We search for the first available partner, not for the closest one
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, animalScript.GetSightRadius(), myMask);
            foreach (var hitCollider in hitColliders)
            {
                // Wildlife layer is 10
                if (animalScript is Bunny)
                {
                    if (hitCollider.gameObject.layer == 10 && hitCollider.gameObject.tag == "Bunny")
                    {
                        AnimalsSearchPartner candidate = hitCollider.gameObject.GetComponent<AnimalsSearchPartner>();

                        if (ValidPartner(candidate))
                        {
                            partner = candidate.gameObject.GetComponent<AnimalsSearchPartner>();
                            break;
                        }
                    }
                } else if (animalScript is Wolf)
                {
                    if (hitCollider.gameObject.layer == 10 && hitCollider.gameObject.tag == "Wolf")
                    {
                        AnimalsSearchPartner candidate = hitCollider.gameObject.GetComponent<AnimalsSearchPartner>();

                        if (ValidPartner(candidate))
                        {
                            //Debug.Log("WOLF FOUND A VALID PARTNER!");
                            partner = candidate.gameObject.GetComponent<AnimalsSearchPartner>();
                            break;
                        } else
                        {
                            //Debug.Log("NOT A VALID WOLF PARTNER!");
                        }
                    }
                }
            }

            if (partner != null)
            {
                // Male side
                if (animalScript.GetGender() == 1)
                {
                    int response = partner.SendMaleToFemaleSignal(this);
                    if (response == 1)
                    {
                        animalScript.SetSearchingPartnerStatus(true);
                        result = Utils.GoToPartner(partner, partner.animalScript.GetMapPosition(), animalScript.GetMapPosition());
                        
                        // It means that something happened and we have no partner anymore
                        if (result.Count == 0)
                        {
                            animalScript.SetSearchingPartnerStatus(false);
                        }
                        else if (result.Count == 1)
                        {
                            partner.SendReproduceSignal();
                            StartCoroutine(Reproduce());
                        }
                        else if (result.Count == 2)
                        {
                            if (animalScript is Bunny)
                            {
                                StartCoroutine(((Bunny)animalScript).Hop(new Vector2Int(result[0], result[1])));
                            } else if (animalScript is Wolf)
                            {
                                StartCoroutine(((Wolf)animalScript).Run(new Vector2Int(result[0], result[1])));
                            }
                            animalScript.SetMapPosition(new Vector2Int(result[0], result[1]));
                        }
                    }
                    else
                    {
                        // TODO: Should create a list of rejected partners
                        rejectedPartners.Add(partner);
                        rejectedPartnersCd.Add(forgetTime);
                        partner = null;
                    }
                }
                else if (animalScript.GetGender() == 0)
                {
                    float bonus = ComputeAttractivenessBonus();
                    float chance = Random.Range(0.0f, 1.0f);

                    //Debug.Log("Bonus attractiveness: " + bonus);
                    
                    // If he's attractive tell him that he has found a partner
                    if (partner.animalScript.GetAttractiveness() + bonus > chance)
                    {
                        //Debug.Log("Accepted male partner!");
                        partner.SendFemaleToMaleSignal(this);
                        animalScript.SetSearchingPartnerStatus(true);
                    }
                    // If he's not attractive tell him that you won't accept him
                    else
                    {
                        //Debug.Log("Rejected male partner!");
                        rejectedPartners.Add(partner);
                        rejectedPartnersCd.Add(forgetTime);
                        partner.TellMaleIsRejected(this);
                        partner = null;
                    }

                }

            }
            else
            {
                if (animalScript is Bunny)
                {
                    ((Bunny)animalScript).Wander();
                } else if (animalScript is Wolf)
                {
                    if (!setAverageFemalePosition && !reachedAverageFemalePosition)
                    {
                        averageFemalePosition = EntitiesGeneralInfo.averageFemaleWolfPosition;
                        setAverageFemalePosition = true;
                    } else if (setAverageFemalePosition && !reachedAverageFemalePosition)
                    {
                        List<int> nextDest = Utils.GoToInterestItem(averageFemalePosition, animalScript.GetMapPosition());
                        if (nextDest.Count == 1)
                        {
                            reachedAverageFemalePosition = true;
                        }
                        else
                        {
                            Vector2Int newLocation = new Vector2Int(nextDest[0], nextDest[1]);
                            StartCoroutine(((Wolf)animalScript).Run(newLocation));
                            animalScript.SetMapPosition(newLocation);
                        }

                    } else
                    {
                        ((Wolf)animalScript).Wander();
                    }
                }
            }
        }
        else
        {
            if (setAverageFemalePosition)
            {
                setAverageFemalePosition = false;
            }

            if (reachedAverageFemalePosition)
            {
                reachedAverageFemalePosition = false;
            }

            // Male goes to female
            if (animalScript.GetGender() == 1)
            {
                if (partner == null || partner.partner == null)
                {
                    partner = null;
                    animalScript.SetSearchingPartnerStatus(false);
                    return;
                }

                result = Utils.GoToPartner(partner, partner.animalScript.GetMapPosition(), animalScript.GetMapPosition());

                // It means that something happened and we have no partner anymore
                if (result.Count == 0)
                {
                    animalScript.SetSearchingPartnerStatus(false);
                    partner = null;
                    return;
                } else if (result.Count == 1)
                {
                    partner.SendReproduceSignal();
                    StartCoroutine(Reproduce());
                } else if (result.Count == 2)
                {
                    if (animalScript is Bunny)
                    {
                        StartCoroutine(((Bunny)animalScript).Hop(new Vector2Int(result[0], result[1])));

                    } else if (animalScript is Wolf)
                    {
                        StartCoroutine(((Wolf)animalScript).Run(new Vector2Int(result[0], result[1])));
                    }
                    animalScript.SetMapPosition(new Vector2Int(result[0], result[1]));
                }
            }
            else if (animalScript.GetGender() == 0)
            {
                // If either the partner is dead or he just changed his interest
                // We should shart seaching for a new partner
                if (partner == null || partner.partner == null)
                {
                    animalScript.SetSearchingPartnerStatus(false);
                    partner = null;
                    //Debug.Log("Resetting partner cause my MALE either died or is not interested in me anymore. This is bunny number: " + animalScript.GetAnimalID());
                    return;
                }

                if (animalScript is Bunny)
                {
                    if (((Bunny)partner.animalScript).IsHunted() || ((Bunny)partner.animalScript).IsDying())
                    {
                        animalScript.SetSearchingPartnerStatus(false);
                        partner = null;
                        //Debug.Log("Resetting partner cause my MALE is hunted or dead. This is bunny number: " + animalScript.GetAnimalID());
                        return;
                    }
                }

                // Wait for partner
                Vector3 dir = (partner.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                lookRotation.x = 0;
                lookRotation.z = 0;

                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * animalScript.GetTurnSpeed() / 50.0f);
            }
        }
    }

    // This will be received only by females
    private int SendMaleToFemaleSignal(AnimalsSearchPartner _partner)
    {
        // TODO: Add a bonus to the roll of attractiveness
        // TODO: The bonus should be bigger the smaller the reproductioon urge
        // TODO: This should assure that the bunnies get to mate eventually

        // For debugging; Making sure that when reproducing males interact females
        if (animalScript.GetGender() == 0)
        {
            if (animalScript is Wolf)
            {
                //Debug.Log("MALE WOLF sent signal!");
            }
        }
        else
        {
            Debug.LogError("Male bunny received a male signal!");
        }

        // Add a bonus depending on the urge of the female to male attractiveness
        float bonus = ComputeAttractivenessBonus();
        
        //Debug.Log("Bonus attractiveness: " + bonus);

        // Decide wether the current partner is attractive enough
        float chance = Random.Range(0.0f, 1.0f);
        if (chance < _partner.animalScript.GetAttractiveness() + bonus)
        {
           // Debug.Log("Accepted MALE partner!");
            animalScript.SetSearchingPartnerStatus(true);
            partner = _partner;
            return 1;
        }
        else
        {
           // Debug.Log("Rejected MALE partner!");
            rejectedPartners.Add(_partner);
            rejectedPartnersCd.Add(forgetTime);
            return 0;
        }
    }

    private float ComputeAttractivenessBonus()
    {
        float bonus = 0;
        if (animalScript.GetReproductionSearchPercent() / 2 > animalScript.GetCurrentReproduction())
        {
            bonus = Mathf.InverseLerp(0, animalScript.GetReproductionSearchPercent() / 2, animalScript.GetCurrentReproduction());
            bonus = Mathf.Lerp(0, 0.25f, (1 - bonus));
        }
        return bonus;
    }

    // This will only be received by males
    private void SendFemaleToMaleSignal(AnimalsSearchPartner _partner)
    {
        if (animalScript.GetGender() == 1)
        {
            if (animalScript is Wolf)
            {
                //Debug.Log("FEMALE WOLF sent signal!");
            }

            animalScript.SetSearchingPartnerStatus(true);
            partner = _partner;
        }
        else
        {
            Debug.LogError("A female bunny received a female signal!");
        }
    }

    // This should also be received only by males
    private void TellMaleIsRejected(AnimalsSearchPartner partner)
    {
        rejectedPartners.Add(partner);
        rejectedPartnersCd.Add(forgetTime);
    }

    bool ValidPartner(AnimalsSearchPartner partner)
    {
        // If my partner dissapeared then... oh well
        if (partner == null)
        {
            return false;
        }

        // All the specific conditions for the bunny
        if (partner.animalScript is Bunny)
        {

            Bunny bunny = (Bunny)(partner.animalScript);

            if (bunny.IsDying())
            {
                return false;
            }

            if (bunny.IsHunted())
            {
                return false;
            }

           
        } else if (partner.animalScript is Wolf)
        {
            // All the specific conditions for wolves
        }

        Animals animal = partner.animalScript;

        if (animal == null)
        {
            return false;
        }

        // Generic animalConditions
        if (animal.GetPriority() != 0)
        {
            return false;
        }

        if (animal.IsSearchingFood() || animal.IsEating())
        {
            return false;
        }

        if (animal.IsSearchingForWater() || animal.IsDrinking())
        {
            return false;
        }

        if (rejectedPartners.Contains(partner))
        {
            return false;
        }

        if (animal.IsSearchingForPartner())
        {
            return false;
        }

        if (animal.IsReproducing())
        {
            return false;
        }

        if (animal.IsPregnant())
        {
            return false;
        }

        if (animal.GetCurrentReproduction() > animal.GetReproductionSearchPercent())
        {
            return false;
        }

        if (animal.GetGender() == animalScript.GetGender())
        {
            return false;
        }

        return true;
    }

    

    private void SendReproduceSignal()
    {
        StartCoroutine(Reproduce());
    }

    IEnumerator Reproduce()
    {
        animalScript.SetReproducingStatus(true);
        animalScript.SetSearchingPartnerStatus(false);

        if (partner.partner == null)
        {
            partner = null;
            yield break;
        }

        // Wait for both partners to finish moving
        while (animalScript.IsMoving())
        {
            yield return null;
        }

        while (partner.animalScript.IsMoving())
        {
            yield return null;
        }

        // Rotate towards partner
        Vector3 dir = (partner.transform.position - transform.position).normalized;
        yield return StartCoroutine(Rotate(dir));

        // Move forward towards partner a bit
        Vector3 newTarget = Vector3.zero;
        Vector2Int myMapPos = animalScript.GetMapPosition();
        Vector2Int partnerMapPos = partner.animalScript.GetMapPosition();

        if (Mathf.Abs(myMapPos.x - partnerMapPos.x) + Mathf.Abs(myMapPos.y - partnerMapPos.y) == 2)
        {
            if (animalScript is Bunny)
            {
                newTarget = transform.position + transform.forward * mapScript.cellSize / 1.2f;
            } else if (animalScript is Wolf)
            {
                newTarget = transform.position + transform.forward * mapScript.cellSize / 1.5f;
            }
        }
        else
        {
            if (animalScript is Bunny)
            {
                newTarget = transform.position + transform.forward * mapScript.cellSize / 1.75f;
            } else if (animalScript is Wolf)
            {
                newTarget = transform.position + transform.forward * mapScript.cellSize / 2.25f;
            }
        }

        Vector3 magnitude = newTarget - transform.position;
        dir = magnitude.normalized;
       
        float duration = 0.5f;
        float time = 0;

        while (time < duration)
        {
            transform.position += magnitude * Time.deltaTime;
            //transform.position = new Vector3(transform.position.x, yAmount, transform.position.z);
            time += Time.deltaTime;
            yield return null;
        }

        animalScript.SetPositionStatus(true);

        while (partner!= null && !partner.animalScript.IsPositionSet())
        {
            yield return null;
        }


        // If both of them got to this point we can reset their focus
        animalScript.ResetFocus();

        float timeLeft = animalScript.GetTimeToReproduce();

        while (timeLeft > 0)
        {
            // If anyone is dying or being hunted, they will stop and start runnning
            if (animalScript is Bunny)
            {
                if (((Bunny)animalScript).IsHunted() || ((Bunny)animalScript).IsDying() || partner == null || ((Bunny)partner.animalScript).IsHunted() || ((Bunny)partner.animalScript).IsDying())
                {
                    if (partner != null)
                    {
                        partner.partner = null;
                    }

                    partner = null;
                    break;
                }
            }

            if (partner == null)
            {
                break;
            }

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // We want to make sure that none of them died before they finished reproducing
        // We want to make sure that nobody is hunted since that will break the process
        // In case somebody died, the process is incomplete and there won't be children
        if (partner != null)
        {
            if (animalScript.GetGender() == 0)
            {
                animalScript.SetPregnantStatus(true);
                remainingGestationTime = animalScript.GetGestationTime();
                animalScript.GenerateGeneticsInfo(partner.animalScript);
            }

            animalScript.SetReproducingStatus(false);
            animalScript.SetSearchingPartnerStatus(false);
            animalScript.SetPositionStatus(false);
            animalScript.ResetUrge();
            partner = null;
        }
        // If the bunny died during the act nothing resets; Treat it like it didn't happen
        else
        {
            animalScript.SetReproducingStatus(false);
            animalScript.SetSearchingPartnerStatus(false);
            animalScript.SetPositionStatus(false);
            partner = null;
        }
        
        yield return null;
    }

    IEnumerator Rotate(Vector3 dir)
    {
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
    }

    IEnumerator Birth()
    {
        animalScript.SetGivingBirthStatus(true);
        animalScript.SetPregnantStatus(false);

        int numberOfChildren = 0;

        if (animalScript is Bunny)
        {
            numberOfChildren = Random.Range(3, 6);
        } else if (animalScript is Wolf)
        {
            numberOfChildren = Random.Range(2, 4);
        }

        for (int i = 0; i < numberOfChildren; ++i)
        {
            if (animalScript is Bunny)
            {
                if (((Bunny)animalScript).IsDying())
                {
                    break;
                }
            }
            mapScript.SpawnNewLife(animalScript);
            yield return new WaitForSeconds(Random.Range(1, 3));
        }

        yield return null;

        animalScript.SetGivingBirthStatus(false);
    }

    public AnimalsSearchPartner GetPartner()
    {
        return partner;
    }
}

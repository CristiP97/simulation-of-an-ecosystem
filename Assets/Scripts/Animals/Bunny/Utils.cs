using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    private static Test mapScript = Test.instance;
    private static List<Vector2Int> directions = new List<Vector2Int>
    {
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1)
    };
       

    // Function that receives list of coordinates and returns the closest one to the position provided
    // Converts the list of coordinates from map coordinates to world coordinates
    public static Vector2Int GetClosestInterestItem(List<Vector2Int> mapItemLocations, out int index, Vector3 position)
    {
        float minDist;
        Vector2Int candidate;

        // Compute distance for first candidate and use it as reference
        Vector3 target = mapScript.ConvertMapPositionToWorldPosition(mapItemLocations[0]);
        Vector3 dir = target - position;

        minDist = dir.magnitude;
        candidate = mapItemLocations[0];
        index = 0;

        // Search for the closest item
        for (int i = 1; i < mapItemLocations.Count; ++i)
        {
            target = mapScript.ConvertMapPositionToWorldPosition(mapItemLocations[i]);
            dir = target - position;

            if (dir.magnitude < minDist)
            {
                minDist = dir.magnitude;
                candidate = mapItemLocations[i];
                index = i;
            }
        }

        return candidate;
    }

    public static Vector2Int GetFarthestPositionFromTarget(List<Vector2Int> options, Vector2Int myPosition, Vector2Int threatPosition)
    {
        float maxDist;
        List<Vector2Int> candidates = new List<Vector2Int>();

        // Compute distance for first candidate and use it as reference
        Vector3 target = mapScript.ConvertMapPositionToWorldPosition(options[0]);
        Vector3 threatWorldPos = mapScript.ConvertMapPositionToWorldPosition(threatPosition);
        Vector3 dir = target - threatWorldPos;

        maxDist = dir.magnitude;
        candidates.Add(options[0]);

        // Search for the closest item
        for (int i = 1; i < options.Count; ++i)
        {
            target = mapScript.ConvertMapPositionToWorldPosition(options[i]);
            dir = target - threatWorldPos;

            if (dir.magnitude > maxDist)
            {
                maxDist = dir.magnitude;
                candidates.Clear();
                candidates.Add(options[i]);
            } else if (dir.magnitude == maxDist)
            {
                candidates.Add(options[i]);
            }
        }

        if (candidates.Count == 1)
        {
            return candidates[0];
        } else
        {
            return candidates[Random.Range(0, candidates.Count)];
        }
    }

    // Function that returns a valid move in the process of searching for something
    public static Vector2Int SearchTiles(List<Vector2Int> memory, Vector2Int mapPos, Transform transform, int memorySize, float searchAngle)
    {
        Vector2Int result = new Vector2Int(0,0);
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        
        // Add the current tile because i didn't see anything of interest
        memory.Add(mapPos);

        // Remove the first tiles added to create new space if memory is exceeded
        if (memory.Count > memorySize)
        {
            memory.RemoveAt(0);
        }

        // Get the all the valid moves that would encourage me to explore
        GetValidMovesAdvanced(availableMoves, mapPos, transform, searchAngle);

        // Filter the moves based on the bunny's memory
        availableMoves = MemoryFilterMoves(availableMoves, memory);

        // Select random move from available filtered moves
        if (availableMoves.Count > 0)
        {
            int index = Random.Range(0, availableMoves.Count);

            //StartCoroutine(Hop(availableMoves[index]));
            //position = availableMoves[index];

            result = availableMoves[index];
        }
        else
        {
            // Choose a random option since we don't seem to have a choice
            GetValidMoves(availableMoves, mapPos);
            if (availableMoves.Count > 0)
            {
                int index = Random.Range(0, availableMoves.Count);

                //StartCoroutine(Hop(availableMoves[index]));
                //position = availableMoves[index];
                result = availableMoves[index];
            }
            else // This means that the bunny is blocked somewhere
            {
                // TODO: Do something about the blocked bunny
            }
        }

        return result;
    }

    // Filters moves based on the memory of the bunny
    private static List<Vector2Int> MemoryFilterMoves(List<Vector2Int> availableMoves, List<Vector2Int> memory)
    {
        List<Vector2Int> filteredMoves = new List<Vector2Int>();

        for (int i = 0; i < availableMoves.Count; ++i)
        {
            if (!memory.Contains(availableMoves[i]))
            {
                filteredMoves.Add(availableMoves[i]);
            }
        }

        return filteredMoves;
    }

    // Retrieves all the valid moves, keeping in mind not going back
    private static void GetValidMovesAdvanced(List<Vector2Int> moves, Vector2Int mapPos, Transform transform, float advancedSearchAngle)
    {
        Vector2Int currentCoord;

        // Clear whatever we had before
        moves.Clear();

        for (int i = 0; i < directions.Count; ++i)
        {
            currentCoord = mapPos + directions[i];
            if (mapScript.CheckValidMovementTile(currentCoord))
            {
                // Check for the angle; if it's too big it means we are going back
                Vector3 worldCoord = mapScript.ConvertMapPositionToWorldPosition(currentCoord);
                worldCoord = (worldCoord - transform.position).normalized;

                //Debug.Log("Angle between: " + worldCoord + " and " + transform.forward + " is: " + Vector3.Angle(worldCoord, transform.forward));
                if (Vector3.Angle(worldCoord, transform.forward) < advancedSearchAngle)
                {
                    moves.Add(currentCoord);
                }

            }
        }
    }

    // Retrieves all the valid moves that the bunny has
    public static void GetValidMoves(List<Vector2Int> moves, Vector2Int mapPos)
    {
        Vector2Int currentCoord;

        // Clear whatever we had before
        moves.Clear();

        for (int i = 0; i < directions.Count; ++i)
        {
            currentCoord = mapPos + directions[i];
            if (mapScript.CheckValidMovementTile(currentCoord))
            {
                moves.Add(currentCoord);
            }
        }
    }

    // Find the shortest path to the target
    public static List<int> GoToInterestItem(Vector2Int target, Vector2Int mapPos)
    {
        List<int> result = new List<int>();
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        for (int i = 0; i < directions.Count; ++i)
        {
            if (mapPos + directions[i] == target)
            {
                //StartCoroutine(coroutine);
                result.Add(1);
                return result;
            }
        }

        GetValidMoves(availableMoves, mapPos);

        if (availableMoves.Count > 0)
        {

            List<Vector2Int> closestMoves = new List<Vector2Int>();
            GetClosestMoves(availableMoves, closestMoves, target);

            if (closestMoves.Count > 1)
            {
                int index = Random.Range(0, closestMoves.Count);

                //StartCoroutine(Hop(closestMoves[index]));
                //position = closestMoves[index];
                result.Add(closestMoves[index].x);
                result.Add(closestMoves[index].y);
                return result;
            }
            else
            {
                //StartCoroutine(Hop(closestMoves[0]));
                //position = closestMoves[0];
                result.Add(closestMoves[0].x);
                result.Add(closestMoves[0].y);
                return result;
            }
        }

        return result;
    }

    private static void GetClosestMoves(List<Vector2Int> availableMoves, List<Vector2Int> closestMoves, Vector2Int targetMapCoord)
    {
        int candidateDist;
        int minDist = Mathf.Abs(targetMapCoord.x - availableMoves[0].x) + Mathf.Abs(targetMapCoord.y - availableMoves[0].y);
        closestMoves.Add(availableMoves[0]);

        for (int i = 1; i < availableMoves.Count; ++i)
        {
            candidateDist = Mathf.Abs(targetMapCoord.x - availableMoves[i].x) + Mathf.Abs(targetMapCoord.y - availableMoves[i].y);
            if (candidateDist == minDist)
            {
                closestMoves.Add(availableMoves[i]);
            }
            else if (candidateDist < minDist)
            {
                closestMoves.Clear();
                minDist = candidateDist;
                closestMoves.Add(availableMoves[i]);
            }
        }
    }

    //public static List<int> GoToPartner(BunnySearchPartner partner, Vector2Int partnerPos, Vector2Int mapPos)
    //{
    //    List<int> result = new List<int>();
    //    List<Vector2Int> availableMoves = new List<Vector2Int>();

    //    // If either our partner is dead or simply not interested anymore
    //    // We have no valid moves and should start looking for another partner
    //    if (partner == null || partner.GetPartner() == null)
    //    {
    //        return result;
    //    }

    //    for (int i = 0; i < directions.Count; ++i)
    //    {
    //        if (mapPos + directions[i] == partnerPos)
    //        {
    //            result.Add(1);
    //            return result;

    //            //partner.SendReproduceSignal();
    //            //StartCoroutine(Reproduce());
    //            //return;
    //        }
    //    }

    //    GetValidMoves(availableMoves, mapPos);

    //    if (availableMoves.Count > 0)
    //    {

    //        List<Vector2Int> closestMoves = new List<Vector2Int>();
    //        GetClosestPartnerMoves(availableMoves, closestMoves, partnerPos);

    //        if (closestMoves.Count > 1)
    //        {
    //            int index = Random.Range(0, closestMoves.Count);

    //            result.Add(closestMoves[index].x);
    //            result.Add(closestMoves[index].y);
    //            return result;

    //            //StartCoroutine(Hop(closestMoves[index]));
    //            //position = closestMoves[index];
    //        }
    //        else
    //        {
    //            result.Add(closestMoves[0].x);
    //            result.Add(closestMoves[0].y);
    //            return result;

    //            //StartCoroutine(Hop(closestMoves[0]));
    //            //position = closestMoves[0];
    //        }
    //    }

    //    return result;
    //}

    public static List<int> GoToPartner(AnimalsSearchPartner partner, Vector2Int partnerPos, Vector2Int mapPos)
    {
        List<int> result = new List<int>();
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        // If either our partner is dead or simply not interested anymore
        // We have no valid moves and should start looking for another partner
        if (partner == null || partner.GetPartner() == null)
        {
            return result;
        }

        for (int i = 0; i < directions.Count; ++i)
        {
            if (mapPos + directions[i] == partnerPos)
            {
                result.Add(1);
                return result;

            }
        }

        GetValidMoves(availableMoves, mapPos);

        if (availableMoves.Count > 0)
        {

            List<Vector2Int> closestMoves = new List<Vector2Int>();
            GetClosestPartnerMoves(availableMoves, closestMoves, partnerPos);

            if (closestMoves.Count > 1)
            {
                int index = Random.Range(0, closestMoves.Count);

                result.Add(closestMoves[index].x);
                result.Add(closestMoves[index].y);
                return result;

            }
            else
            {
                result.Add(closestMoves[0].x);
                result.Add(closestMoves[0].y);
                return result;
            }
        }

        return result;
    }

    private static void GetClosestPartnerMoves(List<Vector2Int> availableMoves, List<Vector2Int> closestMoves, Vector2Int partnerPos)
    {
        int candidateDist;
        int minDist = Mathf.Abs(partnerPos.x - availableMoves[0].x) + Mathf.Abs(partnerPos.y - availableMoves[0].y);
        closestMoves.Add(availableMoves[0]);

        for (int i = 1; i < availableMoves.Count; ++i)
        {
            candidateDist = Mathf.Abs(partnerPos.x - availableMoves[i].x) + Mathf.Abs(partnerPos.y - availableMoves[i].y);
            if (candidateDist == minDist)
            {
                closestMoves.Add(availableMoves[i]);
            }
            else if (candidateDist < minDist)
            {
                closestMoves.Clear();
                minDist = candidateDist;
                closestMoves.Add(availableMoves[i]);
            }
        }
    }

    public Vector2Int ClosestLandTile(Vector2Int reference)
    {
        List<Vector2Int> queue = new List<Vector2Int>();
        List<Vector2Int> explored = new List<Vector2Int>();
        queue.Add(reference);

        while (queue.Count > 0)
        {
            Vector2Int current = queue[0];
            queue.RemoveAt(0);

            if (mapScript.GetMapValue(current) == 1)
            {
                return current;
            }

            explored.Add(current);

            List<Vector2Int> neighbours = mapScript.GetMapNeighbours(reference);

            for (int i = 0; i < neighbours.Count; ++i)
            {
                if (!explored.Contains(neighbours[i]))
                {
                    queue.Add(neighbours[i]);
                }
            }
        }

        return reference;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyMovement : MonoBehaviour
{
    //TODO: RECHECK LOGIC TO SEE IF WE KEEP THIS

    private Test mapScript;
    private Bunny bunnyScript;

    // Start is called before the first frame update
    void Start()
    {
        mapScript = Test.instance;
        bunnyScript = gameObject.GetComponent<Bunny>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Hop(Vector2Int target)
    {
        bunnyScript.SetMovingStatus(true);

        Vector3 updatedTarget = mapScript.ConvertMapPositionToWorldPosition(target);
        Vector3 distance = updatedTarget - transform.position;
        Vector3 direction = distance.normalized;

        yield return StartCoroutine(Rotate(direction));

        float initialY = transform.position.y;
        float jumpDuration = 1.0f / bunnyScript.GetCurrentMovementSpeed();

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

        bunnyScript.SetMovingStatus(false);
    }

    IEnumerator Rotate(Vector3 dir)
    {
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
    }

}

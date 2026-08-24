using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatationAnim : MonoBehaviour
{
    public float ratationSpeed;

    // Update is called once per frame
    void Update()
    {
        Vector3 angle = transform.localEulerAngles;
        angle.z += Time.deltaTime * ratationSpeed;
        transform.localEulerAngles = angle;
    }
}

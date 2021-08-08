using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAngle : MonoBehaviour{
  public float width = 4;

  void Start() {
    GetComponent<Camera>().fieldOfView = Mathf.Atan(width / 2 / -transform.position.z) * 2
      * Mathf.Rad2Deg;
  }
}

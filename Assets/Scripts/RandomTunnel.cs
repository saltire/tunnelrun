using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RandomTunnel : MonoBehaviour {
  public int queueLength = 3;
  public int width = 5;

  [Range(0, 1)]
  public float curveChance = .5f;
  public Vector2Int radiusRange = new Vector2Int(4, 20);
  public Vector2Int straightLengthRange = new Vector2Int(1, 20);

  int curveDirection = 1;

  CameraMove cameraMove;
  MeshCreator meshCreator;

  void Start() {
    cameraMove = FindObjectOfType<CameraMove>();
    meshCreator = FindObjectOfType<MeshCreator>();

    AddRandomTunnels();
  }

  void Update() {
    AddRandomTunnels();
  }

  void AddRandomTunnels() {
    while (cameraMove.tunnels.Count < queueLength) {
      int radius = Random.Range(0f, 1f) > curveChance
        ? Random.Range(radiusRange.x, radiusRange.y) * curveDirection
        : 0;
      int length = radius != 0
        ? (int)Mathf.Round(Mathf.Abs(radius) * 1.25f)
        : Random.Range(straightLengthRange.x, straightLengthRange.y);

      if (radius != 0) {
        curveDirection *= -1;
      }

      meshCreator.CreateTunnel(width, length, radius);
    }
  }
}

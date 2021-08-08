using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {
  public float tilesPerSecond = 3;
  public List<Tunnel> initialTunnels = new List<Tunnel>();
  public Queue<Tunnel> tunnels = new Queue<Tunnel>();

  float tunnelDistance = 0;

  float qt = Mathf.PI / 2;

  void Awake() {
    tunnels.Clear();
    foreach (Tunnel tunnel in initialTunnels) {
      tunnels.Enqueue(tunnel);
    }
  }

  void Update() {
    float moveDistance = Time.deltaTime * tilesPerSecond;

    Tunnel tunnel = tunnels.Peek();

    float tunnelDistanceRemaining = tunnel.length - tunnelDistance;

    if (moveDistance > tunnelDistanceRemaining) {
      moveDistance -= tunnelDistanceRemaining;
      tunnels.Dequeue();

      if (tunnels.Count == 0) {
        foreach (Tunnel t in initialTunnels) {
          tunnels.Enqueue(t);
        }
      }

      tunnel = tunnels.Peek();
      tunnelDistance = 0;
      tunnelDistanceRemaining = tunnel.length;
    }

    tunnelDistance += moveDistance;
    float tunnelPercent = tunnelDistance / tunnel.length;

    if (tunnel.radius == 0) {
      transform.position = tunnel.start + (tunnel.end - tunnel.start) * tunnelPercent;
    }
    else {
      transform.position = tunnel.origin + (tunnel.transform.rotation
        * new Vector3(
          Mathf.Cos(qt * tunnelPercent) * Mathf.Sign(tunnel.radius), 0,
          Mathf.Sin(qt * tunnelPercent))
        * Mathf.Abs(tunnel.radius));

      transform.rotation = tunnel.transform.rotation
        * Quaternion.AngleAxis(-90f * tunnelPercent * Mathf.Sign(tunnel.radius), Vector3.up);
    }
  }
}

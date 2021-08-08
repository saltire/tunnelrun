using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {
  public float tilesPerSecond = 3;
  public List<Tunnel> initialTunnels = new List<Tunnel>();
  public Queue<Tunnel> tunnels = new Queue<Tunnel>();

  float tunnelDistance = 0;

  Quaternion startRotation;
  Quaternion endRotation;

  void Awake() {
    tunnels.Clear();
    foreach (Tunnel tunnel in initialTunnels) {
      tunnels.Enqueue(tunnel);
    }

    startRotation = transform.rotation;
    endRotation = startRotation
      * Quaternion.AngleAxis(90, tunnels.Peek().origin.rotation * Vector3.up);
  }

  void Update() {
    float moveDistance = Time.deltaTime * tilesPerSecond;

    Tunnel tunnel = tunnels.Peek();

    float tunnelDistanceRemaining = tunnel.length - tunnelDistance;

    if (moveDistance > tunnelDistanceRemaining) {
      moveDistance -= tunnelDistanceRemaining;
      tunnels.Dequeue();

      // Requeue the initial tunnels, assuming they are in a loop.
      if (tunnels.Count == 0) {
        foreach (Tunnel t in initialTunnels) {
          tunnels.Enqueue(t);
        }
      }

      tunnel = tunnels.Peek();
      tunnelDistance = 0;
      tunnelDistanceRemaining = tunnel.length;

      startRotation = endRotation;
      endRotation = Quaternion.AngleAxis(90, tunnel.origin.rotation * Vector3.up) * startRotation;
    }

    tunnelDistance += moveDistance;
    float tunnelPercent = tunnelDistance / tunnel.length;

    if (tunnel.radius == 0) {
      transform.position = Vector3.Lerp(tunnel.start.position, tunnel.end.position, tunnelPercent);
    }
    else {
      transform.position = tunnel.start.position;
      transform.rotation = startRotation;
      transform.RotateAround(tunnel.origin.position, tunnel.origin.rotation * Vector3.up,
        90 * tunnelPercent);
    }
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    Camera camera;
    void Awake() {
        camera = Camera.main;
    }

    void Update() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        camera.transform.RotateAround(transform.position, Vector3.up, horizontal);
        camera.transform.RotateAround(transform.position, camera.transform.TransformDirection(Vector3.right), vertical);
        Vector3 eulerAngles = camera.transform.eulerAngles;
        camera.transform.eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, 0);

        float zoom = Input.GetAxis("Zoom");
        camera.transform.transform.Translate(Vector3.forward * zoom * 0.5F);
    }
}

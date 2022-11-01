using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalsDebug : MonoBehaviour {

    Mesh mesh;
    Vector3[] vertices;
    Vector3[] normals;

    void Start() {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        vertices = mesh.vertices;
        normals = mesh.normals;

        print(vertices.Length);
        print(normals.Length);
    }
    
    void OnDrawGizmos() {

        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(vertices[i] + transform.position, normals[i]);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(vertices[i], 0.01F);
        }
    }
}

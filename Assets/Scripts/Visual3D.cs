using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extension;
using System;

public enum AnimationSection {Top,Slice,Bottom}
public class Visual3D : MonoBehaviour {

    [SerializeField]
    Square1 square1;

    [SerializeField]
    Visual2D visual2D;

    [SerializeField]
    GameObject cornerPrefab;
    [SerializeField]
    GameObject edgePrefab;
    [SerializeField]
    GameObject centerPrefab;

    [SerializeField]
    public float animationSpeed;

    [SerializeField]
    float layerSeperation;

    Dictionary<int, Color> intToColor;

    GameObject rightCenter;

    Dictionary<Piece, GameObject> pieceToObject;

    Vector3 seamNormal;
    
    bool animationPlaying;
    class AnimationRequest {
        public List<Piece> Pieces;
        public AnimationSection Section;
        public int Steps;
        public AnimationRequest(List<Piece> pieces, AnimationSection section, int steps)  {
            Pieces = pieces;
            Section = section;
            Steps = steps;
        }
    }

    Queue<AnimationRequest> AnimationQueue;

    public void AddAnimation(List<Piece> pieces, AnimationSection section, int steps) {
        if (steps != 0) {
            AnimationQueue.Enqueue(new AnimationRequest(new List<Piece>(pieces), section, steps));
        }
    }

    IEnumerator PlayAnimationCoroutine(Vector3 axis, int steps, List<GameObject> pieceObjects) {
        animationPlaying = true;
        float degreesPerFrame = 0;
        int totalDegrees = steps * 30;
        //Avoids division by 0 errors
        if (animationSpeed > 0) {
            degreesPerFrame = totalDegrees * Time.deltaTime / animationSpeed;
        } else {
            degreesPerFrame = steps * 30;
        }
        int framesNeeded = Mathf.FloorToInt(steps * 30 / degreesPerFrame);
        for (int i = 0; i < framesNeeded; i++) {
            foreach(GameObject pieceObject in pieceObjects) {
                pieceObject.transform.RotateAround(transform.position, axis, degreesPerFrame);
            }
            yield return null;
        }
        //Goes the remaining angle without going a full degreesPerFrame
        foreach (GameObject pieceObject in pieceObjects) {
            pieceObject.transform.RotateAround(transform.position, axis, steps * 30 - (degreesPerFrame * framesNeeded));
        }
        animationPlaying = false;
    }

    void PlayAnimation() {
        if (!animationPlaying && AnimationQueue.Count != 0) {
            AnimationRequest animation = AnimationQueue.Dequeue();
            AnimationSection section = animation.Section;
            //The angle of the seam used is the direction of the normals of the center mesh
            //that don't point directly along the x, y, or z axis
            Vector3 normal = new Vector3(-seamNormal.x, seamNormal.y, seamNormal.z);
            Quaternion rotation = Quaternion.AngleAxis((int)section * 90, Quaternion.Euler(0, 90, 0) * normal);
            Vector3 axis = rotation * Vector3.up;
            List<GameObject> pieceObjects = new List<GameObject>();
            foreach(Piece piece in animation.Pieces) {
                if (pieceToObject.ContainsKey(piece)) {
                    pieceObjects.Add(pieceToObject[piece]);
                //Checks again but for swapped side colors
                } else {
                    int[] sideColorsCopy = piece.SideColors;
                    Array.Reverse(sideColorsCopy);
                    Piece similarPiece = new Piece(piece.Shape, piece.LayerColor, sideColorsCopy);
                    if (pieceToObject.ContainsKey(similarPiece)) {
                        pieceObjects.Add(pieceToObject[similarPiece]);
                    }
                }
            }
            
            if (section == AnimationSection.Slice) {
                pieceObjects.Add(rightCenter);
            }
            StartCoroutine(PlayAnimationCoroutine(axis, animation.Steps, pieceObjects));
        }
    }

    void Start() {
        animationPlaying = false;

        //Intializes queue
        AnimationQueue = new Queue<AnimationRequest>();

        //Intializes piece dictionary
        pieceToObject = new Dictionary<Piece, GameObject>();

        intToColor = visual2D.intToColor;

        //Gets the seam normal
        Vector3[] normals = centerPrefab.GetComponent<MeshFilter>().sharedMesh.normals;
        foreach(Vector3 normal in normals) {
            int nonZeroValues = 0;
            for (int i = 0; i < 3; i++) {
                if (normal[i] != 0) {
                    nonZeroValues++;
                }
            }
            if (nonZeroValues == 2) {
                seamNormal = normal;
            }
        }

        //Initializes the piece of each layer
        void IntializeLayerPieces(List<Piece> layer, int pieceIndex, int posDir, int textureDir, Vector3 offset, Vector3 rotations, Vector3 cornerRotations) {
            int steps = 0;
            for (int i = 0; i < layer.Count; i++) {
                Piece piece = layer[i];
                //Generates texture
                Texture2D texture = new Texture2D(1, 4, TextureFormat.RGBA32, false);
                //The filter mode is point to avoid mixing between the colors
                texture.filterMode = FilterMode.Point;
                Color[] colors = new Color[4];
                colors[0] = Color.black;
                int usingCorner = piece.Shape - 1;
                for (int c = 0; c < piece.Shape; c++) {
                    //If the piece is a corner it uses ((2 - ((posDir + 1) / 2) + (c * posDir)) to get the correct order of the colors for the direction of the layer
                    //Else if the piece is an edge it fills the 2nd index of the colors array with the right color
                    //colors[((2 - ((posDir + 1) / 2) + (c * posDir)) * usingCorner) + (2 * (1 - usingCorner))] = intToColor[piece.SideColors[c]];
                    //colors[((2 - c) * usingCorner) + (2 * (1 - usingCorner))] = intToColor[piece.SideColors[c]];
                    colors[((2 - ((textureDir + 1) / 2) + (c * textureDir)) * usingCorner) + (2 * (1 - usingCorner))] = intToColor[piece.SideColors[c]];
                }
                colors[3] = intToColor[piece.LayerColor];
                texture.SetPixels(colors);
                texture.Apply();

                GameObject pieceObject = Instantiate(new GameObject[] { edgePrefab, cornerPrefab}[piece.Shape - 1]);
                pieceToObject[piece] = pieceObject;
                pieceObject.transform.position = transform.position + offset;
                pieceObject.transform.SetParent(transform);
                pieceObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
                pieceObject.transform.rotation = Quaternion.Euler(new Vector3(0, steps * 30 * posDir, 180) + rotations + (cornerRotations * usingCorner));
                steps += piece.Shape;
            }
        }
        //Problem: The corner mesh isn't alligned through the z axis like the edge so rotating it along there rotates it around that axis
        //having different uvs to avoid having to flip everything by 180 could be a possible solution
        IntializeLayerPieces(square1.top, 0, -1, -1, Vector3.up * layerSeperation, Vector3.zero, Vector3.zero);
        IntializeLayerPieces(square1.bottom, 0, -1, 1, Vector3.down * layerSeperation, new Vector3(0, 0, -180), new Vector3(0, -30, 0));

        //Intializes the middle pieces
        Color[] centerColors = new Color[] { Color.red, Color.green, Color.Lerp(Color.red, Color.yellow, 0.5F), Color.blue };
        for (int i = 0; i < 2; i++) {
            //Creates the texture
            Texture2D texture = new Texture2D(1, 4, TextureFormat.RGBA32, false);
            //The filter mode is point to avoid mixing between the colors
            texture.filterMode = FilterMode.Point;
            Color[] colors = new Color[4];
            colors[0] = Color.black;
            int start = (i * 3) - i;
            int length = centerColors.Length;
            colors[3] = centerColors[start];
            colors[2] = centerColors[(start + 1) % length];
            colors[1] = centerColors[(start + 2) % length];

            texture.SetPixels(colors);
            texture.Apply();

            GameObject center = Instantiate(centerPrefab);
            center.transform.SetParent(transform);
            center.transform.position = transform.position;
            center.transform.RotateAround(transform.position, Vector3.up, 180 * i);
            center.GetComponent<MeshRenderer>().material.mainTexture = texture;

            if (i == 0) {
                rightCenter = center;
            }
        }       
    }

    void Update() {
        PlayAnimation();
    }
}

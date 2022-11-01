using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Visual2D : MonoBehaviour {
    [SerializeField]
    public Square1 square1;
    [SerializeField]
    Texture2D cornerTexture;
    [SerializeField]
    Texture2D edgeTexture;
    [SerializeField]
    int size;
    [SerializeField]
    float layerDist;

    public bool on;
    List<Piece> previousTop;
    List<Piece> previousBot;

    public Dictionary<int, Color> intToColor;
    
    List<int>[] layerPixels;
    List<int>[][] sideColorPixels;
    void Awake() {
        on = true;
        //Intializes colors
        intToColor = new Dictionary<int, Color>();
        intToColor[(int)Colors.Yellow] = Color.yellow;
        intToColor[(int)Colors.White] = Color.white;
        intToColor[(int)Colors.Red] = Color.red;
        intToColor[(int)Colors.Green] = Color.green;
        intToColor[(int)Colors.Orange] = Color.Lerp(Color.red, Color.yellow, 0.5F);
        intToColor[(int)Colors.Blue] = Color.blue;

        //Gets the positions of the pixels that need to be replaced and stores them
        layerPixels = new List<int>[2];
        sideColorPixels = new List<int>[2][];
        sideColorPixels[0] = new List<int>[2];
        sideColorPixels[1] = new List<int>[2];
        layerPixels[0] = new List<int>();
        layerPixels[1] = new List<int>();
        sideColorPixels[0][0] = new List<int>();
        sideColorPixels[0][1] = new List<int>();
        sideColorPixels[1][0] = new List<int>();
        sideColorPixels[1][1] = new List<int>();

        Color[] cornerPixels = cornerTexture.GetPixels();
        Color[] edgePixels = edgeTexture.GetPixels();
        if (cornerPixels.Length != edgePixels.Length) {
            throw new Exception("Corner texture and edge texture aren't the same size");
        }
        for (int i = 0; i < cornerPixels.Length; i++) {
            Color cornerPixel = cornerPixels[i];
            Color edgePixel = edgePixels[i];
            //Corner
            //White becomes the layer color
            if (cornerPixel == Color.white) {
                layerPixels[1].Add(i);
            }
            //Red becomes the first side color
            if (cornerPixel == Color.red) {
                sideColorPixels[1][0].Add(i);
            }
            //Green becomes the second side color which is only present on corners
            if (cornerPixel == Color.green) {
                sideColorPixels[1][1].Add(i);
            }
            //Edge
            //White becomes the layer color
            if (edgePixel == Color.white) {
                layerPixels[0].Add(i);
            }
            //Red becomes the side color
            if (edgePixel == Color.red) {
                sideColorPixels[0][0].Add(i);
            }
        }
        previousTop = square1.top;
        previousBot = square1.bottom;

        UpdateVisual();
    }

    public void UpdateVisual() {
        //Removes the objects that were used to show the previous state of the cube
        List<GameObject> previous = new List<GameObject>();
        foreach(Transform child in transform) {
            previous.Add(child.gameObject);
        }
        for (int i = 0; i < previous.Count; i++) {
            Destroy(previous[i]);
        }
        void DrawLayer(List<Piece> layer, Vector3 offset, Vector2 flips) {
            int steps = 0;
            for (int i = 0; i < layer.Count; i++) {
                //Gets the right texture for the shape
                Piece piece = layer[i];
                Texture2D texture = new Texture2D(512, 512);
                switch (piece.Shape) {
                    case (int)Shapes.Corner :
                        texture.SetPixels(cornerTexture.GetPixels());
                        break;

                    case (int)Shapes.Edge :
                        texture.SetPixels(edgeTexture.GetPixels());
                        break;
                }
                //Replaces certain pixels which are color coded
                Color[] pixels = texture.GetPixels();
                
                int shape = piece.Shape - 1;
                foreach(int p in layerPixels[shape]) {
                    pixels[p] = intToColor[piece.LayerColor];
                }
                
                for (int s = 0; s < piece.Shape; s++) {
                    foreach(int p in sideColorPixels[shape][s]) {
                        pixels[p] = intToColor[piece.SideColors[s]];
                    }
                }
                
                texture.SetPixels(pixels);
                texture.Apply();
                
                GameObject imageObject = new GameObject();
                imageObject.transform.SetParent(transform);
                imageObject.transform.position = transform.position + offset + (new Vector3(size, -size) / 2);
                imageObject.transform.RotateAround(transform.position + offset, Vector3.back, steps * -30);
                //Manual adjustments
                imageObject.transform.RotateAround(transform.position + offset, Vector3.right, flips.x * 180);
                imageObject.transform.RotateAround(transform.position + offset, Vector3.up, flips.y * 180);
                Image image = (Image)imageObject.AddComponent(typeof(Image));
                image.rectTransform.sizeDelta = new Vector2(size, size);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width, texture.height) / 2, 100, 0, SpriteMeshType.FullRect);

                steps += piece.Shape;
            }
        }
        if (on) {
            DrawLayer(square1.top, new Vector3(0, layerDist / 2), Vector2.zero);
            DrawLayer(square1.bottom, new Vector3(0, -layerDist / 2), new Vector2(1, 0));
        }
    }

    void LateUpdate() {       
        //Checks for changes to the square1 after turns and slices has been executed
        if (square1.top != previousTop || square1.bottom != previousBot) {
            UpdateVisual();
            previousTop = square1.top;
            previousBot = square1.bottom;
        }
        
    }
}
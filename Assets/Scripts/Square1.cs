using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Extension;
using System.Linq;

public enum Shapes {Edge = 1, Corner = 2}
public enum Colors {Yellow, White, Red, Green, Orange, Blue}
public class Piece : IEquatable<Piece> {
    public int Shape;
    public int LayerColor;
    public int[] SideColors;
    public Piece(int shape, int layerColor, int[] sideColors) {
        Shape = shape;
        LayerColor = layerColor;
        SideColors = sideColors;
    }
    public bool Equals(Piece other) {
        bool SameColors(int[] a, int[] b) {
            if (a.Length != b.Length) {
                return false;
            }
            foreach(int value in a ) {
                if (!b.Contains(value)) {
                    return false;
                }
            }
            return true;
        }
        return Shape == other.Shape && LayerColor == other.LayerColor && SameColors(SideColors, other.SideColors);
    }
}
public class Square1 : MonoBehaviour {
    //The visuals to update
    [SerializeField]
    Visual3D visual3D;
    [SerializeField]
    UIControl UIControl;

    //The first element is the piece right of the slice
    public List<Piece> top;
    public List<Piece> bottom;

    public bool centerSquare;
    public bool debugOn;
    
    void Awake() { 
        //Intializes the middle layer
        centerSquare = true;
        //Intializes the top and bottom layers
        int[] allSideColors = new int[] {(int)Colors.Red, (int)Colors.Green, (int)Colors.Orange, (int)Colors.Blue};
        void IntializeLayer(ref List<Piece> layer, int layerColor) {
            layer = new List<Piece>();
            for (int i = 0; i < 8; i++) {
                int shape = 2 - ((i + 1) % 2);
                int[] sideColors = new int[shape];
                for (int c = 0; c < shape; c++) {
                    sideColors[c] = allSideColors[(Mathf.FloorToInt(i / 2) + c) % 4];
                }
                layer.Add(new Piece(shape, layerColor, sideColors));
            }
        }
        IntializeLayer(ref top, (int)Colors.Yellow);
        IntializeLayer(ref bottom, (int)Colors.White);
    }

    void Start() {
        debugOn = false;
        print(Time.realtimeSinceStartup);
    }

    //Returns the steps between two pieces in a layer
    public int StepsBetween(int a, int b, List<Piece> layer, int dir = 1, int countA = 0, int countB = 0) {
        int steps = countA * layer[a].Shape;
        while (a.mod(layer.Count) != (b - dir).mod(layer.Count)) {
            a = (a + dir).mod(layer.Count);
            steps += layer[a].Shape;
        }
        steps += countB * layer[b].Shape;
        return steps;
    }

    //Returns the piece that is an amount of steps before or after another piece
    public Piece InSteps(int a, int steps, List<Piece> layer, out int i, int countA = 0) {
        int stepsChecked = layer[a].Shape * countA;
        int stepsSign = Math.Sign(steps);
        //Iterates through the list of pieces until it has passed enough steps
        i = (a + stepsSign).mod(layer.Count);
        while(stepsChecked < Math.Abs(steps)) {
            stepsChecked += layer[i].Shape;
            if (stepsChecked < Math.Abs(steps)) {
                i = (i + stepsSign).mod(layer.Count);
            }
        }
        return layer[i];
    }

    //Overloaded method that doesn't have out i
    public Piece InSteps(int a, int steps, List<Piece> layer, int countA = 0) {
        return InSteps(a, steps, layer, out int trash, countA);
    }

    //Changes the # of steps so that it would match the directions used when turning a layer
    public int CorrectStepDir(int steps, List<Piece> layer) {
        if (layer == top) {
            return steps * -1;
        } else return steps;
        //i accidently forgot the {} and now i'm questioning everything i know
    }

    //Changes the number of steps turned so that it has the least magnitude needed
    public int OptimizeSteps(int steps) {
        if (Math.Abs(steps) > 6) {
            return steps - 12;
        } else {
            return steps;
        }
    }

    //Manipulation Methods
    public void Rotate(int topSteps, int bottomSteps) {
        void RotateLayer(ref List<Piece> layer, int steps, int cwDir) {
            steps = steps.mod(12);
            //Divides the pieces into the 12 steps that make up a layer
            int[] dividedPieces = new int[12];
            int stepsTraversed = 0;
            for (int i = 0; i < layer.Count; i++) {
                int shape = layer[i].Shape;
                for (int s = 0; s < shape; s++) {
                    dividedPieces[(stepsTraversed + s + (steps * cwDir)).mod(12)] = i;
                }
                stepsTraversed += shape;
            }
            //Joins the steps back into layers after they've been shifted
            List<Piece> newLayer = new List<Piece>();
            for (int i = 0; i < 12;) {
                int pieceIndex = dividedPieces[i];
                Piece piece = layer[pieceIndex];
                //Blocked seam cases:
                //Case 1: If a piece is over the front part of the seam then it the right half of it would be the first step checked
                //        The corner wouldn't continue on afterwards since the other half is on the other side of the seam
                //Case 2: If a corner starts at the 6th step(index 5 in the array) the other half would be over the back half of the seam
                if (piece.Shape == (int)Shapes.Corner && (i == 5 || dividedPieces[(i + 1) % 12] != pieceIndex)) {
                    throw new ArgumentException("Corner over slice");
                }
                newLayer.Add(piece);
                i += piece.Shape;
            }
            layer = newLayer;
        }
        //Shifting elements in the top layer by 1 would actually move the pieces counter clockwise due to the order that they're in the list
        RotateLayer(ref top, topSteps, -1);
        RotateLayer(ref bottom, bottomSteps, 1);

        //Logging
        string str = $"({topSteps.ToString()}, {bottomSteps})";
        if (debugOn) {
            print(str);
        }
        UIControl.AddHistory(str);

        //Sends animation requests to visual3D
        visual3D.AddAnimation(top, AnimationSection.Top, topSteps);
        visual3D.AddAnimation(bottom, AnimationSection.Bottom, bottomSteps);
    }

    public void Slice() {
        //Gets the halves of each layer that will be moved when the cube is sliced
        List<Piece> SwappedPieces(ref List<Piece> layer) {
            List<Piece> pieces = new List<Piece>();
            int i = 0;
            int steps = 0;
            while (steps < 6) {
                pieces.Add(layer[i]);
                steps += layer[i].Shape;
                i++;
            }
            return pieces;
        }
        List<Piece> topSwapped = SwappedPieces(ref top);
        List<Piece> bottomSwapped = SwappedPieces(ref bottom);

        //Swaps the pieces
        //The swapped pieces from the top will be in reverse order on the bottom and vice versa
        //Removing a number of elements equal to the opposite swapped layer pieces count isn't correct since different combinations of edges and corners can represent the same amount of steps

        InSteps(0, 6, top, out int topHalfLast, 1);
        top.RemoveRange(0, topHalfLast + 1);
        bottomSwapped.Reverse();
        top.InsertRange(0, bottomSwapped);

        InSteps(0, 6, bottom, out int botHalfLast, 1);
        bottom.RemoveRange(0, botHalfLast + 1);;
        topSwapped.Reverse();
        bottom.InsertRange(0, topSwapped);

        centerSquare = !centerSquare;

        //Logging
        if (debugOn) {
            print("/");
        }
        UIControl.AddHistory("/ ");

        //Sends animation requests to visual3D
        List<Piece> swappedPieces = topSwapped;
        swappedPieces.AddRange(bottomSwapped); 
        visual3D.AddAnimation(swappedPieces, AnimationSection.Slice, 6);
    }

    public void RunFromString(string str) {
        for (int i = 0; i < str.Length; i++) {
            if (str[i] == '(') {
                int end = i;
                while(str[end] != ')') { 
                    end++;    
                }
                string[] turnStrs = str.Substring(i, end - i).Split(new char[] {',', '(', ')', ' '}, StringSplitOptions.RemoveEmptyEntries);
                string topStr = turnStrs[0];
                string botStr = turnStrs[1];
                Rotate(int.Parse(topStr), int.Parse(botStr));
            } else if (str[i] == '/') {
                Slice();
            }
        }
    }

    public void Scramble(int slices) {
        //Identifies positions that a layer can be sliced from
        List<int> SliceablePositions(List<Piece> layer, int dir) {
            List<int> positions = new List<int>();
            for (int i = 0; i < layer.Count; i++) {
                if (InSteps(i, 6 * dir, layer) != InSteps(i, 7 * dir, layer)) {
                    positions.Add(i);
                }
            }
            return positions;
        }
        string scrambleString = "";
        for (int i = 0; i < slices; i++) {
            List<int> topPositions = SliceablePositions(top, -1);
            List<int> botPositions = SliceablePositions(bottom, -1);
            int topPosition = topPositions[Mathf.RoundToInt(UnityEngine.Random.Range(0, topPositions.Count))];
            int botPosition = botPositions[Mathf.RoundToInt(UnityEngine.Random.Range(0, botPositions.Count))];
            scrambleString += "(" + StepsBetween(topPosition, 0, top, -1, 0, 1) + ",-" + StepsBetween(botPosition, 0, bottom, -1, 0, 1) + ") / ";
            Rotate(StepsBetween(topPosition, 0, top, -1, 0, 1), -StepsBetween(botPosition, 0, bottom, -1, 0, 1));
            Slice();
        }
        print(scrambleString);
    }

    //Debug Functions
    public string LogShapes(List<Piece> layer) {
        string str = "";
        foreach (Piece piece in layer) {
            switch (piece.Shape) {
                case (int)Shapes.Corner:
                    str += "c";
                    break;
                case (int)Shapes.Edge:
                    str+= "e";
                    break;
                default:
                    break;
            }
        }
        return str;
    }

}

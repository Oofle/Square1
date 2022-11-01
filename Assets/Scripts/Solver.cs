using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Extension;
using System.Linq;

public class Solver : MonoBehaviour {
   public Square1 square1;
    delegate void Algorithm();
    public void Solve() {
        //Method adopted from https://ruwix.com/twisty-puzzles/square-1-back-to-square-one/
        //Gets the cube shape
        if (!(square1.LogShapes(square1.top) == "ecececec" || square1.LogShapes(square1.top) == "cececece") || !(square1.LogShapes(square1.bottom) == "ecececec" || square1.LogShapes(square1.bottom) == "cececece")) {
            //Gets groups of corners
            List<(int, int)> GetCornerGroups(List<Piece> layer) {
                List<(int, int)> cornerGroups = new List<(int, int)>();
                //Checks through the layer for corner groups
                //Groups are tuples of two integers
                // Item 1: position
                // Item 2: size
                for (int i = 0; i < layer.Count; i++) {
                    if (layer[i].Shape == (int)Shapes.Corner) {
                        int count = 1;

                        //Iterates through the list backwards from the corner to start at the "front" of the group
                        int b = (i - 1).mod(layer.Count);
                        while (layer[b].Shape == (int)Shapes.Corner) {
                            count++;
                            b = (b - 1).mod(layer.Count);
                            //Breaks the loop if there is only 1 group of 6 corners
                            if (b == i) {
                                break;
                            }
                        }
                        //Iterates through the list forwards from the corner
                        int c = (i + 1).mod(layer.Count);
                        while (layer[c].Shape == (int)Shapes.Corner) {
                            count++;
                            c = (c + 1).mod(layer.Count);
                            //Breaks the loop if there is only 1 group of 6 corners
                            if (c == i) {
                                break;
                            }
                        }

                        //Adds the group to the list
                        (int, int) group = ((b + 1).mod(layer.Count), count);
                        if (!cornerGroups.Contains(group)) {
                            cornerGroups.Add(group);
                        }
                    }
                }
                return cornerGroups;
            }

            //Brings all the corners to one layer
            int CornerAmount(List<Piece> layer) {
                int amount = 0;
                foreach (Piece piece in layer) {
                    if (piece.Shape == (int)Shapes.Corner) {
                        amount++;
                    }
                }
                return amount;
            }
            if (CornerAmount(square1.bottom) < 6 && CornerAmount(square1.top) < 6) {
                //Finds out which groups can be placed adjacent to a slice
                List<(int,int)> SliceGroups(List<(int,int)> groups, List<Piece> layer, int dir) {
                    List<(int,int)> sliceGroups = new List<(int, int)>();
                    foreach((int,int) group in groups) {
                        
                        int p = new int[] {group.Item1, (group.Item1 + group.Item2 - 1).mod(layer.Count) }[(dir + 1) / 2];
                        //Checks if a piece would cross over the slice
                        if (square1.InSteps(p, 6 * dir, layer) != square1.InSteps(p, 7 * dir, layer)) {
                            sliceGroups.Add(group);
                        }
                    }
                    return sliceGroups;
                }

                //Finds the groups that have the largest size
                //If there are multiple groups with the same max side, it will return all of them
                List<(int,int)> LargestGroups(List<(int,int)> groups) {
                    //Recall that item 2 is the size
                    int largestSize = groups[0].Item2;
                    List<(int,int)> largestGroups = new List<(int,int)>();
                    foreach((int,int) group in groups) {
                        if (group.Item2 > largestSize) {
                            largestSize = group.Item2;
                            largestGroups = new List<(int, int)>();
                        }
                        if (group.Item2 == largestSize) {
                            largestGroups.Add(group);
                        }
                    }
                    return largestGroups;
                }

                List<(int, int)> topGroups = GetCornerGroups(square1.top);
                List<(int, int)> botGroups = GetCornerGroups(square1.bottom);

                (int, int) topLargest = LargestGroups(topGroups)[0];
                (int, int) botLargest = LargestGroups(botGroups)[0];
                
                //Updates the variables that are used to represent the various groups
                void UpdateTopGroups() {
                    topGroups = GetCornerGroups(square1.top);
                    topLargest = LargestGroups(topGroups)[0];
                }

                void UpdateBottomGroups() {
                    botGroups = GetCornerGroups(square1.bottom);
                    botLargest = LargestGroups(botGroups)[0];
                }

                void UpdateGroups() {
                    UpdateTopGroups();
                    UpdateBottomGroups();
                }

                //Combines the largest sliceable groups together until there is at least a group of 3 corners
                while (!(topLargest.Item2 >= 3 || botLargest.Item2 >= 3)) {
                    List<(int, int)> topSliceGroups = LargestGroups(SliceGroups(topGroups, square1.top, -1));
                    List<(int, int)> botSliceGroups = LargestGroups(SliceGroups(botGroups, square1.bottom, -1));
                    //Turns the top group so that it is right of the slice
                    //Turns the bottom group so that it is left of the slice
                    int topSteps = square1.StepsBetween(topSliceGroups[0].Item1, 0, square1.top, -1, 0, 1);
                    print(botSliceGroups[0].Item1);
                    int botSteps = -square1.StepsBetween(botSliceGroups[0].Item1, 0, square1.bottom, -1, 0, 1) + 6;


                    square1.Rotate(topSteps, botSteps);
                    square1.Slice();
                    
                    //Updates the variables
                    UpdateGroups();
                }
                
                //Moves the group to the left half of the bottom layer
                if (botLargest.Item2 >= 3) {
                    square1.Rotate(0, -square1.StepsBetween(botLargest.Item1, 0, square1.bottom, -1, 0, 1) + 6);
                }
                UpdateTopGroups();
                
                if (topLargest.Item2 >= 3) {
                    square1.Rotate(square1.StepsBetween(topLargest.Item1, 0, square1.top, -1, 0, 1), 0);
                    square1.Slice();
                    square1.Rotate(0, 6);
                }

                topGroups = GetCornerGroups(square1.top);
                topLargest = LargestGroups(topGroups)[0];

                botLargest = LargestGroups(GetCornerGroups(square1.bottom))[0];

                //Forms another group of 3 in the top layer to complete an all corner bottom
                if (botLargest.Item2 != 6) {
                    while(topLargest.Item2 < 3) {
                        //find the the current largest groups on top layer
                        (int,int) topGroup = LargestGroups(SliceGroups(GetCornerGroups(square1.top), square1.top, -1))[0];

                        //move that  group to the bottom layer
                        square1.Rotate(square1.StepsBetween(topGroup.Item1, 0, square1.top, -1, 0, 1), 0);
                        square1.Slice();
                        
                        //find the next largest group on the top layer
                        topGroup = LargestGroups(SliceGroups(GetCornerGroups(square1.top), square1.top, 1))[0];
                        //rotate the top layer so that the groups can be combined on the next slice

                        square1.Rotate(square1.StepsBetween(topGroup.Item1, 0, square1.top, -1, 0, 1) + (topGroup.Item2 * 2), 0);
                        square1.Slice();

                        topLargest = LargestGroups(GetCornerGroups(square1.top))[0];
                    }
                    //slice to move the group to the bottom layer
                    square1.Rotate(square1.StepsBetween(topLargest.Item1, 0, square1.top, -1, 0, 1), 0);
                    //Moves it to the bottom layer
                    square1.Slice();
                }
            }

            //Checks for various cases to change the shape to a cube
            //check for the amount of edges between the corners
            int edgesBetween = 0;
            //Measures the distance from a -> b and b -> a and uses the lowest value
            List<(int,int)> topCorners = GetCornerGroups(square1.top);
            int rightCorner = topCorners[0].Item1;
            if (topCorners.Count != 1) {
                int edgesPos = 0;
                for (int i = 0; i < square1.top.Count; i++) {
                    if (square1.top[(topCorners[0].Item1 + 1 + i).mod(square1.top.Count)].Shape == (int)Shapes.Corner) {
                        break;
                    }
                    edgesPos++;
                }

                int edgesNeg = 0;
                for (int i = 0; i < square1.top.Count - edgesPos; i++) {
                    if (square1.top[(topCorners[0].Item1 - 1 - i).mod(square1.top.Count)].Shape == (int)Shapes.Corner) {
                        break;
                    }
                    edgesNeg++;
                }
                if (edgesPos > edgesNeg) {
                    edgesBetween = edgesNeg;
                    rightCorner = topCorners[1].Item1;
                } else {
                    edgesBetween = edgesPos;
                }
            }
            //Uses the edges between the corner groups to identify the case
            //and apply the appropriate algorithm
            switch (edgesBetween) { 
                case(0):
                    // /(-2,-4)/(-1,-2)/(-3,-3)/
                    square1.Rotate(square1.StepsBetween(rightCorner, 0, square1.top, -1, 0, 1) - 4, 0);
                    square1.Slice(); 
                    square1.Rotate(-2, -4);
                    square1.Slice();
                    square1.Rotate(-1, -2);
                    square1.Slice();
                    square1.Rotate(-3,-3);
                    square1.Slice();
                    break;
                case(1):
                    // /(2,-2)/(-3,-4)/(4,-3)/(-5,-4)/(6,-3)/	
                    square1.Rotate(square1.StepsBetween(rightCorner, 0, square1.top, -1, 0, 1) - 1, 0);
                    square1.Slice();
                    square1.Rotate(2, -2);
                    square1.Slice();
                    square1.Rotate(-3, -4);
                    square1.Slice();
                    square1.Rotate(4, -3);
                    square1.Slice();
                    square1.Rotate(-5, -4);
                    square1.Slice();
                    square1.Rotate(6, -3);
                    square1.Slice();
                    break;
                case (2):
                    // /(-4,-2)/(-1,4)/(-3,0)/
                    square1.Rotate(square1.StepsBetween(rightCorner, 0, square1.top, -1, 0, 1) + 3, 0);
                    square1.Slice();
                    square1.Rotate(-4, -2);
                    square1.Slice();
                    square1.Rotate(-1, 4);
                    square1.Slice();
                    square1.Rotate(-3, 0);
                    square1.Slice();
                    break;
                case (3):
                    // /(-4,0)/(5,4)/(2,-3)/(-5,-4)/(6,-3)/
                    square1.Rotate(square1.StepsBetween(rightCorner, 0, square1.top, -1, 0, 1) + 2, 0);
                    square1.Slice();
                    square1.Rotate(-4, 0);
                    square1.Slice();
                    square1.Rotate(5, 4);
                    square1.Slice();
                    square1.Rotate(2, -3);
                    square1.Slice();
                    square1.Rotate(-5, -4);
                    square1.Slice();
                    square1.Rotate(6, -3);
                    square1.Slice();
                    break;
                case (4):
                    // /(2, 2)/(0, -1)/(3, 3)/
                    square1.Rotate(square1.StepsBetween(rightCorner, 0, square1.top, -1, 0, 1) - 2, 0);
                    square1.Slice();
                    square1.Rotate(2, 2);
                    square1.Slice();
                    square1.Rotate(0, -1);
                    square1.Slice();
                    square1.Rotate(3, 3);
                    square1.Slice();
                    break;
            }
        }
        if (square1.LogShapes(square1.top) == "cececece") {
            square1.Rotate(-1, 0);
        }
        if (square1.LogShapes(square1.bottom) == "cececece") { 
            square1.Rotate(0, 1);    
        }
        
        //Fixes the center if it is not square
        if (!square1.centerSquare) {
            square1.Slice();
            square1.Rotate(6, 0);
            square1.Slice();
            square1.Rotate(-6, 0);
            square1.Slice();
        }
        
        //Brings all corners to their layers
        //Identifies which pieces need to be switched
        List<int> WrongLayerCorners(List<Piece> layer, int color) {
            List<int> corners = new List<int>();
            for (int i = 0; i < layer.Count; i++) {
                Piece piece = layer[i];
                if (piece.Shape == (int)Shapes.Corner && piece.LayerColor != color) {
                    corners.Add(i);
                }
            }
            return corners;
        }
        List<int> topWrong = WrongLayerCorners(square1.top, (int)Colors.Yellow);
        List<int> botWrong = WrongLayerCorners(square1.bottom, (int)Colors.White);
        while (topWrong.Count != 0) {
            //alligns two wrong corners
            int tQuarters = (topWrong[0] - 1) / 2;
            int bQuarters = (botWrong[0] - 1) / 2;
            square1.Rotate(tQuarters * 3, bQuarters * -3);

            //this algorithm swaps two corners that are above/below each other
            // (0, -4) / (0, 3) / (0, 1)     
            square1.Rotate(0, -4);
            square1.Slice();
            square1.Rotate(0, 3);
            square1.Slice();
            square1.Rotate(0, 1);           

            topWrong = WrongLayerCorners(square1.top, (int)Colors.Yellow);
            botWrong = WrongLayerCorners(square1.bottom, (int)Colors.White);
        }
        
        //Switches corners in the top layer to their correct side colors
        int[] colors = new int[] { (int)Colors.Red, (int)Colors.Green, (int)Colors.Orange, (int)Colors.Blue };
        //Returns the correct side colors for a corner given its index
        int[] CorrectCornerColors(int c) {
            c = ((c + 1) / 2) - 1;
            return (new int[]{colors[c], colors[(c + 1).mod(colors.Length)] });
        }
        //Compares two piece's colors without regard to the order
        bool SameColors(int[] a, int[] b) {
            if (a.Length != b.Length) {
                return false;
            }
            foreach(int element in a) {
                if (!b.Contains(element)) {
                    return false;
                }
            }
            return true;
        }

        //Gets the wrong colored corners
        List<int> WrongCornerColors(List<Piece> layer) {
            List<int> corners = new List<int>();
            for (int i = 0; i < 4; i++) {
                //only iterates over the corner pieces
                int index = (i + 1) * 2 - 1;
                int[] correctColors = CorrectCornerColors(index);
                if (!SameColors(layer[index].SideColors, correctColors)) {
                    corners.Add(index);
                }
            }
            return corners;
        }
        //swaps two top corners 
        //these would be the two right handed side ones(holding the cube in standard position)
        void SwapTopCorners() {
            //(1, 0) / (0, -3) / (0, 3) / (0, -3) / (0, -3) / (0, 6) / (-1, 0)
            square1.Rotate(1, 0);
            square1.Slice();
            square1.Rotate(0, -3);
            square1.Slice();
            square1.Rotate(0, 3);
            square1.Slice();
            square1.Rotate(0, -3);
            square1.Slice();
            square1.Rotate(0, -3);
            square1.Slice();
            square1.Rotate(0, 6);
            square1.Slice();
            square1.Rotate(-1, 0);
        }

        //general algorithm to swap corners given:
        // a list of wrong corners
        // a set of moves to swap the corners
        // the layer
        // the step direction of the layer (depending on if it is top or bottom)
        // an offset to account for rotation following the algorithm
        void SwapCorners(List<int> wrongCorners, int cornerIndex, Algorithm algorithm, ref List<Piece> layer, int stepDir, int postAlgRot = 0) {
            int usingTop = Convert.ToInt32(layer == square1.top);
            int usingBot = 1 - usingTop;
            int cornerOffset = ((cornerIndex - 1) / 2 * 3 * stepDir) + stepDir;
            int layerIndex = Array.IndexOf(new List<Piece>[2] {square1.top, square1.bottom}, layer);
            while (wrongCorners.Count != 0) {
                foreach (int wrongCorner in wrongCorners) {
                    int[] baseColors = layer[wrongCorner].SideColors;
                    int[] correctColors = CorrectCornerColors(wrongCorner);
                    //Checks for the next corner
                    int adjCorner = (wrongCorner + 2).mod(layer.Count);
                    
                    //Swaps the two adjacent corners if it would correctly place at least one of them
                    if (SameColors(layer[adjCorner].SideColors, correctColors) || SameColors(baseColors, CorrectCornerColors(adjCorner))) {
                        //Rotates the corner to the right spot
                        int steps = (square1.StepsBetween(wrongCorner, 0, layer, -1, 0, 1) * ((-1 * usingBot) + usingTop)) + cornerOffset;
                        square1.Rotate(steps * usingTop, steps * usingBot);
                        
                        algorithm();

                        square1.Rotate((-steps + postAlgRot) * usingTop, (-steps + postAlgRot) * usingBot);

                        wrongCorners = WrongCornerColors(layer);
                        break;
                    }

                    //Swaps two oppostie corners if it would correctly place at least one of them
                    int oppCorner = (wrongCorner + 4).mod(layer.Count);
                    if (SameColors(layer[oppCorner].SideColors, correctColors) || SameColors(baseColors, CorrectCornerColors(oppCorner))) {
                        int steps = (square1.StepsBetween(wrongCorner, 0, layer, -1, 0, 1) * ((-1 * usingBot) + usingTop)) + cornerOffset;
                        square1.Rotate(steps * usingTop, steps * usingBot);

                        algorithm();
                        square1.Rotate((3 + postAlgRot) * usingTop, (3 + postAlgRot) * usingBot);
                        algorithm();
                        square1.Rotate((-3 + postAlgRot) * usingTop, (-3 + postAlgRot) * usingBot);
                        algorithm();

                        square1.Rotate((-steps - postAlgRot) * usingTop, (-steps - postAlgRot) * usingBot);
                        wrongCorners = WrongCornerColors(layer);
                        break;
                    }
                }
            }
        }

        List<int> topWrongColor = WrongCornerColors(square1.top);
        SwapCorners(topWrongColor, 1, new Algorithm(SwapTopCorners), ref square1.top, -1);

        //Moves edges to their layers
        //Identifies the edges on the wrong layer(white on yellow/yellow on white)
        List<int> WrongLayerEdges(List<Piece> layer, int color) {
            List<int> pieces = new List<int>();
            for (int i = 0; i < 4; i++) {
                int index = i * 2;
                if (layer[index].LayerColor != color) {
                    pieces.Add(index);
                }
            }
            return pieces;
        }
        List<int> topWrongEdges = WrongLayerEdges(square1.top, (int)Colors.Yellow);
        List<int> botWrongEdges = WrongLayerEdges(square1.bottom, (int)Colors.White);

        //Takes 1 edge from the top and bottom layer and swaps them until there are no more wrong edges
        int te = 0;
        while(topWrongEdges.Count != 0 && te < 10) {
            te++;
            int topEdge = topWrongEdges[0];
            int botEdge = botWrongEdges[0];
            //Rotates the edges to the right hand side to be swapped by algorithm
            int topSteps = ((topEdge / 2) - 1) * 3;
            int botSteps = ((botEdge / 2) - 1) * -3;
            square1.Rotate(topSteps, botSteps);

            // Swaps the edges on the right hand side using the algorithm below
            // (1, 0) / (0, -3) / (0, -3) / (-1, -1) / (1, 4) / (0, 3) / (-1, 0) 
            square1.Rotate(1, 0);
            square1.Slice();
            square1.Rotate(0, -3);
            square1.Slice();
            square1.Rotate(0, -3);
            square1.Slice();
            square1.Rotate(-1, -1);
            square1.Slice();
            square1.Rotate(1, 4);
            square1.Slice();
            square1.Rotate(0, 3);
            square1.Slice();
            square1.Rotate(-1, 0);

            // Rotates the swapped edges back
            square1.Rotate(-topSteps, -botSteps);

            // Identifies new wrong edges
            topWrongEdges = WrongLayerEdges(square1.top, (int)Colors.Yellow);
            botWrongEdges = WrongLayerEdges(square1.bottom, (int)Colors.White);
        }
        
        //Switches corners in the bottom layer to their correct side colors
        List<int> botWrongColor = WrongCornerColors(square1.bottom);

        //Algorithm to swap two wrong corners on the bottom
        void SwapBotCorners() {
            // / (3, -3) / (0, 3) / (-3, 0) / (3, 0) / (-3, 0) /
            square1.Slice();
            square1.Rotate(3, -3);
            square1.Slice();
            square1.Rotate(0, 3);
            square1.Slice();
            square1.Rotate(-3, 0);
            square1.Slice();
            square1.Rotate(3, 0);
            square1.Slice();
            square1.Rotate(-3, 0);
            square1.Slice();
        }
        SwapCorners(botWrongColor, 7, new Algorithm(SwapBotCorners), ref square1.bottom, 1, 3);
        
        //Permutate edges (getting them to the right position in their layer)
        //Gets the edges that are in the wrong position based on their side color
        List<int> WrongEdges(List<Piece> layer) {
            List<int> edges = new List<int>();
            for (int i = 0; i < 4; i++) {
                if (colors[i] != layer[i * 2].SideColors[0]) {
                    edges.Add(i * 2);
                }
            }
            return edges;
        }
        List<int> topEdges = WrongEdges(square1.top);
        List<int> botEdges = WrongEdges(square1.bottom);
        //Swaps the back and right hand edges on the top/bottom layers at the same time
        // Using this algorithm may result in an edge parity where the front and back edges need to be swapped
        // Due to the algorithms available, it is better if the parity is on the top layer
        // The algorithm focuses on solving the bottom layer completely to avoid a parity on the bottom layer
        void SwapEdges() {
            // (0, 2) / (0, -3) / (1, 1) / (-1, 2) / (0, -2)
            square1.Rotate(0, 2);
            square1.Slice();
            square1.Rotate(0, -3);
            square1.Slice();
            square1.Rotate(1, 1);
            square1.Slice();
            square1.Rotate(-1, 2);
            square1.Slice();
            square1.Rotate(0, -2);
        }
        int TopSwapSteps(int offset) {
            foreach (int topEdge in topEdges) {
                int adjEdge = (topEdge + offset).mod(square1.top.Count);
                if (square1.bottom[adjEdge].SideColors[0] == colors[topEdge / 2] && square1.bottom[topEdge].SideColors[0] == colors[adjEdge / 2]) {
                    return square1.StepsBetween(topEdge, 0, square1.top, -1, 0, 1);
                }
            }
            return 0; 
        }
        //Swaps the bottom and top edges by checking for certain cases
        while (botEdges.Count != 0) {
            foreach(int botEdge in botEdges) {
                int bottomLength = square1.bottom.Count;
                int adjEdge = (botEdge + 2).mod(bottomLength);
                int baseColor = square1.bottom[botEdge].SideColors[0];
                int adjColor = square1.bottom[adjEdge].SideColors[0];
                
                //Adjacent
                if (baseColor == colors[adjEdge / 2] || adjColor == colors[botEdge / 2]) {
                    int topSteps = TopSwapSteps(2) - 3;
                    int botSteps = -square1.StepsBetween(botEdge, 0, square1.bottom, -1, 0, 1) + 3;
                    square1.Rotate(topSteps, botSteps);
                    SwapEdges();
                    square1.Rotate(-topSteps, -botSteps);

                    topEdges = WrongEdges(square1.top);
                    botEdges = WrongEdges(square1.bottom);
                    break;
                }
                //Opposite
                int oppEdge = (botEdge + 4).mod(bottomLength);
                int oppColor = square1.bottom[oppEdge].SideColors[0];
                if (baseColor == colors[oppEdge / 2] || oppColor == colors[botEdge / 2]) {
                    int topSteps = TopSwapSteps(4);
                    int botSteps = -square1.StepsBetween(botEdge, 0, square1.bottom, -1, 0, 1) ;
                    square1.Rotate(topSteps, botSteps);
                    SwapEdges();
                    square1.Rotate(-3, 3);
                    SwapEdges();
                    square1.Rotate(3, -3);
                    SwapEdges();
                    square1.Rotate(-topSteps, -botSteps);

                    topEdges = WrongEdges(square1.top);
                    botEdges = WrongEdges(square1.bottom);
                    break;
                }
            }
        }
        // Swaps the top parity
        void OppParity() {
            // / (3,3) / (1,0) / (-2,-2) / (2,0) / (2,2) / (-1,0) / (-3,-3) / (-2,0) / (3,3) / (3,0) / (-1,-1) / (-3,0) / (1,1) / (-4,-3)
            square1.RunFromString("/ (3,3) / (1,0) / (-2,-2) / (2,0) / (2,2) / (-1,0) / (-3,-3) / (-2,0) / (3,3) / (3,0) / (-1,-1) / (-3,0) / (1,1) / (-4,-3)");
        }
        //Fixes top layer if there are any wrong edges
        if (topEdges.Count != 0) {
            //Checks for different cases
            void CheckForSwaps() {
                foreach(int topEdge in topEdges) {
                    int color = square1.top[topEdge].SideColors[0];
                    //Parity(opp)
                    int oppEdge = (topEdge + 4).mod(square1.top.Count);
                    if (colors[topEdge / 2] == square1.top[oppEdge].SideColors[0] || colors[oppEdge / 2] == color) {
                        int steps = square1.StepsBetween(topEdge, 0, square1.top, -1, 0, 1);
                        square1.Rotate(steps, 0);
                        OppParity();
                        square1.Rotate(-steps, 0);
                        return;
                    }
                    //Adjacent
                    // the bottom is preserved because swapEdges() is used twice
                    int adjEdge = (topEdge + 2).mod(square1.top.Count);
                    if (colors[topEdge / 2] == square1.top[adjEdge].SideColors[0] || colors[adjEdge / 2] == color) {
                        int steps = square1.StepsBetween(topEdge, 0, square1.top, -1, 0, 1);
                        square1.Rotate(steps, 0);
                        SwapEdges();
                        OppParity();
                        SwapEdges();
                        square1.Rotate(-steps, 0);
                        return;
                    }
                }
            }
            switch(topEdges.Count) {
                case (2):
                    CheckForSwaps();
                    break;
                case (3):
                    while(topEdges.Count != 0) {
                        CheckForSwaps();
                        topEdges = WrongEdges(square1.top);
                    }
                    break;
                case (4):
                    while (topEdges.Count != 0) {
                        CheckForSwaps();
                        topEdges = WrongEdges(square1.top);
                    }
                    break;
            }
        }
    }

    void Start() {
        //Solve();
    }
}

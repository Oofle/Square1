using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Square1Creator : MonoBehaviour {
    //Allows the user the create a state of the square 1 without using algorithms
    
    List<Piece> referencePieces;

    [SerializeField]
    Dropdown shapeDropdown;
    [SerializeField]
    Dropdown layerCDropdown;
    [SerializeField]
    Dropdown c1Dropdown;
    [SerializeField]
    Dropdown c2Dropdown;
    [SerializeField]
    Dropdown layerDropdown;

    [SerializeField]
    Square1 square1;
    [SerializeField]
    Visual2D visual2d;

    void Start() {
        referencePieces = new List<Piece>(square1.top);
        referencePieces.AddRange(square1.bottom);
        print(square1.LogShapes(square1.top));
    }

    //Changes the square 1 in the main scene to the square 1 made in build mode on scene change
    void OnDisable() {
        SceneManager.sceneLoaded -= OnLoadMainScene;
    }

    void OnLoadMainScene(Scene scene, LoadSceneMode sceneMode) {
        //i should really learn how to do the => things
        //http://answers.unity.com/answers/728192/view.html
        List<Square1> squares = Array.ConvertAll(FindObjectsOfType(typeof(Square1)), item => item as Square1).ToList();
        print(squares.Count);
        squares.Remove(square1);
        print(squares.Count);
        //Putting squares[0] in a variable is just a copy not a reference
        squares[0].top = square1.top;
        squares[0].bottom = square1.bottom;
        Destroy(gameObject);
    }

    //Adds a piece
    public void AddPiece() {
        int shape = shapeDropdown.value + 1;
        int[] colors = new int[shape];
        for (int i = 0; i < shape; i++) {
            colors[i] = new int[] {c1Dropdown.value + 2, c2Dropdown.value + 2}[i];
        }
        Piece piece = new Piece(shape, layerCDropdown.value, colors);
        if (layerDropdown.value == 0) {
            square1.top.Add(piece);
        } else if (layerDropdown.value == 1) {
            square1.bottom.Add(piece);
        }
        visual2d.UpdateVisual();
    }

    //Deletes a piece
    public void DeletePiece() {
        if (layerDropdown.value == 0) {
            square1.top.RemoveAt(square1.top.Count - 1);
        } else if (layerDropdown.value == 1) {
            square1.bottom.RemoveAt(square1.bottom.Count - 1);
        }
        visual2d.UpdateVisual();
    }

    //Checks that the state is valid
    public void ValidityCheck() {
        List<Piece> refCopy = new List<Piece>(referencePieces);
        //Checks off pieces from a list
        void CheckLayer(List<Piece> layer) {
            foreach(Piece piece in layer) {
                if (refCopy.Contains(piece)) {
                    refCopy.Remove(piece);
                } else { 
                    //Checks for reverse
                    piece.SideColors.Reverse();
                    if (refCopy.Contains(piece)) {
                        refCopy.Remove(piece);
                    } else {
                        //Error message here
                        throw new System.Exception("Invalid cube state");
                    }
                }
            }
        }
        
        CheckLayer(square1.top);
        CheckLayer(square1.bottom);
        if (refCopy.Count == 0) {
            //Goes back to the main scene and loads the new square 1 state
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnLoadMainScene;
            SceneManager.LoadScene("Main");
        }
    }
    public void ExitBuildMode() {
        SceneManager.LoadScene("Main");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
public class UIControl : MonoBehaviour {
    [SerializeField]
    InputField topInput;
    [SerializeField]
    InputField botInput;

    [SerializeField]
    Slider speedSlider;
    [SerializeField]
    InputField speedText;

    [SerializeField]
    InputField scrambleLengthInput;

    [SerializeField]
    Text historyText;
    [SerializeField]
    Text historyPauseText;

    [SerializeField]
    Text toggle2DText;

    [SerializeField]
    InputField runStringInput;

    [SerializeField]
    Square1 square1;
    [SerializeField]
    Visual2D visual2D;
    [SerializeField]
    Visual3D visual3D;    

    string pausedHistory;
    bool paused;

    void Awake() {
        pausedHistory = "";
        paused = false;
    }

    public void UserRotate() {
        int.TryParse(topInput.text, out int topSteps);
        int.TryParse(botInput.text, out int botSteps);
        square1.Rotate(topSteps, botSteps);
    }

    public void UpdateSpeedSlider() {
        visual3D.animationSpeed = speedSlider.value;
        speedText.text = speedSlider.value.ToString();
    }

    public void UpdateSpeedInput() {
        if (!float.TryParse(speedText.text, out float newSpeed)) { 
            speedText.text = speedSlider.value.ToString();    
        } else {
            speedSlider.value = newSpeed;
            visual3D.animationSpeed = newSpeed;
        }
    }

    public void UserScramble() {
        square1.Scramble(int.Parse(scrambleLengthInput.text));
    }

    public void ClearHistory() {
        historyText.text = "";
    }

    public void PauseHistory() {
        paused = !paused;
        if (paused) {
            historyPauseText.text = "Unpause";
        } else {
            historyPauseText.text = "Pause";
        }
    }

    public void AddHistory(string str) {
        if (!paused) {
            historyText.text += str;
        }
    }

    public void RunString() {
        square1.RunFromString(runStringInput.text);
    }

    public void LoadBuildScene() {
        SceneManager.LoadScene("Creator");
    }

    public void ToggleVisual2D() {
        visual2D.on = !visual2D.on;
        visual2D.UpdateVisual();
        if (visual2D.on) {
            toggle2DText.text = "2D Off";
        } else {
            toggle2DText.text = "2D On";
        }
    }
}

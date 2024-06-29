using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TutorialTextCycler : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public float delay = 4f; 

    public Button upButton; 
    public Button downButton;
    public Button leftButton; 
    public Button rightButton;

    private string[] tutorialLines = {
        "Welcome to the tutorial",
        "Let's learn how to drive your car",
        "To move forward, press W",
        "To turn left, hold down W and press A",
        "To turn right, hold down W and press D",
        "To reverse, press S",
        "Different cars have different handling",
        "Some cars can tip over faster than others",
        "Try driving between the cones",
        "If you find that your car has tipped",
        "Just hold on! It'll right itself in a bit",
        "Drive carefully!",
        "Sometimes, the car may get stuck",
        "When this happens, press on the Spacebar to reset your car",
        "In the game, there are power ups and power downs",
        "When you see a spinning coin, you can try your luck!",
        "Drive into the coins on the road and see what happens",
        "Look around for a ramp!",
        "Try driving on it and see how the car handles in the air",
        ""
    };

    private int currentLineIndex = 0;

    void Start()
    {
        StartCoroutine(CycleTutorialTextOnce());
    }

    private IEnumerator CycleTutorialTextOnce()
    {
        while (currentLineIndex < tutorialLines.Length)
        {
            tutorialText.text = tutorialLines[currentLineIndex]; 

            yield return new WaitForSeconds(delay); 

            currentLineIndex++; 
        }
    }

    void Update()
    {
       
        upButton.interactable = !Input.GetKey(KeyCode.W); 
        downButton.interactable = !Input.GetKey(KeyCode.S);
        leftButton.interactable = !Input.GetKey(KeyCode.A);
        rightButton.interactable = !Input.GetKey(KeyCode.D);
    }
}
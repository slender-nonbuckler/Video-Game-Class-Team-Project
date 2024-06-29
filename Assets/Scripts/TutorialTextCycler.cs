using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialTextCycler : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public float delay = 4.5f; 

    public Button upButton; 
    public Button downButton;
    public Button leftButton; 
    public Button rightButton;

    public Button finishTutorial;

    private string[] tutorialLines = {
        "Welcome to the tutorial",
        "In this game, you'll be racing to the finish line",
        "Against a set of other drivers!",
        "You'll achieve this by progressing through the set checkpoints throughout the course",
        "As you race, you'll earn points  based on your standing that you can use to unlock new cars",
        "Let's learn how to drive your car first",
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
        "When you see a spinning tire, you can try your luck!",
        "Drive into the tire on the road and see what happens",
        "Look around for a ramp!",
        "Try driving on it and see how the car handles in the air",
        "You've reached the end of the tutorial",
        "Click Continue Tutorial to jump into racing"
    };

    private int currentLineIndex = 0;

    void Start()
    {
        finishTutorial.gameObject.SetActive(false);
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
        finishTutorial.gameObject.SetActive(true);
    }

    void Update()
    {
       
        upButton.interactable = !Input.GetKey(KeyCode.W); 
        downButton.interactable = !Input.GetKey(KeyCode.S);
        leftButton.interactable = !Input.GetKey(KeyCode.A);
        rightButton.interactable = !Input.GetKey(KeyCode.D);
    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Driver : MonoBehaviour
{
    private float speedCap = 3f;
    private float backingCap;
    private float steerSpeedCap = 200f;
    private float speed = 1.2f;
    private float backingSpeed;
    private float steerSpeed = 400f;

    private float moveAmount;
    private float actualMove;
    private float steerAmount;
    private float deAcc = 0.98f;
    private float deAccSteer = 0.96f;
    private bool isSteering = false;
    private bool isMoving = false;
    private float baseFPS = 270;

    private bool isOnRoad = true;
    private bool speedBoost = false;
    private bool gotGrabbed = false;

    [SerializeField] private bool isPlayerOne;

    [SerializeField] private GameManager gameManager;
    private byte laps;
    private bool canCompleteLap = false;
    private bool hasPassedFinishLine = true;
    [SerializeField] private TMP_Text lapText;

    [SerializeField] private GameObject winScreen;
    [SerializeField] private TMP_Text winText;

    [SerializeField] private GameObject musicList;
    // 0 is 321go
    // 1 is music
    // 2 is goalsound
    // 3 is speed pickup
    // 4 is loss of speed
    // 5 is explosion

    void Start()
    {
        moveAmount = 0;
        steerAmount = 0;
        backingCap = speedCap / 4;
        backingSpeed = speed / 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        // INPUT DETECTION

        if (!gameManager.gameIsOver)
        {
            if (isPlayerOne) WASDMovement();
            else ArrowKeyMovement();
            deAcc = 0.98f;
        }
        else
        {
            isSteering = false;
            isMoving = false;
            deAcc = 0.999f;
        }

        // DEACCELERATION

        if (!isSteering)
        {
            steerAmount *= Mathf.Pow(Mathf.Pow(deAccSteer, baseFPS), Time.deltaTime);
        }
        if (!isMoving)
        {
            moveAmount *= Mathf.Pow(Mathf.Pow(deAcc, baseFPS), Time.deltaTime);
        }

        // MOVING THE PLAYER

        transform.Rotate(0, 0, steerAmount * Time.deltaTime);
        if (isOnRoad) actualMove = moveAmount * 2;
        else actualMove = moveAmount;
        if (speedBoost) actualMove *= 1.5f;
        else if (gotGrabbed) actualMove *= 0.5f;
        transform.Translate(0, actualMove * Time.deltaTime, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Package is old name for speed boost.
        // This will make sure that it gets removed after being collected,
        // gives the player a speed boost of course,
        // and will eventually spawn a new speed boost
        if (collision.gameObject.CompareTag("Package"))
        {
            Destroy(collision.gameObject);
            speedBoost = true;
            StartCoroutine(SpeedBoost());
            StartCoroutine(GetRidOfSpawnCooldown());

        }
        // Makes sure the player's faster on the road
        if (collision.gameObject.CompareTag("Road"))
        {
            isOnRoad = true;
        }
        // Makes the player slow and lose a speed boost if they touch a hand
        if (collision.gameObject.CompareTag("Hand"))
        {
            StartCoroutine(GotGrabbed());
        }
        // All of this is for counting a lap, the 'required' triggers prevents cheating.
        if (collision.gameObject.CompareTag("RequiredCheckpoint"))
        {
            canCompleteLap = true;
        }
        if (collision.gameObject.CompareTag("RequiredReset"))
        {
            hasPassedFinishLine = true;
        }
        if (collision.gameObject.CompareTag("Goal") && canCompleteLap && hasPassedFinishLine)
        {
            laps++;
            lapText.text = $"{laps}/3";
            canCompleteLap = false;
            hasPassedFinishLine = false;

            if (laps >= 3)
            {
                YouWin();
            }

            musicList.transform.GetChild(2).GetComponent<AudioSource>().Play();
        }
    }

    // Punished the player for touching grass
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Road"))
        {
            isOnRoad = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        musicList.transform.GetChild(5).GetComponent<AudioSource>().Play();
    }

    // Movement for Player 1 (on the left)
    private void WASDMovement()
    {
        if (Keyboard.current.wKey.isPressed)
        {
            if (moveAmount < speedCap) moveAmount += speed * Time.deltaTime;
            isMoving = true;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            if (moveAmount > backingCap * -1) moveAmount -= backingSpeed * Time.deltaTime;
            isMoving = true;
        }
        else isMoving = false;
        if (Keyboard.current.dKey.isPressed && moveAmount > 0.5f)
        {
            if (steerAmount > steerSpeedCap * -1) steerAmount -= steerSpeed * Time.deltaTime;
            isSteering = true;
        }
        else if (Keyboard.current.dKey.isPressed && moveAmount < -0.5f)
        {
            if (steerAmount > (steerSpeedCap * -1) / 2) steerAmount -= (steerSpeed * 0.5f * Time.deltaTime);
            isSteering = true;
        }
        else if (Keyboard.current.aKey.isPressed && moveAmount > 1f)
        {
            if (steerAmount < steerSpeedCap) steerAmount += steerSpeed * Time.deltaTime;
            isSteering = true;
        }
        else if (Keyboard.current.aKey.isPressed && moveAmount < -0.5f)
        {
            if (steerAmount < steerSpeedCap / 2) steerAmount += (steerSpeed * 0.5f * Time.deltaTime);
            isSteering = true;
        }
        else isSteering = false;
    }

    // Movement for player 2 (on the right)
    private void ArrowKeyMovement()
    {
        if (Keyboard.current.upArrowKey.isPressed)
        {
            if (moveAmount < speedCap) moveAmount += speed * Time.deltaTime;
            isMoving = true;
        }
        else if (Keyboard.current.downArrowKey.isPressed)
        {
            if (moveAmount > backingCap * -1) moveAmount -= backingSpeed * Time.deltaTime;
            isMoving = true;
        }
        else isMoving = false;
        if (Keyboard.current.rightArrowKey.isPressed && moveAmount > 0.5f)
        {
            if (steerAmount > steerSpeedCap * -1) steerAmount -= steerSpeed * Time.deltaTime;
            isSteering = true;
        }
        else if (Keyboard.current.rightArrowKey.isPressed && moveAmount < -0.5f)
        {
            if (steerAmount > (steerSpeedCap * -1) / 2) steerAmount -= (steerSpeed * 0.5f * Time.deltaTime);
            isSteering = true;
        }
        else if (Keyboard.current.leftArrowKey.isPressed && moveAmount > 1f)
        {
            if (steerAmount < steerSpeedCap) steerAmount += steerSpeed * Time.deltaTime;
            isSteering = true;
        }
        else if (Keyboard.current.leftArrowKey.isPressed && moveAmount < -0.5f)
        {
            if (steerAmount < steerSpeedCap / 2) steerAmount += (steerSpeed * 0.5f * Time.deltaTime);
            isSteering = true;
        }
        else isSteering = false;
    }


    // For speedboost
    private IEnumerator GetRidOfSpawnCooldown()
    {
        yield return new WaitForSeconds(3);
        gameManager.speedBoostIsOnMap = false;
    }

    private IEnumerator SpeedBoost()
    {
        speedBoost = true;
        musicList.transform.GetChild(3).GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(10);
        if (speedBoost) musicList.transform.GetChild(4).GetComponent<AudioSource>().Play();
        speedBoost = false;
    }

    // For hands
    private IEnumerator GotGrabbed()
    {
        speedBoost = false;
        if (!gotGrabbed) musicList.transform.GetChild(4).GetComponent<AudioSource>().Play();
        gotGrabbed = true;
        yield return new WaitForSeconds(3);
        gotGrabbed = false;
    }


    // For winning
    private void YouWin()
    {
        winScreen.SetActive(true);
        if (isPlayerOne) winText.text = "Player 1 Wins!";
        else winText.text = "Player 2 Wins!";
        gameManager.gameIsOver = true;
    }
}

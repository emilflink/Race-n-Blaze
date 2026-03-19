using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;

    [SerializeField] private GameObject speedBoost;

    [SerializeField] private TMP_Text timeText;

    [SerializeField] private TMP_Text countdownText;

    private float time = 0;
    private int minutes;
    private byte seconds1;
    private byte seconds2;
    private byte millisecs1;
    private byte millisecs2;

    // This is also used before the game starts, it's a sort of off switch.
    public bool gameIsOver;

    public bool speedBoostIsOnMap;

    void Start()
    {
        // Spawns a speed boost
        speedBoostIsOnMap = true;
        SpawnSpeedBoost();
        StartCoroutine(Countdown());

    }

    void Update()
    {
        // If there isn't a speedboost on the map (and some seconds have passed), a speedboost will spawn.
        if (!speedBoostIsOnMap)
        {
            SpawnSpeedBoost();
            speedBoostIsOnMap = true;
        }

        // All of this is for the timer.
        if (!gameIsOver) time += Time.deltaTime;

        // Calculates a 100th of a second.
        millisecs1 = (byte)(time * 100);
        
        // Calculates a 10th of a second
        if (millisecs1 >= 10)
        {
            millisecs2++;
            millisecs1 = 0;
            time = 0;
        }

        // Calculates seconds
        if (millisecs2 >= 10)
        {
            seconds1++;
            millisecs2 = 0;
        }

        // Calculates 10 seconds
        if (seconds1 >= 10)
        {
            seconds2++;
            seconds1 = 0;
        }

        // Calculates a minute
        if (seconds2 >= 6)
        {
            minutes++;
            seconds2 = 0;
        }

        // Makes it show all nice in the text
        timeText.text = $"{minutes}:{seconds2}{seconds1}:{millisecs2}{millisecs1}";

    }

    // Spawns in speed boost in one of two areas.
    void SpawnSpeedBoost()
    {
        switch (Random.Range(0, 2))
        {
            case 0:
                Instantiate(speedBoost, spawnPoint1);
                break;

            case 1:
                Instantiate(speedBoost, spawnPoint2);
                break;
        }
    }

    private IEnumerator Countdown()
    {
        countdownText.text = "3";
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1);
        countdownText.enabled = false;
        gameIsOver = false;
    }
}

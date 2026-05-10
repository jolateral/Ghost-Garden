using UnityEngine;

public class WinSequenceManager : MonoBehaviour
{
    public static WinSequenceManager Instance;

    // Steps: 0=Scare birds, 1=Tarp exposed (auto), 2=Windchimes, 3=Watering can
    int _nextStep = 0;

    void Awake() => Instance = this;

    public void RegisterNudge(int step)
    {
        if (step != _nextStep)
        {
            HUDManager.Instance?.ShowMessage("Nothing seems to happen...");
            return;
        }

        _nextStep++;

        switch (step)
        {
            case 0: BirdController.Instance?.ScareAway(); break;
            case 1: ExposeDamagedGarden();                break;
            case 2: PlayWindchimes();                     break;
            case 3: DropWateringCan();                    break;
        }

        if (_nextStep >= 4)
            NeighbourAI.Instance?.NoticeGarden();
    }

    void ExposeDamagedGarden()
    {
        GameObject tarp = GameObject.FindWithTag("Tarp");
        if (tarp != null)
            tarp.SetActive(false);
    }

    void PlayWindchimes()
    {
        GameObject chimeObj = GameObject.FindWithTag("Windchimes");
        if (chimeObj != null)
        {
            AudioSource chimes = chimeObj.GetComponent<AudioSource>();
            if (chimes != null)
                chimes.Play();
        }
    }

void DropWateringCan()
{
    GameObject canObj = GameObject.FindWithTag("WateringCan");
    if (canObj != null)
    {
        Rigidbody rb = canObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
}
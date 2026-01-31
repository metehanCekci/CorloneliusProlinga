using UnityEngine;

public class KeepAlive : MonoBehaviour
{
    // Statik referans (Hafýzadaki adres)
    public static KeepAlive Instance;

    void Awake()
    {
        // Eðer daha önce oluþturulmuþ bir kopya yoksa:
        if (Instance == null)
        {
            Instance = this; // O benim!
            DontDestroyOnLoad(gameObject); // Beni koru
        }
        else
        {
            // Eðer zaten bir tane varsa ve ben sonradan oluþtuysam (sahne tekrar yüklendiðinde)
            // Ben fazlalýðým, beni yok et.
            Destroy(gameObject);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManageBotoes : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartCenario(string cena)
    {
        SceneManager.LoadScene(cena);
    }

    public void Voltar()
    {
        var cena = PlayerPrefs.GetString("ultimaCena");
        SceneManager.LoadScene(cena);
    }
}

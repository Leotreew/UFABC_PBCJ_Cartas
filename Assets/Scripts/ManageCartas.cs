using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ManageCartas : MonoBehaviour
{

    public GameObject carta;        // carta a ser descartada
    private bool primeiraCartaSelecionada, segundaCartaSelecionada; // Indicadores para cada carta selecionada em cada linha
    private GameObject carta1, carta2;          // gameObjects da Primeira e Segunda Carta Selecionada
    private string linhaCarta1, linhaCarta2;    // Linha da Carta

    bool timerPausado, timerAcionado;           // Indicador de Pausa no Timer ou Start do Timer
    float timer;                                // Variável de Tempo

    int numTentativas = 0;                      // Numero de Tentativas na Rodada
    int numAcertos = 0;                         // Numero de Macth de pares acertados
    AudioSource somOk;                          // Som de Acerto

    int ultimoJogo = 0;

    // Start is called before the first frame update
    void Start()
    {
        MostraCartas();
        UpdateTentativas();
        somOk = GetComponent<AudioSource>();
        ultimoJogo = 0;
        GameObject.Find("ultimaJogada").GetComponent<Text>().text = ("Jogo Anterior = " + ultimoJogo);

    }

    // Update is called once per frame
    void Update()
    {
        if (timerAcionado)
        {
            timer += Time.deltaTime;
            print(timer);
            if(timer > 1)
            {
                timerPausado = true;
                timerAcionado = false;
                if(carta1.tag == carta2.tag)
                {
                    Destroy(carta1);
                    Destroy(carta2);
                    numAcertos++;
                    somOk.Play();
                    if(numAcertos == 13)
                    {
                        PlayerPrefs.SetInt("Jogadas", numTentativas);
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                }
                else
                {
                    carta1.GetComponent<Tile>().EscondeCarta();
                    carta2.GetComponent<Tile>().EscondeCarta();
                }
                primeiraCartaSelecionada = false;
                segundaCartaSelecionada = false;
                carta1 = null;
                carta2 = null;
                linhaCarta1 = "";
                linhaCarta2 = "";
                timer = 0;
            }
        }
    }

    void MostraCartas()
    {
        int[] arrayEmbaralhado = CriaArrayEmbaralhado();
        int[] arrayEmbaralhado2 = CriaArrayEmbaralhado();
        
        for (int i = 0; i < 13; i++)
        {
            AddUmaCarta(0, i, arrayEmbaralhado[i]);
            AddUmaCarta(1, i, arrayEmbaralhado2[i]);
        }
    }

    void AddUmaCarta(int linha, int rank, int valor)
    {
        GameObject centro = GameObject.Find("centroDaTela");
        float escalaCartaOriginal = carta.transform.localScale.x;
        float fatorEscalaX = (650 * escalaCartaOriginal) / 100.0f;
        float fatorEscalaY = (945 * escalaCartaOriginal) / 100.0f;

        Vector3 novaPosicao = new Vector3(centro.transform.position.x + (rank - 13 / 2) * fatorEscalaX, centro.transform.position.y + ((linha - 0.5f) * fatorEscalaY), centro.transform.position.z);
        GameObject c = (GameObject)Instantiate(carta, novaPosicao, Quaternion.identity);
        c.tag = "" + (valor + 1);
        c.name = "" + linha + "_" + valor;
        string nomeDaCarta = "";
        string numeroDaCarta = "";
        if (valor == 0)
        {
            numeroDaCarta = "ace";
        }
        else if (valor== 10)
        {
            numeroDaCarta = "jack";
        }
        else if (valor == 11)
        {
            numeroDaCarta = "queen";
        }
        else if (valor == 12)
        {
            numeroDaCarta = "king";
        }
        else
        {
            numeroDaCarta = "" + (valor + 1);
        }
        /*
        if (linha == 1)
        {
            nomeDaCarta = numeroDaCarta + "_of_hearts";
        }
        else
        {*/
            nomeDaCarta = numeroDaCarta + "_of_clubs";
        //}
        Sprite s1 = (Sprite)Resources.Load<Sprite>(nomeDaCarta);
        print("S1" + s1);
        GameObject.Find("" + linha + "_" + valor).GetComponent<Tile>().setCartaOriginal(s1);
    }

    public int[] CriaArrayEmbaralhado()
    {
        int[] novoArray = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
        int temp;

        for (int t = 0; t < 13; t++)
        {
            temp = novoArray[t];
            int r = Random.Range(t, 13);
            novoArray[t] = novoArray[r];
            novoArray[r] = temp;
        }
        return novoArray;
    }

    public void CartaSelecionada(GameObject carta)
    {
        if (!primeiraCartaSelecionada)
        {
            string linha = carta.name.Substring(0, 1);
            linhaCarta1 = linha;
            primeiraCartaSelecionada = true;
            carta1 = carta;
            carta1.GetComponent<Tile>().RevelaCarta();
        }
        else if (primeiraCartaSelecionada && !segundaCartaSelecionada)
        {
            string linha = carta.name.Substring(0, 1);
            linhaCarta2 = linha;
            segundaCartaSelecionada = true;
            carta2 = carta;
            carta2.GetComponent<Tile>().RevelaCarta();
            VerificaCarta();
        }
    }

    public void VerificaCarta()
    {
        DisparaTimer();
        numTentativas++;
        UpdateTentativas();
    }

    public void DisparaTimer()
    {
        timerPausado = false;
        timerAcionado = true;

    }

    void UpdateTentativas()
    {
        GameObject.Find("numTentativas").GetComponent<Text>().text = "Tentativas: " + numTentativas;
    }
}

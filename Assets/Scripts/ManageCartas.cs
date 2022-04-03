using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Possiveis modos de jogo de cartas.
public enum ModoJogo { Normal, Duo, Quadiletras }

// Possui a logica para gerenciar o fluxo do jogo de cartas.
public class ManageCartas : MonoBehaviour
{
    // carta a ser descartada
    public GameObject carta;

    // Modo de jogo a ser carregado.
    public ModoJogo modoJogo;

    // Indicadores para cada carta selecionada em cada linha
    bool primeiraCartaSelecionada, segundaCartaSelecionada;

    // gameObjects da Primeira e Segunda Carta Selecionada
    GameObject carta1, carta2;

    // Linha da Carta
    string linhaCarta1, linhaCarta2;

    // Indicador de Pausa no Timer ou Start do Timer
    bool timerPausado, timerAcionado;

    // Variavel de Tempo
    float timer;

    // Numero de Tentativas na Rodada
    int numTentativas = 0;
    
    // Numero de Macth de pares acertados
    int numAcertos = 0;

    // Som de Acerto
    AudioSource somOk;

    int ultimoJogo = 0;

    // Start is called before the first frame update
    void Start()
    {
        MostraCartas(modoJogo);
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

    void MostraCartas(ModoJogo modo)
    {
        int[] arrayEmbaralhado;
        int[] arrayEmbaralhado2;

        switch (modo)
        {
            case ModoJogo.Normal:
                arrayEmbaralhado = CriaArrayEmbaralhado();
                arrayEmbaralhado2 = CriaArrayEmbaralhado();
                
                for (int i = 0; i < 13; i++)
                {
                    AddUmaCarta(0, i, arrayEmbaralhado[i], modo);
                    AddUmaCarta(1, i, arrayEmbaralhado2[i], modo);
                }
                break;
            case ModoJogo.Duo:
                arrayEmbaralhado = CriaArrayEmbaralhado();
                arrayEmbaralhado2 = CriaArrayEmbaralhado();
                
                for (int i = 0; i < 13; i++)
                {
                    AddUmaCarta(0, i, arrayEmbaralhado[i], modo);
                    AddUmaCarta(1, i, arrayEmbaralhado2[i], modo);
                }
                break;
            case ModoJogo.Quadiletras:
                // TODO: pensar em como organizar as cartas na tela.
                break;
        }
    }

    void AddUmaCarta(int linha, int rank, int valor, ModoJogo modo)
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

        switch (valor) {
            case 0: numeroDaCarta = "ace"; break;
            case 10: numeroDaCarta = "jack"; break;
            case 11: numeroDaCarta = "queen"; break;
            case 12: numeroDaCarta = "king"; break;
            default: numeroDaCarta = $"{(valor + 1)}"; break;
        }

        if (modo == ModoJogo.Duo)
        {
            // Utiliza fundo azul se for a segunda linha de cartas do modo Duo.
            if (linha == 1)
            {
                Sprite fundo = (Sprite)Resources.Load<Sprite>("playCardBackBlue");
                GameObject.Find($"{linha}_{valor}").GetComponent<Tile>().setCartaFundo(fundo);
            }
        }

        nomeDaCarta = $"{numeroDaCarta}_of_clubs";
        Sprite s1 = (Sprite)Resources.Load<Sprite>(nomeDaCarta);
        GameObject.Find($"{linha}_{valor}").GetComponent<Tile>().setCartaOriginal(s1);
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

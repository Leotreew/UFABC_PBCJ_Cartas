using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Possiveis modos de jogo de cartas
public enum ModoJogo { Normal, Duo, Quadiletras }

// Possui a logica para gerenciar o fluxo do jogo de cartas
public class ManageCartas : MonoBehaviour
{
    // carta a ser descartada
    public GameObject carta;

    // Modo de jogo a ser carregado
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

    // Salvar o nome da cena
    string nomeDaCena;

    int tentativasAnterior = 0;

    int tentativasMin = 300;

    // Som de Acerto
    AudioSource somOk;

    // Acertos necessarios para terminar o jogo, por modo
    Dictionary<ModoJogo, int> acertosPorModo = new Dictionary<ModoJogo, int>()
    {
        {ModoJogo.Normal, 13},
        {ModoJogo.Duo, 13},
        {ModoJogo.Quadiletras, 16},
    };

    // Start is called before the first frame update
    void Start()
    {
        nomeDaCena = SceneManager.GetActiveScene().name;        
        
        MostraCartas();
        UpdateTentativas();
        
        somOk = GetComponent<AudioSource>();
        
        tentativasAnterior = PlayerPrefs.GetInt(nomeDaCena + "TentativasAnterior", 0);
        GameObject.Find("ultimaJogada").GetComponent<Text>().text = ("Jogo Anterior = " + tentativasAnterior);

        tentativasMin = PlayerPrefs.GetInt(nomeDaCena + "TentativasMin", 300);
        if (tentativasMin != 300)
        {
            GameObject.Find("TentativaMin").GetComponent<Text>().text = ("Menor Tentativa = " + tentativasMin);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (timerAcionado)
        {
            timer += Time.deltaTime;
            
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

                    if(numAcertos >= acertosPorModo[modoJogo])
                    {
                        ChecarTentativaAnterior();
                        ChecarTentativas();
                        PlayerPrefs.Save();
                        SceneManager.LoadScene("MenuInicial");
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
        List<List<(int, int)>> baralho;
        int linhas;
        int colunas;

        var camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        Vector3 centro;

        switch (modoJogo)
        {
            case ModoJogo.Normal:
                linhas = 2;
                colunas = 13;

                centro = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1.0f));

                for (int i = 0; i < linhas; i++)
                {
                    baralho = Embaralha(
                        new List<int>{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        new List<int>{ 0 },
                        1,
                        colunas
                    );

                    for (int j = 0; j < colunas; j++)
                    {
                        var (nipe, numero) = baralho[0][j];
                        AddUmaCarta(nipe, numero, 0, i, j, linhas, colunas, centro);
                    }
                }
                break;
            case ModoJogo.Duo:
                linhas = 2;
                colunas = 13;

                centro = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1.0f));

                for (int i = 0; i < linhas; i++)
                {
                    baralho = Embaralha(
                        new List<int>{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        new List<int>{ 0 },
                        1,
                        colunas
                    );

                    for (int j = 0; j < colunas; j++)
                    {
                        var (nipe, numero) = baralho[0][j];
                        AddUmaCarta(nipe, numero, i, i, j, linhas, colunas, centro);
                    }
                }
                break;
            case ModoJogo.Quadiletras:
                linhas = 4;
                colunas = 4;

                var centros = new List<Vector3>()
                {
                    camera.ViewportToWorldPoint(new Vector3(0.25f, 0.5f, 1.0f)),
                    camera.ViewportToWorldPoint(new Vector3(0.75f, 0.5f, 1.0f))
                };

                for (int g = 0; g < 2; g++)
                {
                    centro = centros[g];

                    baralho = Embaralha(
                        new List<int>{ 0, 10, 11, 12 },
                        new List<int>{ 0, 1, 2, 3 },
                        linhas,
                        colunas
                    );

                    for (int i = 0; i < linhas; i++)
                    {
                        for (int j = 0; j < colunas; j++)
                        {
                            var (nipe, numero) = baralho[i][j];
                            AddUmaCarta(nipe, numero, g, i, j, linhas, colunas, centro);
                        }
                    }
                }
                break;
        }
    }

    void AddUmaCarta(int nipe, int numero, int grupo, int linha, int coluna, int linhas, int colunas, Vector3 centro)
    {
        var escalaCartaOriginalX = carta.transform.localScale.x;
        var escalaCartaOriginalY = carta.transform.localScale.y;
        var fatorEscalaX = (650 * escalaCartaOriginalX) / 100.0f;
        var fatorEscalaY = (945 * escalaCartaOriginalY) / 100.0f;

        var novaPosicao = new Vector3(
            centro.x + (coluna - colunas / 2.0f) * fatorEscalaX,
            centro.y + (linha - linhas / 2.0f) * fatorEscalaY,
            centro.z
        );
        var c = (GameObject)Instantiate(carta, novaPosicao, Quaternion.identity);
        c.tag = $"{nipe}_{numero}";
        c.name = $"{grupo}_{linha}_{nipe}_{numero}";

        var numeroDaCarta = "";
        switch (numero) {
            case 0: numeroDaCarta = "ace"; break;
            case 10: numeroDaCarta = "jack"; break;
            case 11: numeroDaCarta = "queen"; break;
            case 12: numeroDaCarta = "king"; break;
            default: numeroDaCarta = $"{(numero + 1)}"; break;
        }

        var nipeDaCarta = "";
        switch (nipe) {
            case 1: nipeDaCarta = "hearts"; break;
            case 2: nipeDaCarta = "spades"; break;
            case 3: nipeDaCarta = "diamonds"; break;
            default: nipeDaCarta = "clubs"; break;
        }

        var original = (Sprite)Resources.Load<Sprite>($"{numeroDaCarta}_of_{nipeDaCarta}");
        GameObject.Find($"{grupo}_{linha}_{nipe}_{numero}").GetComponent<Tile>().setCartaOriginal(original);

        if (grupo == 1)
        {
            // Utiliza fundo azul se for o segundo grupo de cartas.
            var fundo = (Sprite)Resources.Load<Sprite>("playCardBackBlue");
            GameObject.Find($"{grupo}_{linha}_{nipe}_{numero}").GetComponent<Tile>().setCartaFundo(fundo);
        }
    }

    public List<List<(int, int)>> Embaralha(List<int> valores, List<int> nipes, int linhas, int colunas)
    {
        var cartas = new List<(int, int)>();
        foreach (var n in nipes)
        {
            foreach (var v in valores)
            {
                cartas.Add((n, v));
            }
        }
        
        var baralho = new List<List<(int, int)>>();
        for (int i = 0; i < linhas; i++)
        {
            baralho.Add(new List<(int, int)>());
            for (int j = 0; j < colunas; j++)
            {
                var indice = Random.Range(0, cartas.Count);
                var carta = cartas[indice];
                baralho[i].Add(carta);
                cartas.RemoveAt(indice);
            }
        }

        return baralho;
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

    public void ChecarTentativas()
    {
        if (tentativasMin > numTentativas)
        {
            tentativasMin = numTentativas;
            PlayerPrefs.SetInt(nomeDaCena + "TentativasMin", tentativasMin);
            print("Dentro");
        }
    }

    public void ChecarTentativaAnterior()
    {
        tentativasAnterior = numTentativas;
        PlayerPrefs.SetInt(nomeDaCena + "TentativasAnterior", tentativasAnterior);
    }
}

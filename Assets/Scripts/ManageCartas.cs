using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Possiveis modos de jogo de cartas
/// </summary>
public enum ModoJogo { Normal, Duo, Quadiletras }

/// <summary>
/// Armazena configuracoes de um modo de jogo de cartas
/// </summary>
public class ModoJogoConfig
{
    /// <summary>
    /// Quantidade de acertos para finalizar o jogo
    /// </summary>
    public int Acertos { get; set; }

    /// <summary>
    /// Quantidade maxima de tentativas para considerar como uma vitoria
    /// </summary>
    public int Vitoria { get; set; }
}

/// <summary>
/// Possui a logica para gerenciar o fluxo do jogo de cartas
/// </summary>
public class ManageCartas : MonoBehaviour
{
    /// <summary>
    /// Carta a ser descartada
    /// </summary>
    public GameObject carta;

    /// <summary>
    /// Modo de jogo a ser carregado
    /// </summary>
    public ModoJogo modoJogo;

    /// <summary>
    /// Indicadores para cada carta selecionada em cada linha
    /// </summary>
    bool primeiraCartaSelecionada, segundaCartaSelecionada;

    /// <summary>
    /// GameObjects da Primeira e Segunda Carta Selecionada
    /// </summary>
    GameObject carta1, carta2;

    /// <summary>
    /// Indicador de Pausa no Timer ou Start do Timer
    /// </summary>
    bool timerPausado, timerAcionado;

    /// <summary>
    /// Variavel de Tempo
    /// </summary>
    float timer;

    /// <summary>
    /// Numero de Tentativas na Rodada
    /// </summary>
    int numTentativas = 0;

    /// <summary>
    /// Numero de Macth de pares acertados
    /// </summary>
    int numAcertos = 0;

    /// <summary>
    /// Salvar o nome da cena
    /// </summary>
    string nomeDaCena;

    /// <summary>
    /// Salvar o numero de tentativas da partida anterior
    /// </summary>
    int tentativasAnterior = 0;

    /// <summary>
    /// Salvar o record de tentativas
    /// </summary>
    int tentativasMin = 300;

    /// <summary>
    /// Som de Acerto
    /// </summary>
    AudioSource somOk;

    /// <summary>
    /// Configuracoes gerais para cada modo de jogo
    /// </summary>
    /// <typeparam name="ModoJogo">Modo de jogo a ser configurado</typeparam>
    /// <typeparam name="ModoJogoConfig">Dados de configuracao para o modo de jogo</typeparam>
    Dictionary<ModoJogo, ModoJogoConfig> infoPorModo = new Dictionary<ModoJogo, ModoJogoConfig>()
    {
        {ModoJogo.Normal, new ModoJogoConfig() { Acertos = 13, Vitoria = 45 }},
        {ModoJogo.Duo, new ModoJogoConfig() { Acertos = 13, Vitoria = 45 }},
        {ModoJogo.Quadiletras, new ModoJogoConfig() { Acertos = 13, Vitoria = 68 }},
    };

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        // Utilizado para conseguir o nome da cena atual 
        nomeDaCena = SceneManager.GetActiveScene().name;

        // Guarda a cena atual para uso na tela de vitoria / derrota
        PlayerPrefs.SetString("ultimaCena", nomeDaCena);
        PlayerPrefs.Save();

        MostraCartas();
        UpdateTentativas();

        somOk = GetComponent<AudioSource>();

        /* Esses 3 ifs são utilizados na logica de colocar na tela os valores de Tentativas minimo/atual/anterior */
        if (PlayerPrefs.HasKey(nomeDaCena + "TentativasAnterior"))
        {
            tentativasAnterior = PlayerPrefs.GetInt(nomeDaCena + "TentativasAnterior");
        }

        GameObject.Find("ultimaJogada").GetComponent<Text>().text = ("Jogo Anterior = " + tentativasAnterior);
        if (PlayerPrefs.HasKey(nomeDaCena + "TentativasMin"))
        {
            tentativasMin = PlayerPrefs.GetInt(nomeDaCena + "TentativasMin");
            if (tentativasMin == 0)
            {
                tentativasMin = 300;

            }
        }
        if (tentativasMin != 300)
        {
            GameObject.Find("TentativaMin").GetComponent<Text>().text = ("Tentativa minima = " + tentativasMin);
        }

    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (timerAcionado)
        {
            timer += Time.deltaTime;

            if (timer > 1)
            {
                timerPausado = true;
                timerAcionado = false;

                if (carta1.tag == carta2.tag)
                {
                    Destroy(carta1);
                    Destroy(carta2);
                    numAcertos++;
                    somOk.Play();

                    // Verifica condicao de termino do jogo
                    if (numAcertos >= infoPorModo[modoJogo].Acertos)
                    {
                        // Armazena tentativas do jogador
                        ChecarTentativaAnterior();
                        ChecarTentativas();
                        PlayerPrefs.Save();

                        // Decide a cena com base na condicao de vitoria
                        var cena = (numTentativas <= infoPorModo[modoJogo].Vitoria) ? "Vitoria" : "Derrota";
                        SceneManager.LoadScene(cena);
                    }
                }
                else
                {
                    carta1.GetComponent<Tile>().EscondeCarta();
                    carta2.GetComponent<Tile>().EscondeCarta();
                }

                // Finaliza o timer e zera variaveis relacionadas
                primeiraCartaSelecionada = false;
                segundaCartaSelecionada = false;
                carta1 = null;
                carta2 = null;
                timer = 0;
            }
        }
    }

    /// <summary>
    /// Exibe as cartas na mesa, de acordo com o modo de jogo
    /// </summary>
    void MostraCartas()
    {
        List<List<(int, int)>> baralho;
        int linhas;
        int colunas;

        var camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        Vector3 centro;

        // Monta a mesa de uma forma diferente para cada modo de jogo
        switch (modoJogo)
        {
            case ModoJogo.Normal:
                linhas = 2;
                colunas = 13;

                centro = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1.0f));

                // Para cada linha, gera um baralho de 13 cartas de mesmo nipe e adiciona as cartas na mesa
                for (int i = 0; i < linhas; i++)
                {
                    baralho = Embaralhar(
                        new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        new List<int> { 0 },
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

                /*
                    Para cada linha, gera um baralho de 13 cartas de mesmo nipe e adiciona as cartas na mesa,
                    informando o grupo de cada linha e, consequentemente, alterando o averso das cartas de cada linha
                */
                for (int i = 0; i < linhas; i++)
                {
                    baralho = Embaralhar(
                        new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        new List<int> { 0 },
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

                // Define centros para separar a exibicao das cartas em dois grupos
                var centros = new List<Vector3>()
                {
                    camera.ViewportToWorldPoint(new Vector3(0.25f, 0.5f, 1.0f)),
                    camera.ViewportToWorldPoint(new Vector3(0.75f, 0.5f, 1.0f))
                };

                /*
                    Itera por cada grupo de cartas e, para cada uma delas, gera um baralho 4x4
                    e adiciona as cartas na mesa
                */
                for (int g = 0; g < 2; g++)
                {
                    centro = centros[g];

                    baralho = Embaralhar(
                        new List<int> { 0, 10, 11, 12 },
                        new List<int> { 0, 1, 2, 3 },
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

    /// <summary>
    /// Adiciona uma carta para ser exibida na mesa
    /// </summary>
    /// <param name="nipe">Nipe da carta</param>
    /// <param name="numero">Numero da carta</param>
    /// <param name="grupo">Grupo da carta, decide a cor do averso</param>
    /// <param name="linha">Linha em que a carta sera inserida</param>
    /// <param name="coluna">Coluna em que a carta sera inserida</param>
    /// <param name="linhas">Total de linhas de cartas a serem adicionadas na mesa</param>
    /// <param name="colunas">Total de colunas de cartas a serem adicionadas na mesa</param>
    /// <param name="centro">Relativo a qual ponto centralizar as cartas</param>
    void AddUmaCarta(int nipe, int numero, int grupo, int linha, int coluna, int linhas, int colunas, Vector3 centro)
    {
        var escalaCartaOriginalX = carta.transform.localScale.x;
        var escalaCartaOriginalY = carta.transform.localScale.y;
        var fatorEscalaX = (650 * escalaCartaOriginalX) / 100.0f;
        var fatorEscalaY = (945 * escalaCartaOriginalY) / 100.0f;

        // Centraliza a carta conforme o centro informado no parametro
        var novaPosicao = new Vector3(
            centro.x + (coluna - colunas / 2.0f) * fatorEscalaX,
            centro.y + (linha - linhas / 2.0f) * fatorEscalaY,
            centro.z
        );
        var c = (GameObject)Instantiate(carta, novaPosicao, Quaternion.identity);
        c.tag = $"{nipe}_{numero}";
        c.name = $"{grupo}_{linha}_{nipe}_{numero}";

        // Realiza tratamento especial quando o numero da carta eh especial (letras)
        var numeroDaCarta = "";
        switch (numero)
        {
            case 0: numeroDaCarta = "ace"; break;
            case 10: numeroDaCarta = "jack"; break;
            case 11: numeroDaCarta = "queen"; break;
            case 12: numeroDaCarta = "king"; break;
            default: numeroDaCarta = $"{(numero + 1)}"; break;
        }

        // Mapeia numero de nipe para o nipe em si
        var nipeDaCarta = "";
        switch (nipe)
        {
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

    /// <summary>
    /// Embaralha as cartas com base nas cartas / nipes informados e na organizacao grid das cartas
    /// </summary>
    /// <param name="valores">Numeros de cartas a serem embaralhadas</param>
    /// <param name="nipes">Nipes de cartas a serem embaralhadas</param>
    /// <param name="linhas">Quantidade de linhas para embaralhar as cartas</param>
    /// <param name="colunas">Quantidade de colunas para embaralhar as cartas</param>
    /// <returns>Lista de cartas embaralhadas, em que o item1 eh o nipe da carta e o item2 eh o numero da carta</returns>
    public List<List<(int, int)>> Embaralhar(List<int> valores, List<int> nipes, int linhas, int colunas)
    {
        var cartas = new List<(int, int)>();

        // Gera todas as combinacoes de numeros de cartas + nipes
        foreach (var n in nipes)
        {
            foreach (var v in valores)
            {
                cartas.Add((n, v));
            }
        }

        // Embaralha as combinacoes geradas, organizando conforme a quantidade de linhas e colunas
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

    /// <summary>
    /// Acao realizada quando uma carta eh selecionada
    /// </summary>
    /// <param name="carta">Carta que foi selecionada</param>
    public void CartaSelecionada(GameObject carta)
    {
        if (!primeiraCartaSelecionada)
        {
            string linha = carta.name.Substring(0, 1);
            primeiraCartaSelecionada = true;
            carta1 = carta;
            carta1.GetComponent<Tile>().RevelaCarta();
        }
        else if (primeiraCartaSelecionada && !segundaCartaSelecionada)
        {
            // Se a mesma carta foi selecionada duas vezes, desconsiderar
            if (carta1.name == carta.name) return;

            string linha = carta.name.Substring(0, 1);
            segundaCartaSelecionada = true;
            carta2 = carta;
            carta2.GetComponent<Tile>().RevelaCarta();
            VerificaCarta();
        }
    }

    /// <summary>
    /// Atualiza as tentativas quando duas cartas foram selecionadas e precisam ser verificadas
    /// </summary>
    public void VerificaCarta()
    {
        DisparaTimer();
        numTentativas++;
        UpdateTentativas();
    }

    /// <summary>
    /// Dispara o timer de verificacao das cartas.
    /// </summary>
    public void DisparaTimer()
    {
        timerPausado = false;
        timerAcionado = true;
    }

    /// <summary>
    /// Atualiza as tentativas
    /// </summary>
    void UpdateTentativas()
    {
        GameObject.Find("numTentativas").GetComponent<Text>().text = "Tentativas: " + numTentativas;

    }

    /* Usado para checar o numero de tentativas menor já feito no jogo */
    public void ChecarTentativas()
    {
        if (tentativasMin > numTentativas)
        {
            tentativasMin = numTentativas;
            PlayerPrefs.SetInt(nomeDaCena + "TentativasMin", tentativasMin);
            print("Dentro");
        }
    }

    /* Usado para checar o numero de tentativas obtido na partida anterior */
    public void ChecarTentativaAnterior()
    {
        tentativasAnterior = numTentativas;
        PlayerPrefs.SetInt(nomeDaCena + "TentativasAnterior", tentativasAnterior);
    }
}

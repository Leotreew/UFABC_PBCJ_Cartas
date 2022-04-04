using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Representa uma carta do jogo
public class Tile : MonoBehaviour
{
    // Sprite da Carta desejada
    public Sprite originalCarta;

    // Sprite do avesso da carta
    public Sprite backCarta;

    // Indicador da carta virada ou nao
    bool tileRevelada = false;

    // Start is called before the first frame update
    void Start()
    {
        EscondeCarta();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Acao a executar ao clicar na carta
    public void OnMouseDown()
    {
        GameObject.Find("gameManager").GetComponent<ManageCartas>().CartaSelecionada(gameObject);
    }

    // Esconde a carta, i.e. vira-a ao avesso
    public void EscondeCarta()
    {
        GetComponent<SpriteRenderer>().sprite = backCarta;
        tileRevelada = false;
    }

    // Revela a carta, i.e. mostra a frente da carta
    public void RevelaCarta()
    {
        GetComponent<SpriteRenderer>().sprite = originalCarta;
        tileRevelada = true;
    }

    // Define qual o sprite para ser usado como frente da carta
    public void setCartaOriginal(Sprite novaCarta)
    {
        originalCarta = novaCarta;
    }

    // Define qual o sprite para ser usado como fundo da carta
    public void setCartaFundo(Sprite fundoCarta)
    {
        backCarta = fundoCarta;
    }
}

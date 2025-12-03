using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerSalud))]
[RequireComponent(typeof(PlayerMovimiento))]
public class PlayerStatusManager : MonoBehaviour
{
    // === GESTOR DE ESTADOS (VENENO / HIELO) ===
    // Añade este script a tu Player para que pueda sufrir efectos especiales.

    [Header("Visuales")]
    [SerializeField] private Color colorVeneno = Color.green;
    [SerializeField] private Color colorHielo = Color.cyan;

    private PlayerSalud _salud;
    private PlayerMovimiento _movimiento;
    private SpriteRenderer _sr;
    private Color _colorOriginal;

    void Start()
    {
        _salud = GetComponent<PlayerSalud>();
        _movimiento = GetComponent<PlayerMovimiento>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        if (_sr) _colorOriginal = _sr.color;
    }

    public void AplicarVeneno(int dañoTotal, float duracion)
    {
        StartCoroutine(RutinaVeneno(dañoTotal, duracion));
    }

    public void AplicarCongelacion(float factorLentitud, float duracion)
    {
        StartCoroutine(RutinaCongelacion(factorLentitud, duracion));
    }

    IEnumerator RutinaVeneno(int daño, float tiempo)
    {
        Debug.Log("¡Player Envenenado!");
        if (_sr) _sr.color = colorVeneno;
        
        int ticks = 5;
        float intervalo = tiempo / ticks;
        int dañoPorTick = Mathf.Max(1, daño / ticks);

        for (int i = 0; i < ticks; i++)
        {
            if (_salud) _salud.RecibirDamage(dañoPorTick);
            yield return new WaitForSeconds(intervalo);
        }

        if (_sr) _sr.color = _colorOriginal;
    }

    IEnumerator RutinaCongelacion(float factor, float tiempo)
    {
        Debug.Log("¡Player Congelado!");
        if (_sr) _sr.color = colorHielo;
        if (_movimiento) _movimiento.SetMultiplicadorVelocidad(factor);

        yield return new WaitForSeconds(tiempo);

        if (_movimiento) _movimiento.SetMultiplicadorVelocidad(1f);
        if (_sr) _sr.color = _colorOriginal;
    }
}
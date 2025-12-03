/*******************************************************
 * NOMBRE DEL ARCHIVO: PlayerStatusManager.cs
 * AUTORES: Hannin Abarca, Gael Jimenez, David Sanchez
 * * DESCRIPCIÓN: 
 * Gestiona los efectos de estado (Debuffs) aplicados al jugador.
 * Se comunica con PlayerSalud y PlayerMovimiento.
 * * REFERENCIA LÓGICA: 
 * - Status Effect System: https://www.youtube.com/watch?v=PbL7Wk7sZ94
 *******************************************************/

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerSalud))]
[RequireComponent(typeof(PlayerMovimiento))]
public class PlayerStatusManager : MonoBehaviour
{
    [Header("Configuración Visual")]
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
        if (_sr) _colorOriginal = _sr.color; // Guardar color base para restaurarlo luego
    }

    /// <summary>
    /// Aplica daño por segundo (DoT) al jugador.
    /// </summary>
    public void AplicarVeneno(int dañoTotal, float duracion)
    {
        StartCoroutine(RutinaVeneno(dañoTotal, duracion));
    }

    private IEnumerator RutinaVeneno(int daño, float tiempo)
    {
        Debug.Log("¡Player Envenenado!");
        if (_sr) _sr.color = colorVeneno;
        
        int ticks = 5; // Número de veces que aplica daño
        float intervalo = tiempo / ticks;
        int dañoPorTick = Mathf.Max(1, daño / ticks);

        for (int i = 0; i < ticks; i++)
        {
            if (_salud != null) 
            {
                _salud.RecibirDamage(dañoPorTick);
            }
            yield return new WaitForSeconds(intervalo);
        }

        // Restaurar color
        if (_sr) _sr.color = _colorOriginal;
    }

    /// <summary>
    /// Ralentiza al jugador modificando su multiplicador de velocidad.
    /// </summary>
    public void AplicarCongelacion(float factor, float duracion)
    {
        StartCoroutine(RutinaCongelacion(factor, duracion));
    }

    private IEnumerator RutinaCongelacion(float factor, float tiempo)
    {
        Debug.Log("¡Player Congelado!");
        if (_sr) _sr.color = colorHielo;
        
        // Reducir velocidad
        if (_movimiento) _movimiento.SetMultiplicadorVelocidad(factor);

        yield return new WaitForSeconds(tiempo);

        // Restaurar velocidad
        if (_movimiento) _movimiento.SetMultiplicadorVelocidad(1f);
        if (_sr) _sr.color = _colorOriginal;
    }
}
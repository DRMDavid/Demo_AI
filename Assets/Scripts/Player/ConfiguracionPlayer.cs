/*******************************************************
 * NOMBRE DEL ARCHIVO: ConfiguracionPlayer.cs
 * AUTORES: Gael, David, Steve
 * CURSO: Aprende a crear un videojuego de Acción 2D con Unity - Gianny Dantas (Udemy)
 * FUENTE: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity
 * 
 * DESCRIPCIÓN:
 * Este ScriptableObject almacena la configuración base y los valores
 * actuales del jugador, tales como salud, armadura, energía y estadísticas
 * adicionales. Sirve como contenedor de datos que puede ser modificado
 * fácilmente desde el editor de Unity.
 * 
 * FECHA: 05/10
 * 
 * NOTAS:
 * - No se consultaron fuentes adicionales.
 * - Código basado en el curso mencionado, con comentarios propios para fines educativos.
 *******************************************************/

using UnityEngine;

[CreateAssetMenu] // Permite crear instancias de este ScriptableObject desde el menú de Unity.
public class ConfiguracionPlayer : ScriptableObject
{
    [Header("Datos")]
    public int Nivel;          // Nivel actual del jugador.
    public string Nombre;      // Nombre del jugador, mostrado en UI o estadísticas.
    public Sprite Icono;       // Icono que representa al jugador en interfaz o menús.

    [Header("Valores")]
    public float SaludActual;  // Salud actual del jugador.
    public float SaludMax;     // Salud máxima que puede tener el jugador.
    public float Armadura;     // Cantidad actual de armadura (protección).
    public float ArmaduraMax;  // Valor máximo de armadura posible.
    public float Energia;      // Energía o stamina actual del jugador.
    public float EnergiaMax;   // Energía máxima del jugador.
    public float ChanceCritico; // Probabilidad de realizar un golpe crítico (en %).
    public float DañoCritico;   // Multiplicador de daño en caso de golpe crítico.
}

/*******************************************************
 * NOMBRE DEL ARCHIVO: PlayerShooter.cs
 * AUTOR: Gael, David, Steve
 * * BASADO EN:
 * Curso "Aprende a crear un videojuego de Acción 2D con Unity"
 * Instructor: Gianny Dantas (Udemy)
 * Fuente: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/
 * * DESCRIPCIÓN:
 * Script completamente original, basado en la mecánica de disparo
 * del curso mencionado, pero reescrito desde cero.
 * * Se encarga de:
 * - Crear e instanciar proyectiles hacia el cursor.
 * - Controlar la cadencia de disparo (fire rate).
 * - Rotar el arma hacia la posición del mouse.
 * - Reproducir sonido de disparo mediante un AudioSource.
 * * DIFERENCIAS RESPECTO AL CURSO:
 * - No utiliza ScriptableObjects ni clases separadas (Item, Weapon, ArmaPistola).
 * - Integra toda la lógica en un solo script.
 * - Implementa un sistema de sonido propio.
 * - Mejora la verificación de referencias y control de errores.
 * * FUENTES CONSULTADAS:
 * - Curso de Udemy (Gianny Dantas)
 * - Documentación oficial de Unity:
 * https://docs.unity3d.com/ScriptReference/AudioSource.PlayOneShot.html
 * https://docs.unity3d.com/ScriptReference/Input.GetMouseButton.html
 * * FECHA: 05/10
 *******************************************************/

using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    public GameObject bulletPrefab;           // Prefab de la bala del jugador.
    public Transform firePoint;               // Punto desde donde se dispara.
    public int proyectilDamage = 2;           // Daño infligido por el proyectil.
    public float proyectilSpeed = 12f;        // Velocidad de la bala.
    public float fireRate = 0.25f;            // Tiempo mínimo entre disparos.

    [Header("Colores del Proyectil 🎨")]
    // 💡 Variables nuevas para feedback visual del Power-Up
    public Color colorNormal = Color.white;      // Color estándar.
    public Color colorPerforante = Color.red;    // Color al tener balas perforantes.

    [Header("Referencias Visuales")]
    public Transform armaSprite;              // Sprite del arma, rota hacia el cursor.

    [Header("Sonido de Disparo")]
    public AudioClip shootSound;              // Clip de audio del disparo.

    // --- Variables Privadas ---
    private float lastFireTime;               // Control de tiempo entre disparos.
    private AudioSource audioSource;          // Fuente de audio para reproducir el sonido.

    // --- 💡 Variables para Power-Ups (NUEVO) ---
    private bool tieneEscopeta = false;
    private int nivelPerforacion = 0;

    private void Awake()
    {
        // Busca o agrega automáticamente un componente AudioSource.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // Mantener presionado el click izquierdo para disparar.
        if (Input.GetMouseButton(0))
        {
            TryShoot();
        }

        // Actualiza la rotación del arma hacia el cursor.
        ApuntarArma();
    }

    /// <summary>
    /// Controla el disparo del jugador verificando fire rate y referencias.
    /// </summary>
    private void TryShoot()
    {
        if (Time.time < lastFireTime + fireRate)
            return;

        if (bulletPrefab == null || firePoint == null)
            return;

        lastFireTime = Time.time;

        // Calcula la dirección hacia el mouse.
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - firePoint.position).normalized;

        // 💡 LÓGICA MODIFICADA: Escopeta vs Disparo Normal
        if (tieneEscopeta)
        {
            // Dispara 3 balas en abanico
            CrearBala(dir, 0f);   // Centro
            CrearBala(dir, 15f);  // Derecha (+15 grados)
            CrearBala(dir, -15f); // Izquierda (-15 grados)
        }
        else
        {
            // Disparo único normal
            CrearBala(dir, 0f);
        }

        // Reproduce el sonido del disparo sin interrumpir otros sonidos.
        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    /// <summary>
    /// 💡 NUEVO: Método auxiliar para instanciar la bala con rotación y configuración.
    /// </summary>
    private void CrearBala(Vector2 direccionBase, float anguloExtra)
    {
        // Rota el vector de dirección original según el ángulo extra
        Vector2 dirFinal = Quaternion.Euler(0, 0, anguloExtra) * direccionBase;

        // Instancia la bala.
        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        PlayerBullet bullet = go.GetComponent<PlayerBullet>();

        if (bullet != null)
        {
            bullet.Init(dirFinal, proyectilSpeed, proyectilDamage);
            bullet.SetPerforaciones(nivelPerforacion); // Aplica la perforación actual

            // 💡 CAMBIO DE COLOR: Feedback visual si es perforante
            if (nivelPerforacion > 0)
            {
                bullet.SetColor(colorPerforante);
            }
            else
            {
                bullet.SetColor(colorNormal);
            }
        }
        else
        {
            Debug.LogError("El prefab de la bala NO tiene el script PlayerBullet!");
        }
    }

    /// <summary>
    /// Rota el sprite del arma para apuntar hacia el cursor del mouse.
    /// </summary>
    private void ApuntarArma()
    {
        if (armaSprite == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 direccion = mouseWorld - armaSprite.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        armaSprite.rotation = Quaternion.Euler(0f, 0f, angulo);
    }

    /// <summary>
    /// Dibuja guías visuales del punto de disparo y dirección del mouse (solo en editor).
    /// </summary>
    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.05f);

            if (Application.isPlaying && Camera.main != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(firePoint.position, mouseWorld);
            }
        }
    }

    // --- MÉTODOS PÚBLICOS PARA POWER-UPS ---

    public void ModificarFireRate(float nuevoValor)
    {
        fireRate = nuevoValor;
    }

    // Activa o desactiva el modo escopeta
    public void ActivarEscopeta(bool estado)
    {
        tieneEscopeta = estado;
    }

    // Configura cuántos enemigos pueden atravesar las balas
    public void SetPerforacion(int cantidad)
    {
        nivelPerforacion = cantidad;
    }
}
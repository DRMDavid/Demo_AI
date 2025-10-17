using UnityEngine;
using System.Collections;

/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez

Referencia:
Este código se basó en las ideas y mecánicas presentadas en el siguiente video:
https://www.youtube.com/watch?v=Qp8rauCBLkY&t=50s
*/

/// <summary>
/// TurretEnemy: enemigo tipo torre que detecta al jugador mediante un cono de visión,
/// rota el cono periódicamente y dispara proyectiles al jugador si está detectado.
/// Hereda de BaseEnemy para manejar salud, daño y lógica básica de IA.
/// </summary>
public class TurretEnemy : BaseEnemy
{
    [Header("Cono de visión")]
    public Transform visionCone;           // Objeto que rota, padre del cono visual
    public float visionAngle = 45f;        // Ángulo total del cono de visión
    public float visionDistance = 5f;      // Distancia máxima de detección
    public float rotationStep = 45f;       // Paso de rotación por intervalo
    public float rotationInterval = 2f;    // Tiempo entre rotaciones

    [Header("Visuales del Cono (Mesh)")]
    public MeshFilter visionConeMeshFilter; // MeshFilter del cono de visión
    public Color defaultColor = new Color(1, 1, 0, 0.25f);  // Color normal del cono
    public Color detectedColor = new Color(1, 0, 0, 0.4f);  // Color cuando detecta jugador

    [Header("Renderizado 2D")]
    public string sortingLayer = "Default";  // Capa de render
    public int orderInLayer = 1;             // Orden en la capa

    [Header("Disparo")]
    public GameObject bulletPrefab;          // Prefab del proyectil
    public Transform firePoint;              // Punto de disparo
    public float bulletSpeed = 10f;          // Velocidad de la bala
    public float fireRate = 1f;              // Disparos por segundo

    [Header("Detección")]
    public float timeToLoseTarget = 2f;      // Tiempo que tarda en perder al jugador

    // --- Variables Privadas ---
    private bool playerDetected = false;         // Estado de detección del jugador
    private bool rotating = true;                // Si el cono está rotando
    private Transform player;                    // Transform del jugador
    private Coroutine rotationCoroutine;         // Coroutine para rotación
    private Coroutine fireCoroutine;             // Coroutine para disparo
    private float lastDetectionTime;             // Tiempo de última detección

    private Mesh visionMesh;                     // Mesh dinámico para el cono
    private MeshRenderer visionConeMeshRenderer; // Renderer del Mesh

    /// <summary>
    /// Inicialización del TurretEnemy: asigna componentes, configura el mesh del cono y busca al jugador.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // Configuración del Mesh del cono
        if (visionConeMeshFilter != null)
        {
            visionMesh = new Mesh();
            visionConeMeshFilter.mesh = visionMesh;
            visionConeMeshRenderer = visionConeMeshFilter.GetComponent<MeshRenderer>();

            // Configuración del render 2D
            if (visionConeMeshRenderer != null)
            {
                visionConeMeshRenderer.sortingLayerName = sortingLayer;
                visionConeMeshRenderer.sortingOrder = orderInLayer;
            }
        }

        GenerateConeMesh();
        if (visionConeMeshRenderer != null)
            visionConeMeshRenderer.material.color = defaultColor;

        // Búsqueda inicial del jugador en escena
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        // Inicia la rotación del cono
        rotationCoroutine = StartCoroutine(RotateVisionCone());
    }

    /// <summary>
    /// Update: se encarga de actualizar el mesh en editor y detectar al jugador.
    /// </summary>
    void Update()
    {
        // Actualización visual en editor
#if UNITY_EDITOR
        if (visionConeMeshFilter != null && visionMesh != null)
        {
            GenerateConeMesh();
        }
#endif

        DetectPlayer();
    }

    /// <summary>
    /// Detecta al jugador dentro del cono y gestiona la rotación y disparo.
    /// </summary>
    void DetectPlayer()
    {
        if (player == null) return;

        Vector2 dirToPlayer = (player.position - visionCone.position).normalized;
        float angle = Vector2.Angle(visionCone.right, dirToPlayer);
        float distance = Vector2.Distance(visionCone.position, player.position);

        // --- Lógica de detección ---
        if (angle < visionAngle * 0.5f && distance <= visionDistance)
        {
            lastDetectionTime = Time.time;
            if (!playerDetected)
            {
                // Jugador detectado por primera vez
                playerDetected = true;
                rotating = false;
                if (visionConeMeshRenderer != null)
                    visionConeMeshRenderer.material.color = detectedColor;

                // Detener rotación y comenzar disparo
                if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
                fireCoroutine = StartCoroutine(FireAtPlayer());
            }
        }
        else
        {
            // Jugador fuera del cono
            if (playerDetected && Time.time > lastDetectionTime + timeToLoseTarget)
            {
                // Perder al jugador
                playerDetected = false;
                rotating = true;
                if (visionConeMeshRenderer != null)
                    visionConeMeshRenderer.material.color = defaultColor;

                // Detener disparo y reanudar rotación
                if (fireCoroutine != null) StopCoroutine(fireCoroutine);
                rotationCoroutine = StartCoroutine(RotateVisionCone());
            }
        }
    }

    /// <summary>
    /// Coroutine que rota el cono de visión a intervalos regulares.
    /// </summary>
    IEnumerator RotateVisionCone()
    {
        while (rotating)
        {
            visionCone.Rotate(Vector3.forward, rotationStep);
            yield return new WaitForSeconds(rotationInterval);
        }
    }

    /// <summary>
    /// Coroutine que dispara proyectiles al jugador mientras esté detectado.
    /// </summary>
    IEnumerator FireAtPlayer()
    {
        while (playerDetected)
        {
            if (player == null) yield break;

            if (bulletPrefab != null && firePoint != null)
            {
                Vector2 dir = (player.position - firePoint.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = dir * bulletSpeed;
            }

            yield return new WaitForSeconds(1f / fireRate);
        }
    }

    /// <summary>
    /// Genera dinámicamente el Mesh del cono de visión.
    /// </summary>
    void GenerateConeMesh()
    {
        if (visionMesh == null) return;

        Vector3[] vertices = new Vector3[3];
        int[] triangles = new int[] { 0, 1, 2 };

        vertices[0] = Vector3.zero;
        float halfAngleRad = (visionAngle * 0.5f) * Mathf.Deg2Rad;

        vertices[1] = new Vector3(Mathf.Cos(halfAngleRad), Mathf.Sin(halfAngleRad), 0) * visionDistance;
        vertices[2] = new Vector3(Mathf.Cos(-halfAngleRad), Mathf.Sin(-halfAngleRad), 0) * visionDistance;

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
    }
}

using UnityEngine;
using System.Collections;

public class TurretEnemy : BaseEnemy
{
    [Header("Cono de visión")]
    public Transform visionCone;           // El objeto que rota (el padre del cono visual)
    public float visionAngle = 45f;
    public float visionDistance = 5f;
    public float rotationStep = 45f;
    public float rotationInterval = 2f;

    [Header("Visuales del Cono (Mesh)")]
    public MeshFilter visionConeMeshFilter; // Arrastra aquí el objeto hijo que tiene el MeshFilter
    public Color defaultColor = new Color(1, 1, 0, 0.25f);
    public Color detectedColor = new Color(1, 0, 0, 0.4f);
    
    [Header("Renderizado 2D")]
    public string sortingLayer = "Default";
    public int orderInLayer = 1;

    [Header("Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint;            
    public float bulletSpeed = 10f;
    public float fireRate = 1f;

    [Header("Detección")]
    public float timeToLoseTarget = 2f;

    // --- Variables Privadas ---
    private bool playerDetected = false;
    private bool rotating = true;
    private Transform player;
    private Coroutine rotationCoroutine;
    private Coroutine fireCoroutine;
    private float lastDetectionTime;
    
    private Mesh visionMesh;
    private MeshRenderer visionConeMeshRenderer;

    protected override void Start()
    {
        base.Start();

        // --- Configuración del Cono Visual ---
        if (visionConeMeshFilter != null)
        {
            visionMesh = new Mesh();
            visionConeMeshFilter.mesh = visionMesh;
            visionConeMeshRenderer = visionConeMeshFilter.GetComponent<MeshRenderer>();
            
            // Asignamos la capa y el orden para que se vea en 2D
            if (visionConeMeshRenderer != null)
            {
                visionConeMeshRenderer.sortingLayerName = sortingLayer;
                visionConeMeshRenderer.sortingOrder = orderInLayer;
            }
        }
        
        GenerateConeMesh();
        if (visionConeMeshRenderer != null)
        {
            visionConeMeshRenderer.material.color = defaultColor;
        }

        // --- Búsqueda inicial del jugador ---
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // Inicia la rotación
        rotationCoroutine = StartCoroutine(RotateVisionCone());
    }

    void Update()
    {
        // Actualiza el cono en el editor para ver cambios en tiempo real
        #if UNITY_EDITOR
        if (visionConeMeshFilter != null && visionMesh != null)
        {
            GenerateConeMesh();
        }
        #endif

        DetectPlayer();
    }

    void DetectPlayer()
    {
        if (player == null) return;

        Vector2 dirToPlayer = (player.position - visionCone.position).normalized;
        float angle = Vector2.Angle(visionCone.right, dirToPlayer);
        float distance = Vector2.Distance(visionCone.position, player.position);

        // --- Lógica de Detección ---
        if (angle < visionAngle * 0.5f && distance <= visionDistance)
        {
            lastDetectionTime = Time.time;
            if (!playerDetected)
            {
                // Jugador detectado por primera vez
                playerDetected = true;
                rotating = false;
                if (visionConeMeshRenderer != null) visionConeMeshRenderer.material.color = detectedColor;
                
                if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
                fireCoroutine = StartCoroutine(FireAtPlayer());
            }
        }
        else
        {
            // Jugador fuera del cono
            if (playerDetected && Time.time > lastDetectionTime + timeToLoseTarget)
            {
                // Perder al jugador tras el tiempo de espera
                playerDetected = false;
                rotating = true;
                if (visionConeMeshRenderer != null) visionConeMeshRenderer.material.color = defaultColor;
                
                if (fireCoroutine != null) StopCoroutine(fireCoroutine);
                rotationCoroutine = StartCoroutine(RotateVisionCone());
            }
        }
    }

    IEnumerator RotateVisionCone()
    {
        while (rotating)
        {
            visionCone.Rotate(Vector3.forward, rotationStep);
            yield return new WaitForSeconds(rotationInterval);
        }
    }

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
                {
                    rb.linearVelocity = dir * bulletSpeed;
                }
            }

            yield return new WaitForSeconds(1f / fireRate);
        }
    }

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
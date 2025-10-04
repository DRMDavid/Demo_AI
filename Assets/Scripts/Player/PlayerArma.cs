// PlayerArma.cs
using UnityEngine;

public class PlayerArma : MonoBehaviour
{
    public enum TipoArma { Melee, Pistola }

    [Header("Configuración")]
    public TipoArma armaEquipada = TipoArma.Melee;

    [Header("Melee")]
    public int damageMelee = 2;
    public float cadenciaMelee = 0.8f;
    public float rangoMelee = 1f;

    [Header("Pistola")]
    public int damagePistola = 3;
    public float cadenciaPistola = 0.3f;
    public GameObject prefabProyectil;
    public float velocidadProyectil = 12f;

    [Header("Referencias")]
    public Transform puntoDisparo; // opcional, puede ser el mismo transform del jugador

    private Arma armaActual;

    void Start()
    {
        EquiparArma(armaEquipada);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            armaActual?.Atacar();
        }

        // Cambiar arma con teclas (opcional)
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquiparArma(TipoArma.Melee);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquiparArma(TipoArma.Pistola);
    }

    public void EquiparArma(TipoArma tipo)
    {
        // Destruir arma anterior si es un GameObject hijo
        if (armaActual != null && armaActual is MonoBehaviour mb)
        {
            Destroy(mb.gameObject);
        }

        armaEquipada = tipo;

        GameObject armaGO = new GameObject("ArmaEquipada");
        armaGO.transform.parent = puntoDisparo ? puntoDisparo : transform;
        armaGO.transform.localPosition = Vector3.zero;
        armaGO.transform.localRotation = Quaternion.identity;

        if (tipo == TipoArma.Melee)
        {
            ArmaMelee arma = armaGO.AddComponent<ArmaMelee>();
            arma.Damage= damageMelee;
            arma.cadencia = cadenciaMelee;
            arma.rango = rangoMelee;
            armaActual = arma;
        }
        else
        {
            ArmaPistola arma = armaGO.AddComponent<ArmaPistola>();
            arma.Damage = damagePistola;
            arma.cadencia = cadenciaPistola;
            arma.prefabProyectil = prefabProyectil;
            arma.velocidadProyectil = velocidadProyectil;
            armaActual = arma;
        }
    }
}
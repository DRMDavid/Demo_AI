using UnityEngine;

[CreateAssetMenu]
public class ConfiguracionPlayer : ScriptableObject
{
    [Header("Datos")]
    public int Nivel;
    public string Nombre;
    public Sprite Icono;

    [Header("Valores")] 
    public float SaludActual;
    public float SaludMax;
    public float Armadura;
    public float ArmaduraMax;
    public float Energia;
    public float EnergiaMax;
    public float ChanceCritico;
    public float DañoCritico;

    [Header("Valores Mejora")]
    public float SaludMejoraMax;
    public float ArmaduraMejoraMax;
    public float EnergiaMejoraMax;
    public float ChanceCriticoMejoraMax;
    
    [Header("Extra")] 
    public bool Desbloqueado;
    public float CostoDesbloquear;
    public float CostoMejorar;
    [Range(0, 100)] public int MultiplicadorMejora;

    [Header("Prefab")]
    public GameObject PlayerPrefab;
    
    public void ResetPlayerStats()
    {
        SaludActual = SaludMax;
        Armadura = ArmaduraMax;
        Energia = EnergiaMax;
    }
}
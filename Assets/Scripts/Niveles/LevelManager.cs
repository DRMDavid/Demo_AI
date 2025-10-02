using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : MonoBehaviour
{ 
    public static LevelManager Instance;
  
    [FormerlySerializedAs("platillas")]
    [Header("Congfi")]
    [SerializeField] private PlatillasDeNivel roomPlantillas;
    public PlatillasDeNivel RoomPlantillas  => roomPlantillas;

    private void Awake()
    {
        Instance = this;
    }
}
/*using UnityEngine;

public class ArmaPistola : Arma
{
    [Header("Proyectil")]
    [SerializeField] private Proyectil proyectilPrefab;

    public override void UsarArma()
    {
        MostrarAnimacion();
        
        // Crear un proyectil
        Proyectil proyectil = Instantiate(proyectilPrefab);
        proyectil.transform.position = posDisparo.position;
        proyectil.Direccion = posDisparo.right;

        if (PersonajeArmaParent is PlayerArma player) // Player
        {
            proyectil.Daño = player.ObtenerDañoConsiderandoCritico();
        }
        else // Enemigo
        {
            proyectil.Daño = itemArma.Daño;
        }
        
        // Dispersion
        float dispersionRandom = Random.Range(itemArma.DispersionMin, itemArma.DispersionMax);
        proyectil.transform.rotation = Quaternion.Euler(dispersionRandom * Vector3.forward);
    }
}
*/
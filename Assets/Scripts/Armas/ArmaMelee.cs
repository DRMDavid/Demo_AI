/*using UnityEngine;

public class ArmaMelee : Arma
{
    private IRecibirDaño entidadObjetivo;
    
    public override void UsarArma()
    {
        MostrarAnimacion();
        AtacarObjetivo();
    }

    private void AtacarObjetivo()
    {
        if (entidadObjetivo != null)
        {
            if (PersonajeArmaParent is PlayerArma player)
            {
                entidadObjetivo.RecibirDaño(player.ObtenerDañoConsiderandoCritico());
            }
            else
            {
                entidadObjetivo.RecibirDaño(itemArma.Daño);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IRecibirDaño entidad = other.GetComponent<IRecibirDaño>();
        if (entidad != null)
        {
            entidadObjetivo = entidad;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IRecibirDaño entidad = other.GetComponent<IRecibirDaño>();
        if (entidad != null)
        {
            entidadObjetivo = null;
        }
    }
}
*/
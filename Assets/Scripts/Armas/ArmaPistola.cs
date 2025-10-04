// ArmaPistola.cs
using UnityEngine;

public class ArmaPistola : Arma
{
    public GameObject prefabProyectil;
    public float velocidadProyectil = 10f;

    public override void Atacar()
    {
        if (!PuedeAtacar() || prefabProyectil == null) return;

        ultimoAtaque = Time.time;

        GameObject proyectilGO = Instantiate(prefabProyectil, transform.position, transform.rotation);
        Proyectil proyectil = proyectilGO.GetComponent<Proyectil>();
        if (proyectil != null)
        {
            proyectil.Inicializar(Damage, velocidadProyectil);
        }
        else
        {
            // Si no tiene Proyectil.cs, usa el método antiguo
            Rigidbody2D rb = proyectilGO.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = transform.right * velocidadProyectil;
            else
                proyectilGO.transform.Translate(Vector3.right * velocidadProyectil * Time.deltaTime);
        }
    }
}
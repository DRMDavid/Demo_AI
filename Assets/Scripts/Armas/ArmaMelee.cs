// ArmaMelee.cs
using UnityEngine;

public class ArmaMelee : Arma
{
    public float rango = 1f;

    public override void Atacar()
    {
        if (!PuedeAtacar()) return;

        ultimoAtaque = Time.time;

        // Detectar enemigos cercanos
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, rango);
        foreach (Collider2D hit in hits)
        {
            BaseEnemy enemigo = hit.GetComponent<BaseEnemy>();
            if (enemigo != null)
            {
                enemigo.TakeDamage(Damage); // asumiendo que BaseEnemy tiene este método
            }
        }

        // Opcional: reproducir animación o sonido
    }

    // Opcional: dibujar gizmo en editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rango);
    }
}
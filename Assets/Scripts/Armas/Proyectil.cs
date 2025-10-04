// Proyectil.cs (actualizado)
using UnityEngine;

public class Proyectil : MonoBehaviour
{
    private int damage;
    private float velocidad;

    public void Inicializar(int damage, float velocidad)
    {
        this.damage = damage;
        this.velocidad = velocidad;
    }

    void Update()
    {
        transform.Translate(Vector2.right * velocidad * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemigo"))
        {
            BaseEnemy enemigo = other.GetComponent<BaseEnemy>();
            if (enemigo != null)
            {
                enemigo.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
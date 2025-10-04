using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Config")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public int proyectilDamage = 2;
    public float proyectilSpeed = 12f;
    public float fireRate = 0.25f;

    [Header("Referencias")]
    public Transform armaSprite; // sprite del arma que rota

    private float lastFireTime;

    private void Update()
    {
        if (Input.GetMouseButton(0)) // click izquierdo
        {
            TryShoot();
        }

        ApuntarArma();
    }

    private void TryShoot()
    {
        if (Time.time < lastFireTime + fireRate) return;
        if (bulletPrefab == null || firePoint == null) return;

        lastFireTime = Time.time;

        // Dirección hacia el mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - firePoint.position).normalized;

        // Instanciar bala
        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        PlayerBullet bullet = go.GetComponent<PlayerBullet>();
        if (bullet != null)
        {
            bullet.Init(dir, proyectilSpeed, proyectilDamage);
        }
        else
        {
            Debug.LogError("El prefab de la bala NO tiene el script PlayerBullet!");
        }
    }

    private void ApuntarArma()
    {
        if (armaSprite == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 direccion = mouseWorld - armaSprite.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        armaSprite.rotation = Quaternion.Euler(0f, 0f, angulo);
    }

    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            // 🔴 punto en el FirePoint
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.05f);

            // 🟡 línea hacia el mouse (solo en modo juego)
            if (Camera.main != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(firePoint.position, mouseWorld);
            }
        }
    }
}

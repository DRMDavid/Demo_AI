using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Objeto que la cámara debe seguir (tu personaje/Player)
    public Transform target;

    // Velocidad con la que la cámara se mueve hacia el objetivo
    public float smoothSpeed = 0.125f; 

    // Desplazamiento de la cámara respecto al objetivo (ej: para que no esté justo en el centro)
    public Vector3 offset; 

    void LateUpdate()
    {
        // 1. Verificar si hay un objetivo
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: ¡No hay objetivo (Target) asignado! Arrastra tu personaje al campo 'Target' en el Inspector.");
            return;
        }

        // 2. Calcular la posición deseada de la cámara
        Vector3 desiredPosition = target.position + offset;
        
        // Es importante mantener la coordenada Z de la cámara
        desiredPosition.z = transform.position.z;

        // 3. Suavizar el movimiento de la cámara (interpolación lineal)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // 4. Aplicar la nueva posición
        transform.position = smoothedPosition;
    }
}
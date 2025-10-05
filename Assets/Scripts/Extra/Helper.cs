/*
Este código fue tomado y adaptado de un curso de Udemy del creador Gianny Dantas:
"Aprende a crear un videojuego de acción 2D con Unity"
Enlace: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/?couponCode=KEEPLEARNING
Integrantes del Equipo: 
Hannin Abarca 
Gael Jimenez 
David Sanchez 
*/
using System.Collections;
using UnityEngine;

// Clase estática que contiene métodos de utilidad que no requieren ser instanciados.
public static class Helper
{
    // Corrutina que gestiona un efecto de fundido (fade-in o fade-out) de un elemento de UI.
    // Recibe el CanvasGroup a modificar, el valor de transparencia final y la duración.
    public static IEnumerator IEFade(CanvasGroup canvasGroup, float valorDeseado, float tiempoFade)
    {
        float timer = 0;
        // Guarda la transparencia actual del CanvasGroup como punto de inicio.
        float valorInicial = canvasGroup.alpha;
        
        // Bucle de la corrutina que se ejecuta mientras no se haya alcanzado el tiempo total de fade.
        while (timer < tiempoFade)
        {
            // Calcula el nuevo valor de 'alpha' (transparencia) usando interpolación lineal (Lerp).
            // 'timer / tiempoFade' calcula el porcentaje de progreso del fade (de 0 a 1).
            canvasGroup.alpha = Mathf.Lerp(valorInicial, valorDeseado, timer / tiempoFade);
            // Incrementa el temporizador con el tiempo transcurrido desde el último frame.
            timer += Time.deltaTime;
            // Detiene la ejecución hasta el siguiente frame.
            yield return null;
        }

        // Asegura que el valor final (alpha) sea exactamente el deseado al terminar el bucle.
        canvasGroup.alpha = valorDeseado;
    }
}
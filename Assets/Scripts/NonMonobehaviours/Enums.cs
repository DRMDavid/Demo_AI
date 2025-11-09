/*******************************************************
 * NOMBRE DEL ARCHIVO: Enums.cs
 * AUTOR: Gael, David y Steve
 * BASADO EN: Código original del repositorio
 * DESCRIPCIÓN:
 * Archivo de enumeraciones utilizado por varios scripts del juego.
 * Actualizado para incluir los estados del Jefe (Boss FSM).
 *******************************************************/

using UnityEngine;

// Enum 
public enum ESteeringBehaviors : byte
{
    DontMove,
    Seek,
    Flee,
    Pursuit, 
    Evade,
    Arrive,
}

// NUEVOS ESTADOS PRINCIPALES DEL JEFE (Nivel Superior de la FSM)
public enum EBossState : byte
{
    IdleMove,  // Buscar al jugador (NavMesh: SetDestination)
    Melee,     // Rango de contacto o golpe (NavMesh: Cerca)
    Ranged,    // Rango de distancia (NavMesh: Detenerse o Huir un poco)
    Ultimate,  // Ataque de alta prioridad (HP bajo)
}

// NUEVOS SUBESTADOS DE ATAQUE (Para el Ciclo Selector)
public enum EBossAttackType : byte
{
    BasicAttack = 0,     // Ataque Básico (Tiro Único o Golpe simple)
    SpecialAttack1 = 1,  // Ataque Especial 1 (Área o Tiro Triple)
    SpecialAttack2 = 2,  // Ataque Especial 2 (Dash o Ráfaga Circular)
}


// sirve como bits para una máscara de bits.
public enum ELayer
{
    Default = 1, // [0000 0001] 0 en binario
    Enemy = 2, // [0000 0010] 2 en binario
    Player = 4, // [0000 0100] 4 en binario
    EnemyBullet = 8, // [0000 1000] 8 en binario
    PlayerBullet = 16, // [0001 0000] 16 en binario
    Wall = 32,
}

// los enum tiene un tipo de dato subyacente.
// si tu enumeración no va a llegar a valores muy altos, usa el tipo de dato de tamaño suficiente para 
// contener todas las cosas que vas a enumerar.
public enum EInt : int
{
    // cada variable del tipo "EInt" pesa lo mismo que un entero (4 bytes).
}

public enum EShort : short
{
    // cada variable del tipo "EShort" pesa lo mismo que un short (2 bytes).
}

public enum EByte : byte
{
    // cada variable del tipo "EByte" pesa lo mismo que un byte (1 byte).
}

// ejemplos rápidos:
// En minecraft hay muchísimos tipos de cosas que puedes llevar en tu inventario, probablemente más de 256 tipos,
// entonces tú no usarías una enumeración de tipo byte, porque solo llega hasta 256, tendrías que usar una de short,
// que llega hasta 2^16


public class Enums
{
    
}
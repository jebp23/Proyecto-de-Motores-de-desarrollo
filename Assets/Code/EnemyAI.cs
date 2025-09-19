using UnityEngine;
using UnityEngine.AI; // Necesario para usar NavMeshAgent

public class EnemyAI : MonoBehaviour
{
    public Transform target; // El jugador que el enemigo perseguirá
    public float detectionRange = 10f; // Rango de detección del enemigo
    public float stoppingDistance = 2f; // Distancia a la que el enemigo se detiene del jugador
    public float rotationSpeed = 5f; // Velocidad de rotación del enemigo
    public LayerMask obstaclesLayer; // Capa de los obstáculos (ej. paredes)

    // Opcional: Para un campo de visión angular
    [Range(0, 360)]
    public float fieldOfViewAngle = 120f; // Ángulo de visión del enemigo

    private NavMeshAgent agent;
    private bool playerInSight; // Para saber si el jugador está a la vista

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent no encontrado en el objeto del enemigo.");
            enabled = false;
            return;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("Target (Jugador) no asignado y no se encontró un objeto con la etiqueta 'Player'. El enemigo no perseguirá.");
                enabled = false;
            }
        }
    }

    void Update()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        playerInSight = false; // Asumimos que no está a la vista al inicio de cada frame

        // 1. Comprobar si el jugador está dentro del rango de detección
        if (distanceToTarget <= detectionRange)
        {
            // Opcional: Comprobar si el jugador está dentro del campo de visión angular
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < fieldOfViewAngle / 2) // Dividimos por 2 porque el ángulo se mide desde el frente
            {
                // 2. Usar un Raycast para ver si hay obstáculos
                RaycastHit hit;
                // El Raycast se lanza desde la posición del enemigo hacia el jugador.
                // Asegúrate de que el origen del raycast no esté dentro de un collider propio del enemigo.
                // Puedes ajustar el origen ligeramente hacia arriba o al frente si es necesario.
                if (Physics.Raycast(transform.position, directionToTarget, out hit, detectionRange, obstaclesLayer))
                {
                    // Debug.DrawRay para visualizar el rayo en el editor
                    Debug.DrawRay(transform.position, directionToTarget * detectionRange, Color.red);

                    if (hit.collider.transform == target)
                    {
                        // ¡El Raycast golpeó al jugador! No hay obstáculos entre el enemigo y el jugador.
                        playerInSight = true;
                    }
                    else
                    {
                        // El Raycast golpeó otra cosa (un obstáculo)
                        Debug.Log("Obstáculo bloqueando la visión: " + hit.collider.name);
                    }
                }
                else
                {
                    // El Raycast no golpeó nada dentro del rango de detección,
                    // lo que significa que no hay obstáculos entre el enemigo y el jugador.
                    // Esto asume que el jugador está en una capa que no está en 'obstaclesLayer'.
                    // Si el jugador está en la capa de obstáculos, el hit.collider.transform == target lo manejaría.
                    Debug.DrawRay(transform.position, directionToTarget * detectionRange, Color.green);
                    playerInSight = true; // No hay obstáculos hasta el rango máximo
                }
            }
        }

        // 3. Actuar basándose en si el jugador está a la vista
        if (playerInSight)
        {
            // Si el jugador está a la vista y fuera de la distancia de parada, persíguelo
            if (distanceToTarget > stoppingDistance)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                // Si está dentro de la distancia de parada, detente
                agent.SetDestination(transform.position);
                RotateTowardsTarget();
            }
        }
        else
        {
            // Si el jugador no está a la vista, detente
            agent.SetDestination(transform.position);
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        // Dibujar el campo de visión angular
        if (fieldOfViewAngle < 360)
        {
            Vector3 forwardLimit = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;
            Gizmos.DrawRay(transform.position, forwardLimit);
            forwardLimit = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;
            Gizmos.DrawRay(transform.position, forwardLimit);
        }
    }
}
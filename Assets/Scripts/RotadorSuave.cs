using UnityEngine;

namespace ZenGardenVR
{
    /// <summary>
    /// Componente sencillo para rotar suavemente un objeto en cualquier eje (por defecto Y).
    /// Ideal para elementos decorativos como cristales, orbes, lotos o campanas en RV.
    /// </summary>
    [AddComponentMenu("Zen Garden VR/Rotador Suave")]
    public class RotadorSuave : MonoBehaviour
    {
        [Header("Configuración de Rotación")]
        [Tooltip("Velocidad de giro en grados por segundo.")]
        [SerializeField] private float velocidadRotacion = 30f;

        [Tooltip("Eje de rotación (por defecto el eje vertical Y).")]
        [SerializeField] private Vector3 ejeRotacion = Vector3.up;

        [Tooltip("Si es verdadero, rota en espacio local; si es falso, en espacio global.")]
        [SerializeField] private bool usarEspacioLocal = true;

        private void Update()
        {
            float angulo = velocidadRotacion * Time.deltaTime;
            transform.Rotate(ejeRotacion.normalized * angulo, usarEspacioLocal ? Space.Self : Space.World);
        }
    }
}


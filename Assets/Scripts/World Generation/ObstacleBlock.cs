using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Zenject;

public class ObstacleBlock : MonoBehaviour
{
    [SerializeField] private float length;
    [Header("Ground Marker:")]
    [SerializeField] private Color oddRowColor = Color.white;
    [SerializeField] private Color evenRowColor = Color.black;
    [SerializeField] private bool drawGround = true;
    [SerializeField] private List<Obstacle> obstacles;

    public float Length => length;
    public IEnumerable<Obstacle> Obstacles => obstacles.AsReadOnly();
    
#if UNITY_EDITOR

    private void OnDrawGizmos() 
    {
        if (drawGround)
            DrawGround();
    }

    private void DrawGround()
    {
        for (int i = 0; i < GameManager.Instance.RowCount; i++)
        {
            Color rowColor = i % 2 == 0 ? evenRowColor : oddRowColor;
            float positionX = Utils.RowToXPosition(i);

            Gizmos.color = rowColor;
            Gizmos.DrawCube(Vector3.right * positionX, 
                new Vector3(GameManager.Instance.RowWidth, 0.1f, length));
        }
    }

#endif
}


using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace CasualGames.Operius2D.Configs
{
    [InlineEditor]
    [CreateAssetMenu(menuName = "Operius2D/EnemyFormationMatrix", fileName = "EnemyFormationMatrix.asset")]
    public class EnemyWaveMatrix : SerializedScriptableObject
    {
        [OnValueChanged("RefreshMatrix")]
        [MaxValue(16)]
        [MinValue(1)]
        public int Row = 1;

        [OnValueChanged("RefreshMatrix")]
        [MaxValue(16)]
        [MinValue(1)]
        public int Column = 1;

        public Vector2 Spacing;


        [ShowInInspector]
        [TableMatrix(
            HorizontalTitle = "Enemy Formation Matrix",
            DrawElementMethod = nameof(DrawCell),
            SquareCells = true,
            RowHeight = 40,
            ResizableColumns = false,
            Transpose = true
        )]
        public bool[,] FormationMatrix;

        // Inspector Code

        # region Inspector Functions

        // [Button(ButtonSizes.Medium, Name = "Refresh")]
        public void RefreshMatrix()
        {
            bool[,] temp = FormationMatrix;
            int minRows = Math.Min(Row, temp.GetLength(0));
            int minCols = Math.Min(Column, temp.GetLength(1));
            FormationMatrix = new bool[Row, Column];
            for (int i = 0; i < minRows; i++)
            {
                for (int j = 0; j < minCols; j++)
                {
                    FormationMatrix[i, j] = temp[i, j];
                }
            }
        }

        [OnInspectorInit]
        public void CreateData()
        {
            if (FormationMatrix == null)
            {
                FormationMatrix = new bool[Row, Column];
            }
        }

        private bool DrawCell(Rect rect, bool value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }
#if UNITY_EDITOR
            UnityEditor.EditorGUI.DrawRect(rect.Padding(2), value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0, 0, 0, 0.5f));
#endif
            return value;
        }

        #endregion


        // Other Utility Function Code
        public int GetEnemyCount()
        {
            int count = 0;
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Column; j++)
                {
                    if (FormationMatrix[i, j])
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
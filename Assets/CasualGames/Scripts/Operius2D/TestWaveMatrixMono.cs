using System;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace CasualGames.Operius2D
{
    public class TestWaveMatrixMono : SerializedMonoBehaviour
    {
        private bool _init = true;

        private bool[,] _cacheFormation;

        [OnValueChanged("RefreshMatrix")]
        [MaxValue(30)]
        [MinValue(1)]
        public int Row = 1;


        [OnValueChanged("RefreshMatrix")]
        [MaxValue(30)]
        [MinValue(1)]
        public int Column = 1;

        [Button(ButtonSizes.Medium, Name = "Refresh")]
        public void RefreshMatrix()
        {
            // bool[,] temp = FormationMatrix;
            // int minRows = Math.Min(Row, temp.GetLength(0));
            // int minCols = Math.Min(Column, temp.GetLength(1));
            // FormationMatrix = new bool[Row, Column];
            // for (int i = 0; i < minRows; i++)
            // {
            //     for (int j = 0; j < minCols; j++)
            //     {
            //         FormationMatrix[i, j] = temp[i, j];
            //     }
            // }
            FormationMatrix = new EnumMatrix[4, 2];
            // CreateData(); ////
        }

        [ShowInInspector]
        [TableMatrix(
            HorizontalTitle = "Enemy Formation Celled Matrix",
            DrawElementMethod = "DrawCell",
            SquareCells = false,
            RowHeight = 40,
            ResizableColumns = false
        )]
        public EnumMatrix[,] FormationMatrix;


        [OnInspectorInit]
        public void CreateData()
        {
            Debug.Log(FormationMatrix);
            if (FormationMatrix == null)
            {
                FormationMatrix = new EnumMatrix[4, 2];
            }
        }

        private EnumMatrix DrawCell(Rect rect, EnumMatrix value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                // value = !value;
                value += 1;
                if (value > (EnumMatrix)Enum.GetValues(typeof(EnumMatrix)).Cast<int>().Max())
                {
                    value = (EnumMatrix)(-1);
                }

                GUI.changed = true;
                Event.current.Use(); // asdasdadasdasd
            }

            Color32 color = Color.black;
            switch (value)
            {
                case EnumMatrix.NONE:
                    color = Color.black;
                    break;
                case EnumMatrix.ONE:
                    color = Color.red;
                    break;
                case EnumMatrix.TWO:
                    color = Color.green;
                    break;
                case EnumMatrix.THREE:
                    color = Color.magenta;
                    break;
            }
#if UNITY_EDITOR
            UnityEditor.EditorGUI.DrawRect(rect.Padding(2), color);
#endif
            return value;
        }

        public enum EnumMatrix
        {
            NONE = -1,
            ONE,
            TWO,
            THREE,
        }
    }
}
﻿using UnityEngine;
using Zenject;

namespace PKFramework.Examples
{
    public class ExampleScene1: MonoBehaviour
    {
        [Inject] private ExampleScene1Logic _logic;

        private void Awake()
        {
            _logic.Start();
        }
    }
}
using System;
using UnityEngine;

namespace CasualGames.Operius2D
{
    public class BackgroundScroll : MonoBehaviour
    {
        [SerializeField] private float _x;
        [SerializeField] private float _y;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            _spriteRenderer.material.mainTextureOffset += new Vector2(_x, _y) * Time.deltaTime;
        }
    }
}
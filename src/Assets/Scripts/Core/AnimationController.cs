using System;
using UnityEngine;

namespace Core
{
    public class AnimationController
    {
        private const float DELTA_TIME_MAX = 1.0f;
        
        private int _time = 0;

        private float _inv_time_max = 1.0f;

        public void Set(int maxTime)
        {
            Debug.Assert(0.0f < maxTime);

            _time = maxTime;
            _inv_time_max = 1.0f / (float)maxTime;
        }

        public bool Update()
        {
            _time = Math.Max(--_time, 0);

            return (0 < _time);
        }

        public float GetNormalized()
        {
            return (float)_time * _inv_time_max;
        }
    }
}


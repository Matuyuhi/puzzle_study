using UnityEngine;

namespace Core
{
    public class AnimationController
    {
        private const float DELTA_TIME_MAX = 1.0f;
        
        private float _time = 0.0f;

        private float _inv_time_max = 1.0f;

        public void Set(float maxTime)
        {
            Debug.Assert(0.0f < maxTime);

            _time = maxTime;
            _inv_time_max = 1.0f / maxTime;
        }

        public bool Update(float deltaTime)
        {
            if (DELTA_TIME_MAX < deltaTime) deltaTime = DELTA_TIME_MAX;
            _time -= deltaTime;

            if (_time <= 0.0f)
            {
                _time = 0.0f;
                return false;
            }

            return true;
        }

        public float GetNormalized()
        {
            return _time * _inv_time_max;
        }
    }
}


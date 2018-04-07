
using UnityEngine;

namespace Game
{
    public class CameraController : MonoBehaviour
    {
        private Camera mCamera;
        public float speed = 5.0f;
        public float zoomSpeed = 2.0f;
        public float initialZoom = 8.0f;
        public float minZoom = 5.0f;
        public float maxZoom = 15.0f;

        void Awake()
        {
            mCamera = GetComponent<Camera>();
            mCamera.orthographicSize = initialZoom;
        }

        void Update()
        {
            Vector3 pos = transform.localPosition;
            if (Input.GetKey(KeyCode.LeftArrow))
                pos.x -= Time.deltaTime * speed;
            if (Input.GetKey(KeyCode.RightArrow))
                pos.x += Time.deltaTime * speed;
            if (Input.GetKey(KeyCode.UpArrow))
                pos.y += Time.deltaTime * speed;
            if (Input.GetKey(KeyCode.DownArrow))
                pos.y -= Time.deltaTime * speed;
            transform.localPosition = pos;

            float size = mCamera.orthographicSize;
            if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
                size = Mathf.Max(size - Time.deltaTime * zoomSpeed, minZoom);
            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
                size = Mathf.Min(size + Time.deltaTime * zoomSpeed, maxZoom);
            mCamera.orthographicSize = size;
        }
    }
}

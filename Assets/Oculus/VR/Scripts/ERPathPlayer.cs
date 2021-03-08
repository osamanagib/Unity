using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Assertions;
using EasyRoads3Dv3;
using UnityEditor;
using System.IO;
using System;


namespace ERVertexPath
{
    public class ERPathPlayer : MonoBehaviour
    {
        public static float standardSpeed = 10.0f;
        public static float fastSpeed = 25.0f;
        public List<GameObject> modularRoad;
        //public float speedMs = 30f;
        private ERPathAdapter pathAdapter;
        private Rigidbody cameraToFollow;
        public float cameraPosition;
       // private Text SpeedFild;
        //protected OVRPlayerController CameraRig = null;
        public float speed = 30.0f;
        public int index = 0;
        public float count;
        //public Texture2D textureToDisplay;
        private void Start()
        {
       
            cameraToFollow = GetComponent<Rigidbody>();
            Assert.IsNotNull(cameraToFollow, "Cant find Camera component for ERPathCamera");
            pathAdapter = modularRoad[index].GetComponent<ERPathAdapter>();
            Assert.IsNotNull(pathAdapter, $"Cant find ERPathAdapter for road {modularRoad[0].name}");
        }
        
        private void FixedUpdate()
        {

            count = pathAdapter.TotalDistance;

            var position = pathAdapter.GetPointAtDistance(cameraPosition);
            var lookAt = pathAdapter.GetRotationAtDistance(cameraPosition);

            cameraToFollow.transform.position = position + Vector3.up * 62.0f;
            cameraToFollow.transform.rotation = lookAt;

            
            cameraPosition += Time.deltaTime * speed;
          
            if (cameraPosition > pathAdapter.TotalDistance && index < modularRoad.Count - 1)
            {
                
                    index++;
                    pathAdapter = modularRoad[index].GetComponent<ERPathAdapter>();
                cameraPosition = 0;
            }
        }
    }
}

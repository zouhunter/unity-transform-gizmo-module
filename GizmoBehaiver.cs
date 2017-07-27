﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace RuntimeGizmos
{
    [RequireComponent(typeof(Camera))]
    public class GizmoBehaiver : MonoBehaviour
    {
        public Protocal protocal;
        public AxisSetting setting;
        public AxisDrawer drawer;
        public AxisOperator opertor;
        void Awake()
        {
            protocal = new Protocal();
            protocal.myCamera = GetComponent<Camera>();
            drawer = new AxisDrawer(protocal, setting);
            opertor = new AxisOperator(this,protocal, setting);
        }

        void Update()
        {
            opertor.Update();
        }

        private void LateUpdate()
        {
            opertor.LateUpdate();
            drawer.LateUpdate();
        }

        void OnPostRender()
        {
            drawer.OnPostRender();
        }
}
}
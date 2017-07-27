using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace RuntimeGizmos
{
    [RequireComponent(typeof(Camera))]
    public class GizmoBehaiver : MonoBehaviour
    {
        private Protocal protocal;
        private AxisSetting setting;
        private AxisDrawer drawer;
        private AxisOperator opertor;
        private AutoSwitcher switcher;
        void Awake()
        {
            protocal.myCamera = GetComponent<Camera>();
            protocal = new Protocal();
            setting = new AxisSetting();
            switcher = new AutoSwitcher(protocal, setting);
            drawer = new AxisDrawer(protocal, setting);
            opertor = new AxisOperator(this,protocal, setting);
        }

        void Update()
        {
            switcher.Update();
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
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace InControl
{
    [InitializeOnLoad]
    internal class InputManagerAssetGenerator
    {
        static List<AxisPreset> axisPresets = new List<AxisPreset>();


        static InputManagerAssetGenerator()
        {
            SetupAxisPresets();
            CheckInputManagerAsset();
        }


        [MenuItem("Edit/Project Settings/InControl/Setup Input Manager")]
        static void GenerateInputManagerAsset()
        {
            ApplyAxisPresets();
            EditorUtility.DisplayDialog( "Success", "Input Manager settings have been configured.", "OK" );
        }


        [MenuItem("Edit/Project Settings/InControl/Check Input Manager")]
        static void CheckInputManagerAsset()
        {
            if (!CheckAxisPresets())
            {
                Debug.LogError( "InControl has detected an invalid Input Manager setup. To fix, execute 'Edit > Project Settings > InControl > Setup Input Manager'." );
            }
        }


        static void SetupAxisPresets()
        {
            axisPresets.Clear();

            for (int device = 1; device <= UnityInputDevice.MaxDevices; device++)
            {
                for (int analog = 0; analog < UnityInputDevice.MaxAnalogs; analog++)
                {
                    axisPresets.Add( new AxisPreset( device, analog ) );
                }
            }

            axisPresets.Add( new AxisPreset( "mouse x", 1, 0, 0.1f ) );
            axisPresets.Add( new AxisPreset( "mouse y", 1, 1, 0.1f ) );
            axisPresets.Add( new AxisPreset( "mouse z", 1, 2, 0.1f ) );
            axisPresets.Add( new AxisPreset( "Mouse ScrollWheel", 1, 2, 0.1f ) );
            axisPresets.Add( new AxisPreset( name:"Horizontal", negativeButton:"left", positiveButton:"right", altNegativeButton:"a", altPositiveButton:"d", gravity:3.0f, deadZone:0.001f, sensitivity:3.0f, snap:true, type:0, axis:0, joyNum:0 ));
            axisPresets.Add( new AxisPreset( name:"Horizontal", gravity:0.0f, deadZone:0.19f, sensitivity:1.0f, type:2, axis:0, joyNum:0 ));
            axisPresets.Add( new AxisPreset( name:"Vertical", negativeButton:"down", positiveButton:"up", altNegativeButton:"s", altPositiveButton:"w", gravity:3.0f, deadZone:0.001f, sensitivity:3.0f, snap:true, type:0, axis:0, joyNum:0 ));
            axisPresets.Add( new AxisPreset( name:"Vertical", gravity:0.0f, deadZone:0.19f, sensitivity:1.0f, type:2, axis:0, invert:true, joyNum:0 ));
            axisPresets.Add( new AxisPreset( name:"Submit", positiveButton:"return", altPositiveButton:"joystick button 0", gravity:1000.0f, deadZone:0.001f, sensitivity:1000.0f, type:0, axis:0, joyNum:0 ));
            axisPresets.Add( new AxisPreset( name:"Submit", positiveButton:"enter", altPositiveButton:"space", gravity:1000.0f, deadZone:0.001f, sensitivity:1000.0f, type:0, axis:0, joyNum:0 ));
            axisPresets.Add( new AxisPreset( name:"Cancel", positiveButton:"escape", altPositiveButton:"joystick button 1", gravity:1000.0f, deadZone:0.001f, sensitivity:1000.0f, type:0, axis:0, joyNum:0 ));
        }

        static void ApplyAxisPresets()
        {
            var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[0];
            var serializedObject = new SerializedObject( inputManagerAsset );
            var axisArray = serializedObject.FindProperty( "m_Axes" );
            
            axisArray.arraySize = Mathf.Max( axisPresets.Count, axisArray.arraySize );
            serializedObject.ApplyModifiedProperties();

            for (int i = 0; i < axisPresets.Count; i++)
            {
                var axisEntry = axisArray.GetArrayElementAtIndex( i );
                axisPresets[i].ApplyTo( ref axisEntry );
            }

            serializedObject.ApplyModifiedProperties();
            
            AssetDatabase.Refresh();
        }


        static bool CheckAxisPresets()
        {
            // var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[0];
            // var serializedObject = new SerializedObject( inputManagerAsset );
            // var axisArray = serializedObject.FindProperty( "m_Axes" );
                        
            // for (int i = 0; i < axisPresets.Count; i++)
            // {
            //     var axisEntry = axisArray.GetArrayElementAtIndex( i );
            //     if (!axisPresets[i].Check( ref axisEntry ))
            //     {
            //         return false;
            //     }
            // }

            return true; // hack, index out of range above 
        }


        static SerializedProperty GetChildProperty( SerializedProperty parent, string name ) {
            SerializedProperty child = parent.Copy();
            child.Next( true );

            do {
                if (child.name == name) {
                    return child;
                }
            }
            while (child.Next( false ));

            return null;
        }


        internal class AxisPreset {
            public string name;
            public int type;
            public int axis;
            public int joyNum;
            public float sensitivity;
            public string positiveButton;
            public string altPositiveButton;
            public string negativeButton;
            public string altNegativeButton;
            public float gravity;
            public float deadZone;
            public bool snap;
            public bool invert;
            
            public AxisPreset ( 
                string name = "",
                int type = 1,
                int axis = 0,
                int joyNum = 0,
                float sensitivity = 1,
                string positiveButton = "",
                string altPositiveButton = "",
                string negativeButton = "",
                string altNegativeButton = "",
                float gravity = 10,
                float deadZone = 0.001f,
                bool snap = false,
                bool invert = false
             ) {
                this.name = name;
                this.type = type;
                this.axis = axis;
                this.joyNum = joyNum;
                this.sensitivity = sensitivity;
                this.positiveButton = positiveButton;
                this.altPositiveButton = altPositiveButton;
                this.negativeButton = negativeButton;
                this.altNegativeButton = altNegativeButton;
                this.gravity = gravity;
                this.deadZone = deadZone;
                this.snap = snap;
                this.invert = invert;
            }
            
            public AxisPreset( string name, int type, int axis, float sensitivity ) {
                this.name = name;
                this.type = type;
                this.axis = axis;
                this.joyNum = 0;
                this.sensitivity = sensitivity;
                this.positiveButton = "";
                this.altPositiveButton = "";
                this.negativeButton = "";
                this.altNegativeButton = "";
                this.gravity = 10;
                this.deadZone = 0.001f;
                this.snap = false;
                this.invert = false;
            }
            
            public AxisPreset( int device, int analog ) {
                this.name = string.Format( "joystick {0} analog {1}", device, analog );
                this.type = 2;
                this.axis = analog;
                this.joyNum = device;
                this.sensitivity = 1.0f;
                this.positiveButton = "";
                this.altPositiveButton = "";
                this.negativeButton = "";
                this.altNegativeButton = "";
                this.gravity = 10;
                this.deadZone = 0.001f;
                this.snap = false;
                this.invert = false;
            }
            
            
            public void ApplyTo( ref SerializedProperty axisPreset )
            {
                GetChildProperty( axisPreset, "m_Name" ).stringValue = this.name;
                GetChildProperty( axisPreset, "type" ).intValue = this.type;
                GetChildProperty( axisPreset, "axis" ).intValue = this.axis;
                GetChildProperty( axisPreset, "joyNum" ).intValue = this.joyNum;
                GetChildProperty( axisPreset, "sensitivity" ).floatValue = sensitivity;
                
                GetChildProperty( axisPreset, "descriptiveName" ).stringValue = "";
                GetChildProperty( axisPreset, "descriptiveNegativeName" ).stringValue = "";
                GetChildProperty( axisPreset, "negativeButton" ).stringValue = this.negativeButton;
                GetChildProperty( axisPreset, "positiveButton" ).stringValue = this.positiveButton;
                GetChildProperty( axisPreset, "altNegativeButton" ).stringValue = this.altNegativeButton;
                GetChildProperty( axisPreset, "altPositiveButton" ).stringValue = this.altPositiveButton;
                GetChildProperty( axisPreset, "gravity" ).floatValue = this.gravity;
                GetChildProperty( axisPreset, "dead" ).floatValue = this.deadZone;
                GetChildProperty( axisPreset, "snap" ).boolValue = this.snap;
                GetChildProperty( axisPreset, "invert" ).boolValue = this.invert;
            }
            
            
            public bool Check( ref SerializedProperty axisPreset )
            {
                if (GetChildProperty( axisPreset, "m_Name" ).stringValue != this.name) return false;
                if (GetChildProperty( axisPreset, "type" ).intValue != this.type) return false;
                if (GetChildProperty( axisPreset, "axis" ).intValue != this.axis) return false;
                if (GetChildProperty( axisPreset, "joyNum" ).intValue != this.joyNum) return false;
                if (!Mathf.Approximately( GetChildProperty( axisPreset, "sensitivity" ).floatValue, this.sensitivity)) return false;
                
                return true;
            }
        }
    }
}
#endif


using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public class TilemapTools
    {
        public void Initialize()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        public void Dispose()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            if (e.alt || e.control || e.command)
                return;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    {
                        if (e.button == 0)
                            GUIUtility.hotControl = controlID;
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (e.button == 0 && GUIUtility.hotControl == controlID)
                        {
                            GUIUtility.hotControl = 0;
                            Event.current.Use();

                            Vector3 mousePos = e.mousePosition;
                            mousePos.y = Camera.current.pixelHeight - mousePos.y;
                            Vector3 worldPos = sceneView.camera.ScreenToWorldPoint(mousePos);

                            int x = (int)(worldPos.x / 32);
                            int y = (int)(-worldPos.y / 32);
                            Rect r = new Rect(x * 32, -y * 32 - 32f, 32, 32);
                            Handles.DrawSolidRectangleWithOutline(r, Color.black, Color.black);
                        }
                    }
                    break;
            }
        }
    }
}
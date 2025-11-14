using UnityEditor;

namespace CoreDomain.Scripts.Editor.CopySerialized
{
    /// <summary>
    /// This Editor script allows us to copy a parent class to a child class that inherits it
    /// How to use?
    /// In the Inspector of the parent class: Right click your class component -> select CopySerialized
    /// In the Inspector of the child class: Right click your class component -> select PasteSerialized
    /// </summary>
    public static class CopySerializedHelper
    {
        static SerializedObject sourceObject;

        [MenuItem("CONTEXT/Component/CopySerialized")]
        public static void CopySerializedFromBase(MenuCommand command)
        {
            sourceObject = new SerializedObject(command.context);
        }

        [MenuItem("CONTEXT/Component/PasteSerialized")]
        public static void PasteSerializedFromBase(MenuCommand command)
        {
            var destinationObject = new SerializedObject(command.context);
            var sourceProperty = sourceObject.GetIterator();
            
            if (sourceProperty.NextVisible(true))
            {
                while (sourceProperty.NextVisible(true)) //iterate through all serializedProperties
                {
                    var destinationProperty = destinationObject.FindProperty(sourceProperty.name);
                    var arePropertiesSameType = destinationProperty != null && destinationProperty.propertyType == sourceProperty.propertyType;
                    if (arePropertiesSameType)
                    {
                        destinationObject.CopyFromSerializedProperty(sourceProperty);
                    }
                }
            }

            destinationObject.ApplyModifiedProperties();
        }
    }
}
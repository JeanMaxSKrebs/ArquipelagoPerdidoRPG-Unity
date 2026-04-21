#if UNITY_EDITOR
using UnityEditor;

namespace ArquipelagoPerdidoRPG.Editor
{
    public static class OpenWorldBuilderMenu
    {
        [MenuItem("Tools/Arquipelago/OpenWorld/Build Open World")]
        public static void BuildOpenWorldFromSeparatedMenu()
        {
            SceneAutoBuilder.BuildOpenWorld();
        }
    }
}
#endif

/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using UnityEngine;

namespace InfinityCode.HugeTexture.Components
{
    public class HugeTextureBridge : MonoBehaviour
    {
        public Texture2DArray texture;
        public int rows = 1;
        public int cols = 1;
        public Component component;
        public string memberName;

        private void Awake()
        {
            if (component == null || texture == null || string.IsNullOrEmpty(memberName)) return;
            if (rows * cols != texture.depth)
            {
                Debug.LogError("rows * cols != textureArray.depth. Check parameter values.");
                return;
            }

            MemberInfo[] members = component.GetType().GetMember(memberName, MemberTypes.Field | MemberTypes.Property, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (members.Length == 0)
            {
                Debug.LogError("Cannot find a member!");
                return;
            }

            HugeTexture2D hugeTexture = new HugeTexture2D(texture, rows, cols);

            MemberInfo firstMember = members[0];
            if (firstMember.MemberType == MemberTypes.Field) (firstMember as FieldInfo).SetValue(component, hugeTexture.GetTexture2D());
            else if (firstMember.MemberType == MemberTypes.Property) (firstMember as PropertyInfo).SetValue(component, hugeTexture.GetTexture2D());
        }
    }
}
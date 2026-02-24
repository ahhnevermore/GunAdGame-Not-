// using UnityEngine;
// public class ArmyManager : MonoBehaviour
// {
//     public int armyStrength = 1;
//     public int maxVisibleAvatars = 10;



//     public AvatarController[] avatars;  // 10 max

//     void UpdateVisuals()
//     {
//         int visible = Mathf.Min(armyStrength, maxVisibleAvatars);
//         int powerPerAvatar = armyStrength / visible;

//         for (int i = 0; i < avatars.Length; i++)
//         {
//             avatars[i].gameObject.SetActive(i < visible);
//             if (i < visible)
//                 avatars[i].SetPower(powerPerAvatar);
//         }
//     }
// }
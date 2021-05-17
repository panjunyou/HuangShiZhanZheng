    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Bind Some Delegate Func For Yours.
    /// </summary>
    public class UIBind : MonoBehaviour
    {
        static bool isBind = false;

        public static void Bind()
        {
            if (!isBind)
            {
                isBind = true;
                //Debug.LogWarning("Bind For UI Framework.");

                //bind for your loader api to load UI.
                UIPage.delegateSyncLoadUI = Resources.Load<GameObject>; // 异步方式可以用Resources.LoadAsync
                //TTUIPage.delegateAsyncLoadUI = UILoader.Load;

            }
        }
    }

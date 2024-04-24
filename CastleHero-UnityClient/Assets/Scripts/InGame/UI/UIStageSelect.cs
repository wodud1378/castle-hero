using RGLabs.Common.UI;
using RGLabs.InGame.Behaviours.Player;
using UnityEngine;

namespace RGLabs.InGame.UI
{
    public class UIStage : MonoBehaviour
    {
        [SerializeField] private UIUser _user;
        [SerializeField] private UIWealth _dia;
        [SerializeField] private UIWealth _gold;

        [SerializeField] private Camp _camp;

        #region Button Events.

        public void OnClickEvent()
        {
            
        }

        public void OnClickPackage()
        {
            
        }

        public void OnClickSetting()
        {
            
        }

        public void OnClickShop()
        {
            
        }

        public void OnClickMail()
        {
            
        }
        

        #endregion
        
        #region Animation Events.

        public void OnDisappeared()
        {
            
        }

        #endregion
    }
}
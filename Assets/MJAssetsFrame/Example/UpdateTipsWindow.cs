using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MJ.AssetFrameWork.ABFrame
{
    public class UpdateTipsWindow : MonoBehaviour
    {
        //内容文本
        public Text contentText;
        //更新回调
        public Action OnUpdateCallbakc;
        //退出回调
        public Action OnQuitCallBack;

        public void InitView(string content, Action updateCallBack, Action quitCallBakc)
        {
            contentText.text = content;
            OnUpdateCallbakc = updateCallBack;
            OnQuitCallBack = quitCallBakc;
        }

        public void OnUpdateButtonClick()
        {
            OnUpdateCallbakc?.Invoke();
            Destroy(gameObject);
        }

        public void OnQuitButtonClick()
        {
            OnQuitCallBack?.Invoke();
            Destroy(gameObject);
        }
    }
}


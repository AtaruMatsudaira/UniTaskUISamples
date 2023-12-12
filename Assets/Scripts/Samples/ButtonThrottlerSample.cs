using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Samples
{
    public class ButtonThrottlerSample : MonoBehaviour
    {
        [SerializeField] private Button buttonA;
        [SerializeField] private Button buttonB;

        [SerializeField] private TMP_Text timerLabelA;
        [SerializeField] private TMP_Text timerLabelB;

        private void Start()
        {
            buttonA.onClick.AddListener(() => TimeRunAsync(timerLabelA).Forget());

            buttonB.OnClickAsAsyncEnumerable().SubscribeAwait(async _ => await TimeRunAsync(timerLabelB));
        }

        private async UniTask TimeRunAsync(TMP_Text timerLabel)
        {
            DateTime endTime = DateTime.Now.AddSeconds(5);

            while (DateTime.Now < endTime)
            {
                timerLabel.SetText((endTime - DateTime.Now).TotalSeconds.ToString("F2"));
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            timerLabel.SetText("Finish");
        }
    }
}
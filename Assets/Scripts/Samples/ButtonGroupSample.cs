using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Samples
{
    public class ButtonGroupSample : MonoBehaviour
    {
        [SerializeField] private Button button1;
        [SerializeField] private Button button2;
        [SerializeField] private Button button3;

        [SerializeField] private TMP_Text label1;
        [SerializeField] private TMP_Text label2;
        [SerializeField] private TMP_Text label3;

        [SerializeField] private GameObject processingPanel;

        private ButtonGroup _buttonGroup;

        private void Start()
        {
            _buttonGroup = new ButtonGroup();

            _buttonGroup.AddButton(button1, Button1EventAsync);
            _buttonGroup.AddButton(button2, Button2EventAsync);
            _buttonGroup.AddButton(button3, async ct =>
            {
                label3.SetText("size change");
                for (int i = 0; i < 10; i++)
                {
                    label3.fontSize += 1;
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: ct);
                }

                label3.fontSize = 72;
                label3.SetText("Finish");
            });

            _buttonGroup.AddPreAsyncEvent(async _ => processingPanel.SetActive(true));
            _buttonGroup.AddPostAsyncEvent(async _ => processingPanel.SetActive(false));

            _buttonGroup.RunAsync(destroyCancellationToken).Forget();
        }

        private async UniTask Button1EventAsync(CancellationToken ct)
        {
            for (int i = 3; i > 0; i--)
            {
                label1.SetText($"Wait {i} seconds");
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);
            }

            label1.SetText("Finish");
        }

        private async UniTask Button2EventAsync(CancellationToken ct)
        {
            float time = 0;
            label2.SetText("Color Change");

            while (time < 3)
            {
                label2.color = Color.HSVToRGB(time / 3f, 1, 1);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                time += Time.deltaTime;
            }

            label2.color = Color.black;
            label2.SetText("Finish");
        }

        private void OnDestroy()
        {
            _buttonGroup.Dispose();
        }
    }
}
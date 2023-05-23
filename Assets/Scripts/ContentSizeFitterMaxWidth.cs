namespace Assets.Scripts
{
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteAlways]
    [RequireComponent(typeof(ContentSizeFitter))]
    public class ContentSizeFitterMaxWidth : MonoBehaviour
    {
        public float maxWidth;

        RectTransform _rtfm;
        ContentSizeFitter _fitter;
        ILayoutElement _layout;

        void OnEnable()
        {
            _rtfm = (RectTransform)transform;
            _fitter = GetComponent<ContentSizeFitter>();
            _layout = GetComponent<ILayoutElement>();
        }

        void Update()
        {
            _fitter.horizontalFit = _layout.preferredWidth > maxWidth
                ? ContentSizeFitter.FitMode.Unconstrained
                : ContentSizeFitter.FitMode.PreferredSize;

            if (_layout.preferredWidth > maxWidth)
            {
                _fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                _rtfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
            }
            else
                _fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        void OnValidate() => OnEnable();
    }
}
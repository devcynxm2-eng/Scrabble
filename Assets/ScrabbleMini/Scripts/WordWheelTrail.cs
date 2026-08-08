using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScrabbleMini
{
    public class WordWheelTrail : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform trailContainer;
        [SerializeField] private Image trailSegmentPrefab;

        [Header("Trail Appearance")]
        [SerializeField]
        private Color trailColor =
            new Color(1f, 0.65f, 0.1f, 0.9f);

        [Min(1f)]
        [SerializeField] private float trailWidth = 24f;

        [Header("Live Pointer Trail")]
        [SerializeField] private bool showPointerTrail = true;

        [Min(0f)]
        [SerializeField] private float minimumPointerDistance = 8f;

        private readonly List<Image> fixedSegments = new();

        private Image pointerSegment;

        private Camera canvasCamera;

        public float TrailWidth => trailWidth;
        public Color TrailColor => trailColor;

        private void Awake()
        {
            FindCanvasCamera();
            CreatePointerSegment();
            ClearTrail();
        }

        private void FindCanvasCamera()
        {
            if (trailContainer == null)
                return;

            Canvas canvas =
                trailContainer.GetComponentInParent<Canvas>();

            if (canvas == null)
                return;

            canvasCamera = canvas.renderMode ==
                           RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;
        }

        private void CreatePointerSegment()
        {
            if (trailContainer == null ||
                trailSegmentPrefab == null)
            {
                return;
            }

            pointerSegment = Instantiate(
                trailSegmentPrefab,
                trailContainer
            );

            pointerSegment.name = "LivePointerTrail";
            pointerSegment.raycastTarget = false;
            pointerSegment.gameObject.SetActive(false);
        }

        public void RefreshFixedTrail(
            IReadOnlyList<LetterNode> selectedNodes)
        {
            ClearFixedSegments();

            if (selectedNodes == null ||
                selectedNodes.Count < 2)
            {
                return;
            }

            for (int index = 0;
                 index < selectedNodes.Count - 1;
                 index++)
            {
                LetterNode startNode = selectedNodes[index];
                LetterNode endNode = selectedNodes[index + 1];

                if (startNode == null || endNode == null)
                    continue;

                Vector2 startPosition =
                    GetNodeLocalPosition(startNode);

                Vector2 endPosition =
                    GetNodeLocalPosition(endNode);

                Image segment = CreateSegment(
                    $"Trail_{index}"
                );

                if (segment == null)
                    continue;

                SetSegment(
                    segment.rectTransform,
                    startPosition,
                    endPosition
                );

                fixedSegments.Add(segment);
            }
        }

        public void UpdatePointerTrail(
            LetterNode lastSelectedNode,
            Vector2 pointerScreenPosition)
        {
            if (!showPointerTrail ||
                lastSelectedNode == null ||
                pointerSegment == null ||
                trailContainer == null)
            {
                HidePointerTrail();
                return;
            }

            bool converted =
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    trailContainer,
                    pointerScreenPosition,
                    canvasCamera,
                    out Vector2 localPointerPosition
                );

            if (!converted)
            {
                HidePointerTrail();
                return;
            }

            Vector2 startPosition =
                GetNodeLocalPosition(lastSelectedNode);

            float distance = Vector2.Distance(
                startPosition,
                localPointerPosition
            );

            if (distance < minimumPointerDistance)
            {
                HidePointerTrail();
                return;
            }

            pointerSegment.gameObject.SetActive(true);

            SetSegment(
                pointerSegment.rectTransform,
                startPosition,
                localPointerPosition
            );
        }

        public void HidePointerTrail()
        {
            if (pointerSegment != null)
            {
                pointerSegment.gameObject.SetActive(false);
            }
        }

        public void ClearTrail()
        {
            ClearFixedSegments();
            HidePointerTrail();
        }

        private void ClearFixedSegments()
        {
            foreach (Image segment in fixedSegments)
            {
                if (segment != null)
                {
                    Destroy(segment.gameObject);
                }
            }

            fixedSegments.Clear();
        }

        private Image CreateSegment(string segmentName)
        {
            if (trailContainer == null ||
                trailSegmentPrefab == null)
            {
                return null;
            }

            Image segment = Instantiate(
                trailSegmentPrefab,
                trailContainer
            );

            segment.name = segmentName;
            segment.color = trailColor;
            segment.raycastTarget = false;
            segment.gameObject.SetActive(true);

            /*
             * Live pointer line ko last sibling rakha jayega.
             * Fixed segments uske neeche render honge.
             */
            if (pointerSegment != null)
            {
                segment.transform.SetSiblingIndex(
                    Mathf.Max(
                        0,
                        pointerSegment.transform.GetSiblingIndex()
                    )
                );
            }

            return segment;
        }

        private void SetSegment(
            RectTransform segment,
            Vector2 start,
            Vector2 end)
        {
            if (segment == null)
                return;

            Vector2 difference = end - start;
            float distance = difference.magnitude;

            Vector2 middlePoint =
                (start + end) * 0.5f;

            float angle =
                Mathf.Atan2(
                    difference.y,
                    difference.x
                ) * Mathf.Rad2Deg;

            segment.anchorMin =
                new Vector2(0.5f, 0.5f);

            segment.anchorMax =
                new Vector2(0.5f, 0.5f);

            segment.pivot =
                new Vector2(0.5f, 0.5f);

            segment.anchoredPosition = middlePoint;

            segment.sizeDelta =
                new Vector2(distance, trailWidth);

            segment.localRotation =
                Quaternion.Euler(0f, 0f, angle);

            segment.localScale = Vector3.one;

            Image image =
                segment.GetComponent<Image>();

            if (image != null)
            {
                image.color = trailColor;
                image.raycastTarget = false;
            }
        }

        private Vector2 GetNodeLocalPosition(
            LetterNode node)
        {
            if (node == null ||
                trailContainer == null)
            {
                return Vector2.zero;
            }

            Vector3 worldPosition =
                node.RectTransform.TransformPoint(
                    node.RectTransform.rect.center
                );

            Vector3 localPosition =
                trailContainer.InverseTransformPoint(
                    worldPosition
                );

            return new Vector2(
                localPosition.x,
                localPosition.y
            );
        }

        public void SetTrailColor(Color newColor)
        {
            trailColor = newColor;

            foreach (Image segment in fixedSegments)
            {
                if (segment != null)
                    segment.color = trailColor;
            }

            if (pointerSegment != null)
                pointerSegment.color = trailColor;
        }

        public void SetTrailWidth(float newWidth)
        {
            trailWidth = Mathf.Max(1f, newWidth);
        }
    }
}
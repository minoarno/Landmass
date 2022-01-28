using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Spline : MonoBehaviour
{
    [System.Serializable]
    public struct PointSpline
    {
        public Transform Position;
        public Transform NextSegmentP1;
        public Transform PreviousSegmentP3;
    }

    [SerializeField] Color _color;

    [Range(1,100)]
    [SerializeField] int _interpolation = 20;
    [SerializeField] SplineSegment.PolynomialForm _form = SplineSegment.PolynomialForm.Bernstein;

    //Multiple continuous spline segments
    [SerializeField] private List<PointSpline> _points = new List<PointSpline>();
    //public List<Vector3> _interpolatedPoints = new List<Vector3>();

    private void Start()
    {
        if (_points.Count == 0)
        {
            AddPosition(Vector3.zero);
            AddPosition(Vector3.zero);
        }
    }

    public void AddPosition(Vector3 pos)
    {
        int number = _points.Count;
        GameObject go = new GameObject("Point " + number);
        go.transform.parent = this.gameObject.transform;
        go.transform.position = pos;

        PointSpline point = new PointSpline();

        point.Position = go.transform;

        if (number > 0)
        {

            GameObject p1 = new GameObject("P1");
            p1.transform.parent =  _points[number - 1].Position.transform;
            PointSpline prevPoint = _points[number - 1];
            prevPoint.NextSegmentP1 = p1.transform;

            GameObject p3 = new GameObject("P3");
            p3.transform.parent = go.transform;

            point.PreviousSegmentP3 = p3.transform;

            _points[number - 1] = prevPoint;
        }
        _points.Add(point);
    }

    private void OnDrawGizmos()
    {
        _color.a = 1;
        Gizmos.color = _color;

        float interpolationValue = 1f / (float)_interpolation;

        for (int i = 0; i < _points.Count - 1; i++)
        {
            Vector3 P0 = _points[i].Position.position;
            Vector3 P1 = _points[i].NextSegmentP1.position;
            Vector3 P2 = _points[i + 1].PreviousSegmentP3.position;
            Vector3 P3 = _points[i + 1].Position.position;
            DrawSegment(P0, P1, P2, P3, interpolationValue);
            DrawNormal(P0, P1, P2, P3, interpolationValue);
        }
    }

    public void UpdateSpline()
    {
        SplineTerrainLayer splineTerrainLayer = gameObject.GetComponentInParent<SplineTerrainLayer>();
        if (splineTerrainLayer != null)
        {
            splineTerrainLayer.DataIsChanged();
        }
    }

    private void DrawSegment(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float interpolationValue)
    {
        Vector3 begin = P0;

        switch (_form)
        {
            case SplineSegment.PolynomialForm.Bernstein:

                for (int i = 0; i <= _interpolation; i++)
                {
                    Vector3 end = GetPointBernstein(P0, P1, P2, P3, interpolationValue * i);
                    Gizmos.DrawLine(begin, end);
                    begin = end;
                }
                break;
            case SplineSegment.PolynomialForm.Normal:
            default:
                for (int i = 0; i <= _interpolation; i++)
                {
                    Vector3 end = GetPointLerp(P0, P1, P2, P3, interpolationValue * i);
                    Gizmos.DrawLine(begin, end);
                    begin = end;
                }
                break;
        }
    }

    private void DrawNormal(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float interpolationValue)
    {
        for (int i = 0; i <= _interpolation; i++)
        {
            Vector3 normal = GetNormal(P0, P1, P2, P3, interpolationValue * i);
            Vector3 pos = GetPointLerp(P0, P1, P2, P3, interpolationValue * i);
            Gizmos.DrawLine(pos, pos + Quaternion.Euler(0, 90, 0) *  normal.normalized * 4f );
            Gizmos.DrawLine(pos, pos + Quaternion.Euler(0, 90, 0) *  normal.normalized * -4f );
        }
    }

    private Vector3 GetPointLerp(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
    {
        float oneMinus = 1 - t;
        Vector3 A = Vector3.Lerp(P0, P1, t);
        Vector3 B = Vector3.Lerp(P1, P2, t);
        Vector3 C = Vector3.Lerp(P2, P3, t);

        Vector3 D = Vector3.Lerp(A, B, t);
        Vector3 E = Vector3.Lerp(B, C, t);
        return Vector3.Lerp(D, E, t);
    }

    private Vector3 GetPointBernstein(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3,  float t)
    {
        float t2 = t * t;
        float t3 = t * t2;

        return P0 * (-t3 + (3 * t2) - (3 * t) + 1) +
               P1 * ((3 * t3) - (6 * t2) + (3 * t)) +
               P2 * ((-3 * t3) + (3 * t2)) +
               P3 * t3;
    }

    private Vector3 GetNormal(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
    {
        float t2 = t * t;

        return P0 * (- 3 * t2 + 6 * t - 3) +
               P1 * (9 * t2 - 12 * t + 3) +
               P2 * (-9 * t2 + 6 * t) +
               P3 * (3 * t2);

    }

    public Vector3[] GetInterpolatedPositions()
    {
        List<Vector3> positions = new List<Vector3>();

        positions.Add(_points[0].Position.position);

        for (int i = 0; i < _points.Count - 1; i++)
        {
            Vector3 P0 = _points[i].Position.position;
            Vector3 P1 = _points[i].NextSegmentP1.position;
            Vector3 P2 = _points[i + 1].PreviousSegmentP3.position;
            Vector3 P3 = _points[i + 1].Position.position;

            float interpolationValue = 1f / (float)_interpolation;

            switch (_form)
            {
                case SplineSegment.PolynomialForm.Bernstein:

                    for (int r = 1; r <= _interpolation; r++)
                    {
                        Vector3 end = GetPointBernstein(P0, P1, P2, P3, interpolationValue * r);
                        positions.Add(end);
                    }
                    break;
                case SplineSegment.PolynomialForm.Normal:
                default:
                    for (int r = 1; r <= _interpolation; r++)
                    {
                        Vector3 end = GetPointLerp(P0, P1, P2, P3, interpolationValue * r);
                        positions.Add(end);
                    }
                    break;
            }
        }

        return positions.ToArray();
    }
}

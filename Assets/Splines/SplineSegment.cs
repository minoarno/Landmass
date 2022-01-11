using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineSegment : MonoBehaviour
{
    public enum PolynomialForm
    {
        Normal = 0,
        Bernstein = 1,
    }

    [Range(1, 100)]
    [SerializeField] private int _interpolation = 20;

    [SerializeField] private PolynomialForm _form = PolynomialForm.Normal;

    [SerializeField] private Transform _P0 = null;
    [SerializeField] private Transform _P1 = null;
    [SerializeField] private Transform _P2 = null;
    [SerializeField] private Transform _P3 = null;


    private void OnDrawGizmos()
    {
        float interpolationValue = 1f / (float)_interpolation;
        Vector3 begin = _P0.transform.position;
        Gizmos.color = Color.red;
        switch (_form)
        {
            case PolynomialForm.Bernstein:

                for (int i = 0; i <= _interpolation; i++)
                {
                    Vector3 end = GetPointBernstein(interpolationValue * i);
                    Gizmos.DrawLine(begin, end);
                    begin = end;
                }
                break;
            case PolynomialForm.Normal:
            default:
                for (int i = 0; i <= _interpolation; i++)
                {
                    Vector3 end = GetPointNormal(interpolationValue * i);
                    Gizmos.DrawLine(begin, end);
                    begin = end;
                }
                break;
        }

    }

    private Vector3 GetPointNormal(float t)
    {
        float oneMinus = 1 - t;
        Vector3 A = Vector3.Lerp(_P0.transform.position, _P1.transform.position, t);
        Vector3 B = Vector3.Lerp(_P1.transform.position, _P2.transform.position, t);
        Vector3 C = Vector3.Lerp(_P2.transform.position, _P3.transform.position, t);

        Vector3 D = Vector3.Lerp(A, B, t);
        Vector3 E = Vector3.Lerp(B, C, t);
        return Vector3.Lerp(D, E, t);
    }

    private Vector3 GetPointBernstein(float t)
    {
        Vector3 p0 = _P0.transform.position;
        Vector3 p1 = _P1.transform.position;
        Vector3 p2 = _P2.transform.position;
        Vector3 p3 = _P3.transform.position;

        float t2 = t * t;
        float t3 = t * t2;

        return p0 * (-t3 + (3 * t2) - (3 * t) + 1) +
                p1 * ((3 * t3) - (6 * t2) + (3 * t)) +
                p2 * ((-3 * t3) + (3 * t2)) +
                p3 * t3;
    }
}

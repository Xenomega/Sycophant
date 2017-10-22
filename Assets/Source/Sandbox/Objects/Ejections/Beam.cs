using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
sealed internal class Beam : Orientable
{
    #region Values
    [SerializeField] AxisDirection _activeDirection;

    [SerializeField] GameObject _anchorA;
    [SerializeField] GameObject _anchorB;

    [SerializeField] float _weight; 
    #endregion

    #region Unity Functions
    private void OnDrawGizmos()
    {
        Orient();
    }
    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.gameObject.layer == Globals.BIPED_LAYER)
        {
            Biped biped = collider2D.gameObject.GetComponent<Biped>();
            biped.Die();
        }
    }
    #endregion

    #region Functions
    protected override void OnWorldAngleChanged(float worldAngle)
    {
        float absoluteAngle = Mathf.Abs(worldAngle);
        bool vertical = absoluteAngle == 0 || absoluteAngle == 180;
        bool horizontal = absoluteAngle == 90 || absoluteAngle == 270;
        bool active = _activeDirection == AxisDirection.Vertical && vertical || _activeDirection == AxisDirection.Horizontal && horizontal;
        this.gameObject.SetActive(active);
    }

    private void Orient()
    {
        if (_anchorA == null || _anchorB == null)
            return;

        Vector3 centroid = (_anchorB.transform.position + _anchorA.transform.position) / 2;
        Vector3 direction = (_anchorB.transform.position - _anchorA.transform.position);

        float directionAngle = Quaternion.FromToRotation(Vector3.up, Vector3.forward - direction).eulerAngles.z;
        Quaternion rotation = Quaternion.AngleAxis(directionAngle, Vector3.forward);

        _anchorA.transform.rotation = rotation;
        _anchorB.transform.rotation = rotation * Quaternion.Euler(0, 0, 180);

        this.transform.position = centroid;
        this.transform.rotation = rotation;
        this.transform.localScale = new Vector3(_weight, direction.magnitude, 1);
    }
    #endregion
}
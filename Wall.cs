using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Wall : MonoBehaviour
{
    [SerializeField]
    protected WallProperty m_Property;
    
    public WallProperty property
    {
        get
        {
            return m_Property;
        }
    }

    /*
     * Calculates the normal of a collision that occurs near `point`
     * (Since trigger colliders don't support this functionality)
     */
    public Vector3 CalculateCollisionNormal(Vector3 point)
    {
        Vector3 topEdge = transform.position + (transform.up * transform.localScale.y / 2);
        Vector3 bottomEdge = transform.position - (transform.up * transform.localScale.y / 2);
        Vector3 leftEdge = transform.position - (transform.right * transform.localScale.x / 2);
        Vector3 rightEdge = transform.position + (transform.right * transform.localScale.x / 2);
        Vector3 tlCorner = topEdge - (transform.right * transform.localScale.x / 2);
        Vector3 trCorner = topEdge + (transform.right * transform.localScale.x / 2);
        Vector3 blCorner = bottomEdge - (transform.right * transform.localScale.x / 2);
        Vector3 brCorner = bottomEdge + (transform.right * transform.localScale.x / 2);

        Vector3 centerToPoint = point - transform.position;

        // Check if centerToPoint matches any corners
        if (point == tlCorner)
            return (tlCorner - transform.position).normalized;
        if (point == trCorner)
            return (trCorner - transform.position).normalized;
        if (point == blCorner)
            return (blCorner - transform.position).normalized;
        if (point == brCorner)
            return (brCorner - transform.position).normalized;

        // No corners... Now determine the parametrization of the point along four directions
        // (Ties were resolved in corner case)
        // We want the parametrization closest to 1, and the correct direction
        // (parems on same axis will have the same direction,
        //  since Project(centerToPoint, topEdge-transform.position).magnitude == Project(centerToPoint, bottomEdge-transform.position).magnitude,
        //  and Project(centerToPoint, rightEdge-transform.position).magnitude == Project(centerToPoint, leftEdge-transform.position).magnitude)
        float topParem = Vector3.Project(centerToPoint, topEdge - transform.position).magnitude / transform.localScale.y;
        float rightParem = Vector3.Project(centerToPoint, rightEdge - transform.position).magnitude / transform.localScale.x;

        // Determine which direction each axis goes 
        if (Mathf.Abs(topParem - 1) < Mathf.Abs(rightParem - 1)) // top or bottom edge
        {
            if ((point - topEdge).magnitude < (point - bottomEdge).magnitude)
                return transform.up; // top edge is closer, collision was from top edge
            else
                return -transform.up; // bottom edge is closer, collision was from bottom edge
        }
        else // left or right edge
        {
            if ((point - rightEdge).magnitude < (point - leftEdge).magnitude)
                return transform.right; // right edge is closer, collision was from right edge
            else
                return -transform.right; // left edge is closer, collision was from left edge
        }
    }

    /*
     * The maximum distance you can travel along the normal
     */
    public float MaxDistanceAlongNormal(Vector3 normal)
    {
        normal = normal.normalized;

        if (normal == transform.up || normal == -transform.up)
            return transform.localScale.y / 2;
        if (normal == transform.right || normal == -transform.right)
            return transform.localScale.x / 2;

        // Guaranteed to not divide by 0 since normal != transform.up
        float xProjection = Vector3.Project(normal, transform.right).magnitude;
        float xRatio = (transform.localScale.x / 2) / xProjection;
        // Likewise guaranteed to not divide by 0 since normal != transform.right
        float yProjection = Vector3.Project(normal, transform.up).magnitude;

        if (xRatio * yProjection > transform.localScale.y)
            return transform.localScale.x / 2;
        return transform.localScale.y / 2;
    }

    public enum WallProperty
    {
        None = 0, // ships slide alongside the wall
        Reflect = 0b_1, // reflects ships
        Bonk = 0b_10, // disrupts ships
        Fragile = 0b_100 // breaks upon contact with a Burst
    }
}

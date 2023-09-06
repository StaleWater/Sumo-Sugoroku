using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollisionType {
    FLOOR,
    ROOF,
    WALL,
    NONE
}

public class CollisionHandler : MonoBehaviour
{
    Collider2D passingSurface;
    BoxCollider2D box;

    public bool ignoreCollisions;
    ContactFilter2D noFilter;
    Collider2D[] hits;


    void Awake() {
        ignoreCollisions = false;            
        box = GetComponent<BoxCollider2D>();
        noFilter = new ContactFilter2D().NoFilter();
        hits = new Collider2D[5];
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if(ignoreCollisions) return;

        var other = collision.collider;
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;
        var type = GetCollisionType(normal);
        if(type == CollisionType.NONE) return;

        switch(type) {
            case CollisionType.FLOOR:
                StayAboveFloor(other);
                break;

            case CollisionType.ROOF:
                if(other.gameObject.tag == "OneWay") {
                    passingSurface = other;
                    return;
                }
                StayUnderRoof(other);
                break;

            case CollisionType.WALL:
                if(other.gameObject.tag == "OneWay") {
                    passingSurface = other;
                    return;
                }
                StayOutWall(other, normal);
                break;
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        if(ignoreCollisions) return;

        var normal = collision.GetContact(0).normal;
        var type = GetCollisionType(normal);
        var other = collision.collider;

        if(passingSurface == other) return;

        switch(type) {
            case CollisionType.FLOOR:
                StayAboveFloor(other);
                break;
            case CollisionType.ROOF:
                StayUnderRoof(other);
                break;
            case CollisionType.WALL:
                StayOutWall(other, normal);
                break;
        }
    }

    void StayAboveFloor(Collider2D floor) {
        float topOfFloor = floor.bounds.max.y;
        float bottomOfPlayer = box.bounds.min.y;
        if(bottomOfPlayer >= topOfFloor) return;

        float halfPlayerHeight = box.bounds.extents.y;
        float newY = topOfFloor + halfPlayerHeight;
        transform.position = new Vector2(transform.position.x, newY);
    }

    void StayUnderRoof(Collider2D roof) {
        float bottomOfRoof = roof.bounds.min.y;
        float topOfPlayer = box.bounds.max.y;
        if(topOfPlayer <= bottomOfRoof) return;

        float halfPlayerHeight = box.bounds.extents.y;
        float newY = bottomOfRoof - halfPlayerHeight;
        transform.position = new Vector2(transform.position.x, newY);
    }

    void StayOutWall(Collider2D wall, Vector2 normal) {
        float halfPlayerWidth = box.bounds.extents.x;
        float newX;

        if(normal.x > 0) {
            float rightOfWall = wall.bounds.max.x;
            float leftOfPlayer = box.bounds.min.x;
            if(rightOfWall <= leftOfPlayer) return;
            newX = rightOfWall + halfPlayerWidth;
        }
        else {
            float leftOfWall = wall.bounds.min.x;
            float rightOfPlayer = box.bounds.max.x;
            if(leftOfWall >= rightOfPlayer) return;
            newX = leftOfWall - halfPlayerWidth;
        }

        transform.position = new Vector2(newX, transform.position.y);
    }

    public bool SurfaceExists(CollisionType surface) {
        Bounds bounds;
        switch(surface) {
            case CollisionType.FLOOR:
                bounds = GetFloorBounds();
                break;
            case CollisionType.ROOF:
                bounds = GetRoofBounds();
                break;
            case CollisionType.WALL:
                bounds = GetWallBounds();
                break;
            default:
                Debug.Log("UNEXPECTED SURFACE TYPE");
                bounds = GetWallBounds();
                break;
        }
        
        int angle = 0;
        int num = Physics2D.OverlapBox(bounds.center, bounds.size, angle, noFilter, hits);
        return num > 1;
    }

    public CollisionType GetCollisionType(Vector2 normal) {
        CollisionType t;
        if(Mathf.Abs(normal.y) > Mathf.Abs(normal.x)) {
            if(normal.y > 0.0f) t = CollisionType.FLOOR;
            else t = CollisionType.ROOF;
        }
        else t = CollisionType.WALL;

        if(!SurfaceExists(t)) t = CollisionType.NONE;
        return t;
    }
    
    Bounds GetFloorBounds() {
        Vector3 center = box.bounds.center;
        Vector3 size = box.bounds.size;
        size.x -= size.x / 2.0f;
        
        center.y -= size.y / 4.0f;
        size.y -= size.y / 2.0f;

        return new Bounds(center, size);
    }

    Bounds GetRoofBounds() {
        Vector3 center = box.bounds.center;
        Vector3 size = box.bounds.size;
        size.x -= size.x / 2.0f;
        
        center.y += size.y / 4.0f;
        size.y -= size.y / 2.0f;

        return new Bounds(center, size);
    }

    Bounds GetWallBounds() {
        Vector3 center = box.bounds.center;
        Vector3 size = box.bounds.size;
        size.y -= size.y / 2.0f;

        return new Bounds(center, size);
    }

    void FloorBoundsGizmo() {
        Gizmos.color = Color.red;

        var bounds = GetFloorBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

}
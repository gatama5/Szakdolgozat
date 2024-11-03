
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireDragScript : MonoBehaviour
{
    public Transform quad;  // A Quad referencia
    public SpriteRenderer wireEnd;  // A k�bel v�ge sprite-ja

    private Camera fpsCamera;  // FPS kamera referencia
    private Vector3 offset;
    private Vector3 initialLocalPosition;  // A k�bel kiindul� helyi poz�ci�ja a sz�l�h�z viszony�tva

    void Start()
    {
        fpsCamera = Camera.main;  // Az FPS kamera be�ll�t�sa
        initialLocalPosition = transform.localPosition;  // K�bel kiindul�si pontja
    }

    private void OnMouseDown()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        offset = transform.position - mouseWorldPos;

    }

    private void OnMouseDrag()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 newPosition = mouseWorldPos + offset;

        // Korl�toz�s a Quad ter�let�n bel�l
        newPosition = ClampPositionToQuad(newPosition);

        // K�bel poz�ci�j�nak friss�t�se
        transform.position = newPosition;

        // K�bel ir�ny�t�sa az eredeti kezd�pont fel� (sz�l� helyi poz�ci�j�hoz viszony�tva)
        Vector3 direction = (newPosition - transform.parent.TransformPoint(initialLocalPosition)).normalized;
        direction.z = 0;  // A Z tengely elker�l�se

        // A k�bel elforgat�sa csak a 2D s�kban
        transform.right = direction * transform.lossyScale.x;

        // K�bel ny�jt�sa
        float dist = Vector2.Distance(transform.TransformPoint(initialLocalPosition), newPosition);
        //float dist = Vector2.Distance(newPosition, initialLocalPosition);

        // A sprite size ny�jt�sa
        wireEnd.size = new Vector2(dist, wireEnd.size.y);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = fpsCamera.ScreenPointToRay(Input.mousePosition);
        Plane quadPlane = new Plane(quad.forward, quad.position);

        float distance;
        if (quadPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;  // Hiba eset�n
    }

    private Vector3 ClampPositionToQuad(Vector3 position)
    {
        Vector3 quadSize = quad.localScale;
        Vector3 localPos = quad.InverseTransformPoint(position);

        localPos.x = Mathf.Clamp(localPos.x, -quadSize.x / 2, quadSize.x / 2);
        localPos.y = Mathf.Clamp(localPos.y, -quadSize.y / 2, quadSize.y / 2);

        return quad.TransformPoint(localPos);
    }

}


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class WireDragScript : MonoBehaviour
//{
//    Vector3 startPoint;
//    Vector3 startPos;
//    public Transform quad;  // A Quad referencia
//    public SpriteRenderer wireEnd;
//    public GameObject lightOn;

//    private void Start()
//    {
//        startPoint = transform.parent.position;
//        startPos = transform.position;
//    }

//    private void OnMouseDrag()
//    {
//        Vector3 newPosi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        newPosi.z = 0;

//        Collider2D[] collision = Physics2D.OverlapCircleAll(newPosi, .2f);
//        foreach (Collider2D collider in collision)
//        {
//            if (collider.gameObject != gameObject)
//            {
//                UpdateWire(collider.transform.position);

//                if (transform.parent.name.Equals(collider.transform.parent.name))
//                {
//                    collider.GetComponent<WireDragScript>()?.Done();
//                    Done();
//                }

//                return;
//            }
//        }

//        UpdateWire(newPosi);
//    }

//    void Done()
//    {
//        lightOn.SetActive(true);
//        Destroy(this);
//    }

//    private void OnMouseUp()
//    {
//        UpdateWire(startPos);
//    }

//    void UpdateWire(Vector3 newPos)
//    {

//        transform.position = newPos;

//        Vector3 dir = newPos - startPoint;
//        transform.right = dir * transform.localScale.x;

//        float dist = Vector2.Distance(startPoint, newPos);
//        wireEnd.size = new Vector2(dist, wireEnd.size.y);
//    }

//}
